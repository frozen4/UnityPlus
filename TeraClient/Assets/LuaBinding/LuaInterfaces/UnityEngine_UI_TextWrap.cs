using System;
using UnityEngine;
using LuaInterface;
using Object = UnityEngine.Object;

public class UnityEngine_UI_TextWrap
{
	public static void Register(IntPtr L)
	{
		LuaMethod[] regs = new LuaMethod[]
		{
			new LuaMethod("__eq", Lua_Eq),
		};

		LuaField[] fields = new LuaField[]
		{
			new LuaField("text", get_text, set_text),
			new LuaField("fontSize", get_fontSize, set_fontSize),
			new LuaField("preferredWidth", get_preferredWidth, null),
			new LuaField("preferredHeight", get_preferredHeight, null),
		};

		LuaScriptMgr.RegisterLib(L, "UnityEngine.UI.Text", typeof(UnityEngine.UI.Text), regs, fields, typeof(UnityEngine.UI.MaskableGraphic));
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int get_text(IntPtr L)
	{
		object o = LuaScriptMgr.GetLuaObject(L, 1);
		UnityEngine.UI.Text obj = (UnityEngine.UI.Text)o;

		if (obj == null)
		{
			LuaTypes types = LuaDLL.lua_type(L, 1);

			if (types == LuaTypes.LUA_TTABLE)
			{
				LuaDLL.luaL_error(L, "unknown member name text");
			}
			else
			{
				LuaDLL.luaL_error(L, "attempt to index text on a nil value");
			}
		}

		LuaScriptMgr.Push(L, obj.text);
		return 1;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int get_fontSize(IntPtr L)
	{
		object o = LuaScriptMgr.GetLuaObject(L, 1);
		UnityEngine.UI.Text obj = (UnityEngine.UI.Text)o;

		if (obj == null)
		{
			LuaTypes types = LuaDLL.lua_type(L, 1);

			if (types == LuaTypes.LUA_TTABLE)
			{
				LuaDLL.luaL_error(L, "unknown member name fontSize");
			}
			else
			{
				LuaDLL.luaL_error(L, "attempt to index fontSize on a nil value");
			}
		}

		LuaScriptMgr.Push(L, obj.fontSize);
		return 1;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int get_preferredWidth(IntPtr L)
	{
		object o = LuaScriptMgr.GetLuaObject(L, 1);
		UnityEngine.UI.Text obj = (UnityEngine.UI.Text)o;

		if (obj == null)
		{
			LuaTypes types = LuaDLL.lua_type(L, 1);

			if (types == LuaTypes.LUA_TTABLE)
			{
				LuaDLL.luaL_error(L, "unknown member name preferredWidth");
			}
			else
			{
				LuaDLL.luaL_error(L, "attempt to index preferredWidth on a nil value");
			}
		}

		LuaScriptMgr.Push(L, obj.preferredWidth);
		return 1;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int get_preferredHeight(IntPtr L)
	{
		object o = LuaScriptMgr.GetLuaObject(L, 1);
		UnityEngine.UI.Text obj = (UnityEngine.UI.Text)o;

		if (obj == null)
		{
			LuaTypes types = LuaDLL.lua_type(L, 1);

			if (types == LuaTypes.LUA_TTABLE)
			{
				LuaDLL.luaL_error(L, "unknown member name preferredHeight");
			}
			else
			{
				LuaDLL.luaL_error(L, "attempt to index preferredHeight on a nil value");
			}
		}

		LuaScriptMgr.Push(L, obj.preferredHeight);
		return 1;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int set_text(IntPtr L)
	{
		object o = LuaScriptMgr.GetLuaObject(L, 1);
		UnityEngine.UI.Text obj = (UnityEngine.UI.Text)o;

		if (obj == null)
		{
			LuaTypes types = LuaDLL.lua_type(L, 1);

			if (types == LuaTypes.LUA_TTABLE)
			{
				LuaDLL.luaL_error(L, "unknown member name text");
			}
			else
			{
				LuaDLL.luaL_error(L, "attempt to index text on a nil value");
			}
		}

		obj.text = LuaScriptMgr.GetString(L, 3);
		return 0;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int set_fontSize(IntPtr L)
	{
		object o = LuaScriptMgr.GetLuaObject(L, 1);
		UnityEngine.UI.Text obj = (UnityEngine.UI.Text)o;

		if (obj == null)
		{
			LuaTypes types = LuaDLL.lua_type(L, 1);

			if (types == LuaTypes.LUA_TTABLE)
			{
				LuaDLL.luaL_error(L, "unknown member name fontSize");
			}
			else
			{
				LuaDLL.luaL_error(L, "attempt to index fontSize on a nil value");
			}
		}

		obj.fontSize = (int)LuaScriptMgr.GetNumber(L, 3);
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

