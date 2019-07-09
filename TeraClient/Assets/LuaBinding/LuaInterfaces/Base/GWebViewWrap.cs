using System;
using UnityEngine;
using LuaInterface;
using Object = UnityEngine.Object;

public class GWebViewWrap
{
	public static void Register(IntPtr L)
	{
		LuaMethod[] regs = new LuaMethod[]
		{
			new LuaMethod("Init", Init),
			new LuaMethod("Load", Load),
			new LuaMethod("LoadHTMLString", LoadHTMLString),
			new LuaMethod("Reload", Reload),
			new LuaMethod("Stop", Stop),
			new LuaMethod("Hide", Hide),
			new LuaMethod("CleanCache", CleanCache),
			new LuaMethod("AddJavaScript", AddJavaScript),
			new LuaMethod("EvaluatingJavaScript", EvaluatingJavaScript),
			new LuaMethod("SetHeaderField", SetHeaderField),
			new LuaMethod("__eq", Lua_Eq),
		};

		LuaField[] fields = new LuaField[]
		{
			new LuaField("IsRunWindows", get_IsRunWindows, null),
			new LuaField("URL", get_URL, set_URL),
		};

		LuaScriptMgr.RegisterLib(L, "GWebView", typeof(GWebView), regs, fields, typeof(MonoBehaviour));
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int get_IsRunWindows(IntPtr L)
	{
		object o = LuaScriptMgr.GetLuaObject(L, 1);
		GWebView obj = (GWebView)o;

		if (obj == null)
		{
			LuaTypes types = LuaDLL.lua_type(L, 1);

			if (types == LuaTypes.LUA_TTABLE)
			{
				LuaDLL.luaL_error(L, "unknown member name IsRunWindows");
			}
			else
			{
				LuaDLL.luaL_error(L, "attempt to index IsRunWindows on a nil value");
			}
		}

		LuaScriptMgr.Push(L, obj.IsRunWindows);
		return 1;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int get_URL(IntPtr L)
	{
		object o = LuaScriptMgr.GetLuaObject(L, 1);
		GWebView obj = (GWebView)o;

		if (obj == null)
		{
			LuaTypes types = LuaDLL.lua_type(L, 1);

			if (types == LuaTypes.LUA_TTABLE)
			{
				LuaDLL.luaL_error(L, "unknown member name URL");
			}
			else
			{
				LuaDLL.luaL_error(L, "attempt to index URL on a nil value");
			}
		}

		LuaScriptMgr.Push(L, obj.URL);
		return 1;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int set_URL(IntPtr L)
	{
		object o = LuaScriptMgr.GetLuaObject(L, 1);
		GWebView obj = (GWebView)o;

		if (obj == null)
		{
			LuaTypes types = LuaDLL.lua_type(L, 1);

			if (types == LuaTypes.LUA_TTABLE)
			{
				LuaDLL.luaL_error(L, "unknown member name URL");
			}
			else
			{
				LuaDLL.luaL_error(L, "attempt to index URL on a nil value");
			}
		}

		obj.URL = LuaScriptMgr.GetString(L, 3);
		return 0;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int Init(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 2);
		GWebView obj = (GWebView)LuaScriptMgr.GetUnityObjectSelf(L, 1, "GWebView");
		GameObject arg0 = (GameObject)LuaScriptMgr.GetUnityObject(L, 2, typeof(GameObject));
		obj.Init(arg0);
		return 0;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int Load(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 2);
		GWebView obj = (GWebView)LuaScriptMgr.GetUnityObjectSelf(L, 1, "GWebView");
		string arg0 = LuaScriptMgr.GetLuaString(L, 2);
		obj.Load(arg0);
		return 0;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int LoadHTMLString(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 3);
		GWebView obj = (GWebView)LuaScriptMgr.GetUnityObjectSelf(L, 1, "GWebView");
		string arg0 = LuaScriptMgr.GetLuaString(L, 2);
		string arg1 = LuaScriptMgr.GetLuaString(L, 3);
		obj.LoadHTMLString(arg0,arg1);
		return 0;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int Reload(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 1);
		GWebView obj = (GWebView)LuaScriptMgr.GetUnityObjectSelf(L, 1, "GWebView");
		obj.Reload();
		return 0;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int Stop(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 1);
		GWebView obj = (GWebView)LuaScriptMgr.GetUnityObjectSelf(L, 1, "GWebView");
		obj.Stop();
		return 0;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int Hide(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 1);
		GWebView obj = (GWebView)LuaScriptMgr.GetUnityObjectSelf(L, 1, "GWebView");
		obj.Hide();
		return 0;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int CleanCache(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 1);
		GWebView obj = (GWebView)LuaScriptMgr.GetUnityObjectSelf(L, 1, "GWebView");
		obj.CleanCache();
		return 0;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int AddJavaScript(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 2);
		GWebView obj = (GWebView)LuaScriptMgr.GetUnityObjectSelf(L, 1, "GWebView");
		string arg0 = LuaScriptMgr.GetLuaString(L, 2);
		obj.AddJavaScript(arg0);
		return 0;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int EvaluatingJavaScript(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 2);
		GWebView obj = (GWebView)LuaScriptMgr.GetUnityObjectSelf(L, 1, "GWebView");
		string arg0 = LuaScriptMgr.GetLuaString(L, 2);
		obj.EvaluatingJavaScript(arg0);
		return 0;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int SetHeaderField(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 3);
		GWebView obj = (GWebView)LuaScriptMgr.GetUnityObjectSelf(L, 1, "GWebView");
		string arg0 = LuaScriptMgr.GetLuaString(L, 2);
		string arg1 = LuaScriptMgr.GetLuaString(L, 3);
		obj.SetHeaderField(arg0,arg1);
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

