using UnityEngine;

// Дух: ходит туда-сюда (PingPong + Lerp), в свете замирает и не кусается.
// Наследник Enemy (а тот — Lightable): hp, drop и TakeHit берёт из Enemy.
// Баланс (HP, скорость, урон) — в ассете Spirit_Stats. Маршрут (direction, distance) — свой у каждого духа на уровне.
// На объекте: Rigidbody2D (Kinematic) + CircleCollider2D (Is Trigger).
public class Spirit : Enemy
{
    [SerializeField] EnemyStats stats;
    public Vector2 direction = Vector2.right;   // куда идёт патруль: (1,0) вправо, (0,1) вверх — летающий дух
    public float distance = 4f;        // на сколько единиц уходит от старта (маршрут, а не баланс)
    public Sprite normalSprite;        // обычный дух
    public Sprite frozenSprite;        // замёрзший (в свете)

    Rigidbody2D rb;
    SpriteRenderer sr;
    Vector2 pointA, pointB;
    float timer;                       // свой «счётчик времени», стоит, пока дух в свете

    protected override EnemyStats Stats() { return stats; }

    // Маршрут от LevelLoader (вызывается до Start): путь от start до end относительно точки появления
    public override void SetRoute(Vector2 start, Vector2 end)
    {
        transform.position += (Vector3)start;
        direction = end - start;
        distance = direction.magnitude;
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        pointA = transform.position;
        pointB = pointA + direction.normalized * distance;   // normalized: длина 1, чтобы distance был в единицах
    }

    void FixedUpdate()
    {
        if (!IsLit()) timer += Time.fixedDeltaTime;          // в свете время духа не идёт

        // PingPong даёт число, которое ходит 0 → 1 → 0 → 1...
        float t = Mathf.PingPong(timer * stats.moveSpeed / distance, 1f);
        rb.MovePosition(Vector2.Lerp(pointA, pointB, t));    // Lerp: точка между A и B
    }

    void Update()
    {
        sr.sprite = IsLit() ? frozenSprite : normalSprite;   // ? : — «если в свете, то замёрзший, иначе обычный»
    }

    // Пока Ару касается духа — урон (неуязвимость после удара делает PlayerHealth)
    void OnTriggerStay2D(Collider2D other)
    {
        if (!enabled || IsLit()) return;                     // выключен (нет ассета) или замёрз — безопасен
        if (other.CompareTag("Player"))
        {
            other.GetComponent<PlayerHealth>().TakeDamage(stats.damage);
        }
    }
}
