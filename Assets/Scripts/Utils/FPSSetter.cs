using UnityEngine;

namespace Utils
{
    public class FPSSetter : MonoBehaviour
    {
        void Start()
        {
#if UNITY_ANDROID
            Application.targetFrameRate = 60;
#endif
        }
    }
}