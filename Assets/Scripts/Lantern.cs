using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

// Фонарь Ару: луч (удержание LIGHT), Жарқыл (FIRE), масло.
// Максимум и расход масла, цена и пауза Жарқыла — в ассете Player_Stats.
// Висит на Player рядом с PlayerController: PlayerInput присылает OnMove/OnLight/OnFire всем скриптам объекта.
public class Lantern : MonoBehaviour
{
    [Header("Ссылки")]
    public Transform lanternPoint;   // точка у руки, вокруг неё вращается луч
    public GameObject beam;          // объект луча (выключен, пока не светим)
    public GameObject flashPrefab;   // префаб Жарқыла
    public Image oilBar;             // полоска масла в HUD (Image Type = Filled)

    [Header("Баланс (масло, Жарқыл)")]
    [SerializeField] PlayerStats stats;

    [Header("Звук")]
    [SerializeField] SfxPlayer sfx;
    [SerializeField] AudioClip[] beamOnClips;   // луч включился
    [SerializeField] AudioClip[] flashClips;    // выстрел Жарқылом

    [HideInInspector] public float oil;   // наливает LevelLoader из LevelData.startOil
    float nextFlashTime;             // раньше этого времени Жарқыл не выстрелит (пауза flashCooldown)

    PlayerController player;
    Animator anim;
    Vector2 aim;                     // направление стика
    bool lightHeld;                  // держим ли кнопку LIGHT
    bool beamWasOn;                  // горел ли луч в прошлом кадре (звук — только в момент включения)

    void Awake()
    {
        player = GetComponent<PlayerController>();
        anim = GetComponent<Animator>();
        if (stats == null)
        {
            Debug.LogError(name + ": не назначен ассет Player Stats", this);
            enabled = false;
            return;
        }
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
        if (!enabled) return;
        if (oil < stats.flashOilCost) return;           // не хватает масла
        if (Time.time < nextFlashTime) return;           // пауза после прошлого выстрела ещё идёт
        oil -= stats.flashOilCost;
        nextFlashTime = Time.time + stats.flashCooldown;
        Instantiate(flashPrefab, lanternPoint.position, lanternPoint.rotation);
        sfx.Play(flashClips);
    }

    void Update()
    {
        // 1. Точка фонаря всегда с той стороны, куда смотрит Ару
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
        if (beamOn && !beamWasOn) sfx.Play(beamOnClips);   // был выключен, стал включён
        beamWasOn = beamOn;
        anim.SetBool("Aiming", beamOn);  // поза «фонарь поднят», пока горит луч
        if (beamOn)
        {
            oil -= stats.beamOilPerSecond * Time.deltaTime;
            if (oil < 0f) oil = 0f;
        }

        // 4. Полоска масла: 1 = полная, 0 = пустая
        if (oilBar != null) oilBar.fillAmount = oil / stats.maxOil;
    }

    public float MaxOil()
    {
        return stats.maxOil;
    }

    // Подбор масла
    public void AddOil(float amount)
    {
        oil = Mathf.Min(oil + amount, stats.maxOil);
    }
}
