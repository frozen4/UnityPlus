using System;
using UnityEngine;
using LuaInterface;
using Object = UnityEngine.Object;

public class EntityComponent_AnimationUnitWrap
{
	public static void Register(IntPtr L)
	{
		LuaMethod[] regs = new LuaMethod[]
		{
			new LuaMethod("IsPlaying", IsPlaying),
			new LuaMethod("IsRealPlaying", IsRealPlaying),
			new LuaMethod("HasAnimation", HasAnimation),
			new LuaMethod("PlayAnimation", PlayAnimation),
			new LuaMethod("StopAnimation", StopAnimation),
			new LuaMethod("PlayHurtAnimation", PlayHurtAnimation),
			new LuaMethod("PlayDieAnimation", PlayDieAnimation),
			new LuaMethod("PlayPartialSkillAnimation", PlayPartialSkillAnimation),
			new LuaMethod("StopPartialSkillAnimation", StopPartialSkillAnimation),
			new LuaMethod("BluntCurSkillAnimation", BluntCurSkillAnimation),
			new LuaMethod("GetAniLength", GetAniLength),
			new LuaMethod("GetCurAnimNameAtLayer", GetCurAnimNameAtLayer),
			new LuaMethod("GetCurAnimTimeAtLayer", GetCurAnimTimeAtLayer),
			new LuaMethod("PlayAssignedAniClip", PlayAssignedAniClip),
			new LuaMethod("EnableAnimationComponent", EnableAnimationComponent),
			new LuaMethod("CloneAnimationState", CloneAnimationState),
			new LuaMethod("__eq", Lua_Eq),
		};

		LuaField[] fields = new LuaField[]
		{
		};

		LuaScriptMgr.RegisterLib(L, "EntityComponent.AnimationUnit", typeof(EntityComponent.AnimationUnit), regs, fields, typeof(MonoBehaviour));
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int IsPlaying(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 2);
		EntityComponent.AnimationUnit obj = (EntityComponent.AnimationUnit)LuaScriptMgr.GetUnityObjectSelf(L, 1, "EntityComponent.AnimationUnit");
		string arg0 = LuaScriptMgr.GetLuaString(L, 2);
		bool o = obj.IsPlaying(arg0);
		LuaScriptMgr.Push(L, o);
		return 1;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int IsRealPlaying(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 2);
		EntityComponent.AnimationUnit obj = (EntityComponent.AnimationUnit)LuaScriptMgr.GetUnityObjectSelf(L, 1, "EntityComponent.AnimationUnit");
		string arg0 = LuaScriptMgr.GetLuaString(L, 2);
		bool o = obj.IsRealPlaying(arg0);
		LuaScriptMgr.Push(L, o);
		return 1;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int HasAnimation(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 2);
		EntityComponent.AnimationUnit obj = (EntityComponent.AnimationUnit)LuaScriptMgr.GetUnityObjectSelf(L, 1, "EntityComponent.AnimationUnit");
		string arg0 = LuaScriptMgr.GetLuaString(L, 2);
		bool o = obj.HasAnimation(arg0);
		LuaScriptMgr.Push(L, o);
		return 1;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int PlayAnimation(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 7);
		EntityComponent.AnimationUnit obj = (EntityComponent.AnimationUnit)LuaScriptMgr.GetUnityObjectSelf(L, 1, "EntityComponent.AnimationUnit");
		string arg0 = LuaScriptMgr.GetLuaString(L, 2);
		float arg1 = (float)LuaScriptMgr.GetNumber(L, 3);
		bool arg2 = LuaScriptMgr.GetBoolean(L, 4);
		float arg3 = (float)LuaScriptMgr.GetNumber(L, 5);
		bool arg4 = LuaScriptMgr.GetBoolean(L, 6);
		float arg5 = (float)LuaScriptMgr.GetNumber(L, 7);
		obj.PlayAnimation(arg0,arg1,arg2,arg3,arg4,arg5);
		return 0;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int StopAnimation(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 3);
		EntityComponent.AnimationUnit obj = (EntityComponent.AnimationUnit)LuaScriptMgr.GetUnityObjectSelf(L, 1, "EntityComponent.AnimationUnit");
		string arg0 = LuaScriptMgr.GetLuaString(L, 2);
		int arg1 = (int)LuaScriptMgr.GetNumber(L, 3);
		obj.StopAnimation(arg0,arg1);
		return 0;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int PlayHurtAnimation(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 3);
		EntityComponent.AnimationUnit obj = (EntityComponent.AnimationUnit)LuaScriptMgr.GetUnityObjectSelf(L, 1, "EntityComponent.AnimationUnit");
		bool arg0 = LuaScriptMgr.GetBoolean(L, 2);
		string arg1 = LuaScriptMgr.GetLuaString(L, 3);
		obj.PlayHurtAnimation(arg0,arg1);
		return 0;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int PlayDieAnimation(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 2);
		EntityComponent.AnimationUnit obj = (EntityComponent.AnimationUnit)LuaScriptMgr.GetUnityObjectSelf(L, 1, "EntityComponent.AnimationUnit");
		bool arg0 = LuaScriptMgr.GetBoolean(L, 2);
		obj.PlayDieAnimation(arg0);
		return 0;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int PlayPartialSkillAnimation(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 2);
		EntityComponent.AnimationUnit obj = (EntityComponent.AnimationUnit)LuaScriptMgr.GetUnityObjectSelf(L, 1, "EntityComponent.AnimationUnit");
		string arg0 = LuaScriptMgr.GetLuaString(L, 2);
		obj.PlayPartialSkillAnimation(arg0);
		return 0;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int StopPartialSkillAnimation(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 2);
		EntityComponent.AnimationUnit obj = (EntityComponent.AnimationUnit)LuaScriptMgr.GetUnityObjectSelf(L, 1, "EntityComponent.AnimationUnit");
		string arg0 = LuaScriptMgr.GetLuaString(L, 2);
		obj.StopPartialSkillAnimation(arg0);
		return 0;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int BluntCurSkillAnimation(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 3);
		EntityComponent.AnimationUnit obj = (EntityComponent.AnimationUnit)LuaScriptMgr.GetUnityObjectSelf(L, 1, "EntityComponent.AnimationUnit");
		float arg0 = (float)LuaScriptMgr.GetNumber(L, 2);
		bool arg1 = LuaScriptMgr.GetBoolean(L, 3);
		float o = obj.BluntCurSkillAnimation(arg0,arg1);
		LuaScriptMgr.Push(L, o);
		return 1;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int GetAniLength(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 2);
		EntityComponent.AnimationUnit obj = (EntityComponent.AnimationUnit)LuaScriptMgr.GetUnityObjectSelf(L, 1, "EntityComponent.AnimationUnit");
		string arg0 = LuaScriptMgr.GetLuaString(L, 2);
		float o = obj.GetAniLength(arg0);
		LuaScriptMgr.Push(L, o);
		return 1;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int GetCurAnimNameAtLayer(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 2);
		EntityComponent.AnimationUnit obj = (EntityComponent.AnimationUnit)LuaScriptMgr.GetUnityObjectSelf(L, 1, "EntityComponent.AnimationUnit");
		int arg0 = (int)LuaScriptMgr.GetNumber(L, 2);
		string o = obj.GetCurAnimNameAtLayer(arg0);
		LuaScriptMgr.Push(L, o);
		return 1;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int GetCurAnimTimeAtLayer(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 2);
		EntityComponent.AnimationUnit obj = (EntityComponent.AnimationUnit)LuaScriptMgr.GetUnityObjectSelf(L, 1, "EntityComponent.AnimationUnit");
		int arg0 = (int)LuaScriptMgr.GetNumber(L, 2);
		float o = obj.GetCurAnimTimeAtLayer(arg0);
		LuaScriptMgr.Push(L, o);
		return 1;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int PlayAssignedAniClip(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 3);
		EntityComponent.AnimationUnit obj = (EntityComponent.AnimationUnit)LuaScriptMgr.GetUnityObjectSelf(L, 1, "EntityComponent.AnimationUnit");
		string arg0 = LuaScriptMgr.GetLuaString(L, 2);
		float arg1 = (float)LuaScriptMgr.GetNumber(L, 3);
		obj.PlayAssignedAniClip(arg0,arg1);
		return 0;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int EnableAnimationComponent(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 2);
		EntityComponent.AnimationUnit obj = (EntityComponent.AnimationUnit)LuaScriptMgr.GetUnityObjectSelf(L, 1, "EntityComponent.AnimationUnit");
		bool arg0 = LuaScriptMgr.GetBoolean(L, 2);
		obj.EnableAnimationComponent(arg0);
		return 0;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int CloneAnimationState(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 2);
		EntityComponent.AnimationUnit obj = (EntityComponent.AnimationUnit)LuaScriptMgr.GetUnityObjectSelf(L, 1, "EntityComponent.AnimationUnit");
		string arg0 = LuaScriptMgr.GetLuaString(L, 2);
		obj.CloneAnimationState(arg0);
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

