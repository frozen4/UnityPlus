using UnityEngine;
using System.Collections;
using LuaInterface;
using DG.Tweening;
using System.Collections.Generic;
public class CMutantBezierMotor : CMotor
{
	public enum StepState
	{ 
		Rising,
		Wating,
		FlyToTarget,
	}

	private Transform _Trans;
	private StepState _StepState;
	private float _Acceleration = 20f;				//线性加速度
	private float _StartTime = 0f;				    //开始时间
    private float _RealLeftTime = 0f;               //真实剩余时间(排除了上升时间)
    private float _TotalTime = 1f;					//总共的时间（排除了上升时间）
	private float _RasingDest = 0;					//上升高度

	private float _LinearSpeed = 0f;				//当前线速度
	private float _SqrDistance = 0f;				//当前离目标的距离
	private float _MaxSpeed = 0f;					//最大线速度
	private Vector3 _Speed = Vector3.zero;			//当前速度
	private Vector3 _StartSpeed = Vector3.zero;     //初始速度

    public void SetParms(float fAcc)
    {
        _Acceleration = fAcc;
    }

    protected override void OnStart()
	{
 		base.OnStart();
		_Trans = transform;
		_StepState = StepState.Rising;
		_SqrDistance = Util.SquareDistanceH(_StartPos, _DestPos);
		float distance = Mathf.Sqrt(_SqrDistance);
		var hNormalDis = (_DestPos - _StartPos).normalized;
		_LifeTime = Mathf.Sqrt(2 * distance / _Acceleration);
		_MaxSpeed = Mathf.Max(_LifeTime * _Acceleration, 20);
		_Speed = new Vector3(hNormalDis.x, distance * 0.4f / (_LifeTime * 0.5f), hNormalDis.z);
		_StartSpeed = _Speed;
		_LinearSpeed = _Speed.magnitude;
		_RealLeftTime = _LifeTime;
		_TotalTime = _LifeTime;
		_RasingDest = _StartPos.y + 2;
	    _StartTime = Time.time;
	}

	protected override bool OnMove(float dt)
	{
	    bool arrived = false;
        if (_StepState == StepState.Rising)
		{
            Vector3 position = _Trans.position;
            _Trans.position = new Vector3(position.x, position.y + 5 * Time.deltaTime, position.z);
			if (_Trans.position.y > _RasingDest)
				_StepState = StepState.Wating;
		}
		else if (_StepState == StepState.Wating)
		{
            if(Time.time - _StartTime > 0.5f)
                _StepState = StepState.FlyToTarget;
        }
		else if (_StepState == StepState.FlyToTarget)
		{
			_RealLeftTime -= Time.deltaTime;
			_RealLeftTime = Mathf.Max(0, _RealLeftTime);
			
			_LinearSpeed += _Acceleration * Time.deltaTime;
			_LinearSpeed = Mathf.Min(_LinearSpeed, _MaxSpeed);
			if (_DestTrans != null)
			{
				_DestPos = _DestTrans.position;
			}
			Vector3 vec = _DestPos - _Trans.position;
			_Speed = Vector3.Lerp(_StartSpeed, vec.normalized * _LinearSpeed, 1 - (_RealLeftTime / _TotalTime));
			if (IsPassOverTargetPos(_Trans.position, _DestPos, _Speed, _LinearSpeed) && 1 - (_RealLeftTime / _TotalTime) > 0.5f)
			{
				_Trans.position = _DestPos;
				arrived = true;
			}
			else
			{
				_Trans.position += _Speed * Time.deltaTime;
				_SqrDistance = Util.SquareDistanceH(_Trans.position, _DestPos);

				if (_SqrDistance < _Tolerance * _Tolerance)
				{
					arrived = true;
				}
			}

		}
	    return !arrived;
    }

	private bool IsPassOverTargetPos(Vector3 nowPos, Vector3 desPos, Vector3 dir, float speed)
	{
		Vector3 nextFramePos = PreCalNextFramePosition(nowPos, dir, speed);
		Vector3 desToNextPosDir = desPos - nextFramePos;
		return Vector3.Dot(dir, desToNextPosDir) < 0;
	}

	private Vector3 PreCalNextFramePosition(Vector3 nowPos, Vector3 dir, float speed)
	{
		return nowPos + dir.normalized * speed * Time.deltaTime;
	}
}