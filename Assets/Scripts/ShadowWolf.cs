using UnityEngine;

// Көлеңке-қасқыр — волк-тень. Ходит по земле, видит Ай, подкрадывается и кусает.
// Автомат состояний (enum State), переходы:
//   Patrol  → Chase    : увидел Ай (Raycast до неё не упёрся в стену)
//   Chase   → Patrol   : потерял Ай из виду
//   Chase   → WindUp   : Ай ближе biteRange (числа — в ассете Wolf_Stats) и волк в кадре камеры — замах (видно: волк меняет цвет/спрайт и стоит)
//   WindUp  → Chase    : волк ушёл за край экрана — замах сброшен (из-за экрана не нападает)
//   WindUp  → Bite     : прошло windUpTime — рывок вперёд, кусает только сейчас
//   Bite    → Cooldown : прошло biteTime — стоит и отдыхает
//   Cooldown→ Chase/Patrol : прошло cooldownTime
//   любое   → Frozen   : в луче фонаря (замах и укус отменяются)
//   Frozen  → Chase/Patrol : луч ушёл
// На объекте: SpriteRenderer, Rigidbody2D (Kinematic), BoxCollider2D (Is Trigger).
// Точки патруля можно сделать детьми волка: позиции запоминаются в Start.
public class ShadowWolf : Enemy
{
    enum State { Patrol, Chase, WindUp, Bite, Cooldown, Frozen }

    [Header("Баланс (скорости, зрение, замах, укус)")]
    [SerializeField] ShadowWolfStats stats;

    [Header("Патруль")]
    [SerializeField] Transform[] waypoints;            // точки, между которыми ходит (по оси X)
    [SerializeField] float waypointReachDistance = 0.1f;   // ближе этого к точке — дошёл (точность, не баланс)

    [Header("Тело (размеры спрайта, не баланс)")]
    [SerializeField] float eyeHeight = 0.5f;           // откуда «смотрит» (над ногами)
    [SerializeField] LayerMask groundLayer;            // слой Ground: стены закрывают обзор
    [SerializeField] float wallCheckDistance = 0.6f;   // стена ближе — дальше не идёт
    [SerializeField] float edgeCheckOffset = 0.6f;     // на сколько вперёд ищем землю под лапами
    [SerializeField] float groundCheckHeight = 0.1f;   // с какой высоты над лапами луч ищет землю
    [SerializeField] float groundCheckDepth = 0.5f;    // какой длины этот луч вниз

    [Header("Вид (спрайты можно оставить пустыми)")]
    [SerializeField] Sprite windUpSprite;
    [SerializeField] Sprite biteSprite;
    [SerializeField] Sprite frozenSprite;
    [SerializeField] Color windUpColor = Color.white;   // белый: подготовка видна по спрайту windup
    [SerializeField] Color frozenColor = Color.white;  // белый: спрайт frozen уже голубой

    State state = State.Patrol;
    float timer;                 // сколько ещё длится WindUp / Bite / Cooldown
    bool facingRight = true;
    float[] pointsX;             // X точек патруля
    int pointIndex;              // к какой точке идём
    Rigidbody2D rb;
    SpriteRenderer sr;
    Sprite normalSprite;         // спрайт из префаба
    Transform player;
    Collider2D playerCollider;   // в центр её коллайдера и смотрим
    Camera cam;

    protected override EnemyStats Stats() { return stats; }

    // Маршрут от LevelLoader (вызывается до Start): двигаем две точки патруля (дети волка)
    public override void SetRoute(Vector2 start, Vector2 end)
    {
        if (waypoints.Length < 2) return;
        waypoints[0].localPosition = start;
        waypoints[1].localPosition = end;
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        normalSprite = sr.sprite;
        cam = Camera.main;

        GameObject playerObject = GameObject.FindWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
            playerCollider = playerObject.GetComponent<Collider2D>();
        }

        // Запоминаем X точек сейчас: если точки — дети волка, они потом поедут вместе с ним
        pointsX = new float[waypoints.Length];
        for (int i = 0; i < waypoints.Length; i++) pointsX[i] = waypoints[i].position.x;
    }

    void FixedUpdate()
    {
        timer -= Time.fixedDeltaTime;

        // Свет главнее всего: из любого состояния — в Frozen
        if (IsLit() && state != State.Frozen) ChangeState(State.Frozen);

        switch (state)
        {
            case State.Frozen:
                if (!IsLit()) ChangeState(CanSeePlayer() ? State.Chase : State.Patrol);
                break;

            case State.Patrol:
                Patrol();
                if (CanSeePlayer()) ChangeState(State.Chase);
                break;

            case State.Chase:
                if (!CanSeePlayer())
                {
                    ChangeState(State.Patrol);
                    break;
                }
                float dx = player.position.x - transform.position.x;
                facingRight = dx > 0f;                              // поворачивается к Ай
                if (Mathf.Abs(dx) <= stats.biteRange && IsOnScreen()) ChangeState(State.WindUp);   // из-за экрана не нападает
                else Move(stats.chaseSpeed);                              // у края или стены просто стоит
                break;

            case State.WindUp:
                // Стоит и не поворачивается: направление укуса уже выбрано — от него можно уйти
                if (!IsOnScreen()) ChangeState(State.Chase);          // ушёл за экран — не кусает
                else if (timer <= 0f) ChangeState(State.Bite);
                break;

            case State.Bite:
                Move(stats.biteSpeed);
                if (timer <= 0f) ChangeState(State.Cooldown);
                break;

            case State.Cooldown:
                if (timer <= 0f) ChangeState(CanSeePlayer() ? State.Chase : State.Patrol);
                break;
        }
    }

    // Смена состояния: здесь же заводим таймер нового состояния
    void ChangeState(State newState)
    {
        state = newState;
        if (newState == State.WindUp) timer = stats.windUpTime;
        if (newState == State.Bite) timer = stats.biteTime;
        if (newState == State.Cooldown) timer = stats.cooldownTime;
    }

    // Патруль: идём к точке по X; дошли или упёрлись — к следующей
    void Patrol()
    {
        if (pointsX.Length == 0) return;                    // точек нет — стоит на месте

        float dx = pointsX[pointIndex] - transform.position.x;
        if (Mathf.Abs(dx) < waypointReachDistance)
        {
            pointIndex = (pointIndex + 1) % pointsX.Length; // % — по кругу: 0, 1, 0, 1…
            return;
        }
        facingRight = dx > 0f;
        if (!Move(stats.moveSpeed)) pointIndex = (pointIndex + 1) % pointsX.Length;
    }

    // Шаг вперёд. Возвращает false, если впереди стена или обрыв (тогда не идёт)
    bool Move(float speed)
    {
        Vector2 forward = facingRight ? Vector2.right : Vector2.left;
        Vector2 feet = transform.position;                  // pivot снизу — это лапы

        // Raycast 1: стена прямо перед мордой?
        bool wallAhead = Physics2D.Raycast(feet + Vector2.up * eyeHeight, forward, wallCheckDistance, groundLayer);
        // Raycast 2: есть ли земля чуть впереди под лапами?
        bool groundAhead = Physics2D.Raycast(feet + forward * edgeCheckOffset + Vector2.up * groundCheckHeight, Vector2.down, groundCheckDepth, groundLayer);

        if (wallAhead || !groundAhead) return false;

        rb.MovePosition(rb.position + forward * speed * Time.fixedDeltaTime);
        return true;
    }

    // Видит ли волк Ай: близко, (в патруле — только в угле обзора впереди) и между ними нет стены
    bool CanSeePlayer()
    {
        if (player == null) return false;

        Vector2 eye = (Vector2)transform.position + Vector2.up * eyeHeight;
        Vector2 toPlayer = (Vector2)playerCollider.bounds.center - eye;

        if (toPlayer.magnitude > stats.visionRange) return false;
        // В патруле видит только в угле обзора перед мордой. В погоне волк сам поворачивается к Ай, угол не нужен
        Vector2 forward = facingRight ? Vector2.right : Vector2.left;
        if (state == State.Patrol && Vector2.Angle(forward, toPlayer) > stats.viewAngle / 2f) return false;

        // Raycast 3: луч от глаз к Ай только по слою Ground. Попал во что-то — значит, стена мешает
        RaycastHit2D hit = Physics2D.Raycast(eye, toPlayer.normalized, toPlayer.magnitude, groundLayer);
        return hit.collider == null;
    }

    // В кадре ли камеры. Viewport: левый нижний угол экрана = (0,0), правый верхний = (1,1)
    bool IsOnScreen()
    {
        Vector3 p = cam.WorldToViewportPoint(transform.position);
        float m = stats.screenMargin;
        return p.x > m && p.x < 1f - m && p.y > m && p.y < 1f - m;
    }

    // Кусает только во время рывка — честно: перед этим всегда есть замах
    void OnTriggerStay2D(Collider2D other)
    {
        if (!enabled || state != State.Bite) return;
        if (other.CompareTag("Player"))
        {
            other.GetComponent<PlayerHealth>().TakeDamage(stats.damage);
        }
    }

    // Внешний вид по состоянию
    void Update()
    {
        sr.flipX = !facingRight;               // нарисован вправо, влево отражаем

        Sprite sprite = normalSprite;
        Color color = Color.white;
        if (state == State.WindUp) { sprite = windUpSprite; color = windUpColor; }
        if (state == State.Bite) sprite = biteSprite;
        if (state == State.Frozen) { sprite = frozenSprite; color = frozenColor; }

        sr.sprite = (sprite != null) ? sprite : normalSprite;   // нет своего спрайта — обычный
        sr.color = color;
    }

    // В редакторе (волк выделен): жёлтый круг — дальность зрения, красный — дальность замаха
    void OnDrawGizmosSelected()
    {
        if (stats == null) return;
        Vector3 eye = transform.position + Vector3.up * eyeHeight;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(eye, stats.visionRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, stats.biteRange);
    }
}
