using System;
using UnityEngine;

public class LoadGameConfirmPanel : MonoBehaviour
{
    public Action<bool> OnDecisionMade;

    public void OnClose()
    {
        gameObject.SetActive(false);
    }

    public void OnLoadSavedGame()
    {
        OnDecisionMade?.Invoke(true);
    }

    public void OnStartNewGame()
    {
        OnDecisionMade?.Invoke(false);
    }
}
