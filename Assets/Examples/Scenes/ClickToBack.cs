using UnityEngine;
using UnityEngine.EventSystems;

namespace Examples.Scenes
{
    public class ClickToBack : MonoBehaviour, IPointerClickHandler
    {
        public void OnPointerClick(PointerEventData eventData)
        {
            ScenesMain.Instance.Back(false);
        }
    }
}