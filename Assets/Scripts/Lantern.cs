using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

// Фонарь Ай: луч (удержание LIGHT), Жарқыл (FIRE), масло.
// Висит на Player рядом с PlayerController: PlayerInput присылает OnMove/OnLight/OnFire всем скриптам объекта.
public class Lantern : MonoBehaviour
{
    [Header("Ссылки")]
    public Transform lanternPoint;   // точка у руки, вокруг неё вращается луч
    public GameObject beam;          // объект луча (выключен, пока не светим)
    public GameObject flashPrefab;   // префаб Жарқыла
    public Image oilBar;             // полоска масла в HUD (Image Type = Filled)

    [Header("Масло")]
    public float maxOil = 100f;
    public float startOil = 100f;    // сколько масла в начале уровня (в «Сае» меньше)
    public float beamCost = 10f;     // масла в секунду, пока горит луч
    public float flashCost = 15f;    // масла за один Жарқыл

    [HideInInspector] public float oil;

    PlayerController player;
    Animator anim;
    Vector2 aim;                     // направление стика
    bool lightHeld;                  // держим ли кнопку LIGHT

    void Awake()
    {
        player = GetComponent<PlayerController>();
        anim = GetComponent<Animator>();
        oil = startOil;
        beam.SetActive(false);
    }

    void OnMove(InputValue value)
    {
        aim = value.Get<Vector2>();
    }

    // Press and Release: приходит и при нажатии, и при отпускании
    void OnLight(InputValue value)
    {
        lightHeld = value.isPressed;
    }

    void OnFire(InputValue value)
    {
        if (oil < flashCost) return;              // не хватает масла
        oil -= flashCost;
        Instantiate(flashPrefab, lanternPoint.position, lanternPoint.rotation);
    }

    void Update()
    {
        // 1. Точка фонаря всегда с той стороны, куда смотрит Ай
        float side = player.facingRight ? 1f : -1f;
        lanternPoint.localPosition = new Vector3(0.6f * side, 1f, 0f);

        // 2. Направление: стик, а если стик отпущен — вперёд
        Vector2 dir = aim;
        if (dir.magnitude < 0.3f) dir = new Vector2(side, 0f);
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        lanternPoint.rotation = Quaternion.Euler(0f, 0f, angle);

        // 3. Луч горит, пока держим кнопку и есть масло
        bool beamOn = lightHeld && oil > 0f;
        beam.SetActive(beamOn);
        anim.SetBool("Aiming", beamOn);  // поза «фонарь поднят», пока горит луч
        if (beamOn)
        {
            oil -= beamCost * Time.deltaTime;
            if (oil < 0f) oil = 0f;
        }

        // 4. Полоска масла: 1 = полная, 0 = пустая
        if (oilBar != null) oilBar.fillAmount = oil / maxOil;
    }

    // Подбор масла (бутылочки — на этапе 3)
    public void AddOil(float amount)
    {
        oil = Mathf.Min(oil + amount, maxOil);
    }
}
