using UnityEngine;

public class FPSSetter : MonoBehaviour
{
    void Start()
    {
#if UNITY_ANDROID
        Application.targetFrameRate = 60;
#endif
    }
}
