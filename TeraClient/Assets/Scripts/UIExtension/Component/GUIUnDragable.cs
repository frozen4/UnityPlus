using UnityEngine;
using UnityEngine.EventSystems;
namespace GNewUI
{
    public class GUIUnDragable : MonoBehaviour, IDragHandler
    {
        [NoToLua]
        public void OnDrag(PointerEventData eventData)
        {
        }
    }
}
