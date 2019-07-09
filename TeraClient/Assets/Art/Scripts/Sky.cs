using UnityEngine;
using System.Collections.Generic;

//[ExecuteInEditMode]
public class Sky : MonoBehaviour
{
    static readonly int RENDER_QUEUE = 2300;

    private Cubemap _SkyCubeMap;
    public Cubemap SkyCubeMap
    {
        get
        {
            return _SkyCubeMap;
        }
        set
        {
            _SkyCubeMap = value;
            if (SkyCubeMap != null)
            {
                Shader.SetGlobalTexture(ShaderIDs.GlobalTexture, SkyCubeMap);
            }
        }
    }

    public MeshRenderer SkySphere;

    private Material _SrcMaterial = null;
    private Material _DestMaterial = null;

    private bool _NeedLerping = false;
    private float _CurLerpValue = 0;

    public void SetGolbalPara(float lightmapFeedback, Vector4 addonDirection, float headlightIntesity, float reflectionIntensity
                           , Color refColor, Vector4 windDirection, float windNoise, float windFrequency)
    {
        Shader.SetGlobalFloat(ShaderIDs.SkyNight, lightmapFeedback);
        Shader.SetGlobalVector(ShaderIDs.SkySunDirchar1, addonDirection);
        Shader.SetGlobalFloat(ShaderIDs.SkyAddon, headlightIntesity);
        Shader.SetGlobalFloat(ShaderIDs.SkyRefint, reflectionIntensity);
        Shader.SetGlobalColor(ShaderIDs.SkyReflectionColor, refColor);
        Shader.SetGlobalVector(ShaderIDs.SkyWind, windDirection);
        Shader.SetGlobalFloat(ShaderIDs.SkyWindnoise, windNoise);
        Shader.SetGlobalFloat(ShaderIDs.SkyWindfreq, windFrequency);
    }


    struct ShaderPropertyIds
    {
        public int BlendFactorSrc;
        public int BlendFactorDest;
        public int Alpha;

        public ShaderPropertyIds(int srcFactor, int destFactor, int alpha)
        {
            BlendFactorSrc = srcFactor;
            BlendFactorDest = destFactor;
            Alpha = alpha;
        }
    }
    private ShaderPropertyIds _ShaderPropertyIds = new ShaderPropertyIds(0, 0, 0);

    void Awake()
    {
        _ShaderPropertyIds.BlendFactorSrc = Shader.PropertyToID("_BlendFactorSrc");
        _ShaderPropertyIds.BlendFactorDest = Shader.PropertyToID("_BlendFactorDest");
        _ShaderPropertyIds.Alpha = Shader.PropertyToID("_Alpha");
    }

    public void PushSkyMaterial(Material src, Material dest)
    {
        if (SkySphere == null)
        {
            Common.HobaDebuger.LogWarningFormat("Can not PushSkyMaterial when SkySphere is not ready.");
            return;
        }

        if (_SrcMaterial == null || _DestMaterial == null)
        {
            _SrcMaterial = new Material(src);
            _DestMaterial = new Material(dest);

            Material[] materials = new Material[2];

            materials[0] = _SrcMaterial;
            materials[0].renderQueue = RENDER_QUEUE;
            materials[1] = _DestMaterial;
            materials[1].renderQueue = RENDER_QUEUE + 1;

            SkySphere.sharedMaterials = materials;
        }

        if (src != null)
        {
            _SrcMaterial.CopyPropertiesFromMaterial(src);
            _SrcMaterial.SetInt(_ShaderPropertyIds.BlendFactorSrc, 5);
            _SrcMaterial.SetInt(_ShaderPropertyIds.BlendFactorDest, 10);
            _SrcMaterial.SetFloat(_ShaderPropertyIds.Alpha, 1);

            _NeedLerping = true;
        }

        if (dest != null)
        {
            _DestMaterial.CopyPropertiesFromMaterial(dest);
            _DestMaterial.SetInt(_ShaderPropertyIds.BlendFactorSrc, 5);
            _DestMaterial.SetInt(_ShaderPropertyIds.BlendFactorDest, 10);
            _DestMaterial.SetFloat(_ShaderPropertyIds.Alpha, 0);

            _NeedLerping = true;
        }
    }

    public void Blend(float lerpValue)
    {
        Shader.SetGlobalFloat(ShaderIDs.EnvMapMerge, lerpValue);
        if (lerpValue > 0.9999f)
        {
            Shader.SetGlobalTexture(ShaderIDs.EnvMap1, _DestCubeMap);
            Shader.SetGlobalTexture(ShaderIDs.EnvMap2, null);
            Shader.SetGlobalFloat(ShaderIDs.EnvMapMerge, 0);
        }

        if (!_NeedLerping) return;

        _CurLerpValue = Mathf.Clamp01(lerpValue);

        if (Mathf.Abs(lerpValue - 1) < 1e-3)
        {
            _NeedLerping = false;
        }
        else
        {
            _SrcMaterial.SetFloat(_ShaderPropertyIds.Alpha, 1 - _CurLerpValue);
            _DestMaterial.SetFloat(_ShaderPropertyIds.Alpha, _CurLerpValue);
        }

    }

    private Cubemap _StartCubeMap = null;
    private Cubemap _DestCubeMap = null;

 
    public void SetEnvCubeMap(Cubemap startCubeMap, Cubemap destCubeMap, float lerpValue)
    {
        _StartCubeMap = startCubeMap;
        _DestCubeMap = destCubeMap;
        if (null == destCubeMap)
        {
            Shader.SetGlobalTexture(ShaderIDs.EnvMap1, null);
            Shader.SetGlobalTexture(ShaderIDs.EnvMap2, null);
            Shader.SetGlobalFloat(ShaderIDs.EnvMapMerge, 0);
            return;
        }
        else
        {
            Shader.SetGlobalTexture(ShaderIDs.EnvMap1, startCubeMap);
            Shader.SetGlobalTexture(ShaderIDs.EnvMap2, destCubeMap);
            Shader.SetGlobalFloat(ShaderIDs.EnvMapMerge, lerpValue);
        }

        if (lerpValue > 0.9999f)
        {
            Shader.SetGlobalTexture(ShaderIDs.EnvMap1, destCubeMap);
            Shader.SetGlobalTexture(ShaderIDs.EnvMap2, null);
            Shader.SetGlobalFloat(ShaderIDs.EnvMapMerge, 0);
        }
    }
    public void Reset()
    {
        _CurLerpValue = 0;
        _NeedLerping = false;
    }

    public void Release()
    {
        Reset();
        _SrcMaterial = null;
        _DestMaterial = null;
        _SkyCubeMap = null;
        SkySphere = null;
        _StartCubeMap = null;
        _DestCubeMap = null;
    }
}
