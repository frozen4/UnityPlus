using UnityEngine;
using LuaInterface;

public class TurnBehavior : Behavior
{
    private Vector3 _StartDir;
    private Vector3 _DestDir;
    private float _TotalTime = 0;
    private float _TimePassed = 0;

    #region Continued Turn 
    private bool _Continue = false;
    private float _ContinueAngle = 0.0f;
    private int _StepTime = 0;
    private float _LastAngle = 0.0f;
    private bool _TurnDirect = true; // true 顺时针  false 逆时针
    Quaternion _TargetRotation;
    Quaternion _StartRotation;
    private float _TimeStepPassed = 0;
    #endregion
    public TurnBehavior()
        : base(BehaviorType.Turn)
    {
    }

    public void SetData(Vector3 dir, float speed, LuaFunction cb, bool continued = false, float continued_angle = 0)
    {
        if (speed < 0.00001f) speed = 300f;

        dir.y = 0;
        bool bIsValidDir = Util.IsValidDir(ref dir);

        Vector3 forward = _Owner.forward;

        _StartDir = forward;
        _StartDir.y = 0;
        
        _Continue = continued;

        if (!_Continue)
        {
            if (bIsValidDir)
            {
                _DestDir = dir;
                _TotalTime = Vector3.Angle(_StartDir, _DestDir) / speed;
            }
            else
            {
                _DestDir = _StartDir;
                _TotalTime = 0;
            }
        }
        else   //continue
        {
            Quaternion rotation = _Owner.rotation;

            _StepTime = Mathf.Abs((int)(continued_angle / 60));
            _LastAngle = continued_angle % 60;
            _TurnDirect = (continued_angle > 0.0001f);
            _ContinueAngle = continued_angle;
            _TargetRotation = rotation * Quaternion.Euler(0, _LastAngle, 0);
            _StartRotation = rotation;
            _TotalTime = Mathf.Abs(continued_angle / speed);
            _DestDir = Quaternion.Euler(0, continued_angle, 0) * forward;
        }
        _TimeStepPassed = 0;
        _TimePassed = 0;

        if (OnFinishCallbackRef != null)
            OnFinishCallbackRef.Release();
        OnFinishCallbackRef = cb;
    }

    public override bool Tick(float dt)
    {
        if (_Owner == null)
            return true;

        _TimePassed += dt;

        if (_TimePassed > _TotalTime)
        {
            _Owner.forward = _DestDir;
            RealOnFinish(BEHAVIOR_RETCODE.Success, Vector3.zero);
            return true;
        }

        if (!_Continue)
        {
            var dir = Vector3.Slerp(_StartDir, _DestDir, _TimePassed / _TotalTime);
            _Owner.forward = dir;
        }
        else
        {
            Quaternion rotation = _Owner.rotation;
            Vector3 ownerEulerAngles = rotation.eulerAngles;
            Vector3 targetEulerAngles = _TargetRotation.eulerAngles;

            _TimeStepPassed += dt;
            var condition = Mathf.Abs(ownerEulerAngles.x - targetEulerAngles.x) < 0.0001f &&
                Mathf.Abs(ownerEulerAngles.y - targetEulerAngles.y) < 0.0001f &&
                Mathf.Abs(ownerEulerAngles.z - targetEulerAngles.z) < 0.0001f;
            if (condition && _StepTime > 0)
            {
                _StepTime -= 1;
                _StartRotation = rotation;
                if (_TurnDirect)
                {
                    _TargetRotation = rotation * Quaternion.Euler(0, 60, 0);
                    _LastAngle = 60;
                }
                else
                {
                    _TargetRotation = rotation * Quaternion.Euler(0, -60, 0);
                    _LastAngle = -60;
                }

                _TimeStepPassed = 0;
                return false;
            }
            Owner.rotation = Quaternion.Slerp(_StartRotation, _TargetRotation, _TimeStepPassed / (_LastAngle / _ContinueAngle * _TotalTime));
        }
        return false;
    }
}
