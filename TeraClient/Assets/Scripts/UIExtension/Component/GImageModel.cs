using UnityEngine;
using System;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

public class GImageModel : RawImage, IDragHandler, IPointerClickHandler, IPointerDownHandler
{
    const float STAGE_DIST = 5;
    const float STAGE_Y_POS = -1.1f;
    const float VIEW_DIST = 100;
    const float DEFAULT_CAM_SIZE = 1.2f;

    private static bool _ResourceInited = false;
    private const int _MAX_CAM_COUNT = 12;
    private static GImageModel[] _ActiveCameras = new GImageModel[_MAX_CAM_COUNT];

    public enum FIT_MODE
    {
        None,
        Width,
        Height,
    }
    public FIT_MODE FitMode;

    public enum GROUND_TYPE
    {
        None,
        Water,
        Shadow
    }

    #region serialization
    [NoToLua]
    public bool UsePostEffect;
    //private bool _IsPPRunning;
    [NoToLua]
    public bool FlipX;
    [NoToLua]
    public GROUND_TYPE GroundType;
    [NoToLua]
    public bool FlipMaskX;

    #endregion serialization

    [NonSerialized]
    public Action<GameObject> OnModelClick;

    private int _ID = -1;
    private Camera _UICamera;
    //private RenderTexture _RenderTexture;
    private Transform _UICameraT;
    private Transform _ModelT;
    private Transform _GroundT;
    private Transform _StageT;
    private Transform _PlatformT;

    private Transform _IMRootT;

    private float _CameraDist = STAGE_DIST;
    private float _GroundOffsetY = 0f;
    private float _GroundRotX = 0f;

    private PostProcessChain _PPChain;
    private PostProcessChain _PPChainRef;

    private bool _IsDestroyed;

    private int NUM_MAX_RT = 2;
    private int NUM_MAX_CAM = 5;
    //private float RT_POOL_TIME = 180;
    private float CAM_POOL_TIME = 180;

    //public class ImageModelConfig
    //{
    //    public string ModelPath = "";
    //    public Vector3 Offset = Vector3.zero;
    //    public Vector3 Rot = Vector3.zero;
    //    public Vector3 MOffset = Vector3.zero;
    //    public float Size = 0.0f;
    //    public float CameraRotX = 0.0f;
    //    public bool Shadow = false;
    //    //public Vector3 GroundOffset = Vector3.zero;
    //    public Vector3 StageParam = Vector3.zero;
    //}


    public bool HasModel
    {
        get { return _ModelT != null; }
    }

    //private Vector3 _CameraLocalOffset { get { return new Vector3(0, 0, -MODEL_DIST); } }

    protected override void Start()
    {
        base.Start();

        NUM_MAX_RT = EntryPoint.Instance.GameCustomConfigParams.GImageModelRTNum;
        NUM_MAX_CAM = EntryPoint.Instance.GameCustomConfigParams.GImageModelCameraNum;
        //RT_POOL_TIME = EntryPoint.Instance.GameCustomConfigParams.GImageModelRTPoolTime;
        CAM_POOL_TIME = EntryPoint.Instance.GameCustomConfigParams.GImageModelCameraPoolTime;
        UpdateImageVisibility();

        FitGraph();
    }

    /// <summary>
    /// Adjust the scale of the Graphic but keep ratio.
    /// </summary>

    [NoToLua]
    public static Rect FitSize(GImageModel g, FIT_MODE fit)
    {
        Rect rect = g.rectTransform.rect;
        Rect rv = new Rect(0, 0, 1, 1);

        if (fit == FIT_MODE.Height && rect.height != 0)
        {
            float asp = (float)rect.width / (float)rect.height;
            rv.width = asp;
            rv.x = 0.5f - asp * 0.5f;
        }

        if (fit == FIT_MODE.Width && rect.width != 0)
        {
            float asp = (float)rect.height / (float)rect.width;
            rv.height = asp;
            rv.y = 0.5f - asp * 0.5f;
        }

        if (g.FlipX)
        {
            rv.width = -rv.width;
            rv.x = 1 - rv.x;
        }

        return rv;
    }

    [NoToLua]
    public static Rect FitUV(GImageModel g, FIT_MODE fit)
    {
        Rect rect = g.rectTransform.rect;
        Rect rv = new Rect(0, 0, 1, 1);

        if (fit == FIT_MODE.Height && rect.width != 0)
        {
            float inv_asp = (float)rect.height / (float)rect.width;
            rv.width = inv_asp;
            rv.x = (1 - inv_asp) * 0.5f;
        }

        if (fit == FIT_MODE.Width && rect.height != 0)
        {
            float inv_asp = (float)rect.width / (float)rect.height;
            rv.height = inv_asp;
            rv.y = (1 - inv_asp) * 0.5f;
        }

        if (g.FlipX)
        {
            rv.width = -rv.width;
            rv.x = 1 - rv.x;
        }

        return rv;
    }

    public void FitGraph()
    {
        this.uvRect = GImageModel.FitSize(this, FitMode);

        if (this.material != null && this.material.HasProperty(ShaderIDs.UIAlphaTex))
        {
            Rect rv = GImageModel.FitUV(this, FitMode);
            this.material.SetTextureOffset(ShaderIDs.UIAlphaTex, new Vector2(FlipMaskX ? 1 - rv.x : rv.x, rv.y));
            this.material.SetTextureScale(ShaderIDs.UIAlphaTex, new Vector2(FlipMaskX ? -rv.width : rv.width, rv.height));
        }
    }

    //protected void Update()
    //{
    //    if (enabled && usePostEffect && _isPPRunning)
    //    {
    //        UpdatePP();
    //    }
    //}

    #region IF

    public void SetColor(float r, float g, float b)
    {
        Color c = this.color;
        c.r = r;
        c.g = g;
        c.b = b;
        this.color = c;
    }

    public void SetModel(GameObject obj_model)
    {
        if (IsDestroyed()) { return; }

        if (obj_model == null)
        {
            Release();
        }
        else
        {
            if (_ModelT == null || obj_model != _ModelT.gameObject)
            {
#if !ART_USE
                SafeInit();
#endif

                if (!_ResourceInited)
                {
                    Debug.LogError("IM: Failed to init resources, all functions disabled!");
                    return;
                }

                if (_UICamera == null)
                {
                    CreateCameraAndRenderTexture();
                    if (_UICamera == null) return;
                }

                //if (_ModelT != null)
                //{
                //    _ModelT.SetParent(null, false);

                //    Debug.LogWarning("IMageModel Release Model 3");
                //    LuaScriptMgr.Instance.CallOnTraceBack();

                //    //Debug.LogError("IM Loading multiple Models!" + name);
                //    //LuaScriptMgr.Instance.CallOnTraceBack();
                //    //DestroyImmediate(_ModelT.gameObject);
                //    //Debug.LogWarning("_ModelT not clear");
                //    //_ModelT = null;

                //    //Debug.LogWarning("IM Loading Models" + name);
                //    //LuaScriptMgr.Instance.CallOnTraceBack();
                //}
                UnLoadModel();

                //设置旋转
                _ModelT = obj_model.transform;
                _ModelT.SetParent(_PlatformT, false);
#if !ART_USE
                //修改layer
                Util.SetLayerRecursively(_ModelT.gameObject, LayerMask.NameToLayer("UI"));
#endif

                UpdateStagePos(new Vector3(0, STAGE_Y_POS, 0));
                UpdateModelRot(new Vector3(0, 0, 0));
            }
        }

        UpdateImageVisibility();
    }

    public void UnLoadModel()
    {
        if (_ModelT != null)
        {
            if (_ModelT.parent == _PlatformT)
            {
                _ModelT.SetParent(null, false);
            }
            _ModelT = null;
        }
    }

    public void Release()
    {
        UnLoadModel();
        ClearCameraAndRenderTexture();
    }

    public void SetCameraSize(float camSize)
    {
        if (_UICamera != null)
        {
            if (_UICamera.orthographic)
            {
                _UICamera.orthographicSize = camSize;
            }
            else
            {
                _UICamera.fieldOfView = camSize;
            }
        }
    }

    ////set stage x y
    //public void SetLookAtPosXY(float posy, float posx = 0f)
    //{
    //    if (_StageT != null)
    //    {
    //        Vector3 l_pos = _StageT.localPosition;
    //        l_pos.x = posx;
    //        l_pos.y = posy;
    //        UpdateStagePos(l_pos);
    //    }
    //}

    ////set stage z
    //public void SetStageZ(float z_dist)
    //{
    //    if (_StageT != null)
    //    {
    //        Vector3 l_pos = _StageT.localPosition;
    //        l_pos.z = z_dist;
    //        UpdateStagePos(l_pos);
    //    }
    //}

    public void SetLookAtPos(float posx, float posy, float posz)
    {
        UpdateStagePos(new Vector3(posx, posy, posz));
    }

    //set model x
    public void FixModelAxisX(float offset_x)
    {
        UpdateModelPos(new Vector3(offset_x, 0, 0));
    }
    //rot model y
    public void SetLookAtRot(float rotx, float roty, float rotz)
    {
        UpdateModelRot(new Vector3(rotx, (FlipX ? -roty : roty), (FlipX ? -rotz : rotz)));
    }
    //rot camera xyz
    public void SetCameraAngle(float x, float y, float z)
    {
        UpdateCameraRot(new Vector3(x, y, z));
        UpdateCameraPos();
    }
    //set camera dist
    public void SetCameraDist(float z_dist)
    {
        _CameraDist = STAGE_DIST + z_dist;
        UpdateCameraPos();
    }

    //set groud offset y, rot x
    public void SetGroundOffset(float g_offY, float g_rotX)
    {
        _GroundOffsetY = g_offY;
        _GroundRotX = g_rotX;
        //shadow
        if (_GroundT != null)
        {
            //Vector3 l_pos = new Vector3();
            //if (_ModelT != null)
            //{
            //    l_pos = _ModelT.localPosition;
            //}
            //l_pos.y += _GroundOffset;
            _GroundT.localPosition = new Vector3(0, _GroundOffsetY, 0);
            _GroundT.localRotation = Quaternion.Euler(new Vector3(0, _GroundRotX, 0));
        }
    }

    public void SetCameraType(bool is_2D)
    {
        if (_UICamera != null)
        {
            _UICamera.orthographic = is_2D;
        }
    }

    public void SetCameraFarClip(float far_plane)
    {
        if (_UICamera != null)
        {
            _UICamera.farClipPlane = far_plane;
        }
    }

    public void ShowGround(bool is_show)
    {
        if (_GroundT != null)
        {
            _GroundT.gameObject.SetActive((GroundType != GROUND_TYPE.None) && is_show);
        }
    }

    public void AlignSystemWithModelForward(Vector3 dir)
    {
        dir.y = 0;
        if (_UICamera != null && dir.sqrMagnitude > 0.0001f)
        {
            Quaternion rot = Quaternion.LookRotation(dir, _IMRootT.up);
            _IMRootT.rotation = rot;
        }
    }

    [NoToLua]
    public void OnDrag(PointerEventData eventData)
    {
        if (_PlatformT != null && _ModelT != null)
        {
            _PlatformT.Rotate(Vector3.down * eventData.delta.x * EntryPoint.Instance.PlayerFollowCameraConfig.UIModelRotateSensitivity, Space.World);
        }

    }

    [NoToLua]
    public void OnPointerDown(PointerEventData eventData)
    {
    }

    [NoToLua]
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.dragging || _ModelT == null) return;

        if (OnModelClick != null)
        {
            OnModelClick(gameObject);
        }
    }


    #endregion IF

    private void UpdateCameraRot(Vector3 local_rot)
    {
        if (_UICameraT != null)
        {
            _UICameraT.localRotation = Quaternion.Euler(local_rot);
        }
    }

    private void UpdateCameraPos()
    {
        if (_UICameraT != null)
        {
            _UICameraT.localPosition = _UICameraT.localRotation * new Vector3(0, 0, -_CameraDist);
        }
    }

    private void UpdateStagePos(Vector3 local_pos)
    {
        if (_StageT != null)
        {
            _StageT.localPosition = local_pos;
        }
    }

    private void UpdateModelPos(Vector3 local_pos)
    {
        if (_ModelT != null)
        {
            _ModelT.localPosition = local_pos;
        }
    }

    private void UpdateModelRot(Vector3 local_rot)
    {
        if (_ModelT != null)
        {
            _ModelT.localRotation = Quaternion.Euler(local_rot);
        }
    }

    //turns image's alpha, prevent white block
    private void UpdateImageVisibility()
    {
        color = _ModelT == null ? new Color(0, 0, 0, 0) : Color.white;
    }

#if !ART_USE

    #region Pool

    //pool time
    private static UnityTimedPool _CameraPool = null;
    //private static UnityTimedPool _RenderTexturePool = null;

    private static UnityEngine.Object renderTextureSrc;
    private static UnityEngine.Object imageModelCameraSrc;
    private static GameObject pbShadow;
    private static GameObject pbWater;

    static void OnDeleteObject(System.Object obj)
    {
        UnityEngine.Object uobj = obj as UnityEngine.Object;
        if (uobj != null)
            Destroy(uobj);
    }

    public static void ReleasePool()
    {
        if (_ResourceInited)
        {
            //TimedPoolManager.ReleasePool(_RenderTexturePool);
            //TimedPoolManager.ReleasePool(_CameraPool);
            //_RenderTexturePool = null;
            //_CameraPool = null;

            //renderTextureSrc = null;
            //imageModelCameraSrc = null;

            //_ResourceInited = false;

            //_RenderTexturePool.Clear();
            _CameraPool.Clear();
        }
    }

    private void SafeInit()
    {
        if (!_ResourceInited)
        {
            //GNewUITools.SetVisible(transform, false);
            if (renderTextureSrc == null)
                renderTextureSrc = Resources.Load("ImgModelSupport/ModelTexture") as RenderTexture;
            if (imageModelCameraSrc == null)
                imageModelCameraSrc = Resources.Load("ImgModelSupport/GUICamera") as GameObject;

            //_RenderTexturePool = TimedPoolManager.Instance.GetUnityTimedPool(NUM_MAX_RT, RT_POOL_TIME, null, OnDeleteObject);
            //if (_RenderTexturePool == null) return;

            _CameraPool = TimedPoolManager.Instance.GetUnityTimedPool(NUM_MAX_CAM, CAM_POOL_TIME, null, OnDeleteObject);
            if (_CameraPool == null) return;

            _ResourceInited = true;
        }
    }

    #endregion

    void CreateCam()
    {
        GameObject cam_obj = null;
        RenderTexture rt = null;
        if (_CameraPool != null)
        {
            cam_obj = _CameraPool.Get() as GameObject;
            if (cam_obj == null)
            {
                cam_obj = CUnityUtil.Instantiate(imageModelCameraSrc) as GameObject;
                rt = CUnityUtil.Instantiate(renderTextureSrc) as RenderTexture;
            }

            if (cam_obj != null)
            {
                //_RenderTexture = CreateRT();
                //GameObject cam_obj = CreateCam();
                cam_obj.SetActive(true);

                _IMRootT = cam_obj.transform;
                _IMRootT.localRotation = Quaternion.identity;
                //in case of |-><-|, looking at eact other
                _IMRootT.localPosition = Vector3.right * (2000 + _ID * VIEW_DIST * 2);

                _StageT = _IMRootT.Find("Stage");
                if (_StageT == null) return;

                _PlatformT = _StageT.Find("Platform");
                if (_PlatformT == null) return;
                _PlatformT.localRotation = Quaternion.identity;

                //shadow
                _GroundT = _StageT.Find("Ground");
                if (_GroundT == null) return;

                _UICameraT = _IMRootT.Find("UICamera");
                if (_UICameraT == null) return;

                _UICamera = _UICameraT.GetComponent<Camera>();
                if (_UICamera == null) return;

                _UICamera.orthographic = true;
                _UICamera.orthographicSize = DEFAULT_CAM_SIZE;
                _UICamera.farClipPlane = VIEW_DIST;

                _UICamera.clearFlags = CameraClearFlags.Skybox;
                if (rt != null)
                {
                    _UICamera.targetTexture = rt;
                }
                _UICamera.enabled = this.enabled;
                texture = _UICamera.targetTexture;

                //ground effect
                InitGroundEffect();

                _CameraDist = STAGE_DIST;
                UpdateCameraRot(Vector3.zero);
                UpdateCameraPos();

                //post effects
                SetupPP();
                UpdatePP();
            }
        }
    }

    private void CreateCameraAndRenderTexture()
    {
        if (_UICamera != null /*|| _RenderTexture != null*/)
            return;

        _ID = -1;
        for (int i = 0; i < _MAX_CAM_COUNT; i++)
        {
            if (_ActiveCameras[i] == null)
            {
                _ID = i;
                _ActiveCameras[_ID] = this;
                break;
            }
        }

        if (_ID < 0)
        {
            Common.HobaDebuger.LogWarning("GImageModle has no empty slot, number > " + _MAX_CAM_COUNT);
        }
        else
        {
            CreateCam();

            if (_UICamera == null)
            {
                Debug.LogError("Create GUICamera failed, Check resource!!!");
                ClearCameraAndRenderTexture();
            }
        }
    }

    private void ClearCameraAndRenderTexture()
    {
        texture = null;

        //if (_ModelT != null)
        //{
        //    _ModelT.SetParent(null, false);
        //    //Common.HobaDebuger.LogWarning("Remember to Realse the Model loaded! " + _ModelT.name);

        //    Debug.LogWarning("IMageModel Release Model 1");
        //    LuaScriptMgr.Instance.CallOnTraceBack();
        //    _ModelT = null;
        //}

        CleanPP();

        //if (_UICamera != null && _UICamera.targetTexture == _RenderTexture)
        //    _UICamera.targetTexture = null;

        //if (_RenderTexture != null)
        //{
        //    if (_RenderTexturePool != null)
        //        _RenderTexturePool.Pool(_RenderTexture);
        //    else
        //        DestroyImmediate(_RenderTexture);

        //    _RenderTexture = null;
        //}

        if (_IMRootT != null)
        {
            if (_PlatformT != null)
            {
                while (_PlatformT.childCount > 0)
                {
                    Transform t = _PlatformT.GetChild(0);
                    t.SetParent(null, false);
#if UNITY_EDITOR
                    Debug.LogError("Leaked model found in " + this.name);
                    LuaScriptMgr.Instance.CallOnTraceBack();
#endif
                }
            }

            _IMRootT.gameObject.SetActive(false);

            if (_CameraPool != null)
                _CameraPool.Pool(_IMRootT.gameObject);
            else
                Destroy(_IMRootT.gameObject);

            _IMRootT = null;
            _UICamera = null;
            _UICameraT = null;
            _GroundT = null;
            _StageT = null;
            _PlatformT = null;
            _GroundShadow = null;
            _GroundWater = null;
        }

        if (_ID != -1)
        {
            _ActiveCameras[_ID] = null;
            _ID = -1;
        }
    }

    const string WaterPath = "Assets/Outputs/Scenes/SkySphere/ReflectionQuad.prefab";
    const string WaterBundlePath = "scenes";
    //const string ShadowPath = "";
    //const string ShadowBundlePath = "";
    private Transform _GroundShadow;
    private Transform _GroundWater;

    public void InitGroundEffect()
    {
        //ground effect
        _GroundShadow = _GroundT.Find("Shadow");
        _GroundWater = _GroundT.Find("Water");

        if (GroundType == GROUND_TYPE.Shadow && _GroundShadow == null)
        {
            if (pbShadow == null)
            {
                //GameObject obj =CAssetBundleManager.SyncLoadAssetFromBundle<GameObject>(ShadowPath, ShadowBundlePath);
                //(ShadowPath, OnShadowLoaded, ShadowBundlePath);
                OnPBLoaded(pbShadow, ref _GroundShadow, GROUND_TYPE.Shadow);
            }
            else
            {
                OnPBLoaded(pbShadow, ref _GroundShadow, GROUND_TYPE.Shadow);
            }
        }
        else if (GroundType == GROUND_TYPE.Water && _GroundWater == null)
        {
            if (pbWater == null)
            {
                //CAssetBundleManager.SyncLoadAssetFromBundle(WaterPath, OnWaterLoaded, WaterBundlePath);
                pbWater = CAssetBundleManager.SyncLoadAssetFromBundle<GameObject>(WaterPath, WaterBundlePath);
                OnPBLoaded(pbWater, ref _GroundWater, GROUND_TYPE.Water);
            }
            else
            {
                OnPBLoaded(pbWater, ref _GroundWater, GROUND_TYPE.Water);
            }
        }

        if (_GroundWater != null)
        {
            _GroundWater.gameObject.SetActive(GroundType == GROUND_TYPE.Water);
        }
        if (_GroundShadow != null)
        {
            _GroundShadow.gameObject.SetActive(GroundType == GROUND_TYPE.Shadow);
        }

    }

    void OnPBLoaded(GameObject pb_cache, ref Transform _ground, GROUND_TYPE ground_type)
    {
        if (pb_cache != null)
        {
            //if (pb_cache == null)
            //{
            //    GameObject g_pb_new = obj as GameObject;
            //    pb_cache = g_pb_new;
            //}

            if (_ground == null)
            {
                if (_GroundT != null && pb_cache != null)
                {
                    GameObject g_g = CUnityUtil.Instantiate(pb_cache) as GameObject;
                    if (g_g != null)
                    {
                        _ground = g_g.transform;
                        _ground.SetParent(_GroundT, false);

                        g_g.name = ground_type.ToString();
                        //g_g.SetActive(GroundType == ground_type);
                    }
                }
            }
        }

    }

    //void OnWaterLoaded(UnityEngine.Object obj)
    //{
    //    OnPBLoaded(obj, ref pbWater, ref _GroundWater, GROUND_TYPE.Water);
    //}

    //void OnShadowLoaded(UnityEngine.Object obj)
    //{
    //    OnPBLoaded(obj, ref pbShadow, ref _GroundShadow, GROUND_TYPE.Shadow);
    //}

    #region PostEffect

    private void SetupPP()
    {
        if (!UsePostEffect) return;

        if (_PPChainRef == null)
        {
            Camera main_camera = Main.Main3DCamera;
            if (main_camera != null)
            {
                _PPChainRef = main_camera.GetComponent<PostProcessChain>();
            }
        }

        if (_PPChainRef != null)
        {
            if (_PPChain == null && _UICamera != null)
            {
                _PPChain = _UICamera.GetComponent<PostProcessChain>();
            }
            if (_PPChain == null && _UICamera != null)
            {
                _PPChain = _UICamera.gameObject.AddComponent<PostProcessChain>();
            }
        }

        //_IsPPRunning = true;
    }

    private void UpdatePP()
    {
        if (_PPChainRef != null && _PPChain != null)
        {
            _PPChain.enabled = _PPChainRef.enabled;
            _PPChain.EnableHsvAdjust = _PPChainRef.EnableHsvAdjust;
            _PPChain._hsv_adjust_paramters = _PPChainRef._hsv_adjust_paramters;
            _PPChain.brightness_contrast_paramters = _PPChainRef.brightness_contrast_paramters;
        }
        else
        {
            CleanPP();
        }
    }

    private void CleanPP()
    {
        //_IsPPRunning = false;

        if (_PPChain != null)
        {
            _PPChain.EnableHsvAdjust = false;
            _PPChain.enabled = false;
            _PPChain = null;
        }

        _PPChainRef = null;
    }

    #endregion PostEffect

    protected override void OnDisable()
    {
        base.OnDisable();
        //ClearCameraAndRenderTexture();

        if (_UICamera != null)
        {
            _UICamera.enabled = false;
        }
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        //ClearCameraAndRenderTexture();

        if (_UICamera != null)
        {
            _UICamera.enabled = true;
        }
    }

#if UNITY_EDITOR
    static bool _IsQuiting = false;
    void OnApplicationQuit()
    {
        _IsQuiting = true;
    }
#endif

    protected override void OnDestroy()
    {
#if UNITY_EDITOR
        if (!_IsQuiting && (_ModelT != null || (_PlatformT != null && _PlatformT.childCount > 0)))
        {
            Debug.LogError("CUIMODEL destroy was not called ! " + GNewUITools.PrintScenePath(rectTransform));
            LuaScriptMgr.Instance.CallOnTraceBack();
        }
#endif

        Release();
        base.OnDestroy();

        _IsDestroyed = true;
    }

    protected new bool IsDestroyed()
    {
        return _IsDestroyed;
    }
#endif
}