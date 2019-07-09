using UnityEngine;
using System;
using System.Collections;

public class GMotionLinearModel : GMotionModel
{
    public bool _IsOffsetMode = true;
    public Vector3 _Dest = Vector3.zero;
    public Vector3 _Offset = Vector3.zero;
    public Vector3 GetDest(Vector3 pos)
    {
        return _IsOffsetMode ? pos + _Offset : _Dest;
    }
    public override MotionType GetMotionType()
    {
        return MotionType.Linear;
    }
}
