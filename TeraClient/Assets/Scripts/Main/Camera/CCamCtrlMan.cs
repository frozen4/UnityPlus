using Common;
using UnityEngine;
using System;
using DG.Tweening;
using LuaInterface;

public enum CAM_CTRL_MODE
{
    INVALID = 0,
    LOGIN,
    GAME,
    CG,
    NPC,
    DUNGEON,
    EXTERIOR,
    NEAR,
    BOSS,
    SKILLACT,
    SCENEDIALOG,
};

public class CCamCtrlMan : Singleton<CCamCtrlMan>, GameLogic.ITickLogic
{
    public enum ADJUST_STYLE
    {
        NONE = -1,
        PITCH = 0, // 俯仰角
        YAW,
        DISTANCE, // 到焦点距离
    }
    private CECCamCtrl _CurCamCtrl = null; // Current camera controller
    private CLoginCamCtrl _LoginCamCtrl = new CLoginCamCtrl(); // Login camera controller
    private CPlayerFollowCam _GameCamCtrl = new CPlayerFollowCam(); // Game camera controller
    private CPlayerFollowCam _CGCamCtrl = null; // CG camera controller
    private CNpcDialogueCam _NpcCamCtrl = new CNpcDialogueCam();
    private CDungeonEndCam _DungeonCamCtrl = new CDungeonEndCam();
    private CUIExteriorCam _UIExteriorCamCtrl = new CUIExteriorCam();
    private CPlayerNearCam _NearCamCtrl = new CPlayerNearCam();
    private CBossActionCam _BossCamCtrl = new CBossActionCam();
    private CSKillActionCam _SkillActCamCtrl = new CSKillActionCam();
    private CNpcSceneDialogCam _NpcDlgCamCtrl = new CNpcSceneDialogCam();

    private Camera _MainCamera = null;
    private Vector3 _MainCameraDir = Vector3.forward;
    private Vector3 _MainCameraRight = Vector3.right;
    private CAM_CTRL_MODE _CurCamType = CAM_CTRL_MODE.INVALID;
    private bool _IsTransToPortal = false;
    private float _CurProDefaultSpeed = 0f; //当前职业初始移动速度
    //private Plane[] _MainCameraPlanes = null;

    public Camera MainCamera
    {
        get { return _MainCamera; }
    }

    public Vector3 MainCameraDir
    {
        get { return _MainCameraDir; }
    }

    public Vector3 MainCameraRight
    {
        get { return _MainCameraRight; }
    }

    //特殊的飞行传送状态，这时摄像机跟随模型参数需要特殊设置
    public bool IsTransToPortal
    {
        get { return _IsTransToPortal; }
        set { _IsTransToPortal = value; }
    }

    //public Plane[] MainCameraPlanes
    //{
    //    get { return _MainCameraPlanes; }
    //}

    public void SetNpcInfo(Vector3 vec)
    {
        _NpcCamCtrl.SetDestDir(vec);
    }

    public float CurProDefaultSpeed
    {
        get { return _CurProDefaultSpeed; }
        set { _CurProDefaultSpeed = value; }
    }

    public void Reset()         //回到初始状态
    {
        if (_CurCamCtrl != null) _CurCamCtrl.OnDisable();

        _CurCamCtrl = null; // Current camera controller
        _LoginCamCtrl = new CLoginCamCtrl(); // Login camera controller
        _GameCamCtrl = new CPlayerFollowCam(); // Game camera controller
        _CGCamCtrl = null; // CG camera controller
        _NpcCamCtrl = new CNpcDialogueCam();
        _DungeonCamCtrl = new CDungeonEndCam();
        _UIExteriorCamCtrl = new CUIExteriorCam();
        _NearCamCtrl = new CPlayerNearCam();
        _BossCamCtrl = new CBossActionCam();
        _SkillActCamCtrl = new CSKillActionCam();
    }

    public void SetCurCamCtrl(CAM_CTRL_MODE ctrlType, bool isReset, GameObject target = null, int type = 1, LuaFunction callback = null)
    {
        //if (_CurCamType == ctrlType) return;

        if (_CurCamCtrl != null) _CurCamCtrl.OnDisable();

        switch (ctrlType)
        {
            case CAM_CTRL_MODE.INVALID:
                _CurCamCtrl = null;
                break;
            case CAM_CTRL_MODE.LOGIN:
                _CurCamCtrl = _LoginCamCtrl;
                break;
            case CAM_CTRL_MODE.GAME:
                _CurCamCtrl = _GameCamCtrl;
                _GameCamCtrl.ResetCameraMode();
                break;
            case CAM_CTRL_MODE.CG:
                _CurCamCtrl = _CGCamCtrl;
                break;
            case CAM_CTRL_MODE.NPC:
                _CurCamCtrl = _NpcCamCtrl;
                _NpcCamCtrl.Reset();
                _NpcCamCtrl.SetCameraLocation(target, type, callback);
                break;
            case CAM_CTRL_MODE.DUNGEON:
                _CurCamCtrl = _DungeonCamCtrl;
                _DungeonCamCtrl.Reset();
                _DungeonCamCtrl.SetCameraLocation(target, type, callback);
                break;
            case CAM_CTRL_MODE.EXTERIOR:
                _CurCamCtrl = _UIExteriorCamCtrl;
                _UIExteriorCamCtrl.Reset();
                _UIExteriorCamCtrl.SetCameraLocation(type, callback);
                break;
            case CAM_CTRL_MODE.NEAR:
                _CurCamCtrl = _NearCamCtrl;
                _NearCamCtrl.Reset();
                _NearCamCtrl.SetCameraLocation(type, callback);
                break;
            case CAM_CTRL_MODE.BOSS:
                _CurCamCtrl = _BossCamCtrl;
                _BossCamCtrl.Reset();
                _BossCamCtrl.Start(target);
                break;
            case CAM_CTRL_MODE.SKILLACT:
                _CurCamCtrl = _SkillActCamCtrl;
                _SkillActCamCtrl.Reset();
                _SkillActCamCtrl.Start(target);
                break; 
            case CAM_CTRL_MODE.SCENEDIALOG:
                _CurCamCtrl = _NpcDlgCamCtrl;
                _NpcDlgCamCtrl.Reset();
                _NpcDlgCamCtrl.Start();
                break;
            default:
                return;
        }

        _CurCamType = ctrlType;
        if (_CurCamCtrl != null && isReset)
            _CurCamCtrl.Reset();
    }

    public void Tick(float dt)
    {
        //更新camera
        //_MainCamera = (Camera.main != null) ? Camera.main : Camera.current;
        _MainCamera = Main.Main3DCamera;
        if (_MainCamera != null)
        {
            var camTrans = _MainCamera.transform;
            _MainCameraDir = camTrans.forward;
            _MainCameraRight = camTrans.right;

            //_MainCameraPlanes = GeometryUtility.CalculateFrustumPlanes(_MainCamera);
        }

        if (_CurCamCtrl != null)
            _CurCamCtrl.Tick(dt);
    }
    public void ManualAdjustCamera(ADJUST_STYLE type, float delta)
    {
        if (_CurCamCtrl == null || !_CurCamCtrl.IsAllowManualAdjust())
            return;

        if (type == ADJUST_STYLE.YAW)
            _CurCamCtrl.OnYawChanged(delta);
        else if (type == ADJUST_STYLE.PITCH)
            _CurCamCtrl.OnPitchChanged(delta);
        else if (type == ADJUST_STYLE.DISTANCE)
            _CurCamCtrl.OnDistanceChanged(delta);
        else if (type == ADJUST_STYLE.NONE)
            _CurCamCtrl.OnPointerMoveStop();
    }
    public CAM_CTRL_MODE GetCurCamCtrlMode()
    {
        if (_CurCamCtrl == null)
            return CAM_CTRL_MODE.INVALID;

        if (_CurCamCtrl == _CGCamCtrl)
            return CAM_CTRL_MODE.CG;
        else if (_CurCamCtrl == _GameCamCtrl)
            return CAM_CTRL_MODE.GAME;
        else if (_CurCamCtrl == _LoginCamCtrl)
            return CAM_CTRL_MODE.LOGIN;
        else if (_CurCamCtrl == _NpcCamCtrl)
            return CAM_CTRL_MODE.NPC;
        else if (_CurCamCtrl==_NpcDlgCamCtrl)
            return CAM_CTRL_MODE.SCENEDIALOG;
        else if (_CurCamCtrl == _DungeonCamCtrl)
            return CAM_CTRL_MODE.DUNGEON;
        else if (_CurCamCtrl == _UIExteriorCamCtrl)
            return CAM_CTRL_MODE.EXTERIOR;
        else if (_CurCamCtrl == _NearCamCtrl)
            return CAM_CTRL_MODE.NEAR;
        else if (_CurCamCtrl == _BossCamCtrl)
            return CAM_CTRL_MODE.BOSS;
        else if (_CurCamCtrl == _SkillActCamCtrl)
            return CAM_CTRL_MODE.SKILLACT;

        return CAM_CTRL_MODE.INVALID;
    }
    public CPlayerFollowCam GetGameCamCtrl()
    {
        return _GameCamCtrl;
    }

    public CPlayerNearCam GetNearCamCtrl()
    {
        return _NearCamCtrl;
    }

    public void SetStretchingParams(float wanted_dis_factor, float fadein_time, float stay_time, float fadeout_time)
    {
        _GameCamCtrl.SetStretchingParams(wanted_dis_factor, fadein_time, stay_time, fadeout_time);
    }

    public void StopCameraStretching()
    {
        _GameCamCtrl.StopCameraStretching();
    }

    public void RecoverCamFaceForward()
    {
        _GameCamCtrl.SetToDefaultCamera(true, true, false, false);
    }

    public void QuickRecoverCamToDest(float x, float z)
    {
        _GameCamCtrl.RecoverCameraBySkill(x, z);
    }

    public void DragScreenStart()
    {
        _GameCamCtrl.DragStart();
    }
    public void DragScreenEnd()
    {
        _GameCamCtrl.DragEnd();
    }

    public Vector3 GetLookAtPosOfGameModel()
    {
        return _GameCamCtrl.GetRealLookPos(_GameCamCtrl.GetHeightOffset());
    }

    public bool IsGameObjectInCamera(Transform transform, Vector3 size)
    {
        if (transform == null || _MainCamera == null)
            return false;

        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(_MainCamera);
        if (planes == null)
            return false;

        Vector3 center = transform.position;
        center.y += size.y * 0.5f;
        Bounds bounds = new Bounds(center, size);
        return GeometryUtility.TestPlanesAABB(planes, bounds);
    }

    //测试用
    public void AddOrSubForTest(int type, bool isAdd)
    {
        if (_CurCamType == CAM_CTRL_MODE.EXTERIOR)
            _UIExteriorCamCtrl.AddOrSubForTest(type, isAdd);
    }

    //测试用
    public void SetExteriorDebugParams(float yaw_deg, float pitch_deg, float dist, float height)
    {
        if (_CurCamType == CAM_CTRL_MODE.EXTERIOR)
            _UIExteriorCamCtrl.SetDegAndDistance(yaw_deg, pitch_deg, dist, height);
    }

    public void SetExteriorCamParams(float yaw_deg, float pitch_deg, float dist, float height, float min_distance)
    {
        if (_UIExteriorCamCtrl != null)
            _UIExteriorCamCtrl.SetDestParams(yaw_deg, pitch_deg, dist, height, min_distance);
        if (_CurCamType == CAM_CTRL_MODE.EXTERIOR)
            _UIExteriorCamCtrl.MoveCamToDest();
    }

    //设置外观相机的坐骑页签的视点高度
    public void SetExteriorCamHeightOffset(float fHeightOffset)
    {
        if (_UIExteriorCamCtrl != null)
            _UIExteriorCamCtrl.SetHeightOffset(fHeightOffset);
    }

    //设置副本结算相机的参数
    public void SetDungeonCamParam(Vector3 dest_pos, Vector3 dest_rot, float dest_fov)
    {
        _CurCamCtrl = _DungeonCamCtrl;
        _DungeonCamCtrl.Reset();
        _DungeonCamCtrl.SetParam(dest_pos, dest_rot, dest_fov);
    }

    //开始Boss相机移动
    public void StartBossCamMove(LuaFunction callback, float duration = 0)
    {
        if (_CurCamType == CAM_CTRL_MODE.BOSS)
            //_BossCamCtrl.StartRecover(callback);
            _BossCamCtrl.StartRecoverByTween(callback, duration);
    }

    public void SetSkillActInfo(float angle, float offset, float duration, float ori_angle/*, SKILL_CAM_ENUM type*/)
    {
        if(null != _SkillActCamCtrl)
        {
            _SkillActCamCtrl.SetData(angle, offset, duration, ori_angle/*, type*/);
        }
    }

}