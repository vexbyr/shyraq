using UnityEngine;
using UnityEngine.InputSystem;

// Ай: ходьба, двойной прыжок, проверка земли.
// Ввод приходит от PlayerInput (Behavior = Send Messages): он сам вызывает OnMove и OnJump.
public class PlayerController : MonoBehaviour
{
    [Header("Движение")]
    public float moveSpeed = 6f;
    public float jumpForce = 12f;
    public int maxJumps = 2;              // 2 = двойной прыжок

    [Header("Проверка земли")]
    public Transform groundCheck;         // пустой объект у ног
    public float groundCheckRadius = 0.15f;
    public LayerMask groundLayer;         // слой Ground

    [HideInInspector] public bool facingRight = true;   // куда смотрит Ай (читает фонарь)

    Rigidbody2D rb;
    SpriteRenderer sr;
    float moveX;                          // -1 влево, 0 стоим, 1 вправо
    int jumpsLeft;
    bool isGrounded;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
    }

    // Стик / A-D / стрелки
    void OnMove(InputValue value)
    {
        moveX = value.Get<Vector2>().x;
    }

    // Кнопка JUMP / Space
    void OnJump(InputValue value)
    {
        if (jumpsLeft > 0)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            jumpsLeft--;
        }
    }

    void Update()
    {
        // Есть ли под ногами слой Ground?
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        if (isGrounded && rb.linearVelocity.y <= 0.01f)
        {
            jumpsLeft = maxJumps;         // стоим на земле: прыжки восстановились
        }
        else if (!isGrounded && jumpsLeft == maxJumps)
        {
            jumpsLeft = maxJumps - 1;     // упала с края без прыжка: остаётся только второй
        }

        // Поворот лицом в сторону движения: спрайт нарисован вправо, влево отражаем flipX
        if (moveX > 0) facingRight = true;
        if (moveX < 0) facingRight = false;
        sr.flipX = !facingRight;
    }

    void FixedUpdate()
    {
        // Физику двигаем в FixedUpdate: меняем только X, Y оставляем гравитации
        rb.linearVelocity = new Vector2(moveX * moveSpeed, rb.linearVelocity.y);
    }

    // Рисует круг проверки земли в окне Scene (только для редактора)
    void OnDrawGizmos()
    {
        if (groundCheck == null) return;
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}
