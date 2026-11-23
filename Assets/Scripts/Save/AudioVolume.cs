using UnityEngine;

// Применяет громкость из сохранения.
// Пока в игре один общий регулятор — AudioListener.volume, поэтому берём среднее двух ползунков:
// оба в нуле — тишина, любой из них влияет на звук. В A12 здесь будет AudioMixer (Music/SFX отдельно).
public static class AudioVolume
{
    public static void Apply(SaveData data)
    {
        AudioListener.volume = (data.musicVolume + data.sfxVolume) / 2f;
    }
}
