using UnityEngine;

static class ShaderIDs
{
    // Pre-hashed shader ids - naming conventions are a bit off in this file as we use the same
    // fields names as in the shaders for ease of use

    // sky
    internal static readonly int GlobalTexture = Shader.PropertyToID("_EnvMap");

    // EntityEffectManager
    internal static readonly int RimColor = Shader.PropertyToID("_RimColor");

    ///outward manager
    internal static readonly int AdditionTex = Shader.PropertyToID("_AdditionTex");
    internal static readonly int AdditionNormal = Shader.PropertyToID("_AdditionNormal");
    internal static readonly int AdditionSpecular = Shader.PropertyToID("_AdditionSpecular");
    internal static readonly int AdditionOffset = Shader.PropertyToID("_AdditionOffset");
    internal static readonly int HairColorCustom = Shader.PropertyToID("_HairColorCustom");
    internal static readonly int SkinColor = Shader.PropertyToID("_SkinColor");
    internal static readonly int HeroAdditionOn = Shader.PropertyToID("HERO_ADDITION_ON");
    internal static readonly int HeroAdditionOff = Shader.PropertyToID("HERO_ADDITION_OFF");

    //// aquas
    internal static readonly int ReflectionTex = Shader.PropertyToID("_ReflectionTex");
    internal static readonly int AquasWaterLevel = Shader.PropertyToID("_WaterLevel");
    internal static readonly int AquasDepthFade = Shader.PropertyToID("_DepthFade");
    internal static readonly int AquasUseReflections = Shader.PropertyToID("_UseReflections");
    internal static readonly int AquasTexture = Shader.PropertyToID("_Texture");

    ///sfx
    internal static readonly int TwistScale = Shader.PropertyToID("_TwistScale");
    internal static readonly int TintColor = Shader.PropertyToID("_TintColor");
    internal static readonly int Color = Shader.PropertyToID("_Color");

    ///EntityEffect
    internal static readonly int ShaderPropertyRimColor = Shader.PropertyToID("_RimColor");
    internal static readonly int ShaderPropertyRimPower = Shader.PropertyToID("_RimPower");
    internal static readonly int ShaderPropertyLevelLoadedTime = Shader.PropertyToID("_SinceLevelLoadedTime");
    internal static readonly int ShaderPropertyDeathDuration = Shader.PropertyToID("_DeathDuration");
    internal static readonly int ShaderPropertyDeathColor = Shader.PropertyToID("_DeathColor");
    internal static readonly int ShaderPropertyTransparent = Shader.PropertyToID("_Transparent");
    internal static readonly int ShaderPropertyDissolveWidth = Shader.PropertyToID("_DissolveWidth");
    internal static readonly int FlakeColor1 = Shader.PropertyToID("_FlakeColor1");
    internal static readonly int FlakeColor2 = Shader.PropertyToID("_FlakeColor2");
    internal static readonly int SinceLevelLoadedTime = Shader.PropertyToID("_SinceLevelLoadedTime");
    internal static readonly int DeathDuration = Shader.PropertyToID("_DeathDuration");
    
    //MaterialUtility
    internal static readonly int SrcBlend = Shader.PropertyToID("_SrcBlend");
    internal static readonly int DstBlend = Shader.PropertyToID("_DstBlend");
    internal static readonly int ZWrite = Shader.PropertyToID("_ZWrite");

    /// Ghost effect 
    internal static readonly int GhostEffectColor = Shader.PropertyToID("_Color");
    internal static readonly int GhostEffectPower = Shader.PropertyToID("_MainPower");

    /// UI
    internal static readonly int UIAlphaTex = Shader.PropertyToID("_AlphaTex");
    internal static readonly int UIBlurOffsets = Shader.PropertyToID("_BlurOffsets");
    internal static readonly int UIRect = Shader.PropertyToID("_Rect");
    internal static readonly int UIClipArea = Shader.PropertyToID("_ClipArea");
    internal static readonly int UIClipParam = Shader.PropertyToID("_ClipParam");

    /// BLoom
    internal static readonly int ThresholdId = Shader.PropertyToID("_Threshold");
    internal static readonly int CurveId = Shader.PropertyToID("_Curve");
    internal static readonly int UpSampleScaleId = Shader.PropertyToID("_UpSampleScale");
    internal static readonly int IntensityId = Shader.PropertyToID("_Intensity");
    internal static readonly int BloomTexId = Shader.PropertyToID("_BloomTex");

    ///EdgeLighting
    internal static readonly int EdgeLightingColor = Shader.PropertyToID("_RimColor");
    internal static readonly int EdgeLightingRimPower = Shader.PropertyToID("_RimPower");

    ///EdgeLightOutLine
    internal static readonly int EdgeLightingOutlineColor = Shader.PropertyToID("_OutlineColor");
    internal static readonly int EdgeLightingOutline = Shader.PropertyToID("_Outline");
    internal static readonly int EdgeLightingOutLineRimPower = Shader.PropertyToID("_RimPower");
    internal static readonly int EdgeLightingOutlineColor2 = Shader.PropertyToID("_OutlineColor2");
    internal static readonly int EdgeLightingOutline2 = Shader.PropertyToID("_Outline2");
    internal static readonly int EdgeLightingRimPower2 = Shader.PropertyToID("_RimPower2");
    internal static readonly int EdgeLightingLightInfo = Shader.PropertyToID("_LightInfo");
    internal static readonly int EdgeLightingLocalLightColor = Shader.PropertyToID("_LocalLightColor");

    ////common
    internal static readonly int CommonColor = Shader.PropertyToID("_RimColor");
    internal static readonly int CommonRimPower = Shader.PropertyToID("_RimPower");

    ///PostDeadEffect
    internal static readonly int PostDeadEffectBlurStrength = Shader.PropertyToID("_BlurStrength");
    
    /// RapidBlurEffect
    internal static readonly int RapidBlurEffectDownSampleValue = Shader.PropertyToID("_DownSampleValue");

    ///sky
    internal static readonly int SkyNight = Shader.PropertyToID("_Night");
    internal static readonly int SkySunDirchar1 = Shader.PropertyToID("_SunDirchar1");
    internal static readonly int SkyAddon = Shader.PropertyToID("_Addon");
    internal static readonly int SkyRefint = Shader.PropertyToID("_Refint");
    internal static readonly int SkyReflectionColor = Shader.PropertyToID("_ReflectionColor");
    internal static readonly int SkyWind = Shader.PropertyToID("_Wind");
    internal static readonly int SkyWindnoise = Shader.PropertyToID("_Windnoise");
    internal static readonly int SkyWindfreq = Shader.PropertyToID("_Windfreq");

    internal static readonly int EnvMap1 = Shader.PropertyToID("_EnvMap1");
    internal static readonly int EnvMap2 = Shader.PropertyToID("_EnvMap2");
    internal static readonly int EnvMapMerge = Shader.PropertyToID("_EnvMapMerge");

    ///arc
    internal static readonly int ARCAlphaFadeOut = Shader.PropertyToID("_AlphaFadeOut");
    internal static readonly int ARCCoreCoef = Shader.PropertyToID("_CoreCoef");
    internal static readonly int ARCFadeLevel = Shader.PropertyToID("_FadeLevel");
    internal static readonly int ARCStartColor = Shader.PropertyToID("_StartColor");
    internal static readonly int ARCEndColor = Shader.PropertyToID("_EndColor");
    internal static readonly int ARCCoreColor = Shader.PropertyToID("_CoreColor");
    internal static readonly int Transparent = Shader.PropertyToID("_Transparent");

    ///RFX4
    internal static readonly int RFXTexNextFrame = Shader.PropertyToID("_Tex_NextFrame");
    internal static readonly int RFXInterpolationValue = Shader.PropertyToID("InterpolationValue");
    internal static readonly int RFXTiling = Shader.PropertyToID("_Tiling");
    internal static readonly int RFXCutout = Shader.PropertyToID("_Cutout");
    internal static readonly int RFXInvFade = Shader.PropertyToID("_InvFade");
    internal static readonly int SrcMode = Shader.PropertyToID("SrcMode");
    internal static readonly int DstMode = Shader.PropertyToID("DstMode");

    /// sun_char
    internal static readonly int SunDir = Shader.PropertyToID("_SunDir");
    internal static readonly int SunDirchar = Shader.PropertyToID("_SunDirchar");
    internal static readonly int SunColor = Shader.PropertyToID("_SunColor");
    internal static readonly int SunColorchar = Shader.PropertyToID("_SunColorchar");
    internal static readonly int SunAmbientColor = Shader.PropertyToID("_SunAmbientColor");
    //FxYujing
    internal static readonly int OuterRadius = Shader.PropertyToID("_OuterRadius");
    internal static readonly int InnerRadius = Shader.PropertyToID("_InnerRadius");
    internal static readonly int MainScaler = Shader.PropertyToID("_MainScaler");
    internal static readonly int WihteEdgeLength = Shader.PropertyToID("_WihteEdgeLength");
    
        
    //Dynamic Effect 
    
    internal static readonly int DynEnvLodBias = Shader.PropertyToID("_EnvLodBias");
    internal static readonly int DynNight = Shader.PropertyToID("_Night");
    internal static readonly int DynRefint = Shader.PropertyToID("_Refint");
    internal static readonly int DynReflectionColor = Shader.PropertyToID("_ReflectionColor");
    internal static readonly int DynWind = Shader.PropertyToID("_Wind");
    internal static readonly int DynWindnoise = Shader.PropertyToID("_Windnoise");
    internal static readonly int DynWindfreq = Shader.PropertyToID("_Windfreq");
   
    
        
    //rain && snow 
    internal static readonly int RaindropRipple = Shader.PropertyToID("_RaindropRipple");
    internal static readonly int RainParamters = Shader.PropertyToID("_RainParamters");

    internal static readonly int SnowTexUpperFace = Shader.PropertyToID("_SnowTexUpperFace");
    internal static readonly int SnowTexSideFace = Shader.PropertyToID("_SnowTexSideFace");
    internal static readonly int SnowDensity = Shader.PropertyToID("_SnowDensity");
     
}