using UnityEngine;

// Ворота (тег Exit). Закрыты, пока не собраны все звёзды.
// На объекте: SpriteRenderer (картинка ворот) + BoxCollider2D Is Trigger (зона выхода).
// Дочерний объект Bars: невидимая преграда — твёрдый коллайдер на слое Ground.
public class Gate : MonoBehaviour
{
    public GameObject bars;          // преграда, пока ворота закрыты
    public Sprite openSprite;        // картинка открытых ворот

    [HideInInspector] public bool isOpen;

    public void Open()
    {
        isOpen = true;
        bars.SetActive(false);                           // преграда исчезает — можно пройти
        GetComponent<SpriteRenderer>().sprite = openSprite;
    }
}
