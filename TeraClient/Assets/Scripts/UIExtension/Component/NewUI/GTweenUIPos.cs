using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//[DisallowMultipleComponent]
public class GTweenUIPos : GTweenUIBase
{
    private Vector2 BasePos;

    public GTweenUIPos()
    {
        TweenList = new TweenInfo[2];
        for (int i = 0; i < TweenList.Length; i++)
        {
            TweenList[i] = new TweenInfo();
        }
    }

    protected override void OnStart()
    {
        BasePos = RectTrans.anchoredPosition;
    }

    protected override void DoTween(float normalized_time)
    {
        Vector2 pos = Vector2.zero;

        TweenInfo ti = TweenList[0];
        if (ti != null && ti.IsEnabled)
        {
            pos.x = InterpolateValue(normalized_time, ti);
        }

        ti = TweenList[1];
        if (ti != null && ti.IsEnabled)
        {
            pos.y = InterpolateValue(normalized_time, ti);
        }

        RectTrans.anchoredPosition = pos + BasePos;
    }

#if UNITY_EDITOR
    [NoToLua]
    public override string ItemName4Editor(int idx)
    {
        switch (idx)
        {
            case 0:
                return "Anchored Pos X";
            case 1:
                return "Anchored Pos Y";
        }
        return string.Empty;
    }
#endif
}




