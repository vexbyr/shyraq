using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Окно настроек: ползунки громкости Music и SFX (в меню и в паузе — один и тот же префаб).
// Ползунок двигается — громкость меняется сразу (в памяти). В файл пишем, когда палец отпустил
// ползунок (EventTrigger → PointerUp → SaveSettings) и когда окно закрывают.
// Рядом с ползунком подпись показывает значение в процентах: "Music 20%".
public class SettingsPanel : MonoBehaviour
{
    [SerializeField] Slider musicSlider;
    [SerializeField] Slider sfxSlider;
    [SerializeField] TMP_Text musicLabel;
    [SerializeField] TMP_Text sfxLabel;
    // Что спрятать, пока открыты настройки. В главном меню — логотип и кнопки, в паузе — пусто.
    [SerializeField] GameObject hideWhileOpen;

    public void Open()
    {
        gameObject.SetActive(true);
        if (hideWhileOpen != null) hideWhileOpen.SetActive(false);
        // Показываем сохранённые значения. SetValueWithoutNotify — чтобы не вызвать OnMusicChanged лишний раз
        musicSlider.SetValueWithoutNotify(SaveSystem.Current.musicVolume);
        sfxSlider.SetValueWithoutNotify(SaveSystem.Current.sfxVolume);
        UpdateLabels();
    }

    // Громкость 0..1 → проценты 0..100 в подписях
    void UpdateLabels()
    {
        musicLabel.text = "Music " + Mathf.RoundToInt(musicSlider.value * 100) + "%";
        sfxLabel.text = "SFX " + Mathf.RoundToInt(sfxSlider.value * 100) + "%";
    }

    public void Close()
    {
        SaveSettings();
        gameObject.SetActive(false);
        if (hideWhileOpen != null) hideWhileOpen.SetActive(true);
    }

    // Slider → On Value Changed
    public void OnMusicChanged(float value)
    {
        SaveSystem.Current.musicVolume = value;
        AudioVolume.Apply(SaveSystem.Current);
        UpdateLabels();
    }

    public void OnSfxChanged(float value)
    {
        SaveSystem.Current.sfxVolume = value;
        AudioVolume.Apply(SaveSystem.Current);
        UpdateLabels();
    }

    // Ползунок отпустили (EventTrigger → PointerUp)
    public void SaveSettings()
    {
        SaveSystem.Save();
    }
}
