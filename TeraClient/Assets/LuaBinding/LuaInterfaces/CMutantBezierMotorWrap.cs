using System;
using UnityEngine;
using LuaInterface;
using Object = UnityEngine.Object;

public class CMutantBezierMotorWrap
{
	public static void Register(IntPtr L)
	{
		LuaMethod[] regs = new LuaMethod[]
		{
			new LuaMethod("SetParms", SetParms),
			new LuaMethod("__eq", Lua_Eq),
		};

		LuaField[] fields = new LuaField[]
		{
		};

		LuaScriptMgr.RegisterLib(L, "CMutantBezierMotor", typeof(CMutantBezierMotor), regs, fields, typeof(CMotor));
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int SetParms(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 2);
		CMutantBezierMotor obj = (CMutantBezierMotor)LuaScriptMgr.GetUnityObjectSelf(L, 1, "CMutantBezierMotor");
		float arg0 = (float)LuaScriptMgr.GetNumber(L, 2);
		obj.SetParms(arg0);
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

