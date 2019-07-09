using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using LuaInterface;
using Common;

public enum NearCamStage
{
    Body = 0,       //全身
    HalfBody,       //半身
    Chest           //胸像
}

public class CPlayerNearCam : CECCamCtrl
{
    private const float CAMERA_IN_DURATION = 0.5f;
    private const float CAMERA_OUT_DURATION = 0.5f;
    //private const float DOF_DISTANCE_OFFSET = 0.75f;
    private const float IK_LOOK_ANGLE_LERP = 45f;
    private const float IK_LOOK_ANGLE_MAX = 65f;
    
    private static float DIST_OFFSET_SPEED = 3f; //距离拉进速度
    //private static float DEFAULT_DIST_OFFSET = 1.25f;    //默认相机距离

    #region 属性
    private Transform _HostPlayerModel;
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
    private Transform LookAtTarget
    {
        get
        {
            return Main.HostPalyer;
        }
    }

    private PlayerFollowCameraConfig _CamConfig;
    private PlayerFollowCameraConfig CamConfig
    {
        get
        {
            if (_CamConfig == null)
                _CamConfig = EntryPoint.Instance.PlayerFollowCameraConfig;

            return _CamConfig;
        }
    }

    private SimpleLookIK2 _IKComponent;
    private SimpleLookIK2 IKComponent
    {
        get
        {
            if (HostPlayerModel == null)
                return null;

            if (_IKComponent == null)
            {
                _IKComponent = HostPlayerModel.GetComponent<SimpleLookIK2>();

                if (_IKComponent == null)
                {
                    _IKComponent = HostPlayerModel.gameObject.AddComponent<SimpleLookIK2>();
                    _IKComponent.enabled = false;
                }
            }

            return _IKComponent;
        }
    }

    private Transform _HostPlayerHead;
    private Transform HostPlayerHead
    {
        get
        {
            if (Main.HostPalyer == null)
                return null;

            if (_HostPlayerHead == null)
            {
                var hang_point = Main.HostPalyer.gameObject.FindChildRecursively("Bip001 Head");
                if (hang_point == null)
                    return null;
                _HostPlayerHead = hang_point.transform;
            }

            return _HostPlayerHead;
        }
    }

    private Transform _HostPlayerEyes;
    private Transform HostPlayerEyes
    {
        get
        {
            if (Main.HostPalyer == null)
                return null;

            if (_HostPlayerEyes == null)
            {        
                var holder = Main.HostPalyer.GetComponentInChildren<HangPointHolder>();
                if (holder != null)
                {
                    var hang_point = holder.GetHangPoint(HangPointHolder.HangPointType.LineOfSightDirection);
                    if (hang_point != null)
                        _HostPlayerEyes = hang_point.transform;
                }             
            }

            return _HostPlayerEyes;
        }
    }
    #endregion

    enum MoveStage
    {
        NONE = -1,
        MOVING_NEAR = 0,
        LOOKING_AT,
        MOVING_FAR,
    };
    private MoveStage _CurStage = MoveStage.NONE;

    private Vector3 _CurLookAtPos;

    private PlayerNearCameraConfig.NearCamProfCfg _ProfCfg;
    private PlayerNearCameraConfig.NearCamStageCfg _StageCfg;
    //private float _YawDegDest;
    //private float _PitchDegDest;
    private float _DistOffset = 3f;
    private float _DistOffsetDest = 0;
    private float _HeightOffsetDest = 1.45f;    //目标视点高度
    private float _CurHeightOffset = 1.45f;     //当前视点高度
    private float _HeightOffsetSpeed = 0.5f;    //视点高度调整速度
    private bool _IsEnterNearCam = false;       //是否进入近景相机
    private bool _IsQuitNearCam = false;        //是否退出近景相机
    public float RollSensitivity { get; set; }  //相机距离拉伸敏感度

    public override bool IsAllowManualAdjust() { return _CurStage == MoveStage.LOOKING_AT; }

    public override void OnDisable()
    {
        EnableCameraEffect(false);
        EnableLookIK(false);
        if (_OriginFOV > 0)
            Main.Main3DCamera.fieldOfView = _OriginFOV;
    }

    public void SetProfConfig(bool bOnRide)
    {
        _ProfCfg = EntryPoint.Instance.PlayerNearCameraConfig.GetProfCfg(bOnRide);
    }

    #region 相机移动
    private Vector3 _OriginLookAtPos = Vector3.zero;
    private float _OriginFOV = 0f;
    public void SetCameraLocation(int type, LuaFunction callback)
    {
        _CtrlType = type;

        if (type == 1)
        {
            //进入近景模式
            if (LookAtTarget == null)
            {
                HobaDebuger.LogError("CPlayerNearCam SetCameraLocation to Near failed, Host Player got null.");
                return;
            }
            if (_ProfCfg == null)
            {
                HobaDebuger.LogError("CPlayerNearCam SetCameraLocation failed, Prof Config got null.");
                return;
            }

            _OriginLookAtPos = CCamCtrlMan.Instance.GetLookAtPosOfGameModel();
            _OriginFOV = Main.Main3DCamera.fieldOfView;
            RollSensitivity = 0.05f;
            _IsQuitNearCam = false;

            try
            {
                _StageCfg = _ProfCfg.GetCurStageData(NearCamStage.Body);
                InitDefaultParams();
                float real_height_offset = GetRealHeightOffset();
                _CurLookAtPos = GetRealLookAtPos(real_height_offset);
                _IsEnterNearCam = true;
                EnableCameraEffect(true);

                var cam_dest_pos = GetRealLookAtPos(real_height_offset) - _RealDir * _DistOffset;
                DoCamMove(true, cam_dest_pos, real_height_offset, _StageCfg._FOV, callback);
            }
            catch(System.Exception e)
            {
                HobaDebuger.LogErrorFormat("CPlayerNearCam SetCameraLocation to Near Exception:{0}", e.Message);
            }
        }
        else if (type == 2)
        {
            //退出近景模式
            if (LookAtTarget == null)
            {
                HobaDebuger.LogError("CPlayerNearCam SetCameraLocation to Far failed, Host Player got null.");
                return;
            }

            LuaScriptMgr.Instance.CallLuaFunction("OpenOrCloseUIPanel", "CPanelUINearCam", false);
            CCamCtrlMan.Instance.GetGameCamCtrl().SetToDefaultCamera(false, false, true, true);
            _IsQuitNearCam = true;
            EnableCameraEffect(false);
            EnableLookIK(false);

            float des_height_offset = CCamCtrlMan.Instance.GetGameCamCtrl().GetHeightOffset();
            var dest_game_cam_pos = GetGameCamPosAfterCollionFix(des_height_offset);
            DoCamMove(false, dest_game_cam_pos, des_height_offset, _OriginFOV, null);
        }
    }

    private void CamMoveNear()
    {
        try
        {
            // 设置数据
            NearCamStage newStage = _StageCfg._Stage + 1;
            _StageCfg = _ProfCfg.GetCurStageData(newStage);

            _PitchDeg = ClampPitch(_PitchDeg);
            _DistOffset = _StageCfg._DistanceLimit.y;
            ApplyDirAndUp();
            // 相机开始移动
            float real_height_offset = GetRealHeightOffset();
            var cam_dest_pos = GetRealLookAtPos(real_height_offset) - _RealDir * _DistOffset;
            DoCamMove(true, cam_dest_pos, real_height_offset, _StageCfg._FOV, null);
        }
        catch (System.Exception e)
        {
            HobaDebuger.LogErrorFormat("CPlayerNearCam Move Near Exception:{0}", e.Message);
        }
    }

    private void CamMoveFar()
    {
        try
        {
            // 设置数据
            NearCamStage newStage = _StageCfg._Stage - 1;
            _StageCfg = _ProfCfg.GetCurStageData(newStage);

            _PitchDeg = ClampPitch(_PitchDeg);
            if (newStage == NearCamStage.Body)
                _DistOffset = _ProfCfg.DefaultParams.z;
            else
                _DistOffset = ClampDistOffset(_DistOffset);
            ApplyDirAndUp();
            // 相机开始移动
            float real_height_offset = GetRealHeightOffset();
            var cam_dest_pos = GetRealLookAtPos(real_height_offset) - _RealDir * _DistOffset;
            DoCamMove(false, cam_dest_pos, real_height_offset, _StageCfg._FOV, null);
        }
        catch(System.Exception e)
        {
            HobaDebuger.LogErrorFormat("CPlayerNearCam Move Far Exception:{0}", e.Message);
        }
    }

    protected void DoCamMove(bool bMoveNear, Vector3 camDesPos, float desHeightOffset, float desFOV, LuaFunction callback)
    {
        if (bMoveNear)
        {
            // 相机拉进
            if (_CurStage == MoveStage.MOVING_NEAR) return;
            if (_CurStage == MoveStage.MOVING_FAR)
                _CameraTrans.DOKill();
            _CurStage = MoveStage.MOVING_NEAR;

            float origin_height_offset = _CurHeightOffset;
            float duration = CAMERA_IN_DURATION;
            Main.Main3DCamera.DOFieldOfView(desFOV, duration).SetEase(Ease.Linear);
            _CameraTrans.DOMove(camDesPos, duration).SetEase(Ease.Linear).OnUpdate(() =>
            {
                // 更新视点高度
                float height_delta = GetDelta(origin_height_offset, desHeightOffset, duration);
                if (Mathf.Abs(_CurHeightOffset - desHeightOffset) > height_delta)
                    _CurHeightOffset += height_delta;
                else
                    _CurHeightOffset = desHeightOffset;

                _CameraTrans.LookAt(GetRealLookAtPos(_CurHeightOffset), Vector3.up);
                UpdateHostPlayerIsNeedHide();
            }).OnComplete(() =>
            {
                _CurHeightOffset = _HeightOffsetDest = desHeightOffset;
                _CurStage = MoveStage.LOOKING_AT;

                if (_IsEnterNearCam)
                {
                    EnableLookIK(true);
                    _IsEnterNearCam = false;
                }

                if (callback != null)
                {
                    callback.Call();
                    callback.Release();
                }
            });
        }
        else
        {
            // 相机拉远
            if (_CurStage == MoveStage.MOVING_FAR) return;
            if (_CurStage == MoveStage.MOVING_NEAR)
                _CameraTrans.DOKill();
            _CurStage = MoveStage.MOVING_FAR;

            float origin_height_offset = _CurHeightOffset;
            float duration = CAMERA_OUT_DURATION;
            Main.Main3DCamera.DOFieldOfView(desFOV, duration).SetEase(Ease.Linear);
            _CameraTrans.DOMove(camDesPos, duration).SetEase(Ease.Linear).OnUpdate(() =>
            {
                // 更新视点高度
                float height_delta = GetDelta(origin_height_offset, desHeightOffset, duration);
                if (Mathf.Abs(_CurHeightOffset - desHeightOffset) > height_delta)
                    _CurHeightOffset += height_delta;
                else
                    _CurHeightOffset = desHeightOffset;

                _CameraTrans.LookAt(GetRealLookAtPos(_CurHeightOffset), Vector3.up);
                UpdateHostPlayerIsNeedHide();
            }).OnComplete(() =>
            {
                _CurHeightOffset = _HeightOffsetDest = desHeightOffset;
                _CurStage = MoveStage.LOOKING_AT;
                if (_IsQuitNearCam)
                {
                    CCamCtrlMan.Instance.SetCurCamCtrl(CAM_CTRL_MODE.GAME, false);
                    _CurStage = MoveStage.NONE;
                }

                if (callback != null)
                {
                    callback.Call();
                    callback.Release();
                }
            });
        }
    }

    // 设置进入近景模式的默认参数
    private void InitDefaultParams()
    {
        if (LookAtTarget != null)
            _YawDeg = GetYawDeg(-GetYawDirWithPlayer(LookAtTarget, _ProfCfg.DefaultParams.x));
        else
            _YawDeg = 0f;

        _PitchDeg = -_ProfCfg.DefaultParams.y;
        _DistOffset = _ProfCfg.DefaultParams.z;

        ApplyDirAndUp();
    }

    private Vector3 GetRealLookAtPos(float fHeightOffset)
    {
        if (LookAtTarget == null)
            return Vector3.zero;

        Vector3 look_pos = LookAtTarget.position;
        look_pos.y += fHeightOffset;
        return look_pos;
    }

    private float GetRealHeightOffset()
    {
        float diff = -_PitchDeg - _StageCfg._HPDivide.y;
        if (diff > Utilities.FloatDeviation)
        {
            //当垂直角度大于分界值
            float totalPitch = _StageCfg._PitchLimit.y - _StageCfg._HPDivide.y; //需要调整的总角度
            float totalHeight = _StageCfg._HeightOffsetLimit.y - _StageCfg._HPDivide.x; //可调整的高度范围
            return diff / totalPitch * totalHeight + _StageCfg._HPDivide.x; //最大Pitch对应最大HeightOffset
        }
        else if (diff < -Utilities.FloatDeviation)
        {
            //当垂直角度小于分界值
            float totalPitch = _StageCfg._HPDivide.y - _StageCfg._PitchLimit.x;
            float totalHeight = _StageCfg._HPDivide.x - _StageCfg._HeightOffsetLimit.x;
            return diff / totalPitch * totalHeight + _StageCfg._HPDivide.x; //最小Pitch对应最小HeightOffset
        }
        else
        {
            return _StageCfg._HPDivide.x;
        }
    }

    //获取经过地形和碰撞调整后的跟随相机的位置
    private Vector3 GetGameCamPosAfterCollionFix(float fHeightOffset)
    {
        var game_cam_pos = CCamCtrlMan.Instance.GetGameCamCtrl().GetCamCurPos();    //根据当前跟随相机的位置参数获取相机回正的原始位置

        //根据地形和相机碰撞体，调整目标相机位置
        Vector3 lookAtPos = GetRealLookAtPos(fHeightOffset);
        Vector3 vDelta = game_cam_pos - lookAtPos;
        Vector3 vTracePos;
        float fDistance;
        var ret = CMapUtil.CameraTrace(CUnityUtil.LayerMaskTerrainCameraCollision, lookAtPos, 0.02f, vDelta, out vTracePos, out fDistance);
        if (ret == CameraTraceResult.HitTerrain)
        {
            if (fDistance > 0.1f)       //避免出现摄像机位置和碰撞位置重合的情况，这时方向会乱
            {
                game_cam_pos = vTracePos;
            }
            else
            {
                vDelta.Normalize();
                game_cam_pos = lookAtPos + vDelta * 0.1f;
            }
        }
        else if (ret == CameraTraceResult.HitCameraCollision)
        {
            if (fDistance > _MinCamCollisionDist)       //避免出现摄像机位置和碰撞位置重合的情况，这时方向会乱
            {
                game_cam_pos = vTracePos;
            }
            else
            {
                vDelta.Normalize();
                game_cam_pos = lookAtPos + vDelta * _MinCamCollisionDist;
            }
        }

        //确保在地面一定高度
        float fHeight = 0.0f;
        if (CMapUtil.GetMapHeight(game_cam_pos, _CameraRadius, out fHeight))
        {
            if (game_cam_pos.y < fHeight + _CameraTerrainHitHeight)
            {
                game_cam_pos.y = fHeight + _CameraTerrainHitHeight;
            }
        }

        return game_cam_pos;
    }

    //更新主角模型是否需要隐藏
    private void UpdateHostPlayerIsNeedHide()
    {
        if (HostPlayerModel == null)
            return;

        Vector3 lootAtPos = GetRealLookAtPos(_CurHeightOffset);
        if ((_CameraTrans.position - lootAtPos).magnitude < 0.7f)
        {
            if (HostPlayerModel.localScale.IsOne())
                HostPlayerModel.localScale = Vector3.zero;
        }
        else
        {
            if (HostPlayerModel.localScale.IsZero())
                HostPlayerModel.localScale = Vector3.one;
        }
    }
    #endregion

    //根据相机拉伸的距离速度确定高度调整速度，确保两个量同时达到目标量
    private void SetHeightOffsetSpeed()
    {
        _HeightOffsetSpeed = Mathf.Abs(_CurHeightOffset - _HeightOffsetDest) / Mathf.Abs(_DistOffset - _DistOffsetDest) * DIST_OFFSET_SPEED;
    }

    #region 相机更新逻辑
    public override bool Tick(float dt)
    {
        if (_CameraTrans == null || LookAtTarget == null || _CurStage != MoveStage.LOOKING_AT)
            return false;

        UpdateCamLookAtPos(dt);
        
        _CameraTrans.position = _CurLookAtPos - _DistOffset * _RealDir;
        _CameraTrans.SetDirAndUp(_RealDir * _DistOffset, _RealUp);

        return true;
    }

    //更新相机视点位置
    protected void UpdateCamLookAtPos(float dt)
    {
        if (LookAtTarget == null)
            return;

        _CurHeightOffset = GetRealHeightOffset();
        _CurLookAtPos = GetRealLookAtPos(_CurHeightOffset);
    }

    //更新视点高度修正
    private void UpdateHeightOffset(float dt)
    {
        if (Utilities.FloatEqual(_HeightOffsetDest, _CurHeightOffset))
            return;

        float delta = _HeightOffsetSpeed * dt;
        if (_HeightOffsetDest > _CurHeightOffset)
            _CurHeightOffset = Mathf.Min(_CurHeightOffset + delta, _HeightOffsetDest);
        else
            _CurHeightOffset = Mathf.Max(_CurHeightOffset - delta, _HeightOffsetDest);
    }

    //提交摄像机的修改
    protected void ApplyDirAndUp()
    {
        _RealDir = GetDirFromPitchAndYaw(_PitchDeg, _YawDeg);
        _RealUp = Vector3.up;
    }

    #endregion 相机更新逻辑

    #region 相机参数改变事件
    public override void OnYawChanged(float dt)
    {
        _YawDeg += dt;
        _YawDeg = ClipYawDegree(_YawDeg);
        ApplyDirAndUp();
    }

    public override void OnPitchChanged(float dt)
    {
        _PitchDeg = ClampPitch(_PitchDeg - dt);
        ApplyDirAndUp();
    }

    public override void OnDistanceChanged(float dt)
    {
        //if (InputManager.Instance.IsMultiDragingStarted) return;
        try
        {
            float temp = _DistOffset + RollSensitivity * dt;
            if (dt < 0f)
            {
                if (_StageCfg._Stage != NearCamStage.Chest && temp <= _StageCfg._DistanceLimit.x)
                {
                    CamMoveNear();
                    return;
                }
            }
            else if (dt > 0f)
            {
                if (temp >= _StageCfg._DistanceLimit.y)
                {
                    if (_StageCfg._Stage == NearCamStage.Body)
                        SetCameraLocation(2, null);
                    else
                        CamMoveFar();
                    return;
                }
            }
            _DistOffset = ClampDistOffset(temp);
        }
        catch(System.Exception e)
        {
            HobaDebuger.LogErrorFormat("CPlayerNearCam OnDistanceChanged Excetion:{0}", e.Message);
        }
    }
    #endregion 相机参数改变事件

    #region 相机效果
    private float _OriginNearClipPlane = 0.5f;
    protected void EnableCameraEffect(bool enable)
    {
        EnableLayerVisible(CUnityUtil.LayerMaskFx, !enable);
        Main.SetTopPatesVisible(!enable);

        EnableDOF(enable);

        var mainCam = Main.Main3DCamera;
        mainCam.DOKill();
        if (enable)
        {
            _OriginNearClipPlane = mainCam.nearClipPlane;
            mainCam.nearClipPlane = 0.01f; // 临时处理，防止模型穿透
        }
        else
            mainCam.nearClipPlane = _OriginNearClipPlane;
    }

    //屏蔽特定Layer
    protected void EnableLayerVisible(int layerMask, bool isVisible)
    {
        var cam = Main.Main3DCamera;
        var curMask = cam.cullingMask;
        var newMask = isVisible ? (curMask | layerMask) : (curMask & (~layerMask));
        cam.cullingMask = newMask;
    }

    //开启头部转向
    public void EnableLookIK(bool enable)
    {
        if (IKComponent == null || HostPlayerHead == null || _CameraTrans == null)
            return;

        if (enable)
        {
            IKComponent.HeadNode = HostPlayerHead;
            IKComponent.LookTarget = _CameraTrans;
            IKComponent.EyeNode = HostPlayerEyes;
            IKComponent.LookAngleLerp = IK_LOOK_ANGLE_LERP;
            IKComponent.LookAngleMax = IK_LOOK_ANGLE_MAX;
        }

        IKComponent.enabled = enable;
    }

    protected void EnableDOF(bool enable)
    {
//         float farValue = 0f;
//         if (enable)
//             farValue = (_StageCfg._DistanceLimit.y + DOF_DISTANCE_OFFSET) / Main.Main3DCamera.farClipPlane;

        DynamicEffectManager.Instance.EnableDepthOfField(enable, DOF_DISTANCE, DOF_RANGE, DOF_BOKEH);
    }
    #endregion 相机效果

    #region 角度计算
    protected Vector3 GetYawDirWithPlayer(Transform target, float yaw_deg)
    {
        float yaw_rad = Mathf.Deg2Rad * (360f - yaw_deg);
        var pf = target.forward;
        Vector3 _Ret = Vector3.zero;
        _Ret.x = pf.x * Mathf.Cos(yaw_rad) - pf.z * Mathf.Sin(yaw_rad);
        _Ret.z = pf.z * Mathf.Cos(yaw_rad) + pf.x * Mathf.Sin(yaw_rad);
        _Ret.y = 0;
        _Ret.Normalize();
        return _Ret;
    }

    protected float GetYawDeg(Vector3 crossPlayer)
    {
        crossPlayer.y = 0f;
        crossPlayer.Normalize();
        float yaw_deg_dest = Mathf.Acos(Vector3.Dot(Vector3.forward, crossPlayer)) * Mathf.Rad2Deg;
        if (crossPlayer.x < 0f)
            yaw_deg_dest = 360f - yaw_deg_dest;

        return yaw_deg_dest;
    }

    protected float GetShowYawDeg(Vector3 vec)
    {
        vec.y = 0.0f;
        vec.Normalize();
        Vector3 forward = LookAtTarget.forward;
        Vector3 cross = Vector3.Cross(forward, vec);
        float yaw_deg = Vector3.Angle(forward, vec);
        return cross.y > 0 ? yaw_deg : 360f - yaw_deg;
    }

    protected float GetDelta(float from, float to, float duration)
    {
        return (to - from) / (duration / Time.deltaTime);
    }

    private float ClampPitch(float value)
    {
        return Mathf.Clamp(value, -_StageCfg._PitchLimit.y, -_StageCfg._PitchLimit.x);
    }

    private float ClampDistOffset(float value)
    {
        return Mathf.Clamp(value, _StageCfg._DistanceLimit.x, _StageCfg._DistanceLimit.y);
    }
    #endregion
}