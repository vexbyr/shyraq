using UnityEngine;

// Ворота (тег Exit). Закрыты решёткой, пока не собраны все звёзды.
// На объекте: SpriteRenderer (проход) + BoxCollider2D Is Trigger (зона выхода).
// Дочерний объект Bars: решётка с твёрдым коллайдером на слое Ground.
public class Gate : MonoBehaviour
{
    public GameObject bars;                              // решётка
    public Color openColor = new Color(1f, 0.85f, 0.3f); // проход светится жёлтым

    [HideInInspector] public bool isOpen;

    public void Open()
    {
        isOpen = true;
        bars.SetActive(false);                           // решётка исчезает — можно пройти
        GetComponent<SpriteRenderer>().color = openColor;
    }
}
