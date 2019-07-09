using System;
using UnityEngine;
using LuaInterface;
using Object = UnityEngine.Object;

public class GImageModelWrap
{
    public static void Register(IntPtr L)
    {
        LuaMethod[] regs = new LuaMethod[]
        {
            new LuaMethod("UnLoadModel", UnLoadModel),
            new LuaMethod("SetModel", SetModel),
            new LuaMethod("SetColor", SetColor),
            new LuaMethod("SetLookAtParam", SetLookAtParam),
            new LuaMethod("SetCameraSize", SetCameraSize),
            new LuaMethod("SetCameraDist", SetCameraDist),
            //new LuaMethod("SetStageZ", SetStageZ),
            //new LuaMethod("SetLookAtPosY", SetLookAtPosY),
            new LuaMethod("SetCameraAngle",SetCameraAngle),
            //new LuaMethod("ShowGroundShadow",ShowGroundShadow),
            new LuaMethod("FixModelAxisX",FixModelAxisX),
            new LuaMethod("SetGroundOffset",SetGroundOffset),
            new LuaMethod("ShowGround",ShowGround),
            new LuaMethod("SetCameraType",SetCameraType),
            new LuaMethod("SetCameraFarClip",SetCameraFarClip),
            new LuaMethod("AlignSystemWithModelForward", AlignSystemWithModelForward),
            new LuaMethod("Equals", Equals),

            new LuaMethod("__eq", Lua_Eq),
        };

        LuaField[] fields = new LuaField[]
        {
            new LuaField("HasModel", get_HasModel, null),
        };

        LuaScriptMgr.RegisterLib(L, "GImageModel", typeof(GImageModel), regs, fields, typeof(UnityEngine.UI.RawImage));
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    static int get_HasModel(IntPtr L)
    {
        object o = LuaScriptMgr.GetLuaObject(L, 1);
        var im = (GImageModel)o;

        if (im == null)
        {
            LuaTypes types = LuaDLL.lua_type(L, 1);

            if (types == LuaTypes.LUA_TTABLE)
            {
                LuaDLL.luaL_error(L, "unknown member name enabled");
            }
            else
            {
                LuaDLL.luaL_error(L, "attempt to index enabled on a nil value");
            }
        }

        LuaScriptMgr.Push(L, im.HasModel);
        return 1;
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    static int UnLoadModel(IntPtr L)
    {
        LuaScriptMgr.CheckArgsCount(L, 1);
        GImageModel obj = (GImageModel)LuaScriptMgr.GetUnityObjectSelf(L, 1, "GImageModel");
        obj.UnLoadModel();

        return 0;
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    static int SetModel(IntPtr L)
    {
        LuaScriptMgr.CheckArgsCount(L, 2);
        GImageModel obj = (GImageModel)LuaScriptMgr.GetUnityObjectSelf(L, 1, "GImageModel");
        GameObject arg0 = (GameObject)LuaScriptMgr.GetUnityObject(L, 2, typeof(GameObject));
        obj.SetModel(arg0);
        return 0;
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    static int SetColor(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        if (count == 4 && LuaScriptMgr.CheckTypes(L, 1, typeof(GImageModel), typeof(float), typeof(float), typeof(float)))
        {
            GImageModel obj = (GImageModel)LuaScriptMgr.GetUnityObjectSelf(L, 1, "SetColor");
            float arg0 = (float)LuaScriptMgr.GetNumber(L, 2);
            float arg1 = (float)LuaScriptMgr.GetNumber(L, 3);
            float arg2 = (float)LuaScriptMgr.GetNumber(L, 4);
            obj.SetColor(arg0, arg1, arg2);
        }
        else
        {
            Common.HobaDebuger.LogError("SetColor param count != 4");
        }
        return 0;
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    static int SetLookAtParam(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        if (count == 8 && LuaScriptMgr.CheckTypes(L, 1, typeof(GImageModel), typeof(float), typeof(float), typeof(float), typeof(float), typeof(float), typeof(float), typeof(float)))
        {
            GImageModel obj = (GImageModel)LuaScriptMgr.GetUnityObjectSelf(L, 1, "GImageModel");
            float arg0 = (float)LuaScriptMgr.GetNumber(L, 2);
            float arg1 = (float)LuaScriptMgr.GetNumber(L, 3);
            float arg2 = (float)LuaScriptMgr.GetNumber(L, 4);
            float arg3 = (float)LuaScriptMgr.GetNumber(L, 5);
            float arg4 = (float)LuaScriptMgr.GetNumber(L, 6);
            float arg5 = (float)LuaScriptMgr.GetNumber(L, 7);
            float arg6 = (float)LuaScriptMgr.GetNumber(L, 8);
            obj.SetCameraSize(arg0);
            obj.SetLookAtPos(arg1, arg2, arg3);
            obj.SetLookAtRot(arg4, arg5, arg6);
        }
        //else if (count == 3 && LuaScriptMgr.CheckTypes(L, 1, typeof(GImageModel), typeof(float), typeof(float)))
        //{
        //    GImageModel obj = (GImageModel)LuaScriptMgr.GetUnityObjectSelf(L, 1, "GImageModel");
        //    float arg0 = (float)LuaScriptMgr.GetNumber(L, 2);
        //    float arg1 = (float)LuaScriptMgr.GetNumber(L, 3);
        //    obj.SetLookAtParam(arg0, arg1);
        //}
        else
        {
            Common.HobaDebuger.LogError("SetLookAtParam param count != 8");
        }

        return 0;
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    static int SetLookAtRot(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        if (count == 4 && LuaScriptMgr.CheckTypes(L, 1, typeof(GImageModel), typeof(float), typeof(float), typeof(float)))
        {
            GImageModel obj = (GImageModel)LuaScriptMgr.GetUnityObjectSelf(L, 1, "GImageModel");
            float arg0 = (float)LuaScriptMgr.GetNumber(L, 2);
            float arg1 = (float)LuaScriptMgr.GetNumber(L, 3);
            float arg2 = (float)LuaScriptMgr.GetNumber(L, 4);
            obj.SetLookAtRot(arg0, arg1, arg2);
        }
        else
        {
            Common.HobaDebuger.LogError("SetLookAtRot param count != 4");
        }

        return 0;
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    static int SetCameraDist(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        if (count == 2 && LuaScriptMgr.CheckTypes(L, 1, typeof(GImageModel), typeof(float)))
        {
            GImageModel obj = (GImageModel)LuaScriptMgr.GetUnityObjectSelf(L, 1, "GImageModel");
            float arg0 = (float)LuaScriptMgr.GetNumber(L, 2);
            obj.SetCameraDist(arg0);
        }
        return 0;
    }

    //[MonoPInvokeCallback(typeof(LuaCSFunction))]
    //static int SetStageZ(IntPtr L)
    //{
    //    int count = LuaDLL.lua_gettop(L);
    //    if (count == 2 && LuaScriptMgr.CheckTypes(L, 1, typeof(GImageModel), typeof(float)))
    //    {
    //        GImageModel obj = (GImageModel)LuaScriptMgr.GetUnityObjectSelf(L, 1, "SetModelDist");
    //        float arg0 = (float)LuaScriptMgr.GetNumber(L, 2);
    //        obj.SetStageZ(arg0);
    //    }
    //    return 0;
    //}

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    static int SetCameraSize(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        if (count == 2 && LuaScriptMgr.CheckTypes(L, 1, typeof(GImageModel), typeof(float)))
        {
            GImageModel obj = (GImageModel)LuaScriptMgr.GetUnityObjectSelf(L, 1, "GImageModel");
            float arg0 = (float)LuaScriptMgr.GetNumber(L, 2);
            obj.SetCameraSize(arg0);
        }
        return 0;
    }

    //[MonoPInvokeCallback(typeof(LuaCSFunction))]
    //static int SetLookAtPosY(IntPtr L)
    //{
    //    int count = LuaDLL.lua_gettop(L);
    //    if (count == 2 && LuaScriptMgr.CheckTypes(L, 1, typeof(GImageModel), typeof(float)))
    //    {
    //        GImageModel obj = (GImageModel)LuaScriptMgr.GetUnityObjectSelf(L, 1, "GImageModel");
    //        float arg0 = (float)LuaScriptMgr.GetNumber(L, 2);
    //        obj.SetLookAtPosXY(arg0);
    //    }
    //    return 0;
    //}

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    static int SetCameraAngle(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        if (count == 4 && LuaScriptMgr.CheckTypes(L, 1, typeof(GImageModel), typeof(float), typeof(float), typeof(float)))
        {
            GImageModel obj = (GImageModel)LuaScriptMgr.GetUnityObjectSelf(L, 1, "GImageModel");
            float arg0 = (float)LuaScriptMgr.GetNumber(L, 2);
            float arg1 = (float)LuaScriptMgr.GetNumber(L, 3);
            float arg2 = (float)LuaScriptMgr.GetNumber(L, 4);
            obj.SetCameraAngle(arg0, arg1, arg2);
        }
        return 0;
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    static int ShowGround(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        if (count == 2 && LuaScriptMgr.CheckTypes(L, 1, typeof(GImageModel), typeof(bool)))
        {
            GImageModel obj = (GImageModel)LuaScriptMgr.GetUnityObjectSelf(L, 1, "GImageModel");
            bool arg0 = (bool)LuaScriptMgr.GetBoolean(L, 2);
            obj.ShowGround(arg0);
        }
        return 0;
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    static int FixModelAxisX(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        if (count == 2 && LuaScriptMgr.CheckTypes(L, 1, typeof(GImageModel), typeof(float)))
        {
            GImageModel obj = (GImageModel)LuaScriptMgr.GetUnityObjectSelf(L, 1, "GImageModel");
            float arg0 = (float)LuaScriptMgr.GetNumber(L, 2);
            obj.FixModelAxisX(arg0);
        }
        return 0;
    }


    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    static int SetGroundOffset(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        if (count == 3 && LuaScriptMgr.CheckTypes(L, 1, typeof(GImageModel), typeof(float), typeof(float)))
        {
            GImageModel obj = (GImageModel)LuaScriptMgr.GetUnityObjectSelf(L, 1, "GImageModel");
            float arg0 = (float)LuaScriptMgr.GetNumber(L, 2);
            float arg1 = (float)LuaScriptMgr.GetNumber(L, 3);
            obj.SetGroundOffset(arg0, arg1);
        }
        return 0;
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    static int SetCameraType(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        if (count == 2 && LuaScriptMgr.CheckTypes(L, 1, typeof(GImageModel), typeof(bool)))
        {
            GImageModel obj = (GImageModel)LuaScriptMgr.GetUnityObjectSelf(L, 1, "GImageModel");
            bool arg0 = (bool)LuaScriptMgr.GetBoolean(L, 2);
            obj.SetCameraType(arg0);
        }
        return 0;
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    static int SetCameraFarClip(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        if (count == 2 && LuaScriptMgr.CheckTypes(L, 1, typeof(GImageModel), typeof(float)))
        {
            GImageModel obj = (GImageModel)LuaScriptMgr.GetUnityObjectSelf(L, 1, "GImageModel");
            float arg0 = (float)LuaScriptMgr.GetNumber(L, 2);
            obj.SetCameraFarClip(arg0);
        }
        return 0;
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    static int AlignSystemWithModelForward(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        if (count == 2 && LuaScriptMgr.CheckTypes(L, 1, typeof(GImageModel), typeof(Vector3)))
        {
            GImageModel obj = (GImageModel)LuaScriptMgr.GetUnityObjectSelf(L, 1, "GImageModel");
            //Vector3 arg0 = (Vector3)LuaScriptMgr.GetNumber(L, 2);
            Vector3 arg0 = LuaScriptMgr.GetVector3(L, 2);
            obj.AlignSystemWithModelForward(arg0);
        }
        return 0;
    }

    //[MonoPInvokeCallback(typeof(LuaCSFunction))]
    //static int AlignSystemWithAnother(IntPtr L)
    //{
    //    LuaScriptMgr.CheckArgsCount(L, 2);
    //    GImageModel obj = (GImageModel)LuaScriptMgr.GetUnityObjectSelf(L, 1, "GImageModel");
    //    //Vector3 arg0 = (Vector3)LuaScriptMgr.GetNumber(L, 2);
    //    GImageModel arg0 = LuaScriptMgr.GetUnityObject(L, 2, typeof(GImageModel)) as GImageModel;
    //    obj.AlignSystemWithAnother(arg0);
    //    return 0;
    //}

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    static int Lua_Eq(IntPtr L)
    {
        LuaScriptMgr.CheckArgsCount(L, 2);
        Object arg0 = LuaScriptMgr.GetLuaObject(L, 1) as Object;
        Object arg1 = LuaScriptMgr.GetLuaObject(L, 2) as Object;
        bool o = arg0 == arg1;
        LuaScriptMgr.Push(L, o);
        return 1;
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    static int Equals(IntPtr L)
    {
        LuaScriptMgr.CheckArgsCount(L, 2);
        Object obj = LuaScriptMgr.GetVarObject(L, 1) as Object;
        object arg0 = LuaScriptMgr.GetVarObject(L, 2);
        bool o = obj != null ? obj.Equals(arg0) : arg0 == null;
        LuaScriptMgr.Push(L, o);
        return 1;
    }
}

