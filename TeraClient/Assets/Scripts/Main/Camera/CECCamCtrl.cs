using UnityEngine;

public abstract class CECCamCtrl
{
    protected static float _CameraRadius = 0.1f;
    protected static float _CameraTerrainHitHeight = 0.6f;          //摄像机必须在地面的多少高度以上

    protected const float _MinCamHostDist = 0.4f;      //摄像机和角色的最小距离，摄像机保证大于这个距离不隐藏角色
    protected const float _MinCamCollisionDist = 0.5f;

    public Transform _CameraTrans;

    protected int _CtrlType;         //	Camera controller type
    protected Vector3 _RealPos;      //	Real position in world after adjustion
    protected Vector3 _RealDir;      //	Real direction after adjustion
    protected Vector3 _RealUp;
    public float _PitchDeg;       //	Camera pitch degree
    public float _YawDeg;		 //	camera yaw degree

    protected static float DOF_DISTANCE = 3;
    protected static float DOF_RANGE = 20;
    protected static float DOF_BOKEH = 3;

    //TODO: should use CCamCtrlMan.instance.mainCamera
    private Camera _MainCamera;
    public Camera MainCamera
    {
        get { return _MainCamera; }
    }

    public CECCamCtrl()
    {
        _RealPos = Vector3.zero;
        _RealDir = Vector3.right;
        _RealUp = Vector3.up;
    }

    public virtual bool Reset()
    {
        if (_CameraTrans == null)
        {
            Camera main = Main.Main3DCamera ?? Camera.current;
            if (main != null)
            {
                _MainCamera = main;
                _CameraTrans = main.transform.parent;
            }
        }
        return true;
    }

    public virtual bool Tick(float dt) { return true; }
    public virtual void OnYawChanged(float dt) { }
    public virtual void OnPitchChanged(float dt) { }
    public virtual void OnDistanceChanged(float dt) { }
    public virtual void OnPointerMoveStop() { }

    protected float ClipPitchDegree(float fPitchDeg)
    {
        fPitchDeg = Mathf.Clamp(fPitchDeg, -80.0f, 50.0f);
        return fPitchDeg;
    }

    protected float ClipYawDegree(float fYawDeg)
    {
        while (fYawDeg < 0.1f)
            fYawDeg += 360.0f;

        while (fYawDeg > 360.1f)
            fYawDeg -= 360.0f;

        return fYawDeg;
    }

    protected float ClipDegreeDiff(float fYawDiff)
    {
        while (fYawDiff > 180.0f)
            fYawDiff -= 360.0f;

        while (fYawDiff < -180.0f)
            fYawDiff += 360.0f;

        return fYawDiff;
    }

    static Vector3 _Ret = Vector3.zero;
    protected static Vector3 GetDirFromPitchAndYaw(float fPitchDeg, float fYawDeg)
    {
        float fPitch = Mathf.Deg2Rad * fPitchDeg;
        float fYaw = Mathf.Deg2Rad * fYawDeg;
        float fTempDist = 100.0f;
        float fProject = fTempDist * Mathf.Cos(fPitch);

        _Ret.x = fProject * Mathf.Sin(fYaw);
        _Ret.y = fTempDist * Mathf.Sin(fPitch);
        _Ret.z = fProject * Mathf.Cos(fYaw);
        _Ret.Normalize();

        return _Ret;
    }

    public virtual void OnDisable() 
    {
        
    }

    public virtual bool IsAllowManualAdjust() { return true; }
}