using UnityEngine;

// Албасты — дух-стрелок. Стоит на месте, целится в Ай и стреляет сгустком тьмы.
// Автомат состояний (enum State), переходы:
//   Idle     → Aim      : видит Ай (близко, нет стены, сама в кадре камеры)
//   Aim      → Idle     : Ай пропала из виду — прицел сброшен
//   Aim      → Shoot    : прошло aimTime (всё это время Албасты светится — видно, что сейчас выстрелит)
//   Shoot    → Cooldown : снаряд создан (в Shoot она бывает ровно один кадр)
//   Cooldown → Idle     : прошло cooldownTime
//   любое    → Frozen   : в луче фонаря (прицел сброшен, не стреляет)
//   Frozen   → Idle     : луч ушёл
// На объекте: SpriteRenderer, Rigidbody2D (Kinematic), BoxCollider2D (Is Trigger),
// дочерний объект FirePoint — откуда вылетает снаряд.
public class Albasty : Enemy
{
    enum State { Idle, Aim, Shoot, Cooldown, Frozen }

    [Header("Зрение")]
    [SerializeField] float sightRange = 8f;            // дальше не видит
    [SerializeField] LayerMask groundLayer;            // слой Ground: стены закрывают обзор

    [Header("Выстрел")]
    [SerializeField] float aimTime = 0.65f;            // подготовка, секунды
    [SerializeField] float cooldownTime = 1.5f;        // перезарядка после выстрела
    [SerializeField] Transform firePoint;              // точка выстрела
    [SerializeField] AlbastyProjectile projectilePrefab;
    [SerializeField] float projectileSpeed = 6f;       // единиц в секунду
    [SerializeField] float projectileLifetime = 2.5f;  // секунд, потом исчезает сам
    [SerializeField] int projectileDamage = 1;

    [Header("Вид (спрайты можно оставить пустыми)")]
    [SerializeField] Sprite aimSprite;
    [SerializeField] Sprite frozenSprite;
    [SerializeField] Color aimColor = Color.white;     // белый: свечение уже нарисовано в спрайте aim
    [SerializeField] Color frozenColor = Color.white;  // белый: спрайт frozen уже голубой

    State state = State.Idle;
    float timer;                 // сколько ещё длится Aim / Cooldown
    bool facingRight = true;
    SpriteRenderer sr;
    Sprite normalSprite;         // спрайт из префаба
    Color normalColor;           // цвет из префаба
    Camera cam;
    Collider2D playerCollider;   // целимся в центр коллайдера Ай

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        normalSprite = sr.sprite;
        normalColor = sr.color;
        cam = Camera.main;

        GameObject playerObject = GameObject.FindWithTag("Player");
        if (playerObject != null) playerCollider = playerObject.GetComponent<Collider2D>();
    }

    void Update()
    {
        timer -= Time.deltaTime;

        // Свет главнее всего: из любого состояния — в Frozen
        if (IsLit() && state != State.Frozen) ChangeState(State.Frozen);

        switch (state)
        {
            case State.Frozen:
                if (!IsLit()) ChangeState(State.Idle);       // после света прицел начинается заново
                break;

            case State.Idle:
                if (CanSeePlayer()) ChangeState(State.Aim);
                break;

            case State.Aim:
                if (!CanSeePlayer()) ChangeState(State.Idle);         // ушла за стену / далеко / за экран
                else if (timer <= 0f) ChangeState(State.Shoot);
                break;

            case State.Shoot:
                Shoot();
                ChangeState(State.Cooldown);
                break;

            case State.Cooldown:
                if (timer <= 0f) ChangeState(State.Idle);
                break;
        }

        UpdateLook();
    }

    // Смена состояния: здесь же заводим таймер нового состояния
    void ChangeState(State newState)
    {
        state = newState;
        if (newState == State.Aim) timer = aimTime;
        if (newState == State.Cooldown) timer = cooldownTime;
    }

    // Видит ли Ай: Албасты в кадре, Ай близко и между ними нет стены
    bool CanSeePlayer()
    {
        if (playerCollider == null) return false;
        if (!IsOnScreen()) return false;                    // из-за края экрана не стреляет

        Vector2 eye = firePoint.position;
        Vector2 toPlayer = (Vector2)playerCollider.bounds.center - eye;
        if (toPlayer.magnitude > sightRange) return false;

        facingRight = toPlayer.x > 0f;                      // поворачивается к Ай

        // Raycast только по слою Ground: попал во что-то — стена мешает
        RaycastHit2D hit = Physics2D.Raycast(eye, toPlayer.normalized, toPlayer.magnitude, groundLayer);
        return hit.collider == null;
    }

    // В кадре ли камеры. Viewport: левый нижний угол экрана = (0,0), правый верхний = (1,1)
    bool IsOnScreen()
    {
        Vector3 p = cam.WorldToViewportPoint(transform.position);
        return p.x > 0f && p.x < 1f && p.y > 0f && p.y < 1f;
    }

    // Выстрел: направление выбирается один раз, в момент выстрела. Снаряд не самонаводится —
    // от него можно отойти или перепрыгнуть
    void Shoot()
    {
        Vector2 direction = ((Vector2)playerCollider.bounds.center - (Vector2)firePoint.position).normalized;
        AlbastyProjectile projectile = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
        projectile.Launch(direction, projectileSpeed, projectileLifetime, projectileDamage);
    }

    // Внешний вид по состоянию
    void UpdateLook()
    {
        sr.flipX = !facingRight;               // нарисована вправо, влево отражаем

        Sprite sprite = normalSprite;
        Color color = normalColor;
        if (state == State.Aim) { sprite = aimSprite; color = aimColor; }
        if (state == State.Frozen) { sprite = frozenSprite; color = frozenColor; }

        sr.sprite = (sprite != null) ? sprite : normalSprite;   // нет своего спрайта — обычный
        sr.color = color;
    }

    // В редакторе (выделена): жёлтый круг — дальность зрения
    void OnDrawGizmosSelected()
    {
        if (firePoint == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(firePoint.position, sightRange);
    }
}
