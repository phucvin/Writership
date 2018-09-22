using System.Collections;
using UnityEngine;
using Writership;

namespace Examples.Scenes
{
    public class CoroutineExecutor : MonoBehaviour
    {
        public static CoroutineExecutor Instance { get; private set; }

        public void Awake()
        {
            Instance = this;
        }

        public void StartCoroutine(CompositeDisposable cd, IEnumerator routine)
        {
            var co = StartCoroutine(routine);
            cd.Add(new DisposableAction(() => StopCoroutine(co)));
        }
    }
}