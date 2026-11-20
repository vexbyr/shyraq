using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.OnScreen;
using UnityEngine.InputSystem.UI;

// Экранный стик, привязанный к одному пальцу (замена стандартного OnScreenStick).
// Палец, который нажал на ручку, запоминается по touchId. Дальше стик каждый кадр читает
// положение только этого пальца и отпускается, только когда поднят именно он.
// Другие пальцы (кнопки FIRE / LIGHT / JUMP) стик не трогают. Палец может уйти за край стика —
// ручка просто упирается в movementRange, пока палец касается экрана.
// Значение уходит в виртуальный геймпад (<Gamepad>/leftStick), как у OnScreenStick, поэтому
// PlayerInput и OnMove работают без изменений.
// В редакторе можно двигать мышью (touchId = 0 — это мышь).
// Висит на ручке стика (Stick_Knob, Image с Raycast Target).
public class TouchStick : OnScreenControl, IPointerDownHandler
{
    [InputControl(layout = "Vector2")]
    [SerializeField] string stickControlPath = "<Gamepad>/leftStick";
    [SerializeField] float movementRange = 100f;   // на сколько уходит ручка (единицы Canvas)

    protected override string controlPathInternal
    {
        get => stickControlPath;
        set => stickControlPath = value;
    }

    RectTransform knob;
    Vector2 startPos;            // где ручка стоит в покое
    Vector2 pointerDownPos;      // где палец коснулся (в координатах родителя ручки)
    Camera uiCamera;             // камера Canvas (для Overlay — null)
    bool held;                   // стик сейчас держат
    Touchscreen screen;          // экран, на котором лежит наш палец (null — мышь)
    int touchId;                 // номер нашего пальца

    void Start()
    {
        knob = (RectTransform)transform;
        startPos = knob.anchoredPosition;
    }

    // Палец (или мышь) нажал на ручку. Если стик уже держат — второй палец не забирает его
    public void OnPointerDown(PointerEventData eventData)
    {
        if (held) return;

        ExtendedPointerEventData touchData = eventData as ExtendedPointerEventData;
        if (touchData != null && touchData.touchId != 0)
        {
            screen = touchData.device as Touchscreen;
            touchId = touchData.touchId;
        }
        else
        {
            screen = null;               // мышь
            touchId = 0;
        }

        uiCamera = eventData.pressEventCamera;
        pointerDownPos = ScreenToLocal(eventData.position);
        held = true;
    }

    void Update()
    {
        if (!held) return;

        Vector2 screenPos;
        if (screen != null)
        {
            TouchControl finger = FindFinger();
            if (finger == null)                  // наш палец поднят (или система отменила касание)
            {
                Release();
                return;
            }
            screenPos = finger.position.ReadValue();
        }
        else
        {
            if (Mouse.current == null || !Mouse.current.leftButton.isPressed)
            {
                Release();
                return;
            }
            screenPos = Mouse.current.position.ReadValue();
        }

        // Смещение от точки касания, не дальше movementRange — за краем ручка просто упирается
        Vector2 delta = Vector2.ClampMagnitude(ScreenToLocal(screenPos) - pointerDownPos, movementRange);
        knob.anchoredPosition = startPos + delta;
        SendValueToControl(delta / movementRange);   // от -1 до 1 по каждой оси
    }

    // Ищем среди касаний экрана наш палец по touchId. Нет его или он уже закончился — null
    TouchControl FindFinger()
    {
        foreach (TouchControl touch in screen.touches)
        {
            if (touch.touchId.ReadValue() == touchId && touch.isInProgress) return touch;
        }
        return null;
    }

    // Точка экрана → координаты внутри родителя ручки (там же, где anchoredPosition)
    Vector2 ScreenToLocal(Vector2 screenPos)
    {
        Vector2 local;
        RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)knob.parent, screenPos, uiCamera, out local);
        return local;
    }

    // Отпустили: ручка в центр, геймпад получает ноль
    void Release()
    {
        held = false;
        knob.anchoredPosition = startPos;
        SendValueToControl(Vector2.zero);
    }

    // Стик выключили (например, сцена сменилась), пока его держали, — отпускаем
    protected override void OnDisable()
    {
        if (held) Release();
        base.OnDisable();
    }
}
