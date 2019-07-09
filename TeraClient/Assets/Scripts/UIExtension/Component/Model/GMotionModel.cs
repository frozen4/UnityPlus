using System;
using UnityEngine;
using DG.Tweening;
public abstract class GMotionModel : ScriptableObject
{
    public enum MotionType
    {
        Alpha = 0,
        Linear,
        Scale,
        Curve,
        ParaCurve
    };

    public float _Duration;
    public float _Delay = 0f;
    public Ease _EaseType = Ease.Linear;

    public abstract MotionType GetMotionType();
}