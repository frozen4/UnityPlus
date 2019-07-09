using System;
using UnityEngine;
using LuaInterface;
using Common;

public static class UnityUtilWrap
{

    public static LuaMethod[] engine_regs = new LuaMethod[]
    {
        new LuaMethod("SetPosition", SetPosition),
        new LuaMethod("SetLocalPosition", SetLocalPosition),
        new LuaMethod("SetQuaternion", SetQuaternion),
        new LuaMethod("PlayAnimation", PlayAnimation),
    };

    public static void Register(IntPtr L)
    {
        LuaScriptMgr.RegisterLib(L, "UnityUtil", engine_regs);
        engine_regs = null;
    }

    public static void LogParamError(string methodName, int count)
    {
        HobaDebuger.LogErrorFormat("invalid arguments to method: UnityUtilWrap.{0} count: {1}", methodName, count);
    }


    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int SetPosition(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if ((count == 2 && LuaScriptMgr.CheckTypes(L, 1, typeof(GameObject), typeof(string))))
        {
            
        }
        else
        {
            LogParamError("SetPosition", count);
        }
        return GameUtilWrap.CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int SetLocalPosition(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if ((count == 2 && LuaScriptMgr.CheckTypes(L, 1, typeof(GameObject), typeof(string))))
        {

        }
        else
        {
            LogParamError("SetLocalPosition", count);
        }
        return GameUtilWrap.CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int SetQuaternion(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if ((count == 2 && LuaScriptMgr.CheckTypes(L, 1, typeof(GameObject), typeof(string))))
        {

        }
        else
        {
            LogParamError("SetQuaternion", count);
        }
        return GameUtilWrap.CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int PlayAnimation(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if ((count == 3 && LuaScriptMgr.CheckTypes(L, 1, typeof(GameObject), typeof(string), typeof(float))))
        {
            var go = LuaScriptMgr.GetUnityObject<GameObject>(L, 1);
            var name = LuaDLL.lua_tostring(L, 2);
            var speed = (float)LuaScriptMgr.GetNumber(L, 3);
            if(go != null)
            {
                var aniComp = go.GetComponent<Animation>();
                if(aniComp != null)
                {
                    var aniState = aniComp[name];
                    if(aniState != null)
                    {
                        aniState.speed = speed;
                        aniState.time = speed >= 0 ? 0 : aniState.length;
                        aniComp.Play(name);
                    }
                }
            }
        }
        else
        {
            LogParamError("PlayAnimation", count);
        }
        return GameUtilWrap.CheckReturnNum(L, count, nRet);
    }
}
