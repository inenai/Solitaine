using UnityEngine;

namespace Common{
    public class ExitButton : MonoBehaviour
    {
        public void ExitGame()
        {
            GameNavigator.ExitApplication();
        }
    }
}