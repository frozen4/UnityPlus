using UnityEngine;
using System;
public class GMotionScaleModel : GMotionModel
{

    //scale
    public float _Scale; 
    public Vector3 GetScale(Vector3 origin)
    {
        return origin * _Scale;
    }
    public override MotionType GetMotionType()
    {
        return MotionType.Scale;
    }
}
