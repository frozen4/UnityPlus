using System;
using UnityEngine;
using LuaInterface;
using Object = UnityEngine.Object;

public class GScaleScrollWrap
{
	public static void Register(IntPtr L)
	{
		LuaMethod[] regs = new LuaMethod[]
		{
			new LuaMethod("CenterOnPos", CenterOnPos),
			new LuaMethod("__eq", Lua_Eq),
		};

		LuaField[] fields = new LuaField[]
		{
		};

		LuaScriptMgr.RegisterLib(L, "GScaleScroll", typeof(GScaleScroll), regs, fields, typeof(UnityEngine.UI.ScrollRect));
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int CenterOnPos(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 2);
		GScaleScroll obj = (GScaleScroll)LuaScriptMgr.GetUnityObjectSelf(L, 1, "GScaleScroll");
		Vector2 arg0 = LuaScriptMgr.GetVector2(L, 2);
		obj.CenterOnPos(arg0);
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

