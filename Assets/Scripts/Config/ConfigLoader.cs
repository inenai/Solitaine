using Common;
using UnityEngine;

namespace Utils {
    public class ConfigLoader : MonoBehaviour
    {
        [SerializeField] Configs _configs;

        void Awake()
        {
            CommonUtils.LoadConfig(_configs);
        }
    }
}