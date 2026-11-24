using UnityEngine;

// Тайная тропа: в темноте почти не видна и проходима насквозь,
// в свете фонаря становится твёрдой. Наследник Lightable — луч вызывает Lit().
// На объекте: SpriteRenderer (Draw Mode = Tiled) + BoxCollider2D Is Trigger (его ловит луч).
// Дочерний объект Solid: BoxCollider2D на слое Ground (по нему ходят).
public class SecretPath : Lightable
{
    [SerializeField] ItemValues itemValues;   // secretPathStayTime — сколько секунд тропа держится после луча
    public BoxCollider2D solid;
    public Sprite dimSprite;           // тропа в темноте (еле видна)
    public Sprite litSprite;           // тропа в свете

    [Header("Звук")]
    [SerializeField] SfxPlayer sfx;
    [SerializeField] AudioClip[] appearClips;   // тропа появилась

    SpriteRenderer sr;
    BoxCollider2D trigger;
    float timer;
    bool wasVisible;                   // была ли тропа видна в прошлом кадре

    void Awake()
    {
        if (itemValues == null)
        {
            Debug.LogError(name + ": не назначен ассет Item Values", this);
            enabled = false;
            return;
        }
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
        if (IsLit()) timer = itemValues.secretPathStayTime;     // светим — таймер снова полный
        else timer -= Time.deltaTime;      // не светим — таймер тикает вниз

        bool visible = timer > 0f;
        if (visible && !wasVisible) sfx.Play(appearClips);   // только в момент появления
        wasVisible = visible;
        solid.enabled = visible;           // твёрдая только пока таймер > 0
        sr.sprite = visible ? litSprite : dimSprite;
    }
}
