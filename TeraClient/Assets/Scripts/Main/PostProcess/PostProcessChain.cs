using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Object = UnityEngine.Object;

[RequireComponent(typeof(PostProcess.BloomHD))]
public class PostProcessChain : MonoBehaviour
{
    //创建RenderTexture
    public static RenderTexture CreateRenderTexture(int width, int height, int depth, RenderTextureFormat format, RenderTexture oldTex)
    {
        if (oldTex == null || oldTex.width != width || oldTex.height != height)
        {
            if (oldTex != null)
                Object.Destroy(oldTex);
            var rt = new RenderTexture(width, height, depth, format);
            rt.name = "PostProcessChainRT2";
            return rt;
        }
        return oldTex;
    }

    public static RenderTexture CreateRenderTexture(int width, int height, int depth, RenderTexture oldTex)
    {
        if (oldTex == null || oldTex.width != width || oldTex.height != height)
        {
            if (oldTex != null)
                Object.Destroy(oldTex);
            var rt = new RenderTexture(width, height, depth);
            rt.name = "PostProcessChainRT2";
            return rt;
        }
        return oldTex;
    }

    public static Shader GetShader(string name)
    {
#if IN_GAME
        return ShaderManager.Instance.FindShader(name);
#else
        return Shader.Find(name);
#endif
    }

    private bool _IsCaptruredScreen = false;

    // brightness & contrast
    public Vector3 brightness_contrast_paramters;

    //RenderTexture original_frame;
    public Camera target_camera;

    #region FOG
    // Fog
    public bool EnableFog
    {
        get { return _EnableFog; }
        set
        {
#if IN_GAME
            if (Main.IsInGame() && !GFXConfig.Instance.IsUsePostProcessFog)
                value = false;
#endif

            _EnableFog = value;
        }
    }
    public Color fog_color;
    public Vector4 fog_paramter;

    private bool _EnableFog = false;
    #endregion

    #region DOF
    public bool EnableDepthOfField
    {
        get { return _EnableDof; }
        set
        {
#if IN_GAME
            if (Main.IsInGame() && !GFXConfig.Instance.IsEnableDOF)
                value = false;
#endif
            _EnableDof = value;
        }
    }
    private bool _EnableDof = false;

    public Vector3 depthoffield_paramter;
    #endregion

    #region HSV
    public bool EnableHsvAdjust
    {
        get { return _EnableHsvAdjust; }
        set
        {
            _EnableHsvAdjust = value;
        }
    }
    bool _EnableHsvAdjust = false;
    public Vector3 _hsv_adjust_paramters;
    #endregion

    #region BrightnessContrastGamma

    public bool EnableBrightnessContrastGamma
    {
        get
        {
#if IN_GAME
            if (Main.IsInGame())
                return GFXConfig.Instance.IsEnableBrightnessContrastGamma;
#endif
            return true;
        }
    }

    #endregion

    #region ShadowMidtoneHighlights

    public bool EnableShadowMidtoneHighlights
    {
        get
        {
#if IN_GAME
            if (Main.IsInGame())
                return GFXConfig.Instance.IsEnableShadowMidtoneHighlights;
#endif
            return true;

        }
    }

    #endregion


    #region BLOOMHD
    // bloom
    public bool EnableBloomHD
    {
        get { return _EnableBloomHD; }
        set
        {
#if IN_GAME
            if (Main.IsInGame() && !GFXConfig.Instance.IsEnableBloomHD)
                value = false;
#endif
            _EnableBloomHD = value;
        }
    }
    bool _EnableBloomHD = false;
    #endregion

    #region SpecialVision
    public bool EnableSpecialVision
    {
        get { return _EnableSpecialVision; }
        set
        {
            _EnableSpecialVision = value;
        }
    }
    public Color vision_color;
    public Vector2 special_vision_paramters;

    private bool _EnableSpecialVision = false;
    #endregion

    #region MotionBlur
    public bool EnableMotionBlur
    {
        get { return _EnableMotionBlur; }
        set
        {
#if IN_GAME
            if (Main.IsInGame() && !GFXConfig.Instance.IsEnableMotionBlur)
                value = false;
#endif
            if (_EnableMotionBlur != value)
            {
                _EnableMotionBlur = value;

                if (_EnableMotionBlur)
                {
                    //MotionBlurParamter = new Vector4(0.9f, 1, 0.3f, 0.7f);

                    _MotionBlurComp.Duration = MotionBlurParamter.y <= 0 ? 1 : MotionBlurParamter.y;
                    _MotionBlurComp.Fade_in_timing = Mathf.Clamp01(MotionBlurParamter.z);
                    _MotionBlurComp.Fade_out_timing = Mathf.Clamp01(MotionBlurParamter.w);
                    if (_MotionBlurComp.Fade_out_timing < _MotionBlurComp.Fade_in_timing)
                    {
                        _MotionBlurComp.Fade_out_timing = _MotionBlurComp.Fade_in_timing;
                    }
                    _MotionBlurComp.Elapse = 0;
                }
            }
        }
    }

    bool _EnableMotionBlur = false;
    public Vector4 MotionBlurParamter;

    public bool EnableRadialBlur
    {
        get { return _EnableRadialBlur; }
        set
        {
#if IN_GAME
            if (Main.IsInGame() && !GFXConfig.Instance.IsEnableRadialBlur)
                value = false;
#endif
            _EnableRadialBlur = value;
        }
    }
    bool _EnableRadialBlur = false;
    public Vector2 radial_blur_pos;
    public Vector2 radial_blur_paramters;

    public void DisableRadialBlur(float delayTime)
    {
        if (delayTime <= 0)
            EnableRadialBlur = false;
        else
        {
            StartCoroutine(Delay2DisableRadialBlur(delayTime));
        }
    }

    private IEnumerator Delay2DisableRadialBlur(float delayTime)
    {
        yield return new WaitForSeconds(delayTime);
        EnableRadialBlur = false;
    }
    #endregion

    PostProcess.Fog _FogComp;
    PostProcess.DepthOfField _DepthOfFieldComp;
    PostProcess.HSVAdjust _HsvAdjustComp;

    PostProcess.BloomHD _BloomHdComp;
    PostProcess.SpecialVision _SpecialVisionComp;
    PostProcess.MotionBlur _MotionBlurComp;
    PostProcess.SimpleRadialBlur _RadialBlurComp;

    Colorful.BrightnessContrastGamma _BCGComp;
    Colorful.ShadowsMidtonesHighlights _SMHComp;

    public PostProcess.BloomHD BloomHDComp { get { return _BloomHdComp; } set { _BloomHdComp = value; } }
    public Colorful.BrightnessContrastGamma BCGammaComp { get { return _BCGComp; } }
    public Colorful.ShadowsMidtonesHighlights SMHComp { get { return _SMHComp; } }

    public void DebugSet(bool bFog, bool bDOF, bool bHSV, bool bBloom, bool bMotionBlur)
    {
        _EnableMotionBlur = bMotionBlur;
        _EnableFog = bFog;
        _EnableBloomHD = bBloom;
        _EnableHsvAdjust = bHSV;
        _EnableDof = bDOF;
    }

    void OnDisable()
    {
        StopAllCoroutines();
    }

    void Awake()
    {
        InitResources();
    }

    void InitResources()
    {
        _FogComp = new PostProcess.Fog();
        _FogComp.Init(this, @"Hidden/PostProcessing/Fog");
        EnableFog = false;

        _DepthOfFieldComp = new PostProcess.DepthOfField();
        _DepthOfFieldComp.Init(this, @"Hidden/PostProcessing/DepthOfField");
        EnableDepthOfField = false;

        _HsvAdjustComp = new PostProcess.HSVAdjust();
        _HsvAdjustComp.Init(this, @"Hidden/PostProcessing/HSV");
        EnableHsvAdjust = false;

        _BloomHdComp = GetComponent<PostProcess.BloomHD>();
        if (_BloomHdComp == null)
        {
            _BloomHdComp = gameObject.AddComponent<PostProcess.BloomHD>();

            _BloomHdComp.Threshold = 2;
            _BloomHdComp.Intensity = 1;
            _BloomHdComp.Radius = 0.5f;
            _BloomHdComp.Iteration = 8;
            _BloomHdComp.WithFlicker = false;
        }

        EnableBloomHD = GFXConfig.Instance.IsEnableBloomHD;

        _SpecialVisionComp = new PostProcess.SpecialVision();
        _SpecialVisionComp.Init(this, @"Hidden/PostProcessing/SpecialVision");
        EnableSpecialVision = false;

        _MotionBlurComp = new PostProcess.MotionBlur();
        _MotionBlurComp.Init(this, @"Hidden/PostProcessing/MotionBlur");
        EnableMotionBlur = false;

        _RadialBlurComp = new PostProcess.SimpleRadialBlur();
        _RadialBlurComp.Init(this, @"Hidden/PostProcessing/SimpleRadialBlur");
        EnableRadialBlur = false;

        target_camera = gameObject.GetComponent<Camera>();

        // ColorfulFX
        _BCGComp = gameObject.GetComponent<Colorful.BrightnessContrastGamma>();
        if (null == _BCGComp)
            _BCGComp = gameObject.AddComponent<Colorful.BrightnessContrastGamma>();
        _BCGComp.Shader = GetShader("Hidden/PostProcessing/BrightnessContrastGamma");

        _SMHComp = gameObject.GetComponent<Colorful.ShadowsMidtonesHighlights>();
        if (null == _SMHComp)
            _SMHComp = gameObject.AddComponent<Colorful.ShadowsMidtonesHighlights>();
        _SMHComp.Shader = GetShader("Hidden/PostProcessing/ShadowsMidtonesHighlights");
    }

    void OnDestroy()
    {
        target_camera = null;

        if (_FogComp != null)
            _FogComp.Destroy();

        if (_DepthOfFieldComp != null)
            _DepthOfFieldComp.Destroy();

        if (_HsvAdjustComp != null)
            _HsvAdjustComp.Destroy();

        if (_SpecialVisionComp != null)
            _SpecialVisionComp.Destroy();

        if (_MotionBlurComp != null)
            _MotionBlurComp.Destroy();

        if (_RadialBlurComp != null)
            _RadialBlurComp.Destroy();

        StopAllCoroutines();
    }


    public void Tick(float dt)
    {
        //Check Enable
        this.enabled = _IsCaptruredScreen ||
           EnableFog ||
           EnableDepthOfField ||
           EnableHsvAdjust ||
           EnableBloomHD ||
           EnableSpecialVision ||
           EnableMotionBlur ||
           EnableRadialBlur;

        if (_BCGComp != null)
            _BCGComp.enabled = EnableBrightnessContrastGamma && !_BCGComp.CanTurnOff();

        if (_SMHComp != null)
            _SMHComp.enabled = EnableShadowMidtoneHighlights && !_SMHComp.CanTurnOff();
    }

    void Update()
    {
        if (EnableFog || EnableDepthOfField)          //开启Render Depth Texture
        {
            if (target_camera != null)
                target_camera.depthTextureMode = DepthTextureMode.Depth;
        }
        else
        {
            if (target_camera != null)
                target_camera.depthTextureMode = DepthTextureMode.None;
        }

        if (_BloomHdComp != null)
            _BloomHdComp.enabled = EnableBloomHD;
    }

    public void CaptureScreen()
    {
        if (_IsCaptruredScreen)
            return;

        _IsCaptruredScreen = true;
        target_camera.allowHDR = false;
        enabled = true;
    }

    void OnOverRender(RenderTexture dest)
    {
#if IN_GAME
        Texture2D tex2d = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
        tex2d.name = "CaptureImage";
        RenderTexture prev = RenderTexture.active;
        RenderTexture.active = dest;

        //Common.HobaDebuger.LogWarningFormat("dest Format: {0}", dest.format);

        tex2d.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        tex2d.Apply();
        RenderTexture.active = prev;

        CScreenShotMan.Instance.SetScreenShot(tex2d);
#endif
        _IsCaptruredScreen = false;
        target_camera.allowHDR = true;
    }

    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        bool bMotionBlur = EnableMotionBlur && _MotionBlurComp != null;
        bool bFog = EnableFog && _FogComp != null;
        bool bDOF = EnableDepthOfField && _DepthOfFieldComp != null;
        bool bHSV = EnableHsvAdjust && _HsvAdjustComp != null;
        bool bBoomHD = EnableBloomHD && _BloomHdComp != null;
        bool bRadialBlur = EnableRadialBlur && _RadialBlurComp != null;
        bool bSpecialVision = EnableSpecialVision && _SpecialVisionComp != null;

        int nPostProcess = 0;
        if (bMotionBlur) ++nPostProcess;
        if (bFog) ++nPostProcess;
        if (bDOF) ++nPostProcess;
        if (bHSV) ++nPostProcess;
        if (bBoomHD) ++nPostProcess;
        if (bRadialBlur) ++nPostProcess;
        if (bSpecialVision) ++nPostProcess;

        if (nPostProcess > 1)
        {
            RenderTexture tmpSrc = src;
            RenderTexture tmpDest = RenderTexture.GetTemporary(src.width, src.height, 0, src.format);
            tmpDest.name = "PostProcessChainDest";
            RenderTexture tmpTexture = tmpDest;

            //
            if (bMotionBlur)
            {
                _MotionBlurComp.RenderImage(ref tmpSrc, ref tmpDest);
            }
            if (bFog)
            {
                _FogComp.RenderImage(ref tmpSrc, ref tmpDest);
            }
            if (bDOF)
            {
                _DepthOfFieldComp.RenderImage(ref tmpSrc, ref tmpDest);
            }
            if (bHSV)
            {
                _HsvAdjustComp.RenderImage(ref tmpSrc, ref tmpDest);
            }
            if (bBoomHD)
            {
                _BloomHdComp.RenderImage(ref tmpSrc, ref tmpDest);
            }
            if (bRadialBlur)
            {
                _RadialBlurComp.RenderImage(ref tmpSrc, ref tmpDest);
            }
            if (bSpecialVision)
            {
                _SpecialVisionComp.RenderImage(ref tmpSrc, ref tmpDest);
            }

            Graphics.Blit(tmpSrc, dest);

            RenderTexture.ReleaseTemporary(tmpTexture);

            if (_IsCaptruredScreen)
            {
                OnOverRender(dest);
            }
        }
        else if (nPostProcess == 1)
        {
            if (bMotionBlur)
            {
                _MotionBlurComp.RenderImage(ref src, ref dest);
            }
            else if (bFog)
            {
                _FogComp.RenderImage(ref src, ref dest);
            }
            else if (bDOF)
            {
                _DepthOfFieldComp.RenderImage(ref src, ref dest);
            }
            else if (bHSV)
            {
                _HsvAdjustComp.RenderImage(ref src, ref dest);
            }
            else if (bBoomHD)
            {
                _BloomHdComp.RenderImage(ref src, ref dest);
            }
            else if (bRadialBlur)
            {
                _RadialBlurComp.RenderImage(ref src, ref dest);
            }
            else if (bSpecialVision)
            {
                _SpecialVisionComp.RenderImage(ref src, ref dest);
            }

            if (_IsCaptruredScreen)
            {
                OnOverRender(dest);
            }
        }
        else
        {
            Graphics.Blit(src, dest);

            if (_IsCaptruredScreen)
            {
                OnOverRender(dest);
            }
        }
    }
}

namespace PostProcess
{
    public class PostProcessItem
    {
        protected PostProcessChain _parent;

        protected Shader _shader;
        protected Material _material;
        protected bool _inited;

        public void Init(PostProcessChain parent, string shader_name)
        {
            if (_inited)
                return;

            _parent = parent;

            _shader = PostProcessChain.GetShader(shader_name);

            if (_shader != null)
                _material = new Material(_shader);

            if (_material == null)
                Common.HobaDebuger.LogErrorFormat("PostProcessItem Init Failed: {0}", shader_name);

            OnInit();

            //enabled = false;

            _inited = true;
        }

        protected virtual void OnInit()
        {

        }

        public void Destroy()
        {
            if (!_inited)
                return;

            OnDestroy();

            if (_material != null)
            {
                Object.Destroy(_material);
                _material = null;
            }
            _shader = null;

            _inited = false;
        }

        protected virtual void OnDestroy()
        {

        }

        public void RenderImage(ref RenderTexture src, ref RenderTexture dest)
        {
            OnRenderImage(src, dest);

            RenderTexture tmp = src;
            src = dest;
            dest = tmp;
        }

        protected virtual void OnRenderImage(RenderTexture src, RenderTexture dest)
        {

        }
    }
}
