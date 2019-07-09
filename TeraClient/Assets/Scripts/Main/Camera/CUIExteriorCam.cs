using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using LuaInterface;
using System.Collections.Generic;

public class CUIExteriorCam : CECCamCtrl
{
    private static float CAMERA_IN_DURATION = 0.6f;
    private static float CAMERA_OUT_DURATION = 0.6f;

    private static float MIN_PITCH = -50f;
    private static float MAX_PITCH = 30f;
    private static float MIN_DISTANCE = 1f;
    private static float MAX_DISTANCE = 12f;
    private static float DIS_OFFSET_SPEED = 3f; //距离拉进速度
    private static float HEIGHT_OFFSET_SPEED = 2f; //上下马高度调整速度

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

    private Vector4 _Params; //x:Yaw y:Pitch z:Distance w:HeightOffset
    private CameraTraceResult _CamTraceResult = CameraTraceResult.NoHit;
    private Vector3 _CurTargetPos;

    private float _MinDistance = MIN_DISTANCE;

    private float _DistOffset = 3f;
    private float _DistOffsetDest;
    private float _HeightOffset;
    private float _CurHeightOffset;
    private bool _IsDistChange = false;

    public enum ExteriorType
    {
        NONE = 0,
        RIDE = 1,   //坐骑
        WING = 2,   //翅膀
        
        // 时装
        Armor = 4,  //服饰
        Helmet = 5, //头饰
        Weapon = 6, //武器
    };
    private ExteriorType _Type = ExteriorType.NONE;

    public CUIExteriorCam()
    {
        _Params = new Vector4(300f, 5f, 3f, 1.25f);
        _DistOffset = _DistOffsetDest = _Params.z;
    }

    public void SetDestParams(float yaw, float pitch, float distance, float height_offset, float min_distance)
    {
        _Params.x = yaw;
        _Params.y = pitch;
        _Params.z = distance;
        _Params.w = height_offset;
        _MinDistance = min_distance;
    }

    enum MoveStage
    {
        NONE = -1,
        MOVING_NEAR = 0,
        LOOKING_AT,
        MOVING_FAR,
    };
    private MoveStage _CurStage = MoveStage.NONE;
    public void SetCameraLocation(int type, LuaFunction callback)
    {
        _CtrlType = type;

        if (_CtrlType == 1)
        {
            if (LookAtTarget == null)
            {
                Common.HobaDebuger.LogError("the 'hang_point' object cant be found in Host Player.");
                return;
            }

            if (_CurStage == MoveStage.MOVING_NEAR) return;

            if (_CurStage == MoveStage.MOVING_FAR)
                _CameraTrans.DOKill();

            CamMoveNear(callback);
        }
        else if (_CtrlType == 2)
        {
            if (_CurStage == MoveStage.MOVING_FAR) return;

            if (_CurStage == MoveStage.MOVING_NEAR)
                _CameraTrans.DOKill();

            _CurTargetPos = GetRealLookPos(CCamCtrlMan.Instance.GetGameCamCtrl().GetHeightOffset());
            var dest_pos = CCamCtrlMan.Instance.GetGameCamCtrl().GetCamCurPos();    //根据当前跟随相机的位置参数获取相机回正的原始位置
            Vector3 game_cam_pos = GetCamPosAfterCollionFix(dest_pos);
            var followTargetPos = CCamCtrlMan.Instance.GetLookAtPosOfGameModel();

            _CameraTrans.DOMove(game_cam_pos, CAMERA_OUT_DURATION).SetEase(Ease.OutCirc).OnUpdate(() =>
            {
                UpdateHostPlayerIsNeedHide();
                _CameraTrans.LookAt(_CurTargetPos, Vector3.up);
            }).OnComplete(() =>
            {
                _Type = ExteriorType.NONE;
                if (callback != null)
                {
                    callback.Call();
                    callback.Release();
                }

                _CurStage = MoveStage.NONE;
            });

            _CurStage = MoveStage.MOVING_FAR;
        }
    }

    private void CamMoveNear(LuaFunction callback = null)
    {
        _CurStage = MoveStage.MOVING_NEAR;

        SetDegAndDistance(_Params.x, _Params.y, _Params.z, _Params.w);
        _CurTargetPos = GetRealLookPos(_HeightOffset);

        var dest_point = _CurTargetPos - _RealDir * _DistOffset;
        dest_point = GetCamPosAfterCollionFix(dest_point);
        _CameraTrans.DOMove(dest_point, CAMERA_IN_DURATION).SetEase(Ease.OutCirc).OnUpdate(() =>
        {
            UpdateHostPlayerIsNeedHide();
            _CameraTrans.LookAt(_CurTargetPos);
        }).OnComplete(() =>
        {
            if (callback != null)
            {
                callback.Call();
                callback.Release();
            }
            
            _CurHeightOffset = _CurTargetPos.y - LookAtTarget.position.y;
            _RealPos = _CameraTrans.position;
            _CamTraceResult = CameraTraceResult.NoHit;
            _CurStage = MoveStage.LOOKING_AT;
        });
    }

    //获取经过地形和碰撞调整后的相机位置
    private Vector3 GetCamPosAfterCollionFix(Vector3 dest_pos)
    {
        var cam_pos = dest_pos;

        //根据地形和相机碰撞体，调整目标相机位置
        Vector3 vDelta = cam_pos - _CurTargetPos;
        Vector3 vTracePos;
        float fDistance;
        var ret = CMapUtil.CameraTrace(CUnityUtil.LayerMaskTerrainCameraCollision, _CurTargetPos, 0.02f, vDelta, out vTracePos, out fDistance);
        if (ret == CameraTraceResult.HitTerrain)
        {
            if (fDistance > 0.1f)       //避免出现摄像机位置和碰撞位置重合的情况，这时方向会乱
            {
                cam_pos = vTracePos;
            }
            else
            {
                vDelta.Normalize();
                cam_pos = _CurTargetPos + vDelta * 0.1f;
            }
        }
        else if (ret == CameraTraceResult.HitCameraCollision)
        {
            if (fDistance > _MinCamCollisionDist)       //避免出现摄像机位置和碰撞位置重合的情况，这时方向会乱
            {
                cam_pos = vTracePos;
            }
            else
            {
                vDelta.Normalize();
                cam_pos = _CurTargetPos + vDelta * _MinCamCollisionDist;
            }
        }

        //确保在地面一定高度
        float fHeight = 0.0f;
        if (CMapUtil.GetMapHeight(cam_pos, _CameraRadius, out fHeight))
        {
            if (cam_pos.y < fHeight + _CameraTerrainHitHeight)
            {
                cam_pos.y = fHeight + _CameraTerrainHitHeight;
            }
        }

        return cam_pos;
    }

    //更新主角模式是否需要隐藏
    private void UpdateHostPlayerIsNeedHide()
    {
        if (HostPlayerModel == null)
            return;

        if ((_CameraTrans.position - _CurTargetPos).magnitude < 0.3f)
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

    //根据角度设置相机参数
    public void SetDegAndDistance(float yaw_deg, float pitch_deg, float distance, float height)
    {
        var cross2 = GetYawDirWithPlayer(yaw_deg);
        _YawDeg = GetYawDeg(-cross2);
        _PitchDeg = -pitch_deg;
        _DistOffset = _DistOffsetDest = distance;
        _HeightOffset = height;
        ApplyDirAndUp();
    }

    public void MoveCamToDest()
    {
        if (LookAtTarget == null)
        {
            Common.HobaDebuger.LogError("the 'hang_point' object cant be found in Host Player.");
            return;
        }

        //if (_CurStage == MoveStage.MOVING_NEAR) return;

        if (_CurStage == MoveStage.MOVING_FAR || _CurStage == MoveStage.MOVING_NEAR)
            _CameraTrans.DOKill();

        CamMoveNear();
    }

    protected Vector3 GetRealLookPos(float fHeightOffset)
    {
        if (LookAtTarget == null)
            return Vector3.zero;

        var lookAtDest = LookAtTarget.position;
        lookAtDest.y += + fHeightOffset;
        return lookAtDest;
    }

    public void SetHeightOffset(float fHeightOffset)
    {
        _HeightOffset = fHeightOffset;
    }

    #region 角度计算
    //角度求向量
    private Vector3 _Ret = Vector3.zero;
    private Vector3 GetYawDirWithPlayer(float yaw_deg)
    {
        float yaw_rad = Mathf.Deg2Rad * (360f - yaw_deg);
        pf = Main.HostPalyer.forward;
        _Ret.x = pf.x * Mathf.Cos(yaw_rad) - pf.z * Mathf.Sin(yaw_rad);
        _Ret.z = pf.z * Mathf.Cos(yaw_rad) + pf.x * Mathf.Sin(yaw_rad);
        _Ret.y = 0;
        _Ret.Normalize();
        return _Ret;
    }

    //向量求角度
    private float GetYawDeg(Vector3 crossPlayer)
    {
        crossPlayer.y = 0.0f;
        crossPlayer.Normalize();
        float yaw_deg_dest = Mathf.Acos(Vector3.Dot(Vector3.forward, crossPlayer)) * Mathf.Rad2Deg;
        if (crossPlayer.x < 0.0f)
            yaw_deg_dest = 360.0f - yaw_deg_dest;

        return yaw_deg_dest;
    }
    #endregion 角度计算
    
    public override bool Tick(float dt)
    {
        if (_CurStage != MoveStage.LOOKING_AT || LookAtTarget == null)
            return false;
        
        UpdateCameraHeightOffset(dt);
        UpdateCameraDistOffset(dt);
        bool isFix = CollisionFix(CUnityUtil.LayerMaskTerrainCameraCollision);

        if (!isFix)
            _RealPos = _CurTargetPos - _DistOffset * _RealDir;
        MapHeightFix();

        UpdateHostPlayerIsNeedHide();

        _CameraTrans.position = _RealPos;
        _CameraTrans.SetDirAndUp(_RealDir * _DistOffset, _RealUp);
        return true;
    }
    
    //提交摄像机的修改
    protected void ApplyDirAndUp()
    {
        _RealDir = GetDirFromPitchAndYaw(_PitchDeg, _YawDeg);
        _RealUp = Vector3.up;
    }

    #region 相机更新逻辑
    //上马高度改变缓慢上扬
    protected void UpdateCameraHeightOffset(float dt)
    {
        if (_CameraTrans == null || LookAtTarget == null) return;

        UpdateHeightOffset(dt);
        _CurTargetPos.y = _CurHeightOffset + LookAtTarget.position.y;
    }

    private void UpdateHeightOffset(float dt)
    {
        if (_HeightOffset == _CurHeightOffset)
            return;

        float delta = HEIGHT_OFFSET_SPEED * dt;
        if (_HeightOffset > _CurHeightOffset)
            _CurHeightOffset = Mathf.Min(_CurHeightOffset + delta, _HeightOffset);
        else
            _CurHeightOffset = Mathf.Max(_CurHeightOffset - delta, _HeightOffset);
    }

    //相机被拉进时的调整
    protected void UpdateCameraDistOffset(float dt)
    {
        float offsetTickLength = DIS_OFFSET_SPEED * dt;
        if (_DistOffsetDest - _DistOffset > offsetTickLength)
            _DistOffset += offsetTickLength;
        else if (_DistOffsetDest - _DistOffset < -offsetTickLength)
            _DistOffset -= offsetTickLength;
        else
        {
            _DistOffset = _DistOffsetDest;
        }
    }

    private float dist_before_hit = MIN_DISTANCE;
    //相机遇遮挡时拉进的设置
    private bool CollisionFix(int layerMask)
    {
        CameraTraceResult bLastCollide = _CamTraceResult;
        if (bLastCollide == CameraTraceResult.NoHit || _IsDistChange)
        {
            dist_before_hit = _DistOffset;
            _IsDistChange = false;
        }
        
        Vector3 vDelta = -dist_before_hit * _RealDir;
        bool isFix = false;

        Vector3 vTracePos;
        float fDistance;
        _CamTraceResult = CMapUtil.CameraTrace(layerMask, _CurTargetPos, 0.02f, vDelta, out vTracePos, out fDistance);
        if (_CamTraceResult == CameraTraceResult.HitTerrain)
        {
            if (fDistance > 0.1f)       //避免出现摄像机位置和碰撞位置重合的情况，这时方向会乱
            {
                _RealPos = vTracePos;
            }
            else
            {
                vDelta.Normalize();
                _RealPos = _CurTargetPos + vDelta * 0.1f;
            }
            _DistOffsetDest = fDistance;
            isFix = true;
        }
        else if (_CamTraceResult == CameraTraceResult.HitCameraCollision)
        {
            if (fDistance > _MinCamCollisionDist)       //避免出现摄像机位置和碰撞位置重合的情况，这时方向会乱
            {
                _RealPos = vTracePos;
            }
            else
            {
                vDelta.Normalize();
                _RealPos = _CurTargetPos + vDelta * _MinCamCollisionDist;
            }
        }
        else
        {
            if (bLastCollide != CameraTraceResult.NoHit)    //从有碰撞到无碰撞
            {
                _DistOffset = _DistOffsetDest = dist_before_hit;
            }
        }

        return isFix;
    }

    //确保在地面一定高度
    private void MapHeightFix()
    {
        float fHeight = 0.0f;
        if (CMapUtil.GetMapHeight(_RealPos, _CameraRadius, out fHeight))
        {
            if (_RealPos.y < fHeight + _CameraTerrainHitHeight)
            {
                _RealPos.y = fHeight + _CameraTerrainHitHeight;
            }
        }
    }
    #endregion 相机更新逻辑

    #region 相机参数改变事件
    public override bool IsAllowManualAdjust() { return _CurStage == MoveStage.LOOKING_AT; }

    public override void OnYawChanged(float dt)
    {
        _YawDeg += dt;
        _YawDeg = ClipYawDegree(_YawDeg);
        ApplyDirAndUp();
    }
    public override void OnPitchChanged(float dt)
    {
        _PitchDeg -= dt;
        _PitchDeg = Mathf.Clamp(_PitchDeg, MIN_PITCH, MAX_PITCH);
        _PitchDeg = ClipPitchDegree(_PitchDeg);
        ApplyDirAndUp();
    }
    public override void OnDistanceChanged(float dt)
    {
        _DistOffset += dt * EntryPoint.Instance.PlayerFollowCameraConfig.CamRollSensitivity;
        _DistOffset = Mathf.Clamp(_DistOffset, _MinDistance, MAX_DISTANCE);
        _DistOffsetDest = _DistOffset;
        //if (dt < 0f)
            _IsDistChange = true;
    }
    #endregion 相机参数改变事件


    #region 策划调试用，之后删
    /*--------------------------策划调试用，之后删--------------------------------*/
    private InputField _Yaw_InputField;
    private InputField _Pitch_InputField;
    private InputField _Dist_InputField;
    private InputField _Height_InputField;
    private Vector3 cross;
    private Vector3 pf;

    private float ShowYawDeg(Vector3 vec)
    {
        vec.y = 0.0f;
        vec.Normalize();
        pf = Main.HostPalyer.forward;
        cross = Vector3.Cross(pf, vec);
        float yaw_deg = Vector3.Angle(pf, vec);
        return cross.y > 0 ? yaw_deg : 360f-yaw_deg;
    }

    private GameObject inputField;
    private void FindInputField()
    {
        if (_Yaw_InputField == null)
        {
            inputField = GameObject.Find("InputField_Yaw");
            if (inputField != null)
                _Yaw_InputField = inputField.GetComponent<InputField>();
        }
        if (_Pitch_InputField == null)
        {
            inputField = GameObject.Find("InputField_Pitch");
            if (inputField != null)
                _Pitch_InputField = inputField.GetComponent<InputField>();
        }
        if (_Dist_InputField == null)
        {
            inputField = GameObject.Find("InputField_Dist");
            if (inputField != null)
                _Dist_InputField = inputField.GetComponent<InputField>();
        }
        if (_Height_InputField == null)
        {
            inputField = GameObject.Find("InputField_Height");
            if (inputField != null)
                _Height_InputField = inputField.GetComponent<InputField>();
        }
    }
    
    private void ShowData()
    {
        if (inputField == null)
            FindInputField();

        if (_Yaw_InputField != null)
        {
            float yaw_deg = ShowYawDeg(-_RealDir);
            _Yaw_InputField.text = yaw_deg.ToString();
        }
        if (_Pitch_InputField != null)
        {
            float pitch_deg = -_PitchDeg;
            _Pitch_InputField.text = pitch_deg.ToString();
        }
        if (_Dist_InputField != null)
        {
            _Dist_InputField.text = _DistOffset.ToString();
        }
        if (_Height_InputField != null)
        {
            _Height_InputField.text = _CurHeightOffset.ToString();
        }
    }

    private const float yaw_adjust = 1f;
    private const float pitch_adjust = 0.5f;
    private const float dist_adjust = 0.5f;
    private const float height_adjust = 0.1f;
    //调整摄像机
    public void AddOrSubForTest(int type, bool isAdd)
    {
        float adjust;
        switch (type)
        {
            case 1:
                adjust = isAdd ? yaw_adjust : -yaw_adjust;
                _YawDeg += adjust;
                _YawDeg = ClipYawDegree(_YawDeg);
                ApplyDirAndUp();
                break;
            case 2:
                adjust = isAdd ? pitch_adjust : -pitch_adjust;
                _PitchDeg += adjust;
                _PitchDeg = ClipPitchDegree(_PitchDeg);
                ApplyDirAndUp();
                break;
            case 3:
                adjust = isAdd ? dist_adjust : -dist_adjust;
                _DistOffset += adjust;
                _DistOffsetDest += adjust;
                ApplyDirAndUp();
                break;
            case 4:
                adjust = isAdd ? height_adjust : -height_adjust;
                _CurHeightOffset = _HeightOffset += adjust;
                break;
            default:
                return;
        }
        ShowData();
    }
    #endregion 策划调试用，之后删

    public override void OnDisable()
    {
        inputField = null;
        if (_CurStage == MoveStage.MOVING_NEAR || _CurStage == MoveStage.MOVING_FAR)
        {
            if (_CameraTrans != null)
                _CameraTrans.DOKill();
        }
    }
}