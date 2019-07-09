using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

[RequireComponent(typeof(ScrollRect))]
public class GScrollWatcher : MonoBehaviour
//,IBeginDragHandler
//        , IDragHandler
//,IEndDragHandler
{
    ScrollRect _Scroll;
    RectTransform _RTContent;
    RectTransform _RTView;

    UnityAction<GameObject, float> _OnScroll;

    public enum DragType
    {
        LocalPosition,
        UpperLeftDiff,
        LowerRightDiff
    }
    public DragType WatchType;

    bool _HasValue = false;
    float _RVOld = 0;
    float _InternalCount;
    public float InterVal;


    private float GetOffset()
    {
        float rv = 0;
        if (WatchType == DragType.LocalPosition)
        {
            rv = !_Scroll.vertical ? _RTContent.localPosition.x : _RTContent.localPosition.y;
        }
        else if (WatchType == DragType.UpperLeftDiff)
        {
            if (!_Scroll.vertical)
            {
                rv = _RTContent.localPosition.x + _RTContent.rect.xMin - _RTView.rect.xMin;
            }
            else
            {
                rv = _RTContent.localPosition.y + _RTContent.rect.yMax - _RTView.rect.yMax;
            }
        }
        else if (WatchType == DragType.LowerRightDiff)
        {
            if (!_Scroll.vertical)
            {
                rv = _RTContent.localPosition.x + _RTContent.rect.xMax - _RTView.rect.xMax;
            }
            else
            {
                rv = _RTContent.localPosition.y + _RTContent.rect.yMin - _RTView.rect.yMin;
            }
        }
        return rv;
    }

    public virtual void Update()
    {
        if (_Scroll == null)
        {
            _Scroll = GetComponent<ScrollRect>();
            _RTContent = _Scroll.content;
            _RTView = _Scroll.viewport != null ? _Scroll.viewport : _Scroll.GetComponent<RectTransform>();

            if (_RTContent == null || _RTView == null)
            {
                enabled = false;
                return;
            }
        }

        if (_Scroll.velocity.sqrMagnitude > 0.01f)
        {
            if (_InternalCount > InterVal)
            {
                float rv = GetOffset();

                if (!_HasValue)
                {
                    _RVOld = rv;
                    _HasValue = true;
                }

                if (Mathf.Abs(_RVOld - rv) > 0.9f)
                {
                    if (_OnScroll != null)
                    {
                        _OnScroll(gameObject, rv);
                    }
                    //Debug.Log("OnScroll " + rv);
                    _RVOld = rv;
                }
                _InternalCount -= InterVal;
            }
        }

        _InternalCount += Time.unscaledDeltaTime;
    }

    public void SetHandler(UnityAction<GameObject, float> on_scroll)
    {
        _OnScroll = on_scroll;
    }

    //public virtual void OnBeginDrag(PointerEventData eventData)
    //{
    //    //Execute(EventTriggerType.BeginDrag, eventData);
    //}

    //public virtual void OnEndDrag(PointerEventData eventData)
    //{
    //    //Execute(EventTriggerType.EndDrag, eventData);
    //    _lastPos = -1;
    //}

    //public virtual void OnDrag(PointerEventData eventData)
    //{
    //    if (_Scroll == null)
    //    {
    //        _Scroll = GetComponent<ScrollRect>();
    //        _RTContent = _Scroll.content;
    //        _RTView = _Scroll.viewport != null ? _Scroll.viewport : _Scroll.GetComponent<RectTransform>();

    //        if (_RTContent == null || _RTView == null)
    //        {
    //            enabled = false;
    //            return;
    //        }
    //    }

    //    float rv = 0;
    //    if (WatchType == DragType.LocalPosition)
    //    {
    //        rv = !_Scroll.vertical ? _RTContent.localPosition.x : _RTContent.localPosition.y;
    //    }
    //    else if (WatchType == DragType.UpperLeftDiff)
    //    {
    //        if (!_Scroll.vertical)
    //        {
    //            rv = _RTContent.localPosition.x + _RTContent.rect.xMin - _RTView.rect.xMin;
    //        }
    //        else
    //        {
    //            rv = _RTContent.localPosition.y + _RTContent.rect.yMax - _RTView.rect.yMax;
    //        }
    //    }
    //    else if (WatchType == DragType.LowerRightDiff)
    //    {
    //        if (!_Scroll.vertical)
    //        {
    //            rv = _RTContent.localPosition.x + _RTContent.rect.xMax - _RTView.rect.xMax;
    //        }
    //        else
    //        {
    //            rv = _RTContent.localPosition.y + _RTContent.rect.yMin - _RTView.rect.yMin;
    //        }
    //    }

    //    if (_OnScroll != null)
    //    {
    //        _OnScroll(gameObject, rv);
    //    }

    //    Debug.Log("OnScroll " + rv);
    //}
}







