using UnityEngine;
using System.Collections.Generic;
using System;

public class GMotionParaCurveModel : GMotionModel 
{
    public float _StartX;//配置时假设抛物线的顶点为 （0，0）点，起点（_StartX）和终点(_EndX)分别对应二维坐标系的x轴的最小值和最大值。
    public float _EndX;
    public int _Density;//运动轨迹的密度，控制精确度。
    public float _K;//-开口向下，+开口向上。绝对值越大开口越小，反之亦然。
    public Vector2 _RandomRange = Vector2.zero;
    public GMotionCurveModel.CurveDirection _Direction = GMotionCurveModel.CurveDirection.Right;

    public Vector3[] GetParaCurvePath(Vector3 origin)
    {
        float randomx = _StartX + UnityEngine.Random.Range(_RandomRange.x, _RandomRange.y);
        Vector3 offset = new Vector3(randomx, _K * Mathf.Abs(randomx) * Mathf.Abs(randomx));
        float lenth = _EndX - randomx;
        float unit = lenth / _Density;
        float xx;
        float yy;
        Vector3[] path = new Vector3[_Density + 1];
        path[0] = new Vector3(origin.x + offset.x, origin.y + offset.y);
        for (int num = 1; num <= _Density; num++) 
        {
            xx = num * unit;
            yy = _K * Mathf.Abs(xx + offset.x) * Mathf.Abs(xx + offset.x) - offset.y;
            xx = _Direction == GMotionCurveModel.CurveDirection.Right ? xx : -xx;
            path[num].Set(origin.x + xx, origin.y + yy, origin.z);
        }
        return path;
    }
    public override MotionType GetMotionType()
    {
        return MotionType.ParaCurve;
    }
}
