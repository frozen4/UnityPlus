using System;
using UnityEngine;
using UnityEngine.UI;
using EntityComponent;

public class JoyStickBehavior : Behavior
{
    private Joystick _Joystick = null;
    private Vector3 _MoveDir = Vector3.forward;
    private float _MoveSpeed;

    private float _LastTime = 0;

    public float MoveSpeed
    {
        get { return _MoveSpeed; }
    }

    public Vector3 Velocity
    {
        get { return _MoveSpeed * _MoveDir; }
    }

    public bool Need2ChangeDir = true;

    public JoyStickBehavior()
        : base(BehaviorType.JoyStickMove)
    {
    }

    public void SetData(float speed, bool changeDir)
    {
        if (_ObjectComponent == null)
            _ObjectComponent = _Owner.GetComponent<ObjectBehaviour>();

        if (_Joystick == null)
        {
            _Joystick = Joystick.Instance;
            _Joystick.JoystickDragEndCb = OnStop;
        }

        if (_Joystick != null)
        {
            _MoveDir = _Joystick.MoveDir;
            SyncMoveStamp(changeDir ? _MoveDir : _Owner.forward, false, false, _Owner.position);
        }
        _MoveSpeed = speed;
        Need2ChangeDir = changeDir;
    }

    public override bool Tick(float dt)
    {
        Joystick joystick = Joystick.Instance;
        if (joystick == null) return true;

        _MoveDir = joystick.MoveDir;

        bool bValidDir = Util.IsValidDir(ref _MoveDir); 
        if (!bValidDir)
            return false;

        TickAdjustDir(_MoveDir);

        var curDir = _Owner.forward;

        Vector3 pos = _Owner.position;
        pos.y += 0.6f;

        
        #region 碰撞检测
        RaycastHit hitInfo;
        if (CUnityUtil.RayCastWithRadius(0, pos, curDir, out hitInfo, CUnityUtil.TraceForwardDistance, CUnityUtil.LayerMaskMovementObstacle))
        {
            SyncMoveStampWhenNoProgress();
            return false;
        }

        if (CUnityUtil.RayCastWithRadius(Main.HostPlayerRadius, pos, curDir, out hitInfo, CUnityUtil.TraceForwardDistance, CUnityUtil.LayerMaskNPC))
        {
            // 修正运动方向
            Vector3 adjustDir;
            {
                Vector3 hitNormal = hitInfo.normal;
                var v = Vector3.Cross(hitNormal, curDir);
                adjustDir = Vector3.Cross(v, hitNormal);
                adjustDir.y = 0;
                adjustDir.Normalize();
            }

            //TickAdjustDir(adjustDir);
            //	朝调整方向trace，如果还有boxcollider则不再寻找新的方向
            if (CUnityUtil.RayCastWithRadius(Main.HostPlayerRadius, pos, adjustDir, out hitInfo, CUnityUtil.TraceForwardDistance, CUnityUtil.LayerMaskNPC))
            {
                SyncMoveStampWhenNoProgress();
                return false;
            }

            pos = _Owner.position + _MoveSpeed * dt * adjustDir;
            if (!IsValidPositionStrict(pos))
            {
                SyncMoveStampWhenNoProgress();
                return false;
            }

            pos.y = CUnityUtil.GetMapHeight(pos) + _ObjectComponent.OffsetY;
            _Owner.position = pos;
            SyncMoveStampWhenProgress();

            return false;
        }
        #endregion


        float fTestDist = NavMeshManager.TRACE_EXTEND_DISTANCE;
        if (_MoveSpeed * dt > fTestDist)                //需要测试一个比较近的距离
        {
            Vector3 vTemp = _Owner.position + fTestDist * curDir;
            if (!NavMeshManager.Instance.IsValidPositionStrict(vTemp, false, 0.3f))
            {
                SyncMoveStampWhenNoProgress();
                return false; 
            }
        }

        {
            Vector3 vNearest = pos;
            pos = _Owner.position + _MoveSpeed * dt * curDir;
            pos.y = CUnityUtil.GetMapHeight(pos) + _ObjectComponent.OffsetY;
            if (NavMeshManager.Instance.GetNearestValidPosition(pos, ref vNearest))
            {
                //取目标点和最近navmesh点的插值，这样可以沿移动方向在navmesh周边滑动
                Vector3 vDelta = pos - vNearest;
                vDelta.y = 0;
                float fLen = vDelta.magnitude;
                if (fLen < 0.01f)
                {
                    pos = vNearest;
                }
                else
                {
                    float fv = Math.Min(fLen, NavMeshManager.TRACE_EXTEND_DISTANCE);                //最大超出navmesh边界0.2 但也不能被服务器拉回 NavMeshManager.IsValidPosition
                    vDelta.Normalize();
                    pos = vNearest + vDelta * fv;
                }
            }
            else
            {
                SyncMoveStampWhenNoProgress();
                return false;
            }
        }

        if (NavMeshManager.Instance.IsValidPositionStrict(pos, false, NavMeshManager.TRACE_EXTEND_DISTANCE))           //摇杆必须为navmesh有效点，否则会被服务器拉回
        {
            pos.y = CUnityUtil.GetMapHeight(pos) + _ObjectComponent.OffsetY;
            if (!CUnityUtil.IsHeightAccept(pos, _Owner.position))     //高度不合法
            {
                SyncMoveStampWhenNoProgress();
                return false;
            }

            _Owner.position = pos;
            SyncMoveStampWhenProgress();
        }
        else
        {
            SyncMoveStampWhenNoProgress();
        }

        return false;
    }

    private void SyncMoveStampWhenNoProgress()
    {
        if (Time.time - _LastTime > 0.2f)
        {
            var syncDir = Need2ChangeDir ? _MoveDir : _Owner.forward;
            SyncMoveStamp(syncDir, true, true, _Owner.position);
            _LastTime = Time.time;
        }
    }

    private void SyncMoveStampWhenProgress()
    {
        var syncDir = Need2ChangeDir ? _MoveDir : _Owner.forward;
        SyncMoveStamp(syncDir, false, false, _Owner.position);
        _LastTime = 0;
    }

    private void OnStop()
    {
        RealOnFinish(BEHAVIOR_RETCODE.Success, _Owner.forward);
    }
}
