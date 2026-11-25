using UnityEngine;

// Баланс Көлеңке-қасқыра. Общие числа (HP, урон, патруль, зрение, замах, отдых) — в EnemyStats.
[CreateAssetMenu(fileName = "Wolf_Stats", menuName = "Shyraq/Shadow Wolf Stats")]
public class ShadowWolfStats : EnemyStats
{
    [Header("Волк")]
    [Tooltip("Угол обзора вперёд в патруле, градусов")]
    [Range(0f, 360f)] public float viewAngle = 160f;
    [Tooltip("Скорость погони (у Ару бег 6)")]
    [Min(0f)] public float chaseSpeed = 7.2f;
    [Tooltip("С какого расстояния начинает замах")]
    [Min(0f)] public float biteRange = 1.5f;
    [Tooltip("Сколько длится рывок-укус, секунд")]
    [Min(0f)] public float biteTime = 0.25f;
    [Tooltip("Скорость рывка")]
    [Min(0f)] public float biteSpeed = 9f;
}
