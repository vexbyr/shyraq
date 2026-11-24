using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

// HP, мигание после урона, смерть = перезапуск уровня.
// Смерть: управление выключено, звук смерти, анимация Die, через 0,4 сек — звук проигрыша.
// Уровень начинается заново, когда анимация смерти доиграет: на её последнем кадре стоит
// Animation Event → RestartLevel (метод на этом же объекте, где Animator).
// HP, неуязвимость и отскок от удара — в ассете Player_Stats.
public class PlayerHealth : MonoBehaviour
{
    [SerializeField] PlayerStats stats;
    public float fallY = -10f;            // упала ниже — смерть (граница мира, не баланс)
    public Image[] hearts;                // сердца в HUD
    public Sprite heartFull;              // целое сердце
    public Sprite heartEmpty;             // пустое сердце
    [SerializeField] PlayerInput playerInput;   // после смерти выключаем управление
    [SerializeField] Lantern lantern;           // после смерти фонарь гаснет

    [Header("Звук")]
    [SerializeField] SfxPlayer sfx;
    [SerializeField] AudioClip[] hitClips;
    [SerializeField] AudioClip[] deathClips;   // мемный bruh
    [SerializeField] AudioClip[] loseClips;    // грустная фраза домбры
    [SerializeField] float loseSoundDelay = 0.4f;   // через сколько после смерти звучит проигрыш

    int hp;
    bool invincible;
    bool dead;                            // уже умерла — ждём перезапуска
    SpriteRenderer sr;
    Rigidbody2D rb;
    Animator anim;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        if (stats == null)
        {
            Debug.LogError(name + ": не назначен ассет Player Stats", this);
            enabled = false;
            return;
        }
        hp = stats.maxHP;
        UpdateHearts();
    }

    void Update()
    {
        if (transform.position.y < fallY) Die();
    }

    public void TakeDamage(int amount)
    {
        if (!enabled || invincible || dead) return;   // ещё мигаем или уже умерла — удар не считается

        hp -= amount;
        UpdateHearts();

        if (hp <= 0)
        {
            Die();
            return;
        }

        sfx.Play(hitClips);
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, stats.hurtBounce);   // подпрыгивает от удара
        anim.SetTrigger("Hurt");                                     // кадр «больно» (Trigger срабатывает один раз)
        StartCoroutine(Blink());
    }

    // Корутина: мигаем спрайтом каждые 0,1 сек (шаг мигания — вид, не баланс), пока идёт неуязвимость
    IEnumerator Blink()
    {
        invincible = true;
        for (float t = 0f; t < stats.invincibleTime; t += 0.1f)
        {
            sr.enabled = !sr.enabled;
            yield return new WaitForSeconds(0.1f);
        }
        sr.enabled = true;
        invincible = false;
    }

    // Сердце i целое, если i меньше текущего HP, иначе пустое
    void UpdateHearts()
    {
        for (int i = 0; i < hearts.Length; i++)
        {
            hearts[i].sprite = i < hp ? heartFull : heartEmpty;
        }
    }

    void Die()
    {
        if (dead) return;                 // Update вызывает Die каждый кадр, пока Ай ниже fallY
        dead = true;

        playerInput.DeactivateInput();    // кнопки больше не работают
        StopAllCoroutines();              // мигание после удара больше не нужно
        sr.enabled = true;                // а то Ай может остаться невидимой
        lantern.enabled = false;          // фонарь гаснет: луч выключаем сами, Update фонаря больше не идёт
        lantern.beam.SetActive(false);
        anim.SetBool("Aiming", false);    // иначе переход Any State → Aim выбьет Ай из анимации смерти
        sfx.Play(deathClips);
        anim.SetTrigger("Die");           // Any State → Die, обратно не выходит
        Invoke("PlayLoseSound", loseSoundDelay);
    }

    void PlayLoseSound()
    {
        sfx.Play(loseClips);
    }

    // Вызывает Animation Event на последнем кадре клипа Ai_Death
    public void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);   // начать уровень заново
    }
}
