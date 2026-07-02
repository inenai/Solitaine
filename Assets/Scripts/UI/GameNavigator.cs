using UnityEngine;
using UnityEngine.SceneManagement;

public class GameNavigator : MonoBehaviour
{
    public void ToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void ExitApplication()
    {
        Debug.Log("Quitting application.");
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    public void LoadKlokdike()
    {
        SceneManager.LoadScene("Klondike");
    }
}
