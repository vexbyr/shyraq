using UnityEngine;

// Қара бура — дух чёрного верблюда. Стоит, роет копытом и бросается в разбег по прямой.
// Автомат состояний (enum State), переходы:
//   Idle    → WindUp  : видит Ай (близко, на его высоте, нет стены, сам в кадре камеры)
//   WindUp  → Idle    : Ай пропала из виду — подготовка сброшена
//   WindUp  → Charge  : прошло windUpTime (всё это время он «роет копытом»: цвет/спрайт)
//   Charge  → Stunned : Raycast нашёл стену или обрыв впереди, или прошло chargeMaxTime
//   Stunned → Idle    : прошло stunTime (это и есть перезарядка)
//   любое   → Frozen  : в луче фонаря (подготовка и разбег сброшены)
//   Frozen  → Idle    : луч ушёл
// Направление разбега запоминается в начале WindUp и потом не меняется — от него можно уйти.
// На объекте: SpriteRenderer, Rigidbody2D (Kinematic), BoxCollider2D (Is Trigger).
public class Bura : Enemy
{
    enum State { Idle, WindUp, Charge, Stunned, Frozen }

    [Header("Зрение")]
    [SerializeField] float sightRange = 9f;            // дальше не видит
    [SerializeField] float maxHeightDifference = 1.5f; // Ай выше/ниже этого — не на его дороге, не бежит
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
    [SerializeField] Color windUpColor = new Color(1f, 0.85f, 0.3f);   // жёлтый — «сейчас побежит»
    [SerializeField] Color stunnedColor = new Color(0.6f, 0.6f, 0.6f); // серый — оглушён
    [SerializeField] Color frozenColor = new Color(0.6f, 0.8f, 1f);    // голубой — замёрз

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
                if (CanSeePlayer())
                {
                    facingRight = playerCollider.bounds.center.x > transform.position.x;   // направление запомнено
                    ChangeState(State.WindUp);
                }
                break;

            case State.WindUp:
                if (!CanSeePlayer()) ChangeState(State.Idle);           // ушла за стену / за экран
                else if (timer <= 0f) ChangeState(State.Charge);
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

    // Видит ли Ай: Бура в кадре, Ай близко, примерно на его высоте и между ними нет стены
    bool CanSeePlayer()
    {
        if (playerCollider == null) return false;
        if (!IsOnScreen()) return false;                    // из-за края экрана не нападает

        Vector2 eye = (Vector2)transform.position + Vector2.up * eyeHeight;
        Vector2 toPlayer = (Vector2)playerCollider.bounds.center - eye;
        if (toPlayer.magnitude > sightRange) return false;
        if (Mathf.Abs(toPlayer.y) > maxHeightDifference) return false;

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
