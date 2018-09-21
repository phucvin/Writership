using UnityEngine;

namespace Examples.Scenes
{
    public class CoroutineExecutor : MonoBehaviour
    {
        public static CoroutineExecutor Instance { get; private set; }

        public void Awake()
        {
            Instance = this;
        }
    }
}