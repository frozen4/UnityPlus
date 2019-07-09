using UnityEngine;
using System.Collections.Generic;
using System;

//
//Need the scaling mode of particle be set as Local(5.5 must be this) or Shape
//

public class UISfxBehaviour : MonoBehaviour
{
    //private static List<UISfxBehaviour> _Cache = new List<UISfxBehaviour>();
    //private static Dictionary<string, GameObject> _PrefabCache = new Dictionary<string, GameObject>();
    public class PrefabCacheData
    {
        public const int CACHE_LIFE_TIME = 20;

        public UnityEngine.Object Obj;
        public float LifeTime = 0f;

        public PrefabCacheData(UnityEngine.Object obj, float life_time)
        {
            Obj = obj;
            LifeTime = life_time;
        }
    }

    private static List<UISfxBehaviour> _AllCreatedList = new List<UISfxBehaviour>();
    private static Dictionary<string, PrefabCacheData> _PrefabCache = new Dictionary<string, PrefabCacheData>();

    private float _SecondsStayInCache = 10f;
    private string _FxPath = string.Empty;
    private GameObject _Target = null;
    private float _Duration = 0;
    private int _OrderOffset = 1;
    private GameObject _FxObject;

#if IN_GAME
    private LuaInterface.LuaFunction _OnCreateLuaCB;
#endif

    //private Transform _ParentTrans = null;
    private bool _IsPlaying = false;
    private bool _IsLoading = false;
    //private float _LastStopTime = -1f;
    private Transform _CachedTrans;
    private bool _IsUSortDirty = false;
    //private bool _IsStarted = false;

    //Check to release prefabs every 60s, lifetime of prefab is 300s
    const int CHK_ITVL = 60;
    private static float CheckTime = 0;
    private static List<string> FxList2Delete = new List<string>();

    private AutoPlayFx _AutoPlayComp = null;

    public float TimeToBeDestroy { set { _SecondsStayInCache = value; } get { return /*_LastStopTime + */_SecondsStayInCache; } }
    public string FxPath { get { return _FxPath; } }
    public GameObject Target { get { return _Target; } }

    //set data before playing every single time
    private void SetData(string fxPath, GameObject target, GameObject hook_point, float duration, float secondsStayInCache, int orderOffset)
    {
        if (target == null)
            return;

        if (_IsPlaying) { StopPlay(); }

        this._CachedTrans = transform;
        this._FxPath = fxPath;
        this._Target = target;
        this._SecondsStayInCache = secondsStayInCache;
        this._OrderOffset = orderOffset;
        //duration == -1 refers to use preset
        this._Duration = duration;

        Transform trans = transform;
        if (hook_point != null && trans.parent != hook_point)
        {
            trans.SetParent(hook_point.transform, false);
        }

        if (target != hook_point)
        {
            trans.position = target.transform.position;
        }
        //transform.localRotation = target.transform.rotation;
    }

    private void JustPreLoad(UnityEngine.Object obj_pf)
    {
        if (obj_pf == null)
        {
            Debug.LogWarning("Resource not found: " + this.FxPath);
            return;
        }
        _IsLoading = false;
        PoolPrefabe(FxPath, obj_pf);
    }

    private void OnPrefabLoaded(UnityEngine.Object obj_pf)
    {
        if (obj_pf == null)
        {
            Debug.LogWarning("Resource not found: " + this.FxPath);
            return;
        }
        _IsLoading = false;
        PoolPrefabe(FxPath, obj_pf);

        //1.Am I Destroyed?
        if (_CachedTrans == null || gameObject == null || Target == null)
            return;

        //2.Inst
        _FxObject = CUnityUtil.Instantiate(obj_pf) as GameObject;
        Transform t_fx = _FxObject.transform;
        t_fx.SetParent(_CachedTrans, false);

        //3.Adjust Layer
        if (_FxObject.layer != Target.layer)
            Util.SetLayerRecursively(_FxObject, Target.layer);

        //4.Adjust sorting mode
        _IsUSortDirty = true;
        if (gameObject.activeInHierarchy)
            AjustSorting();
        //else if(_IsStarted)
        //{
        //    Debug.LogError("Playing fx on Inactive Node " + GNewUITools.PrintScenePath(_CachedTrans));
        //}

        //5 

        //6 
        StartPlay();
    }

    void AjustSorting()
    {
        if (_CachedTrans == null || _FxObject == null) return;

        if (_IsUSortDirty)
        {
            Canvas canvas_root = GetCanvasUpward(Target);
            if (canvas_root != null)
            {
                int sort_order = canvas_root.sortingOrder + _OrderOffset;
                int sort_layer = canvas_root.sortingLayerID;

                Renderer[] renders = GetComponentsInChildren<Renderer>(true);
                for (int i = 0; i < renders.Length; ++i)
                {
                    renders[i].sortingLayerID = sort_layer;
                    renders[i].sortingOrder = sort_order;
                }
            }
            _IsUSortDirty = false;
        }
    }

    Canvas GetCanvasUpward(GameObject g)
    {
        Transform t = null;
        if (g != null) t = g.transform;

        while (t != null)
        {
            Canvas canv = t.GetComponent<Canvas>();
            if (canv != null) return canv;
            t = t.parent;
        }
        return null;
    }

    private void RestartPlay()
    {
        Util.SetLayerRecursively(_FxObject, Target.layer);

        StartPlay();
    }

    private void StartPlay()
    {
        _IsPlaying = true;

        gameObject.SetActive(true);
        //If using rectMask2D
        UISFxAreaClip sfx_clip = GetComponent<UISFxAreaClip>();
        if (sfx_clip != null)
        {
            sfx_clip.UpdateFxObject(false);
        }

        if (_Duration <= 0)
        {
            FxDuration fd = _FxObject.GetComponent<FxDuration>();
            if (fd != null)
            {
                _Duration = fd.duration;
            }
        }

        // adapted to the new FX recycle mechanism by LiJian
        _AutoPlayComp = _FxObject.GetComponent<AutoPlayFx>();
        if(_AutoPlayComp == null)
            _AutoPlayComp = _FxObject.AddComponent<AutoPlayFx>();
        _AutoPlayComp.enabled = true;

        if (_Duration > 0)
        {
            Invoke("StopPlay", _Duration);
        }

        //_LastStopTime = Time.time;
#if IN_GAME
        if (_OnCreateLuaCB != null)
        {
            _OnCreateLuaCB.Call(gameObject);
            _OnCreateLuaCB.Release();
            _OnCreateLuaCB = null;
        }
#endif
    }

    private void StopPlay()
    {
        if (_IsPlaying)
        {
            CancelInvoke("StopPlay");

            _IsPlaying = false;
        }

        if (_AutoPlayComp != null)
            _AutoPlayComp.enabled = false;

        if (gameObject != null)
        {
            gameObject.SetActive(false);
        }
        _CachedTrans = null;
    }

    private void SetClipRect(GameObject rect)
    {
        UISFxAreaClip clip_com = GetComponent<UISFxAreaClip>();

        if (rect != null && clip_com == null)
        {
            clip_com = gameObject.AddComponent<UISFxAreaClip>();
        }

        if (clip_com != null)
        {
            clip_com.SetRect(rect == null ? null : rect.transform as RectTransform);
            clip_com.UpdateFxObject(false);
        }
    }

#if IN_GAME
    private void SetOnLoadCallBack(LuaInterface.LuaFunction luaCall)
    {
        if (_FxObject != null)
        {
            if (luaCall != null)
            {
                luaCall.Call(gameObject);
                luaCall.Release();
            }
        }
        else
        {
            _OnCreateLuaCB = luaCall;
        }
    }
#endif

    void OnEnable()
    {
        if (_IsUSortDirty)
        {
            AjustSorting();
            //_IsStarted = true;
        }
    }

    private void OnDisable()
    {
        if (_Duration > 0 && _IsPlaying)
        {
            StopPlay();
        }
    }

    private void OnDestroy()
    {
        if (_OnCreateLuaCB != null)
        {
            _OnCreateLuaCB.Release();
            _OnCreateLuaCB = null;
        }

        _AllCreatedList.Remove(this);

        _Target = null;
        _CachedTrans = null;
    }

    #region Interface

    [ContextMenu("适配层级")]
    public void ResetUSort()
    {
        _IsUSortDirty = true;
        //if (_IsStarted)
        {
            AjustSorting();
        }
    }

    public void MoveUSort(int sort_order)
    {
        if (!_IsUSortDirty)
        {
            if (_CachedTrans == null || _FxObject == null) return;

            Renderer[] renders = GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < renders.Length; ++i)
            {
                renders[i].sortingOrder += sort_order;
            }
        }
    }

    public static bool IsPlaying(string fxPath, GameObject target, GameObject hook_point)
    {
        if (string.IsNullOrEmpty(fxPath) || target == null) return false;
        UISfxBehaviour [] sfxs=hook_point.GetComponentsInChildren<UISfxBehaviour>();
        for(int i=0;i<sfxs.Length;i++)
        {
            UISfxBehaviour item=sfxs[i];
            if(item.Target == target && item.FxPath == fxPath&&item._IsPlaying)
            {
                return true;
            }
        }
        return false;
    }

    public static void PreLoadUIFX(string fxPath)
    {
        if (string.IsNullOrEmpty(fxPath)) return;

        PrefabCacheData sfx_pf;
        if (!_PrefabCache.TryGetValue(fxPath, out sfx_pf))
        {
            Action<UnityEngine.Object> callback = (asset) =>
            {
                PoolPrefabe(fxPath, asset);
            };
            CAssetBundleManager.AsyncLoadResource(fxPath, callback, false, "sfx");
        }
    }


    public static GameObject Play(string fxPath, GameObject target, GameObject hook_point, GameObject clipper = null, float duration = -1, float secondsStayInCache = 20f, int orderOffset = 1
#if IN_GAME
, LuaInterface.LuaFunction luaCall = null
#endif
)
    {
        if (string.IsNullOrEmpty(fxPath) || target == null) return null;

        UISfxBehaviour sfx = _AllCreatedList.Find(item => { return item.Target == target && item.FxPath == fxPath; });

        if (sfx == null)
        {
            GameObject sfx_obj = new GameObject("FXObj");
            sfx = sfx_obj.AddComponent<UISfxBehaviour>();
            _AllCreatedList.Add(sfx);
        }

        if (sfx == null) return null;

        sfx.SetData(fxPath, target, hook_point, duration, secondsStayInCache, orderOffset);
        sfx.SetClipRect(clipper);
        if (luaCall != null)
        {
#if IN_GAME
            sfx.SetOnLoadCallBack(luaCall);
#endif
        }

        if (sfx._FxObject == null)
        {
            if (!sfx._IsLoading)
            {
                PrefabCacheData sfx_pf;
                if (_PrefabCache.TryGetValue(fxPath, out sfx_pf))
                {
                    sfx.OnPrefabLoaded(sfx_pf.Obj as GameObject);
                }
                else
                {
                    sfx._IsLoading = true;
#if IN_GAME
                    Action<UnityEngine.Object> callback = (asset) =>
                    {
                        if (sfx != null)
                            sfx.OnPrefabLoaded(asset);
                    };
                    CAssetBundleManager.AsyncLoadResource(fxPath, callback, false, "sfx");
#elif UNITY_EDITOR && ART_USE
                GameObject prefab = UnityEditor.AssetDatabase.LoadAssetAtPath(fxPath, typeof(GameObject)) as GameObject;
                if (!_PrefabCache.ContainsKey(fxPath))
                    _PrefabCache.Add(fxPath, prefab as GameObject);
                //DoPlay(fxPath, sfx_pf, sfx_obj);
                sfx.OnPrefabLoaded(prefab);
#endif
                }
            }
        }
        else
        {
            sfx.RestartPlay();
        }

        return sfx.gameObject;
    }

    public static void Stop(string fxPath, GameObject target)
    {
        if (string.IsNullOrEmpty(fxPath)) return;

        for (int i = 0; i < _AllCreatedList.Count; i++)
        {
            UISfxBehaviour item = _AllCreatedList[i];
            if (item != null && item.Target == target && item.FxPath == fxPath)
            {
                item.StopPlay();
            }
        }
    }

    public static void Stop(string fxPath, GameObject target, bool destroy)
    {
        if (string.IsNullOrEmpty(fxPath)) return;

        for (int i = 0; i < _AllCreatedList.Count; i++)
        {
            UISfxBehaviour item = _AllCreatedList[i];
            if (item != null && item.Target == target && item.FxPath == fxPath)
            {
                item.StopPlay();
                if (destroy)
                    Destroy(item.gameObject);
            }
        }
    }

    public static void Tick(float dt)
    {
        if (_AllCreatedList == null || _AllCreatedList.Count <= 0) return;
        //float t = Time.time;
        for (int i = 0; i < _AllCreatedList.Count; i++)
        {
            UISfxBehaviour uib = _AllCreatedList[i];
            if (uib == null)
            {
                _AllCreatedList.RemoveAt(i);
                i -= 1;
            }
            else
            {
                if (!uib._IsPlaying && !uib._IsLoading)
                {
                    uib.TimeToBeDestroy -= dt;
                    if (uib.TimeToBeDestroy < 0)
                    {
                        GameObject.Destroy(uib.gameObject);
                    }
                }
            }
        }

        CheckPrefabCached(dt);
    }

    public static void ClearAllCaches()
    {
        _PrefabCache.Clear();
    }

    private static void CheckPrefabCached(float dt)
    {
        CheckTime -= dt;
        if (CheckTime < 0)
        {
            CheckTime = CHK_ITVL;

            using (Dictionary<string, PrefabCacheData>.Enumerator emu = _PrefabCache.GetEnumerator())
            {
                while (emu.MoveNext())
                {
                    var data = emu.Current.Value;

                    if (data.LifeTime < CHK_ITVL)
                    {
                        FxList2Delete.Add(emu.Current.Key);
                        data.Obj = null;
                    }
                    else
                    {
                        data.LifeTime -= CHK_ITVL;
                    }
                }
            }

            for (int i = 0; i < FxList2Delete.Count; i++)
            {
                _PrefabCache.Remove(FxList2Delete[i]);
            }

            FxList2Delete.Clear();
        }
    }

    private static void PoolPrefabe(string res_path, UnityEngine.Object obj)
    {
#if UNITY_EDITOR
        if (_PrefabCache.Count > 200)
        {
            Debug.LogWarning("Prefab Cached seems to cache too many. ");
        }
#endif //UNITY_EDITOR

        if (!_PrefabCache.ContainsKey(res_path))
        {
            _PrefabCache.Add(res_path, new PrefabCacheData(obj, PrefabCacheData.CACHE_LIFE_TIME));
        }
    }

    private static GameObject UnPoolPrefab(string res_path)
    {
        PrefabCacheData sfx_pf;
        if (_PrefabCache.TryGetValue(res_path, out sfx_pf))
        {
            if (sfx_pf.Obj != null)
            {
                return sfx_pf.Obj as GameObject;
            }
        }
        return null;
    }



    #endregion
}
