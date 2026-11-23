using UnityEngine;

// Баланс врага: числа, которые есть почти у всех врагов.
// Ассет создаётся через Assets → Create → Shyraq → Enemy Stats (для духов).
// У волка, Албасты, Буры и босса свои наследники с дополнительными числами.
// Скрипты только ЧИТАЮТ этот ассет: текущее HP и таймеры живут в самих врагах.
[CreateAssetMenu(fileName = "Enemy_Stats", menuName = "Shyraq/Enemy Stats")]
public class EnemyStats : ScriptableObject
{
    [Header("Здоровье и урон")]
    [Tooltip("Сколько урона Жарқыла выдерживает")]
    [Min(1)] public int maxHP = 1;
    [Tooltip("Сколько сердец Ай теряет за одно попадание")]
    [Min(0)] public int damage = 1;

    [Header("Движение")]
    [Tooltip("Скорость патруля, единиц в секунду (0 — стоит на месте)")]
    [Min(0f)] public float moveSpeed = 2f;

    [Header("Атака")]
    [Tooltip("Дальность зрения, единиц (0 — не ищет Ай)")]
    [Min(0f)] public float visionRange = 0f;
    [Tooltip("Замах перед атакой, секунд — время Ай увернуться")]
    [Min(0f)] public float windUpTime = 0f;
    [Tooltip("Отдых после атаки, секунд")]
    [Min(0f)] public float cooldownTime = 0f;
    [Tooltip("Отступ от края кадра (доля экрана): вне кадра враг не нападает")]
    [Range(0f, 0.5f)] public float screenMargin = 0f;
}
