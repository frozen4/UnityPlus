using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[DisallowMultipleComponent]
public class GTweenUIScale : GTweenUIBase
{
    public GTweenUIScale()
    {
        TweenList = new TweenInfo[2];
        for (int i = 0; i < TweenList.Length; i++)
        {
            TweenList[i] = new TweenInfo();
        }
    }

    protected override void DoTween(float normalized_time)
    {
        Vector3 scale = RectTrans.localScale;

        TweenInfo ti = TweenList[0];
        if (ti != null)
        {
            scale.x = InterpolateValue(normalized_time, ti);
        }

        ti = TweenList[1];
        if (ti != null && ti.IsEnabled)
        {
            scale.y = InterpolateValue(normalized_time, ti);
        }

        RectTrans.localScale = scale;
    }

#if UNITY_EDITOR
    [NoToLua]
    public override string ItemName4Editor(int idx)
    {
        switch (idx)
        {
            case 0:
                return "Scale X";
            case 1:
                return "Scale Y";
        }
        return string.Empty;
    }
#endif

}




