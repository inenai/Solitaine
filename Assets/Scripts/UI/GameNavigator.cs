using Common;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class GameNavigator
{
    public static void ToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public static void ExitApplication()
    {
        Debug.Log("Quitting application.");
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    public static void LoadGame(SolitaireKind kind)
    {
        switch (kind)
        {
            case SolitaireKind.KLONDIKE:
                LoadKlondike();
                return;
            case SolitaireKind.SAWAYAMA:
                LoadSawayama();
                return;
            case SolitaireKind.FREECELL:
                LoadFreeCell();
                return;
        }
    }

    public static void LoadKlondike()
    {
        SceneManager.LoadScene("Game_Klondike");
    }

    public static void LoadSawayama()
    {
        SceneManager.LoadScene("Game_Sawayama");
    }

    public static void LoadFreeCell()
    {
        SceneManager.LoadScene("Game_FreeCell");
    }
}

