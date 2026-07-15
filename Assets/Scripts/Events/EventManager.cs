using System;
using UnityEngine;

public static class EventManager
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public static Action OnResetGameEvent;
    public static Action OnDrawFromStockEvent;
    public static Action OnGameWon;
    public static Action OnMenuOpened;
    public static Action OnMenuClosed;
}
