using System;
using UnityEngine;
using LuaInterface;
using Object = UnityEngine.Object;

public class AnimationWrap
{
	public static void Register(IntPtr L)
	{
		LuaMethod[] regs = new LuaMethod[]
		{
			new LuaMethod("Stop", Stop),
			new LuaMethod("Rewind", Rewind),
			new LuaMethod("IsPlaying", IsPlaying),
			new LuaMethod("Play", Play),
			new LuaMethod("PlayQueued", PlayQueued),
			new LuaMethod("GetClip", GetClip),
			new LuaMethod("__eq", Lua_Eq),
		};

		LuaField[] fields = new LuaField[]
		{
			new LuaField("clip", get_clip, set_clip),
		};

		LuaScriptMgr.RegisterLib(L, "UnityEngine.Animation", typeof(Animation), regs, fields, typeof(Behaviour));
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int get_clip(IntPtr L)
	{
		object o = LuaScriptMgr.GetLuaObject(L, 1);
		Animation obj = (Animation)o;

		if (obj == null)
		{
			LuaTypes types = LuaDLL.lua_type(L, 1);

			if (types == LuaTypes.LUA_TTABLE)
			{
				LuaDLL.luaL_error(L, "unknown member name clip");
			}
			else
			{
				LuaDLL.luaL_error(L, "attempt to index clip on a nil value");
			}
		}

		LuaScriptMgr.Push(L, obj.clip);
		return 1;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int set_clip(IntPtr L)
	{
		object o = LuaScriptMgr.GetLuaObject(L, 1);
		Animation obj = (Animation)o;

		if (obj == null)
		{
			LuaTypes types = LuaDLL.lua_type(L, 1);

			if (types == LuaTypes.LUA_TTABLE)
			{
				LuaDLL.luaL_error(L, "unknown member name clip");
			}
			else
			{
				LuaDLL.luaL_error(L, "attempt to index clip on a nil value");
			}
		}

		obj.clip = (AnimationClip)LuaScriptMgr.GetUnityObject(L, 3, typeof(AnimationClip));
		return 0;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int Stop(IntPtr L)
	{
		int count = LuaDLL.lua_gettop(L);

		if (count == 1)
		{
			Animation obj = (Animation)LuaScriptMgr.GetUnityObjectSelf(L, 1, "Animation");
			obj.Stop();
			return 0;
		}
		else if (count == 2)
		{
			Animation obj = (Animation)LuaScriptMgr.GetUnityObjectSelf(L, 1, "Animation");
			string arg0 = LuaScriptMgr.GetLuaString(L, 2);
			obj.Stop(arg0);
			return 0;
		}
		else
		{
			LuaDLL.luaL_error(L, "invalid arguments to method: Animation.Stop");
		}

		return 0;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int Rewind(IntPtr L)
	{
		int count = LuaDLL.lua_gettop(L);

		if (count == 1)
		{
			Animation obj = (Animation)LuaScriptMgr.GetUnityObjectSelf(L, 1, "Animation");
			obj.Rewind();
			return 0;
		}
		else if (count == 2)
		{
			Animation obj = (Animation)LuaScriptMgr.GetUnityObjectSelf(L, 1, "Animation");
			string arg0 = LuaScriptMgr.GetLuaString(L, 2);
			obj.Rewind(arg0);
			return 0;
		}
		else
		{
			LuaDLL.luaL_error(L, "invalid arguments to method: Animation.Rewind");
		}

		return 0;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int IsPlaying(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 2);
		Animation obj = (Animation)LuaScriptMgr.GetUnityObjectSelf(L, 1, "Animation");
		string arg0 = LuaScriptMgr.GetLuaString(L, 2);
		bool o = obj.IsPlaying(arg0);
		LuaScriptMgr.Push(L, o);
		return 1;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int Play(IntPtr L)
	{
		int count = LuaDLL.lua_gettop(L);

		if (count == 1)
		{
			Animation obj = (Animation)LuaScriptMgr.GetUnityObjectSelf(L, 1, "Animation");
			bool o = obj.Play();
			LuaScriptMgr.Push(L, o);
			return 1;
		}
		else if (count == 2 && LuaScriptMgr.CheckTypes(L, 2, typeof(string)))
		{
			Animation obj = (Animation)LuaScriptMgr.GetUnityObjectSelf(L, 1, "Animation");
			string arg0 = LuaScriptMgr.GetString(L, 2);
			bool o = obj.Play(arg0);
			LuaScriptMgr.Push(L, o);
			return 1;
		}
		else if (count == 2 && LuaScriptMgr.CheckTypes(L, 2, typeof(PlayMode)))
		{
			Animation obj = (Animation)LuaScriptMgr.GetUnityObjectSelf(L, 1, "Animation");
			PlayMode arg0 = (PlayMode)LuaScriptMgr.GetLuaObject(L, 2);
			bool o = obj.Play(arg0);
			LuaScriptMgr.Push(L, o);
			return 1;
		}
		else if (count == 3)
		{
			Animation obj = (Animation)LuaScriptMgr.GetUnityObjectSelf(L, 1, "Animation");
			string arg0 = LuaScriptMgr.GetLuaString(L, 2);
			PlayMode arg1 = (PlayMode)LuaScriptMgr.GetNetObject(L, 3, typeof(PlayMode));
			bool o = obj.Play(arg0,arg1);
			LuaScriptMgr.Push(L, o);
			return 1;
		}
		else
		{
			LuaDLL.luaL_error(L, "invalid arguments to method: Animation.Play");
		}

		return 0;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int PlayQueued(IntPtr L)
	{
		int count = LuaDLL.lua_gettop(L);

		if (count == 2)
		{
			Animation obj = (Animation)LuaScriptMgr.GetUnityObjectSelf(L, 1, "Animation");
			string arg0 = LuaScriptMgr.GetLuaString(L, 2);
			obj.PlayQueued(arg0);
			return 0;
		}
		else if (count == 3)
		{
			Animation obj = (Animation)LuaScriptMgr.GetUnityObjectSelf(L, 1, "Animation");
			string arg0 = LuaScriptMgr.GetLuaString(L, 2);
			QueueMode arg1 = (QueueMode)LuaScriptMgr.GetNetObject(L, 3, typeof(QueueMode));
			obj.PlayQueued(arg0,arg1);
			return 0;
		}
		else if (count == 4)
		{
			Animation obj = (Animation)LuaScriptMgr.GetUnityObjectSelf(L, 1, "Animation");
			string arg0 = LuaScriptMgr.GetLuaString(L, 2);
			QueueMode arg1 = (QueueMode)LuaScriptMgr.GetNetObject(L, 3, typeof(QueueMode));
			PlayMode arg2 = (PlayMode)LuaScriptMgr.GetNetObject(L, 4, typeof(PlayMode));
			obj.PlayQueued(arg0,arg1,arg2);
			return 0;
		}
		else
		{
			LuaDLL.luaL_error(L, "invalid arguments to method: Animation.PlayQueued");
		}

		return 0;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int GetClip(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 2);
		Animation obj = (Animation)LuaScriptMgr.GetUnityObjectSelf(L, 1, "Animation");
		string arg0 = LuaScriptMgr.GetLuaString(L, 2);
		AnimationClip o = obj.GetClip(arg0);
		LuaScriptMgr.Push(L, o);
		return 1;
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

