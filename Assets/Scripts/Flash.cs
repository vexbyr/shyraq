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
        // Попала в духа — ранит его и исчезает
        Spirit spirit = other.GetComponent<Spirit>();
        if (spirit != null)
        {
            spirit.TakeHit(damage);
            Destroy(gameObject);
            return;
        }

        // Ударилась о землю или стену — исчезает
        if (other.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            Destroy(gameObject);
        }
    }
}
