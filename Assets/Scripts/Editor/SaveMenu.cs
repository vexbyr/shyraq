using System.IO;
using UnityEditor;
using UnityEngine;

// Только для редактора (папка Editor в игру не попадает): пункты меню Shyraq для проверки сохранений.
public static class SaveMenu
{
    [MenuItem("Shyraq/Open Save Folder")]
    static void OpenSaveFolder()
    {
        EditorUtility.RevealInFinder(SaveSystem.SavePath);   // открывает папку в Проводнике
    }

    [MenuItem("Shyraq/Delete Save File")]
    static void DeleteSaveFile()
    {
        if (File.Exists(SaveSystem.SavePath))
        {
            File.Delete(SaveSystem.SavePath);
            Debug.Log("Сохранение удалено: " + SaveSystem.SavePath);
        }
        else
        {
            Debug.Log("Файла сохранения нет: " + SaveSystem.SavePath);
        }
    }
}
