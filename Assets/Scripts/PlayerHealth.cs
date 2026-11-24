using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

// HP, мигание после урона, смерть = перезапуск уровня.
// HP, неуязвимость и отскок от удара — в ассете Player_Stats.
public class PlayerHealth : MonoBehaviour
{
    [SerializeField] PlayerStats stats;
    public float fallY = -10f;            // упала ниже — смерть (граница мира, не баланс)
    public Image[] hearts;                // сердца в HUD
    public Sprite heartFull;              // целое сердце
    public Sprite heartEmpty;             // пустое сердце

    int hp;
    bool invincible;
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
        if (!enabled || invincible) return;   // ещё мигаем — удар не считается

        hp -= amount;
        UpdateHearts();

        if (hp <= 0)
        {
            Die();
            return;
        }

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
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);   // начать уровень заново
    }
}
