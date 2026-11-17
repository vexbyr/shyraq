using UnityEngine;

// Ай подбирает предметы и заходит в ворота. Всё различаем по тегам.
public class PlayerPickups : MonoBehaviour
{
    public float oilPerBottle = 30f;

    Lantern lantern;

    void Awake()
    {
        lantern = GetComponent<Lantern>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Coin"))              // звезда
        {
            LevelManager.instance.AddStar();
            Destroy(other.gameObject);
        }
        else if (other.CompareTag("Oil"))          // бутылочка масла
        {
            lantern.AddOil(oilPerBottle);
            Destroy(other.gameObject);
        }
        else if (other.CompareTag("Exit"))         // проход ворот
        {
            LevelManager.instance.ReachExit();
        }
    }
}
