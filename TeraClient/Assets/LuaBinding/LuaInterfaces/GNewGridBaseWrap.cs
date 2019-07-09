using System;
using UnityEngine;
using LuaInterface;
using Object = UnityEngine.Object;

public class GNewGridBaseWrap
{
	public static void Register(IntPtr L)
	{
		LuaMethod[] regs = new LuaMethod[]
		{
			new LuaMethod("Repaint", Repaint),
			new LuaMethod("SetPageSize", SetPageSize),
			new LuaMethod("__eq", Lua_Eq),
		};

		LuaField[] fields = new LuaField[]
		{
			new LuaField("ItemCount", get_ItemCount, null),
			new LuaField("PageWidth", get_PageWidth, set_PageWidth),
			new LuaField("PageHeight", get_PageHeight, set_PageHeight),
		};

		LuaScriptMgr.RegisterLib(L, "GNewGridBase", typeof(GNewGridBase), regs, fields, typeof(GNewUIBase));
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int get_ItemCount(IntPtr L)
	{
		object o = LuaScriptMgr.GetLuaObject(L, 1);
		GNewGridBase obj = (GNewGridBase)o;

		if (obj == null)
		{
			LuaTypes types = LuaDLL.lua_type(L, 1);

			if (types == LuaTypes.LUA_TTABLE)
			{
				LuaDLL.luaL_error(L, "unknown member name ItemCount");
			}
			else
			{
				LuaDLL.luaL_error(L, "attempt to index ItemCount on a nil value");
			}
		}

		LuaScriptMgr.Push(L, obj.ItemCount);
		return 1;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int get_PageWidth(IntPtr L)
	{
		object o = LuaScriptMgr.GetLuaObject(L, 1);
		GNewGridBase obj = (GNewGridBase)o;

		if (obj == null)
		{
			LuaTypes types = LuaDLL.lua_type(L, 1);

			if (types == LuaTypes.LUA_TTABLE)
			{
				LuaDLL.luaL_error(L, "unknown member name PageWidth");
			}
			else
			{
				LuaDLL.luaL_error(L, "attempt to index PageWidth on a nil value");
			}
		}

		LuaScriptMgr.Push(L, obj.PageWidth);
		return 1;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int get_PageHeight(IntPtr L)
	{
		object o = LuaScriptMgr.GetLuaObject(L, 1);
		GNewGridBase obj = (GNewGridBase)o;

		if (obj == null)
		{
			LuaTypes types = LuaDLL.lua_type(L, 1);

			if (types == LuaTypes.LUA_TTABLE)
			{
				LuaDLL.luaL_error(L, "unknown member name PageHeight");
			}
			else
			{
				LuaDLL.luaL_error(L, "attempt to index PageHeight on a nil value");
			}
		}

		LuaScriptMgr.Push(L, obj.PageHeight);
		return 1;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int set_PageWidth(IntPtr L)
	{
		object o = LuaScriptMgr.GetLuaObject(L, 1);
		GNewGridBase obj = (GNewGridBase)o;

		if (obj == null)
		{
			LuaTypes types = LuaDLL.lua_type(L, 1);

			if (types == LuaTypes.LUA_TTABLE)
			{
				LuaDLL.luaL_error(L, "unknown member name PageWidth");
			}
			else
			{
				LuaDLL.luaL_error(L, "attempt to index PageWidth on a nil value");
			}
		}

		obj.PageWidth = (int)LuaScriptMgr.GetNumber(L, 3);
		return 0;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int set_PageHeight(IntPtr L)
	{
		object o = LuaScriptMgr.GetLuaObject(L, 1);
		GNewGridBase obj = (GNewGridBase)o;

		if (obj == null)
		{
			LuaTypes types = LuaDLL.lua_type(L, 1);

			if (types == LuaTypes.LUA_TTABLE)
			{
				LuaDLL.luaL_error(L, "unknown member name PageHeight");
			}
			else
			{
				LuaDLL.luaL_error(L, "attempt to index PageHeight on a nil value");
			}
		}

		obj.PageHeight = (int)LuaScriptMgr.GetNumber(L, 3);
		return 0;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int Repaint(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 1);
		GNewGridBase obj = (GNewGridBase)LuaScriptMgr.GetUnityObjectSelf(L, 1, "GNewGridBase");
		obj.Repaint();
		return 0;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int SetPageSize(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 3);
		GNewGridBase obj = (GNewGridBase)LuaScriptMgr.GetUnityObjectSelf(L, 1, "GNewGridBase");
		int arg0 = (int)LuaScriptMgr.GetNumber(L, 2);
		int arg1 = (int)LuaScriptMgr.GetNumber(L, 3);
		obj.SetPageSize(arg0,arg1);
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

