using UnityEngine;
using LuaInterface;

public class CTargetMotor : CMotor
{
    //false: stop move
    //true:  continue move

    protected float _Acc = 0.0f;
    protected Vector3 _Next = Vector3.zero;
    protected bool _StopWhenArrived = false;

    public void SetParms(float fAcc, bool stopWhenArrived)
    {
        if(fAcc > 0)
            _Acc = fAcc;
        _StopWhenArrived = stopWhenArrived;
    }

    protected override bool OnMove(float dt)
    {
        var destPos = GetDestPos();
        var offset = destPos - transform.position;

        float distance = offset.magnitude;
        float step = dt * _HorzSpeed;

        if (distance < 0.001f)
            offset = Vector3.zero;
        else
            offset = offset / distance;     //normalized

        bool arrived = false;
        if (Mathf.Abs(distance - step) < _Tolerance)
        {
            if(_StopWhenArrived)
                arrived = true;
            transform.position = destPos;
        }
        else
        {
            if (_Acc > 0.0f)
            {
                float fDistance = (_HorzSpeed + 0.5f * _Acc * dt) * dt;
                _Next = transform.position + offset * fDistance;
            }
            else
            {
                _Next = transform.position + offset * _HorzSpeed * dt;
            }

            //已越界
           if (Util.SquareDistanceH(transform.position, _Next) > Util.SquareDistanceH(transform.position, destPos))
            {
                if(_StopWhenArrived)
                    arrived = true;
                transform.position = destPos;
            }
           else
            {
                transform.position = _Next;
            }

        }

        Vector3 up = Vector3.up;
        if (up != offset)
        {
            Vector3 right = Vector3.Cross(up, offset);
            up = Vector3.Cross(offset, right);
        }
        transform.LookAt(destPos, up);

        return !arrived;
    }
}