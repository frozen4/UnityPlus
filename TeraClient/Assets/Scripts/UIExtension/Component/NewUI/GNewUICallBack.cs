using UnityEngine;
using System.Collections.Generic;
using LuaInterface;
using UnityEngine.UI;

public class GNewUICallBack : GBase
{
    public enum CallType
    {
        OnClick,
    }

    private class LuaCallSt
    {
        public LuaFunction luaCall;
        public LuaTable owner;
        public GameObject go;
        public object data;

        public void OnClick(GameObject g)
        {
            if (go == g)
            {
                if (owner != null)
                {
                    luaCall.Call(owner, g, data);
                }
                else
                {
                    luaCall.Call(g, data);
                }
            }
        }
    }

    //List<LuaCallSt> _clickList = new List<LuaCallSt>();
    private delegate void OnButtonClick(GameObject go);
    private List<LuaCallSt> clicks;

    public void RegisterCall(CallType ct, GameObject g, LuaTable owner, LuaFunction lua_func, object param)
    {
        if (lua_func != null) { return; }

        if (ct == CallType.OnClick)
        {
            LuaCallSt lc = AddClickWithData(g, owner, lua_func, param);
            if (lc != null)
            {
                GButton gb = GetComponent<GButton>();
                if (gb != null)
                {
                    gb.OnClick += lc.OnClick;
                }
            }
        }
    }

    public void UnRegisterCall(CallType ct, GameObject g, LuaTable owner, LuaFunction lua_func)
    {
        if (lua_func != null) { return; }

        if (ct == CallType.OnClick)
        {
            LuaCallSt lc = RemoveClickWithData(g, owner, lua_func);
            if (lc != null)
            {
                GButton gb = GetComponent<GButton>();
                if (gb != null)
                {
                    gb.OnClick -= lc.OnClick;
                }
            }
        }
    }

    private LuaCallSt AddClickWithData(GameObject go, System.Object lua, System.Object luafuc, object data)
    {
        if (clicks == null)
        {
            clicks = new List<LuaCallSt>();
        }

        LuaCallSt lc = null;
        lc = FindClickCall(go, lua, luafuc);
        if (null == lc)
        {
            lc = new LuaCallSt();
            lc.luaCall = luafuc as LuaFunction;
            lc.owner = lua as LuaTable;
            lc.go = go;
        }
        lc.data = data;
        clicks.Add(lc);
        return lc;
    }

    private LuaCallSt RemoveClickWithData(GameObject go, System.Object lua, System.Object luafuc)
    {
        if (clicks != null)
        {
            LuaCallSt lc = null;
            lc = FindClickCall(go, lua, luafuc);
            if (null!=lc)
            {
                clicks.Remove(lc);
            }
            return lc;
        }

        return null;
    }

    private LuaCallSt FindClickCall(GameObject go, System.Object lua, System.Object luafuc/*, object data*/)
    {
        for (int i = 0; i < clicks.Count; i++)
        {
            LuaCallSt lc = clicks[i];
            if (lc.go == go && lc.luaCall == lua && luafuc == lc.luaCall /*&& lc.data == data*/)
            {
                return lc;
            }
        }
        return null;
    }

}
