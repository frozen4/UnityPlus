using UnityEngine;
using LuaInterface;
using EntityComponent;
using Common;
using UnityEngine.UI;

public class DashBehavior : Behavior
{
    private Vector3 _Destination = Vector3.zero;
    private float _Speed = 0;
    private Vector3 _Direction = Vector3.forward;
    //private Vector3 _DestDir = Vector3.forward;
    private float _FinishedTime = 0;
    private bool _CanPierce = false;
    private bool _DashContinue = false;
    private bool _CanChangeDir = false;
    private bool _OnlyCollideWithSkillTarget = false;
    private int _BlockedEnemyId = 0;

    private Vector3 _LerpDestDir = Vector3.zero;

    private static float LastSyncTimestamp = 0;

    public DashBehavior()
        : base(BehaviorType.Dash)
    {
    }

    public void SetData(Vector3 dest, float time, LuaFunction cbRef, bool canPierce, bool goOnAfterKill, bool onlyCollideWithSkillTarget = false, bool canChangeDir = false)
    {
        if (time <= 0)
        {
            HobaDebuger.Log("Move time can not be 0 !!!!");
            return;
        }

        var offset = dest - _Owner.position;
        offset.y = 0;
        _Speed = offset.magnitude / time;

        if (canChangeDir)
        {
            _Direction = _Owner.forward;
            _LerpDestDir = _Direction;
            var maxDis = _Speed * time;
            var dis = Util.GetMaxValidDistance(_Owner.position, _Direction, maxDis);
            _Destination = _Owner.position + _Direction * dis;
        }
        else
        {
            _Destination = dest;
            _Direction = offset.normalized;
            _LerpDestDir = Vector3.zero;
        }

        _FinishedTime = Time.time + time;
        OnFinishCallbackRef = cbRef;
        _CanPierce = canPierce;
        _DashContinue = goOnAfterKill;
        _OnlyCollideWithSkillTarget = onlyCollideWithSkillTarget;
        _CanChangeDir = canChangeDir;

        if (null == _ObjectComponent)
            _ObjectComponent = _Owner.GetComponent<ObjectBehaviour>();

        LastSyncTimestamp = 0;
    }

    public override bool Tick(float dt)
    {
        if (_FinishedTime <= Time.time)
        {
            //Debug.LogFormat("Dash End {0} @{1}", _Owner.position, Time.time);
            RealOnFinish(BEHAVIOR_RETCODE.Success, _Direction);
            return true;
        }

        // 如果是主角，客户端计算，通知服务器；非主角，完全听服务器的
        if (_OwnerType == ObjectBehaviour.OBJ_TYPE.HOSTPLAYER)
        {
            #region 摇杆控制方向
            if (_CanChangeDir)
            {
                Joystick joystick = Joystick.Instance;
                if (joystick != null && !joystick.MoveDir.IsZero())
                {
                    var destDir = joystick.MoveDir;
                    var resetDir = _LerpDestDir.IsZero();
                    if (!resetDir)
                    {
                        var angle0 = Vector3.Angle(_LerpDestDir, destDir);
                        if (Vector3.Dot(Vector3.up, Vector3.Cross(_LerpDestDir, destDir)) < 0)
                            angle0 = 180 - angle0;

                        resetDir = angle0 > 10;
                    }

                    if (resetDir)
                        _LerpDestDir = destDir;
                }

                var newDir = Vector3.Slerp(_Direction, _LerpDestDir, RotationLerpFactor);
                _Direction = newDir.normalized;
                _Owner.forward = _Direction;

                if (_BlockedEnemyId == 0 && (Time.time - LastSyncTimestamp > 0.1f))
                {
                    var maxDis = _Speed * (_FinishedTime - Time.time);
                    var dis = Util.GetMaxValidDistance(_Owner.position, _Direction, maxDis);
                    _Destination = _Owner.position + _Direction * dis;
                    // 需要重新同步
                    _ObjectComponent.SyncHostDashInfo(_Owner.position, _Direction, _Destination);
                    LastSyncTimestamp = Time.time;
                }
            }
            #endregion

            Vector3 castpos = _Owner.position;
            castpos.y += 0.6f;      //拔高0.6米进行前方向trace   
            RaycastHit hitInfo;

            var raycastStep = _Speed * dt;
            if (raycastStep < 1) raycastStep = 1;

            #region 检查Building和obstacle碰撞
            if (CUnityUtil.RayCastWithRadius(0, castpos, _Direction, out hitInfo, raycastStep, CUnityUtil.LayerMaskMovementObstacle))
            {
                if (_CanChangeDir)
                {
                    return false;
                }
                else
                {
                    RealOnFinish(BEHAVIOR_RETCODE.Blocked, _Direction);
                    //主角需要通知服务器停住位置
                    _ObjectComponent.SyncHostDashCollideInfo(0, false);
                    return true;
                }
            }
            #endregion

            var collideWithEntity = CUnityUtil.RayCastWithRadius(Main.HostPlayerRadius, castpos, _Direction,
                out hitInfo, raycastStep, CUnityUtil.LayerMaskEntity);
            #region Entity碰撞
            if (collideWithEntity)
            {
                var collideObj = hitInfo.collider.gameObject.GetComponentInParent<ObjectBehaviour>();
                if (collideObj != null)
                {
                    var collideObjId = collideObj.ID32Bit;

                    if (_CanPierce)
                    {
                        // 技能移动不穿越大型怪物 范导新需求  added by zhouhenan
                        // 20190112 - 增加穿透目标前提，解决穿到怪物肚子中的问题  added by lijian
                        if (_ObjectComponent.OnCollidingHuge(collideObjId))
                        {
                            RealOnFinish(BEHAVIOR_RETCODE.Blocked, _Direction);
                            _ObjectComponent.SyncHostDashCollideInfo(0, false);
                            return true;
                        }
                    }
                    else
                    {
                        var collideEntityType = ObjectBehaviour.CollideEntityType.All;
                        if (_OnlyCollideWithSkillTarget)
                            collideEntityType = ObjectBehaviour.CollideEntityType.OnlyTarget;
                        else if (_CanChangeDir)
                            collideEntityType = ObjectBehaviour.CollideEntityType.Enemy;

                        if (_DashContinue)
                        {
                            // 被同一只怪挡住，原地冲
                            if (_BlockedEnemyId > 0 && _BlockedEnemyId == collideObjId)
                                return false;

                            // 确认是否被目标挡住 如果被挡，需要同步服务器
                            if (_ObjectComponent.OnCollideWithOther(collideObjId, collideEntityType))
                            {
                                _BlockedEnemyId = collideObjId;
                                _ObjectComponent.SyncHostDashCollideInfo(collideObjId, false);
                                return false;
                            }
                            else if(_BlockedEnemyId > 0)
                            {
                                var maxDis = (_FinishedTime - Time.time) * _Speed;
                                var dis = Util.GetMaxValidDistance(_Owner.position, _Direction, maxDis);
                                var dst = _Owner.position + dis * _Direction;
                                _Destination = dst;
                                _ObjectComponent.SyncHostDashCollideInfo(_BlockedEnemyId, true);
                                _BlockedEnemyId = 0;
                            }
                        }
                        else
                        {
                            if (_ObjectComponent.OnCollideWithOther(collideObjId, collideEntityType))
                            {
                                _ObjectComponent.SyncHostDashCollideInfo(collideObjId, false);
                                RealOnFinish(BEHAVIOR_RETCODE.Blocked, _Direction);
                                return true;
                            }
                        }
                    }
                }
            }
            #endregion
            else
            {
                if(_BlockedEnemyId > 0)
                {
                    var maxDis = (_FinishedTime - Time.time) * _Speed;
                    var dis = Util.GetMaxValidDistance(_Owner.position, _Direction, maxDis);
                    var dst = _Owner.position + dis * _Direction;
                    _Destination = dst;
                    _ObjectComponent.SyncHostDashCollideInfo(_BlockedEnemyId, true);
                    _BlockedEnemyId = 0;
                }
            }
        }
        var pos = _Owner.position + (_Speed * _Direction * dt);
        pos.y = CUnityUtil.GetMapHeight(pos) + _ObjectComponent.OffsetY;

        if (_OwnerType == ObjectBehaviour.OBJ_TYPE.HOSTPLAYER)
        {
            if (!IsValidPositionStrict(pos) || !CUnityUtil.IsHeightAccept(pos, _Owner.position))
            {
                if (_CanChangeDir)
                {
                    return false;
                }
                else
                {
                    RealOnFinish(BEHAVIOR_RETCODE.Blocked, _Direction);
                    //主角需要通知服务器停住位置
                    _ObjectComponent.SyncHostDashCollideInfo(0, true);
                    return true;
                }
            }
        }

        _Owner.position = pos;

        return false;
    }

    public override void OnRemove()
    {
        if (_OwnerType == ObjectBehaviour.OBJ_TYPE.HOSTPLAYER && _FinishedTime <= Time.time)
        {
            // 被异常打断，需要同步给Server
            _ObjectComponent.SyncHostDashCollideInfo(0, true);
        }

        _FinishedTime = 0;
    }
}
