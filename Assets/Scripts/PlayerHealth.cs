using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

// 3 HP, мигание после урона, смерть = перезапуск уровня.
public class PlayerHealth : MonoBehaviour
{
    public int maxHP = 3;
    public float invincibleTime = 1.5f;   // секунды неуязвимости после удара
    public float fallY = -10f;            // упала ниже — смерть
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
        hp = maxHP;
        UpdateHearts();
    }

    void Update()
    {
        if (transform.position.y < fallY) Die();
    }

    public void TakeDamage(int amount)
    {
        if (invincible) return;           // ещё мигаем — удар не считается

        hp -= amount;
        UpdateHearts();

        if (hp <= 0)
        {
            Die();
            return;
        }

        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 8f);   // подпрыгивает от удара
        anim.SetTrigger("Hurt");                                     // кадр «больно» (Trigger срабатывает один раз)
        StartCoroutine(Blink());
    }

    // Корутина: мигаем спрайтом каждые 0,1 сек, пока идёт неуязвимость
    IEnumerator Blink()
    {
        invincible = true;
        for (float t = 0f; t < invincibleTime; t += 0.1f)
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
