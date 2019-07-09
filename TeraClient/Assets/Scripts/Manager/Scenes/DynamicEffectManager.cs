using UnityEngine;
using System;
using System.IO;
using System.Text;
using Common;

public class WeatherData
{
    public class SunConfig
    {
        public Color _SunColor = Color.white;
        public float _Intensity = 0;
        public Color _AmbientColor = Color.white;
        public float _AmbientColorIntensity = 0;
    }

    public class LightConfig
    {
        //public Vector3 _Rotation = Vector3.zero;

        public float _RotationX = 0;
        public float _RotationY = 0;
        public float _RotationZ = 0;
        public Color _LightColor = Color.white;
        public float _Intensity = 0;
        public float _BounceIntensity = 0;
        public float _Strength;
        public float _Bias;
        public float _NormalBias;
        public float _ShadowNearPlane;


    }
    public class PostEffectConfig
    {
        public class Fog
        {
            public bool _FogEnabled = false;
            public Color _FogColor = Color.white;
            //public Vector4 _FogParamter;
            public float _FogParamter0 = 0;
            public float _FogParamter1 = 0;
            public float _FogParamter2 = 0;
            public float _FogParamter3 = 0;
        }


        public class HSV
        {
            public bool _HSVEnabled = false;
            //public Vector3 HsvParamter;
            public float HsvParamter0 = 0;
            public float HsvParamter1 = 0;
            public float HsvParamter2 = 0;
        }

        public class DOF
        {
            public bool _DOFEnabled = false;
            //public Vector3 HsvParamter;
            public float DOFParamter0 = 0;
            public float DOFParamter1 = 0;
            public float DOFParamter2 = 0;
        }

        public class BrightnessAndContrast
        {
            public bool BrightnessAndConstrastEnabled = false;
            //public Vector3 BrightnessAndConstrastParamter;
            public float BrightnessAndConstrastParamter0 = 0;
            public float BrightnessAndConstrastParamter1 = 0;
            public float BrightnessAndConstrastParamter2 = 0;
        }


        public class Bloom
        {
            public bool BloomEnabled = false;
            //public Vector2 BloomParamter;
            public float BloomParamter0 = 0;
            public float BloomParamter1 = 0;
        }



        public class SpecialVision
        {
            public bool SpecialVisionEnabled = false;
            public Color SpecialVisionColor = Color.white;
            //public Vector2 SpecialVisionParamter;
            public float SpecialVisionParamter0;
            public float SpecialVisionParamter1;

        }


        [Range(0, 4)]
        public float Threshold = 1.2f;
        [Range(0, 2)]
        public float Intensity = 1f;
        [Range(0, 2)]
        public float Radius = 0.5f;
        [Range(0, 16)]
        public int Iteration = 10;
        public float SoftKneeA = 0;
        public bool WithFlicker = false;



        public Fog _Fog = new Fog();
        public HSV _HSV = new HSV();
        public DOF _DOF = new DOF();
        public BrightnessAndContrast _BrightnessAndContrast = new BrightnessAndContrast();
        public Bloom _Bloom = new Bloom();
        public SpecialVision _SpecialVision = new SpecialVision();

    }

    public class EnvironmentConfig
    {
        //ENV
        public Color _SkyColor = Color.white;
        public Color _EquatorColor = Color.white;
        public Color _GroundColor = Color.white;
        public float _AmbientIntensity = 0;
        //FOG

    }
    public class FogConfig
    {
        public bool _IsFogOn = false;
        public Color _FogColor = Color.white;
        public FogMode _FogMode = FogMode.Linear;
        public float _FogBeginDis = 0;
        public float _FogEndDis = 0;
    }

    public class SkyConfig
    {
        public string _CubeMapPath;
        public int _CubeMapBias;
        public float _ReflectionIntensity;
        public float _HeadlightIntesity;
        public Color _RefColor;
        public Vector4 _WindDirection;
        public float _WindNoise;
        public float _WindFrequency;
        public float _LightmapFeedback;

        public string _EnvCubeMapPath;
        public Vector4 _AddonDirection;
    }

    public class BrightnessContrastGammaConfig
    {
        public float Brightness = 0f;
        public float Contrast = 0f;
    }
    public class ShadowsMidtonesHighlightsConfig
    {
        public Color Shadows = new Color(1f, 1f, 1f, 0.5f);
        public Color Midtones = new Color(1f, 1f, 1f, 0.5f);
        public Color Highlights = new Color(1f, 1f, 1f, 0.5f);
        public float Amount = 1f;
    }


    public int _DataGuid = 0;
    public SunConfig _SunConfig = new SunConfig();
    public LightConfig _GlobalLightConfig = new LightConfig();
    public string _SkyBoxMatPath = "";
    public string _SkyBoxMatPathEX = "";
    public string _SfxPath = "";
    public SkyConfig _SkyConifg = new SkyConfig();
    public LightConfig _PlayerLightConfig = new LightConfig();
    public EnvironmentConfig _EnvironmentConfig = new EnvironmentConfig();
    public FogConfig _FogConfig = new FogConfig();
    public PostEffectConfig _PostEffectConfig = new PostEffectConfig();
    public float _ShadowConfig = 0;
    public string WwiseEvent = "";
    public BrightnessContrastGammaConfig _GammaConfig = new BrightnessContrastGammaConfig();
    public ShadowsMidtonesHighlightsConfig _HightLightConfig = new ShadowsMidtonesHighlightsConfig();
}

public class DynamicEffectManager : Singleton<DynamicEffectManager>, GameLogic.ITickLogic
{
    /// <summary>
    /// 功能定位
    /// 场景动态效果管理，包括：
    ///     1 不同场景的环境参数（晨 午 夕 夜 雨 雪）
    ///     2 鹰眼效果
    ///     3 其他：CG效果 玩家死亡 UI效果 NPC对话拉近 主角拉近
    ///     4 Server通知改变天气要强制
    /// </summary>

    private const float ADDTION_LERP_TIME = 10f;
    private const string SKY_BOX_PREFAB_PATH = "Assets/Outputs/Scenes/SkySphere/Circulationsphere.prefab";
    private const string DEFAULT_ENV_CUBEMAP_PATH = "Assets/Outputs/Scenes/SkySphere/Cubemaps/defaultCube.cubemap";
    private const int HAWKEYE_EFFECT_ID = 180;
    private const float HAWKEYE_LERP_TIME = 1f;
    private const float CG_LERP_TIME = 1f;
    private readonly Vector3 _HSVOnDeath = new Vector3(0, 0, 0.7f);

    private readonly DynEffectCalculator _DynEffectCalculator = new DynEffectCalculator();
    private WeatherSystemConfig _WeatherConfigData = null;
    private SceneConfig _Config = null;

    // 全局光
    private GameObject _GlobalLightObj;
    private Light _GlobalLight = null;

    // 角色光
    private Light _PlayerLight = null;
    private Sun _SunComponent = null;
    private Sky _SkyComponent = null;

    // 天空盒
    private MeshRenderer _SkyBoxSphere = null;
    private GameObject _CirculationsphereObj;
    private readonly Action[] _SkyboxLoadedCallbacks = new Action[2];

    // 后处理
    private PostProcessChain _PostProcessChainComponent = null;

    // 场景特效
    private GameObject _CurrentSfxObj = null;
    private string _CurrentSfxObjPath = null;
    private int _CurrentSfxGuid = 0;

    private Cubemap _DefaultCubeMap = null;
    // 效果覆盖规则：
    // UI效果 > 死亡效果 > 鹰眼效果 > 雨雪效果 > 昼夜效果

    enum EffectTriggerType
    {
        EnterUIEffect = 0,
        LeaveUIEffect = 1,
        EnterHawkEffect = 2,
        LeaveHawkEffect = 3,
        EnterCGEffect = 4,
        LeaveCGEffect = 5,
        EnterWeatherEffect = 6,
        LeaveWeatherEffect = 7,
        RegionChanged = 8,
        TimeChanged = 9,
        ForceChanged = 10,
        EnterMemoryEffect = 11,
        LeaveMemoryEffect = 12,
        Debug = 100,
        None = -1,
    }
    private EffectTriggerType _CurTriggerType = EffectTriggerType.None;

    // 1 区域变化
    private int _CurrentRegionId = -1;
    // 2 当前动态效果类型变化，昼夜
    private DynamicEffectType _CurrentEffectType = DynamicEffectType.None;
    // 3 当前天气类型，暂不支持雨雪之间变化
    private WeatherType _CurrentWeatherType = WeatherType.None;
    private int _CurrentWeatherId = 0;
    private int _CurrentEffectId = 0;

    private WeatherData _LerpFromData = null;
    private WeatherData _LerpToData = null;
    private WeatherData _CurrentData = null;
    private readonly WeatherData _ReusedData1 = new WeatherData();
    private readonly WeatherData _ReusedData2 = new WeatherData();

    private float _LerpTime = 0f;
    private bool _IsInLerping = false;
    private float _CurLerpValue = 0f;

    private int _CurHawkEffectId = 0;
    private int _CurUIEffectId = 0;
    private int _CurCGEffectId = 0;

    private bool _IsPlayerDead = false;
    private bool _IsChangeDOF = false;
    private Vector3 _ChangedDOFParam = Vector3.zero;

    private DynamicEffectType _DebugWeatherType = DynamicEffectType.None;

    private int _ForceChangedEffectID = 0;
    private int _MomoryChangedEffectID = 0;
    private float _LastTime = 0;

    public void Init(SceneConfig cfg)
    {
        _CurrentRegionId = ScenesRegionManager.Instance.CurLightmapRegionID;

        if (_CurrentRegionId < 0 || _CurrentRegionId >= cfg._RegionIdGroup.Count)
        {
            HobaDebuger.LogWarningFormat("RegionIdGroup config has no cfg data with id = {0}", _CurrentRegionId);
            return;
        }
        _CurTriggerType = EffectTriggerType.None;
        _Config = cfg;

        _DynEffectCalculator.Init();
        _CurrentEffectType = _DynEffectCalculator.GetDynamicEffectType();
        LuaScriptMgr.Instance.CallLuaOnWeatherEventFunc((int)_CurrentEffectType);
        ScenesAnimationManager.Instance.Init(_CurrentEffectType == DynamicEffectType.Night);

        // 场景全局光
        InitGlobalLightObjAndComps();  // 同步
        // 角色光
        InitPlayerLightObjAndComps();   //同步，playerlight & sun & sky
        // 后处理组件
        InitPostEffectChainComp();   // 同步
        // 天空盒

        #region 计算当前效果
        // UIEffect CGEffect 不可能出现在OnEnterScene中
        if (_CurHawkEffectId > 0)
        {
            _CurrentData = GetConfigDataByID(_CurHawkEffectId);
        }
        // 如果雨雪变化
        else if (_CurrentWeatherType != WeatherType.None)
        {
            int weatherId = GetCurrentWeatherID(_CurrentWeatherType);
            _CurrentData = GetConfigDataByID(weatherId);
            _CurrentWeatherId = weatherId;
        }
        else
        {
            var curEffectId = GetCurrentEffectID(_CurrentEffectType);
            _CurrentData = GetConfigDataByID(curEffectId);
        }
        #endregion

        if (null != _CurrentData)
        {
            InitSkyBoxSphere();  // 异步
            LoadSceneSfx(_CurrentData._SfxPath);

            SetFinallyWeatherData(_CurrentData);
        }
        // 设置当前效果
        //LoadSkyboxMaterial(_CurrentData._SkyBoxMatPath, _CurrentData._SkyBoxMatPath);
        //LoadSkyCubemap(_CurrentData._SkyConifg._CubeMapPath);

    }

    private int GetCurrentWeatherID(WeatherType weatherType)
    {
        var weatherId = 0;
        if (null == _Config || null == _Config._RegionIdGroup || _CurrentRegionId > _Config._RegionIdGroup.Count)
            return weatherId;
        switch (weatherType)
        {
            case WeatherType.Rain:
                weatherId = _Config._RegionIdGroup[_CurrentRegionId].RainID;
                break;
            case WeatherType.Snow:
                weatherId = _Config._RegionIdGroup[_CurrentRegionId].SnowID;
                break;
        }
        return weatherId;
    }

    private int GetCurrentEffectID(DynamicEffectType effectType)
    {
        var curEffectId = 0;
        if (null == _Config || null == _Config._RegionIdGroup || _CurrentRegionId > _Config._RegionIdGroup.Count)
            return curEffectId;
        switch (effectType)
        {
            case DynamicEffectType.Morning:
                curEffectId = _Config._RegionIdGroup[_CurrentRegionId].MorningID;
                break;
            case DynamicEffectType.Day:
                curEffectId = _Config._RegionIdGroup[_CurrentRegionId].DayID;
                break;
            case DynamicEffectType.Dusk:
                curEffectId = _Config._RegionIdGroup[_CurrentRegionId].DuskID;
                break;
            case DynamicEffectType.Night:
                curEffectId = _Config._RegionIdGroup[_CurrentRegionId].NightID;
                break;
        }
        return curEffectId;
    }

    private void CalculateEffectLerpData()
    {
        if (_CurTriggerType == EffectTriggerType.None || _CurrentData == null)
            return;

        HobaDebuger.LogFormat("Trigger Dynamic Effect Change - {0}", _CurTriggerType);

        _LerpFromData = _CurrentData;
        var currentEnvMapPth = _CurrentData._SkyConifg._EnvCubeMapPath;
        var needChange = false;

        if (_CurTriggerType == EffectTriggerType.EnterUIEffect)
        {
            _LerpToData = GetConfigDataByID(_CurUIEffectId);
            _LerpTime = 0;
            needChange = true;
        }
        else if (_CurTriggerType == EffectTriggerType.EnterCGEffect)
        {
            if (_CurUIEffectId != 0)
            {
                _LerpToData = GetConfigDataByID(_CurCGEffectId);
                _LerpTime = CG_LERP_TIME;
                needChange = true;
            }
        }
        else if (_CurTriggerType == EffectTriggerType.EnterHawkEffect)
        {
            if (_CurUIEffectId == 0 && _CurCGEffectId == 0)
            {
                _LerpToData = GetConfigDataByID(HAWKEYE_EFFECT_ID);
                _LerpTime = HAWKEYE_LERP_TIME;
                needChange = true;
            }
        }
        else if (_CurTriggerType == EffectTriggerType.EnterMemoryEffect)
        {
            if (_CurUIEffectId == 0 && _CurCGEffectId == 0)
            {
                _LerpToData = GetConfigDataByID(_MomoryChangedEffectID);
                _LerpTime = HAWKEYE_LERP_TIME;
                needChange = true;
            }
        }
        else if (_CurTriggerType == EffectTriggerType.EnterWeatherEffect)
        {
            if (_CurUIEffectId == 0 && _CurCGEffectId == 0 && _CurHawkEffectId == 0)
            {
                int weatherId = GetCurrentWeatherID(_CurrentWeatherType);
                if (_CurrentWeatherId != weatherId)
                    _LerpToData = GetConfigDataByID(weatherId);

                _LerpTime = ADDTION_LERP_TIME;
                needChange = true;
            }
        }
        else if (_CurTriggerType == EffectTriggerType.LeaveUIEffect)
        {
            var destEffectId = 0;
            if (_CurCGEffectId > 0)
                destEffectId = _CurCGEffectId;
            else if (_CurHawkEffectId > 0)
                destEffectId = _CurHawkEffectId;
            else if (_ForceChangedEffectID > 0)
                destEffectId = _ForceChangedEffectID;
            else
                destEffectId = GetCurrentEffectID(_CurrentEffectType);

            _LerpToData = GetConfigDataByID(destEffectId);
            _LerpTime = 0;
            needChange = true;
        }
        else if (_CurTriggerType == EffectTriggerType.LeaveCGEffect)
        {
            var destEffectId = 0;
            if (_CurUIEffectId > 0)
                destEffectId = 0;
            else if (_CurHawkEffectId > 0)
                destEffectId = _CurHawkEffectId;
            else if (_ForceChangedEffectID > 0)
                destEffectId = _ForceChangedEffectID;
            else
                destEffectId = GetCurrentEffectID(_CurrentEffectType);

            _LerpToData = GetConfigDataByID(destEffectId);
            _LerpTime = CG_LERP_TIME;
            needChange = true;
        }
        else if (_CurTriggerType == EffectTriggerType.LeaveHawkEffect || _CurTriggerType == EffectTriggerType.LeaveMemoryEffect)
        {
            var destEffectId = 0;
            if (_CurUIEffectId > 0 || _CurCGEffectId > 0)
                destEffectId = 0;
            else if (_ForceChangedEffectID > 0)
                destEffectId = _ForceChangedEffectID;
            else
                destEffectId = GetCurrentEffectID(_CurrentEffectType);

            _LerpToData = GetConfigDataByID(destEffectId);
            _LerpTime = HAWKEYE_LERP_TIME;
            needChange = true;
        }
        else if (_CurTriggerType == EffectTriggerType.ForceChanged)
        {
            if (_CurUIEffectId == 0 && _CurCGEffectId == 0 && _CurHawkEffectId == 0)
            {
                _LerpToData = GetConfigDataByID(_ForceChangedEffectID);
                _LerpTime = 10f;
                needChange = true;
            }
        }
        else if (_CurTriggerType == EffectTriggerType.Debug)
        {
            var destEffectId = GetCurrentEffectID(_DebugWeatherType);
            _LerpToData = GetConfigDataByID(destEffectId);
            _LerpTime = 5f;
            needChange = true;
        }
        else // RegionChanged || TimeChanged || LeaveWeatherEffect
        {
            currentEnvMapPth = _CurrentData._SkyConifg._EnvCubeMapPath;
            var destEffectId = 0;
            if (_CurUIEffectId > 0 || _CurCGEffectId > 0 || _CurHawkEffectId > 0 || _ForceChangedEffectID > 0)
                destEffectId = 0;
            else
                destEffectId = GetCurrentEffectID(_CurrentEffectType);

            if (destEffectId != 0 && destEffectId != _CurrentEffectId)
            {
                _LerpToData = GetConfigDataByID(destEffectId);
                _CurrentEffectId = destEffectId;

                if (_CurTriggerType == EffectTriggerType.LeaveWeatherEffect)
                    _LerpTime = ADDTION_LERP_TIME;
                else if (_CurTriggerType == EffectTriggerType.RegionChanged)
                    _LerpTime = _DynEffectCalculator.RegionLerpTime;
                else if (_CurTriggerType == EffectTriggerType.TimeChanged)
                    _LerpTime = _DynEffectCalculator.GetDynamicEffectLerpTime(_CurrentEffectType);
                needChange = true;
            }
        }

        if (needChange && _LerpFromData != null && _LerpToData != null)
        {
            if (_CurrentData == _ReusedData1)
                _CurrentData = _ReusedData2;
            else
                _CurrentData = _ReusedData1;

            LoadSkyboxMaterial(_LerpFromData._SkyBoxMatPath, _LerpToData._SkyBoxMatPath);
            LoadSkyCubemap(_LerpToData._SkyConifg._CubeMapPath);
            LoadSceneSfx(_LerpToData._SfxPath);
            
                LoadStartEnvCubeMap(currentEnvMapPth, _LerpFromData._SkyConifg._EnvCubeMapPath, false);

            //LoadStartEnvCubeMap(_CurrentData._SkyConifg._EnvCubeMapPath, _LerpFromData._SkyConifg._EnvCubeMapPath, false);
            _CurLerpValue = 0;
            _IsInLerping = true;

            HobaDebuger.LogFormat("Dynamic Effect Start Lerp - {0}", _LerpToData._DataGuid);
        }

        _CurTriggerType = EffectTriggerType.None;
    }

    private void DoEffectDataLerp(WeatherData startData, WeatherData endData, float currentLerpValue)
    {
        if (Math.Abs(currentLerpValue) < 1e-3)
        {
            _CurrentData._SkyBoxMatPath = startData._SkyBoxMatPath;
        }
        else if (Math.Abs(currentLerpValue - 1) < 1e-3)
        {
            _CurrentData._SkyBoxMatPath = endData._SkyBoxMatPath;
        }

        #region 全局光计算
        _CurrentData._GlobalLightConfig._LightColor = Color.Lerp(startData._GlobalLightConfig._LightColor, endData._GlobalLightConfig._LightColor, currentLerpValue);
        _CurrentData._GlobalLightConfig._RotationX = Mathf.Lerp(startData._GlobalLightConfig._RotationX, endData._GlobalLightConfig._RotationX, currentLerpValue);
        _CurrentData._GlobalLightConfig._RotationY = Mathf.Lerp(startData._GlobalLightConfig._RotationY, endData._GlobalLightConfig._RotationY, currentLerpValue);
        _CurrentData._GlobalLightConfig._RotationZ = Mathf.Lerp(startData._GlobalLightConfig._RotationZ, endData._GlobalLightConfig._RotationZ, currentLerpValue);
        _CurrentData._GlobalLightConfig._Intensity = Mathf.Lerp(startData._GlobalLightConfig._Intensity, endData._GlobalLightConfig._Intensity, currentLerpValue);
        _CurrentData._GlobalLightConfig._BounceIntensity = Mathf.Lerp(startData._GlobalLightConfig._BounceIntensity, endData._GlobalLightConfig._BounceIntensity, currentLerpValue);
        #endregion

        #region Calc SkyConfig

        _CurrentData._SkyConifg._ReflectionIntensity = Mathf.Lerp(startData._SkyConifg._ReflectionIntensity, endData._SkyConifg._ReflectionIntensity, currentLerpValue);
        _CurrentData._SkyConifg._HeadlightIntesity = Mathf.Lerp(startData._GlobalLightConfig._RotationX, endData._GlobalLightConfig._RotationX, currentLerpValue);
        _CurrentData._SkyConifg._RefColor = Color.Lerp(startData._SkyConifg._RefColor, endData._SkyConifg._RefColor, currentLerpValue);
        _CurrentData._SkyConifg._WindDirection = Vector4.Lerp(startData._SkyConifg._WindDirection, endData._SkyConifg._WindDirection, currentLerpValue);
        _CurrentData._SkyConifg._WindFrequency = Mathf.Lerp(startData._SkyConifg._WindFrequency, endData._SkyConifg._WindFrequency, currentLerpValue);
        _CurrentData._SkyConifg._WindNoise = Mathf.Lerp(startData._SkyConifg._WindNoise, endData._SkyConifg._WindNoise, currentLerpValue);
        _CurrentData._SkyConifg._LightmapFeedback = Mathf.Lerp(startData._SkyConifg._LightmapFeedback, endData._SkyConifg._LightmapFeedback, currentLerpValue);
        _CurrentData._SkyConifg._AddonDirection = Vector4.Lerp(startData._SkyConifg._AddonDirection, endData._SkyConifg._AddonDirection, currentLerpValue);

        #endregion

        #region 角色光计算
        _CurrentData._PlayerLightConfig._LightColor = Color.Lerp(startData._PlayerLightConfig._LightColor, endData._PlayerLightConfig._LightColor, currentLerpValue);
        _CurrentData._PlayerLightConfig._RotationX = Mathf.Lerp(startData._PlayerLightConfig._RotationX, endData._PlayerLightConfig._RotationX, currentLerpValue);
        _CurrentData._PlayerLightConfig._RotationY = Mathf.Lerp(startData._PlayerLightConfig._RotationY, endData._PlayerLightConfig._RotationY, currentLerpValue);
        _CurrentData._PlayerLightConfig._RotationZ = Mathf.Lerp(startData._PlayerLightConfig._RotationZ, endData._PlayerLightConfig._RotationZ, currentLerpValue);
        _CurrentData._PlayerLightConfig._Intensity = Mathf.Lerp(startData._PlayerLightConfig._Intensity, endData._PlayerLightConfig._Intensity, currentLerpValue);
        _CurrentData._PlayerLightConfig._BounceIntensity = Mathf.Lerp(startData._PlayerLightConfig._BounceIntensity, endData._PlayerLightConfig._BounceIntensity, currentLerpValue);
        _CurrentData._PlayerLightConfig._Strength = Mathf.Lerp(startData._PlayerLightConfig._Strength, endData._PlayerLightConfig._Strength, currentLerpValue);
        _CurrentData._PlayerLightConfig._Bias = Mathf.Lerp(startData._PlayerLightConfig._Bias, endData._PlayerLightConfig._Bias, currentLerpValue);
        _CurrentData._PlayerLightConfig._NormalBias = Mathf.Lerp(startData._PlayerLightConfig._NormalBias, endData._PlayerLightConfig._NormalBias, currentLerpValue);
        _CurrentData._PlayerLightConfig._ShadowNearPlane = Mathf.Lerp(startData._PlayerLightConfig._ShadowNearPlane, endData._PlayerLightConfig._ShadowNearPlane, currentLerpValue);

        #endregion

        #region 太阳光计算
        _CurrentData._SunConfig._SunColor = Color.Lerp(startData._SunConfig._SunColor, endData._SunConfig._SunColor, currentLerpValue);
        _CurrentData._SunConfig._Intensity = Mathf.Lerp(startData._SunConfig._Intensity, endData._SunConfig._Intensity, currentLerpValue);
        _CurrentData._SunConfig._AmbientColor = Color.Lerp(startData._SunConfig._AmbientColor, endData._SunConfig._AmbientColor, currentLerpValue);
        _CurrentData._SunConfig._AmbientColorIntensity = Mathf.Lerp(startData._SunConfig._AmbientColorIntensity, endData._SunConfig._AmbientColorIntensity, currentLerpValue);
        #endregion

        #region 环境光计算
        _CurrentData._EnvironmentConfig._SkyColor = Color.Lerp(startData._EnvironmentConfig._SkyColor, endData._EnvironmentConfig._SkyColor, currentLerpValue);
        _CurrentData._EnvironmentConfig._GroundColor = Color.Lerp(startData._EnvironmentConfig._GroundColor, endData._EnvironmentConfig._GroundColor, currentLerpValue);
        _CurrentData._EnvironmentConfig._EquatorColor = Color.Lerp(startData._EnvironmentConfig._EquatorColor, endData._EnvironmentConfig._EquatorColor, currentLerpValue);
        _CurrentData._EnvironmentConfig._AmbientIntensity = Mathf.Lerp(startData._EnvironmentConfig._AmbientIntensity, endData._EnvironmentConfig._AmbientIntensity, currentLerpValue);
        #endregion

        #region 雾效计算
        _CurrentData._FogConfig._IsFogOn = endData._FogConfig._IsFogOn;
        //暂时取消开关设置，理论上多个插值间的值应该保持一致，不一致视为数据填写错误。  
        _CurrentData._FogConfig._FogColor = Color.Lerp(startData._FogConfig._FogColor, endData._FogConfig._FogColor, currentLerpValue);
        _CurrentData._FogConfig._FogMode = endData._FogConfig._FogMode;
        _CurrentData._FogConfig._FogBeginDis = Mathf.Lerp(startData._FogConfig._FogBeginDis, endData._FogConfig._FogBeginDis, currentLerpValue);
        _CurrentData._FogConfig._FogEndDis = Mathf.Lerp(startData._FogConfig._FogEndDis, endData._FogConfig._FogEndDis, currentLerpValue);
        #endregion

        if (_CurCGEffectId == 0)
        {
            #region 后处理参数计算
            _CurrentData._PostEffectConfig._Fog._FogEnabled = endData._PostEffectConfig._Fog._FogEnabled;
            _CurrentData._PostEffectConfig._Fog._FogColor = Color.Lerp(startData._PostEffectConfig._Fog._FogColor, endData._PostEffectConfig._Fog._FogColor, currentLerpValue);
            _CurrentData._PostEffectConfig._Fog._FogParamter0 = Mathf.Lerp(startData._PostEffectConfig._Fog._FogParamter0, endData._PostEffectConfig._Fog._FogParamter0, currentLerpValue);
            _CurrentData._PostEffectConfig._Fog._FogParamter1 = Mathf.Lerp(startData._PostEffectConfig._Fog._FogParamter1, endData._PostEffectConfig._Fog._FogParamter1, currentLerpValue);
            _CurrentData._PostEffectConfig._Fog._FogParamter2 = Mathf.Lerp(startData._PostEffectConfig._Fog._FogParamter2, endData._PostEffectConfig._Fog._FogParamter2, currentLerpValue);
            _CurrentData._PostEffectConfig._Fog._FogParamter3 = Mathf.Lerp(startData._PostEffectConfig._Fog._FogParamter3, endData._PostEffectConfig._Fog._FogParamter3, currentLerpValue);


            _CurrentData._PostEffectConfig._Bloom.BloomEnabled = endData._PostEffectConfig._Bloom.BloomEnabled;
            _CurrentData._PostEffectConfig._Bloom.BloomParamter0 = Mathf.Lerp(startData._PostEffectConfig._Bloom.BloomParamter0, endData._PostEffectConfig._Bloom.BloomParamter0, currentLerpValue);
            _CurrentData._PostEffectConfig._Bloom.BloomParamter1 = Mathf.Lerp(startData._PostEffectConfig._Bloom.BloomParamter1, endData._PostEffectConfig._Bloom.BloomParamter1, currentLerpValue);


            _CurrentData._PostEffectConfig._BrightnessAndContrast.BrightnessAndConstrastEnabled = endData._PostEffectConfig._BrightnessAndContrast.BrightnessAndConstrastEnabled;
            _CurrentData._PostEffectConfig._BrightnessAndContrast.BrightnessAndConstrastParamter0 = Mathf.Lerp(startData._PostEffectConfig._BrightnessAndContrast.BrightnessAndConstrastParamter0,
                endData._PostEffectConfig._BrightnessAndContrast.BrightnessAndConstrastParamter0, currentLerpValue);
            _CurrentData._PostEffectConfig._BrightnessAndContrast.BrightnessAndConstrastParamter1 = Mathf.Lerp(startData._PostEffectConfig._BrightnessAndContrast.BrightnessAndConstrastParamter1,
                endData._PostEffectConfig._BrightnessAndContrast.BrightnessAndConstrastParamter1, currentLerpValue);
            _CurrentData._PostEffectConfig._BrightnessAndContrast.BrightnessAndConstrastParamter2 = Mathf.Lerp(startData._PostEffectConfig._BrightnessAndContrast.BrightnessAndConstrastParamter2,
                endData._PostEffectConfig._BrightnessAndContrast.BrightnessAndConstrastParamter2, currentLerpValue);

            _CurrentData._PostEffectConfig._HSV._HSVEnabled = endData._PostEffectConfig._HSV._HSVEnabled;
            _CurrentData._PostEffectConfig._HSV.HsvParamter0 = Mathf.Lerp(startData._PostEffectConfig._HSV.HsvParamter0, endData._PostEffectConfig._HSV.HsvParamter0, currentLerpValue);
            _CurrentData._PostEffectConfig._HSV.HsvParamter1 = Mathf.Lerp(startData._PostEffectConfig._HSV.HsvParamter1, endData._PostEffectConfig._HSV.HsvParamter1, currentLerpValue);
            _CurrentData._PostEffectConfig._HSV.HsvParamter2 = Mathf.Lerp(startData._PostEffectConfig._HSV.HsvParamter2, endData._PostEffectConfig._HSV.HsvParamter2, currentLerpValue);


            _CurrentData._PostEffectConfig._DOF._DOFEnabled = endData._PostEffectConfig._DOF._DOFEnabled;
            _CurrentData._PostEffectConfig._DOF.DOFParamter0 = Mathf.Lerp(startData._PostEffectConfig._DOF.DOFParamter0, endData._PostEffectConfig._DOF.DOFParamter0, currentLerpValue);
            _CurrentData._PostEffectConfig._DOF.DOFParamter1 = Mathf.Lerp(startData._PostEffectConfig._DOF.DOFParamter1, endData._PostEffectConfig._DOF.DOFParamter1, currentLerpValue);
            _CurrentData._PostEffectConfig._DOF.DOFParamter2 = Mathf.Lerp(startData._PostEffectConfig._DOF.DOFParamter2, endData._PostEffectConfig._DOF.DOFParamter2, currentLerpValue);


            _CurrentData._PostEffectConfig._SpecialVision.SpecialVisionEnabled = endData._PostEffectConfig._SpecialVision.SpecialVisionEnabled;
            _CurrentData._PostEffectConfig._SpecialVision.SpecialVisionColor = Color.Lerp(startData._PostEffectConfig._SpecialVision.SpecialVisionColor, endData._PostEffectConfig._SpecialVision.SpecialVisionColor, currentLerpValue);
            _CurrentData._PostEffectConfig._SpecialVision.SpecialVisionParamter0 = Mathf.Lerp(startData._PostEffectConfig._SpecialVision.SpecialVisionParamter0, endData._PostEffectConfig._SpecialVision.SpecialVisionParamter0, currentLerpValue);
            _CurrentData._PostEffectConfig._SpecialVision.SpecialVisionParamter1 = Mathf.Lerp(startData._PostEffectConfig._SpecialVision.SpecialVisionParamter1, endData._PostEffectConfig._SpecialVision.SpecialVisionParamter1, currentLerpValue);


            _CurrentData._PostEffectConfig.Threshold = Mathf.Lerp(startData._PostEffectConfig.Threshold, endData._PostEffectConfig.Threshold, currentLerpValue);
            _CurrentData._PostEffectConfig.Intensity = Mathf.Lerp(startData._PostEffectConfig.Intensity, endData._PostEffectConfig.Intensity, currentLerpValue);
            _CurrentData._PostEffectConfig.Radius = Mathf.Lerp(startData._PostEffectConfig.Radius, endData._PostEffectConfig.Radius, currentLerpValue);
            _CurrentData._PostEffectConfig.Iteration = (int)Mathf.Lerp(startData._PostEffectConfig.Iteration, endData._PostEffectConfig.Iteration, currentLerpValue);
            _CurrentData._PostEffectConfig.SoftKneeA = Mathf.Lerp(startData._PostEffectConfig.SoftKneeA, endData._PostEffectConfig.SoftKneeA, currentLerpValue);
            _CurrentData._PostEffectConfig.WithFlicker = startData._PostEffectConfig.WithFlicker;

            _CurrentData._GammaConfig.Brightness = Mathf.Lerp(startData._GammaConfig.Brightness, endData._GammaConfig.Brightness, currentLerpValue);
            _CurrentData._GammaConfig.Contrast = Mathf.Lerp(startData._GammaConfig.Contrast, endData._GammaConfig.Contrast, currentLerpValue);


            _CurrentData._HightLightConfig.Amount = Mathf.Lerp(startData._HightLightConfig.Amount, endData._HightLightConfig.Amount, currentLerpValue);
            _CurrentData._HightLightConfig.Highlights = Color.Lerp(startData._HightLightConfig.Highlights, endData._HightLightConfig.Highlights, currentLerpValue);
            _CurrentData._HightLightConfig.Midtones = Color.Lerp(startData._HightLightConfig.Midtones, endData._HightLightConfig.Midtones, currentLerpValue);
            _CurrentData._HightLightConfig.Shadows = Color.Lerp(startData._HightLightConfig.Shadows, endData._HightLightConfig.Shadows, currentLerpValue);
        }

            #endregion
    }

    private void SetFinallyWeatherData(WeatherData calcData)
    {

        if (null == calcData) return;
        // fog
        SetFogInfo(calcData._FogConfig._IsFogOn, calcData._FogConfig._FogColor, calcData._FogConfig._FogMode,
            calcData._FogConfig._FogBeginDis, calcData._FogConfig._FogEndDis);
        // environment light
        SetEnvInfo(calcData._EnvironmentConfig._EquatorColor,
            calcData._EnvironmentConfig._GroundColor,
            calcData._EnvironmentConfig._SkyColor, calcData._EnvironmentConfig._AmbientIntensity);
        // global light
        Vector3 rot = new Vector3(calcData._GlobalLightConfig._RotationX, calcData._GlobalLightConfig._RotationY,
            calcData._GlobalLightConfig._RotationZ);
        SetGlobalLightInfo(calcData._GlobalLightConfig._LightColor, rot, calcData._GlobalLightConfig._Intensity);
        // player light
        rot = new Vector3(calcData._PlayerLightConfig._RotationX, calcData._PlayerLightConfig._RotationY,
            calcData._PlayerLightConfig._RotationZ);
        SetPlayerLightInfo(calcData._PlayerLightConfig._LightColor, rot, calcData._PlayerLightConfig._Intensity,
            calcData._PlayerLightConfig._Strength, calcData._PlayerLightConfig._Bias,
            calcData._PlayerLightConfig._NormalBias, calcData._PlayerLightConfig._ShadowNearPlane);
        // sun
        SetSunInfo(calcData._SunConfig._SunColor, calcData._SunConfig._Intensity, calcData._SunConfig._AmbientColor,
            calcData._SunConfig._AmbientColorIntensity);
        // gamma
        SetGammaInfo(calcData._GammaConfig.Brightness, calcData._GammaConfig.Contrast);
        // hightlight
        SetHightLightInfo(calcData._HightLightConfig.Amount, calcData._HightLightConfig.Shadows,
            calcData._HightLightConfig.Midtones,
            calcData._HightLightConfig.Highlights);

        // postprocess
        if (_PostProcessChainComponent == null) return;
        var postprocessCfg = calcData._PostEffectConfig;
        Vector4 param = new Vector4(postprocessCfg._Fog._FogParamter0, postprocessCfg._Fog._FogParamter1,
            postprocessCfg._Fog._FogParamter2, postprocessCfg._Fog._FogParamter3);
        SetPostEffectInfo_Fog(postprocessCfg._Fog._FogEnabled, postprocessCfg._Fog._FogColor, param);

        rot = new Vector3(postprocessCfg._HSV.HsvParamter0, postprocessCfg._HSV.HsvParamter1,
            postprocessCfg._HSV.HsvParamter2);
        SetPostEffectInfo_HSV(postprocessCfg._HSV._HSVEnabled, rot);

        rot = new Vector3(postprocessCfg._DOF.DOFParamter0, postprocessCfg._DOF.DOFParamter1,
            postprocessCfg._DOF.DOFParamter2);
        SetPostEffectInfo_DOF(postprocessCfg._DOF._DOFEnabled, rot);

        rot = new Vector3(postprocessCfg._BrightnessAndContrast.BrightnessAndConstrastParamter0,
            postprocessCfg._BrightnessAndContrast.BrightnessAndConstrastParamter1,
            postprocessCfg._BrightnessAndContrast.BrightnessAndConstrastParamter2);
        SetPostEffectInfo_Brightness(rot);

        var startSpecialParam = new Vector2(postprocessCfg._SpecialVision.SpecialVisionParamter0,
            postprocessCfg._SpecialVision.SpecialVisionParamter1);
        SetPostEffectInfo_SpecialVision(postprocessCfg._SpecialVision.SpecialVisionEnabled, startSpecialParam);


        SetPostEffectInfo_BloomHD(postprocessCfg.Threshold, postprocessCfg.Intensity, postprocessCfg.Radius,
            postprocessCfg.Iteration, postprocessCfg.SoftKneeA, postprocessCfg.WithFlicker);

        var config = calcData._SkyConifg;
        SetShaderGlobalProperty(config._CubeMapBias, config._LightmapFeedback, config._ReflectionIntensity,
            config._RefColor, config._WindDirection, config._WindNoise, config._WindFrequency);

        if (_SkyComponent != null)
        {
            _SkyComponent.Blend(_CurLerpValue);
            _SkyComponent.SetGolbalPara(calcData._SkyConifg._LightmapFeedback
                , calcData._SkyConifg._AddonDirection
                , calcData._SkyConifg._HeadlightIntesity
                , calcData._SkyConifg._ReflectionIntensity
                , calcData._SkyConifg._RefColor
                , calcData._SkyConifg._WindDirection
                , calcData._SkyConifg._WindNoise
                , calcData._SkyConifg._WindFrequency
            );
        }


    }

    private bool IsRegionIdValid(int regionID)
    {
        if (null == _Config) return false;
        if (null == _Config._RegionIdGroup) return false;
        if (0 == _Config._RegionIdGroup.Count) return false;
        if (regionID >= _Config._RegionIdGroup.Count) return false;
        if (1 > _Config._RegionIdGroup[regionID].DayID) return false;
        return true;
    }

    /// 根据ID获取氛围数据
    private WeatherData GetConfigDataByID(int effectID)
    {
        if (effectID <= 0)
        {
            HobaDebuger.Log("effect id cannot be negative , Current Trigger is " + _CurTriggerType);
            return null;
        }

        if (null == _WeatherConfigData)
        {
            _WeatherConfigData = new WeatherSystemConfig();
            string path = Path.Combine(EntryPoint.Instance.ConfigPath, "WeatherConfigXml.xml");

            byte[] bytes = Util.ReadFile(path);
            if (bytes == null)
            {
                HobaDebuger.Log("WeatherConfigXml文件读取失败！ " + path);
                return null;
            }
            bool isReadSuccess = _WeatherConfigData.ParseFromXmlString(Encoding.UTF8.GetString(bytes));
            if (!isReadSuccess)
            {
                HobaDebuger.Log("WeatherConfigXml文件解析失败！ " + path);
                return null;
            }
        }

        for (int i = 0; i < _WeatherConfigData.WeatherDataList.Count; i++)
        {
            var data = _WeatherConfigData.WeatherDataList[i];
            if (data._DataGuid == effectID)
            {
                return data;
            }
        }
        return null;
    }

    #region 游戏对象初始化区
    private void InitGlobalLightObjAndComps()
    {
        if (null == _GlobalLight)
        {
            _GlobalLightObj = new GameObject("GlobalLight");
            _GlobalLightObj.transform.position = Vector3.zero;
            _GlobalLightObj.transform.rotation = Quaternion.Euler(56.67795f, 111.3001f, 310.5828f);
            _GlobalLightObj.transform.localScale = Vector3.one;
            _GlobalLight = _GlobalLightObj.AddComponent<Light>();
            _GlobalLight.type = LightType.Directional;
            _GlobalLight.color = new Color(1, 1, 1, 1);
            _GlobalLight.intensity = 1f;
            _GlobalLight.bounceIntensity = 0f;
            _GlobalLight.cullingMask = CUnityUtil.LayerMaskDefault | CUnityUtil.LayerMaskWater;
        }
    }

    private void InitPlayerLightObjAndComps()
    {
        if (null == _PlayerLight)
        {
            var tempTran = Main.PlayerLight;
            if (null == tempTran) return;

            _PlayerLight = tempTran.GetComponent<Light>();
            if (null == _PlayerLight)
                _PlayerLight = tempTran.AddComponent<Light>();

        }

        _SunComponent = _PlayerLight.GetComponent<Sun>();
        if (null == _SunComponent)
            _SunComponent = _PlayerLight.gameObject.AddComponent<Sun>();

        _SkyComponent = _PlayerLight.gameObject.GetComponent<Sky>();
        if (null == _SkyComponent)
            _SkyComponent = _PlayerLight.gameObject.AddComponent<Sky>();
    }

    private void InitPostEffectChainComp()
    {
        if (null == Main.Main3DCamera)
        {
            HobaDebuger.Log("cannot find main camera");
            return;
        }

        if (_PostProcessChainComponent == null)
        {
            _PostProcessChainComponent = Main.Main3DCamera.GetComponent<PostProcessChain>();
            if (null == _PostProcessChainComponent)
                _PostProcessChainComponent = Main.Main3DCamera.gameObject.AddComponent<PostProcessChain>();
        }

    }

    private void InitSkyBoxSphere()
    {
        if (_SkyBoxSphere == null)
        {
            Action<UnityEngine.Object> callback = (asset) =>
            {
                if (null == asset)
                {
                    HobaDebuger.Log("天空盒加载失败");
                    return;
                }
                _CirculationsphereObj = GameObject.Instantiate(asset) as GameObject;
                if (null == _CirculationsphereObj)
                {
                    HobaDebuger.Log("天空盒实例化失败");
                    return;
                }
                _CirculationsphereObj.layer = CUnityUtil.Layer_Background;

                _SkyBoxSphere = _CirculationsphereObj.GetComponentInChildren<MeshRenderer>();
                if (null != _SkyBoxSphere)
                {
                    //关闭投影
                    CUnityUtil.DisableLightAndShadow(_SkyBoxSphere);

                    if (_SkyComponent != null)
                    {
                        _SkyComponent.SkySphere = _SkyBoxSphere;
                        _SkyComponent.Reset();
                    }

                    foreach (var func in _SkyboxLoadedCallbacks)
                    {
                        if (func != null) func();
                    }

                    _SkyboxLoadedCallbacks[0] = null;
                    _SkyboxLoadedCallbacks[1] = null;

                    if (null != _CurrentData)
                    {
                        LoadSkyboxMaterial(_CurrentData._SkyBoxMatPath, _CurrentData._SkyBoxMatPath);
                        LoadSkyCubemap(_CurrentData._SkyConifg._CubeMapPath);
                        LoadStartEnvCubeMap(_CurrentData._SkyConifg._EnvCubeMapPath, DEFAULT_ENV_CUBEMAP_PATH, true);
                    }
                }
                else
                {
                    HobaDebuger.LogError("cannot load asset Circulation sphere");
                }

            };
            CAssetBundleManager.AsyncLoadResource(SKY_BOX_PREFAB_PATH, callback, false);
        }
        else
        {
            if (null == _CurrentData) return;
            LoadSkyboxMaterial(_CurrentData._SkyBoxMatPath, _CurrentData._SkyBoxMatPath);
            LoadSkyCubemap(_CurrentData._SkyConifg._CubeMapPath);
            LoadStartEnvCubeMap(_CurrentData._SkyConifg._EnvCubeMapPath, DEFAULT_ENV_CUBEMAP_PATH, true);
        }
    }
    #endregion

    #region 资源加载接口
    private void LoadSkyboxMaterial(string startSkyPath, string endSkyPath)
    {
        if (string.IsNullOrEmpty(startSkyPath) && string.IsNullOrEmpty(endSkyPath))
        {
            //HobaDebuger.Log("Failed to load SkyboxMaterial, bcz both startSkyPath and endSkyPath are null");
            if (null != _CirculationsphereObj)
                _CirculationsphereObj.SetActive(false);
            return;
        }
        if (null != _CirculationsphereObj)
            _CirculationsphereObj.SetActive(true);
        Action realCallback = () =>
        {
            if (!string.IsNullOrEmpty(startSkyPath) || !string.IsNullOrEmpty(endSkyPath))
            {
                bool loadOneMat = (startSkyPath != null && startSkyPath.Equals(endSkyPath)) ||
                                  string.IsNullOrEmpty(startSkyPath) ||
                                  string.IsNullOrEmpty(endSkyPath);
                // 前后材质球相同，不需要过渡效果
                if (loadOneMat)
                {
                    var path = startSkyPath;
                    if (string.IsNullOrEmpty(path))
                        path = endSkyPath;

                    Action<UnityEngine.Object> callback = (asset) =>
                    {
                        var matAsset = asset as Material;
                        if (null == matAsset || _SkyComponent == null)
                        {
                            HobaDebuger.Log("天空盒材质实例化失败");
                            return;
                        }

                        _SkyComponent.PushSkyMaterial(matAsset, matAsset);
                    };
                    CAssetBundleManager.AsyncLoadResource(path, callback, false);
                }
                else
                {
                    Material startMat = null, endMat = null;
                    Action<UnityEngine.Object> callback = (asset) =>
                    {
                        var matAsset = asset as Material;
                        if (null == matAsset || _SkyComponent == null)
                        {
                            HobaDebuger.Log("天空盒材质实例化失败");
                            return;
                        }

                        if (startSkyPath.Contains(matAsset.name))
                            startMat = matAsset;
                        else if (endSkyPath.Contains(matAsset.name))
                            endMat = matAsset;

                        if (startMat != null && endMat != null)
                        {
                            _SkyComponent.PushSkyMaterial(startMat, endMat);
                        }
                    };
                    CAssetBundleManager.AsyncLoadResource(startSkyPath, callback, false);
                    CAssetBundleManager.AsyncLoadResource(endSkyPath, callback, false);
                }
            }
        };

        if (null != _SkyBoxSphere)
            realCallback();
        else
            _SkyboxLoadedCallbacks[0] = realCallback;
    }


    private void LoadStartEnvCubeMap(string startCubeMapPath, string endCubeMapPath, bool isDefault = false)
    {
        if (string.IsNullOrEmpty(startCubeMapPath) || string.IsNullOrEmpty(endCubeMapPath)) return;
        Action<UnityEngine.Object> callback = (asset) =>
        {
            var cubeMap = asset as Cubemap;
            if (null == cubeMap || _SkyComponent == null)
            {
                HobaDebuger.LogFormat("Env CubeMap Init Failed and path is {0}", startCubeMapPath);
                return;
            }
            LoadEnvCubeMapEnd(cubeMap, endCubeMapPath, isDefault);
        };
        CAssetBundleManager.AsyncLoadResource(startCubeMapPath, callback, false);
    }
    private void LoadEnvCubeMapEnd(Cubemap startCubeMap, string endCubeMapPath, bool isDefault)
    {
        if (isDefault)
        {
            endCubeMapPath = DEFAULT_ENV_CUBEMAP_PATH;

            if (null != _DefaultCubeMap && null != _SkyComponent)
            {
                _SkyComponent.SetEnvCubeMap(startCubeMap, _DefaultCubeMap, 0);
                return;
            }
        }
        Action<UnityEngine.Object> callback = (asset) =>
        {
            var cubeMap = asset as Cubemap;
            if (null == cubeMap || _SkyComponent == null)
            {
                HobaDebuger.LogFormat("Env CubeMap Init Failed and path is {0}", endCubeMapPath);
                return;
            }
            if (isDefault)
            {
                _DefaultCubeMap = cubeMap;
            }
            _SkyComponent.SetEnvCubeMap(startCubeMap, cubeMap, 0);
        };
        CAssetBundleManager.AsyncLoadResource(endCubeMapPath, callback, false);




    }

    private void LoadSkyCubemap(string cubeMapPath)
    {
        Action realCallback = () =>
        {
            if (null == _SkyComponent) return;
            if (string.IsNullOrEmpty(cubeMapPath)) return;

            Action<UnityEngine.Object> callback = (asset) =>
            {
                if (_SkyComponent == null)
                {
                    HobaDebuger.LogWarning("SkyComponent can not be null");
                    return;
                }
                if (null != asset && _SkyComponent != null)
                    _SkyComponent.SkyCubeMap = asset as Cubemap;
            };
            CAssetBundleManager.AsyncLoadResource(cubeMapPath, callback, false);
        };

        if (null != _SkyBoxSphere)
            realCallback();
        else
            _SkyboxLoadedCallbacks[1] = realCallback;
    }

    private void LoadSceneSfx(string sfxPath)
    {
        if (null != _CurrentSfxObj)
        {
            UnityEngine.Object.Destroy(_CurrentSfxObj);
            _CurrentSfxObj = null;
        }
        _CurrentSfxGuid = _CurrentSfxGuid + 1;

        _CurrentSfxObjPath = sfxPath;

        if (string.IsNullOrEmpty(sfxPath)) return;

        var sfxGuid = _CurrentSfxGuid;
        Action<UnityEngine.Object> callback = (asset) =>
        {
            if (_CurrentSfxObjPath != sfxPath) return;
            if (sfxGuid != _CurrentSfxGuid) return;
            if (null != asset)
            {
                _CurrentSfxObj = GameObject.Instantiate(asset) as GameObject;
                var followPos = _CurrentSfxObj.GetComponent<FollowHostPlayer>();
                if (null == followPos)
                    followPos = _CurrentSfxObj.AddComponent<FollowHostPlayer>();
                followPos.HostPlayer = Main.HostPalyer;
            }
            else
            {
                HobaDebuger.LogFormat("Failed to load SceneSfx asset: {0}", sfxPath);
            }
        };
        CAssetBundleManager.AsyncLoadResource(sfxPath, callback, false);
    }
    #endregion

    #region 参数修改私有接口
    private void SetGlobalLightInfo(Color color, Vector3 rotation, float intensity)
    {
        if (null == _GlobalLight) return;
        _GlobalLight.color = color;
        _GlobalLight.transform.rotation = Quaternion.Euler(rotation);
        _GlobalLight.intensity = intensity;
    }

    private void SetGammaInfo(float brightness, float contrast)
    {
        if (null == _PostProcessChainComponent) return;
        var bcgComp = _PostProcessChainComponent.BCGammaComp;
        if (null == bcgComp) return;

        bcgComp.Brightness = brightness;
        bcgComp.Contrast = contrast;
    }

    private void SetHightLightInfo(float amount, Color shadows, Color midtones, Color highLight)
    {
        if (null == _PostProcessChainComponent) return;
        var smhComp = _PostProcessChainComponent.SMHComp;
        if (null == smhComp) return;

        smhComp.Amount = amount;
        smhComp.Shadows = shadows;
        smhComp.Midtones = midtones;
        smhComp.Highlights = highLight;
    }
    private void SetPlayerLightInfo(Color color, Vector3 rotation, float intensity, float strength, float bias, float normalBias, float shadowNearPlane)
    {
        if (null == _PlayerLight) return;
        _PlayerLight.color = color;
        _PlayerLight.transform.rotation = Quaternion.Euler(rotation);
        _PlayerLight.intensity = intensity;
        _PlayerLight.shadowBias = bias > 0.0f ? bias : 0.05f;
        _PlayerLight.shadowNormalBias = normalBias > 0.0f ? normalBias : 0.4f;
        _PlayerLight.shadowStrength = strength;
        _PlayerLight.shadowNearPlane = shadowNearPlane > 0.0f ? shadowNearPlane : 0.1f;
    }

    private void SetPostEffectInfo_Fog(bool enableFog, Color fogColor, Vector4 fogParamter)
    {
        if (null == _PostProcessChainComponent) return;
        if (GFXConfig.Instance.IsUsePostProcessFog)
        {
            _PostProcessChainComponent.EnableFog = enableFog;
            _PostProcessChainComponent.fog_color = fogColor;
            _PostProcessChainComponent.fog_paramter = fogParamter;
        }
        else
        {
            _PostProcessChainComponent.EnableFog = false;
        }
    }

    private void SetPostEffectInfo_HSV(bool enableHSVAdjust, Vector3 hsvAdjustParamters)
    {
        if (null == _PostProcessChainComponent) return;
        if (_IsPlayerDead)
        {
            _PostProcessChainComponent.EnableHsvAdjust = true;
            _PostProcessChainComponent._hsv_adjust_paramters = _HSVOnDeath;
        }
        else if (GFXConfig.Instance.IsEnableHSV)
        {
            _PostProcessChainComponent.EnableHsvAdjust = enableHSVAdjust;
            _PostProcessChainComponent._hsv_adjust_paramters = hsvAdjustParamters;
        }
        else
        {
            _PostProcessChainComponent.EnableHsvAdjust = false;
        }
    }

    private void SetPostEffectInfo_DOF(bool enableDof, Vector3 dofAdjustParamters)
    {
        if (null == _PostProcessChainComponent) return;
        if (GFXConfig.Instance.IsEnableDOF)
        {
            if (_IsChangeDOF)
            {
                _PostProcessChainComponent.EnableDepthOfField = true;
                _PostProcessChainComponent.depthoffield_paramter = _ChangedDOFParam;
            }
            else
            {
                _PostProcessChainComponent.EnableDepthOfField = enableDof;
                _PostProcessChainComponent.depthoffield_paramter = dofAdjustParamters;
            }
        }
        else
        {
            _PostProcessChainComponent.EnableDepthOfField = false;
        }
    }

    private void SetPostEffectInfo_Brightness(Vector3 brightnessContrastParamters)
    {
        if (null == _PostProcessChainComponent) return;
        _PostProcessChainComponent.brightness_contrast_paramters = brightnessContrastParamters;
    }

    private void SetPostEffectInfo_SpecialVision(bool enabled, Vector2 param)
    {
        if (null == _PostProcessChainComponent) return;
        _PostProcessChainComponent.EnableSpecialVision = enabled;
        _PostProcessChainComponent.special_vision_paramters = param;
    }

    private void SetPostEffectInfo_BloomHD(float threshold, float intensity, float radius, int iteration, float softKneeA, bool withFlicker)
    {

        if (null == _PostProcessChainComponent) return;
        if (GFXConfig.Instance.IsEnableBloomHD)
        {
            _PostProcessChainComponent.EnableBloomHD = true;
            _PostProcessChainComponent.BloomHDComp.Threshold = threshold;
            _PostProcessChainComponent.BloomHDComp.Intensity = intensity;
            _PostProcessChainComponent.BloomHDComp.Radius = radius;
            _PostProcessChainComponent.BloomHDComp.Iteration = iteration;
            _PostProcessChainComponent.BloomHDComp.SoftKneeA = softKneeA;
            _PostProcessChainComponent.BloomHDComp.WithFlicker = withFlicker;
        }
        else
        {
            _PostProcessChainComponent.EnableBloomHD = false;
        }
    }

    private void SetShaderGlobalProperty(int cubemapBias, float lightMapFeedBack, float reflectionIntensity, Color refColor, Vector4 windDirection, float windNoise, float windFrequency)
    {
        // config._HeadlightIntesity 这个参数暂时是未使用的
        Shader.SetGlobalInt(ShaderIDs.DynEnvLodBias, cubemapBias);
        Shader.SetGlobalFloat(ShaderIDs.DynNight, lightMapFeedBack);
        Shader.SetGlobalFloat(ShaderIDs.DynRefint, reflectionIntensity);
        Shader.SetGlobalColor(ShaderIDs.DynReflectionColor, refColor);
        Shader.SetGlobalVector(ShaderIDs.DynWind, windDirection);
        Shader.SetGlobalFloat(ShaderIDs.DynWindnoise, windNoise);
        Shader.SetGlobalFloat(ShaderIDs.DynWindfreq, windFrequency);
    }
    private void SetFogInfo(bool isFogOn, Color fogColor, FogMode fogMode, float fogStartDistance, float fogEndDistance)
    {
        RenderSettings.fog = isFogOn;
        RenderSettings.fogColor = fogColor;
        RenderSettings.fogMode = fogMode;
        RenderSettings.fogStartDistance = fogStartDistance;
        RenderSettings.fogEndDistance = fogEndDistance;
    }

    private void SetEnvInfo(Color ambientEquatorColor, Color ambientGroundColor, Color ambientSkyColor, float ambientIntensity)
    {
        RenderSettings.ambientEquatorColor = ambientEquatorColor;
        RenderSettings.ambientGroundColor = ambientGroundColor;
        RenderSettings.ambientSkyColor = ambientSkyColor;
        RenderSettings.ambientIntensity = ambientIntensity;
    }

    private void SetSunInfo(Color sunColor, float sunColorIntensity, Color ambientColor, float ambientColorIntensity)
    {
        if (null == _SunComponent) return;
        _SunComponent.SunColor = sunColor;
        _SunComponent.SunColorIntensity = sunColorIntensity;
        _SunComponent.AmbientColor = ambientColor;
        _SunComponent.AmbientColorIntensity = ambientColorIntensity;
        _SunComponent.Apply();
    }

    #endregion

    public void Tick(float dt)
    {
        if (_Config == null) return;

        #region 检查主角所在区域是否变化，昼夜是否变化
        float now = Time.time;
        if (now - _LastTime > 2)
        {
            _LastTime = now;

            if (_CurTriggerType == EffectTriggerType.None)
            {
                var newRegionId = ScenesRegionManager.Instance.CurLightmapRegionID;
                if (-1 != newRegionId && (newRegionId != _CurrentRegionId))
                {
                    if (IsRegionIdValid(newRegionId))
                    {
                        _CurrentRegionId = newRegionId;
                        _CurTriggerType = EffectTriggerType.RegionChanged;
                    }
                }
            }
            //HobaDebuger.LogFormat()
            // 为简化逻辑，优先进行区域切换，区域完成后，再做昼夜切换
            // 可能存在的漏洞：玩家在两个区域之间来回切换，昼夜变化将不再生效
            var newType = _DynEffectCalculator.GetDynamicEffectType();
            if (_CurrentEffectType != newType)
            {
                LuaScriptMgr.Instance.CallLuaOnWeatherEventFunc((int)newType);
            }
            if (_CurTriggerType == EffectTriggerType.None)
            {
                if (_CurrentEffectType != newType)
                {
                    if (_CurrentEffectType == DynamicEffectType.Dusk && newType == DynamicEffectType.Night)
                    {
                        ScenesAnimationManager.Instance.Init(true);
                        ScenesAnimationManager.Instance.OpenLightAnimaiotn();
                    }
                    else if ( newType == DynamicEffectType.Morning)
                    {
                        ScenesAnimationManager.Instance.Init(false);
                        ScenesAnimationManager.Instance.CloseAnimaiontion();
                    }
                    _CurrentEffectType = newType;
                    _CurTriggerType = EffectTriggerType.TimeChanged;
                   
                }
            }

        }
        #endregion

        CalculateEffectLerpData();

        if (_IsInLerping)
        {
            if (_LerpTime < 1e-3)
                _CurLerpValue = 1;
            else
                _CurLerpValue += (Time.deltaTime / _LerpTime);

            if (!GFXConfig.Instance.IsUseWeatherLerp || _CurLerpValue > 0.99f)
            {
                _IsInLerping = false;
                _CurLerpValue = 1f;
                _CurTriggerType = EffectTriggerType.None;
            }
            DoEffectDataLerp(_LerpFromData, _LerpToData, _CurLerpValue);
            SetFinallyWeatherData(_CurrentData);
        }
    }

    #region 效果外部触发接口
    public void EnterUIDynamicEffect(int effectID)
    {
        _CurTriggerType = effectID > 0 ? EffectTriggerType.EnterUIEffect : EffectTriggerType.LeaveUIEffect;
        _CurUIEffectId = effectID;
    }

    public void EnableSpecialVisionEffect(bool isOn)
    {
        _CurTriggerType = isOn ? EffectTriggerType.EnterHawkEffect : EffectTriggerType.LeaveHawkEffect;
        _CurHawkEffectId = isOn ? HAWKEYE_EFFECT_ID : 0;
    }

    public void EnterWeatherDynamicEffect(WeatherType weatherType)
    {
        if (_CurrentWeatherType == weatherType) return;

        _CurTriggerType = weatherType != WeatherType.None ? EffectTriggerType.EnterWeatherEffect : EffectTriggerType.LeaveWeatherEffect;
        _CurrentWeatherType = weatherType;

        if (weatherType == WeatherType.None)
        {
            LuaScriptMgr.Instance.CallLuaOnWeatherEventFunc((int)_CurrentEffectType);
        }
        else
        {
            //显示雨雪？
        }
    }

    public void EnterCGEffect(int effectID, PostProcessChain[] postChainComponets)
    {
        // 退出当前CG效果

        if (effectID == -1 && postChainComponets == null)
        {
            //Debug.LogError("Leave CG Effect" + effectID);
            _CurTriggerType = EffectTriggerType.LeaveCGEffect;
            _CurCGEffectId = 0;
        }
        else
        {
            //Debug.LogError("Enter CG" + effectID);
            _CurCGEffectId = effectID;
            _CurTriggerType = EffectTriggerType.EnterCGEffect;

            // CG中可能会操作非主相机的postChainComponets
            #region CG PostChainComponets修改
            var data = GetConfigDataByID(effectID);
            if (null != data && postChainComponets != null)
            {
                for (int i = 0; i < postChainComponets.Length; i++)
                {
                    var postChainComponet = postChainComponets[i];
                    if (null != postChainComponets[i])
                    {
                        postChainComponet.EnableFog = data._PostEffectConfig._Fog._FogEnabled;
                        postChainComponet.EnableHsvAdjust = data._PostEffectConfig._HSV._HSVEnabled;
                        postChainComponet.EnableDepthOfField = data._PostEffectConfig._DOF._DOFEnabled;
                        postChainComponet.EnableBloomHD = GFXConfig.Instance.IsEnableBloomHD;
                        postChainComponet.EnableSpecialVision = data._PostEffectConfig._SpecialVision.SpecialVisionEnabled;
                        Vector3 rot = Vector3.zero;
                        Vector4 param = new Vector4(data._PostEffectConfig._Fog._FogParamter0
                            , data._PostEffectConfig._Fog._FogParamter1
                            , data._PostEffectConfig._Fog._FogParamter2
                            , data._PostEffectConfig._Fog._FogParamter3);

                        postChainComponet.fog_color = data._PostEffectConfig._Fog._FogColor;
                        postChainComponet.fog_paramter = param;

                        rot = new Vector3(data._PostEffectConfig._HSV.HsvParamter0,
                            data._PostEffectConfig._HSV.HsvParamter1,
                            data._PostEffectConfig._HSV.HsvParamter2);
                        postChainComponet._hsv_adjust_paramters = rot;


                        rot = new Vector3(data._PostEffectConfig._DOF.DOFParamter0,
                            data._PostEffectConfig._DOF.DOFParamter1,
                            data._PostEffectConfig._DOF.DOFParamter2);

                        postChainComponet.depthoffield_paramter = rot;

                        rot = new Vector3(data._PostEffectConfig._BrightnessAndContrast.BrightnessAndConstrastParamter0,
                            data._PostEffectConfig._BrightnessAndContrast.BrightnessAndConstrastParamter1,
                            data._PostEffectConfig._BrightnessAndContrast.BrightnessAndConstrastParamter2);
                        postChainComponet.brightness_contrast_paramters = rot;
                    }
                }
            }


            #endregion
        }
    }

    public void EnableDepthOfField(bool enable, float distance, float range, float bokeh)
    {
        _IsChangeDOF = enable;
        _ChangedDOFParam = new Vector3(distance, range, bokeh);
        SetFinallyWeatherData(_CurrentData);
    }

    public void EnablePlayerDeathEffect(bool isDead)
    {
        _IsPlayerDead = isDead;
        SetFinallyWeatherData(_CurrentData);
    }

    public void ChangeWeatherByEffectID(int effectID)
    {
        if (effectID <= 0) return;
        _ForceChangedEffectID = effectID;
        _CurTriggerType = EffectTriggerType.ForceChanged;
    }

    public void ChangeWeatherByMemory(int effectID)
    {
        //if (effectID < 0) return;
        //_MomoryChangedEffectID = effectID;
        //_CurTriggerType = EffectTriggerType.EnterMemoryEffect;


        _CurTriggerType = effectID > 0 ? EffectTriggerType.EnterMemoryEffect : EffectTriggerType.LeaveMemoryEffect;
        _MomoryChangedEffectID = effectID;

    }
    public void OnDebugCmd(DynamicEffectType cmd)
    {
        _DebugWeatherType = cmd;
        _CurTriggerType = EffectTriggerType.Debug;
    }
    #endregion

    #region 效果LOD设置接口
    public void OnGfxConfigPostProcessChanged()
    {
        SetFinallyWeatherData(_CurrentData);
    }
    #endregion

    public DynamicEffectType GetDynamicEffectType()
    {
        return _CurrentEffectType;
    }

    public void SetServerStartTime(string timeString, double currentServerTime)
    {
        _DynEffectCalculator.SetServerStartTime(timeString, currentServerTime);
    }

    public void Cleanup()
    {
        _SkyboxLoadedCallbacks[0] = null;
        _SkyboxLoadedCallbacks[1] = null;

        if (null != _CurrentSfxObj)
            GameObject.Destroy(_CurrentSfxObj);
        _CurrentSfxObj = null;
        _CurrentSfxObjPath = null;

        _Config = null;

        _CurrentRegionId = -1;
        _CurTriggerType = EffectTriggerType.None;
        _CurrentEffectType = DynamicEffectType.None;
        _LerpTime = 0f;
        _IsInLerping = false;
        _CurLerpValue = 1f;

        _LerpFromData = null;
        _LerpToData = null;
        _CurrentData = null;
    }

    public void Release()
    {
        Cleanup();

        if (_GlobalLightObj != null)
        {
            GameObject.Destroy(_GlobalLightObj);
            _GlobalLightObj = null;
        }
        _GlobalLight = null;

        _PlayerLight = null;
        if (_SkyComponent != null)
        {
            _SkyComponent.Release();
            _SkyComponent = null;
        }
        _SunComponent = null;

        if (_CirculationsphereObj != null)
        {
            GameObject.Destroy(_CirculationsphereObj);
            _CirculationsphereObj = null;
        }
        _SkyBoxSphere = null;

        _PostProcessChainComponent = null;

        _CurrentWeatherType = WeatherType.None;
        _CurHawkEffectId = 0;
        _CurUIEffectId = 0;
        _CurCGEffectId = 0;
        _IsPlayerDead = false;
        _IsChangeDOF = false;
        _ChangedDOFParam = Vector3.zero;

        _WeatherConfigData = null;

        _DynEffectCalculator.Release();

    }
}
