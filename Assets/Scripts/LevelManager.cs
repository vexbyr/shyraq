using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

// Один на сцену: считает звёзды, открывает ворота, грузит следующий уровень.
public class LevelManager : MonoBehaviour
{
    public static LevelManager instance;   // «сам себя» — чтобы другие скрипты нашли его как LevelManager.instance

    public Gate gate;                      // ворота этого уровня
    public TMP_Text starText;              // счётчик звёзд в HUD
    public string nextScene = "Level2";    // куда ведут ворота
    public Color skyWhenDone = new Color(0.13f, 0.17f, 0.35f);   // небо светлеет, когда звёзды вернулись

    int starsTotal;
    int starsCollected;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        // Ворота на каждом уровне свои: находим их по тегу Exit (в сцене одни ворота)
        if (gate == null) gate = GameObject.FindGameObjectWithTag("Exit").GetComponent<Gate>();

        // Сколько звёзд на уровне = сколько объектов с тегом Coin
        starsTotal = GameObject.FindGameObjectsWithTag("Coin").Length;
        UpdateStarText();
    }

    public void AddStar()
    {
        starsCollected++;
        UpdateStarText();

        if (starsCollected >= starsTotal)
        {
            gate.Open();                   // все звёзды собраны — ворота открываются
            StartCoroutine(BrightenSky());
        }
    }

    // Корутина: за 2 секунды плавно меняем цвет фона камеры
    IEnumerator BrightenSky()
    {
        Camera cam = Camera.main;
        Color startColor = cam.backgroundColor;
        for (float t = 0f; t < 1f; t += Time.deltaTime / 2f)
        {
            cam.backgroundColor = Color.Lerp(startColor, skyWhenDone, t);
            yield return null;             // подождать один кадр
        }
        cam.backgroundColor = skyWhenDone;
    }

    void UpdateStarText()
    {
        if (starText != null) starText.text = starsCollected + " / " + starsTotal;
    }

    // Player зашёл в проход ворот
    public void ReachExit()
    {
        if (!gate.isOpen) return;

        // Сцена есть в Build Settings? Тогда грузим, иначе просто пишем в Console
        if (Application.CanStreamedLevelBeLoaded(nextScene))
        {
            SceneManager.LoadScene(nextScene);
        }
        else
        {
            Debug.Log("Уровень пройден! Сцены " + nextScene + " пока нет в Build Settings.");
        }
    }
}
