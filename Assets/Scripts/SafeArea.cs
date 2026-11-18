using UnityEngine;

// Висит на панели SafeArea внутри Canvas. Растягивает панель на «безопасную» часть экрана:
// без выреза камеры и без закруглённых углов. Кнопки и текст кладём внутрь этой панели.
public class SafeArea : MonoBehaviour
{
    RectTransform rt;
    Rect lastSafeArea;
    int lastWidth;                            // размер экрана, по которому считали в прошлый раз
    int lastHeight;

    void Awake()
    {
        rt = GetComponent<RectTransform>();
        Apply();
    }

    // Если экран повернули или в Device Simulator сменили телефон — пересчитываем.
    // Проверяем и зону, и размер экрана: в первом кадре размер может быть ещё от прошлого устройства.
    void Update()
    {
        if (Screen.safeArea != lastSafeArea || Screen.width != lastWidth || Screen.height != lastHeight)
            Apply();
    }

    void Apply()
    {
        Rect safe = Screen.safeArea;          // безопасная зона в пикселях
        lastSafeArea = safe;
        lastWidth = Screen.width;
        lastHeight = Screen.height;

        // Якоря задаются долями экрана (0..1), поэтому делим пиксели на размер экрана
        Vector2 min = safe.position;
        Vector2 max = safe.position + safe.size;
        min.x /= Screen.width;  min.y /= Screen.height;
        max.x /= Screen.width;  max.y /= Screen.height;

        rt.anchorMin = min;
        rt.anchorMax = max;
    }
}
