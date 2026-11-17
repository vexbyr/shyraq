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
    public SpriteRenderer sky;             // картинка неба; ночью она затемнена цветом, в конце светлеет
    public GameObject starsInSky;          // уровень с боссом: звёзды на небе (выключены до победы)
    public string winScene = "Win";

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

        // Сколько звёзд на уровне = сколько объектов с тегом Coin
        starsTotal = GameObject.FindGameObjectsWithTag("Coin").Length;
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
