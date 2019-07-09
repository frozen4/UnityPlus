using UnityEngine;
using System.Collections.Generic;
using Common;
using System;

public class CFxCacheMan : MonoBehaviourSingleton<CFxCacheMan>, GameLogic.ITickLogic
{
    // TODO: 
    // 如何能将string的查询获取转换为int，减少string在托管非托管之间的转换消耗

    private const float FX_ASSET_CACHE_TIME = 60f;

    // FxOne组件缓存
    private List<CFxOne> _CachedFxOnes = new List<CFxOne>();
    // Fx资源Cache (基础资源 + 实例化对象缓存)
    private readonly Dictionary<string, CFxCache> _FxAssetCaches = new Dictionary<string, CFxCache>();
    // 正在使用的FxOne （包含_ActiveFxsRootTrans下的，以及挂在其他对象挂点的）
    private List<CFxOne> _ActiveFxs = new List<CFxOne>();
    // Fx资源Cache (基础资源 + 实例化对象缓存)
    private readonly Dictionary<string, CFxOne> _UncachedFxs = new Dictionary<string, CFxOne>();

    private Transform _CachedFxOnesRootTrans = null;
    private Transform _ActiveFxsRootTrans = null;
    private Transform _UncachedFxsRootTrans = null;

    private int _FxOneID = 0;
    public bool IsRenderHide { get; set; }

    private float _LastTime = 0;

    public int MaxActiveFxCount
    {
        get
        {
            return _MaxActiveFxCount;
        }
        set
        {
            if (value > 0)
                _MaxActiveFxCount = value;
        }
    }
    private int _MaxActiveFxCount = 100;        // 最多同时播放的fx

    public Transform UncachedFxsRootTrans
    {
        get { return _UncachedFxsRootTrans; }
    }


    public void Init()
    {
        {
            GameObject go = new GameObject("UnusedFxOnes");
            _CachedFxOnesRootTrans = go.transform;
            _CachedFxOnesRootTrans.parent = transform;
            _CachedFxOnesRootTrans.localPosition = new Vector3(10000, 0, 0);
        }

        {
            GameObject go = new GameObject("ActiveFxOnes");
            _ActiveFxsRootTrans = go.transform;
            _ActiveFxsRootTrans.parent = transform;
        }

        {
            GameObject go = new GameObject("FxCaches");
            CFxCache.CachedFxsRoot = go.transform;
            CFxCache.CachedFxsRoot.parent = transform;
            CFxCache.CachedFxsRoot.localPosition = new Vector3(5000, 0, 0);
        }

        {
            GameObject go = new GameObject("UncachedFxs");
            _UncachedFxsRootTrans = go.transform;
            _UncachedFxsRootTrans.parent = transform;
            CFxCache.CachedFxsRoot.localPosition = new Vector3(6000, 0, 0);
        }

        CGManager.Instance.OnPlayCallback = () => { Delay.IsAutoRun = true; };
        CGManager.Instance.OnStopCallback = () => { Delay.IsAutoRun = false; };
    }

    private float CalcFxAssetCacheTime()
    {
        if (_FxAssetCaches.Count > 30)
            return FX_ASSET_CACHE_TIME * 0.3f;
        else if (_FxAssetCaches.Count > 20)
            return FX_ASSET_CACHE_TIME * 0.6f;
        else
            return FX_ASSET_CACHE_TIME;
    }

    public void Tick(float dt)
    {
        for (int i = 0; i < _ActiveFxs.Count; ++i)
        {
            var fx = _ActiveFxs[i];
            fx.Tick(dt);
        }
    }

    public void LateTick(float dt)
    {
        for (int i = 0; i < _ActiveFxs.Count; ++i)
        {
            var fx = _ActiveFxs[i];
            fx.LateTick(dt);
        }

        float now = Time.time;
        if (now - _LastTime > 10)
        {
            _LastTime = now;

            // cache中没有active的特效，? 秒后将其清理
            float fxAssetCacheTime = CalcFxAssetCacheTime();
            var it = _FxAssetCaches.GetEnumerator();
            while (it.MoveNext())
            {
                var cache = it.Current.Value;
                if (cache != null && cache.IsAutoClear && cache.DeactiveTime > 0 && (now - cache.DeactiveTime) >= fxAssetCacheTime)
                {
                    cache.Clear();
                    var key = it.Current.Key;
                    _FxAssetCaches.Remove(key);
                    break;
                }
            }
            it.Dispose();
        }
    }

    private int NewID()
    {
        _FxOneID++;
        if (_FxOneID <= 0) _FxOneID = 1;

        return _FxOneID;
    }

    private CFxOne GetEmptyFxOne()
    {
        CFxOne fxone = null;

        for(var i = _CachedFxOnes.Count - 1; i >= 0 ; i--)
        {
            fxone = _CachedFxOnes[i];
            if(fxone != null && fxone.gameObject != null)
            {
                _CachedFxOnes.RemoveAt(i);
#if UNITY_EDITOR
                fxone.gameObject.name = "gfx";
#endif
                //fxone.gameObject.SetActive(true);

                return fxone;
            }
            else
            {
                _CachedFxOnes.RemoveAt(i);
            }
        }

        GameObject go = new GameObject("gfx");
        fxone = go.AddComponent<CFxOne>();

        return fxone;
    }

    public CFxOne RequestFxOne(string fxName, int priority, out int fxId)
    {
        // 超出上限，先关掉优先级最低的
        #region 存量检查
        if (_ActiveFxs.Count >= _MaxActiveFxCount)
        {
            int maxPriority = -1;
            int realCount = 0;
            for (int i = 0; i < _ActiveFxs.Count; ++i)
            {
                CFxOne fx = _ActiveFxs[i];

                //-1 常驻 不参与计算
                if (fx.Priority != -1)
                {
                    if (fx.Priority > maxPriority)
                        maxPriority = fx.Priority;

                    ++realCount;
                }
            }

            if (realCount > _MaxActiveFxCount)
            {
                if (priority >= maxPriority)
                {
                    fxId = 0;
                    return null;
                }

                // 关闭一个最低优先级的特效
                for (int i = 0; i < _ActiveFxs.Count; ++i)
                {
                    CFxOne fx1 = _ActiveFxs[i];
                    if (fx1.Priority == maxPriority)
                    {
                        fx1.Stop();
                        _ActiveFxs.Remove(fx1);
                        break;
                    }
                }
            }
        }

        #endregion

        // 检查资源缓存池
        CFxCache fxcache = null;
        #region FxAssetCache_Area
        if (!_FxAssetCaches.TryGetValue(fxName, out fxcache))
        {
            fxcache = new CFxCache();
            fxcache.Init(fxName);
            _FxAssetCaches[fxName] = fxcache;
        }
        #endregion

        CFxOne fxone = GetEmptyFxOne();
        #region GetFxOne_Area
        fxone.IsCached = true;
        fxone.ID = NewID();
        //fxone.DontUseL3 = dontUseL3;
        var fxoneTrans = fxone.transform;
        fxoneTrans.parent = _ActiveFxsRootTrans;
        fxoneTrans.localPosition = Vector3.zero;
        fxoneTrans.localScale = Vector3.one;
        fxoneTrans.rotation = Quaternion.identity;

#if UNITY_EDITOR
        fxone.Name = fxName;
#endif

        fxone.Priority = priority;
        _ActiveFxs.Add(fxone);
        #endregion

        fxId = fxone.ID;

        if (!fxcache.Touch(fxone))
        {
            RecycleFx(fxone);
            return null;
        }

        return fxone;
    }

    public void PreloadFxAsset(string fxName)
    {
        if (!_FxAssetCaches.ContainsKey(fxName))
        {
            var fxcache = new CFxCache();
            fxcache.Init(fxName);
            fxcache.DeactiveTime = -1;
            fxcache.IsAutoClear = false;
            _FxAssetCaches.Add(fxName, fxcache);            
        }
    }
    
    public CFxOne RequestUncachedFx(string fxName, bool isSingleton = true)
    {
        CFxOne fxone = null;
        if(!isSingleton || !_UncachedFxs.TryGetValue(fxName, out fxone) || fxone == null || fxone.gameObject == null)           //被清理
        {
            fxone = new GameObject("UncachedFx").AddComponent<CFxOne>();
            fxone.Priority = -1;
            fxone.IsCached = false;
            Action<UnityEngine.Object> callback = (asset) =>
            {
                if (asset != null && fxone != null && fxone.gameObject != null)
                {
                    GameObject fx = GameObject.Instantiate(asset) as GameObject;
                    if (fx != null)
                    {
                        Transform fxTrans = fx.transform;
                        fxTrans.parent = fxone.transform;
                        fxTrans.localPosition = Vector3.zero;
                        fxTrans.localRotation = Quaternion.identity;
                        fxTrans.localScale = Vector3.one;
                        Util.SetLayerRecursively(fx, fxone.gameObject.layer);
                        fxone.SetFxGameObject(fx);
                        fxone.Active(true);
                        fxone.SetScale(fxone.RealScale);
                    }
                    else
                    {
                        HobaDebuger.LogWarningFormat("RequestUncachedFx asset is not GameObject!: {0}", fxName);
                    }
                }
            };

            CAssetBundleManager.AsyncLoadResource(fxName, callback, false, "sfx");
            if (isSingleton)
            {
                fxone.transform.parent = _UncachedFxsRootTrans;

                if (!_UncachedFxs.ContainsKey(fxName))
                    _UncachedFxs.Add(fxName, fxone);
                else
                    _UncachedFxs[fxName] = fxone;
            }
        }
        else
        {
            fxone.Active(true);
            fxone.SetScale(fxone.RealScale);
        }
        
        return fxone;
    }

    public void RecycleFx(CFxOne fxone)
    {
        _ActiveFxs.Remove(fxone);

        var ms = fxone.gameObject.GetComponents<CMotor>();
        if (ms != null && ms.Length > 0)
        {
            for (var i = 0; i < ms.Length; i++)
                UnityEngine.Object.Destroy(ms[i]);
        }
#if UNITY_EDITOR
        fxone.gameObject.name = "gfx_one_unused";
#endif
        fxone.transform.position = Vector3.zero;
        fxone.transform.parent = _CachedFxOnesRootTrans;
        //fxone.DontUseL3 = false;

        //fxone.gameObject.SetActive(false);
        _CachedFxOnes.Add(fxone);
    }


    // 此接口慎用，非正常清理方式
    public void RemoveFxOneUsingTheWrongWay(CFxOne fxone)
    {
        if(_ActiveFxs.Contains(fxone))
            _ActiveFxs.Remove(fxone);
    }

    private readonly List<string> _KeysToRemove = new List<string>();
    public void ClearCaches()
    {
        _KeysToRemove.Clear();
        foreach (var kv in _FxAssetCaches)
        {
            CFxCache fxCache = kv.Value;

            if (!fxCache.IsAutoClear) 
                continue;

            if (kv.Value != null)
                kv.Value.Clear();
            _KeysToRemove.Add(kv.Key);
        }

        foreach (var v in _KeysToRemove)
            _FxAssetCaches.Remove(v);
        _KeysToRemove.Clear();
    }

    public void Cleanup()
    {
        for (var i = 0; i < _ActiveFxs.Count; i++)
        {
            if (_ActiveFxs[i] != null)
                _ActiveFxs[i].Stop();
        }
        _ActiveFxs.Clear();

        for (var i = 0; i < _CachedFxOnes.Count; i++)
        {
            if (_CachedFxOnes[i] != null)
            { 
                if (_CachedFxOnes[i].gameObject != null)
                    UnityEngine.Object.Destroy(_CachedFxOnes[i].gameObject);
            }
        }
        _CachedFxOnes.Clear();

        foreach(var kv in _FxAssetCaches)
        {
            if (kv.Value != null)
                kv.Value.Clear();
        }
        _FxAssetCaches.Clear();

        foreach(var kv in _UncachedFxs)
        {
            if (kv.Value != null)
            {
                if (kv.Value.gameObject != null)
                    UnityEngine.Object.Destroy(kv.Value.gameObject);
            }
        }
        _UncachedFxs.Clear();  
    }

    public void Debug()
    {
        HobaDebuger.LogWarningFormat("VertexPool CreateCount is {0}", Xft.VertexPool.CreateCount);
    }
}
