using System;
using UnityEngine;
using LuaInterface;
using Object = UnityEngine.Object;

public class GNewListBaseWrap
{
	public static void Register(IntPtr L)
	{
		LuaMethod[] regs = new LuaMethod[]
		{
			new LuaMethod("GetItem", GetItem),
			new LuaMethod("SetItemCount", SetItemCount),
			new LuaMethod("SetSelection", SetSelection),
			new LuaMethod("SetManualSelection", SetManualSelection),
			new LuaMethod("EnableScroll", EnableScroll),
			new LuaMethod("RefreshItem", RefreshItem),
			new LuaMethod("AddItem", AddItem),
			new LuaMethod("RemoveItem", RemoveItem),
			new LuaMethod("ScrollToStep", ScrollToStep),
			new LuaMethod("__eq", Lua_Eq),
		};

		LuaField[] fields = new LuaField[]
		{
			new LuaField("SingleSelect", get_SingleSelect, set_SingleSelect),
		};

		LuaScriptMgr.RegisterLib(L, "GNewListBase", typeof(GNewListBase), regs, fields, typeof(GNewGridBase));
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int get_SingleSelect(IntPtr L)
	{
		object o = LuaScriptMgr.GetLuaObject(L, 1);
		GNewListBase obj = (GNewListBase)o;

		if (obj == null)
		{
			LuaTypes types = LuaDLL.lua_type(L, 1);

			if (types == LuaTypes.LUA_TTABLE)
			{
				LuaDLL.luaL_error(L, "unknown member name SingleSelect");
			}
			else
			{
				LuaDLL.luaL_error(L, "attempt to index SingleSelect on a nil value");
			}
		}

		LuaScriptMgr.Push(L, obj.SingleSelect);
		return 1;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int set_SingleSelect(IntPtr L)
	{
		object o = LuaScriptMgr.GetLuaObject(L, 1);
		GNewListBase obj = (GNewListBase)o;

		if (obj == null)
		{
			LuaTypes types = LuaDLL.lua_type(L, 1);

			if (types == LuaTypes.LUA_TTABLE)
			{
				LuaDLL.luaL_error(L, "unknown member name SingleSelect");
			}
			else
			{
				LuaDLL.luaL_error(L, "attempt to index SingleSelect on a nil value");
			}
		}

		obj.SingleSelect = LuaScriptMgr.GetBoolean(L, 3);
		return 0;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int GetItem(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 2);
		GNewListBase obj = (GNewListBase)LuaScriptMgr.GetUnityObjectSelf(L, 1, "GNewListBase");
		int arg0 = (int)LuaScriptMgr.GetNumber(L, 2);
		GameObject o = obj.GetItem(arg0);
		LuaScriptMgr.Push(L, o);
		return 1;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int SetItemCount(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 2);
		GNewListBase obj = (GNewListBase)LuaScriptMgr.GetUnityObjectSelf(L, 1, "GNewListBase");
		int arg0 = (int)LuaScriptMgr.GetNumber(L, 2);
		obj.SetItemCount(arg0);
		return 0;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int SetSelection(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 2);
		GNewListBase obj = (GNewListBase)LuaScriptMgr.GetUnityObjectSelf(L, 1, "GNewListBase");
		int arg0 = (int)LuaScriptMgr.GetNumber(L, 2);
		obj.SetSelection(arg0);
		return 0;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int SetManualSelection(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 3);
		GNewListBase obj = (GNewListBase)LuaScriptMgr.GetUnityObjectSelf(L, 1, "GNewListBase");
		int arg0 = (int)LuaScriptMgr.GetNumber(L, 2);
		bool arg1 = LuaScriptMgr.GetBoolean(L, 3);
		obj.SetManualSelection(arg0,arg1);
		return 0;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int EnableScroll(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 2);
		GNewListBase obj = (GNewListBase)LuaScriptMgr.GetUnityObjectSelf(L, 1, "GNewListBase");
		bool arg0 = LuaScriptMgr.GetBoolean(L, 2);
		obj.EnableScroll(arg0);
		return 0;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int RefreshItem(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 2);
		GNewListBase obj = (GNewListBase)LuaScriptMgr.GetUnityObjectSelf(L, 1, "GNewListBase");
		int arg0 = (int)LuaScriptMgr.GetNumber(L, 2);
		obj.RefreshItem(arg0);
		return 0;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int AddItem(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 3);
		GNewListBase obj = (GNewListBase)LuaScriptMgr.GetUnityObjectSelf(L, 1, "GNewListBase");
		int arg0 = (int)LuaScriptMgr.GetNumber(L, 2);
		int arg1 = (int)LuaScriptMgr.GetNumber(L, 3);
		obj.AddItem(arg0,arg1);
		return 0;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int RemoveItem(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 3);
		GNewListBase obj = (GNewListBase)LuaScriptMgr.GetUnityObjectSelf(L, 1, "GNewListBase");
		int arg0 = (int)LuaScriptMgr.GetNumber(L, 2);
		int arg1 = (int)LuaScriptMgr.GetNumber(L, 3);
		obj.RemoveItem(arg0,arg1);
		return 0;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int ScrollToStep(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 2);
		GNewListBase obj = (GNewListBase)LuaScriptMgr.GetUnityObjectSelf(L, 1, "GNewListBase");
		int arg0 = (int)LuaScriptMgr.GetNumber(L, 2);
		obj.ScrollToStep(arg0);
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

