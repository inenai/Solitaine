using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System;
using System.Threading.Tasks;

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

        public static async Task<GameObject> InstantiateAsync(string reference, Transform parent)
        {
            if (reference == null)
                throw new ArgumentNullException(nameof(reference));

            var handle = Addressables.InstantiateAsync(reference, parent);

            await handle.Task;

            if (handle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogError($"Failed to load asset at {reference}. Status: {handle.Status}");
                throw new Exception($"Failed to instantiate {reference}");
            }

            return handle.Result;
        }
    }
}