using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

// Окно выбора уровня в главном меню. Какие уровни открыты — из сохранения (SaveSystem.Current).
// Открытый уровень: кнопка активна, видно лучшие звёзды и очки. Закрытый: кнопка тусклая, не нажимается, надпись «Locked».
// Здесь же сброс прогресса с окном подтверждения.
// Висит на Canvas меню (он всегда включён), а включает и выключает своё окно panel.
public class LevelSelect : MonoBehaviour
{
    [SerializeField] GameObject panel;               // окно выбора уровня
    [SerializeField] LevelData[] levels;            // Level_01, Level_02, Level_03 — по порядку кнопок
    [SerializeField] Button[] levelButtons;
    [SerializeField] TMP_Text[] levelLabels;
    [SerializeField] GameObject confirmResetPanel;  // «Reset all progress? Settings are kept.» Yes / No
    [SerializeField] Color lockedColor = new Color(1f, 1f, 1f, 0.4f);   // тусклый текст закрытого уровня (вид)

    // С экрана итогов нажали «Level select» — меню сразу показывает это окно
    void Start()
    {
        if (GameSession.OpenLevelSelect)
        {
            GameSession.OpenLevelSelect = false;
            Open();
        }
    }

    public void Open()
    {
        panel.SetActive(true);
        confirmResetPanel.SetActive(false);
        Refresh();
    }

    public void Close()
    {
        panel.SetActive(false);
    }

    // Перерисовать кнопки по сохранению
    void Refresh()
    {
        SaveData save = SaveSystem.Current;
        for (int i = 0; i < levels.Length; i++)
        {
            bool unlocked = save.IsUnlocked(levels[i].levelNumber);
            levelButtons[i].interactable = unlocked;        // выключенная кнопка тусклая (Disabled Color) и не нажимается
            levelLabels[i].color = unlocked ? Color.white : lockedColor;
            levelLabels[i].text = LevelText(levels[i], save, unlocked);
        }
    }

    // Текст на кнопке: номер и название, потом рекорд или «Locked»
    string LevelText(LevelData level, SaveData save, bool unlocked)
    {
        string title = level.levelNumber + ". " + level.displayName;
        if (!unlocked) return title + "\nLocked";

        LevelRecord record = save.GetRecord(level.levelNumber);
        if (record == null) return title + "\nNew";
        return title + "\nStars " + record.bestStars + " / " + level.starsRequired + "\nBest " + record.bestScore;
    }

    // Кнопка уровня → On Click (номер кнопки 0, 1, 2)
    public void StartLevel(int index)
    {
        GameSession.CurrentLevel = levels[index];
        SceneManager.LoadScene(levels[index].sceneName);
    }

    public void AskReset()
    {
        confirmResetPanel.SetActive(true);
    }

    public void CancelReset()
    {
        confirmResetPanel.SetActive(false);
    }

    // «Yes»: прогресс заново, громкость остаётся, сразу пишем файл и перерисовываем кнопки
    public void ConfirmReset()
    {
        SaveSystem.Current.ResetProgress();
        SaveSystem.Save();
        confirmResetPanel.SetActive(false);
        Refresh();
    }
}
