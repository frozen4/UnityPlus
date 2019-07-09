using UnityEngine;
using System;
using System.Collections.Generic;
using MapRegion;
using Common;


public class SceneConfig : MonoBehaviour
{
    [System.Serializable]
    public struct CPositionSetting
    {
        public string _BlockName;
        public Vector3 _BlockPosition;
        public int _ShowDistance;
        public int _SceneQuality;
        public int[] _LightIndexArray;
        public int[] _RegionArray;
        public int _EffectiveType;
    }
    [System.Serializable]
    public struct RendererLightmapInfo
    {
        public Renderer _Renderer;
        public string _RendererName;
        public int _LightmapIndex;
        public Vector4 _LightmapScaleOffset;
    }

    [System.Serializable]
    public struct TerrainLightmapInfo
    {
        public Terrain _Terrain;
        public string _TerrainName;
        public int _LightmapIndex;
        public Vector4 _LightmapScaleOffset;
    }

    [System.Serializable]
    public struct LightmapsConfig
    {
        public LightmapsMode _LightmapMode;
        public string _LightmapBundleName;
        public string _LightmapAssetName;
        public RendererLightmapInfo[] _MeshLightmapInfos;
        public TerrainLightmapInfo[] _TerrainLightmapInfos;
    }
    [System.Serializable]
    public enum SceneType
    {
        ENSceneType_Neither = 1,
        ENSceneType_RegionSplit = 2,
        ENSceneType_BlockSplit = 3,
        ENSceneType_Both = 4
    }
    [System.Serializable]
    public struct RegionWeatherConfig
    {
        public int MorningID;
        public int DayID;
        public int DuskID;
        public int NightID;
        public int RainID;
        public int SnowID;
    }

    [System.Serializable]
    public struct ColliderInfo
    {
        //1为BoxCollider 2为SphereCollider
        public int ColiderType;
        public int Layer;
        public Vector3 Pos;
        public Vector3 Rot;
        public Vector3 Scale;
        public Vector3 Center;
        //BoxCollider 
        public Vector3 Size;
        //SphereCollider
        public float Radius;
        public float Height;
    }
    [System.Serializable]
    public class ColliderInfosArray
    {
        public float PosX;
        public float PosZ;
        public ColliderInfo[] ColiderInfos;
    }
    public List<RegionWeatherConfig> _RegionIdGroup = new List<RegionWeatherConfig>();
    public List<CPositionSetting> _BlockPositionList = new List<CPositionSetting>();
    public LightmapsConfig _LightmapConfig;
    public SceneType _SceneType;
    public string _LightRegionName;
    public int _TexCount;
    //[HideInInspector]
    public BoxCollider[] _BoxColliders;
    //[HideInInspector]
    public CapsuleCollider[] _CapsuleColliders;
    //[HideInInspector]
    public ColliderInfo[] _ColliderInfos;

    public string BundleName;
}