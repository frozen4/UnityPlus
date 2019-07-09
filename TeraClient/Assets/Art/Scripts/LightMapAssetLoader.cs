using System;
using UnityEngine;

public class LightMapAssetLoader : MonoBehaviour
{
    private bool _SetLightMap = false;
    private LightMapAsset _LightMapAsset = null;
    private SceneConfig _Config = null;
    private LightmapData[] _LightMapDatas = null;
    void OnEnable()
    {
        if (null == _LightMapAsset)
        {
            if (null == _Config)
            {
                _Config = this.GetComponent<SceneConfig>();
            }
            if (null == _Config) return;
            string assetName = _Config._LightmapConfig._LightmapAssetName;
            if (string.IsNullOrEmpty(assetName)) return;
            Action<UnityEngine.Object> callback = (asset) =>
            {
                _LightMapAsset = asset as LightMapAsset;
                if (null == _LightMapAsset) return;
                if (null == _Config) return;

                int Count = _LightMapAsset._LightmapFar.Length;
                _LightMapDatas = new LightmapData[Count];
                for (int i = 0; i < Count; ++i)
                {
                    LightmapData Lightmap = new LightmapData();
                    Lightmap.lightmapColor = _LightMapAsset._LightmapFar[i];
                    Lightmap.lightmapDir = _LightMapAsset._LightmapNear[i];
                    _LightMapDatas[i] = Lightmap;
                }
                LightmapSettings.lightmapsMode = _Config._LightmapConfig._LightmapMode;
                LightmapSettings.lightmaps = _LightMapDatas;

                _SetLightMap = true;
                #region

                for (int i = 0; i < _Config._LightmapConfig._MeshLightmapInfos.Length; i++)
                {
                    if (null != _Config._LightmapConfig._MeshLightmapInfos[i]._Renderer)
                    {
                        _Config._LightmapConfig._MeshLightmapInfos[i]._Renderer.lightmapIndex = _Config._LightmapConfig._MeshLightmapInfos[i]._LightmapIndex;
                        _Config._LightmapConfig._MeshLightmapInfos[i]._Renderer.lightmapScaleOffset = _Config._LightmapConfig._MeshLightmapInfos[i]._LightmapScaleOffset;
                    }
                }
                var terrainInfo = _Config._LightmapConfig._TerrainLightmapInfos;
                for (var i = 0; i < terrainInfo.Length; i++)
                {
                    if (terrainInfo[i]._Terrain != null)
                    {
                        terrainInfo[i]._Terrain.lightmapIndex = terrainInfo[i]._LightmapIndex;
                        terrainInfo[i]._Terrain.lightmapScaleOffset = terrainInfo[i]._LightmapScaleOffset;
                    }

                }
                #endregion
            };
            CAssetBundleManager.AsyncLoadResource(_Config._LightmapConfig._LightmapAssetName, callback, false);
        }

    }

    void OnDestroy()
    {
        if (_SetLightMap)
        {
            LightmapSettings.lightmaps = null;
            _SetLightMap = false;
        }
        _LightMapAsset = null;
        _Config = null;
        _LightMapDatas = null;
    }
}
