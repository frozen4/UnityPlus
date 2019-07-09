using Common;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class ScenesBlocksManager : ISceneManagerLogic
{
    private SceneConfig _Config = null;
    private Transform _BlocksRootTran;
    private LightMapAsset _PlayerLightmapBundleAsset;
    private LightmapData[] _SceneLightmapDatas;

    #region 分块加载参数设置
    private const int MAX_CACHE_OBJ_COUNT = 3;
    private const int DESTROY_DISTANCE_SPAN = 30;
    private const int CACHE_DISTANCE_SPAN = 5;
    private static readonly Vector3 FAR_POS = new Vector3(10000, 10000, 10000);
    #endregion

    // 场景中的特殊物件
    private readonly Dictionary<GameObject, AQUAS_Reflection[]> _LoadedAQUASReflectionDic = new Dictionary<GameObject, AQUAS_Reflection[]>();
    private readonly Dictionary<GameObject, StaticObjectAudio> _LoadedStaticObjectAudioDic = new Dictionary<GameObject, StaticObjectAudio>();

    // 异步加载标志
    private int _CurSceneGuid = 0;

    private readonly HashSet<string> _LoadedPoints = new HashSet<string>();
    private readonly Dictionary<string, GameObject> _LoadedObjectDic = new Dictionary<string, GameObject>();
    private readonly Dictionary<string, GameObject> _ObjectCacheDic = new Dictionary<string, GameObject>();

    // 预加载状态相关
    private bool _BeginFrameUpdate = false;
    private Action _CallBack = null;
    private int _PreLoadBlockCounter = 0;

    // 光照图相关
    private readonly Dictionary<int, int> _LightmapIdx2RefCountDic = new Dictionary<int, int>();
    private bool _IsLightmapsUpdated = false;

    private int _SceneQuality = 3;
    private bool _isDebug = false;
    public int SceneQuality
    {
        get { return _SceneQuality; }
        set
        {
            _SceneQuality = value;
            UpdateWaterReflection();
        }
    }

    public void Init(SceneConfig cfg)
    {
        Clear();

        if (cfg == null) return;

        _Config = cfg;
        _CurSceneGuid = _CurSceneGuid + 1;

        _BlocksRootTran = _Config.transform.Find("BlockObjects");
        if (null == _BlocksRootTran)
        {
            _BlocksRootTran = _Config.transform;
        }

        LightmapSettings.lightmapsMode = cfg._LightmapConfig._LightmapMode;

        var lightmapCount = cfg._TexCount;


        _SceneLightmapDatas = new LightmapData[lightmapCount];
        for (int i = 0; i < lightmapCount; i++)
        {
            _LightmapIdx2RefCountDic.Add(i, 0);
            _SceneLightmapDatas[i] = new LightmapData();
        }

        LoadAssetWithPlayerLight(cfg._LightmapConfig);
    }

    /// 根据角色位置，预加载周围的分块后回调
    public void Preload(float posx, float posz, Action callback)
    {
        if (null == _Config)
        {
            HobaDebuger.Log("Scene Config Init Failed,Please Make Sure Scene Prefab Loaded Success ！ ");
            return;
        }

        _CallBack = callback;
        var preLoadBlockList = new List<SceneConfig.CPositionSetting>();
        _PreLoadBlockCounter = 0;

        int blockCount = _Config._BlockPositionList.Count;
        for (int i = 0; i < blockCount; ++i)
        {
            var currentBlock = _Config._BlockPositionList[i];

            if (_SceneQuality < currentBlock._SceneQuality)
                continue;

            float fDistance = Util.DistanceH(currentBlock._BlockPosition, new Vector3(posx, 0, posz));
            if (fDistance <= currentBlock._ShowDistance)
            {
                preLoadBlockList.Add(currentBlock);
            }
        }

        int nBlockCount = preLoadBlockList.Count;
        _PreLoadBlockCounter = nBlockCount;

        if (nBlockCount == 0)
        {
            AddToSpecialObjectList(_Config.gameObject);
        }
        else
        {
            for (var index = 0; index < nBlockCount; index++)
            {
                var currentPos = preLoadBlockList[index];
                if (!_LoadedPoints.Contains(currentPos._BlockName))
                {
                    for (int i = 0; i < currentPos._LightIndexArray.Length; i++)
                    {
                        int val;
                        if (_LightmapIdx2RefCountDic.TryGetValue(currentPos._LightIndexArray[i], out val))
                        {
                            _LightmapIdx2RefCountDic[currentPos._LightIndexArray[i]] = val + 1;
                            _IsLightmapsUpdated = true;
                        }
                        else
                        {
                            HobaDebuger.LogFormat("光照贴图索引不正确，最大索引{0}，当前索引为{1}，资源{2}", _LightmapIdx2RefCountDic.Count - 1, currentPos._LightIndexArray[i], currentPos._BlockName);
                            continue;
                        }
                    }
                    _LoadedPoints.Add(currentPos._BlockName);
                    LoadBlockWithCounter(currentPos._BlockName, currentPos._EffectiveType);
                }
                else
                {
                    --_PreLoadBlockCounter;
                    HobaDebuger.LogErrorFormat("{0} 重复名字的Block! {1}", index, currentPos._BlockName);
                }
            }
        }

        ///计数加载，加载完成之后进行回调
        if (0 == _PreLoadBlockCounter && _CallBack != null)
        {
            _CallBack();
            _CallBack = null;
        }
    }

    
    public void OnDebugCmd(bool value)
    {
        _isDebug = value;
    }
    public void UpdateBlocks(Vector3 position)
    {
        if (_Config == null)
            return;

        int blockCount = _Config._BlockPositionList.Count;
        bool bUpdateLightMap = false;
        for (int i = 0; i < blockCount; ++i)
        {
            var currentBlock = _Config._BlockPositionList[i];
            int quality = currentBlock._SceneQuality;
            if (_SceneQuality < quality)
                continue;

            bUpdateLightMap = true;
            float fDistance = Util.DistanceH(currentBlock._BlockPosition, position);
            float fShowDistance = currentBlock._ShowDistance;

            if (_isDebug)
            {
                LoadInstGo(ref currentBlock, position);
            }
            else
            {
                if (fDistance <= fShowDistance)
                    LoadInstGo(ref currentBlock, position);
                else if (fDistance > (fShowDistance + DESTROY_DISTANCE_SPAN))
                    DestroyInstGo(ref currentBlock);
                else if (fDistance > (fShowDistance + CACHE_DISTANCE_SPAN))
                    CacheInstGo(ref currentBlock);
            }

        }

        if (bUpdateLightMap)
            UpdateLightmaps();
    }

    private void LoadAssetWithPlayerLight(SceneConfig.LightmapsConfig lightmapConf)
    {
        if (string.IsNullOrEmpty(lightmapConf._LightmapAssetName))
        {
            HobaDebuger.Log("this scenes 's lightmap asset name is null ,please check resources");
            return;
        }

        var loadedSceneId = _CurSceneGuid;
        Action<UnityEngine.Object> callback = (asset) =>
        {
            if (loadedSceneId != _CurSceneGuid)
            {
                HobaDebuger.LogWarning("the asset being loaded is not belong to this scene ");
                return;
            }
            LightMapAsset lightmapAsset = asset as LightMapAsset;

            if (null == lightmapAsset)
            {
                HobaDebuger.LogWarning("Lightmap asset loaded failed  ! ");
                return;
            }
            _PlayerLightmapBundleAsset = lightmapAsset;

            #region 预加载的物件光照信息补全

            List<int> tempList = new List<int>();
            for (int i = 0; i < lightmapConf._MeshLightmapInfos.Length; i++)
            {
                if (null != lightmapConf._MeshLightmapInfos[i]._Renderer)
                {
                    if (!tempList.Contains(lightmapConf._MeshLightmapInfos[i]._LightmapIndex))
                    {
                        tempList.Add(lightmapConf._MeshLightmapInfos[i]._LightmapIndex);
                    }
                    lightmapConf._MeshLightmapInfos[i]._Renderer.lightmapIndex = lightmapConf._MeshLightmapInfos[i]._LightmapIndex;
                    lightmapConf._MeshLightmapInfos[i]._Renderer.lightmapScaleOffset = lightmapConf._MeshLightmapInfos[i]._LightmapScaleOffset;
                }
            }
            for (int i = 0; i < lightmapConf._TerrainLightmapInfos.Length; i++)
            {
                if (null != lightmapConf._TerrainLightmapInfos[i]._Terrain)
                {
                    if (!tempList.Contains(lightmapConf._TerrainLightmapInfos[i]._LightmapIndex))
                    {
                        tempList.Add(lightmapConf._TerrainLightmapInfos[i]._LightmapIndex);
                    }

                    lightmapConf._TerrainLightmapInfos[i]._Terrain.lightmapIndex = lightmapConf._TerrainLightmapInfos[i]._LightmapIndex;
                    lightmapConf._TerrainLightmapInfos[i]._Terrain.lightmapScaleOffset = lightmapConf._TerrainLightmapInfos[i]._LightmapScaleOffset;
                }
            }

            for (int i = 0; i < tempList.Count; i++)
            {
                int v = _LightmapIdx2RefCountDic[tempList[i]];
                if (0 == v)
                    _IsLightmapsUpdated = true;
                _LightmapIdx2RefCountDic[tempList[i]] = v + 1;
            }
            UpdateLightmaps();
            #endregion 预加载的物件光照信息补全
            // if (EntryPoint.Instance._UsingStaticBatching)
            //   CUnityHelper.StaticBatching(gameObject);
        };
        CAssetBundleManager.AsyncLoadResource(lightmapConf._LightmapAssetName, callback, false, "scenes");
    }

    private void LoadBlockWithCounter(string blockName, int effectiveType)
    {
        if (string.IsNullOrEmpty(_Config.BundleName))
        {
            var subStrings = blockName.Split('/');
            if (subStrings.Length > 2)
                _Config.BundleName = subStrings[2].ToLower(); ;
        }

        if (string.IsNullOrEmpty(_Config.BundleName))
            return;

        int currentSceneId = _CurSceneGuid;
        Action<UnityEngine.Object> callback = (asset) =>
        {
            if (currentSceneId != _CurSceneGuid) return;

            if (null != asset)
            {
                GameObject sceneBlock = GameObject.Instantiate(asset) as GameObject;
                if (sceneBlock != null)
                {
                    sceneBlock.transform.SetParent(_BlocksRootTran);
                    if (!_LoadedObjectDic.ContainsKey(blockName))
                    {
                        if (!string.IsNullOrEmpty(blockName))
                            _LoadedObjectDic.Add(blockName, sceneBlock);

                        AddToSpecialObjectList(sceneBlock);
                    }
                    if (effectiveType != 0)
                    {
                        ShowSpecialBlock(sceneBlock, effectiveType, blockName);
                    }
                }
            }
            --_PreLoadBlockCounter;
            if (0 == _PreLoadBlockCounter && null != _CallBack)
            {
                _CallBack();
                _CallBack = null;
                _BeginFrameUpdate = true;
            }
        };
        CAssetBundleManager.AsyncLoadResource(blockName, callback, false, _Config.BundleName);
    }

    private void LoadBlockFromBundle(string blockName, int effectiveType)
    {
        if (_Config == null) return;

        if (string.IsNullOrEmpty(_Config.BundleName))
        {
            var subStrings = blockName.Split('/');
            if (subStrings.Length > 2)
                _Config.BundleName = subStrings[2].ToLower(); ;
        }

        if (string.IsNullOrEmpty(_Config.BundleName))
            return;

        int currentSceneId = _CurSceneGuid;
        Action<UnityEngine.Object> callback = (asset) =>
        {
            if (null != asset && currentSceneId == _CurSceneGuid)
            {
                GameObject sceneBlock = GameObject.Instantiate(asset) as GameObject;
                if (sceneBlock != null)
                {
                    sceneBlock.transform.SetParent(_BlocksRootTran);

                    if (!_LoadedObjectDic.ContainsKey(blockName))
                    {
                        if (!string.IsNullOrEmpty(blockName))
                            _LoadedObjectDic.Add(blockName, sceneBlock);
                        AddToSpecialObjectList(sceneBlock);
                    }
                    if (effectiveType != 0)
                    {
                        ShowSpecialBlock(sceneBlock, effectiveType, blockName);
                    }

                }
            }
        };

        CAssetBundleManager.AsyncLoadResource(blockName, callback, false, _Config.BundleName);
    }

    private bool IsContainsKey(int[] array, int key)
    {
        if (null == array || 0 == array.Length) return true;
        for (int i = 0; i < array.Length; i++)
        {
            if (array[i] == key) return true;
        }
        return false;
    }

    public void Update(Vector3 position){}

    public IEnumerable TickCoroutine(Vector3 position)
    {
        bool bUpdateLightMap = false;

        if (null != _Config && _BeginFrameUpdate)
        {
            foreach (var v in _Config._BlockPositionList)
            {
                if (_SceneQuality < v._SceneQuality)
                    continue;

                float fDistance = Util.DistanceH(v._BlockPosition, position);
                float fShowDistance = v._ShowDistance;
                var currentBlock = v;

                if (fDistance <= fShowDistance)
                    LoadInstGo(ref currentBlock, position);
                else if (fDistance > (fShowDistance + DESTROY_DISTANCE_SPAN))
                    DestroyInstGo(ref currentBlock);
                else if (fDistance > (fShowDistance + CACHE_DISTANCE_SPAN))
                    CacheInstGo(ref currentBlock);

                bUpdateLightMap = true;
            }
        }

        if (bUpdateLightMap)
        {
            UpdateLightmaps();
            yield return null;
        }
    }

    /// 加载分块
    private void LoadInstGo(ref SceneConfig.CPositionSetting currentPos, Vector3 position)
    {
        int filterRegionIndex = ScenesRegionManager.Instance.CurBlockReigionID;
        var isContains = IsContainsKey(currentPos._RegionArray, filterRegionIndex);
        if (!isContains) return;

        /// 同名文件不重复加载
        if (_LoadedPoints.Contains(currentPos._BlockName))
        {
            GameObject cacheGo;
            if (_ObjectCacheDic.TryGetValue(currentPos._BlockName, out cacheGo))
            {
                cacheGo.transform.transform.localPosition = currentPos._BlockPosition;
                AddToSpecialObjectList(cacheGo);
            }
        }
        else
        {
            for (int i = 0; i < currentPos._LightIndexArray.Length; i++)
            {
                int val = 0;
                if (_LightmapIdx2RefCountDic.TryGetValue(currentPos._LightIndexArray[i], out val))
                {
                    _LightmapIdx2RefCountDic[currentPos._LightIndexArray[i]] = val + 1;
                    _IsLightmapsUpdated = true;
                }
                else
                {
                    HobaDebuger.LogFormat("光照贴图索引不正确，最大索引{0}，当前索引为{1}，资源{2}", _LightmapIdx2RefCountDic.Count - 1, currentPos._LightIndexArray[i], currentPos._BlockName);
                    continue;
                }
            }
            _LoadedPoints.Add(currentPos._BlockName);
            LoadBlockFromBundle(currentPos._BlockName, currentPos._EffectiveType);
        }

    }

    /// 缓存分块
    private void CacheInstGo(ref SceneConfig.CPositionSetting currentPos)
    {
        if (!_LoadedPoints.Contains(currentPos._BlockName)) return;
        GameObject _cacheGo;
        if (_ObjectCacheDic.TryGetValue(currentPos._BlockName, out _cacheGo))
        {
            if (null != _cacheGo && _cacheGo.activeSelf)
                //_cacheGo.SetActive(false);
                _cacheGo.transform.transform.localPosition = FAR_POS;
        }
        else if (_ObjectCacheDic.Count < MAX_CACHE_OBJ_COUNT)
        {
            _cacheGo = FindRegObj(currentPos._BlockName);
            if (null != _cacheGo)
            {
                //_cacheGo.SetActive(false);
                _cacheGo.transform.transform.localPosition = FAR_POS;
                _ObjectCacheDic.Add(currentPos._BlockName, _cacheGo);
            }
        }
    }

    /// 销毁分块
    private void DestroyInstGo(ref SceneConfig.CPositionSetting currentPos)
    {
        if (!_LoadedPoints.Contains(currentPos._BlockName)) return;
        var _cacheGo = FindRegObj(currentPos._BlockName);
        if (null == _cacheGo) return;

        _LoadedObjectDic.Remove(currentPos._BlockName);
        RemoveFromSpecialObjectList(_cacheGo);
        ScenesAnimationManager.Instance.RemoveAnimationObj(currentPos._BlockName);
        _ObjectCacheDic.Remove(currentPos._BlockName);
        GameObject.Destroy(_cacheGo);
        _LoadedPoints.Remove(currentPos._BlockName);

        for (int i = 0; i < currentPos._LightIndexArray.Length; i++)
        {
            int v;
            if (_LightmapIdx2RefCountDic.TryGetValue(currentPos._LightIndexArray[i], out v))
            {
                if (v > 0)
                {
                    _LightmapIdx2RefCountDic[currentPos._LightIndexArray[i]] = v - 1;
                    _IsLightmapsUpdated = true;
                }
                else
                {
                    HobaDebuger.LogFormat("尝试销毁一张不存在的贴图，分块名称为 {0}", currentPos._BlockName);
                }
            }
            else
            {
                HobaDebuger.LogFormat("索引不正确，名称为 {0}", currentPos._BlockName);
                continue;
            }

        }
    }

    /// 特殊分块，如只有在晚上亮起的灯， 
    private void ShowSpecialBlock(GameObject go, int effectiveType, string blockName)
    {
        //开关灯动画
        if (effectiveType == 1)
        {
            ScenesAnimationManager.Instance.AddAnimaitonObj(blockName, go);
        }
        //场景动态效果，如，烟囱冒烟
        else if (effectiveType == 2)
        {
            //ScenesAnimationManager.Instance.AddLightAnimation(go);
            //var effectType = DynamicEffectManager.Instance.GetDynamicEffectType();
            //if (effectType != DynamicEffectType.Night && null != go)
            //{
            //    go.SetActive(false);
            //}
        }
    }

    private GameObject FindRegObj(string name)
    {
        GameObject go;
        if (_LoadedObjectDic.TryGetValue(name, out go))
            return go;

        return null;
    }

    private void RemoveFromSpecialObjectList(GameObject go)
    {
        if (go == null) return;

        //reflection
        if (_LoadedAQUASReflectionDic.ContainsKey(go))
            _LoadedAQUASReflectionDic.Remove(go);

        //static audio
        if (_LoadedStaticObjectAudioDic.ContainsKey(go))
            _LoadedStaticObjectAudioDic.Remove(go);
    }

    private void AddToSpecialObjectList(GameObject go)
    {
        if (go == null) return;

        //reflection
        if (!_LoadedAQUASReflectionDic.ContainsKey(go))
        {
            var reflections = go.GetComponentsInChildren<AQUAS_Reflection>();
            if (reflections != null && reflections.Length > 0)
            {
                _LoadedAQUASReflectionDic.Add(go, reflections);

                for (int i = 0; i < reflections.Length; ++i)
                {
                    if (reflections[i] != null)
                        reflections[i].SetFullReflection(GFXConfig.Instance.IsEnableWaterReflection);
                }
            }
        }

        //static audio
        if (!_LoadedStaticObjectAudioDic.ContainsKey(go))
        {
            var staticAudio = go.GetComponentInChildren<StaticObjectAudio>();
            if (staticAudio != null)
                _LoadedStaticObjectAudioDic.Add(go, staticAudio);
        }
    }

    private void UpdateLightmaps()
    {
        if (null == _PlayerLightmapBundleAsset) return;

        var changed = _IsLightmapsUpdated;
        //判断是否发生改变  //没有变化, 无需设置
        if (!changed) return;

        var it = _LightmapIdx2RefCountDic.GetEnumerator();
        while (it.MoveNext())
        {
            var kv = it.Current;
            if (0 == kv.Value)
            {
                _SceneLightmapDatas[kv.Key].lightmapColor = Texture2D.whiteTexture;
                _SceneLightmapDatas[kv.Key].lightmapDir = Texture2D.whiteTexture;
            }
            else if (kv.Value > 0)
            {
                _SceneLightmapDatas[kv.Key].lightmapColor = _PlayerLightmapBundleAsset._LightmapFar[kv.Key];
                _SceneLightmapDatas[kv.Key].lightmapDir = _PlayerLightmapBundleAsset._LightmapNear[kv.Key];
            }
            else
            {
                HobaDebuger.Log("引用计数错误，此值不应该小于0");
            }
        }
        it.Dispose();
        LightmapSettings.lightmaps = _SceneLightmapDatas;
    }

    public Dictionary<GameObject, StaticObjectAudio> GetAudioObjectDic()
    {
        return _LoadedStaticObjectAudioDic;
    }

    public void UpdateWaterReflection()
    {
        var itor = _LoadedAQUASReflectionDic.GetEnumerator();
        while (itor.MoveNext())
        {
            var list = itor.Current.Value;
            for (int i = 0; i < list.Length; ++i)
            {
                if (list[i] != null)
                    list[i].SetFullReflection(GFXConfig.Instance.IsEnableWaterReflection);
            }
        }
        itor.Dispose();
    }

    public void Clear()
    {
        _BeginFrameUpdate = false;
        _BlocksRootTran = null;
        _PlayerLightmapBundleAsset = null;

        _SceneLightmapDatas = null;
        LightmapSettings.lightmaps = null;

        _LoadedAQUASReflectionDic.Clear();
        _LoadedStaticObjectAudioDic.Clear();
        _LoadedPoints.Clear();

        _CurSceneGuid = 0;

        foreach (var kv in _LoadedObjectDic)
        {
            if (null != kv.Value)
                GameObject.Destroy(kv.Value);
        }
        _LoadedObjectDic.Clear();

        foreach (var kv in _ObjectCacheDic)
        {
            if (null != kv.Value)
                GameObject.Destroy(kv.Value);
        }
        _ObjectCacheDic.Clear();

        _LightmapIdx2RefCountDic.Clear();

        _BeginFrameUpdate = false;
        _CallBack = null;
        _PreLoadBlockCounter = 0;

        _IsLightmapsUpdated = false;

        if (_Config != null && !string.IsNullOrEmpty(_Config.BundleName))
            CAssetBundleManager.UnloadBundle(_Config.BundleName);

        _Config = null;
    }
}