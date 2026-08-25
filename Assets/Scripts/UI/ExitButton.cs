using UnityEngine;

namespace Common{
    public class ExitButton : MonoBehaviour
    {
        #if UNITY_WEBGL && !UNITY_EDITOR
        void Awake()
        {
            gameObject.SetActive(false);
        }
            #endif
        public void ExitGame()
        {
            GameNavigator.ExitApplication();
        }
    }
}