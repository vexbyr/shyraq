using UnityEngine;

// Щелчок кнопки. Button → On Click → UiClick.Click (в Inspector, рядом с основным действием кнопки).
// Многие кнопки сразу грузят другую сцену, и звук на объекте сцены оборвался бы.
// Поэтому играем через SfxPlayer на MusicPlayer (тег Music): он живёт между сценами.
// MusicPlayer нет (уровень запущен прямо в редакторе, не из меню) — играем через свой localSfx.
public class UiClick : MonoBehaviour
{
    [SerializeField] SfxPlayer localSfx;
    [SerializeField] AudioClip[] clips;

    public void Click()
    {
        GameObject music = GameObject.FindGameObjectWithTag("Music");
        if (music != null) music.GetComponent<SfxPlayer>().Play(clips);
        else localSfx.Play(clips);
    }
}
