using System;
using UnityEngine;
using LuaInterface;

public class JumpBehavior : Behavior
{
    private Vector3 _HoriVelocity = Vector3.zero;
    private float _PeakHeight = 0;
    private float _OrigHeight = 0;
    private float _DestHeight = 0;
    private float _UpwardAcc = 0;
    private float _DownwardAcc = 0;
    private float _OrigVertVelocity = 0;

    public enum JUMP_PHASE
    {
        JP_INVALID = -1,
        JP_UPWARDS = 0,
        JP_DOWNWARDS,
        JP_END,
    }
    private JUMP_PHASE _CurPhase = JUMP_PHASE.JP_INVALID;
    
    float _ElapseTime = 0;

    public JumpBehavior()
        : base(BehaviorType.Jump)
    {
    }

    public void SetData(Vector3 dest, float height, float duration, float upward_ratio, LuaFunction cbref)
    {
        if (duration <= 0 || upward_ratio <= 0 || upward_ratio >= 1) return;

        _HoriVelocity = (dest - _Owner.position) / duration;
        _HoriVelocity.y = 0;

        _OrigHeight = _Owner.position.y;
        _DestHeight = dest.y;
        _PeakHeight = _OrigHeight + height;
        float upward_time = duration * upward_ratio;
        _UpwardAcc = 2 * height / (upward_time * upward_time);
        _OrigVertVelocity = 2 * height / upward_time;
        float downward_time = duration * (1 - upward_ratio);
        _DownwardAcc = 2 * height / (downward_time * downward_time);

        OnFinishCallbackRef = cbref;

        _CurPhase = JUMP_PHASE.JP_UPWARDS;
        _ElapseTime = 0;
    }

    public override bool Tick(float dt)
    {
        Vector3 delta = _HoriVelocity * dt;
        Vector3 cur_pos = _Owner.position + delta;

        _ElapseTime += dt;
        if (_CurPhase == JUMP_PHASE.JP_UPWARDS)
        {
            float h = _OrigVertVelocity * _ElapseTime - 0.5f * _UpwardAcc * _ElapseTime * _ElapseTime;
            if(_OrigVertVelocity -  _UpwardAcc * _ElapseTime > 0)
                cur_pos.y = h;
            else
            {
                cur_pos.y = _PeakHeight;
                _CurPhase = JUMP_PHASE.JP_DOWNWARDS;
                _ElapseTime = 0;
            }
        }
        else if (_CurPhase == JUMP_PHASE.JP_DOWNWARDS)
        {
            float h = _PeakHeight - 0.5f * _DownwardAcc * _ElapseTime * _ElapseTime;
            if(h > _DestHeight)
                cur_pos.y = h;
            else
            {
                cur_pos.y = _DestHeight;
                _CurPhase = JUMP_PHASE.JP_END;
            }
        }

        return _CurPhase == JUMP_PHASE.JP_END;
    }
}
