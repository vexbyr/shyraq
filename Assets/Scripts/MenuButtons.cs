using UnityEngine;
using UnityEngine.SceneManagement;

// Кнопки главного меню и экрана победы. Кнопка Play открывает выбор уровня (LevelSelect.Open).
public class MenuButtons : MonoBehaviour
{
    public void Quit()
    {
        Debug.Log("Quit");               // в редакторе Quit ничего не делает, поэтому пишем в Console
        Application.Quit();              // на телефоне закрывает игру
    }

    public void ToMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
