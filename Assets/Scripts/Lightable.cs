using UnityEngine;

// Базовый класс для всего, что реагирует на луч фонаря.
// Тайные тропы и духи будут наследниками: class SecretPath : Lightable
public class Lightable : MonoBehaviour
{
    float lastLitTime = -10f;   // когда луч касался нас в последний раз

    // Луч вызывает этот метод каждый кадр, пока касается объекта
    public void Lit()
    {
        lastLitTime = Time.time;
    }

    // Освещён ли объект прямо сейчас (луч касался меньше 0,1 сек назад)
    public bool IsLit()
    {
        return Time.time - lastLitTime < 0.1f;
    }
}
