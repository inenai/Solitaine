using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class AssetsService : Service
{
    [SerializeField] CardTextureGetter _cardTextureGetter;

    public CardTextureGetter Cards => _cardTextureGetter;
    public override void Init()
    {

    }

    public static void InstantiateAsync(string reference, Transform parent, Action<GameObject> onInstantiated, Action<string> onError)
    {
        if (reference == null)
        {
            onError?.Invoke("Cannot instantiate asset, reference argument is null!");
            return;
        }

        var op = Addressables.InstantiateAsync(reference, parent);
        op.Completed += (opHandle) =>
        {
            if (opHandle.Status == AsyncOperationStatus.Succeeded)
            {
                onInstantiated?.Invoke(opHandle.Result);
            }
            else
            {
                onError?.Invoke($"Failed to load asset at {reference}: {op.OperationException.Message}");
            }
        };
    }
}
