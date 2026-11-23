using UnityEngine;

// Простая анимация кадрами без Animator: по таймеру меняет спрайт по кругу (0, 1, 0, 1…), как кадры покоя у босса.
// Сейчас висит на воде реки в Level2 (River_Water, River_Water_1).
// На объекте: SpriteRenderer. Draw Mode (Tiled) и размер SpriteRenderer не трогает — меняет только картинку.
public class FrameAnimation : MonoBehaviour
{
    [SerializeField] Sprite[] frames;            // кадры по порядку
    [SerializeField] float frameTime = 0.5f;     // сколько секунд показываем один кадр

    SpriteRenderer sr;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (frames.Length == 0) return;
        int frame = (int)(Time.time / frameTime) % frames.Length;   // номер кадра растёт каждые frameTime секунд, % — по кругу
        sr.sprite = frames[frame];
    }
}
