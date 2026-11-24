using UnityEngine;

// Қара бура — дух чёрного верблюда. Стоит, роет копытом и бросается в разбег по прямой.
// Автомат состояний (enum State), переходы:
//   Idle    → WindUp  : видит Ай (близко, не выше maxHeightDifference, нет стены, сам в кадре камеры)
//   WindUp  → Idle    : Ай ушла за стену или Бура ушёл за экран (дальность и прыжок замах НЕ сбрасывают)
//   WindUp  → Charge  : прошло windUpTime (всё это время он «роет копытом»: цвет/спрайт)
//   Charge  → Stunned : Raycast нашёл стену или обрыв впереди, или прошло chargeMaxTime
//   Stunned → Idle    : прошло cooldownTime (оглушение = перезарядка; числа — в ассете Bura_Stats)
//   любое   → Frozen  : в луче фонаря (подготовка и разбег сброшены)
//   Frozen  → Idle    : луч ушёл
// Во время WindUp Бура поворачивается к Ай, направление разбега фиксируется в конце замаха
// и в разбеге уже не меняется — от него можно уйти или перепрыгнуть.
// На объекте: SpriteRenderer, Rigidbody2D (Kinematic), BoxCollider2D (Is Trigger).
public class Bura : Enemy
{
    enum State { Idle, WindUp, Charge, Stunned, Frozen }

    [Header("Баланс (зрение, замах, разбег, оглушение)")]
    [SerializeField] BuraStats stats;

    [Header("Тело (размеры спрайта, не баланс)")]
    [SerializeField] float eyeHeight = 0.7f;           // откуда «смотрит» (над ногами)
    [SerializeField] LayerMask groundLayer;            // слой Ground: стены закрывают обзор
    [SerializeField] float wallCheckDistance = 1f;     // стена ближе — удар, оглушён
    [SerializeField] float edgeCheckOffset = 1f;       // на сколько вперёд ищем землю под ногами
    [SerializeField] float groundCheckDepth = 0.5f;    // как глубоко вниз ищем землю

    [Header("Вид (спрайты можно оставить пустыми)")]
    [SerializeField] Sprite windUpSprite;
    [SerializeField] Sprite chargeSprite;
    [SerializeField] Sprite stunnedSprite;
    [SerializeField] Sprite frozenSprite;
    [SerializeField] Color windUpColor = Color.white;   // белый: подготовка видна по спрайту windup
    [SerializeField] Color stunnedColor = Color.white; // белый: оглушение видно по спрайту stunned
    [SerializeField] Color frozenColor = Color.white;  // белый: спрайт frozen уже голубой

    State state = State.Idle;
    float timer;                 // сколько ещё длится WindUp / Charge / Stunned
    bool facingRight = true;     // в Charge — направление разбега, выбранное на подготовке
    Rigidbody2D rb;
    SpriteRenderer sr;
    Sprite normalSprite;         // спрайт из префаба
    Color normalColor;           // цвет из префаба
    Camera cam;
    Collider2D playerCollider;   // смотрим в центр коллайдера Ай

    protected override EnemyStats Stats() { return stats; }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        normalSprite = sr.sprite;
        normalColor = sr.color;
        cam = Camera.main;

        GameObject playerObject = GameObject.FindWithTag("Player");
        if (playerObject != null) playerCollider = playerObject.GetComponent<Collider2D>();
    }

    void FixedUpdate()
    {
        timer -= Time.fixedDeltaTime;

        // Свет главнее всего: из любого состояния — в Frozen
        if (IsLit() && state != State.Frozen) ChangeState(State.Frozen);

        switch (state)
        {
            case State.Frozen:
                if (!IsLit()) ChangeState(State.Idle);       // после света подготовка начинается заново
                break;

            case State.Idle:
                if (CanSeePlayer()) ChangeState(State.WindUp);
                break;

            case State.WindUp:
                // Высоту и дальность здесь не проверяем: замах сбрасывают только стена и край экрана
                if (!ClearView()) ChangeState(State.Idle);
                else
                {
                    facingRight = PlayerIsRight();                      // смотрит на Ай, пока роет копытом
                    if (timer <= 0f) ChangeState(State.Charge);         // в конце замаха направление зафиксировано
                }
                break;

            case State.Charge:
                if (!CanMoveForward() || timer <= 0f) ChangeState(State.Stunned);
                else rb.MovePosition(rb.position + Forward() * stats.chargeSpeed * Time.fixedDeltaTime);
                break;

            case State.Stunned:
                if (timer <= 0f) ChangeState(State.Idle);
                break;
        }
    }

    // Смена состояния: здесь же заводим таймер нового состояния
    void ChangeState(State newState)
    {
        state = newState;
        if (newState == State.WindUp)
        {
            timer = stats.windUpTime;
            sfx.Play(windUpClips);       // первый кадр замаха (роет копытом)
        }
        if (newState == State.Charge) timer = stats.chargeMaxTime;
        if (newState == State.Stunned) timer = stats.cooldownTime;            // оглушение = перезарядка
    }

    Vector2 Forward()
    {
        return facingRight ? Vector2.right : Vector2.left;
    }

    // Можно ли бежать дальше: впереди нет стены и есть земля (два Raycast по слою Ground)
    bool CanMoveForward()
    {
        Vector2 feet = transform.position;                  // pivot снизу — это ноги
        bool wallAhead = Physics2D.Raycast(feet + Vector2.up * eyeHeight, Forward(), wallCheckDistance, groundLayer);
        // луч вниз стартует с высоты глаз, чтобы не начинаться внутри земли
        bool groundAhead = Physics2D.Raycast(feet + Forward() * edgeCheckOffset + Vector2.up * eyeHeight, Vector2.down, eyeHeight + groundCheckDepth, groundLayer);
        return !wallAhead && groundAhead;
    }

    bool PlayerIsRight()
    {
        return playerCollider.bounds.center.x > transform.position.x;
    }

    // Начать замах: ничто не мешает, Ай близко и не слишком высоко над / под Бурой
    bool CanSeePlayer()
    {
        if (!ClearView()) return false;
        Vector2 toPlayer = ToPlayer();
        if (toPlayer.magnitude > stats.visionRange) return false;
        return Mathf.Abs(toPlayer.y) <= stats.maxHeightDifference;
    }

    // Ничто не мешает: Бура в кадре и между его глазами и Ай нет стены (дальность и высота не важны)
    bool ClearView()
    {
        if (playerCollider == null) return false;
        if (!IsOnScreen()) return false;                    // из-за края экрана не нападает

        Vector2 toPlayer = ToPlayer();
        // Raycast только по слою Ground: попал во что-то — стена мешает
        RaycastHit2D hit = Physics2D.Raycast(Eye(), toPlayer.normalized, toPlayer.magnitude, groundLayer);
        return hit.collider == null;
    }

    Vector2 Eye()
    {
        return (Vector2)transform.position + Vector2.up * eyeHeight;
    }

    // Вектор от глаз Буры к центру коллайдера Ай
    Vector2 ToPlayer()
    {
        return (Vector2)playerCollider.bounds.center - Eye();
    }

    // В кадре ли камеры. Viewport: левый нижний угол экрана = (0,0), правый верхний = (1,1)
    bool IsOnScreen()
    {
        Vector3 p = cam.WorldToViewportPoint(transform.position);
        float m = stats.screenMargin;
        return p.x > m && p.x < 1f - m && p.y > m && p.y < 1f - m;
    }

    // Ранит только в разбеге — перед этим всегда есть подготовка
    void OnTriggerStay2D(Collider2D other)
    {
        if (!enabled || state != State.Charge) return;
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
        Color color = normalColor;
        if (state == State.WindUp) { sprite = windUpSprite; color = windUpColor; }
        if (state == State.Charge) sprite = chargeSprite;
        if (state == State.Stunned) { sprite = stunnedSprite; color = stunnedColor; }
        if (state == State.Frozen) { sprite = frozenSprite; color = frozenColor; }

        sr.sprite = (sprite != null) ? sprite : normalSprite;   // нет своего спрайта — обычный
        sr.color = color;
    }

    // В редакторе (выделен): жёлтый круг — дальность зрения
    void OnDrawGizmosSelected()
    {
        if (stats == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position + Vector3.up * eyeHeight, stats.visionRange);
    }
}
