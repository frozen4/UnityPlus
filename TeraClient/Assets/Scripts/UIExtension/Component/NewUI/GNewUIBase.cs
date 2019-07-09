using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class GNewUIBase : UIBehaviour
{
    private Transform _Transform;
    private RectTransform _RectTransform;

    [NoToLua]
    public Transform Trans
    {
        get
        {
            if (_Transform == null)
                _Transform = transform;
            return _Transform;
        }
    }

    [NoToLua]
    public RectTransform RectTrans
    {
        get
        {
            if (_RectTransform == null)
                _RectTransform = transform as RectTransform;
            if (_RectTransform == null)
                _RectTransform = gameObject.AddComponent<RectTransform>();
            return _RectTransform;
        }
    }

    bool _IsInited = false;

    protected void SafeInit()
    {
        if (!_IsInited)
        {
            _IsInited = true;
			OnSafeInit();
        }
    }

    //override, dont call
    protected virtual void OnSafeInit()
    {
    }

    protected bool IsInited
    {
        get { return _IsInited; }
    }

    protected override void Awake()
    {
        base.Awake();
        SafeInit();
    }

}

public class GNewPrivatePool<T> where T : class
{
    protected Stack<T> _PoolItems;
    private int _MaxSize;

    public const int DefualtSize = 32;

    public GNewPrivatePool()
    {
        _PoolItems = new Stack<T>();
        _MaxSize = DefualtSize;
    }

    public GNewPrivatePool(int pool_size)
    {
        _PoolItems = new Stack<T>();
        if (pool_size < 0)
            _MaxSize = DefualtSize;
        else
            _MaxSize = pool_size;
    }

    public void SetSize(int count)
    {
        if (_PoolItems.Count < count)
        {
            _MaxSize = count;
        }
    }

    public bool PutIn(T t)
    {
        if (t == null) return false;

        bool rv;

        if (_PoolItems.Count < _MaxSize)
        {
            _PoolItems.Push(t);
            rv = true;
        }
        else
        {
            //i didnt take it.
            rv = false;
            //#if UNITY_EDITOR
            //            Debug.LogWarning("_poolItems.Count exceeds max size!!!");
            //#endif
        }

        return rv;
    }

    public T TakeOut()
    {
        if (_PoolItems.Count > 0)
        {
            T t = _PoolItems.Pop();

            return t;
        }
        return null;
    }

    public void Clear()
    {
        _PoolItems.Clear();
    }

}
