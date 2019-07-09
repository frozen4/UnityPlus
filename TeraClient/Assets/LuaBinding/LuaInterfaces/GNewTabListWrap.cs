using System;
using UnityEngine;
using LuaInterface;
using Object = UnityEngine.Object;

public class GNewTabListWrap
{
	public static void Register(IntPtr L)
	{
		LuaMethod[] regs = new LuaMethod[]
		{
			new LuaMethod("SetItemCount", SetItemCount),
			new LuaMethod("OpenTab", OpenTab),
			new LuaMethod("CloseTab", CloseTab),
			new LuaMethod("SelectItem", SelectItem),
			new LuaMethod("PlayEffect", PlayEffect),
			new LuaMethod("SetSelection", SetSelection),
			new LuaMethod("__eq", Lua_Eq),
		};

		LuaField[] fields = new LuaField[]
		{
			new LuaField("MainSelected", get_MainSelected, null),
			new LuaField("SubSelected", get_SubSelected, null),
			new LuaField("LastMainSelected", get_LastMainSelected, null),
			new LuaField("LastSubSelected", get_LastSubSelected, null),
		};

		LuaScriptMgr.RegisterLib(L, "GNewTabList", typeof(GNewTabList), regs, fields, typeof(GNewTableBase));
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int get_MainSelected(IntPtr L)
	{
		object o = LuaScriptMgr.GetLuaObject(L, 1);
		GNewTabList obj = (GNewTabList)o;

		if (obj == null)
		{
			LuaTypes types = LuaDLL.lua_type(L, 1);

			if (types == LuaTypes.LUA_TTABLE)
			{
				LuaDLL.luaL_error(L, "unknown member name MainSelected");
			}
			else
			{
				LuaDLL.luaL_error(L, "attempt to index MainSelected on a nil value");
			}
		}

		LuaScriptMgr.Push(L, obj.MainSelected);
		return 1;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int get_SubSelected(IntPtr L)
	{
		object o = LuaScriptMgr.GetLuaObject(L, 1);
		GNewTabList obj = (GNewTabList)o;

		if (obj == null)
		{
			LuaTypes types = LuaDLL.lua_type(L, 1);

			if (types == LuaTypes.LUA_TTABLE)
			{
				LuaDLL.luaL_error(L, "unknown member name SubSelected");
			}
			else
			{
				LuaDLL.luaL_error(L, "attempt to index SubSelected on a nil value");
			}
		}

		LuaScriptMgr.Push(L, obj.SubSelected);
		return 1;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int get_LastMainSelected(IntPtr L)
	{
		object o = LuaScriptMgr.GetLuaObject(L, 1);
		GNewTabList obj = (GNewTabList)o;

		if (obj == null)
		{
			LuaTypes types = LuaDLL.lua_type(L, 1);

			if (types == LuaTypes.LUA_TTABLE)
			{
				LuaDLL.luaL_error(L, "unknown member name LastMainSelected");
			}
			else
			{
				LuaDLL.luaL_error(L, "attempt to index LastMainSelected on a nil value");
			}
		}

		LuaScriptMgr.Push(L, obj.LastMainSelected);
		return 1;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int get_LastSubSelected(IntPtr L)
	{
		object o = LuaScriptMgr.GetLuaObject(L, 1);
		GNewTabList obj = (GNewTabList)o;

		if (obj == null)
		{
			LuaTypes types = LuaDLL.lua_type(L, 1);

			if (types == LuaTypes.LUA_TTABLE)
			{
				LuaDLL.luaL_error(L, "unknown member name LastSubSelected");
			}
			else
			{
				LuaDLL.luaL_error(L, "attempt to index LastSubSelected on a nil value");
			}
		}

		LuaScriptMgr.Push(L, obj.LastSubSelected);
		return 1;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int SetItemCount(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 2);
		GNewTabList obj = (GNewTabList)LuaScriptMgr.GetUnityObjectSelf(L, 1, "GNewTabList");
		int arg0 = (int)LuaScriptMgr.GetNumber(L, 2);
		obj.SetItemCount(arg0);
		return 0;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int OpenTab(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 2);
		GNewTabList obj = (GNewTabList)LuaScriptMgr.GetUnityObjectSelf(L, 1, "GNewTabList");
		int arg0 = (int)LuaScriptMgr.GetNumber(L, 2);
		obj.OpenTab(arg0);
		return 0;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int CloseTab(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 1);
		GNewTabList obj = (GNewTabList)LuaScriptMgr.GetUnityObjectSelf(L, 1, "GNewTabList");
		obj.CloseTab();
		return 0;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int SelectItem(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 3);
		GNewTabList obj = (GNewTabList)LuaScriptMgr.GetUnityObjectSelf(L, 1, "GNewTabList");
		int arg0 = (int)LuaScriptMgr.GetNumber(L, 2);
		int arg1 = (int)LuaScriptMgr.GetNumber(L, 3);
		obj.SelectItem(arg0,arg1);
		return 0;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int PlayEffect(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 1);
		GNewTabList obj = (GNewTabList)LuaScriptMgr.GetUnityObjectSelf(L, 1, "GNewTabList");
		obj.PlayEffect();
		return 0;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int SetSelection(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 3);
		GNewTabList obj = (GNewTabList)LuaScriptMgr.GetUnityObjectSelf(L, 1, "GNewTabList");
		int arg0 = (int)LuaScriptMgr.GetNumber(L, 2);
		int arg1 = (int)LuaScriptMgr.GetNumber(L, 3);
		obj.SetSelection(arg0,arg1);
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

