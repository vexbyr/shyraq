using UnityEngine;

// Двигающаяся платформа (лодка). Двигает её Animator с клипом (между берегами, Loop).
// Этот скрипт только «сажает» Ай в лодку: пока Ай касается лодки, она — ребёнок лодки
// и едет вместе с ней. Сошла — возвращается к прежнему родителю.
public class MovingPlatform : MonoBehaviour
{
    Transform oldParent;                       // кто был родителем Ай до лодки
    RigidbodyInterpolation2D oldInterpolation; // как Ай сглаживала движение до лодки

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            oldParent = collision.transform.parent;      // запоминаем, чтобы потом вернуть
            collision.transform.SetParent(transform);    // Ai становится ребёнком лодки

            // Сглаживание (Interpolate) спорит с движущимся родителем: Ай «вязнет» в лодке.
            // Пока Ай в лодке — выключаем его.
            Rigidbody2D rb = collision.rigidbody;
            oldInterpolation = rb.interpolation;
            rb.interpolation = RigidbodyInterpolation2D.None;
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.transform.SetParent(oldParent);    // вернули на место
            collision.rigidbody.interpolation = oldInterpolation;
        }
    }
}
