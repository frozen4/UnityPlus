using UnityEngine;
using System.Collections;
using DG.Tweening;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

public class GNewIOSToggle : Selectable, IPointerClickHandler
{
    #region serialization
    [NoToLua]
    public Image BackGroud;
    [NoToLua]
    public RectTransform Knob;

    [NoToLua]
    public AnimationCurve CV_Knob;
    [NoToLua]
    public Vector3 knobPos0;
    [NoToLua]
    public Vector3 knobPos1;
    [NoToLua]
    public float Duration = 0.5f;

    //[NoToLua]
    //public bool IsManual = false;
    #endregion

    //[NoToLua]
    //[System.NonSerialized]
    //public UnityAction<GameObject, bool> OnValueChanged;

    [NoToLua]
    [System.NonSerialized]
    public UnityAction<GameObject> OnClick;

    public bool Value
    {
        get { return _IsOn; }
        set { SetValue(value, true); }
    }

    private bool _IsOn;
    private Tween _TweenKnob;

    public void ToggleValue()
    {
        Value = !Value;
    }

    [NoToLua]
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        if (!IsActive() || !IsInteractable())
            return;

        //if (IsManual)
        //{
            if (OnClick != null)
            {
                OnClick(gameObject);
            }
        //}
        //else
        //{
        //    SetValue(!_IsOn, false);
        //}
    }

    protected override void Start()
    {
        base.Start();
        PlayEffect(true);
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        PlayEffect(true);
    }

    public void SetValue(bool is_on, bool is_instant)
    {
        if (_IsOn != is_on)
        {
            _IsOn = is_on;
            if (enabled && gameObject.activeInHierarchy)
            {
                PlayEffect(false);
            }
            else
            {
                PlayEffect(true);
            }

            //if (OnValueChanged != null)
            //{
            //    OnValueChanged(gameObject, _IsOn);
            //}
        }
    }

    private void PlayEffect(bool is_instant)
    {
        BackGroud.CrossFadeAlpha(_IsOn ? 1f : 0f, is_instant ? 0f : Duration, true);
        if (is_instant)
        { Knob.localPosition = _IsOn ? knobPos1 : knobPos0; }
        else
        {
            _TweenKnob = Knob.DOLocalMove(_IsOn ? knobPos1 : knobPos0, Duration).OnComplete(() => { _TweenKnob = null; });
            _TweenKnob.SetAutoKill();
            _TweenKnob.SetEase(CV_Knob);
        }
    }

}
