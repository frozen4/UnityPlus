using System;
using UnityEngine;
using LuaInterface;
using EntityComponent;

public class MoveBehavior : Behavior
{
    private const float NEAREST_DIST = 0.05f;      //距离目标点这个距离内，则认为已到达

    private float _MoveSpeed = 2;
    private Vector3 _TargetPos = Vector3.zero;
    private Vector3 _MoveDir;

    private int _BlockOccurCount = 0;

    //Navigation
    private float _DestOffset = 0.0f;          //距离终点多长时结束
    private bool _IsCollidedWithObjects = false;
    private Vector3 _NavCalcStartPosition = Vector3.zero;
    private int _CollideTryCount = 0;

    public bool IsDirChanged = true;
    public bool IgnoreCollisionDetect = false;           //忽略碰撞

    public MoveBehavior()
        : base(BehaviorType.Move)
    {
    }

    public float MoveSpeed
    {
        get { return _MoveSpeed; }
        set { _MoveSpeed = value; }
    }

    public Vector3 Velocity
    {
        get { return _MoveSpeed * _MoveDir; }
    }

    public Vector3 TargetPos
    {
        get { return _TargetPos; }
    }

    private bool CalcHostPathFindingInfo()
    {
        CHostPathFindingInfo.Instance.Clear();

        Vector3 startPos = _Owner.transform.position;
        Vector3 endPos = _TargetPos;
        const float slop = 0.1f;

        bool bNavigation = CHostPathFindingInfo.Instance.RecalcPathFindFollow(startPos, endPos, slop);
        if (!CHostPathFindingInfo.Instance.IsNavigating())
            bNavigation = false;

        if (bNavigation)
            _NavCalcStartPosition = startPos;

        return bNavigation;
    }

    public void SetData(Vector3 targetPos, float speed, LuaFunction cb, int objID, float fOffset, bool autopathing)
    {
        if (_ObjectComponent == null)
            _ObjectComponent = _Owner.GetComponent<ObjectBehaviour>();

        _TargetPos = targetPos;

        #region 主角自动寻路判断逻辑
        if (_OwnerType == ObjectBehaviour.OBJ_TYPE.HOSTPLAYER)
        {
            if (fOffset > 0.0f && Util.DistanceH(_Owner.transform.position, targetPos) < fOffset)        //检查是否距离过近， 取消寻路，走直线逻辑
            {
                CHostPathFindingInfo.Instance.Clear();
            }
            else
            {
                CalcHostPathFindingInfo();
                SetCameraQuickFollow(autopathing);
            }
        }
        #endregion

        _MoveSpeed = speed;
        _TargetPos.y = CUnityUtil.GetMapHeight(_TargetPos) + _ObjectComponent.OffsetY;  // 只需要在设置目标点时，计算一次高度
        _DestOffset = fOffset;
        if (OnFinishCallbackRef != null)
            OnFinishCallbackRef.Release();
        OnFinishCallbackRef = cb;

        _BlockOccurCount = 0;
    }

    public override bool Tick(float dt)
    {
        if (_OwnerType == ObjectBehaviour.OBJ_TYPE.HOSTPLAYER)
        {
            if(CHostPathFindingInfo.Instance.IsNavigating())
                return TickNavigation(dt);

            return TickHostPlayer3(dt);
        }

        return TickOtherEntity(dt);
    }

    private bool TickNavigation(float dt)
    {
        Vector3 curPos = _Owner.position;

        //offset判断, 如果offset为0，则停在target位置，否则停在当前位置
        float distSqr = Util.SquareDistanceH(curPos, _TargetPos);
        if (_DestOffset > Util.FloatZero && distSqr <= _DestOffset * _DestOffset)
        {
            _DestOffset = 0.0f;
            RealOnFinish(BEHAVIOR_RETCODE.Success, _Owner.forward);
            return true;
        }

        if (distSqr < NEAREST_DIST * NEAREST_DIST)
        {
            Vector3 vPos = _TargetPos;
            vPos.y = CUnityUtil.GetMapHeight(vPos) + _ObjectComponent.OffsetY;
            _Owner.position = vPos;
            _DestOffset = 0.0f;
            RealOnFinish(BEHAVIOR_RETCODE.Success, _Owner.forward);
            return true;
        }

        // 吸附中，需要重新计算路线
        if (_ObjectComponent.HasBehavior(BehaviorType.Adsorb)) 
        {
            if (!CalcHostPathFindingInfo())
            {
                _DestOffset = 0.0f;
                RealOnFinish(BEHAVIOR_RETCODE.Blocked, _MoveDir);
                return true;
            }
        }

        Vector3 vNextPos;
        var step = _MoveSpeed * dt;
        int iPath = 0;
        var bArrive = CHostPathFindingInfo.Instance.GetNextNavPosition(step, ref curPos, ref iPath, out vNextPos);
        vNextPos.y = CUnityUtil.GetMapHeight(vNextPos) + _ObjectComponent.OffsetY;

        var facedir = vNextPos - curPos;
        facedir.y = 0;
        if (!Util.IsValidDir(ref facedir))
            facedir = _Owner.forward;

        //寻路到达判断
        if (bArrive)
        {
            _Owner.position = vNextPos;
            _DestOffset = 0.0f;
            RealOnFinish(BEHAVIOR_RETCODE.Success, facedir);
            return true;
        }
        _MoveDir = facedir;

        curPos.y += 0.6f;
        RaycastHit hitInfo;
        #region Obstacle碰撞检测
        //检查obstacle碰撞，中断
        if (CUnityUtil.RayCastWithRadius(0, curPos, _MoveDir, out hitInfo, CUnityUtil.TraceForwardDistance, CUnityUtil.LayerMaskMovementObstacle))
        {
            _DestOffset = 0.0f;
            RealOnFinish(BEHAVIOR_RETCODE.Blocked, _MoveDir);
            return true;
        }
        #endregion

        if (IgnoreCollisionDetect)           //忽略碰撞
        {
            _Owner.position = vNextPos;

            if (IsDirChanged)
                TickAdjustDir(_MoveDir);

            SyncMoveStamp(_MoveDir, false, false, vNextPos);
            return false;
        }
        else
        {
            bool bLastCollide = _IsCollidedWithObjects;

            #region NPC碰撞检测
            _IsCollidedWithObjects = CUnityUtil.RayCastWithRadius(Main.HostPlayerRadius, curPos, _MoveDir, out hitInfo, CUnityUtil.TraceForwardDistance, CUnityUtil.LayerMaskNPC);

            if (_IsCollidedWithObjects)
            {
                float dist = Util.SquareDistanceH(curPos, _NavCalcStartPosition);
                if (dist < 1.0f)
                    ++_CollideTryCount;
                else
                    _CollideTryCount = 0;

                if (_CollideTryCount >= 1)
                {
                    _IsCollidedWithObjects = false;
                    _CollideTryCount = 0;
                }
            }
            #endregion

            if (_IsCollidedWithObjects)
            {
                Vector3 ahead = _Owner.position + _Owner.forward * 1.5f;
                ahead.y = 0;
                Vector3 avoidanceForce = ahead - hitInfo.collider.transform.position;
                avoidanceForce.y = 0;
                avoidanceForce = avoidanceForce.normalized * MAX_AVOID_FORCE;
                Vector3 steering = (_MoveDir * _MoveSpeed + avoidanceForce).normalized;

                vNextPos = _Owner.position + _MoveSpeed * Math.Max(0.03f, dt) * steering;
                if (!IsValidPositionStrict(vNextPos))                  //尝试不成功则恢复，否则会卡住
                    vNextPos = _Owner.position;
                else
                    vNextPos.y = CUnityUtil.GetMapHeight(vNextPos) + _ObjectComponent.OffsetY;

                _Owner.position = vNextPos;
                if (IsDirChanged)
                    TickAdjustDir(steering);

                SyncMoveStamp(steering, false, true, _TargetPos);
                return false;
            }
            else
            {
                if (bLastCollide)       //由碰撞变为不碰撞
                {
                    if (!CalcHostPathFindingInfo())         //以当前点继续寻路
                    {
                        _DestOffset = 0.0f;
                        RealOnFinish(BEHAVIOR_RETCODE.Blocked, _MoveDir);
                        return true;
                    }

                    return false;
                }
            }

            _Owner.position = vNextPos;
            if (IsDirChanged)
                TickAdjustDir(_MoveDir);

            SyncMoveStamp(_MoveDir, false, false, _TargetPos);

            return false;
        }
    }

    // 算法来源：
    // http://gamedevelopment.tutsplus.com/tutorials/understanding-steering-behaviors-collision-avoidance--gamedev-7777
    private int MAX_AVOID_FORCE = 4;
    private bool TickHostPlayer3(float dt)
    {
        Vector3 curPos = _Owner.position;
        curPos.y += 0.6f;

        var distLeft = Util.DistanceH(curPos, _TargetPos);

        //到达检查, 如果offset为0，则停在target位置，否则停在当前位置
        if (_DestOffset > 0.0f)
        {
            if (distLeft <= _DestOffset)
            {
                _DestOffset = 0.0f;
                RealOnFinish(BEHAVIOR_RETCODE.Success, _Owner.forward);
                return true;
            }
        }
        else if (distLeft < NEAREST_DIST)
        {
            Vector3 vPos = _TargetPos;
            vPos.y = CUnityUtil.GetMapHeight(vPos) + _ObjectComponent.OffsetY;
            _Owner.position = vPos;
            _DestOffset = 0.0f;
            RealOnFinish(BEHAVIOR_RETCODE.Success, _Owner.forward);
            return true;
        }

        Vector3 dir = (_TargetPos - curPos);
        dir.y = 0;
        _MoveDir = dir.normalized;

        var nextPos = Vector3.zero;
        #region 碰撞检测
        RaycastHit hitInfo;
        //检查obstacle碰撞，中断
        if (CUnityUtil.RayCastWithRadius(0, curPos, _MoveDir, out hitInfo, CUnityUtil.TraceForwardDistance, CUnityUtil.LayerMaskMovementObstacle))
        {
            RealOnFinish(BEHAVIOR_RETCODE.Blocked, _MoveDir);
            return true;
        }
        
        if (CUnityUtil.RayCastWithRadius(Main.HostPlayerRadius, curPos, _MoveDir, out hitInfo, CUnityUtil.TraceForwardDistance, CUnityUtil.LayerMaskNPC))
        {
            Vector3 ahead = _Owner.position + _Owner.forward * 1.5f;
            Vector3 avoidanceForce = ahead - hitInfo.collider.transform.position;
            avoidanceForce = avoidanceForce.normalized * MAX_AVOID_FORCE;
            Vector3 steering = (_MoveDir * _MoveSpeed + avoidanceForce).normalized;

            nextPos = _Owner.position + _MoveSpeed * dt * steering;
            nextPos.y = CUnityUtil.GetMapHeight(nextPos) + _ObjectComponent.OffsetY;

            if (!IsValidPositionStrict(nextPos))
            {
                RealOnFinish(BEHAVIOR_RETCODE.Blocked, steering);
                return true;
            }

            steering.y = 0;
            if (!Util.IsValidDir(ref steering))
                steering = _Owner.forward;

            _Owner.position = nextPos;
            if (IsDirChanged)
                TickAdjustDir(steering);

            SyncMoveStamp(dir, false, true, _TargetPos);
            return false;
        }
        #endregion

        #region 步长是否超过剩余距离
        //判断这次移动的距离是否超过剩余距离，停止
        float fDistMove = _MoveSpeed * dt;
        if (fDistMove >= distLeft)
        {
            Vector3 vPos = _TargetPos;
            vPos.y = CUnityUtil.GetMapHeight(vPos) + _ObjectComponent.OffsetY;
            _Owner.position = vPos;
            RealOnFinish(BEHAVIOR_RETCODE.Success, _Owner.forward);
            return true;
        }
        #endregion

        nextPos = curPos + _MoveSpeed * dt * _MoveDir;
        
        #region NextStepPos NavMesh有效性检查
        Vector3 nextValidPos = Vector3.zero;
        if (NavMeshManager.Instance.GetNearestValidPosition(nextPos, ref nextValidPos, 1.0f))
        {
            Vector3 vDelta = nextPos - nextValidPos;
            vDelta.y = 0;
            float fLen = vDelta.magnitude;
            if (fLen < 0.01f)       //navmesh寻路点
            {
                nextPos = nextValidPos;
            }
            else
            {
                float fv = Math.Min(fLen, NavMeshManager.TRACE_EXTEND_DISTANCE);        //最大可以跨出navmesh范围
                vDelta.Normalize();
                nextPos = nextValidPos + vDelta * fv;
            }

            nextPos.y = CUnityUtil.GetMapHeight(nextPos) + _ObjectComponent.OffsetY;

            Vector3 v = nextPos - curPos;
            v.y = 0;
            if (v.sqrMagnitude <= 0.0004f && _MoveSpeed * dt > 0.02f)            //停止不动且和building碰撞, block
            {
                ++_BlockOccurCount;

                if (_BlockOccurCount > 3)
                {
                    _BlockOccurCount = 0;
                    _Owner.position = nextPos;
                    RealOnFinish(BEHAVIOR_RETCODE.Blocked, _MoveDir);
                    return true;
                }
            }
            else
            {
                _BlockOccurCount = 0;
            }
        }
        else
        {
            nextPos.y = CUnityUtil.GetMapHeight(nextPos) + _ObjectComponent.OffsetY;

            _BlockOccurCount = 0;
            RealOnFinish(BEHAVIOR_RETCODE.Blocked, _MoveDir);
            return true;
        }
        #endregion

        _Owner.position = nextPos;

        if (IsDirChanged)
            TickAdjustDir(_MoveDir);

        //避免频繁发送
        SyncMoveStamp(_MoveDir, false, false, _TargetPos);

        return false;
    }

    public bool TickOtherEntity(float dt)
    {
        Vector3 pos = _Owner.position;

        //计算方向，移动
        _MoveDir = _TargetPos - pos;
        _MoveDir.y = 0;

        float fDistLeft = 0;
        if (!Util.IsValidDir(ref _MoveDir, ref fDistLeft))
            _MoveDir = _Owner.forward;

        float fDeltaMove = _MoveSpeed * dt;
        float fMinDist = Mathf.Max(fDeltaMove, NEAREST_DIST);
        //判断这次移动的距离是否超过剩余距离，停止
        if (fDistLeft <= fMinDist)
        {
            Vector3 vPos = _TargetPos;      //到达
            vPos.y = CUnityUtil.GetMapHeight(vPos) + _ObjectComponent.OffsetY;
            _Owner.position = vPos;
            //到达
            RealOnFinish(BEHAVIOR_RETCODE.Success, _MoveDir);
            return true;
        }

        pos = _Owner.position + fDeltaMove * _MoveDir;
        if (Util.IsNaN(pos))
            return false;

        pos.y = CUnityUtil.GetMapHeight(pos) + _ObjectComponent.OffsetY;
        _Owner.position = pos;

        if (!_ObjectComponent.HasTurnBehavior())              //如果同时有转向，则不设置方向
        {
            if (IsDirChanged)
                TickAdjustDir(_MoveDir);
        }

        return false;
    }

    public override void OnRemove()
    {
        if (_OwnerType == ObjectBehaviour.OBJ_TYPE.HOSTPLAYER)
        {
            SetCameraQuickFollow(false);
            CHostPathFindingInfo.Instance.Clear();
        }
    }

    private void SetCameraQuickFollow(bool bQuick)
    {
        CPlayerFollowCam cam = CCamCtrlMan.Instance.GetGameCamCtrl();
        if (cam != null)
            cam.IsQuickFollow = bQuick;
    }

}
