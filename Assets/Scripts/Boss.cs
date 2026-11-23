using System.Collections;
using UnityEngine;

// Вожак Жеті қарақшы. Наследник Lightable: луч вызывает Lit().
// Без автомата состояний (это A10): только hp, номер фазы (int), таймеры и корутины.
// Все числа баланса — в ассете Boss_Stats (BossStats), читаем их в момент использования.
// На объекте: SpriteRenderer + BoxCollider2D (Is Trigger) — его ловят луч и Жарқыл,
// Rigidbody2D (Kinematic) — чтобы ходить, как дух.
// Дочерний объект LassoZone: BoxCollider2D (Is Trigger) + DamageZone, выключен, пока нет броска.
public class Boss : Lightable
{
    [Header("Баланс (HP, фазы, аркан, духи)")]
    [SerializeField] BossStats stats;

    [Header("Ссылки")]
    public GameObject lassoZone;           // зона удара перед боссом
    public GameObject smallSpiritPrefab;
    public Transform[] spawnPoints;        // откуда появляются духи

    [Header("Спрайты")]
    public Sprite shadowSprite;            // в тени — неуязвим
    public Sprite idleSprite;              // проявлен, фаза 1 (кадр 0)
    [SerializeField] Sprite idleSprite1;   // проявлен, фаза 1 (кадр 1), можно оставить пустым
    [SerializeField] float idleFrameTime = 0.5f;   // сколько секунд показываем один кадр покоя (вид, не баланс)
    [SerializeField] float hurtFlashTime = 0.2f;   // сколько секунд показываем кадр урона (вид, не баланс)
    public Sprite phase2Sprite;            // проявлен, фаза 2 (красный)
    public Sprite attackSprite0;           // замах
    public Sprite attackSprite1;           // бросок
    public Sprite hurtSprite;              // получил урон

    int hp;                                // текущее здоровье (только во время игры)
    int phase = 1;
    float revealTimer;                     // > 0 — босс проявлен
    float attackTimer;
    float summonTimer;
    float hurtTimer;                       // > 0 — показываем кадр урона
    bool attacking;
    Sprite attackFrame;                    // какой кадр броска показывать сейчас
    SpriteRenderer sr;
    Rigidbody2D rb;
    Vector2 pointA;                        // правый конец патруля (старт)
    float patrolTimer;                     // идёт только в тени
    Transform player;                      // Ай — к ней поворачивается аркан
    BoxCollider2D beamCollider;            // луч Ай: от его длины считается длина аркана
    int lassoSide = -1;                    // куда бросок: -1 влево, 1 вправо (выбирается в начале замаха)
    Camera cam;

    void Awake()
    {
        if (stats == null)
        {
            Debug.LogError(name + ": не назначен ассет баланса (Boss Stats)", this);
            enabled = false;
        }
    }

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindWithTag("Player").transform;
        beamCollider = player.GetComponentInChildren<LightBeam>(true).GetComponent<BoxCollider2D>();
        cam = Camera.main;
        pointA = transform.position;
        hp = stats.maxHP;
        attackTimer = stats.cooldownTime;
        summonTimer = stats.summonDelay;
        PlaceLassoZone();
        lassoZone.SetActive(false);
    }

    // Ставим зону аркана на сторону lassoSide. Длина = доля от длины луча Ай (size.x коллайдера луча).
    // Красная подсветка — на самой зоне, того же размера, переезжает вместе с ней
    void PlaceLassoZone()
    {
        float lassoLength = beamCollider.size.x * stats.lassoBeamFraction;

        BoxCollider2D zone = lassoZone.GetComponent<BoxCollider2D>();
        zone.size = new Vector2(lassoLength, zone.size.y);
        lassoZone.GetComponent<SpriteRenderer>().size = zone.size;

        float centerX = lassoSide * (stats.lassoInnerEdge + lassoLength / 2f);   // -1 — слева, 1 — справа
        lassoZone.transform.localPosition = new Vector3(centerX, 0f, 0f);
    }

    void Update()
    {
        // 1. Свет: луч проявляет босса, и он держится ещё revealTime секунд (как тайная тропа)
        if (IsLit()) revealTimer = stats.revealTime;
        else revealTimer -= Time.deltaTime;

        // 2. Таймер аркана: дошёл до нуля — бросаем и заводим заново. Вне кадра не бросает (ждёт, пока появится)
        attackTimer -= Time.deltaTime;
        if (attackTimer <= 0f && !attacking && IsOnScreen())
        {
            StartCoroutine(ThrowLasso());
            attackTimer = (phase == 1) ? stats.cooldownTime : stats.cooldownTimePhase2;
        }

        // 3. Фаза 2: раз в summonDelay секунд зовём духов. Вне кадра не зовёт (ждёт, пока появится)
        if (phase == 2)
        {
            summonTimer -= Time.deltaTime;
            if (summonTimer <= 0f && IsOnScreen())
            {
                SummonSpirits();
                summonTimer = stats.summonDelay;
            }
        }

        hurtTimer -= Time.deltaTime;
        UpdateSprite();
    }

    // Патруль: в тени ходит туда-сюда, проявлен — стоит (его «время» не идёт, как у духа)
    void FixedUpdate()
    {
        if (!IsRevealed()) patrolTimer += Time.fixedDeltaTime;
        Vector2 pointB = pointA + Vector2.left * stats.patrolDistance;
        float t = Mathf.PingPong(patrolTimer * stats.moveSpeed / stats.patrolDistance, 1f);
        rb.MovePosition(Vector2.Lerp(pointA, pointB, t));
    }

    // В кадре ли камеры (как у Албасты и Буры). Viewport: левый нижний угол экрана = (0,0), правый верхний = (1,1)
    bool IsOnScreen()
    {
        Vector3 p = cam.WorldToViewportPoint(transform.position);
        float m = stats.screenMargin;
        return p.x > m && p.x < 1f - m && p.y > m && p.y < 1f - m;
    }

    // Проявлен ли босс сейчас (Жарқыл спрашивает перед ударом)
    public bool IsRevealed()
    {
        return revealTimer > 0f;
    }

    // Попал Жарқыл. Возвращает true, если урон прошёл.
    public bool TakeHit(int damage)
    {
        if (!IsRevealed()) return false;   // в тени неуязвим — снаряд отскочит

        hp -= damage;
        hurtTimer = hurtFlashTime;         // кадр урона

        if (phase == 1 && hp < stats.maxHP * stats.phase2HpFraction) phase = 2;   // меньше доли (по умолчанию половины) — фаза 2

        if (hp <= 0)
        {
            LevelManager.instance.BossDefeated();
            gameObject.SetActive(false);   // босс исчезает
        }
        return true;
    }

    // Корутина броска: замах windUpTime, потом зона урона включена lassoActiveTime
    IEnumerator ThrowLasso()
    {
        attacking = true;
        // Направление выбираем один раз — знак разницы X между Ай и боссом. До конца удара оно не меняется
        lassoSide = (int)Mathf.Sign(player.position.x - transform.position.x);
        PlaceLassoZone();
        attackFrame = attackSprite0;
        yield return new WaitForSeconds(stats.windUpTime);

        attackFrame = attackSprite1;
        lassoZone.SetActive(true);
        yield return new WaitForSeconds(stats.lassoActiveTime);

        lassoZone.SetActive(false);
        attacking = false;
    }

    // summonCount малых духов, по очереди в точках из списка
    void SummonSpirits()
    {
        for (int i = 0; i < stats.summonCount; i++)
        {
            Transform point = spawnPoints[i % spawnPoints.Length];   // % — остаток: 0, 1, 0, 1… если точек меньше, чем духов
            Instantiate(smallSpiritPrefab, point.position, Quaternion.identity);
        }
    }

    // Какую картинку показать: урон > бросок > тень > проявлен (фаза 1 или 2)
    void UpdateSprite()
    {
        sr.flipX = attacking && lassoSide > 0;   // кадры броска нарисованы влево, вправо — отражаем

        if (hurtTimer > 0f) sr.sprite = hurtSprite;
        else if (attacking) sr.sprite = attackFrame;
        else if (!IsRevealed()) sr.sprite = shadowSprite;
        else if (phase == 2) sr.sprite = phase2Sprite;
        else sr.sprite = IdleFrame();
    }

    // Покой из двух кадров: чётный отрезок времени — кадр 0, нечётный — кадр 1
    Sprite IdleFrame()
    {
        if (idleSprite1 == null) return idleSprite;
        int frame = (int)(Time.time / idleFrameTime) % 2;
        return (frame == 0) ? idleSprite : idleSprite1;
    }
}
