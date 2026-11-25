using UnityEngine;

// Ару подбирает предметы и заходит в ворота. Всё различаем по тегам.
public class PlayerPickups : MonoBehaviour
{
    [SerializeField] SfxPlayer sfx;
    [SerializeField] AudioClip[] starClips;
    [SerializeField] AudioClip[] oilClips;

    Lantern lantern;
    LevelManager levelManager;                    // находим один раз в Start по тегу GameController

    void Awake()
    {
        lantern = GetComponent<Lantern>();
    }

    void Start()
    {
        levelManager = GameObject.FindWithTag("GameController").GetComponent<LevelManager>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Coin"))              // звезда
        {
            levelManager.AddStar();
            sfx.Play(starClips);
            Destroy(other.gameObject);
        }
        else if (other.CompareTag("Exit"))         // проход ворот
        {
            levelManager.ReachExit();
        }
    }

    // Масло — в Stay, а не в Enter: если Ару стоит на бутылке с полным фонарём,
    // бутылка останется и подберётся, как только масло начнёт тратиться
    void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Oil") && lantern.oil < lantern.MaxOil())
        {
            OilBottle bottle = other.GetComponent<OilBottle>();
            lantern.AddOil(bottle.Amount());
            bottle.PickUp();
            sfx.Play(oilClips);
        }
    }
}
