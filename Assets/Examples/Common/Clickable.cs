using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Examples.Common
{
    public class Clickable : MonoBehaviour, IPointerClickHandler
    {
        public UnityEvent onClick = null;
        public UnityEvent onNonInteractableClick = null;

        private Selectable selectable;

        public void Awake()
        {
            selectable = GetComponent<Selectable>();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            onClick.Invoke();
            if (selectable && !selectable.interactable)
            {
                onNonInteractableClick.Invoke();
            }
        }
    }
}