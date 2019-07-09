using UnityEngine;
using System.Collections;
using Common;
using UnityEngine.EventSystems;
public class EventTriggerListener : UnityEngine.EventSystems.EventTrigger
{
    public delegate void VoidDelegate(GameObject go);
    public VoidDelegate onClick;
    public VoidDelegate onDown;
    public VoidDelegate onEnter;
    public VoidDelegate onExit;
    public VoidDelegate onUp;
    public VoidDelegate onSelect;
    public VoidDelegate onDeselect;
    public VoidDelegate onUpdateSelect;
    public VoidDelegate onScroll;

    public static EventTriggerListener Get(GameObject go)
    {
        EventTriggerListener listener = go.GetComponent<EventTriggerListener>();
        if (listener == null) listener = go.AddComponent<EventTriggerListener>();
        return listener;
    }
    public override void OnPointerClick(PointerEventData eventData)
    {
        HobaDebuger.Log("OnPointerClick " + eventData.pointerEnter.name);
        if (onClick != null) onClick(gameObject);
    }
    public override void OnPointerDown(PointerEventData eventData)
    {
        if (onDown != null) onDown(gameObject);
    }
    public override void OnPointerEnter(PointerEventData eventData)
    {
        if (onEnter != null) onEnter(gameObject);
    }
    public override void OnPointerExit(PointerEventData eventData)
    {
        if (onExit != null) onExit(gameObject);
    }
    public override void OnPointerUp(PointerEventData eventData)
    {
        if (onUp != null) onUp(gameObject);
    }
    public override void OnSelect(BaseEventData eventData)
    {
        HobaDebuger.Log("OnSelect ");

        if (onSelect != null) onSelect(gameObject);
    }

    public new virtual void OnDeselect(BaseEventData eventData)
    {
        HobaDebuger.Log("OnDeselect ");
        if (onDeselect != null) onDeselect(gameObject);
    }

    public override void OnUpdateSelected(BaseEventData eventData)
    {
        HobaDebuger.Log("OnUpdateSelected ");
        if (onUpdateSelect != null) onUpdateSelect(gameObject);
    }

    public new virtual void OnScroll(PointerEventData eventData)
    {
        HobaDebuger.Log("OnScroll ");
        if (onScroll != null) onScroll(gameObject);
    }
}