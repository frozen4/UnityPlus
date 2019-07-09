using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//[DisallowMultipleComponent]
public class GTweenUIRot : GTweenUIBase
{
    public GTweenUIRot()
    {
        TweenList = new TweenInfo[3];
        for (int i = 0; i < TweenList.Length; i++)
        {
            TweenList[i] = new TweenInfo();
        }
    }

    protected override void DoTween(float normalized_time)
    {
        Vector3 rot = RectTrans.localRotation.eulerAngles;

        TweenInfo ti = TweenList[0];
        if (ti != null && ti.IsEnabled)
        {
            rot.x = InterpolateValue(normalized_time, ti);
        }

        ti = TweenList[1];
        if (ti != null && ti.IsEnabled)
        {
            rot.y = InterpolateValue(normalized_time, ti);
        }

        ti = TweenList[2];
        if (ti != null && ti.IsEnabled)
        {
            rot.z = InterpolateValue(normalized_time, ti);
        }

        RectTrans.localRotation = Quaternion.Euler(rot);
    }

#if UNITY_EDITOR
    [NoToLua]
    public override string ItemName4Editor(int idx)
    {
        switch (idx)
        {
            case 0:
                return "Along X";
            case 1:
                return "Along Y";
            case 2:
                return "Along Z";
        }
        return string.Empty;
    }
#endif
}




