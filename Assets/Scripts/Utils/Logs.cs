using UnityEngine;
namespace Common
{
    public static class Logs
    {
        public static void Log(string str)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log(str);
#endif
        }
    }
}