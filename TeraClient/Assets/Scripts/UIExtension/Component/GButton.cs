using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class GButton : Selectable, IPointerClickHandler, ISubmitHandler, IButtonClickCallBack
{
    //public class ButtonClickedEvent : UnityEvent { }
    //private ButtonClickedEvent _OnClick = new ButtonClickedEvent();

    [NoToLua]
    public delegate void OnButtonClick(GameObject go);
    [NoToLua]
    public OnButtonClick OnClick, OnSelected, PointerDownHandler, PointerUpHandler, OnPointerEnterHandler, OnPointerExitHandler, OnPointerLongPressHandler;

    private bool _Selected = false;//当前选中状态
    private Text _Text;
    [SerializeField]
    private bool _Selectable = false;//是否支持选中
    [SerializeField]
    private bool _AlwaysCallSelect = false;//选中和取消选中是不是都需要调用OnSelected回调
    [SerializeField]
    private bool _IsProfessionModel = false;        //注册OnClick以外的事件到Lua

    private bool _isEnterred = false;   //是否在按钮区域内

    [NoToLua]
    public bool IsProfessionModel { get { return _IsProfessionModel; } }

    //protected Rect _ORect = new Rect(0, 0, 0, 0);
    //protected Camera UICamera;
    //private Vector3[] _WorldCorners = new Vector3[4];

    private Transform _Transform;
    private RectTransform _RectTransform;

    [NoToLua]
    public Transform Trans
    {
        get
        {
            if (_Transform == null)
                _Transform = transform;
            return _Transform;
        }
    }

    [NoToLua]
    public RectTransform RectTrans
    {
        get
        {
            if (_RectTransform == null)
                _RectTransform = Trans as RectTransform;
            if (_RectTransform == null)
                _RectTransform = gameObject.AddComponent<RectTransform>();
            return _RectTransform;
        }
    }

    [NoToLua]
    public string text
    {
        get
        {
            if (_Text == null)
                _Text = GetComponentInChildren<Text>();

            string result = "";
            if (_Text != null)
                result = _Text.text;
            return result;
        }
        set
        {
            if (_Text == null)
                _Text = GetComponentInChildren<Text>();

            if (_Text != null)
            {
                _Text.text = value;
            }
        }
    }

    //按下button后，是否已经移开button的rect范围
    public bool IsPointerOverThis()
    {
        //·不行就还原 
        ////if (!InputManager.Instance.IsPointerBeginOnUI || _ORect == null || UICamera == null)
        ////    return false;
        ////return _ORect.Contains(UICamera.ScreenToWorldPoint(InputManager.Instance.CurrentTouchPosition));
        return IsActive() && IsInteractable() && _isEnterred;
    }

    [NoToLua]
    public bool Selectable { get { return _Selectable; } set { _Selectable = value; } }
    [NoToLua]
    public bool Selected { get { return _Selected; } }

    protected GButton() { }

    #region sysytem functions and button basic logic
    //protected override void Awake()
    //{
    //    //_Text = GetComponentInChildren<Text>();
    //    //if (targetGraphic == null)
    //    //    targetGraphic = GetComponent<Image>() as Graphic;
    //    //Graphic graphic = targetGraphic;
    //    //base.Awake();
    //    //targetGraphic = graphic;
    //}

    [NoToLua]
    public void TransToNormalWithoutCall()
    {
        _Selected = false;
        ButtonDeselectEffect();
    }

    [NoToLua]
    public void TransToSelectedWithoutCall()
    {
        _Selected = true;
        ButtonSelectEffect();
    }

    private PointerEventData _pressEvent = null;    //for long press
    private float _pressTime = 0;       //for long press
    void StartLongPressCount(PointerEventData eventData)
    {
        if (_IsProfessionModel)
        {
            _pressEvent = eventData;
            _pressTime = 0;
        }
    }

    void TurnOffLongPressCount(PointerEventData eventData)
    {
        if (_IsProfessionModel)
        {
            if (_pressEvent != null && eventData.pointerId == _pressEvent.pointerId)
            {
                _pressEvent = null;
            }
        }
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        _pressEvent = null;
        _pressTime = 0;
    }

    private void Update()
    {
        if (_IsProfessionModel)
        {
            if (_pressEvent != null)
            {
                _pressTime += Time.unscaledDeltaTime;
                if (_pressTime > 1f)
                {
                    _pressEvent.eligibleForClick = false;

                    _pressTime = 0;
                    _pressEvent = null;

                    if (OnPointerLongPressHandler != null)
                    {
                        OnPointerLongPressHandler(gameObject);
                    }
                }
            }
        }
    }

    [NoToLua]
    public virtual void OnPointerClick(PointerEventData eventData)
    {
        ////·不行就还原 
        if (IsActive() && !IsInteractable()) return;
        if (_Selectable) return;
        OnButtonClickHandler(this.gameObject);
    }

    [NoToLua]
    public virtual void OnSubmit(BaseEventData eventData)
    {
        if (!IsInteractable()) return;
        //不确定，待修改
        OnButtonClickHandler(this.gameObject);
    }




    [NoToLua]
    public override void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);

        if (IsActive() && !IsInteractable()) return;

        //_ORect = GetCurrentRect();
        if (_Selectable)
        {
            _Selected = !_Selected;
            if (_Selected)
            {
                OnButtonSelectHandler(this.gameObject);
                ButtonSelectEffect();
            }
            else
            {
                if (_AlwaysCallSelect)
                    OnButtonSelectHandler(this.gameObject);
                ButtonDeselectEffect();
            }
        }
        ButtonDownEffect();
        if (PointerDownHandler != null)
            PointerDownHandler(gameObject);

        StartLongPressCount(eventData);
    }

    [NoToLua]
    public override void OnPointerUp(PointerEventData eventData)
    {
        base.OnPointerUp(eventData);

        if (IsActive() && !IsInteractable()) return;
        if (_Selectable) return;
        ButtonUpEffect();

        //Why do it here?
        //·不行就还原 
        //if (UICamera != null && _ORect.Contains(UICamera.ScreenToWorldPoint(eventData.position)))
        //{
        //    OnButtonClickHandler(gameObject);
        //}

        TurnOffLongPressCount(eventData);

        if (PointerUpHandler != null)
            PointerUpHandler(gameObject);

    }

    [NoToLua]
    public override void OnPointerEnter(PointerEventData eventData)
    {
        base.OnPointerEnter(eventData);

        if (IsActive() && !IsInteractable()) return;
        if (_Selectable) return;

        _isEnterred = true;

        ButtonEnterEffect();

        if (OnPointerEnterHandler != null)
            OnPointerEnterHandler(gameObject);
    }

    [NoToLua]
    public override void OnPointerExit(PointerEventData eventData)
    {
        base.OnPointerExit(eventData);

        if (IsActive() && !IsInteractable()) return;
        if (_Selectable) return;

        _isEnterred = false;

        ButtonExitEffect();

        if (OnPointerExitHandler != null)
            OnPointerExitHandler(gameObject);
    }
    #endregion

    [NoToLua]
    public virtual void OnButtonClickHandler(GameObject obj)
    {
        if (_Selectable) return;
        if (OnClick != null)
            OnClick(obj);
        //AudioManager.Instance.Play("button_click");
    }

    [NoToLua]
    public virtual void OnButtonSelectHandler(GameObject value)
    {
        if (!_Selectable) return;
        if (OnSelected != null)
            OnSelected(value);
    }

    //protected Rect GetCurrentRect()
    //{
    //    RectTrans.GetWorldCorners(_WorldCorners);

    //    return Rect.MinMaxRect(Mathf.Min(_WorldCorners[0].x, _WorldCorners[2].x),
    //        Mathf.Min(_WorldCorners[0].y, _WorldCorners[2].y),
    //        Mathf.Max(_WorldCorners[0].x, _WorldCorners[2].x),
    //        Mathf.Max(_WorldCorners[0].y, _WorldCorners[2].y));
    //}

    [NoToLua]
    public virtual void ButtonDownEffect() { }

    [NoToLua]
    public virtual void ButtonUpEffect() { }

    [NoToLua]
    public virtual void ButtonEnterEffect() { }

    [NoToLua]
    public virtual void ButtonExitEffect() { }

    [NoToLua]
    public virtual void ButtonSelectEffect() { }

    [NoToLua]
    public virtual void ButtonDeselectEffect() { }

    // object类型的参数虽然灵活，但是很容易出现装箱拆箱操作，效率不高，故禁用
    //     public virtual void SetData(object data)
    //     {
    //         _Data = data;
    //     }
}
