using UnityEngine;
using UnityEngine.SceneManagement;

// Один и тот же скрипт во всех сценах уровней. В Awake (до всех Start) читает LevelData и:
// ставит Ай на точку старта, наливает масло, создаёт врагов и предметы.
// Какой уровень — берёт из GameSession.CurrentLevel (выбран в меню).
// Если сцену запустили прямо в редакторе — берёт свой уровень sceneLevel.
// Объект LevelLoader должен стоять в (0, 0, 0): созданные объекты — его дети.
public class LevelLoader : MonoBehaviour
{
    [Tooltip("Уровень этой сцены: используется, если сцену запустили не из меню")]
    [SerializeField] LevelData sceneLevel;

    void Awake()
    {
        LevelData level = ChooseLevel();
        if (level == null)
        {
            Debug.LogError(name + ": нет LevelData для сцены " + SceneManager.GetActiveScene().name, this);
            return;
        }
        GameSession.CurrentLevel = level;   // дальше LevelManager и экран итогов читают его отсюда

        PlacePlayer(level);
        SpawnEnemies(level);
        SpawnPickups(level);
    }

    // Уровень из меню подходит, только если это уровень именно этой сцены
    LevelData ChooseLevel()
    {
        LevelData fromMenu = GameSession.CurrentLevel;
        if (fromMenu != null && fromMenu.sceneName == SceneManager.GetActiveScene().name) return fromMenu;
        return sceneLevel;
    }

    // Ай уже есть в сцене (внутри Core вместе с HUD) — переносим её на старт и наливаем масло
    void PlacePlayer(LevelData level)
    {
        GameObject player = GameObject.FindWithTag("Player");
        player.transform.position = level.playerSpawn;
        player.GetComponent<Lantern>().oil = level.startOil;
    }

    void SpawnEnemies(LevelData level)
    {
        foreach (EnemySpawn spawn in level.enemies)
        {
            GameObject enemy = Instantiate(spawn.prefab, spawn.position, Quaternion.identity, transform);

            // Свой маршрут патруля (если задан): Enemy сам решает, что с ним делать
            Enemy patrol = enemy.GetComponent<Enemy>();
            if (patrol != null && spawn.routeStart != spawn.routeEnd) patrol.SetRoute(spawn.routeStart, spawn.routeEnd);
        }
    }

    void SpawnPickups(LevelData level)
    {
        int stars = 0;
        foreach (PickupSpawn spawn in level.pickups)
        {
            GameObject pickup = Instantiate(spawn.prefab, spawn.position, Quaternion.identity, transform);
            if (pickup.CompareTag("Coin")) stars++;
        }

        // Ошибка в данных: звёзд на уровне меньше, чем нужно — ворота никогда не откроются
        if (stars < level.starsRequired)
            Debug.LogWarning(level.name + ": звёзд " + stars + ", а нужно " + level.starsRequired, level);
    }
}
