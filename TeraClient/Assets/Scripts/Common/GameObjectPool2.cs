using System.Collections.Generic;
using UnityEngine;
public class UnityObjectPool
{
    private readonly Stack<Object> _Stack = new Stack<Object>();

    private Object _Prefab;
    private int _Limite;

    public UnityObjectPool(Object prefab, int limite)
    {
        _Prefab = prefab;
        _Limite = limite;
    }

    public Object Get()
    {
        Object element = null;
        if (_Stack.Count == 0)
        {
            element = CUnityUtil.Instantiate(_Prefab);
        }
        else
        {
            element = _Stack.Pop();
        }

        return element;
    }

    public void Release(Object element)
    {
        if (_Stack.Count >= _Limite)
        {
            Object.Destroy(element);
            return;
        }

        _Stack.Push(element);
    }

    public void ReleaseAll()
    {
        while (_Stack.Count > 0)
        {
            Object element = _Stack.Pop();
            Object.Destroy(element);
        }
    }
}

public class TimedPoolData
{
    //public const int CACHE_LIFE_TIME = 300;

    public System.Object Obj;
    public float LifeTime = 0f;

    public TimedPoolData(System.Object obj, float life_time)
    {
        Obj = obj;
        LifeTime = life_time;
    }

    public void Set(System.Object obj, float life_time)
    {
        Obj = obj;
        LifeTime = life_time;
    }
}

public interface ITimedPool
{
    void Tick(float delta_time);
    void Clear();
    void SetValid(bool is_valid);
    bool IsValid();
}

public class UnityTimedPool : System.Object, System.IDisposable, ITimedPool
{
    private readonly List<TimedPoolData> _list = new List<TimedPoolData>();
    private float _LifeTime;
    private int _MaxCount;
    public int MaxCount { get { return _MaxCount; } }

    //private System.Func<System.Object> _OnCreate;
    private UnityEngine.Events.UnityAction<System.Object> _OnPool,_OnDelete;

    private bool _IsValid;

    public UnityTimedPool(int max_count, float time, UnityEngine.Events.UnityAction<System.Object> on_pool, UnityEngine.Events.UnityAction<System.Object> on_delete)
    {
        _MaxCount = max_count;
        _LifeTime = time;
        //_OnCreate = on_create;
        _OnPool = on_pool;
        _OnDelete = on_delete;
    }

    public void SetValid(bool is_valid)
    {
        _IsValid = is_valid;
    }

    public bool IsValid()
    {
        return _IsValid;
    }

    public virtual System.Object Get()
    {
        if (!IsValid()) return null;

        System.Object element = null;
        if (_list.Count > 0)// { return _OnCreate != null ? _OnCreate() : null; }
        {
            while (element == null)
            {
                if (_list.Count == 0) break;

                element = _list[0].Obj;
                _list.RemoveAt(0);
            }
        }

        return element;
    }

    public virtual void Pool(System.Object element)
    {
        if (!IsValid()) return;
        if (element == null) return;

        if (_list.Count >= _MaxCount)
        {
            if (_OnDelete != null) _OnDelete(element);
        }
        else
        {
            if (_OnPool!=null) _OnPool(element);
            _list.Add(new TimedPoolData(element, _LifeTime));
        }
    }

    public void Clear()
    {
        if (_OnDelete != null)
        {
            for (int i = 0; i < _list.Count; i++)
            {
                TimedPoolData data = _list[i];
                if (data != null)
                {
                    _OnDelete(data.Obj);
                }
            }
        }
        _list.Clear();
    }

    public void Dispose()
    {
        Clear();
    }

    public void Tick(float delta_time)
    {
        for (int i = 0; i < _list.Count; i++)
        {
            TimedPoolData data = _list[i];
            data.LifeTime -= delta_time;

            if (data.LifeTime < 0)
            {
                if (_OnDelete != null) _OnDelete(_list[i].Obj);
                _list.RemoveAt(i);
                i -= 1;
            }
        }
    }

}

public class UnityTimedDict : System.Object, System.IDisposable, ITimedPool
{
    private static List<string> _ToDelete = new List<string>();

    private readonly Dictionary<string, TimedPoolData> _Dict = new Dictionary<string, TimedPoolData>();
    private float _LifeTime;
    public int Count { get { return _Dict.Count; } }

    //private System.Func<System.Object> _OnPool;
    private UnityEngine.Events.UnityAction<System.Object> _OnPool, _OnDelete;

    private bool _IsValid;

    public UnityTimedDict(float time, UnityEngine.Events.UnityAction<System.Object> on_pool, UnityEngine.Events.UnityAction<System.Object> on_delete)
    {
        _LifeTime = time;
        _OnPool = on_pool;
        _OnDelete = on_delete;
    }

    public void SetValid(bool is_valid)
    {
        _IsValid = is_valid;
    }

    public bool IsValid()
    {
        return _IsValid;
    }

    public System.Object Get(string key)
    {
        if (!IsValid()) return null;

        TimedPoolData data;
        _Dict.TryGetValue(key, out data);

        if (data != null)
        {
            if (data.Obj != null)
            {
                data.LifeTime = _LifeTime;
                return data.Obj;
            }
            else
            {
                _Dict.Remove(key);
            }
        }
        return null;
    }

    public void Pool(string key, object element)
    {
        if (!IsValid()) return;
        if (element == null) return;

        TimedPoolData data;
        _Dict.TryGetValue(key, out data);
        if (data == null)
        {
            if(_OnPool != null) _OnPool(element);
                _Dict.Add(key, new TimedPoolData(element, _LifeTime));
        }
        else
        {
            if (data.Obj == null)
            {
                if(_OnPool != null) _OnPool(element);
                data.Set(element, _LifeTime);
            }
        }
    }

    public void Clear()
    {
        if (_OnDelete != null)
        {
            using (Dictionary<string, TimedPoolData>.Enumerator emu = _Dict.GetEnumerator())
            {
                while (emu.MoveNext())
                {
                    var data = emu.Current.Value;
                    if (data != null)
                    {
                        _OnDelete(data.Obj);
                    }
                }
            }
        }
        _Dict.Clear();
    }

    public void Dispose()
    {
        Clear();
    }

    public void Tick(float delta_time)
    {
        _ToDelete.Clear();
        using (Dictionary<string, TimedPoolData>.Enumerator emu = _Dict.GetEnumerator())
        {
            while (emu.MoveNext())
            {
                var data = emu.Current.Value;
                data.LifeTime -= delta_time;

                if (data.LifeTime <= 0)
                {
                    _ToDelete.Add(emu.Current.Key);
                    if (_OnDelete != null) _OnDelete(data.Obj);
                }
            }
        }

        for (int i = 0; i < _ToDelete.Count; i++)
        {
            _Dict.Remove(_ToDelete[i]);
        }
        _ToDelete.Clear();
    }
}


public class TimedPoolManager : Common.Singleton<TimedPoolManager>
{
    private readonly List<ITimedPool> _ListPool = new List<ITimedPool>();

    public UnityTimedPool GetUnityTimedPool(int max_count, float life_time, UnityEngine.Events.UnityAction<System.Object> on_pool, UnityEngine.Events.UnityAction<System.Object> on_delete)
    {
        UnityTimedPool pool = new UnityTimedPool(max_count, life_time, on_pool, on_delete);
        _ListPool.Add(pool);
        pool.SetValid(true);
        return pool;
    }

    public UnityTimedDict GetUnityTimedDict(int max_count, float life_time, UnityEngine.Events.UnityAction<System.Object> on_pool, UnityEngine.Events.UnityAction<System.Object> on_delete)
    {
        UnityTimedDict pool = new UnityTimedDict(life_time, on_pool, on_delete);
        _ListPool.Add(pool);
        pool.SetValid(true);
        return pool;
    }


    public void LateTick(float dt)
    {
        for (int i = 0; i < _ListPool.Count; i++)
        {
            _ListPool[i].Tick(dt);
        }
    }

    public void ReleasePool(ITimedPool pool)
    {
        if (pool != null)
        {
            pool.Clear();
            _ListPool.Remove(pool);
            pool.SetValid(false);
        }
    }

    //public static void Clear()
    //{
    //    if (Instance != null)
    //    {
    //        for (int i = 0; i < Instance._listPool.Count; i++)
    //        {
    //            ReleasePool(Instance._listPool[i]);
    //        }
    //        Instance._listPool.Clear();
    //    }
    //}
}