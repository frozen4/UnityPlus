using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.Events;


/// <summary>
/// 有减血跟随效果的血条，加血不跟随
/// </summary>
public class GBlood : GBase
{
    [NoToLua]
    public RectTransform _FrontRect;
    [NoToLua]
    public RectTransform _BackRect;
    [NoToLua]
    public RectTransform _BGRect;

    [SerializeField]
    private bool _UseFilled = false;
    [SerializeField]
    private float _Delay = .3f;
    [SerializeField]
    private float _Time = .5f;

    [NoToLua]
    public Sprite SpFront1;
    [NoToLua]
    public Sprite SpFront2;
    [NoToLua]
    public Sprite SpBG;

    [SerializeField]
    [NoToLua]
    public Image _FrontImg;
    [SerializeField]
    [NoToLua]
    public Image _BackImg;
    [SerializeField]
    [NoToLua]
    public Image _BGImg;
    [SerializeField]
    [NoToLua]
    public Image _GuardImg;

    private float _GuardValue = 0;
    private float _Value = 0;
    //private float _MaxSize;
    //private float _InvokeValue;
    private bool _IsInited;

    private Vector2 _OriSize;

    [NoToLua]
    public UnityAction<string, string> OnTweenFinishCallBack;

    Tween _Tween;
    private void OnTweenComplete()
    {
        if (OnTweenFinishCallBack != null)
        {
            OnTweenFinishCallBack(gameObject.name, "GBlood");
        }

        _Tween = null;
    }

    private void SyncValue()
    {
        if (_IsInited)
        {
            if (_Tween != null)
            {
                _Tween.Kill(false);
            }

            if (_UseFilled)
            {
                _FrontImg.fillAmount = _Value;
                _BackImg.fillAmount = _Value;
            }
            else
            {
                Vector2 size = _OriSize;
                size.x = Mathf.Max(size.x * _Value, 0.01f);

                _FrontRect.sizeDelta = size;
                _BackRect.sizeDelta = size;
            }
        }
    }

    private void PlayValue()
    {
        if (_IsInited)
        {
            if (_Tween != null)
            {
                _Tween.Kill(false);
            }

            if (_UseFilled)
            {
                //_FrontImg.DOFillAmount(_Value, 0.1f);
                _FrontImg.fillAmount = _Value;
                _Tween = _BackImg.DOFillAmount(_Value, _Time);
                _Tween.SetDelay(_Delay);
                _Tween.OnComplete(OnTweenComplete);
            }
            else
            {
                Vector2 size = _OriSize;
                size.x = Mathf.Max(size.x * _Value, 0.01f);
                //_FrontRect.DOSizeDelta(size, 0.1f);
                _FrontRect.sizeDelta = size;
                _Tween = _BackRect.DOSizeDelta(size, _Time);
                _Tween.SetDelay(_Delay);
                _Tween.OnComplete(OnTweenComplete);
            }
        }
    }

    protected void SafeInit()
    {
        if (!_IsInited)
        {
            if (_BGRect)
            {
                if (_BGImg == null)
                    _BGImg = _BGRect.GetComponent<Image>();
            }

            if (_UseFilled)
            {
                if (_FrontImg == null)
                    _FrontImg = _FrontRect.GetComponent<Image>();
                _FrontImg.type = Image.Type.Filled;
                if (_BackImg == null)
                    _BackImg = _BackRect.GetComponent<Image>();
                _BackImg.type = Image.Type.Filled;
            }
            else
            {
                //_MaxSize = RectTrans.sizeDelta.x;
                //_Value = _FrontRect.sizeDelta.x / _MaxSize;
                //_Value = _FrontRect.sizeDelta.x / RectTrans.sizeDelta.x;
                _FrontRect.anchorMin = _FrontRect.anchorMax = _FrontRect.pivot = _BackRect.anchorMin = _BackRect.anchorMax = _BackRect.pivot = Vector2.up * .5f;
                //_FrontRect.anchoredPosition = _BackRect.anchoredPosition = Vector2.zero;
                //_FrontRect.sizeDelta = _BackRect.sizeDelta = RectTrans.sizeDelta;
                _OriSize = _FrontRect.sizeDelta;
                _BackRect.sizeDelta = _OriSize;
            }

            if (_GuardImg != null)
            {
                _GuardImg.type = Image.Type.Filled;
                _GuardImg.fillAmount = 0;
            }

            _IsInited = true;
        }
    }

    protected override void Awake()
    {
        base.Awake();
        SafeInit();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        SyncValue();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        OnTweenFinishCallBack = null;
    }

    public void SetParam(float delay, float time)//主要是为程序提供lua中修改参数的方法
    {
        _Delay = delay;
        _Time = time;
    }

    public void SetValue(float value)
    {
        SafeInit();

        value = Mathf.Clamp(value, 0f, 1f);
        if (Mathf.Abs(_Value - value) < Util.FloatZero) return;

        if (value > _Value)
        {
            _Value = value;
            if (gameObject.activeInHierarchy)
            {
                SyncValue();
            }
        }
        else
        {
            _Value = value;
            if (gameObject.activeInHierarchy)
            {
                PlayValue();
            }
        }
    }

    public void SetGuardValue(float value)
    {
        SafeInit();

        value = Mathf.Clamp(value, 0f, 1f);
        if (Mathf.Abs(_GuardValue - value) < Util.FloatZero) return;

        if (_GuardImg != null)
        {
            _GuardValue = value;
            _GuardImg.fillAmount = _GuardValue;
        }
    }

    public void MakeInvalid()
    {
        if (_Tween != null)
        {
            _Tween.Kill(false);
        }
        _Value = -1;
    }

    public void SetValueImmediately(float value)
    {
        SafeInit();

        value = Mathf.Clamp(value, 0f, 1f);
        if (Mathf.Abs(_Value - value) < Util.FloatZero) return;
        //float offset = value - _Value;
        _Value = value;
        //if (float.IsNaN(_Value)) _Value = 0f;

        SyncValue();
    }

    public void SetLineStyle(int i_line)
    {
        if (_BGImg != null && _FrontImg != null)
        {
            if (i_line < 2)
            {
                _FrontImg.sprite = SpFront1;
                _BGImg.sprite = SpBG;
                _BGImg.color = new Color(0, 0, 0, 0);
            }
            else if (i_line % 2 == 0)
            {
                _FrontImg.sprite = SpFront1;
                _BGImg.sprite = SpFront2;
                _BGImg.color = Color.white;
            }
            else
            {
                _FrontImg.sprite = SpFront2;
                _BGImg.sprite = SpFront1;
                _BGImg.color = Color.white;
            }
        }
    }
}
