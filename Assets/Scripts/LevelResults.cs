using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using TMPro;

// Экран «уровень пройден» (в HUD каждого уровня). Показывает звёзды, время, очки и «New best!».
// Кнопки: Next level — следующий уровень из LevelData, Level select — главное меню с окном выбора уровня.
// Пока экран открыт, игра стоит (Time.timeScale = 0) и управление Ай выключено, как в паузе.
public class LevelResults : MonoBehaviour
{
    [SerializeField] TMP_Text statsText;       // звёзды, время, очки
    [SerializeField] GameObject newBestText;   // надпись «New best!»
    [SerializeField] GameObject nextButton;    // прячем, если следующего уровня нет
    [SerializeField] PlayerInput playerInput;

    public void Show(int stars, int starsTotal, float timeSeconds, int score, bool newBest)
    {
        gameObject.SetActive(true);
        Time.timeScale = 0f;
        playerInput.DeactivateInput();

        statsText.text = "Stars " + stars + " / " + starsTotal + "\nTime " + FormatTime(timeSeconds) + "\nScore " + score;
        newBestText.SetActive(newBest);
        nextButton.SetActive(GameSession.CurrentLevel.nextLevel != null);
    }

    // 83,4 секунды → «1:23»
    string FormatTime(float seconds)
    {
        int total = Mathf.FloorToInt(seconds);
        return (total / 60) + ":" + (total % 60).ToString("00");
    }

    public void NextLevel()
    {
        Time.timeScale = 1f;
        GameSession.CurrentLevel = GameSession.CurrentLevel.nextLevel;
        SceneManager.LoadScene(GameSession.CurrentLevel.sceneName);
    }

    public void ToLevelSelect()
    {
        Time.timeScale = 1f;
        GameSession.OpenLevelSelect = true;
        SceneManager.LoadScene("MainMenu");
    }
}
