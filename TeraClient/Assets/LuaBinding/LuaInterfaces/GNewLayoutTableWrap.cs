using System;
using UnityEngine;
using LuaInterface;
using Object = UnityEngine.Object;

public class GNewLayoutTableWrap
{
	public static void Register(IntPtr L)
	{
		LuaMethod[] regs = new LuaMethod[]
		{
			new LuaMethod("AddItem", AddItem),
			new LuaMethod("RemoveItem", RemoveItem),
			new LuaMethod("SelectItem", SelectItem),
			new LuaMethod("RefreshItem", RefreshItem),
			new LuaMethod("SetItemCount", SetItemCount),
			new LuaMethod("ScrollToStep", ScrollToStep),
			new LuaMethod("__eq", Lua_Eq),
		};

		LuaField[] fields = new LuaField[]
		{
			new LuaField("poolNode", get_poolNode, set_poolNode),
			new LuaField("MainSelected", get_MainSelected, null),
		};

		LuaScriptMgr.RegisterLib(L, "GNewLayoutTable", typeof(GNewLayoutTable), regs, fields, typeof(GNewTableBase));
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int get_poolNode(IntPtr L)
	{
		object o = LuaScriptMgr.GetLuaObject(L, 1);
		GNewLayoutTable obj = (GNewLayoutTable)o;

		if (obj == null)
		{
			LuaTypes types = LuaDLL.lua_type(L, 1);

			if (types == LuaTypes.LUA_TTABLE)
			{
				LuaDLL.luaL_error(L, "unknown member name poolNode");
			}
			else
			{
				LuaDLL.luaL_error(L, "attempt to index poolNode on a nil value");
			}
		}

		LuaScriptMgr.Push(L, obj.poolNode);
		return 1;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int get_MainSelected(IntPtr L)
	{
		object o = LuaScriptMgr.GetLuaObject(L, 1);
		GNewLayoutTable obj = (GNewLayoutTable)o;

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
	static int set_poolNode(IntPtr L)
	{
		object o = LuaScriptMgr.GetLuaObject(L, 1);
		GNewLayoutTable obj = (GNewLayoutTable)o;

		if (obj == null)
		{
			LuaTypes types = LuaDLL.lua_type(L, 1);

			if (types == LuaTypes.LUA_TTABLE)
			{
				LuaDLL.luaL_error(L, "unknown member name poolNode");
			}
			else
			{
				LuaDLL.luaL_error(L, "attempt to index poolNode on a nil value");
			}
		}

		obj.poolNode = (RectTransform)LuaScriptMgr.GetUnityObject(L, 3, typeof(RectTransform));
		return 0;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int AddItem(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 2);
		GNewLayoutTable obj = (GNewLayoutTable)LuaScriptMgr.GetUnityObjectSelf(L, 1, "GNewLayoutTable");
		int arg0 = (int)LuaScriptMgr.GetNumber(L, 2);
		obj.AddItem(arg0);
		return 0;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int RemoveItem(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 2);
		GNewLayoutTable obj = (GNewLayoutTable)LuaScriptMgr.GetUnityObjectSelf(L, 1, "GNewLayoutTable");
		int arg0 = (int)LuaScriptMgr.GetNumber(L, 2);
		obj.RemoveItem(arg0);
		return 0;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int SelectItem(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 2);
		GNewLayoutTable obj = (GNewLayoutTable)LuaScriptMgr.GetUnityObjectSelf(L, 1, "GNewLayoutTable");
		int arg0 = (int)LuaScriptMgr.GetNumber(L, 2);
		obj.SelectItem(arg0);
		return 0;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int RefreshItem(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 2);
		GNewLayoutTable obj = (GNewLayoutTable)LuaScriptMgr.GetUnityObjectSelf(L, 1, "GNewLayoutTable");
		int arg0 = (int)LuaScriptMgr.GetNumber(L, 2);
		obj.RefreshItem(arg0);
		return 0;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int SetItemCount(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 2);
		GNewLayoutTable obj = (GNewLayoutTable)LuaScriptMgr.GetUnityObjectSelf(L, 1, "GNewLayoutTable");
		int arg0 = (int)LuaScriptMgr.GetNumber(L, 2);
		obj.SetItemCount(arg0);
		return 0;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int ScrollToStep(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 2);
		GNewLayoutTable obj = (GNewLayoutTable)LuaScriptMgr.GetUnityObjectSelf(L, 1, "GNewLayoutTable");
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

