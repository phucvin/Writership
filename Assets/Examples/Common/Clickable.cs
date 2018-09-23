using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Examples.Common
{
    public class Clickable : MonoBehaviour, IPointerClickHandler
    {
        public UnityEvent onClick = null;

        public void OnPointerClick(PointerEventData eventData)
        {
            onClick.Invoke();
        }
    }
}