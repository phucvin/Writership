using System;
using UnityEngine;

namespace Examples.Common
{
    public class Map : MonoBehaviour
    {
        [Serializable]
        public class Pair
        {
            public string Name;
            public GameObject GameObject;
        }

        [SerializeField]
        private Pair[] list = null;

        public T GetComponent<T>(string name)
            where T : Component
        {
            var gameObject = Get(name);
            if (gameObject) return gameObject.GetComponent<T>();
            else return null;
        }

        public GameObject Get(string name)
        {
            for (int i = 0, n = list != null ? list.Length : 0; i < n; ++i)
            {
                var pair = list[i];
                if (pair.Name == name) return pair.GameObject;
            }
            return null;
        }
    }
}