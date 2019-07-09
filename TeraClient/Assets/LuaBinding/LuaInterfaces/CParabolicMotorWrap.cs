using System;
using UnityEngine;
using LuaInterface;
using Object = UnityEngine.Object;

public class CParabolicMotorWrap
{
	public static void Register(IntPtr L)
	{
		LuaMethod[] regs = new LuaMethod[]
		{
			new LuaMethod("SetParams", SetParams),
			new LuaMethod("__eq", Lua_Eq),
		};

		LuaField[] fields = new LuaField[]
		{
		};

		LuaScriptMgr.RegisterLib(L, "CParabolicMotor", typeof(CParabolicMotor), regs, fields, typeof(CMotor));
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int SetParams(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 3);
		CParabolicMotor obj = (CParabolicMotor)LuaScriptMgr.GetUnityObjectSelf(L, 1, "CParabolicMotor");
		float arg0 = (float)LuaScriptMgr.GetNumber(L, 2);
		float arg1 = (float)LuaScriptMgr.GetNumber(L, 3);
		obj.SetParams(arg0,arg1);
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

