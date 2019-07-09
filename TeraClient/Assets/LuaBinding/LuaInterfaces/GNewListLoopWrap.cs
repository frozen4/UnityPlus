using System;
using UnityEngine;
using LuaInterface;
using Object = UnityEngine.Object;

public class GNewListLoopWrap
{
	public static void Register(IntPtr L)
	{
		LuaMethod[] regs = new LuaMethod[]
		{
			new LuaMethod("SetItemCount", SetItemCount),
			new LuaMethod("Repaint", Repaint),
			new LuaMethod("RefreshItem", RefreshItem),
			new LuaMethod("AddItem", AddItem),
			new LuaMethod("RemoveItem", RemoveItem),
			new LuaMethod("ScrollToStep", ScrollToStep),
			new LuaMethod("IsListItemVisible", IsListItemVisible),
			new LuaMethod("__eq", Lua_Eq),
		};

		LuaField[] fields = new LuaField[]
		{
			new LuaField("PreOutLook", get_PreOutLook, set_PreOutLook),
		};

		LuaScriptMgr.RegisterLib(L, "GNewListLoop", typeof(GNewListLoop), regs, fields, typeof(GNewListBase));
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int get_PreOutLook(IntPtr L)
	{
		object o = LuaScriptMgr.GetLuaObject(L, 1);
		GNewListLoop obj = (GNewListLoop)o;

		if (obj == null)
		{
			LuaTypes types = LuaDLL.lua_type(L, 1);

			if (types == LuaTypes.LUA_TTABLE)
			{
				LuaDLL.luaL_error(L, "unknown member name PreOutLook");
			}
			else
			{
				LuaDLL.luaL_error(L, "attempt to index PreOutLook on a nil value");
			}
		}

		LuaScriptMgr.Push(L, obj.PreOutLook);
		return 1;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int set_PreOutLook(IntPtr L)
	{
		object o = LuaScriptMgr.GetLuaObject(L, 1);
		GNewListLoop obj = (GNewListLoop)o;

		if (obj == null)
		{
			LuaTypes types = LuaDLL.lua_type(L, 1);

			if (types == LuaTypes.LUA_TTABLE)
			{
				LuaDLL.luaL_error(L, "unknown member name PreOutLook");
			}
			else
			{
				LuaDLL.luaL_error(L, "attempt to index PreOutLook on a nil value");
			}
		}

		obj.PreOutLook = (float)LuaScriptMgr.GetNumber(L, 3);
		return 0;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int SetItemCount(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 2);
		GNewListLoop obj = (GNewListLoop)LuaScriptMgr.GetUnityObjectSelf(L, 1, "GNewListLoop");
		int arg0 = (int)LuaScriptMgr.GetNumber(L, 2);
		obj.SetItemCount(arg0);
		return 0;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int Repaint(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 1);
		GNewListLoop obj = (GNewListLoop)LuaScriptMgr.GetUnityObjectSelf(L, 1, "GNewListLoop");
		obj.Repaint();
		return 0;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int RefreshItem(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 2);
		GNewListLoop obj = (GNewListLoop)LuaScriptMgr.GetUnityObjectSelf(L, 1, "GNewListLoop");
		int arg0 = (int)LuaScriptMgr.GetNumber(L, 2);
		obj.RefreshItem(arg0);
		return 0;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int AddItem(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 3);
		GNewListLoop obj = (GNewListLoop)LuaScriptMgr.GetUnityObjectSelf(L, 1, "GNewListLoop");
		int arg0 = (int)LuaScriptMgr.GetNumber(L, 2);
		int arg1 = (int)LuaScriptMgr.GetNumber(L, 3);
		obj.AddItem(arg0,arg1);
		return 0;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int RemoveItem(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 3);
		GNewListLoop obj = (GNewListLoop)LuaScriptMgr.GetUnityObjectSelf(L, 1, "GNewListLoop");
		int arg0 = (int)LuaScriptMgr.GetNumber(L, 2);
		int arg1 = (int)LuaScriptMgr.GetNumber(L, 3);
		obj.RemoveItem(arg0,arg1);
		return 0;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int ScrollToStep(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 2);
		GNewListLoop obj = (GNewListLoop)LuaScriptMgr.GetUnityObjectSelf(L, 1, "GNewListLoop");
		int arg0 = (int)LuaScriptMgr.GetNumber(L, 2);
		obj.ScrollToStep(arg0);
		return 0;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int IsListItemVisible(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 3);
		GNewListLoop obj = (GNewListLoop)LuaScriptMgr.GetUnityObjectSelf(L, 1, "GNewListLoop");
		int arg0 = (int)LuaScriptMgr.GetNumber(L, 2);
		float arg1 = (float)LuaScriptMgr.GetNumber(L, 3);
		bool o = obj.IsListItemVisible(arg0,arg1);
		LuaScriptMgr.Push(L, o);
		return 1;
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

