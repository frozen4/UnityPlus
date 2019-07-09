using System;
using UnityEngine;
using LuaInterface;
using Object = UnityEngine.Object;

public class GameObjectPoolWrap
{
	public static void Register(IntPtr L)
	{
		LuaMethod[] regs = new LuaMethod[]
		{
			new LuaMethod("Regist", Regist),
			new LuaMethod("Get", Get),
			new LuaMethod("Release", Release),
			new LuaMethod("__eq", Lua_Eq),
		};

		LuaField[] fields = new LuaField[]
		{
		};

		LuaScriptMgr.RegisterLib(L, "GameObjectPool", typeof(GameObjectPool), regs, fields, typeof(MonoBehaviour));
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int Regist(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 3);
		GameObjectPool obj = (GameObjectPool)LuaScriptMgr.GetUnityObjectSelf(L, 1, "GameObjectPool");
		GameObject arg0 = (GameObject)LuaScriptMgr.GetUnityObject(L, 2, typeof(GameObject));
		int arg1 = (int)LuaScriptMgr.GetNumber(L, 3);
		obj.Regist(arg0,arg1);
		return 0;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int Get(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 1);
		GameObjectPool obj = (GameObjectPool)LuaScriptMgr.GetUnityObjectSelf(L, 1, "GameObjectPool");
		GameObject o = obj.Get();
		LuaScriptMgr.Push(L, o);
		return 1;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int Release(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 2);
		GameObjectPool obj = (GameObjectPool)LuaScriptMgr.GetUnityObjectSelf(L, 1, "GameObjectPool");
		GameObject arg0 = (GameObject)LuaScriptMgr.GetUnityObject(L, 2, typeof(GameObject));
		obj.Release(arg0);
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

