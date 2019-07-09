using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

//[DisallowMultipleComponent]
public class GTweenUIFill : GTweenUIBase
{
    private Image _target;

    public GTweenUIFill()
    {
        TweenList = new TweenInfo[1];
        TweenList[0] = new TweenInfo();
    }

    protected override void OnStart()
    {
        if (_target == null)
        {
            _target = GetComponent<Image>();
        }
    }

    protected override void DoTween(float normalized_time)
    {
        float val_a = 1;

        TweenInfo ti = TweenList[0];
        if (ti != null && ti.IsEnabled)
        {
            val_a = InterpolateValue(normalized_time, ti);
        }

        if (_target != null)
        {
            _target.fillAmount = val_a;
        }
    }

#if UNITY_EDITOR
    [NoToLua]
    public override string ItemName4Editor(int idx)
    {
        switch (idx)
        {
            case 0:
                return "Fill";
        }
        return string.Empty;
    }
#endif
}




