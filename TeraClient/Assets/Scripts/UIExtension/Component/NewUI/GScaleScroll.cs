using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

////*********Debug
//using System.Text;
////*********

public class GScaleScroll : ScrollRect, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
{
    [NoToLua]
    public float maxScale = 2f;
    [NoToLua]
    public float minScale = 1f;
    [NoToLua]
    public float MouseWheelSensitivity = 0.25f;

    [NoToLua]
    public delegate void OnScaleChangeCall(GameObject g, float scale);
    [NoToLua]
    public OnScaleChangeCall onScaleChanged;

    private float scaleAmount = 0f;
    private List<PointerEventData> pointerList = new List<PointerEventData>();
    private PointerEventData draggingPointer;

    ////*********Debug
    //Text MessagePointer;
    //Text MessageEvent;
    //Text SliderValue;
    //Slider TestSlider;

    //protected override void Awake()
    //{
    //    base.Awake();

    //    GameObject g;
    //    RectTransform t;

    //    g = Instantiate<GameObject>(Resources.Load<GameObject>("SScroll_Debug"));
    //    t = g.transform as RectTransform;
    //    t.SetParent(transform.parent, false);
    //    t = g.transform as RectTransform;
    //    MessagePointer = t.Find("MessagePointer").GetComponent<Text>();
    //    MessageEvent = t.Find("MessageEvent").GetComponent<Text>();
    //    SliderValue = t.Find("SliderValue").GetComponent<Text>();
    //    TestSlider = t.Find("Slider").GetComponent<Slider>();
    //    TestSlider.value = 0.125f;
    //}

    //void DebugInfo()
    //{
    //    MessagePointer.text = EventSystem.current.ToString();

    //    float delta_pos = 0;
    //    Vector2 center_pos = Vector2.zero;
    //    if (pointerList.Count == 2)
    //    {
    //        Vector2 posA;
    //        Vector2 posB;
    //        Vector2 posA_O;
    //        Vector2 posB_O;

    //        RectTransformUtility.ScreenPointToLocalPointInRectangle(content, pointerList[1].pressPosition + pointerList[1].delta, pointerList[1].pressEventCamera, out posA);
    //        RectTransformUtility.ScreenPointToLocalPointInRectangle(content, pointerList[0].pressPosition + pointerList[0].delta, pointerList[0].pressEventCamera, out posB);
    //        RectTransformUtility.ScreenPointToLocalPointInRectangle(content, pointerList[1].pressPosition, pointerList[1].pressEventCamera, out posA_O);
    //        RectTransformUtility.ScreenPointToLocalPointInRectangle(content, pointerList[0].pressPosition, pointerList[0].pressEventCamera, out posB_O);

    //        delta_pos = (posA - posB).sqrMagnitude / (posA_O - posB_O).sqrMagnitude;
    //        delta_pos -= 1;
    //        delta_pos *= GetTouchSpeed();

    //        center_pos = (posA_O + posB_O) * 0.5f;
    //    }

    //    StringBuilder sb = new StringBuilder();
    //    sb.Append("Pointers "); sb.Append(pointerList.Count); sb.Append("\n");
    //    for (int i = 0; i < pointerList.Count; i++)
    //    {
    //        sb.Append(pointerList[i].pointerId); sb.Append("/"); sb.Append(pointerList[i].pressPosition); sb.Append("/"); sb.Append(pointerList[i].position); sb.Append("\n");
    //    }
    //    sb.Append("\n");

    //    sb.Append("Drag \n");
    //    if (draggingPointer != null)
    //    {
    //        sb.Append(draggingPointer.pointerId); sb.Append("/"); sb.Append(draggingPointer.pressPosition); sb.Append("/"); sb.Append(draggingPointer.position); sb.Append("\n");
    //        sb.Append("\n");
    //    }

    //    sb.Append("Center "); sb.Append(pointerList.Count); sb.Append(center_pos.ToString()); sb.Append("\n");
    //    sb.Append("Delta "); sb.Append(pointerList.Count); sb.Append(delta_pos.ToString()); sb.Append("\n");

    //    MessageEvent.text = sb.ToString();

    //    SliderValue.text = TestSlider.value.ToString();
    //}
    ////*********

    protected bool _isInited=false;

    public void CenterOnPos(Vector2 pos)
    {
        SafeInit();

        float sc2 = Mathf.Lerp(minScale, maxScale, scaleAmount);
        Vector2 offset = pos * sc2 + content.anchoredPosition;

        ClampPos(ref offset);
        content.anchoredPosition -= offset;
    }

    [NoToLua]
    public override void OnBeginDrag(PointerEventData eventData)
    {
        if (pointerList.Count > 1)
        {
            if (draggingPointer != null)
            {
                OnEndDrag(draggingPointer);
            }
        }
        else
        {
            if (draggingPointer == null)
            {
                draggingPointer = eventData;
                base.OnBeginDrag(draggingPointer);
                //Debug.Log("OnBeginDrag " + draggingPointer.pointerId + "/" + draggingPointer.position);
            }
        }
    }

    [NoToLua]
    public override void OnEndDrag(PointerEventData eventData)
    {
        if (draggingPointer != null && draggingPointer.pointerId == eventData.pointerId)
        {
            //Debug.Log("OnEndDrag " + draggingPointer.pointerId + "/" + draggingPointer.position);
            base.OnEndDrag(draggingPointer);
            draggingPointer = null;
        }
    }

    [NoToLua]
    public override void OnDrag(PointerEventData eventData)
    {
        if (pointerList.Count > 1)
        {
            for (int i = 0; i < pointerList.Count; i++)
            {
                pointerList[i].eligibleForClick = false;
            }
        }
        else //if (pointerList.Count == 1 /*&& touchNum > 1*/)
        {
            if (draggingPointer == null)
            {
                OnBeginDrag(eventData);
            }

            if (draggingPointer != null && draggingPointer.pointerId == eventData.pointerId)
            {
                base.OnDrag(draggingPointer);
                //Debug.Log("OnDrag " + draggingPointer.pointerId+"/"+ draggingPointer.position);
            }
        }
    }

    [NoToLua]
    public void OnPointerDown(PointerEventData eventData)
    {
        RecordPointer(eventData);
    }

    [NoToLua]
    public void OnPointerUp(PointerEventData eventData)
    {
        RemovePointer(eventData);
    }

    [NoToLua]
    public void OnPointerClick(PointerEventData eventData)
    {
        if (!eventData.dragging)
        {
            Transform p = transform.parent;
            GameObject g = ExecuteEvents.GetEventHandler<IPointerClickHandler>(p.gameObject);
            ExecuteEvents.Execute(g, eventData, ExecuteEvents.pointerClickHandler);
        }
    }

    protected virtual void SafeInit()
    {
        if (!_isInited)
        {
            if (content != null)
            {
                content.anchorMin = content.anchorMax = content.pivot = new Vector2(0.5f, 0.5f);
            }
            _isInited = true;
        }
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        SafeInit();
    }

    protected override void LateUpdate()
    {
        if (content == null || viewport == null) return; //refuse to work.

#if (UNITY_EDITOR||UNITY_STANDALONE_WIN) && !ART_USE
        if (Input.GetAxis("Mouse ScrollWheel") != 0)
        {
            if (InputManager.Instance.GetPointerOverUI(-1) == content.gameObject)
            {
                Vector2 pos = Input.mousePosition;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(content, pos, GameObjectUtil.GetUICamera(), out pos);

                float w = Input.GetAxis("Mouse ScrollWheel");
                if (Mathf.Abs(w) > 0.01f)
                {
                    ScaleContain(w * MouseWheelSensitivity, pos);

                    if (draggingPointer != null)
                    {
                        //Debug.Log("OnEndDrag " + draggingPointer.pointerId + "/" + draggingPointer.position);
                        base.OnEndDrag(draggingPointer);
                        draggingPointer = null;
                    }

                    for (int i = 0; i < pointerList.Count; i++)
                    {
                        pointerList[i].eligibleForClick = false;
                    }
                }
            }
        }
#endif

        ////*********Debug
        //DebugInfo();
        ////*********

        if (pointerList.Count == 2)
        {
            Vector2 posA;
            Vector2 posB;
            Vector2 posA_O;
            Vector2 posB_O;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(content, pointerList[1].pressPosition + pointerList[1].delta, pointerList[1].pressEventCamera, out posA);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(content, pointerList[0].pressPosition + pointerList[0].delta, pointerList[0].pressEventCamera, out posB);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(content, pointerList[1].pressPosition, pointerList[1].pressEventCamera, out posA_O);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(content, pointerList[0].pressPosition, pointerList[0].pressEventCamera, out posB_O);

            float delta_pos = (posA - posB).magnitude / (posA_O - posB_O).magnitude;
            delta_pos -= 1;

            if (Mathf.Abs(delta_pos) > 0.01f)
            {
                delta_pos *= GetTouchSpeed();

                Vector2 center_pos = (posA_O + posB_O) * 0.5f;
                ScaleContain(delta_pos, center_pos);
            }
        }

        base.LateUpdate();
    }

    private float GetTouchSpeed()
    {
        return MouseWheelSensitivity;
        ////*********Debug
        //return (TestSlider.value + 0.01f) * 2;
        ////*********
    }

    private void ScaleContain(float delta, Vector2 pos)
    {
        float sc1 = content.localScale.x;
        scaleAmount = Mathf.Clamp01(scaleAmount + delta);

        float sc2 = Mathf.Lerp(minScale, maxScale, scaleAmount);
        content.localScale = Vector3.one * sc2;

        Vector2 offset = pos * (sc2 - sc1);

        ClampPos(ref offset);
        content.anchoredPosition -= offset;

        if (onScaleChanged != null)
        {
            onScaleChanged(gameObject, sc2);
        }
    }

    private void RecordPointer(PointerEventData eventData)
    {
        for (int i = 0; i < pointerList.Count; i++)
        {
            if (pointerList[i].pointerId == eventData.pointerId)
            {
                pointerList[i] = eventData;
                return;
            }
        }

        if (pointerList.Count < 2)
        {
            pointerList.Add(eventData);
        }
    }

    private void RemovePointer(PointerEventData eventData)
    {
        for (int i = 0; i < pointerList.Count; i++)
        {
            if (pointerList[i].pointerId == eventData.pointerId)
            {
                pointerList.RemoveAt(i);
            }
        }
    }

    private bool HasPointer(PointerEventData eventData)
    {
        for (int i = 0; i < pointerList.Count; i++)
        {
            if (pointerList[i].pointerId == eventData.pointerId)
            {
                return true;
            }
        }
        return false;
    }

    private void ClampPos(ref Vector2 offset)
    {
        float sc2 = Mathf.Lerp(minScale, maxScale, scaleAmount);

        if (movementType == MovementType.Clamped)
        {
            if (content.anchorMin == content.anchorMax)
            {
                Vector2 cur_pos = content.anchoredPosition - offset;

                Rect rt_view = viewport.rect;
                Rect rt_con = content.rect;

                Vector2 min_v = new Vector2(-rt_view.width * content.anchorMin.x, -rt_view.height * content.anchorMin.y);
                Vector2 max_v = new Vector2(rt_view.width, rt_view.height) + min_v;

                Vector2 min_c = cur_pos - new Vector2(rt_con.width * content.pivot.x, rt_con.height * content.pivot.y) * sc2;
                Vector2 max_c = new Vector2(rt_con.width, rt_con.height) * sc2 + min_c;

                //Debug.Log(offset + " Clamped by " + min_v + ", " + max_v + ", " + min_c + ", " + max_c);



                Vector2 offset2 = Vector2.zero;
                if (horizontal)
                {
                    if (min_c.x > min_v.x)
                        offset2.x = min_v.x - min_c.x;
                    else if (max_c.x < max_v.x)
                        offset2.x = max_v.x - max_c.x;
                }

                if (vertical)
                {
                    if (max_c.y < max_v.y)
                        offset2.y = max_v.y - max_c.y;
                    else if (min_c.y > min_v.y)
                        offset2.y = min_v.y - min_c.y;
                }

                offset -= offset2;
            }

            //Debug.Log(" Clamped offset " + offset);
        }
    }

}
