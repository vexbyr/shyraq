using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

// Чтение и запись save.json в Application.persistentDataPath.
// Пока игра идёт, сохранение лежит в памяти (Current), в файл пишем в нужные моменты (Save).
// Правило: сломанный или пропавший файл НИКОГДА не роняет игру — тогда берём новую игру (new SaveData()).
public static class SaveSystem
{
    static SaveData current;   // сохранение в памяти; null — ещё не читали файл

    public static string SavePath { get { return Path.Combine(Application.persistentDataPath, "save.json"); } }
    static string TempPath { get { return Path.Combine(Application.persistentDataPath, "save.tmp"); } }
    static string BrokenPath { get { return Path.Combine(Application.persistentDataPath, "save_broken.json"); } }

    // Текущее сохранение. Первый раз — читаем файл
    public static SaveData Current
    {
        get
        {
            if (current == null) current = Load();
            return current;
        }
    }

    // В проекте выключена перезагрузка домена при входе в Play (Enter Play Mode Options),
    // поэтому static-поля живут между запусками в редакторе. Здесь забываем старое — читаем файл заново
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ForgetOnPlay()
    {
        current = null;
    }

    // Записать Current в файл. Сначала во временный save.tmp, потом заменить им save.json:
    // если запись оборвётся (кончилась батарея), старый save.json останется целым
    public static void Save()
    {
        try
        {
            string json = JsonUtility.ToJson(Current, true);   // true — с отступами, файл можно читать глазами
            File.WriteAllText(TempPath, json);
            if (File.Exists(SavePath)) File.Replace(TempPath, SavePath, null);
            else File.Move(TempPath, SavePath);
        }
        catch (Exception e)
        {
            Debug.LogWarning("Не удалось записать сохранение: " + e.Message);
        }
    }

    static SaveData Load()
    {
        if (!File.Exists(SavePath)) return new SaveData();   // первый запуск — новая игра

        try
        {
            string text = File.ReadAllText(SavePath);
            if (string.IsNullOrWhiteSpace(text)) return Broken("файл пустой");

            SaveData loaded = JsonUtility.FromJson<SaveData>(text);   // мусор в файле — здесь будет исключение
            if (loaded == null) return Broken("JSON пустой");

            return Validate(loaded);
        }
        catch (Exception e)
        {
            return Broken(e.Message);
        }
    }

    // Файл испорчен: откладываем его копию в save_broken.json (чтобы можно было посмотреть) и начинаем новую игру
    static SaveData Broken(string reason)
    {
        Debug.LogWarning("Сохранение испорчено (" + reason + "), начинаем новую игру. Копия: " + BrokenPath);
        try
        {
            File.Copy(SavePath, BrokenPath, true);
        }
        catch (Exception e)
        {
            Debug.LogWarning("Не удалось сделать копию испорченного сохранения: " + e.Message);
        }
        return new SaveData();
    }

    // Файл прочитан, но данные могут быть неправильными (правили руками, старая версия) — чиним
    static SaveData Validate(SaveData data)
    {
        if (data.version != SaveData.CurrentVersion)
        {
            Debug.LogWarning("Неизвестная версия сохранения " + data.version + ", начинаем новую игру");
            return new SaveData();
        }

        if (data.unlockedLevels == null) data.unlockedLevels = new List<int>();
        if (data.levelRecords == null) data.levelRecords = new List<LevelRecord>();

        // Убираем номера уровней вне 1..LevelCount (идём с конца, чтобы удаление не сбивало индексы)
        for (int i = data.unlockedLevels.Count - 1; i >= 0; i--)
        {
            if (!IsValidLevel(data.unlockedLevels[i])) data.unlockedLevels.RemoveAt(i);
        }
        for (int i = data.levelRecords.Count - 1; i >= 0; i--)
        {
            if (data.levelRecords[i] == null || !IsValidLevel(data.levelRecords[i].levelNumber)) data.levelRecords.RemoveAt(i);
        }

        data.Unlock(1);                                     // первый уровень открыт всегда
        data.UpdateTotalScore();                            // сумму пересчитываем, а не верим файлу
        data.musicVolume = Mathf.Clamp01(data.musicVolume);
        data.sfxVolume = Mathf.Clamp01(data.sfxVolume);
        return data;
    }

    static bool IsValidLevel(int levelNumber)
    {
        return levelNumber >= 1 && levelNumber <= SaveData.LevelCount;
    }
}
