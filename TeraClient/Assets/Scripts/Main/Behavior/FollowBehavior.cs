using System;
using UnityEngine;
using LuaInterface;
using EntityComponent;

public class FollowBehavior : Behavior
{
    private const int MAX_AVOID_FORCE = 4;

    private float _MoveSpeed = 2;
    private Transform _TargetTrans;
    private float _MaxDistanceSqr = 0f;
    private float _MinDistanceSqr = 0f;
    private bool _IsLoop = false;
    private int _BlockCount = 0;

    public float MoveSpeed
    {
        get { return _MoveSpeed; }
    }

    public FollowBehavior()
        : base(BehaviorType.Follow)
    {
    }

    public void SetData(GameObject target, float speed, float max_dis, float min_dis, bool once, LuaFunction cb_ref)
    {
        _TargetTrans = target.transform;
        _MoveSpeed = speed;
        _MaxDistanceSqr = max_dis * max_dis;
        _MinDistanceSqr = min_dis * min_dis;
        _IsLoop = !once;
        OnFinishCallbackRef = cb_ref;

        if (null == _ObjectComponent)
            _ObjectComponent = _Owner.GetComponent<ObjectBehaviour>();
    }

    public override bool Tick(float dt)
    {
        if (_OwnerType == ObjectBehaviour.OBJ_TYPE.HOSTPLAYER)
            return TickHostPlayer3(dt);
        else
            return TickOtherEntity(dt);
    }
    
    bool TickHostPlayer3(float dt)
    {
        if (_TargetTrans == null)
        {
            RealOnFinish(BEHAVIOR_RETCODE.Failed, _Owner.forward);
            return true;
        }

        Vector3 curPos = _Owner.position;
        Vector3 targetPos = _TargetTrans.position;
        float distanceSqr = Util.SquareDistanceH(curPos, targetPos);

        Vector3 dir = (targetPos - curPos);
        dir.y = 0;
        Vector3 moveDir = dir.normalized;
        if (distanceSqr < _MaxDistanceSqr && distanceSqr > _MinDistanceSqr)
        {
            if (!_IsLoop)
            {
                RealOnFinish(BEHAVIOR_RETCODE.Success, moveDir);
                return true;
            }
            else
            {
                return false;
            }
        }

        curPos.y += 0.6f;
        RaycastHit hitInfo;
        Vector3 nextPos;

        //检查obstacle碰撞，中断
        if (CUnityUtil.RayCastWithRadius(0, curPos, moveDir, out hitInfo, CUnityUtil.TraceForwardDistance, CUnityUtil.LayerMaskMovementObstacle))
        {
            RealOnFinish(BEHAVIOR_RETCODE.Blocked, moveDir);
            return true;
        }

        if (CUnityUtil.RayCastWithRadius(Main.HostPlayerRadius, curPos, moveDir, out hitInfo, CUnityUtil.TraceForwardDistance, CUnityUtil.LayerMaskNPC))
        {
            Vector3 ahead = _Owner.position + _Owner.forward * 1.5f;
            Vector3 avoidanceForce = ahead - hitInfo.collider.transform.position;
            avoidanceForce = avoidanceForce.normalized * MAX_AVOID_FORCE;
            Vector3 steering = (moveDir * _MoveSpeed + avoidanceForce).normalized;

            nextPos = _Owner.position + _MoveSpeed * dt * steering;
            if (!IsValidPositionStrict(nextPos))
            {
                RealOnFinish(BEHAVIOR_RETCODE.Blocked, steering);
                return true;
            }

            dir = steering;
            dir.y = 0;
            Vector3 vNormalDir;
            if (!Util.IsValidDir(ref dir))
                dir = _Owner.forward;

            nextPos.y = CUnityUtil.GetMapHeight(nextPos) + _ObjectComponent.OffsetY;
            _Owner.position = nextPos;

            SyncMoveStamp(dir, false, true, targetPos);
            return false;
        }

        //判断这次移动的距离是否超过剩余距离，停止
        float fDistMove = _MoveSpeed * dt;
        if (fDistMove * fDistMove >= distanceSqr)
        {
            targetPos.y = CUnityUtil.GetMapHeight(targetPos) + _ObjectComponent.OffsetY;
            _Owner.position = targetPos;
            var facedir = targetPos - curPos;
            facedir.y = 0;
            if (!Util.IsValidDir(ref facedir))
                facedir = _Owner.forward;

            RealOnFinish(BEHAVIOR_RETCODE.Success, facedir);
            return true;
        }

        nextPos = curPos + _MoveSpeed * dt * moveDir;
        nextPos.y = CUnityUtil.GetMapHeight(nextPos) + _ObjectComponent.OffsetY;
        Vector3 nearestPos = new Vector3();
        if (NavMeshManager.Instance.GetNearestValidPosition(nextPos, ref nearestPos, 1.0f))
        {
            Vector3 vDelta = nextPos - nearestPos;
            vDelta.y = 0;
            float fLen = vDelta.magnitude;
            if (fLen < 0.01f)       //navmesh寻路点
            {
                nextPos = nearestPos;
            }
            else
            {
                float fv = Math.Min(fLen, NavMeshManager.TRACE_EXTEND_DISTANCE);        //最大可以跨出navmesh范围
                vDelta.Normalize();
                nextPos = nearestPos + vDelta * fv;
            }

            Vector3 v = nextPos - curPos;
            v.y = 0;
            if (v.sqrMagnitude <= 0.0004f && _MoveSpeed * dt > 0.02f)            //停止不动且和building碰撞, block
            {
                ++_BlockCount;

                if (_BlockCount > 3)
                {
                    _BlockCount = 0;
                    RealOnFinish(BEHAVIOR_RETCODE.Blocked, moveDir);
                    return true;
                }
            }
            else
            {
                _BlockCount = 0;
            }

        }
        else
        {
            _BlockCount = 0;
            RealOnFinish(BEHAVIOR_RETCODE.Blocked, moveDir);
            return true;
        }

        nextPos.y = CUnityUtil.GetMapHeight(nextPos) + _ObjectComponent.OffsetY;
        _Owner.position = nextPos;

        TickAdjustDir(moveDir);

        SyncMoveStamp(moveDir, false, false, targetPos);
        return false;
    }

    public bool TickOtherEntity(float dt)
    {
        if (_TargetTrans == null)
        {
            RealOnFinish(BEHAVIOR_RETCODE.Failed, _Owner.forward);
            return true;
        }

        Vector3 curPos = _Owner.position;
        Vector3 targetPos = _TargetTrans.position;
        float distanceSqr = Util.SquareDistanceH(curPos, targetPos);

        //计算方向，移动
        Vector3 desired = targetPos - _Owner.position;
        desired.y = 0;

        if (!Util.IsValidDir(ref desired))
            desired = _Owner.forward;

        if (distanceSqr < _MaxDistanceSqr && distanceSqr > _MinDistanceSqr)
        {
            if (!_IsLoop)
            {
                RealOnFinish(BEHAVIOR_RETCODE.Success, desired);
                return true;
            }
            else
            {
                return false;
            }
        }

        Vector3 pos = _Owner.position + _MoveSpeed * dt * desired;
        if (!_ObjectComponent.HasTurnBehavior())              //如果同时有转向，则不设置方向
        {
            TickAdjustDir(desired);  // desired
        }

        pos.y = CUnityUtil.GetMapHeight(pos) + _ObjectComponent.OffsetY;
        _Owner.position = pos;
        return false;
    }

    /*
    public override void OnRemove()
    {
        var dir = _HostTransformWrap.GetTransform().forward;
        RealOnFinish(BEHAVIOR_RETCODE.Failed, dir);
    }
    */
}
