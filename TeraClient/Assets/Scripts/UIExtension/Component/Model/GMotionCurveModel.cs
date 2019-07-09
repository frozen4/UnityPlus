using UnityEngine;
using System.Collections.Generic;
using System;

public class GMotionCurveModel : GMotionModel
{
    public enum CurveDirection
    {
        Left,
        Right
    }
    public CurveDirection _FloatingDirection = CurveDirection.Right;
    public float _Radius;
    public int _Density;
    public Vector3[] GetCurvePath(Vector3 pos)
    {
        Vector3[] path = new Vector3[_Density + 1];
        float piece = Mathf.PI / _Density;

        for (int i = 0; i <= _Density; i++)
        {
            float offsetx = _Radius - Mathf.Cos(i * piece) * _Radius;
            float offsety = Mathf.Sin(i * piece) * _Radius;
            offsetx = _FloatingDirection == CurveDirection.Right ? offsetx : -offsetx;
            path[i].Set(pos.x + offsetx, pos.y + offsety, pos.z);
        }
        return path;
    }
    public override MotionType GetMotionType()
    {
        return MotionType.Curve;
    }
}
