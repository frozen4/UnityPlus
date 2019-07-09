using System;
using UnityEngine;
using LuaInterface;
using Object = UnityEngine.Object;

public class EntityComponent_CombatStateChangeBehaviourWrap
{
	public static void Register(IntPtr L)
	{
		LuaMethod[] regs = new LuaMethod[]
		{
			new LuaMethod("ChangeState", ChangeState),
			new LuaMethod("EnableWeaponStateSwtichable", EnableWeaponStateSwtichable),
			new LuaMethod("__eq", Lua_Eq),
		};

		LuaField[] fields = new LuaField[]
		{
		};

		LuaScriptMgr.RegisterLib(L, "EntityComponent.CombatStateChangeBehaviour", typeof(EntityComponent.CombatStateChangeBehaviour), regs, fields, typeof(MonoBehaviour));
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int ChangeState(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 5);
		EntityComponent.CombatStateChangeBehaviour obj = (EntityComponent.CombatStateChangeBehaviour)LuaScriptMgr.GetUnityObjectSelf(L, 1, "EntityComponent.CombatStateChangeBehaviour");
		bool arg0 = LuaScriptMgr.GetBoolean(L, 2);
		bool arg1 = LuaScriptMgr.GetBoolean(L, 3);
		float arg2 = (float)LuaScriptMgr.GetNumber(L, 4);
		float arg3 = (float)LuaScriptMgr.GetNumber(L, 5);
		obj.ChangeState(arg0,arg1,arg2,arg3);
		return 0;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int EnableWeaponStateSwtichable(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 2);
		EntityComponent.CombatStateChangeBehaviour obj = (EntityComponent.CombatStateChangeBehaviour)LuaScriptMgr.GetUnityObjectSelf(L, 1, "EntityComponent.CombatStateChangeBehaviour");
		bool arg0 = LuaScriptMgr.GetBoolean(L, 2);
		obj.EnableWeaponStateSwtichable(arg0);
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

