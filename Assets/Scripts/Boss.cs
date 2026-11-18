using System.Collections;
using UnityEngine;

// Вожак Жеті қарақшы. Наследник Lightable: луч вызывает Lit().
// Без автомата состояний (это A10): только hp, номер фазы (int), таймеры и корутины.
// На объекте: SpriteRenderer + BoxCollider2D (Is Trigger) — его ловят луч и Жарқыл,
// Rigidbody2D (Kinematic) — чтобы ходить, как дух.
// Дочерний объект LassoZone: BoxCollider2D (Is Trigger) + DamageZone, выключен, пока нет броска.
public class Boss : Lightable
{
    [Header("Здоровье и фазы")]
    public int maxHp = 14;
    public float revealTime = 1.2f;        // сколько секунд босс остаётся проявленным после луча

    [Header("Патруль в тени (как у духа)")]
    public float patrolDistance = 8f;      // на сколько единиц уходит влево от старта
    public float patrolSpeed = 1.5f;       // единиц в секунду

    [Header("Аркан")]
    public GameObject lassoZone;           // зона удара перед боссом
    public float attackDelay = 2.2f;       // фаза 1: бросок раз в 2,2 сек
    public float attackDelayPhase2 = 2.2f; // фаза 2: сейчас как в фазе 1 (можно сделать чаще)

    [Header("Малые духи (фаза 2)")]
    public GameObject smallSpiritPrefab;
    public Transform[] spawnPoints;        // откуда появляются духи
    public float summonDelay = 7f;
    public int summonCount = 2;            // сколько духов за раз

    [Header("Спрайты")]
    public Sprite shadowSprite;            // в тени — неуязвим
    public Sprite idleSprite;              // проявлен, фаза 1
    public Sprite phase2Sprite;            // проявлен, фаза 2 (красный)
    public Sprite attackSprite0;           // замах
    public Sprite attackSprite1;           // бросок
    public Sprite hurtSprite;              // получил урон

    int hp;
    int phase = 1;
    float revealTimer;                     // > 0 — босс проявлен
    float attackTimer;
    float summonTimer;
    float hurtTimer;                       // > 0 — показываем кадр урона
    bool attacking;
    Sprite attackFrame;                    // какой кадр броска показывать сейчас
    SpriteRenderer sr;
    Rigidbody2D rb;
    Vector2 pointA, pointB;                // концы патруля
    float patrolTimer;                     // идёт только в тени

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        pointA = transform.position;
        pointB = pointA + Vector2.left * patrolDistance;
        hp = maxHp;
        attackTimer = attackDelay;
        summonTimer = summonDelay;
        lassoZone.SetActive(false);
    }

    void Update()
    {
        // 1. Свет: луч проявляет босса, и он держится ещё revealTime секунд (как тайная тропа)
        if (IsLit()) revealTimer = revealTime;
        else revealTimer -= Time.deltaTime;

        // 2. Таймер аркана: дошёл до нуля — бросаем и заводим заново
        attackTimer -= Time.deltaTime;
        if (attackTimer <= 0f && !attacking)
        {
            StartCoroutine(ThrowLasso());
            attackTimer = (phase == 1) ? attackDelay : attackDelayPhase2;
        }

        // 3. Фаза 2: раз в summonDelay секунд зовём духов
        if (phase == 2)
        {
            summonTimer -= Time.deltaTime;
            if (summonTimer <= 0f)
            {
                SummonSpirits();
                summonTimer = summonDelay;
            }
        }

        hurtTimer -= Time.deltaTime;
        UpdateSprite();
    }

    // Патруль: в тени ходит туда-сюда, проявлен — стоит (его «время» не идёт, как у духа)
    void FixedUpdate()
    {
        if (!IsRevealed()) patrolTimer += Time.fixedDeltaTime;
        float t = Mathf.PingPong(patrolTimer * patrolSpeed / patrolDistance, 1f);
        rb.MovePosition(Vector2.Lerp(pointA, pointB, t));
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
        hurtTimer = 0.2f;                  // кадр урона на 0,2 сек

        if (phase == 1 && hp < maxHp / 2f) phase = 2;   // меньше половины — фаза 2

        if (hp <= 0)
        {
            LevelManager.instance.BossDefeated();
            gameObject.SetActive(false);   // босс исчезает
        }
        return true;
    }

    // Корутина броска: замах 0,3 сек, потом зона урона включена 0,5 сек
    IEnumerator ThrowLasso()
    {
        attacking = true;
        attackFrame = attackSprite0;
        yield return new WaitForSeconds(0.3f);

        attackFrame = attackSprite1;
        lassoZone.SetActive(true);
        yield return new WaitForSeconds(0.5f);

        lassoZone.SetActive(false);
        attacking = false;
    }

    // summonCount малых духов, по очереди в точках из списка
    void SummonSpirits()
    {
        for (int i = 0; i < summonCount; i++)
        {
            Transform point = spawnPoints[i % spawnPoints.Length];   // % — остаток: 0, 1, 0, 1… если точек меньше, чем духов
            Instantiate(smallSpiritPrefab, point.position, Quaternion.identity);
        }
    }

    // Какую картинку показать: урон > бросок > тень > проявлен (фаза 1 или 2)
    void UpdateSprite()
    {
        if (hurtTimer > 0f) sr.sprite = hurtSprite;
        else if (attacking) sr.sprite = attackFrame;
        else if (!IsRevealed()) sr.sprite = shadowSprite;
        else if (phase == 2) sr.sprite = phase2Sprite;
        else sr.sprite = idleSprite;
    }
}
