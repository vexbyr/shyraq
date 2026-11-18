using UnityEngine;

// Бутылочка масла (тег Oil). Подбирает её PlayerPickups.
// Обычная исчезает навсегда, «возрождаемая» (арена босса) прячется и через respawnTime появляется снова.
public class OilBottle : MonoBehaviour
{
    public float amount = 30f;           // сколько масла даёт
    public bool respawns = false;        // возрождается ли
    public float respawnTime = 20f;      // через сколько секунд

    public void PickUp()
    {
        if (respawns)
        {
            // Не удаляем объект, а прячем: выключаем картинку и коллайдер
            GetComponent<SpriteRenderer>().enabled = false;
            GetComponent<Collider2D>().enabled = false;
            Invoke("Respawn", respawnTime);   // через respawnTime секунд Unity вызовет Respawn()
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
