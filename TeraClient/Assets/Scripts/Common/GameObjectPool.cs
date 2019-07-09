using UnityEngine;
using System.Collections.Generic;
using Common;

public class GameObjectPool : MonoBehaviour
{
    private Transform _Parent = null;
    private GameObject _Template;
    private int _MaxCacheCount = 5;
    private readonly Stack<GameObject> _Stack = new Stack<GameObject>();

    void Awake()
    {
        if(_Parent == null)
        {
            var poolObject = new GameObject("_Pool");
            _Parent = poolObject.transform;
            _Parent.parent = gameObject.transform;
            poolObject.SetActive(false);
        }
    }

    public void Regist(GameObject template, int maxCount = 5)
    {
        _Template = template;
        _MaxCacheCount = maxCount;
        if(template == null)
            HobaDebuger.LogError("Can not Regist null to GameObjectPool");
    }

    public GameObject Get()
    {
        GameObject element = null;
        if (_Stack.Count == 0)
        {
            if (_Template == null)
                return null;

            element = Object.Instantiate<GameObject>(_Template);
            if (!element.activeSelf)
                element.SetActive(true);
        }
        else
        {
            element = _Stack.Pop();
        }

        return element;
    }

    public void Release(GameObject element)
    {
        if (_Stack.Count >= _MaxCacheCount)
        {
            Destroy(element);
            return;
        }

        if(_Stack.Contains(element))
        {
            HobaDebuger.LogWarningFormat("Can not release one element two times: {0}", element.name);
            //LuaScriptMgr.Instance.CallOnTraceBack();
            return;
        }
        element.transform.SetParent(_Parent);
        _Stack.Push(element);
    }
}
