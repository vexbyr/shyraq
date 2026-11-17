using UnityEngine;

// Висит на объекте Beam (жёлтый полупрозрачный спрайт + BoxCollider2D с галочкой Is Trigger).
// Пока луч касается чего-то, Unity каждый кадр вызывает OnTriggerStay2D.
public class LightBeam : MonoBehaviour
{
    void OnTriggerStay2D(Collider2D other)
    {
        // Есть ли на объекте компонент Lightable (или его наследник)?
        Lightable target = other.GetComponent<Lightable>();
        if (target != null)
        {
            target.Lit();   // сообщаем: «тебя осветили»
        }
    }
}
