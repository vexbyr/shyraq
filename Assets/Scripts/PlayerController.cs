using UnityEngine;
using UnityEngine.InputSystem;

// Ару: ходьба, двойной прыжок, проверка земли.
// Ввод приходит от PlayerInput (Behavior = Send Messages): он сам вызывает OnMove и OnJump.
public class PlayerController : MonoBehaviour
{
    [Header("Баланс (скорость, прыжок, число прыжков)")]
    [SerializeField] PlayerStats stats;

    [Header("Проверка земли")]
    public Transform groundCheck;         // пустой объект у ног
    public float groundCheckRadius = 0.15f;
    public LayerMask groundLayer;         // слой Ground

    [Header("Звук")]
    [SerializeField] SfxPlayer sfx;
    [SerializeField] AudioClip[] jumpClips;

    [HideInInspector] public bool facingRight = true;   // куда смотрит Ару (читает фонарь)

    Rigidbody2D rb;
    SpriteRenderer sr;
    Animator anim;
    float moveX;                          // -1 влево, 0 стоим, 1 вправо
    int jumpsLeft;
    bool isGrounded;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        if (stats == null)
        {
            Debug.LogError(name + ": не назначен ассет Player Stats", this);
            enabled = false;
        }
    }

    // Стик / A-D / стрелки
    void OnMove(InputValue value)
    {
        moveX = value.Get<Vector2>().x;
    }

    // Кнопка JUMP / Space
    void OnJump(InputValue value)
    {
        if (enabled && jumpsLeft > 0)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, stats.jumpForce);
            jumpsLeft--;
            sfx.Play(jumpClips);
        }
    }

    void Update()
    {
        // Есть ли под ногами слой Ground?
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        if (isGrounded && rb.linearVelocity.y <= 0.01f)
        {
            jumpsLeft = stats.maxJumps;   // стоим на земле: прыжки восстановились
        }
        else if (!isGrounded && jumpsLeft == stats.maxJumps)
        {
            jumpsLeft = stats.maxJumps - 1;   // упала с края без прыжка: остаётся только второй
        }

        // Поворот лицом в сторону движения: спрайт нарисован вправо, влево отражаем flipX
        if (moveX > 0) facingRight = true;
        if (moveX < 0) facingRight = false;

        // Сообщаем Animator, что происходит: он сам выберет нужную анимацию
        anim.SetFloat("Speed", Mathf.Abs(rb.linearVelocity.x));   // Abs: скорость без знака (влево тоже > 0)
        anim.SetBool("Grounded", isGrounded);
        sr.flipX = !facingRight;
    }

    void FixedUpdate()
    {
        // Физику двигаем в FixedUpdate: меняем только X, Y оставляем гравитации
        rb.linearVelocity = new Vector2(moveX * stats.moveSpeed, rb.linearVelocity.y);
    }

    // Рисует круг проверки земли в окне Scene (только для редактора)
    void OnDrawGizmos()
    {
        if (groundCheck == null) return;
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}
