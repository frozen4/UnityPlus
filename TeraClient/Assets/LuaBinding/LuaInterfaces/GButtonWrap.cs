using System;
using UnityEngine;
using LuaInterface;
using Object = UnityEngine.Object;

public class GButtonWrap
{
	public static void Register(IntPtr L)
	{
		LuaMethod[] regs = new LuaMethod[]
		{
			new LuaMethod("IsPointerOverThis", IsPointerOverThis),
			new LuaMethod("__eq", Lua_Eq),
		};

		LuaField[] fields = new LuaField[]
		{
		};

		LuaScriptMgr.RegisterLib(L, "GButton", typeof(GButton), regs, fields, typeof(UnityEngine.UI.Selectable));
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int IsPointerOverThis(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 1);
		GButton obj = (GButton)LuaScriptMgr.GetUnityObjectSelf(L, 1, "GButton");
		bool o = obj.IsPointerOverThis();
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

