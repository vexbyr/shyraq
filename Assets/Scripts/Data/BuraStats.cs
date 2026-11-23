using UnityEngine;

// Баланс Қара буры. windUpTime — «роет копытом», cooldownTime — оглушение после разбега.
[CreateAssetMenu(fileName = "Bura_Stats", menuName = "Shyraq/Bura Stats")]
public class BuraStats : EnemyStats
{
    [Header("Разбег")]
    [Tooltip("Ай выше или ниже этого — не начинает замах")]
    [Min(0f)] public float maxHeightDifference = 3f;
    [Tooltip("Скорость разбега, единиц в секунду")]
    [Min(0f)] public float chargeSpeed = 10f;
    [Tooltip("Дольше этого не бежит, секунд")]
    [Min(0f)] public float chargeMaxTime = 1.8f;
}
