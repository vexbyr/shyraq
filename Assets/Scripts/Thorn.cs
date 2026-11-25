using UnityEngine;

// Шипы. Пока Ару касается их — урон (itemValues.thornDamage).
// Повторные удары гасит неуязвимость в PlayerHealth.
// На объекте: SpriteRenderer + Collider2D (Is Trigger).
public class Thorn : MonoBehaviour
{
    [SerializeField] ItemValues itemValues;

    void Awake()
    {
        if (itemValues == null)
        {
            Debug.LogError(name + ": не назначен ассет Item Values", this);
            enabled = false;
        }
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (!enabled) return;
        if (other.CompareTag("Player"))
        {
            other.GetComponent<PlayerHealth>().TakeDamage(itemValues.thornDamage);
        }
    }
}
