using UnityEngine;

namespace Common
{
    public class MainMenuButton : MonoBehaviour
    {
        public void ToMainMenu()
        {
            EventManager.OnExitToMainMenu?.Invoke();
            GameNavigator.ToMainMenu();
        }
    }
}