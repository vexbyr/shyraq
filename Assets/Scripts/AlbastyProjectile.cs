using UnityEngine;

// Снаряд Албасты (сгусток тьмы). Летит прямо в направлении, которое выбрала Албасты.
// Исчезает: попал в Ай (урон один раз), ударился о землю/стену или кончилось время жизни.
// Скорость, время жизни и урон задаёт Албасты через Launch — числа хранятся в ассете Albasty_Stats.
// На префабе: SpriteRenderer, Rigidbody2D (Dynamic, Gravity Scale 0), CircleCollider2D (Is Trigger) — как у Жарқыла.
public class AlbastyProjectile : MonoBehaviour
{
    [SerializeField] Sprite[] frames;          // кадры мерцания (0, 1), можно оставить пустым
    [SerializeField] float frameTime = 0.15f;  // сколько секунд показываем один кадр

    int damage;
    SpriteRenderer sr;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    // Вызывает Албасты сразу после Instantiate
    public void Launch(Vector2 direction, float speed, float lifetime, int damageAmount)
    {
        damage = damageAmount;
        GetComponent<Rigidbody2D>().linearVelocity = direction * speed;
        Destroy(gameObject, lifetime);         // ни во что не попал — исчезнет сам
    }

    // Мерцание: номер кадра растёт каждые frameTime секунд, % — по кругу 0, 1, 0, 1…
    void Update()
    {
        if (frames.Length == 0) return;
        int frame = (int)(Time.time / frameTime) % frames.Length;
        sr.sprite = frames[frame];
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
