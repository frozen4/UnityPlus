using System.Collections.Generic;
using UnityEngine;
using EntityComponent;

public class CPlayerFollowCam : CECCamCtrl
{
    public enum CTRL_MODE
    {
        FOLLOW = 0,
        FIX3D,
        FIX25D,

        //LOCK_DIR,
    };

    public static bool Is3DCameraMode(CTRL_MODE mode)
    {
        return mode == CTRL_MODE.FOLLOW || mode == CTRL_MODE.FIX3D;
    }

    private Transform _HostPlayerModel;
    private ObjectBehaviour _HostPlayerComponent;

    private Transform HostPlayerModel
    {
        get
        {
            if (Main.HostPalyer == null) return null;
            if (_HostPlayerModel == null)
                _HostPlayerModel = Main.HostPalyer.Find("Model");
            return _HostPlayerModel;
        }
    }

    private ObjectBehaviour HostPlayerComponent
    {
        get
        {
            if (Main.HostPalyer == null) return null;
            if (_HostPlayerComponent == null)
                _HostPlayerComponent = Main.HostPalyer.GetComponent<ObjectBehaviour>();
            return _HostPlayerComponent;
        }
    }

    private Transform LookAtTarget
    {
        get
        {
            return Main.HostPalyer;
        }
    }

    private CTRL_MODE _DestCamCtrlMode = CTRL_MODE.FOLLOW;
    private CTRL_MODE _CamCtrlMode = CTRL_MODE.FOLLOW;
    private Vector3 _CurLookAtPos;
    private float _PitchDegDest;
    private float _YawDegDest;

    private float _DistOffset;
    private float _DistOffsetDest;
    private bool _IsDistChangeByForce = false;      //相机距离是否被强制修改（非自动调整）
    private float _CurMaxDistOffset;        //当前区域的最大距离
    private CameraTraceResult _CamTraceResult = CameraTraceResult.NoHit;

    public float _HeightOffset = 1.25f;
    private float _CurHeightOffset = 1.25f;
    private float _HeightOffsetSpeed = 1f;
    private float _HeightOffsetMin = 0f;
    private float _HeightOffsetMax = 0f;
    private float _HeightOffset2D = 0f;

    private bool _IsMoveToDest = false; //是否移动到特定位置
    private float _DistOffsetSpeed = 0f;
    private float _PitchSpeed = 0f;

    private bool _IsDraggingScreen = false;
    private bool _IsMoving;
    private bool _IsModeChangeRecover;          //更改跟随模式旋转
    private bool _IsQuickRecoverDist = false;
    private bool _HasOwnDistOffset = false;

    private int _CamLockPriority = -1;
    private bool _IsLockCurState = false;                   //是否锁定相机当前状态
    private Dictionary<int, CTRL_MODE> _LockConfigs = new Dictionary<int, CTRL_MODE>();

    private bool _IsSkillRecover;                           //技能旋转
    private Vector3 _SkillRecoverDir = Vector3.forward;     //技能旋转方向（水平）

    /*---战斗锁定视角---*/
    private bool IsFightLock { get; set; }                    //是否战斗锁定视角
    private bool _IsChangeLockTarget = false;                 //是否切换了锁定目标
    private GameObject _FightLockTarget;                      //视角锁定的目标
    private float _LockTargetHeightOffset = 0f;               //目标高度调整
    private const float LOOK_AT_POS_OFFSET_SPEED = 50f;       //更改视点时相机的移动速度

    private LuaInterface.LuaFunction _Enter_Near_Cam_Lua_Func;     //进入近景模式的Lua函数

    private Vector3 _LastVStart = Vector3.zero;
    private Vector3 _LastVEnd = Vector3.zero;

    private PlayerFollowCameraConfig CamConfig
    {
        get
        {
            return EntryPoint.Instance.PlayerFollowCameraConfig;
        }
    }

    public CPlayerFollowCam()
    {
        _DistOffset = _DistOffsetDest = CamConfig.CamDefaultOffsetDist;
        _PitchDeg = _PitchDegDest = CamConfig.DefaultPitchDegDest;
        _YawDeg = _YawDegDest = CamConfig.DefaultYawDegDest;

        _CurMaxDistOffset = CamConfig.CamMaxOffsetDist;
    }

    public void DragStart()
    {
        if (_IsDraggingScreen)
            return;
        _IsDraggingScreen = true;
    }

    public void DragEnd()
    {
        if (!_IsDraggingScreen)
            return;
        _IsDraggingScreen = false;
    }

    #region 外部接口
    public float DistOffset
    {
        get { return _DistOffset; }
    }

    public float DistOffsetDest
    {
        get { return _DistOffsetDest; }
    }

    public float PitchDegDest
    {
        get { return _PitchDegDest; }
    }

    public void SetHeightOffset2D(float fHeightOffset)
    {
        _HeightOffset2D = fHeightOffset;
    }

    public void SetHeightOffsetInterval(float fNormalMin, float fNormalMax, bool isImmediately)
    {
        _HeightOffsetMin = fNormalMin;
        _HeightOffsetMax = fNormalMax;

        if (isImmediately)
        {
            float fHeightOffset = GetHeightOffsetByDistOffset(_DistOffset);
            _CurHeightOffset = fHeightOffset;
            _CurLookAtPos = GetRealLookPos(fHeightOffset);
        }
    }

    public void SetOwnDestDistOffset(float fDistOffset, bool bImmediate)
    {
        if (DestCamCtrlMode == CTRL_MODE.FIX25D)
        {
            _CurMaxDistOffset = Mathf.Max(fDistOffset, CamConfig.CamMaxOffsetDist);
            _DistOffsetDest = _CurMaxDistOffset;
        }
        else
        {
            _CurMaxDistOffset = Mathf.Max(fDistOffset, CamConfig.CamMaxOffsetDist);
            _DistOffsetDest = Mathf.Clamp(fDistOffset, CamConfig.CamMinOffsetDist, _CurMaxDistOffset);
        }

        if (bImmediate)
        {
            _IsDistChangeByForce = true;
            _DistOffset = _DistOffsetDest;
            _CurHeightOffset = GetHeightOffsetByDistOffset(_DistOffset);
        }

        _HasOwnDistOffset = true;
    }

    public void SetDefaultDestDistOffset(bool bImmediate)
    {
        if (DestCamCtrlMode == CTRL_MODE.FIX25D)
        {
            //2.5D默认距离为最大相机距离
            _CurMaxDistOffset = CamConfig.CamMaxOffsetDist;
            _DistOffsetDest = CamConfig.CamMaxOffsetDist;
        }
        else
        {
            _CurMaxDistOffset = CamConfig.CamMaxOffsetDist;
            _DistOffsetDest = Mathf.Clamp(_DistOffsetDest, CamConfig.CamMinOffsetDist, CamConfig.CamMaxOffsetDist);
        }

        if (bImmediate)
        {
            _IsDistChangeByForce = true;
            _DistOffset = _DistOffsetDest;
            _CurHeightOffset = GetHeightOffsetByDistOffset(_DistOffset);
        }

        _HasOwnDistOffset = false;
    }

    public void SetDestDistOffset(float fDist, bool bImmediate)
    {
        _DistOffsetDest = fDist;

        if (bImmediate)
        {
            _IsDistChangeByForce = true;
            _DistOffset = _DistOffsetDest;
            _CurHeightOffset = GetHeightOffsetByDistOffset(_DistOffset);
        }
    }

    public void SetDestDistOffsetAndDestPitchDeg(float fDistOffset, float fPitchDeg, float fDistOffsetSpeed, float fPitchSpeed)
    {
        float maxDist = _HasOwnDistOffset ? _CurMaxDistOffset : CamConfig.CamMaxOffsetDist;
        _DistOffsetDest = Mathf.Clamp(fDistOffset, CamConfig.CamMinOffsetDist, maxDist);
        _PitchDegDest = ClipPitchDegree(fPitchDeg);
        _DistOffsetSpeed = fDistOffsetSpeed;
        _PitchSpeed = fPitchSpeed;
        _IsMoveToDest = true;
    }

    //退出战斗锁定视角
    private void QuitCamFightLock()
    {
        _FightLockTarget = null;
        IsFightLock = false;
        _IsChangeLockTarget = false;
    }

    public void SetCameraCtrlMode(CTRL_MODE mode, bool bChangeYaw, bool bChangePitch, bool bChangeDist, bool isImmediately)
    {
        _DestCamCtrlMode = mode;
        SetCamParams(_IsLockCurState, 10);

        SetToDefaultCamera(bChangeYaw, bChangePitch, bChangeDist, isImmediately);
    }

    public CTRL_MODE DestCamCtrlMode
    {
        get { return _DestCamCtrlMode; }
    }

    public void SetCamParams(bool is_locked, int priority)
    {
        _IsLockCurState = is_locked;
        CTRL_MODE new_mode = _DestCamCtrlMode;

        if (_LockConfigs.ContainsKey(priority))
        {
            _LockConfigs[priority] = new_mode;
        }
        else
        {
            _LockConfigs.Add(priority, new_mode);
        }

        if (priority < _CamLockPriority)
            return;

        _CamLockPriority = priority;
        _CamCtrlMode = new_mode;

        if (!is_locked)//高优先级的如果解锁，自动回到低优先级状态（这里是默认状态）
        {
            _CamLockPriority = 10;
            if (_LockConfigs.TryGetValue(10, out new_mode))
            {
                _CamCtrlMode = new_mode;
            }
            else
            {
                _CamCtrlMode = _DestCamCtrlMode;
                _LockConfigs.Add(10, _DestCamCtrlMode);
            }
        }
    }

    public void ResetCameraMode()           //去掉lockdir
    {
        CTRL_MODE new_mode = _DestCamCtrlMode;
        _CamLockPriority = -1;
        _CamCtrlMode = new_mode;

        _LockConfigs.Clear();
        {
            _CamLockPriority = 10;

            _CamCtrlMode = _DestCamCtrlMode;
            _LockConfigs.Add(10, _DestCamCtrlMode);
        }
    }

    public void SetToDefaultCamera(bool bChangeYaw, bool bChangePitch, bool bChangeDistOffset, bool isImmediately = true)
    {
        QuitCamFightLock();
        _CurLookAtPos = GetRealLookPos(_CurHeightOffset);

        if (!bChangeYaw && !bChangePitch && !bChangeDistOffset)
            return;

        if (bChangeYaw)
        {
            float yaw_deg_dest = 0;
            if (LookAtTarget != null)
            {
                var local_forward = LookAtTarget.forward;
                local_forward.y = 0.0f;
                local_forward.Normalize();
                yaw_deg_dest = Mathf.Acos(Vector3.Dot(Vector3.forward, local_forward)) * Mathf.Rad2Deg;
                if (local_forward.x < 0.0f)
                    yaw_deg_dest = 360.0f - yaw_deg_dest;
            }
            _YawDegDest = yaw_deg_dest;
        }
        else
        {
            _YawDegDest = _YawDeg;
        }

        if (DestCamCtrlMode != CTRL_MODE.FIX25D)
        {
            if (bChangePitch)
            {
                _PitchDegDest = CamConfig.DefaultPitchDegDest;
            }
            else
            {
                _PitchDegDest = _PitchDeg;
            }

            if (bChangeDistOffset)
            {
                if (!_HasOwnDistOffset)
                    _DistOffsetDest = CamConfig.CamDefaultOffsetDist;
                else
                    _DistOffsetDest = _CurMaxDistOffset;
            }
            else
            {
                var maxDist = _HasOwnDistOffset ? _CurMaxDistOffset : CamConfig.CamMaxOffsetDist;
                _DistOffsetDest = _DistOffset = Mathf.Clamp(_DistOffset, CamConfig.CamMinOffsetDist, maxDist);
                _CurHeightOffset = GetHeightOffsetByDistOffset(_DistOffset);
            }
        }
        else
        {
            if (bChangePitch)
            {
                _PitchDegDest = CamConfig.CamDefPitchDegDest25D;
            }
            else
            {
                _PitchDegDest = _PitchDeg;
            }

            if (bChangeDistOffset)
            {
                if (!_HasOwnDistOffset)
                    //2.5D默认距离为最大相机距离
                    _DistOffsetDest = CamConfig.CamMaxOffsetDist;
                else
                    _DistOffsetDest = _CurMaxDistOffset;
            }
            else
            {
                var maxDist = _HasOwnDistOffset ? _CurMaxDistOffset : CamConfig.CamMaxOffsetDist;
                _DistOffsetDest = _DistOffset = Mathf.Clamp(_DistOffset, CamConfig.CamMinOffsetDist, maxDist);
                _CurHeightOffset = GetHeightOffsetByDistOffset(_DistOffset);
            }
        }

        _IsSkillRecover = false;
        if (!isImmediately)
        {
            if (bChangeYaw || bChangePitch)
                _IsModeChangeRecover = true;
        }
        else
        {
            if (bChangeYaw)
                _YawDeg = _YawDegDest;

            if (bChangePitch)
                _PitchDeg = _PitchDegDest;

            if (bChangeDistOffset)
            {
                _IsDistChangeByForce = true;
                _DistOffset = _DistOffsetDest;
                _CurHeightOffset = GetHeightOffsetByDistOffset(_DistOffset);
            }
        }
    }

    //设置是否进入战斗锁定视角
    public void SetCamFightLockState(bool isLock, GameObject target = null)
    {
        if (isLock && target != null && _CamCtrlMode == CTRL_MODE.FOLLOW)
        {
            _IsChangeLockTarget = _FightLockTarget != target;

            //只有跟随模式才能进入锁定
            _FightLockTarget = target;
            IsFightLock = true;
            _LockTargetHeightOffset = CUnityUtil.GetModelHeight(_FightLockTarget, true) / 2;
        }
        else
        {
            QuitCamFightLock();
        }
    }

    //技能回正
    public void RecoverCameraBySkill(float x, float z)
    {
        Vector3 dest = new Vector3(x, 0, z);
        Vector3 dir = dest - LookAtTarget.position;
        dir.y = 0.0f;
        dir.Normalize();

        if (dir.sqrMagnitude < 0.0005f)         //过小
            return;
        _SkillRecoverDir = dir;

        _IsSkillRecover = true;
        _IsModeChangeRecover = false;

        UpdateForwardDeg(_SkillRecoverDir);
    }

    //获取当前相机位置，可以在获取之前把参数设为默认从而获得默认的相机位置
    public Vector3 GetCamCurPos()
    {
        //float yaw = 0f;
        //float pitch = 0f;
        //float dist = 0f;

        //if (LookAtTarget != null)
        //{
        //    var local_forward = LookAtTarget.forward;
        //    local_forward.y = 0.0f;
        //    local_forward.Normalize();
        //    yaw = Mathf.Acos(Vector3.Dot(Vector3.forward, local_forward)) * Mathf.Rad2Deg;
        //    if (local_forward.x < 0.0f)
        //        yaw = 360.0f - yaw;
        //}

        //if (_CamCtrlMode != CTRL_MODE.FIX25D)
        //{
        //    pitch = CamConfig._DefaultPitchDegDest;
        //    dist = CamConfig._CamDefaultOffsetDist;
        //}
        //else
        //{
        //    pitch = CamConfig._CamDefPitchDegDest25D;
        //    dist = CamConfig._CamMaxOffsetDist;
        //}

        Vector3 dir = GetDirFromPitchAndYaw(_PitchDeg, _YawDeg);
        dir.Normalize();

        return GetRealLookPos(_CurHeightOffset) - dir * _DistOffset;
    }

    public Vector3 GetCamDir()
    {
        return _RealDir;
    }

    public float GetHeightOffset()
    {
        return GetHeightOffsetByDistOffset(_DistOffsetDest);
    }

    public bool IsQuickFollow { get; set; }

    public void StopRecoverCamera()
    {
        _IsModeChangeRecover = false;
        _IsSkillRecover = false;
    }
    #endregion 外部接口

    #region 相机更新逻辑

    public override bool Tick(float dt)
    {
        if (Main.HostPalyer == null || LookAtTarget == null || _CameraTrans == null)
            return false;

        bool bHideTarget = false;
        if (CCamCtrlMan.Instance.IsTransToPortal)
        {
            if (HostPlayerModel != null)
            {
                if (HostPlayerModel.localScale.IsZero())
                    HostPlayerModel.localScale = Vector3.one;
            }

            return false;
        }
        else
        {
            //按顺序执行
            UpdateCamFightLockState();
            UpdateCameraOffset(dt);

            UpdateCameraLookAtPos(dt);
            UpdateCameraCtrlInfo();
            UpdateCameraOrientation(dt);            //屏蔽2.5D模式

            ApplyDirAndUp();

            if (IsQuickFollow)
                bHideTarget = CollisionFix(CUnityUtil.LayerMaskTerrainBuildingCameraCollision);
            else
                bHideTarget = CollisionFix(CUnityUtil.LayerMaskTerrainCameraCollision);
        }

        if (bHideTarget)
        {
            if (HostPlayerModel != null)
            {
                if (HostPlayerModel.localScale.IsOne())
                    HostPlayerModel.localScale = Vector3.zero;
            }
        }
        else 
        {
            if (HostPlayerModel != null)
            {
                if (HostPlayerModel.localScale.IsZero())
                    HostPlayerModel.localScale = Vector3.one;
            }
        }


        //dir修正，避免抖动
        Vector3 vDir = _CurLookAtPos - _RealPos;
        if (Util.IsValidDir(ref vDir))
            _RealDir = vDir;

        _CameraTrans.position = _RealPos;
        _CameraTrans.SetDirAndUp(_RealDir, _RealUp);

        if (!CCamCtrlMan.Instance.IsTransToPortal)
        {
            AdjustBuildingTrans();
        }

        return true;
    }

    protected void UpdateCamFightLockState()
    {
        if (!IsFightLock || _CamCtrlMode != CTRL_MODE.FOLLOW)
            return;

        if (_FightLockTarget == null)
        {
            QuitCamFightLock();
            return;
        }

        Vector3 player_look_pos = GetRealLookPos(_CurHeightOffset); //主角身上的视点
        Vector3 monster_look_pos = GetLockTargetLookAtPos();
        //锁定视角的视点，在怪物和玩家连线上的一个点
        Vector3 cur_dir = monster_look_pos - player_look_pos;
        float length = cur_dir.magnitude * CamConfig.FightLogkLookAtPointRate;
        float dist_ratio = (_DistOffset - CamConfig.CamMinOffsetDist) / _CurMaxDistOffset;
        float cur_lock_dist = length * dist_ratio; //视点与锁定目标当前的水平距离

        float lock_dist_rate = cur_lock_dist / CamConfig.GetCamToPlayerMinDist();
        if (lock_dist_rate > CamConfig.UnLockDistRate)
        {
            //主角与目标的距离与最小距离比率超出配置比率，退出锁定视角
            QuitCamFightLock();
        }
    }

    protected void UpdateCameraCtrlInfo()
    {
        _IsMoving = HostPlayerComponent != null && (HostPlayerComponent.HasBehavior(BehaviorType.Move) || HostPlayerComponent.HasBehavior(BehaviorType.Follow) || HostPlayerComponent.HasBehavior(BehaviorType.JoyStickMove));

        if (!_IsModeChangeRecover && !_IsSkillRecover)
        {
            if (IsFightLock && _FightLockTarget != null)
            {
                //PVE锁定视角
                Vector3 local_forward = _FightLockTarget.transform.position - LookAtTarget.position;
                UpdateForwardDeg(local_forward);
            }
            else if (_IsMoving && _CamCtrlMode == CTRL_MODE.FOLLOW && !_IsLockCurState)
            {
                //移动中相机Follow
                Vector3 local_forward = LookAtTarget.forward;
                UpdateForwardDeg(local_forward);
            }
        }
    }

    //更新相机向前的角度
    protected void UpdateForwardDeg (Vector3 local_forward)
    {
        local_forward.y = 0.0f;
        local_forward.Normalize();
        float yaw_deg_dest = Mathf.Acos(Vector3.Dot(Vector3.forward, local_forward)) * Mathf.Rad2Deg;
        if (local_forward.x < 0.0f)
            yaw_deg_dest = 360.0f - yaw_deg_dest;

        _YawDegDest = yaw_deg_dest;
        _PitchDegDest = Mathf.Clamp(_PitchDegDest, CamConfig.GetMinPitch(), CamConfig.GetMaxPitch());
    }

    public Vector3 GetRealLookPos(float fHeightOffset)
    {
        if (LookAtTarget == null)
            return Vector3.zero;

        var lookAtDest = LookAtTarget.position;
        lookAtDest.y += fHeightOffset;
        return lookAtDest;
    }

    //计算怪物身上的视点
    private Vector3 GetLockTargetLookAtPos()
    {
        if (_FightLockTarget == null)
            return Vector3.zero;

        Vector3 monster_pos = _FightLockTarget.transform.position;
        monster_pos.y += _LockTargetHeightOffset;

        return monster_pos;
    }

    //更新主角高度修正
    private void UpdateHeightOffset(float dt)
    {
        _HeightOffset = GetHeightOffsetByDistOffset(_DistOffset);
        if (Common.Utilities.FloatEqual(_HeightOffset, _CurHeightOffset))
            return;

        float delta = _HeightOffsetSpeed * dt;
        if (_HeightOffset > _CurHeightOffset)
            _CurHeightOffset = Mathf.Min(_CurHeightOffset + delta, _HeightOffset);
        else
            _CurHeightOffset = Mathf.Max(_CurHeightOffset - delta, _HeightOffset);
    }

    //更新相机视点
    protected void UpdateCameraLookAtPos(float dt)
    {
        if (_CameraTrans == null || LookAtTarget == null) return;

        UpdateHeightOffset(dt);
        Vector3 player_look_pos = GetRealLookPos(_CurHeightOffset); //主角身上的视点
        Vector3 dest_look_pos;   //目标视点
        
        if (IsFightLock)
        {
            Vector3 monster_look_pos = GetLockTargetLookAtPos();
            //锁定视角的视点，在怪物和玩家连线上的一个点
            Vector3 cur_dir = monster_look_pos - player_look_pos;
            float scale = cur_dir.magnitude * CamConfig.FightLogkLookAtPointRate; //比率为0视点在主角上，为1视点在目标上
            float dist_ratio = (_DistOffset - CamConfig.CamMinOffsetDist) / _CurMaxDistOffset; //根据相机距离调整视点，保证相机最小距离时，视点在主角身上
            cur_dir = cur_dir.normalized * scale * dist_ratio;

            dest_look_pos = player_look_pos + cur_dir;
        }
        else
            dest_look_pos = player_look_pos;
        
        float posTickLength = LOOK_AT_POS_OFFSET_SPEED * dt;
        Vector3 dest_dir = dest_look_pos - _CurLookAtPos;
        if (dest_dir.magnitude > posTickLength)
        {
            dest_dir.Normalize();
            _CurLookAtPos += dest_dir * posTickLength;
        }
        else
        {
            _CurLookAtPos = dest_look_pos;
        }
    }
    
    protected float GetDistOffsetSpeed()
    {
        if (_IsModeChangeRecover)
        {
            return 5.0f + Mathf.Abs((_DistOffsetDest - _DistOffset) / CamConfig.CamDistSpeedOffset);
        }

        if (_IsSkillRecover)
            return Mathf.Abs((_DistOffsetDest - _DistOffset) / 0.15f);

        if (_IsQuickRecoverDist)
            return 15.0f;

        if (_IsMoveToDest)
            return _DistOffsetSpeed;

        return 3.0f;
    }

    protected float GetCamYawSpeed(float fYawDiff)
    {
        if (_IsModeChangeRecover)
            return CamConfig.CamYawRecoverSpeed;

        fYawDiff = ClipDegreeDiff(fYawDiff);
        float yaw_speed;
        float ratio = 1.0f - (Mathf.Abs(Mathf.Abs(fYawDiff) - 90.0f)) / 90.0f;

        if (_IsSkillRecover || _IsChangeLockTarget)
        {
            //释放技能或锁定回正
            float slow_deg = 45f;               //开始减速的角度
            float diff = Mathf.Abs(fYawDiff);
            if (diff > slow_deg)
                return CamConfig.CamYawRecoverSpeed;

            ratio /= (slow_deg / 90);
            float slow_recover_speed = CamConfig.CamYawRecoverSpeedSlow;    //开始减速的速度
            float percentage = 0.1f; //速度的比率
            yaw_speed = slow_recover_speed * percentage + slow_recover_speed * (1 - percentage) * ratio;
            return yaw_speed;
        }

        if (IsFightLock)
            return CamConfig.FightLockYawSpeed;

        float fScale = IsQuickFollow ? 4.0f : 1.0f;            //quick时最大速度是原来2倍
        yaw_speed = CamConfig.CamMinYawSpeed + (fScale * CamConfig.CamMaxYawSpeed - CamConfig.CamMinYawSpeed) * ratio;

        float fDistRatio = 1.0f;
        if (_DistOffset > 10.0f)
            fDistRatio = 1.0f - Mathf.Min((_DistOffset - 10.0f) / 40.0f, 0.5f);

        return yaw_speed * fDistRatio;
    }

    protected float GetCamPitchSpeed(float fPitchDiff)
    {
        if (_IsModeChangeRecover || _IsSkillRecover)
            return CamConfig.CamPitchRecoverSpeed;

        if (_IsMoveToDest)
            return _PitchSpeed;

        float fScale = IsQuickFollow ? 4.0f : 1.0f;            //quick时最大速度是原来4倍

        fPitchDiff = ClipDegreeDiff(fPitchDiff);
        float ratio = 1f - Mathf.Abs(fPitchDiff) / 90.0f;
        float pitch_speed = CamConfig.CamMinPitchSpeed + (fScale * CamConfig.CamMaxPitchSpeed - CamConfig.CamMinPitchSpeed) * ratio;

        return pitch_speed;
    }

    //更新相机和视点的距离
    protected void UpdateCameraOffset(float dt)
    {
        if (!_IsStretching)
        {
            //_DistOffset = Mathf.Lerp(_DistOffset,_DistOffsetDest,dt);
            float speed = GetDistOffsetSpeed();

            float offsetTickLength = speed * dt;
            if (_DistOffsetDest - _DistOffset > offsetTickLength)
            {
                _DistOffset += offsetTickLength;
                _CurHeightOffset = GetHeightOffsetByDistOffset(_DistOffset);
            }
            else if (_DistOffsetDest - _DistOffset < -offsetTickLength)
            {
                _DistOffset -= offsetTickLength;
                _CurHeightOffset = GetHeightOffsetByDistOffset(_DistOffset);
            }
            else
            {
                _DistOffset = _DistOffsetDest;
                _IsQuickRecoverDist = false;
                CheckMoveToDestState();
            }
        }
        else
        {
            float now = Time.time;
            if (now < _FadeInTime)
                _DistOffset -= (Time.deltaTime * _FadeInSpeed);
            else if (now < _StayTime)
                _DistOffset = _StretchedOffsetDis;
            else if (now < _FadeOutTime)
                _DistOffset += (Time.deltaTime * _FadeOutSpeed);
            else
            {
                _DistOffset = _DistOffsetDest;
                _IsStretching = false;
            }
        }
    }

    //更新相机方向
    protected void UpdateCameraOrientation(float dt)
    {
        if (_IsDraggingScreen)
            return;

        float deg_diff_pitch = 0;
        float deg_diff_yaw = 0;
        float tick_pich = 0;
        float tick_yaw = 0;

        if (IsFightLock || _IsModeChangeRecover || _IsSkillRecover || _CamCtrlMode == CTRL_MODE.FOLLOW)
        {
            bool isMovingFixed = false; //是否属于移动中3D相机自动修正
            if (!IsFightLock && !_IsModeChangeRecover && !_IsSkillRecover && !_IsMoveToDest && _CamCtrlMode == CTRL_MODE.FOLLOW)
            {
                if (!_IsMoving || _IsLockCurState)
                    return;
                isMovingFixed = true;
            }

            if (!_IsSkillRecover)
            {
                //pitch
                deg_diff_pitch = ClipDegreeDiff(_PitchDegDest - _PitchDeg);
                float pitch_speed = GetCamPitchSpeed(deg_diff_pitch);
                tick_pich = pitch_speed * dt;

                if (deg_diff_pitch > tick_pich)
                    _PitchDeg += tick_pich;
                else if (deg_diff_pitch < -tick_pich)
                    _PitchDeg -= tick_pich;
                else
                {
                    _PitchDeg = _PitchDegDest;
                    CheckMoveToDestState();
                }
                _PitchDeg = ClipPitchDegree(_PitchDeg);
            }

            //yaw
            {
                deg_diff_yaw = ClipDegreeDiff(_YawDegDest - _YawDeg);
                float yaw_speed = GetCamYawSpeed(deg_diff_yaw);

                if (isMovingFixed)
                {
                    float role_cur_speed = 0f; //主角当前移动速度
                    if (HostPlayerComponent != null)
                        role_cur_speed = HostPlayerComponent.GetHostPlayerMoveSpeed();

                    float role_default_speed = CCamCtrlMan.Instance.CurProDefaultSpeed; //当前职业默认移动速度

                    //Common.HobaDebuger.LogFormat("role_cur_speed:{0}, role_default_speed:{1}, yaw_speed_origin:{2}", role_cur_speed, role_default_speed, yaw_speed);

                    //摄像机最终跟随速度 = 摄像机跟随速度 * 当前移动速度 / 初始移动速度
                    if (role_cur_speed > 0f && role_default_speed > 0f)
                        yaw_speed *= (role_cur_speed / role_default_speed);
                }

                //Common.HobaDebuger.LogFormat("yaw_speed:{0}", yaw_speed);
                tick_yaw = yaw_speed * dt;

                if (deg_diff_yaw > tick_yaw)
                    _YawDeg += tick_yaw;
                else if (deg_diff_yaw < -tick_yaw)
                    _YawDeg -= tick_yaw;
                else
                    _YawDeg = _YawDegDest;
                _YawDeg = ClipYawDegree(_YawDeg);
            }

            if (_IsModeChangeRecover)
            {
                if (Mathf.Abs(_PitchDegDest - _PitchDeg) <= Util.FloatZero && Mathf.Abs(_YawDegDest - _YawDeg) <= Util.FloatZero)
                {
                    _IsModeChangeRecover = false;
                }
            }

            if (_IsSkillRecover)
            {
                if (Mathf.Abs(_YawDegDest - _YawDeg) <= Util.FloatZero)
                {
                    _IsSkillRecover = false;
                }
            }

            if (_IsChangeLockTarget)
            {
                if (Mathf.Abs(_YawDegDest - _YawDeg) <= Util.FloatZero)
                {
                    _IsChangeLockTarget = false;
                }
            }
        }
    }

    private float dist_before_hit = 0f;
    private bool CollisionFix(int layerMask)
    {
        bool bHideTarget = false;

        CameraTraceResult bLastCollide = _CamTraceResult;
        if (bLastCollide == CameraTraceResult.NoHit || _IsDistChangeByForce)
        {
            dist_before_hit = _DistOffset;
            _IsDistChangeByForce = false;
        }
        
        Vector3 vDelta = -dist_before_hit * _RealDir;
        Vector3 vTracePos;
        float fDistance;
        _CamTraceResult = CMapUtil.CameraTrace(layerMask, _CurLookAtPos, _CameraRadius, vDelta, out vTracePos, out fDistance);
        if (_CamTraceResult == CameraTraceResult.HitTerrain)
        {
            _IsQuickRecoverDist = false;
            _DistOffset = _DistOffsetDest;

            float dist = fDistance;
            if (fDistance <= 0.1f)   //避免出现摄像机位置和碰撞位置重合的情况，这时方向会乱      
            {
                dist = 0.1f;
            }
            //_CurHeightOffset = GetHeightOffsetByDistOffset(dist);
            //real_look_at_pos = GetRealLookPos(_CurHeightOffset);
            vDelta.Normalize();
            _RealPos = _CurLookAtPos + vDelta * dist;
        }
        else if (_CamTraceResult == CameraTraceResult.HitCameraCollision)
        {
            _IsQuickRecoverDist = false;
            _DistOffset = _DistOffsetDest;

            float dist = fDistance;
            if (fDistance <= _MinCamCollisionDist)
            {
                dist = _MinCamCollisionDist;
            }
            vDelta.Normalize();
            _RealPos = _CurLookAtPos + vDelta * dist;
        }
        else
        {
            if (bLastCollide != CameraTraceResult.NoHit)
            {
                _DistOffset = Mathf.Clamp(dist_before_hit, CamConfig.CamMinOffsetDist, _CurMaxDistOffset);
                //_DistOffset = (_CurLookAtPos - _RealPos).magnitude;
                //_DistOffset = Mathf.Clamp(_DistOffset, CamConfig._CamMinOffsetDist, _CurMaxDistOffset);

                _IsQuickRecoverDist = true;
            }
            
            _RealPos = _CurLookAtPos - _DistOffset * _RealDir;
        }

        bool bTerrainFix = false;

        float fTargetHeight;
        if (!CMapUtil.GetMapHeight(_CurLookAtPos, _CameraRadius, out fTargetHeight))
            fTargetHeight = CUnityUtil.InvalidHeight;

        //确保在地面一定高度
        float fHeight = 0.0f;
        if (CMapUtil.GetMapHeight(_RealPos, _CameraRadius, out fHeight))
        {
            Vector3 vPos = _RealPos;
            if (vPos.y < fHeight + _CameraTerrainHitHeight)
            {
                vPos.y = fHeight + _CameraTerrainHitHeight;

                if ((vPos - _CurLookAtPos).sqrMagnitude < _DistOffset * _DistOffset)
                {
                    _RealPos = vPos;
                    bTerrainFix = true;
                }
                //_DistOffset = (_CurLookAtPos - _RealPos).magnitude;
            }

            //不能低于目标所在的高度
            if (fTargetHeight != CUnityUtil.InvalidHeight && vPos.y < fTargetHeight + _CameraTerrainHitHeight)
            {
                vPos.y = fTargetHeight + _CameraTerrainHitHeight;
                _RealPos = vPos;
                bTerrainFix = true;
            }
        }

        if (!bTerrainFix)
            bHideTarget = (_RealPos - _CurLookAtPos).sqrMagnitude < _MinCamHostDist * _MinCamHostDist;

        return bHideTarget;
    }

    protected void ApplyDirAndUp()
    {
        _RealDir = GetDirFromPitchAndYaw(_PitchDeg, _YawDeg);
        //Vector3 vRight = Vector3.Cross(Vector3.up, _RealDir);
        //vRight.Normalize();
        //_RealUp = Vector3.Cross(_RealDir, vRight);
        //_RealUp.Normalize();
        _RealUp = Vector3.up;
    }


    protected void AdjustBuildingTrans()
    {
        Vector3 vStart = _CurLookAtPos;
        Vector3 vEnd = _RealPos;

        if (vStart != _LastVStart || vEnd != _LastVEnd)
        {
            CEnvTransparentEffectMan.Instance.RayAdjustBuildings(vStart, vEnd - vStart);
           _LastVStart = vStart;
           _LastVEnd = vEnd;
        }
    }

    protected float GetHeightOffsetByDistOffset(float fDistOffset)
    {
        fDistOffset = Mathf.Clamp(fDistOffset, CamConfig.CamMinOffsetDist, CamConfig.CamMaxOffsetDist);
        float rate = (fDistOffset - CamConfig.CamMinOffsetDist / CamConfig.CamMaxOffsetDist - CamConfig.CamMinOffsetDist);
        return rate * (_HeightOffsetMax - _HeightOffsetMin) + _HeightOffsetMin;
    }

    protected void CheckMoveToDestState()
    {
        if (_IsMoveToDest)
        {
            if (Common.Utilities.FloatEqual(_DistOffset, _DistOffsetDest) && Common.Utilities.FloatEqual(_PitchDeg, _PitchDegDest))
            {
                //移动完成
                _IsMoveToDest = false;
            }
        }
    }

    #endregion 相机更新逻辑

    #region 相机参数改变事件
    public override void OnYawChanged(float dt)
    {
        _IsModeChangeRecover = false;
        _IsSkillRecover = false;
        _IsMoveToDest = false;

        if (IsFightLock)
        {
            if (Mathf.Abs(dt) < 1.5f) return; //锁定视角时无视小幅度的水平拖拽
            QuitCamFightLock(); //解除锁定视角
            LuaScriptMgr.Instance.CallLuaFunction("QuitCameraLockState");
        }

        //Common.HobaDebuger.LogFormat("Yaw dt:{0}", dt);
        _YawDeg += dt;
        _YawDeg = ClipYawDegree(_YawDeg);
        //Debug.Log(_YawDeg);
        _YawDegDest = _YawDeg;
        ApplyDirAndUp();
    }

    public override void OnPitchChanged(float dt)
    {
        if (_CamCtrlMode == CTRL_MODE.FIX25D) return;

        //Common.HobaDebuger.LogFormat("Pitch dt:{0}", dt);
        _IsModeChangeRecover = false;
        _IsSkillRecover = false;
        _IsMoveToDest = false;

        _PitchDeg -= dt;
        _PitchDeg = ClipPitchDegree(_PitchDeg);
        _PitchDegDest = _PitchDeg;
        ApplyDirAndUp();
    }

    public override void OnDistanceChanged(float dt)
    {
        if (_CamCtrlMode == CTRL_MODE.FIX25D) return;

        _IsModeChangeRecover = false;
        _IsSkillRecover = false;
        _IsMoveToDest = false;

        float dist = _DistOffset + dt * CamConfig.CamRollSensitivity;

        if (!InputManager.Instance.IsMultiDragingStarted && dist < CamConfig.CamMinOffsetDist && Mathf.Abs(dt) >= 0.25f)
        {
            if (_Enter_Near_Cam_Lua_Func == null)
                _Enter_Near_Cam_Lua_Func = LuaScriptMgr.Instance.GetLuaFunction("TryEnterNearCam");

            if (_Enter_Near_Cam_Lua_Func != null)
            {
                _Enter_Near_Cam_Lua_Func.Call();    //尝试进入近景模式
                return;
            }
        }

        _IsDistChangeByForce = true;
        _DistOffset = _DistOffsetDest = Mathf.Clamp(dist, CamConfig.CamMinOffsetDist, _CurMaxDistOffset);
        _CurHeightOffset = GetHeightOffsetByDistOffset(_DistOffset);
    }
    #endregion

    #region 技能中相机拉近拉远方法

    private bool _IsStretching = false;
    private float _StretchedOffsetDis = 0f;
    private float _FadeInSpeed = 0f;
    private float _FadeOutSpeed = 0f;
    private float _FadeInTime = 0f;
    private float _StayTime = 0f;
    private float _FadeOutTime = 0f;

    public void SetStretchingParams(float wanted_dis_factor, float fadein_time, float stay_time, float fadeout_time)
    {
        _StretchedOffsetDis = _DistOffset * wanted_dis_factor / 100;
        if (fadein_time < Time.deltaTime) fadein_time = Time.deltaTime;
        _FadeInSpeed = (_DistOffset - _StretchedOffsetDis) / fadein_time;
        if (fadeout_time < Time.deltaTime) fadeout_time = Time.deltaTime;
        _FadeOutSpeed = (_DistOffset - _StretchedOffsetDis) / fadeout_time;
        _FadeInTime = Time.time + fadein_time;
        _StayTime = _FadeInTime + stay_time;
        _FadeOutTime = _StayTime + fadeout_time;

        _IsStretching = true;
    }

    public void StopCameraStretching()
    {
        _DistOffset = _DistOffsetDest;
        _IsStretching = false;
    }

    #endregion 技能中相机拉近拉远方法
}