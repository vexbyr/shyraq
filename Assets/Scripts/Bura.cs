using UnityEngine;

// Қара бура — дух чёрного верблюда. Стоит, роет копытом и бросается в разбег по прямой.
// Автомат состояний (enum State), переходы:
//   Idle    → WindUp  : видит Ай (близко, не выше maxHeightDifference, нет стены, сам в кадре камеры)
//   WindUp  → Idle    : Ай ушла за стену, далеко или за экран (прыжок замах НЕ сбрасывает)
//   WindUp  → Charge  : прошло windUpTime (всё это время он «роет копытом»: цвет/спрайт)
//   Charge  → Stunned : Raycast нашёл стену или обрыв впереди, или прошло chargeMaxTime
//   Stunned → Idle    : прошло stunTime (это и есть перезарядка)
//   любое   → Frozen  : в луче фонаря (подготовка и разбег сброшены)
//   Frozen  → Idle    : луч ушёл
// Во время WindUp Бура поворачивается к Ай, направление разбега фиксируется в конце замаха
// и в разбеге уже не меняется — от него можно уйти или перепрыгнуть.
// На объекте: SpriteRenderer, Rigidbody2D (Kinematic), BoxCollider2D (Is Trigger).
public class Bura : Enemy
{
    enum State { Idle, WindUp, Charge, Stunned, Frozen }

    [Header("Зрение")]
    [SerializeField] float sightRange = 9f;            // дальше не видит
    [SerializeField] float maxHeightDifference = 3f;   // Ай выше/ниже этого — не начинает замах (3 — видит и в прыжке)
    [SerializeField] float eyeHeight = 0.7f;           // откуда «смотрит» (над ногами)
    [SerializeField] LayerMask groundLayer;            // слой Ground: стены закрывают обзор

    [Header("Разбег")]
    [SerializeField] float windUpTime = 0.8f;          // подготовка, секунды
    [SerializeField] float chargeSpeed = 10f;          // единиц в секунду
    [SerializeField] float chargeMaxTime = 1.2f;       // дольше не бежит
    [SerializeField] float stunTime = 1.5f;            // оглушён после разбега (перезарядка)
    [SerializeField] int chargeDamage = 1;

    [Header("Проверки стены и края")]
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
                // Высоту здесь не проверяем: подпрыгнувшая Ай замах не сбрасывает, сбрасывает только стена / даль / экран
                if (!HasLineOfSight()) ChangeState(State.Idle);
                else
                {
                    facingRight = PlayerIsRight();                      // смотрит на Ай, пока роет копытом
                    if (timer <= 0f) ChangeState(State.Charge);         // в конце замаха направление зафиксировано
                }
                break;

            case State.Charge:
                if (!CanMoveForward() || timer <= 0f) ChangeState(State.Stunned);
                else rb.MovePosition(rb.position + Forward() * chargeSpeed * Time.fixedDeltaTime);
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
        if (newState == State.WindUp) timer = windUpTime;
        if (newState == State.Charge) timer = chargeMaxTime;
        if (newState == State.Stunned) timer = stunTime;
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

    // Начать замах: Ай видна и не слишком высоко над / под Бурой
    bool CanSeePlayer()
    {
        if (!HasLineOfSight()) return false;
        float heightDifference = playerCollider.bounds.center.y - (transform.position.y + eyeHeight);
        return Mathf.Abs(heightDifference) <= maxHeightDifference;
    }

    // Прямая видимость: Бура в кадре, Ай близко и между ними нет стены (высота не важна)
    bool HasLineOfSight()
    {
        if (playerCollider == null) return false;
        if (!IsOnScreen()) return false;                    // из-за края экрана не нападает

        Vector2 eye = (Vector2)transform.position + Vector2.up * eyeHeight;
        Vector2 toPlayer = (Vector2)playerCollider.bounds.center - eye;
        if (toPlayer.magnitude > sightRange) return false;

        // Raycast только по слою Ground: попал во что-то — стена мешает
        RaycastHit2D hit = Physics2D.Raycast(eye, toPlayer.normalized, toPlayer.magnitude, groundLayer);
        return hit.collider == null;
    }

    // В кадре ли камеры. Viewport: левый нижний угол экрана = (0,0), правый верхний = (1,1)
    bool IsOnScreen()
    {
        Vector3 p = cam.WorldToViewportPoint(transform.position);
        return p.x > 0f && p.x < 1f && p.y > 0f && p.y < 1f;
    }

    // Ранит только в разбеге — перед этим всегда есть подготовка
    void OnTriggerStay2D(Collider2D other)
    {
        if (state != State.Charge) return;
        if (other.CompareTag("Player"))
        {
            other.GetComponent<PlayerHealth>().TakeDamage(chargeDamage);
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
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position + Vector3.up * eyeHeight, sightRange);
    }
}
