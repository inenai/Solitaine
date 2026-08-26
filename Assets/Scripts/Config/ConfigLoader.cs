using Common;
using UnityEngine;

namespace Utils {
    public class ConfigLoader : MonoBehaviour
    {
        [SerializeField] Configs _configs;

        void Start()
        {
            CommonUtils.LoadConfig(_configs);
        }
    }
}