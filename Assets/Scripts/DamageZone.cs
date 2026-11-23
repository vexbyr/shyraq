using UnityEngine;

// Зона урона аркана босса. Пока Ай внутри — получает урон (stats.damage из Boss_Stats).
// Повторные удары гасит неуязвимость в PlayerHealth.
public class DamageZone : MonoBehaviour
{
    [SerializeField] EnemyStats stats;   // чей это удар: у аркана — Boss_Stats

    void Awake()
    {
        if (stats == null)
        {
            Debug.LogError(name + ": не назначен ассет баланса (Stats)", this);
            enabled = false;
        }
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (!enabled) return;
        if (other.CompareTag("Player"))
        {
            other.GetComponent<PlayerHealth>().TakeDamage(stats.damage);
        }
    }
}
