using System;
using UnityEngine;
using LuaInterface;
using Object = UnityEngine.Object;

public class GTextWrap
{
	public static void Register(IntPtr L)
	{
		LuaMethod[] regs = new LuaMethod[]
		{
			new LuaMethod("__eq", Lua_Eq),
		};

		LuaField[] fields = new LuaField[]
		{
			new LuaField("TextID", null, set_TextID),
		};

		LuaScriptMgr.RegisterLib(L, "GText", typeof(GText), regs, fields, typeof(UnityEngine.UI.Text));
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int set_TextID(IntPtr L)
	{
		object o = LuaScriptMgr.GetLuaObject(L, 1);
		GText obj = (GText)o;

		if (obj == null)
		{
			LuaTypes types = LuaDLL.lua_type(L, 1);

			if (types == LuaTypes.LUA_TTABLE)
			{
				LuaDLL.luaL_error(L, "unknown member name TextID");
			}
			else
			{
				LuaDLL.luaL_error(L, "attempt to index TextID on a nil value");
			}
		}

		obj.TextID = (int)LuaScriptMgr.GetNumber(L, 3);
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

