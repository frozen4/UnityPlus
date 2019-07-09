using UnityEngine;
using System.Collections.Generic;

#region ConfigItem
class _ConfigItem
{
    string _name;
    int _max;
    int _level;
    bool _dirty;
    System.Action _function;

    public _ConfigItem(string name, int level, int max, System.Action func)
    {
        _name = name;
        _max = max;
        _level = level;
        _function = func;
    }

    public string Name { get { return _name; } }
    public int Max { get { return _max; } }
    public int Level
    {
        get { return _level; }
        set
        {
            int new_value = Mathf.Clamp(value, 0, Max);
            _dirty = true;
            _level = new_value;
        }
    }

    public void Apply()
    {
        if (_dirty)
        {
            if (_function != null) _function();
            _dirty = false;
        }
    }
}

#endregion

public delegate void PostProcessChainConfigChanged();

public class GFXConfig
{
    static GFXConfig _instance = null;
    public static GFXConfig Instance 
    { 
        get 
        {
            if (_instance == null)
                _instance = new GFXConfig();
            return _instance;
        } 
    }

    public static void CreateGFXConfig()
    {
        if (_instance == null) _instance = new GFXConfig();
    }

    private bool _IsUsePostProcessFog = false;
    public bool IsUsePostProcessFog         //高度雾
    {
        get { return _IsUsePostProcessFog; }
        set 
        { 
            _IsUsePostProcessFog = value;
            OnConfigChanged();
        }
    }       

    public bool IsUseWeatherEffect      //天气特效
    {
#if IN_GAME
        get { return ScenesManager.Instance.IsUseWeatherEffect; }
        set { ScenesManager.Instance.IsUseWeatherEffect = value; }
#else
        get; set;
#endif
    }

    private bool _IsEnableWaterReflection = false;
    public bool IsEnableWaterReflection
    {
        get { return _IsEnableWaterReflection; }
        set 
        {
            _IsEnableWaterReflection = value;
#if IN_GAME
            ScenesManager.Instance.UpdateWaterReflection();
#endif
        }
    }

    private bool _IsEnableDOF = false;
    public bool IsEnableDOF
    {
        get { return _IsEnableDOF; }
        set
        {
            _IsEnableDOF = value;
            OnConfigChanged();
        }
    }

    public bool IsUseDetailFootStepSound { get; set; }      //细节音效

    public int FPSLimit 
    {
        get 
        {
            return Application.targetFrameRate; 
        }
        set 
        {
#if UNITY_EDITOR
            Application.targetFrameRate = 200;
#else
            Application.targetFrameRate = value;
#endif
        }
    }

    public EFxLODLevel GetFxLODLevel() 
    {
        if (_fx_level.Level == 0)
            return EFxLODLevel.L0;
        else if (_fx_level.Level == 1)
            return EFxLODLevel.L1;
        else
            return EFxLODLevel.L3;
    }

    public float GetDynamicBoneDistance()
    {
        if (_character_level.Level == 0)
            return 0;
        else if (_character_level.Level == 1)
            return 10;
        else
            return 20;
    }

    private _ConfigItem _postprocess_level;     //后处理质量    4
    private _ConfigItem _shadow_level;          //阴影质量      4
    private _ConfigItem _character_level;       //角色效果      3
    private _ConfigItem _scenedetail_level;     //场景细节      3
    private _ConfigItem _fx_level;              //特效质量      3

    public int PostProcessLevel
    {
        get { return _postprocess_level.Level; }
        set { _postprocess_level.Level = value; }
    }

    public int ShadowLevel
    {
        get { return _shadow_level.Level; }
        set { _shadow_level.Level = value; }
    }

    public int CharacterLevel
    {
        get { return _character_level.Level; }
        set { _character_level.Level = value; }
    }

    public int SceneDetailLevel
    {
        get { return _scenedetail_level.Level; }
        set { _scenedetail_level.Level = value; }
    }

    public int FxLevel
    {
        get { return _fx_level.Level; }
        set { _fx_level.Level = value; }
    }

    private void OnConfigChanged()
    {
#if IN_GAME
        DynamicEffectManager.Instance.OnGfxConfigPostProcessChanged();
#endif
    }
   
    public void Init()
    {
        _postprocess_level = new _ConfigItem("PostProcessLevel", 0, 3, _PostProcessLevelChanged);                 //水面反射开关
        _shadow_level = new _ConfigItem("ShadowLevel", 0, 2, _ShadowLevelChanged);             //是否实时阴影开关

        _character_level = new _ConfigItem("CharacterLevel", 0 , 2, _CharacterLevelChanged);
        _scenedetail_level = new _ConfigItem("ScenesDetailLevel", 0, 2, _ScenesDetailLevelChanged);
		_fx_level = new _ConfigItem("FxLevel", 0, 2, null);

        //初始化
        IsEnableWaterReflection = false;
        IsEnableDOF = false;
        IsUsePostProcessFog = false;
        IsUseWeatherEffect = true;
        IsUseDetailFootStepSound = true;

        IsEnableBloomHD = true;
        IsEnableMotionBlur = true;
        IsEnableRadialBlur = true;
        IsEnableBrightnessContrastGamma = true;
        IsEnableShadowMidtoneHighlights = true;

        FPSLimit = 30;   
    }

    public bool IsEnableRealTimeShadow
    {
        get { return _shadow_level.Level > 0; }
    }

    public bool IsUseWeatherLerp
    {
        get { return _shadow_level.Level > 0; }
    }

    public bool IsUseSimpleBloomHD          //使用简化版BloomHD
    {
        get { return _postprocess_level.Level == 1; }
    }

    public bool IsEnableHSV
    {
        get;
        private set;
    }

    public bool IsEnableBloomHD
    {
        get;
        private set;
    }

    public bool IsEnableMotionBlur
    {
        get;
        private set;
    }

    public bool IsEnableRadialBlur
    {
        get;
        private set;
    }

    public bool IsEnableBrightnessContrastGamma
    {
        get;
        private set;
    }

    public bool IsEnableShadowMidtoneHighlights
    {
        get;
        private set;
    }

    void _PostProcessLevelChanged()
    {
//         if (_postprocess_level.Level == 0)
//         {
//             IsEnableMotionBlur = false;
//             IsEnableRadialBlur = false;
//             IsEnableBloomHD = false;
//             IsEnableHSV = false;
//             IsEnableBrightnessContrastGamma = false;
//             IsEnableShadowMidtoneHighlights = false;
//         }
        if (_postprocess_level.Level == 0)
        {
            IsEnableMotionBlur = false;
            IsEnableRadialBlur = false;
            IsEnableBloomHD = false;
            IsEnableHSV = true;
            IsEnableBrightnessContrastGamma = true;
            IsEnableShadowMidtoneHighlights = true;
        }
        else if (_postprocess_level.Level == 1)
        {
            IsEnableMotionBlur = false;
            IsEnableRadialBlur = true;
            IsEnableBloomHD = true;
            IsEnableHSV = true;
            IsEnableBrightnessContrastGamma = true;
            IsEnableShadowMidtoneHighlights = true;
        }
        else if (_postprocess_level.Level == 2)
        {
            IsEnableMotionBlur = true;
            IsEnableRadialBlur = true;
            IsEnableBloomHD = true;
            IsEnableHSV = true;
            IsEnableBrightnessContrastGamma = true;
            IsEnableShadowMidtoneHighlights = true;
        }
        else  //关闭
        {
            IsEnableMotionBlur = false;
            IsEnableRadialBlur = false;
            IsEnableBloomHD = false;
            IsEnableHSV = false;
            IsEnableBrightnessContrastGamma = false;
            IsEnableShadowMidtoneHighlights = false;
        }
     
        OnConfigChanged();
    }

    void _ShadowLevelChanged()
    {
        if (_shadow_level.Level == 0)
        {
            if (Main.PlayerLight != null)
            {
                var light = Main.PlayerLight.GetComponent<Light>();
                if (light != null)
                    light.shadows = LightShadows.None;

                light.cullingMask = CUnityUtil.LayerMaskShadows_L0;

                Light ui_lit = Main.UILight.GetComponent<Light>();
                if (ui_lit != null)
                {
                    ui_lit.shadows = LightShadows.None;
                }
            }
            QualitySettings.shadowDistance = 10;
            QualitySettings.shadowResolution = ShadowResolution.Medium;
        }
        else if (_shadow_level.Level == 1)
        {
            if (Main.PlayerLight != null)
            {
                var light = Main.PlayerLight.GetComponent<Light>();
                if (light != null)
                    light.shadows = LightShadows.Hard;

                light.cullingMask = CUnityUtil.LayerMaskShadows_L0;
                
                Light ui_lit = Main.UILight.GetComponent<Light>();
                if (ui_lit != null)
                {
                    ui_lit.shadows = LightShadows.Hard;
                }
            }
            QualitySettings.shadowDistance = 10;
            QualitySettings.shadowResolution = ShadowResolution.Medium;
        }
        else 
        {
            if (Main.PlayerLight != null)
            {
                var light = Main.PlayerLight.GetComponent<Light>();
                if (light != null)
                    light.shadows = LightShadows.Hard;

                light.cullingMask = CUnityUtil.LayerMaskShadows_L2;
                
                Light ui_lit = Main.UILight.GetComponent<Light>();
                if (ui_lit != null)
                {
                    ui_lit.shadows = LightShadows.Hard;
                }
            }
            QualitySettings.shadowDistance = 12;
            QualitySettings.shadowResolution = ShadowResolution.High;
        }
    }

    void _CharacterLevelChanged()
    {   
#if IN_GAME
        var outwardManagerList = EntityComponent.OutwardComponent.GetAll();
        if (outwardManagerList != null)
        {
            for (int i = 0; i < outwardManagerList.Count; ++i)
            {
                for (int k = 0; k < outwardManagerList[i].DynamicBonesCompList.Length; ++k)
                {
                    var dynamicBonesList = outwardManagerList[i].DynamicBonesCompList[k];
                    foreach (var dynamicBone in dynamicBonesList)
                    {
                        if(dynamicBone != null) 
                            dynamicBone.m_DistanceToObject = GetDynamicBoneDistance();
                    }
                }
            }
        }
#endif

        if (true) //(Main.IsInGame())
        {
            if (_character_level.Level == 0)
            {
                QualitySettings.blendWeights = BlendWeights.TwoBones;
                Shader.globalMaximumLOD = 200;
            }
            else if (_character_level.Level == 1)
            {
                QualitySettings.blendWeights = BlendWeights.FourBones;
                Shader.globalMaximumLOD = 400;
            }
            else
            {
                QualitySettings.blendWeights = BlendWeights.FourBones;
                Shader.globalMaximumLOD = 600;
            }
        }
//         else
//         {
//             QualitySettings.blendWeights = BlendWeights.FourBones;
//             Shader.globalMaximumLOD = 600;
//         }
    }

    private void _ScenesDetailLevelChanged()
    {
#if IN_GAME
        if (_scenedetail_level.Level == 0)
            ScenesManager.Instance.ChangeDetailLevel(1);
        else if (_scenedetail_level.Level == 1)
            ScenesManager.Instance.ChangeDetailLevel(2);
        else
            ScenesManager.Instance.ChangeDetailLevel(3);
#endif
    }

    public void ApplyChange()
    {
        _postprocess_level.Apply();
        _shadow_level.Apply();
        _character_level.Apply();
        _scenedetail_level.Apply();
        _fx_level.Apply();
    }


}
