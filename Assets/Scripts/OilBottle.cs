using UnityEngine;

// Бутылочка масла (тег Oil). Подбирает её PlayerPickups.
// Обычная исчезает навсегда, «возрождаемая» (арена босса) прячется и через oilRespawnTime появляется снова.
// Сколько масла даёт и через сколько возрождается — в ассете Item_Values.
public class OilBottle : MonoBehaviour
{
    [SerializeField] ItemValues itemValues;
    [SerializeField] bool isSpiritDrop = false;   // выпала из малого духа — даёт меньше масла
    public bool respawns = false;                 // возрождается ли (арена босса)

    void Awake()
    {
        if (itemValues == null)
        {
            Debug.LogError(name + ": не назначен ассет Item Values", this);
            enabled = false;
        }
    }

    // Сколько масла даёт эта бутылочка
    public float Amount()
    {
        return isSpiritDrop ? itemValues.oilDropAmount : itemValues.oilBottleAmount;
    }

    public void PickUp()
    {
        if (respawns)
        {
            // Не удаляем объект, а прячем: выключаем картинку и коллайдер
            GetComponent<SpriteRenderer>().enabled = false;
            GetComponent<Collider2D>().enabled = false;
            Invoke("Respawn", itemValues.oilRespawnTime);   // через oilRespawnTime секунд Unity вызовет Respawn()
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Respawn()
    {
        GetComponent<SpriteRenderer>().enabled = true;
        GetComponent<Collider2D>().enabled = true;
    }
}
