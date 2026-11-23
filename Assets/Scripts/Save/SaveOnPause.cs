using UnityEngine;

// Есть в каждой сцене (меню, уровни, победа). При старте сцены применяет сохранённую громкость,
// а когда игру сворачивают (телефон: кнопка «Домой», звонок) или закрывают — пишет сохранение в файл.
// На телефоне свёрнутую игру система может убить без OnApplicationQuit, поэтому важен именно OnApplicationPause(true).
public class SaveOnPause : MonoBehaviour
{
    void Start()
    {
        AudioVolume.Apply(SaveSystem.Current);
    }

    void OnApplicationPause(bool paused)
    {
        if (paused) SaveSystem.Save();
    }

    void OnApplicationQuit()
    {
        SaveSystem.Save();
    }
}
