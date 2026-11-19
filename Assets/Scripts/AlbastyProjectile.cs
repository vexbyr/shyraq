using UnityEngine;

// Снаряд Албасты (сгусток тьмы). Летит прямо в направлении, которое выбрала Албасты.
// Исчезает: попал в Ай (урон один раз), ударился о землю/стену или кончилось время жизни.
// Скорость, время жизни и урон задаёт Албасты через Launch — числа хранятся в её префабе.
// На префабе: SpriteRenderer, Rigidbody2D (Dynamic, Gravity Scale 0), CircleCollider2D (Is Trigger) — как у Жарқыла.
public class AlbastyProjectile : MonoBehaviour
{
    int damage;

    // Вызывает Албасты сразу после Instantiate
    public void Launch(Vector2 direction, float speed, float lifetime, int damageAmount)
    {
        damage = damageAmount;
        GetComponent<Rigidbody2D>().linearVelocity = direction * speed;
        Destroy(gameObject, lifetime);         // ни во что не попал — исчезнет сам
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<PlayerHealth>().TakeDamage(damage);
            Destroy(gameObject);               // исчез сразу — второй раз не ударит
            return;
        }

        if (other.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            Destroy(gameObject);
        }
    }
}
