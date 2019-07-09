using System;
public class GMotionAlphaModel : GMotionModel
{
    public float _Alpha;

    public override MotionType GetMotionType()
    {
        return MotionType.Alpha;
    }
}