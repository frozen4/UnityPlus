using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

struct SGameObjInfo
{
    public Transform transform;
}

public static class GameObjectUtil
{
    static GameObject _go_UICamera = null;
    static Camera _UICamera = null;
    static GameObject _go_BackUICamera = null;

    //static List<UIEventListener> _top

    public static GameObject GetBackUICamera()
    {
        if (_go_BackUICamera == null)
            _go_BackUICamera = GameObject.Find("BackUICamera");
        
        return _go_BackUICamera;
    }

    public static Camera GetUICamera()
    {
        if (_UICamera == null)
        {
            _go_UICamera = GameObject.Find("UICamera");
            if (_go_UICamera != null)
                _UICamera = _go_UICamera.GetComponent<Camera>();
        }
        return _UICamera;
    }

    public static GameObject GetUIRoot()
    {
        return Main.UIRootCanvas != null ? Main.UIRootCanvas.gameObject : null;
    }

    public static Transform GetUIRootTranform()
    {
        return Main.UIRootCanvas;
    }

    public static Transform GetGameObjTransform(GameObject go)
    {
        if (go == null)
            return null;

        return go.transform;
    }




}