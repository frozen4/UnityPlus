using System;
using UnityEngine;
using LuaInterface;
using Object = UnityEngine.Object;

public class RectTransformWrap
{
	public static void Register(IntPtr L)
	{
		LuaMethod[] regs = new LuaMethod[]
		{
			new LuaMethod("__eq", Lua_Eq),
		};

		LuaField[] fields = new LuaField[]
		{
			new LuaField("rect", get_rect, null),
			new LuaField("anchorMin", get_anchorMin, set_anchorMin),
			new LuaField("anchorMax", get_anchorMax, set_anchorMax),
			new LuaField("anchoredPosition3D", get_anchoredPosition3D, set_anchoredPosition3D),
			new LuaField("anchoredPosition", get_anchoredPosition, set_anchoredPosition),
			new LuaField("sizeDelta", get_sizeDelta, set_sizeDelta),
			new LuaField("pivot", get_pivot, set_pivot),
			new LuaField("offsetMin", get_offsetMin, set_offsetMin),
			new LuaField("offsetMax", get_offsetMax, set_offsetMax),
		};

		LuaScriptMgr.RegisterLib(L, "UnityEngine.RectTransform", typeof(RectTransform), regs, fields, null);
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int get_rect(IntPtr L)
	{
		object o = LuaScriptMgr.GetLuaObject(L, 1);
		RectTransform obj = (RectTransform)o;

		if (obj == null)
		{
			LuaTypes types = LuaDLL.lua_type(L, 1);

			if (types == LuaTypes.LUA_TTABLE)
			{
				LuaDLL.luaL_error(L, "unknown member name rect");
			}
			else
			{
				LuaDLL.luaL_error(L, "attempt to index rect on a nil value");
			}
		}

		LuaScriptMgr.Push(L, obj.rect);
		return 1;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int get_anchorMin(IntPtr L)
	{
		object o = LuaScriptMgr.GetLuaObject(L, 1);
		RectTransform obj = (RectTransform)o;

		if (obj == null)
		{
			LuaTypes types = LuaDLL.lua_type(L, 1);

			if (types == LuaTypes.LUA_TTABLE)
			{
				LuaDLL.luaL_error(L, "unknown member name anchorMin");
			}
			else
			{
				LuaDLL.luaL_error(L, "attempt to index anchorMin on a nil value");
			}
		}

		LuaScriptMgr.Push(L, obj.anchorMin);
		return 1;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int get_anchorMax(IntPtr L)
	{
		object o = LuaScriptMgr.GetLuaObject(L, 1);
		RectTransform obj = (RectTransform)o;

		if (obj == null)
		{
			LuaTypes types = LuaDLL.lua_type(L, 1);

			if (types == LuaTypes.LUA_TTABLE)
			{
				LuaDLL.luaL_error(L, "unknown member name anchorMax");
			}
			else
			{
				LuaDLL.luaL_error(L, "attempt to index anchorMax on a nil value");
			}
		}

		LuaScriptMgr.Push(L, obj.anchorMax);
		return 1;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int get_anchoredPosition3D(IntPtr L)
	{
		object o = LuaScriptMgr.GetLuaObject(L, 1);
		RectTransform obj = (RectTransform)o;

		if (obj == null)
		{
			LuaTypes types = LuaDLL.lua_type(L, 1);

			if (types == LuaTypes.LUA_TTABLE)
			{
				LuaDLL.luaL_error(L, "unknown member name anchoredPosition3D");
			}
			else
			{
				LuaDLL.luaL_error(L, "attempt to index anchoredPosition3D on a nil value");
			}
		}

		LuaScriptMgr.Push(L, obj.anchoredPosition3D);
		return 1;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int get_anchoredPosition(IntPtr L)
	{
		object o = LuaScriptMgr.GetLuaObject(L, 1);
		RectTransform obj = (RectTransform)o;

		if (obj == null)
		{
			LuaTypes types = LuaDLL.lua_type(L, 1);

			if (types == LuaTypes.LUA_TTABLE)
			{
				LuaDLL.luaL_error(L, "unknown member name anchoredPosition");
			}
			else
			{
				LuaDLL.luaL_error(L, "attempt to index anchoredPosition on a nil value");
			}
		}

		LuaScriptMgr.Push(L, obj.anchoredPosition);
		return 1;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int get_sizeDelta(IntPtr L)
	{
		object o = LuaScriptMgr.GetLuaObject(L, 1);
		RectTransform obj = (RectTransform)o;

		if (obj == null)
		{
			LuaTypes types = LuaDLL.lua_type(L, 1);

			if (types == LuaTypes.LUA_TTABLE)
			{
				LuaDLL.luaL_error(L, "unknown member name sizeDelta");
			}
			else
			{
				LuaDLL.luaL_error(L, "attempt to index sizeDelta on a nil value");
			}
		}

		LuaScriptMgr.Push(L, obj.sizeDelta);
		return 1;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int get_pivot(IntPtr L)
	{
		object o = LuaScriptMgr.GetLuaObject(L, 1);
		RectTransform obj = (RectTransform)o;

		if (obj == null)
		{
			LuaTypes types = LuaDLL.lua_type(L, 1);

			if (types == LuaTypes.LUA_TTABLE)
			{
				LuaDLL.luaL_error(L, "unknown member name pivot");
			}
			else
			{
				LuaDLL.luaL_error(L, "attempt to index pivot on a nil value");
			}
		}

		LuaScriptMgr.Push(L, obj.pivot);
		return 1;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int get_offsetMin(IntPtr L)
	{
		object o = LuaScriptMgr.GetLuaObject(L, 1);
		RectTransform obj = (RectTransform)o;

		if (obj == null)
		{
			LuaTypes types = LuaDLL.lua_type(L, 1);

			if (types == LuaTypes.LUA_TTABLE)
			{
				LuaDLL.luaL_error(L, "unknown member name offsetMin");
			}
			else
			{
				LuaDLL.luaL_error(L, "attempt to index offsetMin on a nil value");
			}
		}

		LuaScriptMgr.Push(L, obj.offsetMin);
		return 1;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int get_offsetMax(IntPtr L)
	{
		object o = LuaScriptMgr.GetLuaObject(L, 1);
		RectTransform obj = (RectTransform)o;

		if (obj == null)
		{
			LuaTypes types = LuaDLL.lua_type(L, 1);

			if (types == LuaTypes.LUA_TTABLE)
			{
				LuaDLL.luaL_error(L, "unknown member name offsetMax");
			}
			else
			{
				LuaDLL.luaL_error(L, "attempt to index offsetMax on a nil value");
			}
		}

		LuaScriptMgr.Push(L, obj.offsetMax);
		return 1;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int set_anchorMin(IntPtr L)
	{
		object o = LuaScriptMgr.GetLuaObject(L, 1);
		RectTransform obj = (RectTransform)o;

		if (obj == null)
		{
			LuaTypes types = LuaDLL.lua_type(L, 1);

			if (types == LuaTypes.LUA_TTABLE)
			{
				LuaDLL.luaL_error(L, "unknown member name anchorMin");
			}
			else
			{
				LuaDLL.luaL_error(L, "attempt to index anchorMin on a nil value");
			}
		}

		obj.anchorMin = LuaScriptMgr.GetVector2(L, 3);
		return 0;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int set_anchorMax(IntPtr L)
	{
		object o = LuaScriptMgr.GetLuaObject(L, 1);
		RectTransform obj = (RectTransform)o;

		if (obj == null)
		{
			LuaTypes types = LuaDLL.lua_type(L, 1);

			if (types == LuaTypes.LUA_TTABLE)
			{
				LuaDLL.luaL_error(L, "unknown member name anchorMax");
			}
			else
			{
				LuaDLL.luaL_error(L, "attempt to index anchorMax on a nil value");
			}
		}

		obj.anchorMax = LuaScriptMgr.GetVector2(L, 3);
		return 0;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int set_anchoredPosition3D(IntPtr L)
	{
		object o = LuaScriptMgr.GetLuaObject(L, 1);
		RectTransform obj = (RectTransform)o;

		if (obj == null)
		{
			LuaTypes types = LuaDLL.lua_type(L, 1);

			if (types == LuaTypes.LUA_TTABLE)
			{
				LuaDLL.luaL_error(L, "unknown member name anchoredPosition3D");
			}
			else
			{
				LuaDLL.luaL_error(L, "attempt to index anchoredPosition3D on a nil value");
			}
		}

		obj.anchoredPosition3D = LuaScriptMgr.GetVector3(L, 3);
		return 0;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int set_anchoredPosition(IntPtr L)
	{
		object o = LuaScriptMgr.GetLuaObject(L, 1);
		RectTransform obj = (RectTransform)o;

		if (obj == null)
		{
			LuaTypes types = LuaDLL.lua_type(L, 1);

			if (types == LuaTypes.LUA_TTABLE)
			{
				LuaDLL.luaL_error(L, "unknown member name anchoredPosition");
			}
			else
			{
				LuaDLL.luaL_error(L, "attempt to index anchoredPosition on a nil value");
			}
		}

		obj.anchoredPosition = LuaScriptMgr.GetVector2(L, 3);
		return 0;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int set_sizeDelta(IntPtr L)
	{
		object o = LuaScriptMgr.GetLuaObject(L, 1);
		RectTransform obj = (RectTransform)o;

		if (obj == null)
		{
			LuaTypes types = LuaDLL.lua_type(L, 1);

			if (types == LuaTypes.LUA_TTABLE)
			{
				LuaDLL.luaL_error(L, "unknown member name sizeDelta");
			}
			else
			{
				LuaDLL.luaL_error(L, "attempt to index sizeDelta on a nil value");
			}
		}

		obj.sizeDelta = LuaScriptMgr.GetVector2(L, 3);
		return 0;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int set_pivot(IntPtr L)
	{
		object o = LuaScriptMgr.GetLuaObject(L, 1);
		RectTransform obj = (RectTransform)o;

		if (obj == null)
		{
			LuaTypes types = LuaDLL.lua_type(L, 1);

			if (types == LuaTypes.LUA_TTABLE)
			{
				LuaDLL.luaL_error(L, "unknown member name pivot");
			}
			else
			{
				LuaDLL.luaL_error(L, "attempt to index pivot on a nil value");
			}
		}

		obj.pivot = LuaScriptMgr.GetVector2(L, 3);
		return 0;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int set_offsetMin(IntPtr L)
	{
		object o = LuaScriptMgr.GetLuaObject(L, 1);
		RectTransform obj = (RectTransform)o;

		if (obj == null)
		{
			LuaTypes types = LuaDLL.lua_type(L, 1);

			if (types == LuaTypes.LUA_TTABLE)
			{
				LuaDLL.luaL_error(L, "unknown member name offsetMin");
			}
			else
			{
				LuaDLL.luaL_error(L, "attempt to index offsetMin on a nil value");
			}
		}

		obj.offsetMin = LuaScriptMgr.GetVector2(L, 3);
		return 0;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int set_offsetMax(IntPtr L)
	{
		object o = LuaScriptMgr.GetLuaObject(L, 1);
		RectTransform obj = (RectTransform)o;

		if (obj == null)
		{
			LuaTypes types = LuaDLL.lua_type(L, 1);

			if (types == LuaTypes.LUA_TTABLE)
			{
				LuaDLL.luaL_error(L, "unknown member name offsetMax");
			}
			else
			{
				LuaDLL.luaL_error(L, "attempt to index offsetMax on a nil value");
			}
		}

		obj.offsetMax = LuaScriptMgr.GetVector2(L, 3);
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

