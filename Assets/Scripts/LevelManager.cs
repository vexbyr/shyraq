using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

// Один на сцену: считает звёзды, открывает ворота, грузит следующий уровень.
// Сколько звёзд нужно и какой уровень следующий — из LevelData (GameSession.CurrentLevel, ставит LevelLoader).
public class LevelManager : MonoBehaviour
{
    public static LevelManager instance;   // «сам себя» — чтобы другие скрипты нашли его как LevelManager.instance

    public Gate gate;                      // ворота этого уровня
    public TMP_Text starText;              // счётчик звёзд в HUD
    public SpriteRenderer sky;             // картинка неба; ночью она затемнена цветом, в конце светлеет
    public GameObject starsInSky;          // уровень с боссом: звёзды на небе (выключены до победы)
    public string winScene = "Win";
    [SerializeField] ScoreSettings scoreSettings;   // как считать очки в конце уровня

    int starsTotal;
    int starsCollected;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        // Ворота на каждом уровне свои: находим их по тегу Exit (в сцене одни ворота).
        // На уровне с боссом ворот нет — тогда gate остаётся пустым.
        GameObject exit = GameObject.FindGameObjectWithTag("Exit");
        if (gate == null && exit != null) gate = exit.GetComponent<Gate>();

        // Сколько звёзд нужно — цель уровня из LevelData
        starsTotal = GameSession.CurrentLevel.starsRequired;
        UpdateStarText();
    }

    public void AddStar()
    {
        starsCollected++;
        UpdateStarText();

        if (starsCollected >= starsTotal && gate != null)
        {
            gate.Open();                   // все звёзды собраны — ворота открываются
            StartCoroutine(BrightenSky());
        }
    }

    // Босс побеждён: небо светлеет, звёзды возвращаются на небо, потом сцена Win
    public void BossDefeated()
    {
        SaveResult();
        StartCoroutine(Finale());
    }

    IEnumerator Finale()
    {
        if (starsInSky != null) starsInSky.SetActive(true);   // звёзды снова на небе
        yield return StartCoroutine(BrightenSky());            // ждём, пока небо посветлеет (2 сек)
        yield return new WaitForSeconds(1.5f);
        SceneManager.LoadScene(winScene);
    }

    // Корутина: за 2 секунды небо плавно светлеет (цвет спрайта → белый = картинка без затемнения)
    IEnumerator BrightenSky()
    {
        Color startColor = sky.color;
        for (float t = 0f; t < 1f; t += Time.deltaTime / 2f)
        {
            sky.color = Color.Lerp(startColor, Color.white, t);
            yield return null;             // подождать один кадр
        }
        sky.color = Color.white;
    }

    void UpdateStarText()
    {
        if (starText != null) starText.text = starsCollected + " / " + starsTotal;
    }

    // Уровень пройден: считаем очки, обновляем рекорд, открываем следующий уровень и сразу пишем файл.
    // Время — сколько прошло с загрузки сцены (пауза его не считает: Time.timeScale = 0).
    // Возвращает true, если очки — новый рекорд
    bool SaveResult()
    {
        LevelData level = GameSession.CurrentLevel;
        float time = Time.timeSinceLevelLoad;
        int score = scoreSettings.CalculateScore(starsCollected, time, level.parTimeSeconds);

        SaveData save = SaveSystem.Current;
        bool newBest = save.AddResult(level.levelNumber, starsCollected, score, time);
        if (level.nextLevel != null) save.Unlock(level.nextLevel.levelNumber);
        SaveSystem.Save();
        return newBest;
    }

    // Player зашёл в проход ворот
    public void ReachExit()
    {
        if (!gate.isOpen) return;
        SaveResult();

        // Следующий уровень из LevelData: запоминаем его в GameSession и грузим его сцену
        LevelData next = GameSession.CurrentLevel.nextLevel;
        if (next == null) return;          // последний уровень заканчивается победой над боссом, не воротами
        GameSession.CurrentLevel = next;
        SceneManager.LoadScene(next.sceneName);
    }
}
