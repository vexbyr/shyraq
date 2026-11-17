using UnityEngine;

// Тайная тропа: в темноте почти не видна и проходима насквозь,
// в свете фонаря становится твёрдой. Наследник Lightable — луч вызывает Lit().
// На объекте: SpriteRenderer (Draw Mode = Tiled) + BoxCollider2D Is Trigger (его ловит луч).
// Дочерний объект Solid: BoxCollider2D на слое Ground (по нему ходят).
public class SecretPath : Lightable
{
    public BoxCollider2D solid;
    public float stayTime = 1.5f;      // сколько секунд тропа держится после того, как луч ушёл
    public Sprite dimSprite;           // тропа в темноте (еле видна)
    public Sprite litSprite;           // тропа в свете

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
        sr.sprite = visible ? litSprite : dimSprite;
    }
}
