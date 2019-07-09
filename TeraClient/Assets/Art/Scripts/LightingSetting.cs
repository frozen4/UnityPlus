using System;
using UnityEngine;

public class LightingSetting : MonoBehaviour
{
    // 环境光
    public Color _SkyColor;
    public Color _EquatorColor;
    public Color _GroundColor;
    public float _AmbientIntensity;

    //old
    private Color _OldSkyColor;
    private Color _OldEquatorColor;
    private Color _OldGroundColor;
    private float _OldAmbientIntensity;

    // fog
    public bool _IsFogOn;
    public Color _FogColor;
    public FogMode _FogMode;
    public float _FogBeginDis;
    public float _FogEndDis;

    // old
    private bool _OldIsFogOn;
    private Color _OldFogColor;
    private FogMode _OldFogMode;
    private float _OldFogBeginDis;
    private float _OldFogEndDis;

    private bool _IsSaved = false;
    //public string cubeMapName;
    // TODO: 整合进动态效果管理器  added by lijian

    public Sky _Sky;
    public string _CubeMapName;
    private Cubemap _CubeMap = null;



    public string _EnvCubeMapNameStart;
    public string _EnvCubeMapNameEnd;


    private Cubemap _EnvCubeMapStart = null;
    private Cubemap _EnvCubeMapEnd = null;

    public Vector4 AddonDirection;
    public Color RefColor;
    public float HeadlightIntesity;
    public void Enable(bool enable)
    {


        if (enable)
        {
            if (!_IsSaved)
            {
                _OldSkyColor = RenderSettings.ambientSkyColor;
                _OldEquatorColor = RenderSettings.ambientEquatorColor;
                _OldGroundColor = RenderSettings.ambientGroundColor;
                _OldAmbientIntensity = RenderSettings.ambientIntensity;

                _OldIsFogOn = RenderSettings.fog;
                _OldFogColor = RenderSettings.fogColor;
                _OldFogMode = RenderSettings.fogMode;
                _OldFogBeginDis = RenderSettings.fogStartDistance;
                _OldFogEndDis = RenderSettings.fogEndDistance;

                _IsSaved = true;
            }

            RenderSettings.ambientSkyColor = _SkyColor;
            RenderSettings.ambientEquatorColor = _EquatorColor;
            RenderSettings.ambientGroundColor = _GroundColor;
            RenderSettings.ambientIntensity = _AmbientIntensity;
            RenderSettings.fog = _IsFogOn;
            RenderSettings.fogColor = _FogColor;
            RenderSettings.fogMode = _FogMode;
            RenderSettings.fogStartDistance = _FogBeginDis;
            RenderSettings.fogEndDistance = _FogEndDis;
        }
        else
        {
            if (_IsSaved)
            {
                RenderSettings.ambientSkyColor = _OldSkyColor;
                RenderSettings.ambientEquatorColor = _OldEquatorColor;
                RenderSettings.ambientGroundColor = _OldGroundColor;
                RenderSettings.ambientIntensity = _OldAmbientIntensity;
                RenderSettings.fog = _OldIsFogOn;
                RenderSettings.fogColor = _OldFogColor;
                RenderSettings.fogMode = _OldFogMode;
                RenderSettings.fogStartDistance = _OldFogBeginDis;
                RenderSettings.fogEndDistance = _OldFogEndDis;
                _IsSaved = false;
            }
        }

        Shader.SetGlobalVector(ShaderIDs.SkySunDirchar1, AddonDirection);
        Shader.SetGlobalColor(ShaderIDs.SkyReflectionColor, RefColor);
        Shader.SetGlobalFloat(ShaderIDs.SkyAddon, HeadlightIntesity);
        if (null != _Sky)
        {
            if (null != _CubeMap)
            {
                _Sky.SkyCubeMap = _CubeMap;
            }
            else
            {
                if (string.IsNullOrEmpty(_CubeMapName)) return;
                var cubeMapName = HobaText.Format("Assets/Outputs/Scenes/SkySphere/Cubemaps/{0}.cubemap", _CubeMapName);
                Action<UnityEngine.Object> callback = (asset) =>
                {
                    if (null != asset)
                    {
                        _CubeMap = asset as Cubemap;

                        if (_Sky != null)
                            _Sky.SkyCubeMap = _CubeMap;
                    }
                };
                CAssetBundleManager.AsyncLoadResource(cubeMapName, callback, false, "scenes");
            }
            if (null != _EnvCubeMapStart)
            {
                Shader.SetGlobalTexture(ShaderIDs.EnvMap1, _EnvCubeMapStart);
            }
            else
            {
                if (string.IsNullOrEmpty(_EnvCubeMapNameStart)) return;
                var cubeMapName = HobaText.Format("Assets/Outputs/Scenes/SkySphere/Cubemaps/{0}.cubemap", _EnvCubeMapStart);
                Action<UnityEngine.Object> callback = (asset) =>
                {
                    Shader.SetGlobalTexture(ShaderIDs.EnvMap1, _EnvCubeMapStart);
                };
                CAssetBundleManager.AsyncLoadResource(cubeMapName, callback, false, "scenes");
            }
            if (null != _EnvCubeMapEnd)
            {
                Shader.SetGlobalTexture(ShaderIDs.EnvMap2, _EnvCubeMapEnd);

            }
            else
            {
                if (string.IsNullOrEmpty(_EnvCubeMapNameEnd)) return;
                var cubeMapName = HobaText.Format("Assets/Outputs/Scenes/SkySphere/Cubemaps/{0}.cubemap", _EnvCubeMapEnd);
                Action<UnityEngine.Object> callback = (asset) =>
                {
                    Shader.SetGlobalTexture(ShaderIDs.EnvMap2, _EnvCubeMapEnd);
                };
                CAssetBundleManager.AsyncLoadResource(cubeMapName, callback, false, "scenes");
            }
        }

      

    }
}
