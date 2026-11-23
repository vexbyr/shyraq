using UnityEngine;

// Баланс вожака Жеті қарақшы.
// Из EnemyStats: maxHP, damage (удар аркана), moveSpeed (патруль в тени),
// windUpTime (замах аркана), cooldownTime (пауза между бросками в фазе 1).
[CreateAssetMenu(fileName = "Boss_Stats", menuName = "Shyraq/Boss Stats")]
public class BossStats : EnemyStats
{
    [Header("Свет и фазы")]
    [Tooltip("Сколько секунд босс проявлен после луча")]
    [Min(0f)] public float revealTime = 1.2f;
    [Tooltip("HP ниже этой доли от maxHP — фаза 2")]
    [Range(0f, 1f)] public float phase2HpFraction = 0.5f;

    [Header("Патруль")]
    [Tooltip("На сколько единиц уходит влево от старта")]
    [Min(0f)] public float patrolDistance = 8f;

    [Header("Аркан")]
    [Tooltip("Пауза между бросками в фазе 2, секунд")]
    [Min(0f)] public float cooldownTimePhase2 = 2.2f;
    [Tooltip("Сколько секунд зона удара включена")]
    [Min(0f)] public float lassoActiveTime = 0.5f;
    [Tooltip("Ближний край зоны удара от центра босса, единиц")]
    [Min(0f)] public float lassoInnerEdge = 1.5f;
    [Tooltip("Длина аркана = эта доля от длины луча Ай")]
    [Range(0f, 2f)] public float lassoBeamFraction = 0.85f;

    [Header("Малые духи (фаза 2)")]
    [Tooltip("Раз в сколько секунд зовёт духов")]
    [Min(0f)] public float summonDelay = 7f;
    [Tooltip("Сколько духов за раз")]
    [Min(0)] public int summonCount = 2;
}
