using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

// Пауза на уровне. Методы вызываются кнопками (Button → On Click в Inspector).
public class PauseMenu : MonoBehaviour
{
    public GameObject pausePanel;        // затемнение + окно с кнопками
    public PlayerInput playerInput;      // на паузе выключаем управление Ай

    void Start()
    {
        pausePanel.SetActive(false);
        Time.timeScale = 1f;             // на всякий случай: уровень всегда начинается не на паузе
    }

    public void Pause()
    {
        pausePanel.SetActive(true);
        Time.timeScale = 0f;             // время стоит: физика, таймеры, корутины с WaitForSeconds
        playerInput.DeactivateInput();   // кнопки JUMP / FIRE не срабатывают, пока пауза
    }

    public void Resume()
    {
        pausePanel.SetActive(false);
        Time.timeScale = 1f;
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
        SceneManager.LoadScene("MainMenu");
    }
}
