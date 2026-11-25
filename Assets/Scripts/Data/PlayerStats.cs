using UnityEngine;

// Баланс Ару: движение, здоровье, фонарь и Жарқыл.
// Масло в начале уровня задаёт уровень (LevelData), здесь — только максимум и расход.
[CreateAssetMenu(fileName = "Player_Stats", menuName = "Shyraq/Player Stats")]
public class PlayerStats : ScriptableObject
{
    [Header("Движение")]
    [Tooltip("Скорость бега, единиц в секунду")]
    [Min(0f)] public float moveSpeed = 6f;
    [Tooltip("Скорость вверх в начале прыжка")]
    [Min(0f)] public float jumpForce = 12f;
    [Tooltip("Прыжков в воздухе подряд (2 — двойной прыжок)")]
    [Min(1)] public int maxJumps = 2;

    [Header("Здоровье")]
    [Tooltip("Сердец в начале уровня (в HUD нарисовано 3)")]
    [Min(1)] public int maxHP = 3;
    [Tooltip("Неуязвимость после удара, секунд")]
    [Min(0f)] public float invincibleTime = 1.5f;
    [Tooltip("Скорость вверх, с которой Ару подбрасывает от удара")]
    [Min(0f)] public float hurtBounce = 8f;

    [Header("Фонарь и масло")]
    [Tooltip("Максимум масла (полная полоска)")]
    [Min(1f)] public float maxOil = 100f;
    [Tooltip("Масла в секунду, пока горит луч")]
    [Min(0f)] public float beamOilPerSecond = 10f;

    [Header("Жарқыл")]
    [Tooltip("Масла за один выстрел")]
    [Min(0f)] public float flashOilCost = 15f;
    [Tooltip("Урон по врагу и боссу")]
    [Min(0)] public int flashDamage = 1;
    [Tooltip("Скорость полёта, единиц в секунду")]
    [Min(0f)] public float flashSpeed = 14f;
    [Tooltip("Через сколько секунд исчезает, если ни во что не попал")]
    [Min(0f)] public float flashLifetime = 1.5f;
    [Tooltip("Пауза между выстрелами, секунд (0 — без паузы)")]
    [Min(0f)] public float flashCooldown = 0f;
}
