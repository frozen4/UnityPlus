using System;
using UnityEngine;
using LuaInterface;
using Object = UnityEngine.Object;

public class GNewListWrap
{
	public static void Register(IntPtr L)
	{
		LuaMethod[] regs = new LuaMethod[]
		{
			new LuaMethod("SetItemCount", SetItemCount),
			new LuaMethod("RefreshItem", RefreshItem),
			new LuaMethod("AddItem", AddItem),
			new LuaMethod("RemoveItem", RemoveItem),
			new LuaMethod("__eq", Lua_Eq),
		};

		LuaField[] fields = new LuaField[]
		{
		};

		LuaScriptMgr.RegisterLib(L, "GNewList", typeof(GNewList), regs, fields, typeof(GNewListBase));
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int SetItemCount(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 2);
		GNewList obj = (GNewList)LuaScriptMgr.GetUnityObjectSelf(L, 1, "GNewList");
		int arg0 = (int)LuaScriptMgr.GetNumber(L, 2);
		obj.SetItemCount(arg0);
		return 0;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int RefreshItem(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 2);
		GNewList obj = (GNewList)LuaScriptMgr.GetUnityObjectSelf(L, 1, "GNewList");
		int arg0 = (int)LuaScriptMgr.GetNumber(L, 2);
		obj.RefreshItem(arg0);
		return 0;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int AddItem(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 3);
		GNewList obj = (GNewList)LuaScriptMgr.GetUnityObjectSelf(L, 1, "GNewList");
		int arg0 = (int)LuaScriptMgr.GetNumber(L, 2);
		int arg1 = (int)LuaScriptMgr.GetNumber(L, 3);
		obj.AddItem(arg0,arg1);
		return 0;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int RemoveItem(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 3);
		GNewList obj = (GNewList)LuaScriptMgr.GetUnityObjectSelf(L, 1, "GNewList");
		int arg0 = (int)LuaScriptMgr.GetNumber(L, 2);
		int arg1 = (int)LuaScriptMgr.GetNumber(L, 3);
		obj.RemoveItem(arg0,arg1);
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

