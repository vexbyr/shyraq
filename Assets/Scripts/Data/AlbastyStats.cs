using UnityEngine;

// Баланс Албасты. windUpTime — прицеливание, cooldownTime — перезарядка, damage — урон снаряда.
[CreateAssetMenu(fileName = "Albasty_Stats", menuName = "Shyraq/Albasty Stats")]
public class AlbastyStats : EnemyStats
{
    [Header("Снаряд")]
    [Tooltip("Скорость сгустка тьмы, единиц в секунду")]
    [Min(0f)] public float projectileSpeed = 6f;
    [Tooltip("Через сколько секунд снаряд исчезает сам")]
    [Min(0f)] public float projectileLifetime = 2.5f;
}
