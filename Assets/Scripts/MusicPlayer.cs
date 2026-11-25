using UnityEngine;
using UnityEngine.SceneManagement;

// Музыка всей игры. Объект стоит в MainMenu (первая сцена), тег Music, живёт между сценами (DontDestroyOnLoad).
// Два AudioSource (Output = MusicPlay, Loop): пока один затихает, другой становится громче — плавный переход.
// Каждый кадр смотрим, какая сцена открыта: меню и победа — музыка меню, уровни — музыка уровня.
// Если нужная музыка уже играет (следующий уровень, перезапуск) — ничего не делаем, трек не начинается заново.
// На этом же объекте ещё SfxPlayer (свой AudioSource → SFX): через него играют щелчки кнопок (UiClick).
public class MusicPlayer : MonoBehaviour
{
    [SerializeField] AudioSource sourceA;
    [SerializeField] AudioSource sourceB;
    [SerializeField] AudioClip menuMusic;
    [SerializeField] AudioClip levelMusic;
    [SerializeField] float fadeTime = 1f;   // сколько секунд длится переход

    AudioSource current;    // играет сейчас (или становится громче)
    AudioSource previous;   // затихает
    float fadeTimer;        // сколько ещё секунд длится переход; 0 — перехода нет

    void Awake()
    {
        // Вернулись в MainMenu — там снова есть MusicPlayer. Если с прошлых сцен уже живёт другой, этот лишний
        GameObject[] players = GameObject.FindGameObjectsWithTag("Music");
        if (players.Length > 1)
        {
            gameObject.SetActive(false);   // сразу прячем, чтобы UiClick в этом кадре нашёл старый
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
        current = sourceA;
        previous = sourceB;
    }

    void Update()
    {
        AudioClip wanted = WantedMusic();
        if (wanted != current.clip) StartFade(wanted);   // нужна другая музыка

        if (fadeTimer > 0f)
        {
            fadeTimer -= Time.deltaTime;                  // музыка меняется только при смене сцены, а перед LoadScene timeScale всегда 1
            float t = 1f - fadeTimer / fadeTime;          // 0 → 1 за fadeTime секунд
            if (t > 1f) t = 1f;
            current.volume = t;
            previous.volume = 1f - t;
            if (fadeTimer <= 0f) previous.Stop();         // старый трек совсем затих
        }
    }

    // Какая музыка нужна в открытой сцене
    AudioClip WantedMusic()
    {
        string scene = SceneManager.GetActiveScene().name;
        if (scene == "MainMenu" || scene == "Win") return menuMusic;
        return levelMusic;
    }

    // Источники меняются ролями: тот, что играл, будет затихать, а второй начинает новый трек с нуля громкости
    void StartFade(AudioClip clip)
    {
        AudioSource old = current;
        current = previous;
        previous = old;

        current.clip = clip;
        current.volume = 0f;
        current.Play();
        fadeTimer = fadeTime;
    }
}
