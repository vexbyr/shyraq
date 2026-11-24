using System;
using System.Collections.Generic;

// Всё, что игра хранит в save.json. [Serializable] — чтобы JsonUtility мог превратить объект в текст и обратно.
// Только простые поля и List: Dictionary JsonUtility не сохраняет.
// Значения по умолчанию (новая игра) задаются в ОДНОМ месте — в конструкторе.
[Serializable]
public class SaveData
{
    public const int CurrentVersion = 1;    // меняем, если поменяется формат файла
    public const int LevelCount = 3;        // уровни 1..3, другие номера в файле — ошибка
    public const float DefaultVolume = 0.8f;

    public int version;
    public List<int> unlockedLevels;
    public List<LevelRecord> levelRecords;
    public int totalBestScore;              // сумма лучших очков по всем уровням
    public float musicVolume;               // 0..1
    public float sfxVolume;                 // 0..1

    // Новая игра: открыт только уровень 1, рекордов нет, громкость 0,8
    public SaveData()
    {
        version = CurrentVersion;
        unlockedLevels = new List<int> { 1 };
        levelRecords = new List<LevelRecord>();
        totalBestScore = 0;
        musicVolume = DefaultVolume;
        sfxVolume = DefaultVolume;
    }

    public bool IsUnlocked(int levelNumber)
    {
        return unlockedLevels.Contains(levelNumber);
    }

    public void Unlock(int levelNumber)
    {
        if (!IsUnlocked(levelNumber)) unlockedLevels.Add(levelNumber);
    }

    // Рекорд уровня или null, если уровень ещё ни разу не пройден
    public LevelRecord GetRecord(int levelNumber)
    {
        foreach (LevelRecord record in levelRecords)
        {
            if (record.levelNumber == levelNumber) return record;
        }
        return null;
    }

    // Записать результат прохождения. Возвращает true, если очки — новый рекорд
    public bool AddResult(int levelNumber, int stars, int score, float timeSeconds)
    {
        LevelRecord record = GetRecord(levelNumber);
        if (record == null)
        {
            record = new LevelRecord();
            record.levelNumber = levelNumber;
            levelRecords.Add(record);
        }

        // Каждое значение рекорда улучшаем отдельно: лучшие звёзды, лучшие очки, лучшее время
        bool newBest = score > record.bestScore;
        if (stars > record.bestStars) record.bestStars = stars;
        if (newBest) record.bestScore = score;
        if (record.bestTimeSeconds <= 0f || timeSeconds < record.bestTimeSeconds) record.bestTimeSeconds = timeSeconds;

        UpdateTotalScore();
        return newBest;
    }

    // Сбросить прогресс, громкость оставить как есть
    public void ResetProgress()
    {
        SaveData fresh = new SaveData();
        unlockedLevels = fresh.unlockedLevels;
        levelRecords = fresh.levelRecords;
        totalBestScore = fresh.totalBestScore;
    }

    public void UpdateTotalScore()
    {
        totalBestScore = 0;
        foreach (LevelRecord record in levelRecords) totalBestScore += record.bestScore;
    }
}

// Лучший результат одного уровня
[Serializable]
public class LevelRecord
{
    public int levelNumber;
    public int bestStars;
    public int bestScore;
    public float bestTimeSeconds;   // 0 — ещё не пройден
}
