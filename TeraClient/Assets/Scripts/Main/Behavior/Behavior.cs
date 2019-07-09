using LuaInterface;
using UnityEngine;
using EntityComponent;

public enum BehaviorType
{
    Invalid = -1,
    Move = 0,
    Turn = 1,
    Jump = 2,
    Dash = 3,  // 冲锋
    Follow = 4,  // 跟随
    Adsorb = 5,  // 吸附
    JoyStickMove = 6,

    Count,

    WanderMove = 500,  // 废弃
}

public enum BEHAVIOR_RETCODE
{
    Failed = 0,
    Success = 1,
    Blocked = 2,  // 被阻挡，用于移动时
    InvalidPos = 3,  // 被阻挡，用于移动时
}

//public delegate void OnBehaviorFinish(BEHAVIOR_RETCODE retcode);

public abstract class Behavior
{

    public static float RotationLerpFactor = 0.3f;


    protected Transform _Owner;
    protected BehaviorType _Type;
    protected ObjectBehaviour.OBJ_TYPE _OwnerType;
    protected ObjectBehaviour _ObjectComponent = null;

    public LuaFunction OnFinishCallbackRef = null;
    public bool IsInCallback = false;
    public bool ReActive = false;

    protected Behavior(BehaviorType type)
    {
        _Type = type;
    }

    public void Init(Transform trans, ObjectBehaviour.OBJ_TYPE objType)
    {
        _Owner = trans;
        _OwnerType = objType;
    }

    public Transform Owner
    {
        get { return _Owner; }
    }

    public BehaviorType Type
    {
        get { return _Type; }
    }

    public ObjectBehaviour.OBJ_TYPE OwnerType
    {
        get { return _OwnerType; }
    }

    protected bool IsValidPositionStrict(Vector3 pos)
    {
        return NavMeshManager.Instance.IsValidPositionStrict(pos, false, NavMeshManager.TRACE_EXTEND_DISTANCE);
    }

    protected virtual void RealOnFinish(BEHAVIOR_RETCODE ret, Vector3 moveDir)
    {
        IsInCallback = true;

        SyncMoveStamp(moveDir, true, true, _Owner.position);

        if (OnFinishCallbackRef != null)
        {
            OnFinishCallbackRef.Call((int)ret);
            if (!ReActive)
            {
                OnFinishCallbackRef.Release();
                OnFinishCallbackRef = null;
            }
        }
        IsInCallback = false;
    }

    protected void TickAdjustDir(Vector3 destDir)
    {
        if(_ObjectComponent == null)
            return;

        destDir.y = 0;
        var destRot = Quaternion.LookRotation(destDir, Vector3.up);
        var curDir = _Owner.forward;
        curDir.y = 0;
        var curRotation = Quaternion.LookRotation(curDir, Vector3.up);
        // 非匀速Lerp无限接近目标值
        _Owner.rotation = Quaternion.Slerp(curRotation, destRot, RotationLerpFactor);      
    }

    public virtual void OnRemove(){}

    public abstract bool Tick(float dt);

    protected void SyncMoveStamp(Vector3 moveDir, bool isStop, bool forceSync, Vector3 destPos)
    {
        if (_Type == BehaviorType.Follow || _Type == BehaviorType.Move || _Type == BehaviorType.JoyStickMove)
        {
            if (_ObjectComponent == null)
                _ObjectComponent = _Owner.GetComponent<ObjectBehaviour>();

            if (_ObjectComponent != null)
                _ObjectComponent.SyncHostMoveInfo(moveDir, isStop, forceSync, destPos);
        }
    }
}

public class BehaviorFactory
{
     public static Behavior CreateBehavior(BehaviorType type)
    {
        if (type == BehaviorType.Move)
            return new MoveBehavior();
        if (type == BehaviorType.Turn)
            return new TurnBehavior();
        if (type == BehaviorType.Dash)
            return new DashBehavior();
        if (type == BehaviorType.Follow)
            return new FollowBehavior();
        if (type == BehaviorType.JoyStickMove)
            return new JoyStickBehavior();
        if (type == BehaviorType.Adsorb)
            return new AdsorbBehavior();

        Debug.LogWarning("Undefined Behavior Type");
        return null;
    }
}
