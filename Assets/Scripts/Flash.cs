using UnityEngine;

// Жарқыл — вспышка-снаряд света. Летит прямо и исчезает.
// Скорость, время жизни и урон — в ассете Player_Stats.
// На префабе: Rigidbody2D (Gravity Scale 0) + CircleCollider2D (Is Trigger).
public class Flash : MonoBehaviour
{
    [SerializeField] PlayerStats stats;

    void Start()
    {
        if (stats == null)
        {
            Debug.LogError(name + ": не назначен ассет Player Stats", this);
            Destroy(gameObject);
            return;
        }
        // transform.right — «вперёд» с учётом поворота, который дал фонарь
        GetComponent<Rigidbody2D>().linearVelocity = transform.right * stats.flashSpeed;
        Destroy(gameObject, stats.flashLifetime);   // не долетела ни до чего — исчезнет сама
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (stats == null) return;

        // Попала во врага (дух или любой наследник Enemy) — ранит его и исчезает
        Enemy enemy = other.GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.TakeHit(stats.flashDamage);
            Destroy(gameObject);
            return;
        }

        // Попала в босса: проявлен — урон и исчезает; в тени — отскакивает назад без урона
        Boss boss = other.GetComponent<Boss>();
        if (boss != null)
        {
            if (boss.TakeHit(stats.flashDamage))
            {
                Destroy(gameObject);
            }
            else
            {
                Rigidbody2D rb = GetComponent<Rigidbody2D>();
                rb.linearVelocity = -rb.linearVelocity;   // минус = летит обратно
            }
            return;
        }

        // Ударилась о землю или стену — исчезает
        if (other.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            Destroy(gameObject);
        }
    }
}
