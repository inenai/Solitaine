using Common;
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

    public void LoadGame(SolitaireKind kind)
    {
        switch (kind)
        {
            case SolitaireKind.KLONDIKE:
                LoadKlondike();
                return;
            case SolitaireKind.SAWAYAMA:
                LoadSawayama();
                return;
        }
    }

    public void LoadKlondike()
    {
        SceneManager.LoadScene("Game_Klondike");
    }

    public void LoadSawayama()
    {
        SceneManager.LoadScene("Game_Sawayama");
    }
}

