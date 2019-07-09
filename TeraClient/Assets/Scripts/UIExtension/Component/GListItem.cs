using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;
using System;

public class GListItem : GBase, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler
{
    [NoToLua]
    public delegate void ItemDelegate(GameObject go, int index);

    [NoToLua]
    [System.NonSerialized]
    public int index;

    [NoToLua]
    protected ItemDelegate _onItemInit;

    [NoToLua]
    protected ItemDelegate _onItemClick;

    [NoToLua]
    protected ItemDelegate _onItemLongPress;

    [NoToLua]
    protected ItemDelegate _onItemClickButton;

    [Serializable]
    public class ItemSelectEvent : UnityEvent<bool> { }
    public ItemSelectEvent _OnItemSelected = new ItemSelectEvent();
    public ItemSelectEvent _OnItemSelectedInv = new ItemSelectEvent();
	
    private PointerEventData _pressEvent = null;    //for long press
    private float _pressTime = 0;       //for long press

    private bool _isOn = false; //selected
    public virtual bool IsOn
    {
        get { return _isOn; }
        set
        {
            // if (_IsOn == value) return;
            _isOn = value;
            if (_OnItemSelected != null)
            {
                _OnItemSelected.Invoke(_isOn);
            }
			
            if (_OnItemSelectedInv != null)
            {
                _OnItemSelectedInv.Invoke(!_isOn);
            }
        }
    }

    //animation
    //private GTweenUIBase[] _tweenList;

    public static GListItem Get(GameObject g)
    {
        GListItem gListitem=g.GetComponent<GListItem>();
        if (gListitem==null)
        {
            gListitem = g.AddComponent<GListItem>();
        }
        return gListitem;
    }

    public ItemDelegate OnItemInit
    {
        get { return _onItemInit; }
        set { _onItemInit = value; }
    }

    public ItemDelegate OnItemClick
    {
        get { return _onItemClick; }
        set { _onItemClick = value; }
    }

    //public ItemDelegate OnItemPointerUp
    //{
    //    get { return _onItemPointerUp; }
    //    set { _onItemPointerUp = value; }
    //}

    //public ItemDelegate OnItemPointerDown
    //{
    //    get { return _onItemPointerDown; }
    //    set { _onItemPointerDown = value; }
    //}

    public ItemDelegate OnItemLongPress
    {
        get { return _onItemLongPress; }
        set { _onItemLongPress = value; }
    }

    public ItemDelegate OnItemClickButton
    {
        get { return _onItemClickButton; }
        set
        {
            _onItemClickButton = value;
            InitChildButtons();
        }
    }

    [NoToLua]
    public virtual void UpdateItem(int index, bool is_need_init)
    {
        this.index = index;
        this.gameObject.name = "item-" + index;

        if (is_need_init /*&& this.IsActive()*/ && OnItemInit != null)
        {
            OnItemInit(gameObject, index);
        }
    }

    [NoToLua]
    public virtual void OnPointerClick(PointerEventData eventData)
    {
        if (OnItemClick != null)
            OnItemClick(gameObject, index);
    }

    [NoToLua]
    public virtual void OnPointerUp(PointerEventData eventData)
    {
        if (_pressEvent != null && eventData.pointerId == _pressEvent.pointerId)
        {
            _pressEvent = null;
        }
    }

    [NoToLua]
    public virtual void OnPointerDown(PointerEventData eventData)
    {
        _pressEvent = eventData;
        _pressTime = 0;

        EventSystem.current.SetSelectedGameObject(gameObject);
    }

    public void SetPosition(Vector2 anchoredPos)
    {
        StopTween();
        RectTrans.anchoredPosition = anchoredPos;
    }

    public void PlayTween(float delay)
    {
        //if (_tweenList == null)
        //{
        //    _tweenList = GetComponents<GTweenUIBase>();
        //}

        //if (_tweenList != null)
        //{
        //    for (int i = 0; i < _tweenList.Length; i++)
        //    {
        //        _tweenList[i].PlayTween(delay);
        //    }
        //}
    }

    public void StopTween()
    {
        //if (_tweenList != null)
        //{
        //    for (int i = 0; i < _tweenList.Length; i++)
        //    {
        //        _tweenList[i].StopTween();
        //    }
        //}
    }

    private void Update()
    {
        if (_pressEvent != null)
        {
            //if (!_pressEvent.dragging)
            //{
            //    if (_pressEvent.pointerEnter != null && _pressEvent.pointerEnter.transform.IsChildOf(RectTrans))
            //    {
                    _pressTime += Time.unscaledDeltaTime;
                    if (_pressTime > 1f)
                    {
                        _pressEvent.eligibleForClick = false;

                        _pressTime = 0;
                        _pressEvent = null;

                        if (OnItemLongPress != null)
                        {
                            OnItemLongPress(gameObject, index);
                        }
                    }
                //}
                //else
                //{
                //    _pressTime = 0;
                //}
            }
        //}
    }

    private void OnItemButtonClicked(GameObject go)
    {
        if (OnItemClickButton != null)
            OnItemClickButton(go, index);
    }

    private void InitChildButtons()
    {
        GButton[] gbtns = gameObject.GetComponentsInChildren<GButton>(true);
        for (int i = 0; i < gbtns.Length; i++)
        {
            gbtns[i].OnClick += OnItemButtonClicked;
        }
        Button[] buttons = gameObject.GetComponentsInChildren<Button>(true);
        for (int i = 0; i < buttons.Length; i++)
        {
            Button btn = buttons[i];

            //this doesnt leak
            btn.onClick.AddListener(delegate() { OnItemButtonClicked(btn.gameObject); });
        }
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        _pressEvent = null;
        _pressTime = 0;
    }

    protected override void OnDestroy()
    {
        _onItemInit = null;
        _onItemClick = null;
        _onItemClickButton = null;
    }
}