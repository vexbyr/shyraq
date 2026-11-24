// Что выбрано в меню и живёт между сценами. static — одно значение на всю игру,
// не пропадает при SceneManager.LoadScene (в отличие от объектов сцены).
public static class GameSession
{
    public static LevelData CurrentLevel;     // какой уровень играем сейчас (ставит меню или LevelLoader)
    public static bool OpenLevelSelect;       // true — главное меню сразу открывает выбор уровня (кнопка «Level select» на экране итогов)
}
