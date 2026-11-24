using UnityEngine;

// Как считаются очки в конце уровня: за звёзды и за скорость.
// Очки = звёзды × pointsPerStar + бонус за время.
// Бонус за время: уложилась в parTimeSeconds уровня — весь maxTimeBonus,
// дольше — за каждую лишнюю секунду минус timeBonusLostPerSecond (но не меньше нуля).
[CreateAssetMenu(fileName = "Score_Settings", menuName = "Shyraq/Score Settings")]
public class ScoreSettings : ScriptableObject
{
    [Tooltip("Очков за одну собранную звезду")]
    [Min(0)] public int pointsPerStar = 100;
    [Tooltip("Бонус, если уровень пройден не дольше целевого времени")]
    [Min(0)] public int maxTimeBonus = 500;
    [Tooltip("Сколько бонуса теряется за каждую секунду сверх целевого времени")]
    [Min(0f)] public float timeBonusLostPerSecond = 5f;

    // Только считает, в ассет ничего не пишет
    public int CalculateScore(int stars, float timeSeconds, float parTimeSeconds)
    {
        float lateSeconds = Mathf.Max(0f, timeSeconds - parTimeSeconds);
        int timeBonus = Mathf.Max(0, Mathf.RoundToInt(maxTimeBonus - lateSeconds * timeBonusLostPerSecond));
        return stars * pointsPerStar + timeBonus;
    }
}
