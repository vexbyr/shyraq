using UnityEngine;

// Проигрывает короткие звуки (SFX). Из массива берёт случайный клип и чуть меняет высоту звука,
// чтобы частые звуки (прыжок, звезда) не звучали одинаково.
// AudioSource ставим в поле в Inspector, у него Output = группа SFX в ShyraqMixer.
public class SfxPlayer : MonoBehaviour
{
    [SerializeField] AudioSource source;

    public void Play(AudioClip[] clips)
    {
        if (clips == null || clips.Length == 0) return;           // звук не назначен — просто молчим

        AudioClip clip = clips[Random.Range(0, clips.Length)];    // Range с int: верхняя граница не входит
        source.pitch = Random.Range(0.92f, 1.08f);                // чуть ниже или выше обычного
        source.PlayOneShot(clip);                                 // OneShot: звуки не обрывают друг друга
    }
}
