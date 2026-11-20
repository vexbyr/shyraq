using UnityEngine;

// Көлеңке-қасқыр — волк-тень. Ходит по земле, видит Ай, подкрадывается и кусает.
// Автомат состояний (enum State), переходы:
//   Patrol  → Chase    : увидел Ай (Raycast до неё не упёрся в стену)
//   Chase   → Patrol   : потерял Ай из виду
//   Chase   → WindUp   : Ай ближе biteRange и волк в кадре камеры — замах (видно: волк меняет цвет/спрайт и стоит)
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

    [Header("Патруль")]
    [SerializeField] Transform[] waypoints;            // точки, между которыми ходит (по оси X)
    [SerializeField] float patrolSpeed = 2f;           // единиц в секунду

    [Header("Зрение и погоня")]
    [SerializeField] float sightRange = 7f;            // дальше не видит
    [SerializeField] float viewAngle = 160f;           // угол обзора вперёд в градусах (по 80° вверх и вниз от морды), за спиной не видит
    [SerializeField] float eyeHeight = 0.5f;           // откуда «смотрит» (над ногами)
    [SerializeField] float chaseSpeed = 7.2f;          // примерно в 1,2 раза быстрее бега Ай (moveSpeed 6)
    [SerializeField] float biteRange = 1.5f;           // с какого расстояния начинает замах
    [SerializeField] LayerMask groundLayer;            // слой Ground: стены закрывают обзор

    [Header("Укус")]
    [SerializeField] float windUpTime = 0.5f;          // подготовка, секунды
    [SerializeField] float biteTime = 0.25f;           // сколько длится рывок
    [SerializeField] float biteSpeed = 9f;             // скорость рывка
    [SerializeField] float cooldownTime = 1f;          // отдых после укуса
    [SerializeField] int biteDamage = 1;

    [Header("Проверки стены и края")]
    [SerializeField] float wallCheckDistance = 0.6f;   // стена ближе — дальше не идёт
    [SerializeField] float edgeCheckOffset = 0.6f;     // на сколько вперёд ищем землю под лапами

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
                if (Mathf.Abs(dx) <= biteRange && IsOnScreen()) ChangeState(State.WindUp);   // из-за экрана не нападает
                else Move(chaseSpeed);                              // у края или стены просто стоит
                break;

            case State.WindUp:
                // Стоит и не поворачивается: направление укуса уже выбрано — от него можно уйти
                if (!IsOnScreen()) ChangeState(State.Chase);          // ушёл за экран — не кусает
                else if (timer <= 0f) ChangeState(State.Bite);
                break;

            case State.Bite:
                Move(biteSpeed);
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
        if (newState == State.WindUp) timer = windUpTime;
        if (newState == State.Bite) timer = biteTime;
        if (newState == State.Cooldown) timer = cooldownTime;
    }

    // Патруль: идём к точке по X; дошли или упёрлись — к следующей
    void Patrol()
    {
        if (pointsX.Length == 0) return;                    // точек нет — стоит на месте

        float dx = pointsX[pointIndex] - transform.position.x;
        if (Mathf.Abs(dx) < 0.1f)
        {
            pointIndex = (pointIndex + 1) % pointsX.Length; // % — по кругу: 0, 1, 0, 1…
            return;
        }
        facingRight = dx > 0f;
        if (!Move(patrolSpeed)) pointIndex = (pointIndex + 1) % pointsX.Length;
    }

    // Шаг вперёд. Возвращает false, если впереди стена или обрыв (тогда не идёт)
    bool Move(float speed)
    {
        Vector2 forward = facingRight ? Vector2.right : Vector2.left;
        Vector2 feet = transform.position;                  // pivot снизу — это лапы

        // Raycast 1: стена прямо перед мордой?
        bool wallAhead = Physics2D.Raycast(feet + Vector2.up * eyeHeight, forward, wallCheckDistance, groundLayer);
        // Raycast 2: есть ли земля чуть впереди под лапами?
        bool groundAhead = Physics2D.Raycast(feet + forward * edgeCheckOffset + Vector2.up * 0.1f, Vector2.down, 0.5f, groundLayer);

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

        if (toPlayer.magnitude > sightRange) return false;
        // В патруле видит только в угле обзора перед мордой. В погоне волк сам поворачивается к Ай, угол не нужен
        Vector2 forward = facingRight ? Vector2.right : Vector2.left;
        if (state == State.Patrol && Vector2.Angle(forward, toPlayer) > viewAngle / 2f) return false;

        // Raycast 3: луч от глаз к Ай только по слою Ground. Попал во что-то — значит, стена мешает
        RaycastHit2D hit = Physics2D.Raycast(eye, toPlayer.normalized, toPlayer.magnitude, groundLayer);
        return hit.collider == null;
    }

    // В кадре ли камеры. Viewport: левый нижний угол экрана = (0,0), правый верхний = (1,1)
    bool IsOnScreen()
    {
        Vector3 p = cam.WorldToViewportPoint(transform.position);
        return p.x > 0f && p.x < 1f && p.y > 0f && p.y < 1f;
    }

    // Кусает только во время рывка — честно: перед этим всегда есть замах
    void OnTriggerStay2D(Collider2D other)
    {
        if (state != State.Bite) return;
        if (other.CompareTag("Player"))
        {
            other.GetComponent<PlayerHealth>().TakeDamage(biteDamage);
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
        Vector3 eye = transform.position + Vector3.up * eyeHeight;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(eye, sightRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, biteRange);
    }
}
