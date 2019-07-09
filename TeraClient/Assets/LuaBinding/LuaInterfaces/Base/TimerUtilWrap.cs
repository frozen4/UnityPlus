using Common;
using LuaInterface;
using System;
using UnityEngine;
using EntityComponent;

public static class TimerUtilWrap
{


    public static LuaMethod[] timer_regs = new LuaMethod[]
    {
        new LuaMethod("AddTimer", AddTimer),
        new LuaMethod("AddGlobalTimer", AddGlobalTimer),
        new LuaMethod("AddGlobalLateTimer", AddGlobalLateTimer),
        new LuaMethod("RemoveTimer", RemoveTimer),
        new LuaMethod("RemoveGlobalTimer", RemoveGlobalTimer),
        new LuaMethod("ResetTimer", ResetTimer),
        new LuaMethod("ResetGlobalTimer", ResetGlobalTimer),
    };

    public static void Register(IntPtr L)
    {
        LuaScriptMgr.RegisterLib(L, "TimerUtil", timer_regs);
        timer_regs = null;
    }

    public static void LogParamError(string methodName, int count)
    {
        HobaDebuger.LogErrorFormat("invalid arguments to method: TimerUtilWrap.{0} count: {1}", methodName, count);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int AddTimer(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 1;

        if (count == 4 && LuaScriptMgr.CheckTypes(L, 1, typeof(GameObject), typeof(double), typeof(bool), typeof(LuaFunction)))
        {
            GameObject obj = LuaScriptMgr.GetUnityObject<GameObject>(L, 1);
            if (obj == null)
            {
                HobaDebuger.LogError("AddTimer: param 1 must be GameObject");
                LuaDLL.lua_pushinteger(L, -1);
                return GameUtilWrap.CheckReturnNum(L, count, nRet);
            }

            ObjectBehaviour comp = obj.GetComponent<ObjectBehaviour>();
            if (comp == null)
            {
                HobaDebuger.LogError("AddTimer: param 1 must have ObjectBehaviour");
                LuaDLL.lua_pushinteger(L, -1);
                return GameUtilWrap.CheckReturnNum(L, count, nRet);
            }

            float ttl = (float)LuaDLL.lua_tonumber(L, 2);
            bool once = LuaDLL.lua_toboolean(L, 3);
            //LuaFunction cb = LuaScriptMgr.GetLuaFunction(L, 4);
            LuaDLL.lua_pushvalue(L, 4);
            int callbackRef = LuaDLL.luaL_ref(L, LuaIndexes.LUA_REGISTRYINDEX);
            int id = comp.AddTimer(ttl, once, callbackRef, string.Empty);

            LuaDLL.lua_pushinteger(L, id);
            return GameUtilWrap.CheckReturnNum(L, count, nRet);
        }
        else if (count == 5 && LuaScriptMgr.CheckTypes(L, 1, typeof(GameObject), typeof(double), typeof(bool), typeof(LuaFunction), typeof(string)))
        {
            GameObject obj = LuaScriptMgr.GetUnityObject<GameObject>(L, 1);
            if (obj == null)
            {
                HobaDebuger.LogError("AddTimer: param 1 must be GameObject");
                LuaDLL.lua_pushinteger(L, -1);
                return GameUtilWrap.CheckReturnNum(L, count, nRet);
            }

            ObjectBehaviour comp = obj.GetComponent<ObjectBehaviour>();
            if (comp == null)
            {
                HobaDebuger.LogError("AddTimer: param 1 must have ObjectBehaviour");
                LuaDLL.lua_pushinteger(L, -1);
                return GameUtilWrap.CheckReturnNum(L, count, nRet);
            }

            float ttl = (float)LuaDLL.lua_tonumber(L, 2);
            bool once = LuaDLL.lua_toboolean(L, 3);
            //LuaFunction cb = LuaScriptMgr.GetLuaFunction(L, 4);
            LuaDLL.lua_pushvalue(L, 4);
            int callbackRef = LuaDLL.luaL_ref(L, LuaIndexes.LUA_REGISTRYINDEX);
            string debugInfo = LuaScriptMgr.GetString(L, 5);

            int id = comp.AddTimer(ttl, once, callbackRef, debugInfo);

            LuaDLL.lua_pushinteger(L, id);
            return GameUtilWrap.CheckReturnNum(L, count, nRet);
        }
        else
        {
            LogParamError("AddTimer", count);
            LuaDLL.lua_pushinteger(L, -1);
            return GameUtilWrap.CheckReturnNum(L, count, nRet);
        }
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int AddGlobalTimer(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 1;

        if (count == 3 && LuaScriptMgr.CheckTypes(L, 1, typeof(double), typeof(bool), typeof(LuaFunction)))
        {
            float ttl = (float)LuaDLL.lua_tonumber(L, 1);
            bool once = LuaDLL.lua_toboolean(L, 2);
            //LuaFunction cb = LuaScriptMgr.GetLuaFunction(L, 3);
            LuaDLL.lua_pushvalue(L, 3);
            int callbackRef = LuaDLL.luaL_ref(L, LuaIndexes.LUA_REGISTRYINDEX);
            int id = EntryPoint.Instance.AddTimer(ttl, once, callbackRef, string.Empty, false);
            LuaDLL.lua_pushinteger(L, id);
            return GameUtilWrap.CheckReturnNum(L, count, nRet);
        }
        else if (count == 4 && LuaScriptMgr.CheckTypes(L, 1, typeof(double), typeof(bool), typeof(LuaFunction), typeof(string)))
        {
            float ttl = (float)LuaDLL.lua_tonumber(L, 1);
            bool once = LuaDLL.lua_toboolean(L, 2);
            //LuaFunction cb = LuaScriptMgr.GetLuaFunction(L, 3);
            LuaDLL.lua_pushvalue(L, 3);
            int callbackRef = LuaDLL.luaL_ref(L, LuaIndexes.LUA_REGISTRYINDEX);

            string debugInfo = LuaScriptMgr.GetString(L, 4);
            int id = EntryPoint.Instance.AddTimer(ttl, once, callbackRef, debugInfo, false);
            LuaDLL.lua_pushinteger(L, id);
            return GameUtilWrap.CheckReturnNum(L, count, nRet);
        }
        else
        {
            LogParamError("AddGlobalTimer", count);
            LuaDLL.lua_pushinteger(L, -1);
            return GameUtilWrap.CheckReturnNum(L, count, nRet);
        }
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int AddGlobalLateTimer(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 1;

        if (count == 3 && LuaScriptMgr.CheckTypes(L, 1, typeof(double), typeof(bool), typeof(LuaFunction)))
        {
            float ttl = (float)LuaDLL.lua_tonumber(L, 1);
            bool once = LuaDLL.lua_toboolean(L, 2);
            //LuaFunction cb = LuaScriptMgr.GetLuaFunction(L, 3);
            LuaDLL.lua_pushvalue(L, 3);
            int cb = LuaDLL.luaL_ref(L, LuaIndexes.LUA_REGISTRYINDEX);
            int id = EntryPoint.Instance.AddTimer(ttl, once, cb, string.Empty, true);
            LuaDLL.lua_pushinteger(L, id);
            return GameUtilWrap.CheckReturnNum(L, count, nRet);
        }
        else if (count == 4 && LuaScriptMgr.CheckTypes(L, 1, typeof(double), typeof(bool), typeof(LuaFunction), typeof(string)))
        {
            float ttl = (float)LuaDLL.lua_tonumber(L, 1);
            bool once = LuaDLL.lua_toboolean(L, 2);
            //LuaFunction cb = LuaScriptMgr.GetLuaFunction(L, 3);
            LuaDLL.lua_pushvalue(L, 3);
            int cb = LuaDLL.luaL_ref(L, LuaIndexes.LUA_REGISTRYINDEX);
            string debugInfo = LuaScriptMgr.GetString(L, 4);
            int id = EntryPoint.Instance.AddTimer(ttl, once, cb, debugInfo, true);
            LuaDLL.lua_pushinteger(L, id);
            return GameUtilWrap.CheckReturnNum(L, count, nRet);
        }
        else
        {
            LogParamError("AddGlobalLateTimer", count);
            LuaDLL.lua_pushinteger(L, -1);
            return GameUtilWrap.CheckReturnNum(L, count, nRet);
        }
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int RemoveTimer(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 2 && LuaScriptMgr.CheckTypes(L, 1, typeof(GameObject), typeof(int)))
        {
            GameObject obj = LuaScriptMgr.GetUnityObject<GameObject>(L, 1);
            if (obj == null)
            {
                HobaDebuger.LogError("RemoveTimer: param 1 must be GameObject");
                return GameUtilWrap.CheckReturnNum(L, count, nRet);
            }

            ObjectBehaviour comp = obj.GetComponent<ObjectBehaviour>();
            if (comp == null)
            {
                HobaDebuger.LogError("RemoveTimer: param 1 must have ObjectBehaviour");
                return GameUtilWrap.CheckReturnNum(L, count, nRet);
            }

            int id = (int)LuaScriptMgr.GetNumber(L, 2);
            comp.RemoveTimer(id);
            return GameUtilWrap.CheckReturnNum(L, count, nRet);
        }
        else
        {
            LogParamError("RemoveTimer", count);
            return GameUtilWrap.CheckReturnNum(L, count, nRet);
        }
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int RemoveGlobalTimer(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 1 && LuaScriptMgr.CheckTypes(L, 1, typeof(int)))
        {
            int id = (int)LuaScriptMgr.GetNumber(L, 1);
            EntryPoint.Instance.RemoveTimer(id);
        }
        else
        {
            LogParamError("RemoveGlobalTimer", count);
        }
        return GameUtilWrap.CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int ResetTimer(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 2 && LuaScriptMgr.CheckTypes(L, 1, typeof(GameObject), typeof(int)))
        {
            GameObject obj = LuaScriptMgr.GetUnityObject<GameObject>(L, 1);
            if (obj == null)
            {
                HobaDebuger.LogError("ResetTimer: param 1 must be GameObject");
                return GameUtilWrap.CheckReturnNum(L, count, nRet);
            }

            ObjectBehaviour comp = obj.GetComponent<ObjectBehaviour>();
            if (comp == null)
            {
                HobaDebuger.LogError("ResetTimer: param 1 must have ObjectBehaviour");
                return GameUtilWrap.CheckReturnNum(L, count, nRet);
            }

            int id = (int)LuaScriptMgr.GetNumber(L, 2);
            comp.ResetTimer(id);
            return GameUtilWrap.CheckReturnNum(L, count, nRet);
        }
        else
        {
            LogParamError("ResetTimer", count);
            return GameUtilWrap.CheckReturnNum(L, count, nRet);
        }
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int ResetGlobalTimer(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 1 && LuaScriptMgr.CheckTypes(L, 1, typeof(int)))
        {
            int id = (int)LuaScriptMgr.GetNumber(L, 1);
            EntryPoint.Instance.ResetTimer(id);
        }
        else
        {
            LogParamError("ResetGlobalTimer", count);
        }
        return GameUtilWrap.CheckReturnNum(L, count, nRet);
    }
}