using UnityEngine;
using System.Collections;

public class CParabolicMotor : CMotor
{
    //false: stop move
    //true:  continue move
    protected float _Gravity = 0f; //9.8e-6f;
    protected Vector3 _HorzVelocity = Vector3.zero; 
    private float _VertVelocity = 0;
    private float _ElapsedTime = 0;
    //private float _CurveHeight = 0;

    public void SetParams(float fGravity, float curveHeight)
    {
        _Gravity = fGravity;
        //_CurveHeight = curveHeight;
    }

    protected override void OnStart()
    {
        base.OnStart();

        _HorzVelocity = GetDestPos() - _StartPos;
       
        float fVertDis = _HorzVelocity.y;
        _HorzVelocity.y = 0;

        var dist = Vector3.Magnitude(_HorzVelocity);
        _HorzVelocity.Normalize();
        var speed = dist / _LifeTime;
        _HorzVelocity *= speed;
        _VertVelocity = fVertDis / _LifeTime + .5f * _Gravity * _LifeTime;
        _ElapsedTime = 0;
    }

    protected override bool OnMove(float dt)
    {
        if (_IsArrived)
            return false;

        _ElapsedTime += dt;

        if (_ElapsedTime >= _LifeTime)
        {
            _IsArrived = true;
            return false;
        }
        else
        {
            float fVertVel = _VertVelocity - _Gravity * _ElapsedTime;
            float fVertDist = .5f * (fVertVel + _VertVelocity) * _ElapsedTime;
            transform.position = _StartPos + _ElapsedTime * _HorzVelocity + fVertDist * Vector3.up;
            return true;
        }
    }
}