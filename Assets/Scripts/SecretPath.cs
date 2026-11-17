using UnityEngine;

// Тайная тропа: в темноте почти не видна и проходима насквозь,
// в свете фонаря становится твёрдой. Наследник Lightable — луч вызывает Lit().
// На объекте: SpriteRenderer (Draw Mode = Tiled) + BoxCollider2D Is Trigger (его ловит луч).
// Дочерний объект Solid: BoxCollider2D на слое Ground (по нему ходят).
public class SecretPath : Lightable
{
    public BoxCollider2D solid;
    public float stayTime = 1.5f;      // сколько секунд тропа держится после того, как луч ушёл
    public Color hiddenColor = new Color(0.2f, 0.3f, 0.8f, 0.12f);
    public Color shownColor = new Color(0.25f, 0.4f, 1f, 1f);

    SpriteRenderer sr;
    BoxCollider2D trigger;
    float timer;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        trigger = GetComponent<BoxCollider2D>();

        // Оба коллайдера подгоняем под размер спрайта (pivot снизу, поэтому offset = половина высоты)
        trigger.size = sr.size;
        trigger.offset = new Vector2(0f, sr.size.y / 2f);
        solid.size = sr.size;
        solid.offset = trigger.offset;
    }

    void Update()
    {
        if (IsLit()) timer = stayTime;     // светим — таймер снова полный
        else timer -= Time.deltaTime;      // не светим — таймер тикает вниз

        bool visible = timer > 0f;
        solid.enabled = visible;           // твёрдая только пока таймер > 0
        sr.color = visible ? shownColor : hiddenColor;
    }
}
