using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

// Пауза на уровне. Методы вызываются кнопками (Button → On Click в Inspector).
// Звук: на паузе снапшот Paused (музыка тише и глуше), при выходе из паузы — снова Normal.
// Переход 0,2 сек идёт и при timeScale = 0: у микшера Update Mode = Unscaled Time.
public class PauseMenu : MonoBehaviour
{
    public GameObject pausePanel;        // затемнение + окно с кнопками
    public PlayerInput playerInput;      // на паузе выключаем управление Ай
    [SerializeField] AudioMixerSnapshot normalSnapshot;
    [SerializeField] AudioMixerSnapshot pausedSnapshot;
    [SerializeField] float snapshotTime = 0.2f;   // за сколько секунд меняется звук

    void Start()
    {
        pausePanel.SetActive(false);
        Time.timeScale = 1f;             // на всякий случай: уровень всегда начинается не на паузе
        normalSnapshot.TransitionTo(0f); // и звук тоже обычный (например, после Restart из паузы)
    }

    public void Pause()
    {
        pausePanel.SetActive(true);
        Time.timeScale = 0f;             // время стоит: физика, таймеры, корутины с WaitForSeconds
        pausedSnapshot.TransitionTo(snapshotTime);
        playerInput.DeactivateInput();   // кнопки JUMP / FIRE не срабатывают, пока пауза
    }

    public void Resume()
    {
        pausePanel.SetActive(false);
        Time.timeScale = 1f;
        normalSnapshot.TransitionTo(snapshotTime);
        playerInput.ActivateInput();
    }

    public void Restart()
    {
        Time.timeScale = 1f;             // иначе новый уровень начнётся «замороженным»
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ToMenu()
    {
        Time.timeScale = 1f;
        normalSnapshot.TransitionTo(snapshotTime);   // в меню музыка снова обычная
        SceneManager.LoadScene("MainMenu");
    }
}
