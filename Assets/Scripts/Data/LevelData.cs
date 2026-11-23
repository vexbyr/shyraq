using System.Collections.Generic;
using UnityEngine;

// Один уровень в виде данных: какая сцена, где появляется Ай, какие враги и предметы где стоят,
// сколько звёзд нужно собрать и с каким маслом начинаем.
// Геометрия уровня (земля, платформы, тайные тропы, ворота, шипы) остаётся в сцене.
// Сцену заполняет LevelLoader — один и тот же скрипт на всех уровнях.
[CreateAssetMenu(fileName = "Level_01", menuName = "Shyraq/Level Data")]
public class LevelData : ScriptableObject
{
    [Header("Уровень")]
    [Tooltip("Номер уровня: 1, 2, 3 (по нему хранится прогресс в сохранении)")]
    [Min(1)] public int levelNumber = 1;
    [Tooltip("Название в меню")]
    public string displayName = "Dala";
    [Tooltip("Имя сцены (должна быть в Build Settings)")]
    public string sceneName = "Level1";
    [Tooltip("Следующий уровень. Пусто — это последний уровень (босс)")]
    public LevelData nextLevel;

    [Header("Цель и старт")]
    [Tooltip("Сколько звёзд нужно собрать, чтобы открылись ворота")]
    [Min(0)] public int starsRequired = 6;
    [Tooltip("Масла в фонаре в начале уровня")]
    [Min(0f)] public float startOil = 100f;
    [Tooltip("Целевое время, секунд: быстрее — больше очков (жёсткого лимита нет)")]
    [Min(1f)] public float parTimeSeconds = 90f;
    [Tooltip("Где появляется Ай")]
    public Vector2 playerSpawn = Vector2.zero;

    [Header("Что появляется на уровне")]
    public List<EnemySpawn> enemies = new List<EnemySpawn>();
    public List<PickupSpawn> pickups = new List<PickupSpawn>();
}

// Враг на уровне: какой префаб и где. Маршрут — смещения от позиции появления
// (волк: левая и правая точка патруля; дух: начало и конец пути). Оба нуля — маршрут из префаба.
[System.Serializable]
public class EnemySpawn
{
    public GameObject prefab;
    public Vector2 position;
    public Vector2 routeStart;
    public Vector2 routeEnd;
}

// Предмет на уровне: звезда или бутылочка масла
[System.Serializable]
public class PickupSpawn
{
    public GameObject prefab;
    public Vector2 position;
}
