using UnityEngine;

// Жарқыл — вспышка-снаряд света. Летит прямо и исчезает.
// На префабе: Rigidbody2D (Gravity Scale 0) + CircleCollider2D (Is Trigger).
public class Flash : MonoBehaviour
{
    public float speed = 14f;
    public float lifeTime = 1.5f;
    public int damage = 1;           // пригодится для духов и босса

    void Start()
    {
        // transform.right — «вперёд» с учётом поворота, который дал фонарь
        GetComponent<Rigidbody2D>().linearVelocity = transform.right * speed;
        Destroy(gameObject, lifeTime);   // не долетела ни до чего — исчезнет сама
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Попала во врага (дух или любой наследник Enemy) — ранит его и исчезает
        Enemy enemy = other.GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.TakeHit(damage);
            Destroy(gameObject);
            return;
        }

        // Попала в босса: проявлен — урон и исчезает; в тени — отскакивает назад без урона
        Boss boss = other.GetComponent<Boss>();
        if (boss != null)
        {
            if (boss.TakeHit(damage))
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
