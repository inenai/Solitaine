using System;

public static class EventManager
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public static Action OnResetGameRequested;
    public static Action OnGameStarted;
    public static Action OnDrawFromStock;
    public static Action OnGameWon;
    public static Action OnMenuOpened;
    public static Action OnMenuClosed;
    public static Action OnCardsMoved;
    public static Action OnUndo;
    public static Action OnRedo;
}
