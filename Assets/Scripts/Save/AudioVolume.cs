using UnityEngine;
using UnityEngine.Audio;

// Применяет громкость из сохранения к микшеру ShyraqMixer: Music и SFX отдельно.
// В сохранении громкость линейная 0,0001..1 (как ползунок), а микшер считает в децибелах:
// дБ = Log10(громкость) * 20. 1 → 0 дБ (как есть), 0,5 → −6 дБ, 0,0001 → −80 дБ (тишина).
// Вызываем из Start, не из Awake: в Awake микшер ещё не готов и SetFloat не срабатывает.
public static class AudioVolume
{
    public const float MinVolume = 0.0001f;   // ноль нельзя: Log10(0) — минус бесконечность

    public static void Apply(SaveData data, AudioMixer mixer)
    {
        mixer.SetFloat("MusicVol", ToDecibels(data.musicVolume));
        mixer.SetFloat("SfxVol", ToDecibels(data.sfxVolume));
    }

    static float ToDecibels(float volume)
    {
        if (volume < MinVolume) volume = MinVolume;
        return Mathf.Log10(volume) * 20f;
    }
}
