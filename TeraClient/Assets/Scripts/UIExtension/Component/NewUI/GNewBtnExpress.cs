using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class GNewBtnExpress : GNewUIBase
{
    //const float ColorFadeTime = 0f;

    Selectable selectable;
    enum Type
    {
        None,
        Toggle,
        Button,
    }
    Type btnType=Type.None;

    [System.Serializable]
    public struct ChangeColorItem
    {
        public Graphic graphic;
        public Color color1;
        public Color color2;
    }
    public ChangeColorItem[] ChangeColors;

    [System.Serializable]
    public class BoolCall : UnityEvent { }

    public BoolCall CallEvent;
    public BoolCall CallEventNot;

    public void OnValueChanged(bool v)
    {
        _ChnageValue(v, false);
    }

    void _ChnageValue(bool v, bool is_instant)
    {
        if (v)
        {
            if (CallEvent != null) CallEvent.Invoke();
        }
        else
        {
            if (CallEventNot != null) CallEventNot.Invoke();
        }

        if (ChangeColors != null)
        {
            for (int i = 0; i < ChangeColors.Length; i++)
            {
                if (ChangeColors[i].graphic)
                {
                    ChangeColors[i].graphic.color = (v ? ChangeColors[i].color1 : ChangeColors[i].color2);
                    //ChangeColors[i].graphic.CrossFadeColor((v ? ChangeColors[i].color1 : ChangeColors[i].color2), is_instant?0:ColorFadeTime, true, true);
                }
            }
        }
    }

    public void RegisterToToggle(Toggle tg)
    {
        if (tg)
        {
            btnType = Type.Toggle;
            selectable = tg;

            tg.onValueChanged.AddListener(OnValueChanged);
            _ChnageValue(tg.isOn, true);
        }
    }

    protected override void OnSafeInit()
    {
#if! IN_GAME
        base.OnSafeInit();
        selectable = GetComponent<Selectable>();
        Toggle tg = selectable as Toggle;
        if (tg)
        {
            btnType = Type.Toggle;

            tg.onValueChanged.AddListener(OnValueChanged);
        }

        if (btnType == Type.None)
        {
            _ChnageValue(false, true);
        }
#endif
    }

    protected override void OnEnable()
    {
        if (btnType == Type.Toggle)
        {
            Toggle tg = selectable as Toggle;
            if (tg)
                _ChnageValue(tg.isOn, true);
        }

    }
}