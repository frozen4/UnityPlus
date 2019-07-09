using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class GInputField : InputField
{

    public override void OnSelect(BaseEventData eventData)
    {
        base.OnSelect(eventData);
        if (placeholder != null)
            placeholder.gameObject.SetActive(false);
    }

    public override void OnDeselect(BaseEventData eventData)
    {
        base.OnDeselect(eventData);
        if (text != null)
        {
            if (text.Length < 1)
                placeholder.gameObject.SetActive(true);
        }
    }
}
