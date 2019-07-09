using System;
using System.Collections.Generic;
using Common;
using UnityEngine;
using GameLogic;
using EntityComponent;
using System.Collections;

public class ResCacheMan : Singleton<ResCacheMan>, ITickLogic
{
    public struct CacheItem:IEquatable<CacheItem>
    {
        public GameObject Model;
        public float TimeStamp;
        public bool Equals(CacheItem other)
        {
            if (!(this.Model.Equals(other.Model)))
                return false;
            if (!(this.TimeStamp.Equals(other.TimeStamp)))
                return false;
            else
                return true;
        }
        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (!(obj is CacheItem))
                return false;
            return Equals((CacheItem)obj);

        }
        public override int GetHashCode()
        {
            return Model.GetHashCode() ^ TimeStamp.GetHashCode();
        }
    }

    private Transform _ResCacheRoot;
    private readonly Dictionary<int, LinkedList<CacheItem>> _CacheMap = new Dictionary<int, LinkedList<CacheItem>>();
    private readonly List<int> _TempList = new List<int>();
    private int _CurCacheCount = 0;

    public int MaxCacheCount = 30;

    private readonly List<GameObject> _CachedBaseResMap = new List<GameObject>();

    public void Init()
    {
        if(_ResCacheRoot == null)
        {
            var root = new GameObject("ResourceCache");
            root.SetActive(false);
            _ResCacheRoot = root.transform;
        }
    }

    public bool FetchResFromCache(int resId, out CacheItem ent)
    {
        LinkedList<CacheItem> ls;
        if (_CacheMap.TryGetValue(resId, out ls))
        {
            ent = ls.First.Value;
            ls.RemoveFirst();

            if (ls.Count == 0)
                _CacheMap.Remove(resId);

            _CurCacheCount--;

            return true;
        }

        ent = default(CacheItem);
        return false;
    }

    public void AddResToCache(int resId, GameObject go)
    {
        if (MaxCacheCount == 0)
        {
            if (go != null)
                UnityEngine.Object.Destroy(go);
            return;
        }

        LinkedList<CacheItem> ls;
        if (!_CacheMap.TryGetValue(resId, out ls))
        {
            ls = new LinkedList<CacheItem>();
            _CacheMap[resId] = ls;
            _CurCacheCount++;
        }
        else
        {
            if (_CurCacheCount >= MaxCacheCount)
            {
                var oldGo = ls.First.Value.Model;
                ls.RemoveFirst();

                if (oldGo != null)
                    UnityEngine.Object.Destroy(oldGo);
            }
            else
            {
                _CurCacheCount++;
            }
        }

        if (go != null)
        {
            go.transform.parent = _ResCacheRoot;
            go.transform.localPosition = Vector3.zero;
            go.transform.forward = Vector3.forward;
            go.transform.localScale = Vector3.one;
            var entityComps = go.GetComponents<MonoBehaviour>();
            for(var i = 0; i < entityComps.Length; i++)
            {
                IRecyclable recyclable = entityComps[i] as IRecyclable;
                if (recyclable != null)
                    recyclable.OnRecycle();
            }

            var bocol = go.GetComponent<BodyPartCollector>();
            if(null != bocol)
                bocol.Revert();
        }
        
        CacheItem ent = new CacheItem();
        ent.Model = go;
        ent.TimeStamp = Time.time;
        ls.AddLast(ent);
    }

    
    public GameObject GetEntityBaseRes()
    {
        GameObject go = null;
        if (_CachedBaseResMap.Count > 0)
        {
            go = _CachedBaseResMap[0];
            _CachedBaseResMap.RemoveAt(0);

            go.transform.position = Vector3.zero;
            go.transform.rotation = Quaternion.identity;
            go.transform.localScale = Vector3.one;
        }
        else
        {
            go = new GameObject();
        }
        go.transform.parent = null;

        return go;
    }

    public void RecycleEntityBaseRes(GameObject go)
    {
        if (go == null) return;

        var entityComp = go.GetComponent<ObjectBehaviour>();
        if (entityComp != null)
            entityComp.OnRecycle();

        var ms = go.GetComponents<CMotor>();
        if (ms != null && ms.Length > 0)
        {
            for(var i = 0; i < ms.Length; i++)
                UnityEngine.Object.Destroy(ms[i]);
        }

        _CachedBaseResMap.Add(go);
        go.transform.parent = _ResCacheRoot;
    }

    public void Cleanup()
    {
        foreach(var ls in _CacheMap.Values)
        {
            foreach (CacheItem ent in ls)
            {
                var go = ent.Model;
                if (go != null)
                    UnityEngine.Object.Destroy(go);
            }
        }

        _CacheMap.Clear();

        for(var i = 0; i < _CachedBaseResMap.Count; i++)
        {
            var go = _CachedBaseResMap[i];
            if (go != null)
                UnityEngine.Object.Destroy(go);
        }
        _CachedBaseResMap.Clear();
    }

    public void Tick(float dt) {}

    public IEnumerable TickCoroutine()
    {
        if (Time.frameCount % 600 == 0)
        {
            _TempList.Clear();
            int curCount = 0;
            bool isFull = false;

            var it = _CacheMap.GetEnumerator();
            while (it.MoveNext())
            {
                var ls = it.Current.Value;
                var pos = ls.First;

                while (pos != null)
                {
                    var posNext = pos.Next;
                    var ent = pos.Value;

                    float curtime = Time.time;
                    if (curtime > ent.TimeStamp + 20f)
                    {
                        ls.Remove(pos);
                        _CurCacheCount--;
                        var go = ent.Model;

                        if (go != null)
                        {
                            UnityEngine.Object.Destroy(go);
                            curCount++;

                            if (curCount >= 5)
                            {
                                isFull = true;
                                break;
                            }
                        }

                    }

                    pos = posNext;
                }

                if (ls.Count == 0)
                    _TempList.Add(it.Current.Key);

                if (isFull)
                    break;
            }
            it.Dispose();

            foreach (var v in _TempList)
                _CacheMap.Remove(v);

            yield return null;
        }
    }
}
