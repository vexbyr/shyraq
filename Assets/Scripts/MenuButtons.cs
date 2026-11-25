using UnityEngine;
using UnityEngine.SceneManagement;

// Кнопки главного меню и экрана победы. Кнопка Play открывает выбор уровня (LevelSelect.Open).
public class MenuButtons : MonoBehaviour
{
    public void Quit()
    {
#if UNITY_EDITOR
        Debug.Log("Quit");               // в редакторе Quit ничего не делает, поэтому пишем в Console
#else
        Application.Quit();              // на телефоне закрывает игру
#endif
    }

    public void ToMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
