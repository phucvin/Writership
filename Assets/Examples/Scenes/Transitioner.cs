using UnityEngine;

namespace Examples.Scenes
{
    public class Transitioner : MonoBehaviour
    {
        [SerializeField]
        private Animator animator = null;

        private bool isInEnd = false;
        private bool isOutEnd = false;

        public CustomYieldInstruction In()
        {
            isInEnd = false;
            animator.SetTrigger("in");
            return new WaitUntil(() => isInEnd);
        }

        public CustomYieldInstruction Out()
        {
            isOutEnd = false;
            animator.SetTrigger("out");
            return new WaitUntil(() => isOutEnd);
        }

        public void EndIn()
        {
            isInEnd = true;
        }

        public void EndOut()
        {
            isOutEnd = true;
        }
    }
}