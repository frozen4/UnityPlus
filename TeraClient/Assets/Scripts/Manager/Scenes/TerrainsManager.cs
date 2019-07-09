using System.Collections.Generic;
using UnityEngine;
public class TerrainsManager : ISceneManagerLogic
{
    /// <summary>
    /// 功能定位
    /// Terrain动态效果管理，根据与玩家的距离动态调整显隐和参数
    /// </summary>


    /// detailObjectDensity 即默认细节物体密度
    //private const float DEFAULT_DETAIL_OBJECT_DENSITY = 40f;
    ///  地形参数设置距离1 远距距离
    private const int HIGH_TERRAIN_DISTANCE = 200;
    ///  地形参数设置距离2 中距距离
    private const int MIDD_TERRAIN_DISTANCE = 100;

    public class CTerrainEntry
    {
        public Terrain TerrainComp;
        public Vector3 Position;
        public float SizeH;
    }

    private readonly List<CTerrainEntry> _CurrentSceneTerrain = new List<CTerrainEntry>();

    public void Init(SceneConfig.TerrainLightmapInfo[] terrainConfig)
    {
        _CurrentSceneTerrain.Clear();
        int curTerrainCount = terrainConfig.Length;
        for (int i = 0; i < curTerrainCount; i++)
        {
            Terrain terrain = terrainConfig[i]._Terrain;
            if (terrain == null) continue;
            terrain.detailObjectDensity = 40f;
            if (terrain.terrainData != null)
            {
                //设置terrain的参数为效果最低，若有分块则会自动调节
                terrain.drawTreesAndFoliage = false;
                terrain.heightmapPixelError = 200;
                terrain.basemapDistance = 100;
                terrain.castShadows = false;

                CTerrainEntry entry = new CTerrainEntry();
                entry.TerrainComp = terrain;
                entry.Position = terrain.transform.position;
                entry.SizeH = (terrain.terrainData.size.x + terrain.terrainData.size.z) * 0.5f;

                _CurrentSceneTerrain.Add(entry);
            }
        }
    }

    public void Update(Vector3 camPos)
    {
        if (null == _CurrentSceneTerrain || 0 == _CurrentSceneTerrain.Count) return;

        for (int i = 0; i < _CurrentSceneTerrain.Count; i++)
        {
            var curTerrain = _CurrentSceneTerrain[i].TerrainComp;
            var curTerrainPos = _CurrentSceneTerrain[i].Position;
            float curTerrainSizeH = _CurrentSceneTerrain[i].SizeH;

            float fDistanceH = Util.DistanceH(curTerrainPos, camPos) - curTerrainSizeH;
            if (fDistanceH > HIGH_TERRAIN_DISTANCE)
            {
                curTerrain.drawHeightmap = false;
                curTerrain.drawTreesAndFoliage = false;
                curTerrain.heightmapPixelError = 200;
                curTerrain.basemapDistance = 500;
                curTerrain.castShadows = false;
            }
            else if (fDistanceH > MIDD_TERRAIN_DISTANCE)
            {
                curTerrain.drawHeightmap = true;
                curTerrain.drawTreesAndFoliage = false;
                curTerrain.heightmapPixelError = 120;
                curTerrain.basemapDistance = 500;
                curTerrain.castShadows = false;
            }
            else
            {
                curTerrain.drawHeightmap = true;
                curTerrain.drawTreesAndFoliage = false;
                curTerrain.heightmapPixelError = 50;
                curTerrain.basemapDistance = 500;
                curTerrain.castShadows = false;
            }
        }
    }

    public void Clear()
    {
        foreach (var item in _CurrentSceneTerrain)
        {
            if (null != item.TerrainComp)
                GameObject.Destroy(item.TerrainComp);
        }
        _CurrentSceneTerrain.Clear();
    }
}
