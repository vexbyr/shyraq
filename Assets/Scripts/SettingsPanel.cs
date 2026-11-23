using UnityEngine;
using UnityEngine.UI;

// Окно настроек: ползунки громкости Music и SFX (в меню и в паузе — один и тот же префаб).
// Ползунок двигается — громкость меняется сразу (в памяти). В файл пишем, когда палец отпустил
// ползунок (EventTrigger → PointerUp → SaveSettings) и когда окно закрывают.
public class SettingsPanel : MonoBehaviour
{
    [SerializeField] Slider musicSlider;
    [SerializeField] Slider sfxSlider;

    public void Open()
    {
        gameObject.SetActive(true);
        // Показываем сохранённые значения. SetValueWithoutNotify — чтобы не вызвать OnMusicChanged лишний раз
        musicSlider.SetValueWithoutNotify(SaveSystem.Current.musicVolume);
        sfxSlider.SetValueWithoutNotify(SaveSystem.Current.sfxVolume);
    }

    public void Close()
    {
        SaveSettings();
        gameObject.SetActive(false);
    }

    // Slider → On Value Changed
    public void OnMusicChanged(float value)
    {
        SaveSystem.Current.musicVolume = value;
        AudioVolume.Apply(SaveSystem.Current);
    }

    public void OnSfxChanged(float value)
    {
        SaveSystem.Current.sfxVolume = value;
        AudioVolume.Apply(SaveSystem.Current);
    }

    // Ползунок отпустили (EventTrigger → PointerUp)
    public void SaveSettings()
    {
        SaveSystem.Save();
    }
}
