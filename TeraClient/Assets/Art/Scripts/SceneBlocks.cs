using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SceneBlocks : MonoBehaviour
{
    public SceneConfig.RendererLightmapInfo[] _MeshLightmapInfos;
    public SceneConfig.TerrainLightmapInfo[] _TerrainLightmapInfos;

    public int _EffectiveType = 0;

    [System.Serializable]
    public struct ObjecttLightSetting
    {
        public MeshRenderer _Renderer;
        public List<SceneConfig.RendererLightmapInfo> _RendererLightInfo;
    }

    public ObjecttLightSetting[] _ObjectLightInfo;
     
    void Start()
    {
        ChangeLightMapInfo();
        DynamicGI.UpdateEnvironment();
    }

    public void ChangeLightMapInfo(int lightAssetIndex = -1 )
    {

        if (-1 == lightAssetIndex)
        {
            for (int index = 0; index < _MeshLightmapInfos.Length; index++)
            {
                if (null != _MeshLightmapInfos[index]._Renderer)
                {
                    _MeshLightmapInfos[index]._Renderer.lightmapIndex = _MeshLightmapInfos[index]._LightmapIndex;
                    _MeshLightmapInfos[index]._Renderer.lightmapScaleOffset = _MeshLightmapInfos[index]._LightmapScaleOffset;
                }
            }

            for (int index = 0; index < _TerrainLightmapInfos.Length; index++)
            {
                if (null != _TerrainLightmapInfos[index]._Terrain)
                {
                    _TerrainLightmapInfos[index]._Terrain.lightmapIndex = _TerrainLightmapInfos[index]._LightmapIndex;
                    _TerrainLightmapInfos[index]._Terrain.lightmapScaleOffset = _TerrainLightmapInfos[index]._LightmapScaleOffset;
                }
            }
        }
        else
        {

            for (int index = 0; index < _ObjectLightInfo.Length; index++)
            {

                if (null != _ObjectLightInfo[index]._Renderer)
                {
                    _ObjectLightInfo[index]._Renderer.lightmapIndex = _ObjectLightInfo[index]._RendererLightInfo[lightAssetIndex]._LightmapIndex;
                    _ObjectLightInfo[index]._Renderer.lightmapScaleOffset = _ObjectLightInfo[index]._RendererLightInfo[lightAssetIndex]._LightmapScaleOffset;
                }
            }
        }

        //DynamicGI.UpdateEnvironment();
    }
}
