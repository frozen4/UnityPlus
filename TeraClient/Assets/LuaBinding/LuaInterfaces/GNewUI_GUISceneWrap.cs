using System;
using UnityEngine;
using LuaInterface;
using Object = UnityEngine.Object;

public class GNewUI_GUISceneWrap
{
	public static void Register(IntPtr L)
	{
		LuaMethod[] regs = new LuaMethod[]
		{
			new LuaMethod("Init", Init),
			new LuaMethod("SetLuaHandler", SetLuaHandler),
			new LuaMethod("PlaySequence", PlaySequence),
			new LuaMethod("SetVisible", SetVisible),
			new LuaMethod("SetCameraDepth", SetCameraDepth),
			new LuaMethod("PossessImage", PossessImage),
			new LuaMethod("__eq", Lua_Eq),
		};

		LuaField[] fields = new LuaField[]
		{
			new LuaField("EnvEffectID", get_EnvEffectID, set_EnvEffectID),
		};

		LuaScriptMgr.RegisterLib(L, "GNewUI.GUIScene", typeof(GNewUI.GUIScene), regs, fields, typeof(GNewUIBase));
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int get_EnvEffectID(IntPtr L)
	{
		object o = LuaScriptMgr.GetLuaObject(L, 1);
		GNewUI.GUIScene obj = (GNewUI.GUIScene)o;

		if (obj == null)
		{
			LuaTypes types = LuaDLL.lua_type(L, 1);

			if (types == LuaTypes.LUA_TTABLE)
			{
				LuaDLL.luaL_error(L, "unknown member name EnvEffectID");
			}
			else
			{
				LuaDLL.luaL_error(L, "attempt to index EnvEffectID on a nil value");
			}
		}

		LuaScriptMgr.Push(L, obj.EnvEffectID);
		return 1;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int set_EnvEffectID(IntPtr L)
	{
		object o = LuaScriptMgr.GetLuaObject(L, 1);
		GNewUI.GUIScene obj = (GNewUI.GUIScene)o;

		if (obj == null)
		{
			LuaTypes types = LuaDLL.lua_type(L, 1);

			if (types == LuaTypes.LUA_TTABLE)
			{
				LuaDLL.luaL_error(L, "unknown member name EnvEffectID");
			}
			else
			{
				LuaDLL.luaL_error(L, "attempt to index EnvEffectID on a nil value");
			}
		}

		obj.EnvEffectID = (int)LuaScriptMgr.GetNumber(L, 3);
		return 0;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int Init(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 1);
		GNewUI.GUIScene obj = (GNewUI.GUIScene)LuaScriptMgr.GetUnityObjectSelf(L, 1, "GNewUI.GUIScene");
		obj.Init();
		return 0;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int SetLuaHandler(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 4);
		GNewUI.GUIScene obj = (GNewUI.GUIScene)LuaScriptMgr.GetUnityObjectSelf(L, 1, "GNewUI.GUIScene");
		LuaTable arg0 = LuaScriptMgr.GetLuaTable(L, 2);
		LuaFunction arg1 = LuaScriptMgr.GetLuaFunction(L, 3);
		LuaFunction arg2 = LuaScriptMgr.GetLuaFunction(L, 4);
		obj.SetLuaHandler(arg0,arg1,arg2);
		return 0;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int PlaySequence(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 3);
		GNewUI.GUIScene obj = (GNewUI.GUIScene)LuaScriptMgr.GetUnityObjectSelf(L, 1, "GNewUI.GUIScene");
		int arg0 = (int)LuaScriptMgr.GetNumber(L, 2);
		string arg1 = LuaScriptMgr.GetLuaString(L, 3);
		float o = obj.PlaySequence(arg0,arg1);
		LuaScriptMgr.Push(L, o);
		return 1;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int SetVisible(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 2);
		GNewUI.GUIScene obj = (GNewUI.GUIScene)LuaScriptMgr.GetUnityObjectSelf(L, 1, "GNewUI.GUIScene");
		bool arg0 = LuaScriptMgr.GetBoolean(L, 2);
		obj.SetVisible(arg0);
		return 0;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int SetCameraDepth(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 2);
		GNewUI.GUIScene obj = (GNewUI.GUIScene)LuaScriptMgr.GetUnityObjectSelf(L, 1, "GNewUI.GUIScene");
		int arg0 = (int)LuaScriptMgr.GetNumber(L, 2);
		obj.SetCameraDepth(arg0);
		return 0;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int PossessImage(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 3);
		GNewUI.GUIScene obj = (GNewUI.GUIScene)LuaScriptMgr.GetUnityObjectSelf(L, 1, "GNewUI.GUIScene");
		GameObject arg0 = (GameObject)LuaScriptMgr.GetUnityObject(L, 2, typeof(GameObject));
		float arg1 = (float)LuaScriptMgr.GetNumber(L, 3);
		obj.PossessImage(arg0,arg1);
		return 0;
	}

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
}

