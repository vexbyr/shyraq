using UnityEngine;

// Камера плавно едет за игроком и не выходит за края уровня.
public class CameraFollow : MonoBehaviour
{
    public Transform target;                          // Ай
    public Vector3 offset = new Vector3(0f, 2f, -10f);   // позиция Ай = ноги, поэтому камеру поднимаем на 2; z = -10, чтобы камера видела сцену
    public float smooth = 5f;                         // больше = быстрее догоняет

    [Header("Края уровня по X (левый и правый край земли)")]
    public float minX = -10f;
    public float maxX = 100f;
    [Header("Центр камеры не ниже")]
    public float minY = -100f;                        // ниже не опускаемся, чтобы не показывать пустоту под землёй

    Camera cam;

    void Awake()
    {
        cam = GetComponent<Camera>();
    }

    // Сразу ставим камеру на место, чтобы в первом кадре не было видно пустоты за краем
    void Start()
    {
        if (target != null) transform.position = Goal();
    }

    // LateUpdate идёт после движения игрока, поэтому камера не дрожит
    void LateUpdate()
    {
        if (target == null) return;
        transform.position = Vector3.Lerp(transform.position, Goal(), smooth * Time.deltaTime);
    }

    // Куда камере надо встать
    Vector3 Goal()
    {
        Vector3 goal = target.position + offset;

        // Половина ширины кадра в единицах: высота кадра / 2 = orthographicSize, ширина = высота * aspect.
        // На широком экране (19:9) кадр шире, поэтому центр камеры держим дальше от краёв.
        float halfWidth = cam.orthographicSize * cam.aspect;
        goal.x = Mathf.Clamp(goal.x, minX + halfWidth, maxX - halfWidth);   // Clamp: держим число между min и max
        goal.y = Mathf.Max(goal.y, minY);             // Max: не меньше minY

        return goal;
    }
}
