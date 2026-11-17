using UnityEngine;

// Камера плавно едет за игроком и не выходит за края уровня.
public class CameraFollow : MonoBehaviour
{
    public Transform target;                          // Ай
    public Vector3 offset = new Vector3(0f, 2f, -10f);   // позиция Ай = ноги, поэтому камеру поднимаем на 2; z = -10, чтобы камера видела сцену
    public float smooth = 5f;                         // больше = быстрее догоняет

    [Header("Границы уровня (центр камеры)")]
    public float minX = -100f;
    public float maxX = 100f;
    public float minY = -100f;                        // ниже не опускаемся, чтобы не показывать пустоту под землёй

    // LateUpdate идёт после движения игрока, поэтому камера не дрожит
    void LateUpdate()
    {
        if (target == null) return;

        Vector3 goal = target.position + offset;
        goal.x = Mathf.Clamp(goal.x, minX, maxX);     // Clamp: держим число между min и max
        goal.y = Mathf.Max(goal.y, minY);             // Max: не меньше minY

        transform.position = Vector3.Lerp(transform.position, goal, smooth * Time.deltaTime);
    }
}
