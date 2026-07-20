using System;
using UnityEngine;

public abstract class Game
{
    public abstract IGameState State { get; }
    protected abstract string DebugTag { get; }

    protected abstract bool Won();
    /// <summary>
    /// Command Wrapper!
    /// </summary>
    /// <param name="action"></param>
    /// <returns>Whether a UI refresh is needed</returns>
    protected bool ExecuteAction(Func<bool> action)
    {
        bool moveMade = action();

        if (moveMade)
        {
            Log();
            if (Won())
            {
                State.OnWin();
                EventManager.OnGameWon?.Invoke();
            }
        }
        return moveMade;
    }


    #region utils
    protected void Log(string message)
    {
        Debug.Log($"[{DebugTag}] {message}");
    }

    public void Log()
    {
        Debug.Log("=== SOLITAIRE STATE ===");
        State.LogState();
    }
    #endregion
}
