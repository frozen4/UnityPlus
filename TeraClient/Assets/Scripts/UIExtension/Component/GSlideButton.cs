using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

[DisallowMultipleComponent]
public class GSlideButton : GBase, IDragHandler, IEndDragHandler
{
#region serialization
    public Vector2 Thresehold;
#endregion

    public UnityAction<GameObject> OnSlide;

    public void OnEndDrag(PointerEventData eventData)
    {
        if (enabled && OnSlide!=null)
        {
            Vector2 d = eventData.position - eventData.pressPosition;
            /*Debug.LogWarning("pressPosition : " + eventData.pressPosition + ", position : " + eventData.position + " " + d);
            Debug.LogWarning("screen : " + Screen.width + ", " + Screen.height);*/

            d.x *= 1f / Screen.width;
            d.y *= 1f / Screen.height;

            bool test_x=false;
            bool test_y=false;
            if (Thresehold.x > 0)
            {
                test_x = d.x>Thresehold.x;
            }
            if (Thresehold.y > 0)
            {
                test_y = d.y > Thresehold.y;
            }
            if (Thresehold.x < 0)
            {
                test_x = d.x < Thresehold.x;
            }
            if (Thresehold.y < 0)
            {
                test_y = d.y < Thresehold.y;
            }

            if (test_x || test_y)
            {
                OnSlide(gameObject);
            }
        }
    }

    public void OnDrag(PointerEventData eventData) { }
}
