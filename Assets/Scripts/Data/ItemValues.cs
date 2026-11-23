using UnityEngine;

// Числа предметов и объектов мира: масло, шипы, тайные тропы.
[CreateAssetMenu(fileName = "Item_Values", menuName = "Shyraq/Item Values")]
public class ItemValues : ScriptableObject
{
    [Header("Масло")]
    [Tooltip("Сколько масла даёт бутылочка на уровне")]
    [Min(0f)] public float oilBottleAmount = 30f;
    [Tooltip("Сколько масла даёт бутылочка, выпавшая из малого духа")]
    [Min(0f)] public float oilDropAmount = 15f;
    [Tooltip("Через сколько секунд возрождается бутылочка на арене босса")]
    [Min(0f)] public float oilRespawnTime = 20f;

    [Header("Мир")]
    [Tooltip("Урон шипов")]
    [Min(0)] public int thornDamage = 1;
    [Tooltip("Сколько секунд тайная тропа держится после луча")]
    [Min(0f)] public float secretPathStayTime = 1.5f;
}
