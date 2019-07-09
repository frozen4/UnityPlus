using System;
using UnityEngine;
using LuaInterface;
using Object = UnityEngine.Object;

public class EntityComponent_HorseStandBehaviourWrap
{
	public static void Register(IntPtr L)
	{
		LuaMethod[] regs = new LuaMethod[]
		{
			new LuaMethod("Init", Init),
			new LuaMethod("StartIdle", StartIdle),
			new LuaMethod("StopIdle", StopIdle),
			new LuaMethod("StartBorn", StartBorn),
			new LuaMethod("__eq", Lua_Eq),
		};

		LuaField[] fields = new LuaField[]
		{
		};

		LuaScriptMgr.RegisterLib(L, "EntityComponent.HorseStandBehaviour", typeof(EntityComponent.HorseStandBehaviour), regs, fields, typeof(MonoBehaviour));
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int Init(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 5);
		EntityComponent.HorseStandBehaviour obj = (EntityComponent.HorseStandBehaviour)LuaScriptMgr.GetUnityObjectSelf(L, 1, "EntityComponent.HorseStandBehaviour");
		int arg0 = (int)LuaScriptMgr.GetNumber(L, 2);
		int arg1 = (int)LuaScriptMgr.GetNumber(L, 3);
		string arg2 = LuaScriptMgr.GetLuaString(L, 4);
		GameObject arg3 = (GameObject)LuaScriptMgr.GetUnityObject(L, 5, typeof(GameObject));
		obj.Init(arg0,arg1,arg2,arg3);
		return 0;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int StartIdle(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 1);
		EntityComponent.HorseStandBehaviour obj = (EntityComponent.HorseStandBehaviour)LuaScriptMgr.GetUnityObjectSelf(L, 1, "EntityComponent.HorseStandBehaviour");
		obj.StartIdle();
		return 0;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int StopIdle(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 1);
		EntityComponent.HorseStandBehaviour obj = (EntityComponent.HorseStandBehaviour)LuaScriptMgr.GetUnityObjectSelf(L, 1, "EntityComponent.HorseStandBehaviour");
		obj.StopIdle();
		return 0;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int StartBorn(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 1);
		EntityComponent.HorseStandBehaviour obj = (EntityComponent.HorseStandBehaviour)LuaScriptMgr.GetUnityObjectSelf(L, 1, "EntityComponent.HorseStandBehaviour");
		obj.StartBorn();
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

