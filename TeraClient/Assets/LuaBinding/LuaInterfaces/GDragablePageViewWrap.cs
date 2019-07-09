using System;
using UnityEngine;
using LuaInterface;
using Object = UnityEngine.Object;

public class GDragablePageViewWrap
{
	public static void Register(IntPtr L)
	{
		LuaMethod[] regs = new LuaMethod[]
		{
			new LuaMethod("SetPageItemCount", SetPageItemCount),
			new LuaMethod("ChangePageIndex", ChangePageIndex),
			new LuaMethod("ChangePageIndexRightNow", ChangePageIndexRightNow),
			new LuaMethod("SetTimeInterval", SetTimeInterval),
			new LuaMethod("__eq", Lua_Eq),
		};

		LuaField[] fields = new LuaField[]
		{
		};

		LuaScriptMgr.RegisterLib(L, "GDragablePageView", typeof(GDragablePageView), regs, fields, typeof(GNewUIBase));
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int SetPageItemCount(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 2);
		GDragablePageView obj = (GDragablePageView)LuaScriptMgr.GetUnityObjectSelf(L, 1, "GDragablePageView");
		int arg0 = (int)LuaScriptMgr.GetNumber(L, 2);
		obj.SetPageItemCount(arg0);
		return 0;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int ChangePageIndex(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 2);
		GDragablePageView obj = (GDragablePageView)LuaScriptMgr.GetUnityObjectSelf(L, 1, "GDragablePageView");
		int arg0 = (int)LuaScriptMgr.GetNumber(L, 2);
		obj.ChangePageIndex(arg0);
		return 0;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int ChangePageIndexRightNow(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 2);
		GDragablePageView obj = (GDragablePageView)LuaScriptMgr.GetUnityObjectSelf(L, 1, "GDragablePageView");
		int arg0 = (int)LuaScriptMgr.GetNumber(L, 2);
		obj.ChangePageIndexRightNow(arg0);
		return 0;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int SetTimeInterval(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 2);
		GDragablePageView obj = (GDragablePageView)LuaScriptMgr.GetUnityObjectSelf(L, 1, "GDragablePageView");
		float arg0 = (float)LuaScriptMgr.GetNumber(L, 2);
		obj.SetTimeInterval(arg0);
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

