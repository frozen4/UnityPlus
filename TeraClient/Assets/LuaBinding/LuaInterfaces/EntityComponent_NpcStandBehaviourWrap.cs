using System;
using UnityEngine;
using LuaInterface;
using Object = UnityEngine.Object;

public class EntityComponent_NpcStandBehaviourWrap
{
	public static void Register(IntPtr L)
	{
		LuaMethod[] regs = new LuaMethod[]
		{
			new LuaMethod("Init", Init),
			new LuaMethod("StartIdle", StartIdle),
			new LuaMethod("StopIdle", StopIdle),
			new LuaMethod("StartNpcTalk", StartNpcTalk),
			new LuaMethod("__eq", Lua_Eq),
		};

		LuaField[] fields = new LuaField[]
		{
		};

		LuaScriptMgr.RegisterLib(L, "EntityComponent.NpcStandBehaviour", typeof(EntityComponent.NpcStandBehaviour), regs, fields, typeof(MonoBehaviour));
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int Init(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 2);
		EntityComponent.NpcStandBehaviour obj = (EntityComponent.NpcStandBehaviour)LuaScriptMgr.GetUnityObjectSelf(L, 1, "EntityComponent.NpcStandBehaviour");
		string arg0 = LuaScriptMgr.GetLuaString(L, 2);
		obj.Init(arg0);
		return 0;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int StartIdle(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 1);
		EntityComponent.NpcStandBehaviour obj = (EntityComponent.NpcStandBehaviour)LuaScriptMgr.GetUnityObjectSelf(L, 1, "EntityComponent.NpcStandBehaviour");
		obj.StartIdle();
		return 0;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int StopIdle(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 1);
		EntityComponent.NpcStandBehaviour obj = (EntityComponent.NpcStandBehaviour)LuaScriptMgr.GetUnityObjectSelf(L, 1, "EntityComponent.NpcStandBehaviour");
		obj.StopIdle();
		return 0;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int StartNpcTalk(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 1);
		EntityComponent.NpcStandBehaviour obj = (EntityComponent.NpcStandBehaviour)LuaScriptMgr.GetUnityObjectSelf(L, 1, "EntityComponent.NpcStandBehaviour");
		obj.StartNpcTalk();
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

