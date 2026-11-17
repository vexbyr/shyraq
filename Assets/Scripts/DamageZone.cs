using UnityEngine;

// Зона урона (аркан босса). Пока Ай внутри — получает урон.
// Повторные удары гасит неуязвимость в PlayerHealth.
public class DamageZone : MonoBehaviour
{
    public int damage = 1;

    void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<PlayerHealth>().TakeDamage(damage);
        }
    }
}
