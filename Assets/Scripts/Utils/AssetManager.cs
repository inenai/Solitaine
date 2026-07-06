using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System;

namespace Utils
{
    public static class AssetManager
    {
        public static void InstantiateAsync(string reference, Transform parent, Action<GameObject> onInstantiated, Action onError)
        {
            if (reference == null)
            {
                onError?.Invoke();
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
                    Debug.LogError($"Failed to load asset at {reference}. Status: {op.Status}");
                    onError?.Invoke();
                }
            };
        }
    }
}