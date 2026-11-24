using UnityEngine;

// Ай подбирает предметы и заходит в ворота. Всё различаем по тегам.
public class PlayerPickups : MonoBehaviour
{
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
        else if (other.CompareTag("Exit"))         // проход ворот
        {
            LevelManager.instance.ReachExit();
        }
    }

    // Масло — в Stay, а не в Enter: если Ай стоит на бутылке с полным фонарём,
    // бутылка останется и подберётся, как только масло начнёт тратиться
    void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Oil") && lantern.oil < lantern.MaxOil())
        {
            OilBottle bottle = other.GetComponent<OilBottle>();
            lantern.AddOil(bottle.Amount());
            bottle.PickUp();
        }
    }
}
