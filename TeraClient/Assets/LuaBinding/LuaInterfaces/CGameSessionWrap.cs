using System;
using LuaInterface;

public class CGameSessionWrap
{
	public static void Register(IntPtr L)
	{
		LuaMethod[] regs = new LuaMethod[]
		{
			new LuaMethod("Instance", Instance),
			new LuaMethod("IsValidIpAddress", IsValidIpAddress),
			new LuaMethod("ConnectToServer", ConnectToServer),
			new LuaMethod("IsConnecting", IsConnecting),
			new LuaMethod("IsConnected", IsConnected),
			new LuaMethod("Close", Close),
			new LuaMethod("TestBytes", TestBytes),
			new LuaMethod("CheckConnection", CheckConnection),
			new LuaMethod("IntToBytes", IntToBytes),
		};

		LuaField[] fields = new LuaField[]
		{
			new LuaField("IsProcessingPaused", get_IsProcessingPaused, set_IsProcessingPaused),
			new LuaField("IsSendingPaused", get_IsSendingPaused, set_IsSendingPaused),
		};

		LuaScriptMgr.RegisterLib(L, "CGameSession", typeof(CGameSession), regs, fields, typeof(object));
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int get_IsProcessingPaused(IntPtr L)
	{
		object o = LuaScriptMgr.GetLuaObject(L, 1);
		CGameSession obj = (CGameSession)o;

		if (obj == null)
		{
			LuaTypes types = LuaDLL.lua_type(L, 1);

			if (types == LuaTypes.LUA_TTABLE)
			{
				LuaDLL.luaL_error(L, "unknown member name IsProcessingPaused");
			}
			else
			{
				LuaDLL.luaL_error(L, "attempt to index IsProcessingPaused on a nil value");
			}
		}

		LuaScriptMgr.Push(L, obj.IsProcessingPaused);
		return 1;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int get_IsSendingPaused(IntPtr L)
	{
		object o = LuaScriptMgr.GetLuaObject(L, 1);
		CGameSession obj = (CGameSession)o;

		if (obj == null)
		{
			LuaTypes types = LuaDLL.lua_type(L, 1);

			if (types == LuaTypes.LUA_TTABLE)
			{
				LuaDLL.luaL_error(L, "unknown member name IsSendingPaused");
			}
			else
			{
				LuaDLL.luaL_error(L, "attempt to index IsSendingPaused on a nil value");
			}
		}

		LuaScriptMgr.Push(L, obj.IsSendingPaused);
		return 1;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int set_IsProcessingPaused(IntPtr L)
	{
		object o = LuaScriptMgr.GetLuaObject(L, 1);
		CGameSession obj = (CGameSession)o;

		if (obj == null)
		{
			LuaTypes types = LuaDLL.lua_type(L, 1);

			if (types == LuaTypes.LUA_TTABLE)
			{
				LuaDLL.luaL_error(L, "unknown member name IsProcessingPaused");
			}
			else
			{
				LuaDLL.luaL_error(L, "attempt to index IsProcessingPaused on a nil value");
			}
		}

		obj.IsProcessingPaused = LuaScriptMgr.GetBoolean(L, 3);
		return 0;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int set_IsSendingPaused(IntPtr L)
	{
		object o = LuaScriptMgr.GetLuaObject(L, 1);
		CGameSession obj = (CGameSession)o;

		if (obj == null)
		{
			LuaTypes types = LuaDLL.lua_type(L, 1);

			if (types == LuaTypes.LUA_TTABLE)
			{
				LuaDLL.luaL_error(L, "unknown member name IsSendingPaused");
			}
			else
			{
				LuaDLL.luaL_error(L, "attempt to index IsSendingPaused on a nil value");
			}
		}

		obj.IsSendingPaused = LuaScriptMgr.GetBoolean(L, 3);
		return 0;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int Instance(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 0);
		CGameSession o = CGameSession.Instance();
		LuaScriptMgr.PushObject(L, o);
		return 1;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int IsValidIpAddress(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 2);
		CGameSession obj = (CGameSession)LuaScriptMgr.GetNetObjectSelf(L, 1, "CGameSession");
		string arg0 = LuaScriptMgr.GetLuaString(L, 2);
		bool o = obj.IsValidIpAddress(arg0);
		LuaScriptMgr.Push(L, o);
		return 1;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int ConnectToServer(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 5);
		CGameSession obj = (CGameSession)LuaScriptMgr.GetNetObjectSelf(L, 1, "CGameSession");
		string arg0 = LuaScriptMgr.GetLuaString(L, 2);
		int arg1 = (int)LuaScriptMgr.GetNumber(L, 3);
		string arg2 = LuaScriptMgr.GetLuaString(L, 4);
		string arg3 = LuaScriptMgr.GetLuaString(L, 5);
		obj.ConnectToServer(arg0,arg1,arg2,arg3);
		return 0;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int IsConnecting(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 1);
		CGameSession obj = (CGameSession)LuaScriptMgr.GetNetObjectSelf(L, 1, "CGameSession");
		bool o = obj.IsConnecting();
		LuaScriptMgr.Push(L, o);
		return 1;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int IsConnected(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 1);
		CGameSession obj = (CGameSession)LuaScriptMgr.GetNetObjectSelf(L, 1, "CGameSession");
		bool o = obj.IsConnected();
		LuaScriptMgr.Push(L, o);
		return 1;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int Close(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 1);
		CGameSession obj = (CGameSession)LuaScriptMgr.GetNetObjectSelf(L, 1, "CGameSession");
		obj.Close();
		return 0;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int TestBytes(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 4);
		CGameSession obj = (CGameSession)LuaScriptMgr.GetNetObjectSelf(L, 1, "CGameSession");
		CS2CPrtcData arg0 = (CS2CPrtcData)LuaScriptMgr.GetNetObject(L, 2, typeof(CS2CPrtcData));
		bool arg1 = LuaScriptMgr.GetBoolean(L, 3);
		bool arg2 = LuaScriptMgr.GetBoolean(L, 4);
		obj.TestBytes(arg0,arg1,arg2);
		return 0;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int CheckConnection(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 2);
		CGameSession obj = (CGameSession)LuaScriptMgr.GetNetObjectSelf(L, 1, "CGameSession");
		int arg0 = (int)LuaScriptMgr.GetNumber(L, 2);
		//obj.CheckConnection(arg0);
		return 0;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int IntToBytes(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 3);
		int arg0 = (int)LuaScriptMgr.GetNumber(L, 1);
		LuaStringBuffer arg1 = LuaScriptMgr.GetStringBuffer(L, 2);
		int arg2 = (int)LuaScriptMgr.GetNumber(L, 3);
		CGameSession.IntToBytes(arg0,arg1.buffer,arg2);
		return 0;
	}
}

