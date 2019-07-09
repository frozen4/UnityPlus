using System;
using UnityEngine;
using System.Collections.Generic;
using LuaInterface;
using Object = UnityEngine.Object;
using System.Text;

public static class GameObjectWrap
{
	public static void Register(IntPtr L)
	{
		LuaMethod[] regs = new LuaMethod[]
		{
            // 为lua书写简单，将Transform的部分接口直接注册在GameObject中
			new LuaMethod("SetParent", SetParent),
			new LuaMethod("FindChild", FindChild),
			new LuaMethod("GetChild", GetChild),
			new LuaMethod("TransformPoint", TransformPoint),
            new LuaMethod("SetAsFirstSibling", SetAsFirstSibling),
            new LuaMethod("SetAsLastSibling", SetAsLastSibling),
     
            new LuaMethod("PositionXYZ", get_positionXYZ),
            new LuaMethod("PositionXZ", get_positionXZ), 
            new LuaMethod("ForwardXYZ", get_forwardXYZ),
            new LuaMethod("ForwardXZ", get_forwardXZ),

			new LuaMethod("GetComponent", GetComponent),
            new LuaMethod("GetCurAnimation", GetCurAnimation),
            new LuaMethod("GetComponentInChildren", GetComponentInChildren),
			new LuaMethod("GetComponents", GetComponents),
			new LuaMethod("GetComponentsInChildren", GetComponentsInChildren),
			new LuaMethod("SetActive", SetActive),
			new LuaMethod("AddComponent", AddComponent),
            new LuaMethod("SmartAddComponent", SmartAddComponent),

            new LuaMethod("Find", Find),
			new LuaMethod("New", _CreateGameObject),
			new LuaMethod("__eq", Lua_Eq),
		};

		LuaField[] fields = new LuaField[]
		{
            // 为lua书写简单，将Transform的部分接口直接注册在GameObject中
            new LuaField("position", get_position, set_position),
			new LuaField("localPosition", get_localPosition, set_localPosition),
            new LuaField("forward", get_forward, set_forward),
			new LuaField("rotation", get_rotation, set_rotation),
			new LuaField("localRotation", get_localRotation, set_localRotation),
			new LuaField("localScale", get_localScale, set_localScale),
            new LuaField("parent", get_parent, set_parent),
			new LuaField("childCount", get_childCount, null),

            new LuaField("transform", get_transform, null),
			new LuaField("layer", get_layer, set_layer),
			new LuaField("activeSelf", get_activeSelf, null),
			new LuaField("activeInHierarchy", get_activeInHierarchy, null),
			new LuaField("isStatic", get_isStatic, set_isStatic),
			new LuaField("tag", get_tag, set_tag),
            new LuaField("name", get_name, set_name),
			new LuaField("gameObject", get_gameObject, null),
		};

		LuaScriptMgr.RegisterLib(L, "GameObject", typeof(GameObject), regs, fields, typeof(Object));
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int _CreateGameObject(IntPtr L)
	{
		int count = LuaDLL.lua_gettop(L);

		if (count == 0)
		{
			GameObject obj = new GameObject();
			LuaScriptMgr.Push(L, obj);
			return 1;
		}
		else if (count == 1)
		{
			string arg0 = LuaScriptMgr.GetString(L, 1);
			GameObject obj = new GameObject(arg0);
			LuaScriptMgr.Push(L, obj);
			return 1;
		}
		else if (LuaScriptMgr.CheckTypes(L, 1, typeof(string)) && LuaScriptMgr.CheckParamsType(L, typeof(Type), 2, count - 1))
		{
			string arg0 = LuaScriptMgr.GetString(L, 1);
			Type[] objs1 = LuaScriptMgr.GetParamsObject<Type>(L, 2, count - 1);
			GameObject obj = new GameObject(arg0,objs1);
			LuaScriptMgr.Push(L, obj);
			return 1;
		}
		else
		{
			LuaDLL.luaL_error(L, "invalid arguments to method: GameObject.New");
		}

		return 0;
	}

	static Type classType = typeof(GameObject);

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int GetClassType(IntPtr L)
	{
		LuaScriptMgr.Push(L, classType);
		return 1;
	}

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    static int SetParent(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);

        if (count == 2)
        {
            GameObject obj = LuaScriptMgr.GetUnityObject<GameObject>(L, 1);
            GameObject arg0 = LuaScriptMgr.GetUnityObject<GameObject>(L, 2);
            if (obj == null)
            {
                LuaDLL.luaL_error(L, "Method SetParent can not be indexed to a null value");
                return 0;
            }

            if (arg0 == null)
            {
                LuaDLL.luaL_error(L, "Method SetParent's param is null");
                return 0;
            }
            if(obj.transform.parent != arg0.transform)
                obj.transform.SetParent(arg0.transform);
            return 0;
        }
        else if (count == 3)
        {
            GameObject obj = LuaScriptMgr.GetUnityObject<GameObject>(L, 1);
            GameObject arg0 = LuaScriptMgr.GetUnityObject<GameObject>(L, 2);
            bool arg1 = LuaScriptMgr.GetBoolean(L, 3);

            if (obj == null)
            {
                LuaDLL.luaL_error(L, "Method SetParent can not be indexed to a null value");
                return 0;
            }

            if (arg0 == null)
            {
                LuaDLL.luaL_error(L, "Method SetParent's param is null");
                return 0;
            }

            if(obj.transform != arg0.transform)
                obj.transform.SetParent(arg0.transform, arg1);
            return 0;
        }
        else
        {
            LuaDLL.luaL_error(L, "invalid arguments to method: Transform.SetParent");
        }

        return 0;
    }


    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    static int FindChild(IntPtr L)
    {
        LuaScriptMgr.CheckArgsCount(L, 2);
        GameObject obj = (GameObject)LuaScriptMgr.GetUnityObjectSelf(L, 1, "GameObject");
        string arg0 = LuaScriptMgr.GetLuaString(L, 2);
        GameObject g = null;

#if UNITY_EDITOR
        //Debug.LogFormat("GameObject FindChild {0}", arg0);
#endif
        if (arg0.Contains("/"))
        {
            Transform cur_node = obj.transform;
            //Split
            var sl = HobaText.GetStringList();
            var sb = HobaText.GetStringBuilder();
            for (int i = 0; i < arg0.Length; ++i)
            {
                char c = arg0[i];
                if (c == '/')
                {
                    sl.Add(sb.ToString());      //use
                    sb.Length = 0;
                }
                else
                {
                    sb.Append(c);
                }
            }
            if (sb.Length > 0)
            {
                sl.Add(sb.ToString());      //use
                sb.Length = 0;
            }

            //Use
            for (int i = 0; i < sl.Count; i++)
            {
                cur_node = cur_node.Find(sl[i]);
                if (cur_node == null)
                {
                    g = null;
                    break;
                }
                if (i == sl.Count - 1)
                    g = cur_node.gameObject;
            }
        }
        else
        {
            Transform t = obj.transform.Find(arg0);
            if (t != null)
                g = t.gameObject;
        }

        LuaScriptMgr.Push(L, g);

        return 1;
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    static int GetChild(IntPtr L)
    {
        LuaScriptMgr.CheckArgsCount(L, 2);
        GameObject obj = (GameObject)LuaScriptMgr.GetUnityObjectSelf(L, 1, "GameObject");
        int arg0 = (int)LuaScriptMgr.GetNumber(L, 2);
        Transform o = obj.transform.GetChild(arg0);
        LuaScriptMgr.Push(L, o.gameObject);
        return 1;
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    static int TransformPoint(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);

        if (count == 2)
        {
            GameObject obj = (GameObject)LuaScriptMgr.GetUnityObjectSelf(L, 1, "GameObject");
            Vector3 arg0 = LuaScriptMgr.GetVector3(L, 2);
            Vector3 o = obj.transform.TransformPoint(arg0);
            LuaScriptMgr.Push(L, o);
            return 1;
        }
        else if (count == 4)
        {
            GameObject obj = (GameObject)LuaScriptMgr.GetUnityObjectSelf(L, 1, "GameObject");
            float arg0 = (float)LuaScriptMgr.GetNumber(L, 2);
            float arg1 = (float)LuaScriptMgr.GetNumber(L, 3);
            float arg2 = (float)LuaScriptMgr.GetNumber(L, 4);
            Vector3 o = obj.transform.TransformPoint(arg0, arg1, arg2);
            LuaScriptMgr.Push(L, o);
            return 1;
        }
        else
        {
            LuaDLL.luaL_error(L, "invalid arguments to method: Transform.TransformPoint");
        }

        return 0;
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    static int SetAsFirstSibling(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        if (count==1)
        {
            GameObject obj = (GameObject)LuaScriptMgr.GetUnityObjectSelf(L, 1, "GameObject");
            obj.transform.SetAsFirstSibling();
        }
        else
        {
            LuaDLL.luaL_error(L, "invalid arguments to method: Transform.SetAsFirstSibling");
        }
        return 0;
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    static int SetAsLastSibling(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        if (count == 1)
        {
            GameObject obj = (GameObject)LuaScriptMgr.GetUnityObjectSelf(L, 1, "GameObject");
            obj.transform.SetAsLastSibling();
        }
        else
        {
            LuaDLL.luaL_error(L, "invalid arguments to method: Transform.SetAsLastSibling");
        }
        return 0;
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    static int get_position(IntPtr L)
    {
        object o = LuaScriptMgr.GetLuaObject(L, 1);
        GameObject obj = (GameObject)o;

        if (obj == null)
        {
            LuaTypes types = LuaDLL.lua_type(L, 1);

            if (types == LuaTypes.LUA_TTABLE)
            {
                LuaDLL.luaL_error(L, "unknown member name position");
            }
            else
            {
                LuaDLL.luaL_error(L, "attempt to index position on a nil value");
            }
        }

        LuaScriptMgr.Push(L, obj.transform.position);
        return 1;
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    static int get_positionXYZ(IntPtr L)
    {
        object o = LuaScriptMgr.GetLuaObject(L, 1);
        GameObject obj = (GameObject)o;

        if (obj == null)
        {
            LuaTypes types = LuaDLL.lua_type(L, 1);

            if (types == LuaTypes.LUA_TTABLE)
            {
                LuaDLL.luaL_error(L, "unknown member name position");
            }
            else
            {
                LuaDLL.luaL_error(L, "attempt to index position on a nil value");
            }
        }

        Vector3 v = obj.transform.position;
        LuaScriptMgr.Push(L, v.x);
        LuaScriptMgr.Push(L, v.y);
        LuaScriptMgr.Push(L, v.z);
        return 3;
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    static int get_positionXZ(IntPtr L)
    {
        object o = LuaScriptMgr.GetLuaObject(L, 1);
        GameObject obj = (GameObject)o;

        if (obj == null)
        {
            LuaTypes types = LuaDLL.lua_type(L, 1);

            if (types == LuaTypes.LUA_TTABLE)
            {
                LuaDLL.luaL_error(L, "unknown member name position");
            }
            else
            {
                LuaDLL.luaL_error(L, "attempt to index position on a nil value");
            }
        }

        Vector3 v = obj.transform.position;
        LuaScriptMgr.Push(L, v.x);
        LuaScriptMgr.Push(L, v.z);

        return 2;
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    static int get_localPosition(IntPtr L)
    {
        object o = LuaScriptMgr.GetLuaObject(L, 1);
        GameObject obj = (GameObject)o;

        if (obj == null)
        {
            LuaTypes types = LuaDLL.lua_type(L, 1);

            if (types == LuaTypes.LUA_TTABLE)
            {
                LuaDLL.luaL_error(L, "unknown member name localPosition");
            }
            else
            {
                LuaDLL.luaL_error(L, "attempt to index localPosition on a nil value");
            }
        }

        LuaScriptMgr.Push(L, obj.transform.localPosition);
        return 1;
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    static int get_forward(IntPtr L)
    {
        object o = LuaScriptMgr.GetLuaObject(L, 1);
        GameObject obj = (GameObject)o;

        if (obj == null)
        {
            LuaTypes types = LuaDLL.lua_type(L, 1);

            if (types == LuaTypes.LUA_TTABLE)
            {
                LuaDLL.luaL_error(L, "unknown member name forward");
            }
            else
            {
                LuaDLL.luaL_error(L, "attempt to index forward on a nil value");
            }
        }

        LuaScriptMgr.Push(L, obj.transform.forward);
        return 1;
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    static int get_forwardXYZ(IntPtr L)
    {
        object o = LuaScriptMgr.GetLuaObject(L, 1);
        GameObject obj = (GameObject)o;

        if (obj == null)
        {
            LuaTypes types = LuaDLL.lua_type(L, 1);

            if (types == LuaTypes.LUA_TTABLE)
            {
                LuaDLL.luaL_error(L, "unknown member name forward");
            }
            else
            {
                LuaDLL.luaL_error(L, "attempt to index forward on a nil value");
            }
        }

        Vector3 v = obj.transform.forward;
        LuaScriptMgr.Push(L, v.x);
        LuaScriptMgr.Push(L, v.y);
        LuaScriptMgr.Push(L, v.z);
        return 3;
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    static int get_forwardXZ(IntPtr L)
    {
        object o = LuaScriptMgr.GetLuaObject(L, 1);
        GameObject obj = (GameObject)o;

        if (obj == null)
        {
            LuaTypes types = LuaDLL.lua_type(L, 1);

            if (types == LuaTypes.LUA_TTABLE)
            {
                LuaDLL.luaL_error(L, "unknown member name forward");
            }
            else
            {
                LuaDLL.luaL_error(L, "attempt to index forward on a nil value");
            }
        }

        Vector3 v = obj.transform.forward;
        LuaScriptMgr.Push(L, v.x);
        LuaScriptMgr.Push(L, v.z);
        return 2;
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    static int get_rotation(IntPtr L)
    {
        object o = LuaScriptMgr.GetLuaObject(L, 1);
        GameObject obj = (GameObject)o;

        if (obj == null)
        {
            LuaTypes types = LuaDLL.lua_type(L, 1);

            if (types == LuaTypes.LUA_TTABLE)
            {
                LuaDLL.luaL_error(L, "unknown member name rotation");
            }
            else
            {
                LuaDLL.luaL_error(L, "attempt to index rotation on a nil value");
            }
        }

        LuaScriptMgr.Push(L, obj.transform.rotation);
        return 1;
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    static int get_localRotation(IntPtr L)
    {
        object o = LuaScriptMgr.GetLuaObject(L, 1);
        GameObject obj = (GameObject)o;

        if (obj == null)
        {
            LuaTypes types = LuaDLL.lua_type(L, 1);

            if (types == LuaTypes.LUA_TTABLE)
            {
                LuaDLL.luaL_error(L, "unknown member name localRotation");
            }
            else
            {
                LuaDLL.luaL_error(L, "attempt to index localRotation on a nil value");
            }
        }

        LuaScriptMgr.Push(L, obj.transform.localRotation);
        return 1;
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    static int get_localScale(IntPtr L)
    {
        object o = LuaScriptMgr.GetLuaObject(L, 1);
        GameObject obj = (GameObject)o;

        if (obj == null)
        {
            LuaTypes types = LuaDLL.lua_type(L, 1);

            if (types == LuaTypes.LUA_TTABLE)
            {
                LuaDLL.luaL_error(L, "unknown member name localScale");
            }
            else
            {
                LuaDLL.luaL_error(L, "attempt to index localScale on a nil value");
            }
        }

        LuaScriptMgr.Push(L, obj.transform.localScale);
        return 1;
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    static int get_parent(IntPtr L)
    {
        object o = LuaScriptMgr.GetLuaObject(L, 1);
        GameObject obj = (GameObject)o;

        if (obj == null)
        {
            LuaTypes types = LuaDLL.lua_type(L, 1);

            if (types == LuaTypes.LUA_TTABLE)
            {
                LuaDLL.luaL_error(L, "unknown member name parent");
            }
            else
            {
                LuaDLL.luaL_error(L, "attempt to index parent on a nil value");
            }
        }

        LuaScriptMgr.Push(L, obj.transform.parent.gameObject);
        return 1;
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    static int get_childCount(IntPtr L)
    {
        object o = LuaScriptMgr.GetLuaObject(L, 1);
        GameObject obj = (GameObject)o;

        if (obj == null)
        {
            LuaTypes types = LuaDLL.lua_type(L, 1);

            if (types == LuaTypes.LUA_TTABLE)
            {
                LuaDLL.luaL_error(L, "unknown member name childCount");
            }
            else
            {
                LuaDLL.luaL_error(L, "attempt to index childCount on a nil value");
            }
        }

        LuaScriptMgr.Push(L, obj.transform.childCount);
        return 1;
    }

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int get_transform(IntPtr L)
	{
		object o = LuaScriptMgr.GetLuaObject(L, 1);
		GameObject obj = (GameObject)o;

		if (obj == null)
        {
            Common.HobaDebuger.LogWarning("attempt to index transform on a nil value");
            LuaScriptMgr.Instance.CallOnTraceBack();
            LuaDLL.lua_pushnil(L);
        }
        else
            LuaScriptMgr.Push(L, obj.transform);
		return 1;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int get_layer(IntPtr L)
	{
		object o = LuaScriptMgr.GetLuaObject(L, 1);
		GameObject obj = (GameObject)o;

		if (obj == null)
		{
			LuaTypes types = LuaDLL.lua_type(L, 1);

			if (types == LuaTypes.LUA_TTABLE)
			{
				LuaDLL.luaL_error(L, "unknown member name layer");
			}
			else
			{
				LuaDLL.luaL_error(L, "attempt to index layer on a nil value");
			}
		}

		LuaScriptMgr.Push(L, obj.layer);
		return 1;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int get_activeSelf(IntPtr L)
	{
		object o = LuaScriptMgr.GetLuaObject(L, 1);
		GameObject obj = (GameObject)o;

		if (obj == null)
		{
			LuaTypes types = LuaDLL.lua_type(L, 1);

			if (types == LuaTypes.LUA_TTABLE)
			{
				LuaDLL.luaL_error(L, "unknown member name activeSelf");
			}
			else
			{
				LuaDLL.luaL_error(L, "attempt to index activeSelf on a nil value");
			}
		}

		LuaScriptMgr.Push(L, obj.activeSelf);
		return 1;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int get_activeInHierarchy(IntPtr L)
	{
		object o = LuaScriptMgr.GetLuaObject(L, 1);
		GameObject obj = (GameObject)o;

		if (obj == null)
		{
			LuaTypes types = LuaDLL.lua_type(L, 1);

			if (types == LuaTypes.LUA_TTABLE)
			{
				LuaDLL.luaL_error(L, "unknown member name activeInHierarchy");
			}
			else
			{
				LuaDLL.luaL_error(L, "attempt to index activeInHierarchy on a nil value");
			}
		}

		LuaScriptMgr.Push(L, obj.activeInHierarchy);
		return 1;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int get_isStatic(IntPtr L)
	{
		object o = LuaScriptMgr.GetLuaObject(L, 1);
		GameObject obj = (GameObject)o;

		if (obj == null)
		{
			LuaTypes types = LuaDLL.lua_type(L, 1);

			if (types == LuaTypes.LUA_TTABLE)
			{
				LuaDLL.luaL_error(L, "unknown member name isStatic");
			}
			else
			{
				LuaDLL.luaL_error(L, "attempt to index isStatic on a nil value");
			}
		}

		LuaScriptMgr.Push(L, obj.isStatic);
		return 1;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int get_tag(IntPtr L)
	{
		object o = LuaScriptMgr.GetLuaObject(L, 1);
		GameObject obj = (GameObject)o;

		if (obj == null)
		{
			LuaTypes types = LuaDLL.lua_type(L, 1);

			if (types == LuaTypes.LUA_TTABLE)
			{
				LuaDLL.luaL_error(L, "unknown member name tag");
			}
			else
			{
				LuaDLL.luaL_error(L, "attempt to index tag on a nil value");
			}
		}

		LuaScriptMgr.Push(L, obj.tag);
		return 1;
	}

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    static int get_name(IntPtr L)
    {
        object o = LuaScriptMgr.GetLuaObject(L, 1);
        GameObject obj = (GameObject)o;
        if (obj == null)
        {
            LuaTypes types = LuaDLL.lua_type(L, 1);

            if (types == LuaTypes.LUA_TTABLE)
            {
                LuaDLL.luaL_error(L, "unknown member name");
            }
            else
            {
                LuaDLL.luaL_error(L, "attempt to index tag on a nil value");
            }
        }
        LuaScriptMgr.Push(L, obj.name);
        return 1;
    }

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int get_gameObject(IntPtr L)
	{
		object o = LuaScriptMgr.GetLuaObject(L, 1);
		GameObject obj = (GameObject)o;

		if (obj == null)
		{
			LuaTypes types = LuaDLL.lua_type(L, 1);

			if (types == LuaTypes.LUA_TTABLE)
			{
				LuaDLL.luaL_error(L, "unknown member name gameObject");
			}
			else
			{
				LuaDLL.luaL_error(L, "attempt to index gameObject on a nil value");
			}
		}

		LuaScriptMgr.Push(L, obj.gameObject);
		return 1;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int set_layer(IntPtr L)
	{
		object o = LuaScriptMgr.GetLuaObject(L, 1);
		GameObject obj = (GameObject)o;

		if (obj == null)
		{
			LuaTypes types = LuaDLL.lua_type(L, 1);

			if (types == LuaTypes.LUA_TTABLE)
			{
				LuaDLL.luaL_error(L, "unknown member name layer");
			}
			else
			{
				LuaDLL.luaL_error(L, "attempt to index layer on a nil value");
			}
		}

		obj.layer = (int)LuaScriptMgr.GetNumber(L, 3);
		return 0;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int set_isStatic(IntPtr L)
	{
		object o = LuaScriptMgr.GetLuaObject(L, 1);
		GameObject obj = (GameObject)o;

		if (obj == null)
		{
			LuaTypes types = LuaDLL.lua_type(L, 1);

			if (types == LuaTypes.LUA_TTABLE)
			{
				LuaDLL.luaL_error(L, "unknown member name isStatic");
			}
			else
			{
				LuaDLL.luaL_error(L, "attempt to index isStatic on a nil value");
			}
		}

		obj.isStatic = LuaScriptMgr.GetBoolean(L, 3);
		return 0;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int set_tag(IntPtr L)
	{
		object o = LuaScriptMgr.GetLuaObject(L, 1);
		GameObject obj = (GameObject)o;

		if (obj == null)
		{
			LuaTypes types = LuaDLL.lua_type(L, 1);

			if (types == LuaTypes.LUA_TTABLE)
			{
				LuaDLL.luaL_error(L, "unknown member name tag");
			}
			else
			{
				LuaDLL.luaL_error(L, "attempt to index tag on a nil value");
			}
		}

		obj.tag = LuaScriptMgr.GetString(L, 3);
		return 0;
	}

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    static int set_name(IntPtr L)
    {
        object o = LuaScriptMgr.GetLuaObject(L, 1);
        GameObject obj = (GameObject)o;

        if (obj == null)
        {
            LuaTypes types = LuaDLL.lua_type(L, 1);

            if (types == LuaTypes.LUA_TTABLE)
            {
                LuaDLL.luaL_error(L, "unknown member name");
            }
            else
            {
                LuaDLL.luaL_error(L, "attempt to index tag on a nil value");
            }
        }

        obj.name = LuaScriptMgr.GetString(L, 3);
        return 0;
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    static int set_position(IntPtr L)
    {
        object o = LuaScriptMgr.GetLuaObject(L, 1);
        GameObject obj = (GameObject)o;

        if (obj == null)
        {
            LuaTypes types = LuaDLL.lua_type(L, 1);

            if (types == LuaTypes.LUA_TTABLE)
            {
                LuaDLL.luaL_error(L, "unknown member name position");
            }
            else
            {
                LuaDLL.luaL_error(L, "attempt to index position on a nil value");
            }
        }

        Transform transform = GameObjectUtil.GetGameObjTransform(obj);
        var pos = transform.position;
        LuaScriptMgr.SetVector3FromLua(L, 3, ref pos);
        transform.position = pos;

//         var pos = obj.transform.position;
//         LuaScriptMgr.SetVector3FromLua(L, 3, ref pos);
//         obj.transform.position = pos;
        return 0;
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    static int set_localPosition(IntPtr L)
    {
        object o = LuaScriptMgr.GetLuaObject(L, 1);
        GameObject obj = (GameObject)o;

        if (obj == null)
        {
            LuaTypes types = LuaDLL.lua_type(L, 1);

            if (types == LuaTypes.LUA_TTABLE)
            {
                LuaDLL.luaL_error(L, "unknown member name localPosition");
            }
            else
            {
                LuaDLL.luaL_error(L, "attempt to index localPosition on a nil value");

                LuaScriptMgr.Instance.CallOnTraceBack();
            }
        }

        Transform transform = GameObjectUtil.GetGameObjTransform(obj);
        var pos = transform.localPosition;
        LuaScriptMgr.SetVector3FromLua(L, 3, ref pos);
        transform.localPosition = pos;

//         var pos = obj.transform.localPosition;
//         LuaScriptMgr.SetVector3FromLua(L, 3, ref pos);
//         obj.transform.localPosition = pos;
        //obj.transform.localPosition = LuaScriptMgr.GetVector3(L, 3);
        return 0;
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    static int set_forward(IntPtr L)
    {
        object o = LuaScriptMgr.GetLuaObject(L, 1);
        GameObject obj = (GameObject)o;

        if (obj == null)
        {
            LuaTypes types = LuaDLL.lua_type(L, 1);

            if (types == LuaTypes.LUA_TTABLE)
            {
                LuaDLL.luaL_error(L, "unknown member name forward");
            }
            else
            {
                LuaDLL.luaL_error(L, "attempt to index forward on a nil value");
            }
        }

        Transform transform = GameObjectUtil.GetGameObjTransform(obj);
        var forward = transform.forward;
        LuaScriptMgr.SetVector3FromLua(L, 3, ref forward);
        if (!forward.IsZero())
            transform.forward = forward;

//         var forward = obj.transform.forward;
//         LuaScriptMgr.SetVector3FromLua(L, 3, ref forward);
//         if(forward != Vector3.zero)
//             obj.transform.forward = forward;

        //obj.transform.forward = LuaScriptMgr.GetVector3(L, 3);
        return 0;
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    static int set_rotation(IntPtr L)
    {
        object o = LuaScriptMgr.GetLuaObject(L, 1);
        GameObject obj = (GameObject)o;

        if (obj == null)
        {
            LuaTypes types = LuaDLL.lua_type(L, 1);

            if (types == LuaTypes.LUA_TTABLE)
            {
                LuaDLL.luaL_error(L, "unknown member name rotation");
            }
            else
            {
                LuaDLL.luaL_error(L, "attempt to index rotation on a nil value");
            }
        }

        Transform transform = GameObjectUtil.GetGameObjTransform(obj);
        var rotation = transform.rotation;
        LuaScriptMgr.SetQuaternionFromLua(L, 3, ref rotation);
        transform.rotation = rotation;

//         var rotation = obj.transform.rotation;
//         LuaScriptMgr.SetQuaternionFromLua(L, 3, ref rotation);
//         obj.transform.rotation = rotation;
        
        //obj.transform.rotation = LuaScriptMgr.GetQuaternion(L, 3);
        return 0;
    }


    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    static int set_localRotation(IntPtr L)
    {
        object o = LuaScriptMgr.GetLuaObject(L, 1);
        GameObject obj = (GameObject)o;

        if (obj == null)
        {
            LuaTypes types = LuaDLL.lua_type(L, 1);

            if (types == LuaTypes.LUA_TTABLE)
            {
                LuaDLL.luaL_error(L, "unknown member name localRotation");
            }
            else
            {
                LuaDLL.luaL_error(L, "attempt to index localRotation on a nil value");
            }
        }

        Transform transform = GameObjectUtil.GetGameObjTransform(obj);
        var localRotation = transform.localRotation;
        LuaScriptMgr.SetQuaternionFromLua(L, 3, ref localRotation);
        transform.localRotation = localRotation;

//         var localRotation = obj.transform.localRotation;
//         LuaScriptMgr.SetQuaternionFromLua(L, 3, ref localRotation);
//         obj.transform.localRotation = localRotation;
        //obj.transform.localRotation = LuaScriptMgr.GetQuaternion(L, 3);
        return 0;
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    static int set_localScale(IntPtr L)
    {
        object o = LuaScriptMgr.GetLuaObject(L, 1);
        GameObject obj = (GameObject)o;

        if (obj == null)
        {
            LuaTypes types = LuaDLL.lua_type(L, 1);

            if (types == LuaTypes.LUA_TTABLE)
            {
                LuaDLL.luaL_error(L, "unknown member name localScale");
            }
            else
            {
                LuaDLL.luaL_error(L, "attempt to index localScale on a nil value");
            }
        }

        Transform transform = GameObjectUtil.GetGameObjTransform(obj);
        var localScale = transform.localScale;
        LuaScriptMgr.SetVector3FromLua(L, 3, ref localScale);
        transform.localScale = localScale;

//         var localScale = obj.transform.localScale;
//         LuaScriptMgr.SetVector3FromLua(L, 3, ref localScale);
//         obj.transform.localScale = localScale;
        //obj.transform.localScale = LuaScriptMgr.GetVector3(L, 3);
        return 0;
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    static int set_parent(IntPtr L)
    {
        object o = LuaScriptMgr.GetLuaObject(L, 1);
        GameObject obj = (GameObject)o;

        if (obj == null)
        {
            LuaTypes types = LuaDLL.lua_type(L, 1);

            if (types == LuaTypes.LUA_TTABLE)
            {
                LuaDLL.luaL_error(L, "unknown member name parent");
            }
            else
            {
                LuaDLL.luaL_error(L, "attempt to index parent on a nil value");
            }
        }

        if(LuaDLL.lua_isnil(L, 2))
        {
            Transform transform = GameObjectUtil.GetGameObjTransform(obj);
            transform.parent = null;

            //obj.transform.parent = null;
        }
        else
        {
            Transform transform = GameObjectUtil.GetGameObjTransform(obj);
            GameObject arg0 = (GameObject)LuaScriptMgr.GetUnityObject(L, 3, typeof(GameObject));
            Transform transform_Arg0 = GameObjectUtil.GetGameObjTransform(arg0);

            transform.parent = arg0 != null ? transform_Arg0 : null;
            //obj.transform.parent = arg0 != null ? arg0.transform : null;
        }
        
        return 0;
    }

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int GetComponent(IntPtr L)
	{
		int count = LuaDLL.lua_gettop(L);

		if (count == 2 && LuaScriptMgr.CheckTypes(L, 2, typeof(int)))
		{
			GameObject obj = (GameObject)LuaScriptMgr.GetUnityObjectSelf(L, 1, "GameObject");
			var classId = (int)LuaScriptMgr.GetNumber(L, 2);
            var t = WrapClassID.GetClassType(classId);
            if(t == null)
            {
                LuaDLL.luaL_error(L, "invalid arguments to method: GameObject.GetComponent");
            }
            else
            {
                
                Component o = obj.GetComponent(t);
                LuaScriptMgr.Push(L, o);
                return 1;
            }
		}
		else
		{
			LuaDLL.luaL_error(L, "invalid arguments to method: GameObject.GetComponent");
		}

		return 0;
	}

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    static int GetCurAnimation(IntPtr L)
    {
        LuaScriptMgr.CheckArgsCount(L, 1);
        GameObject obj = (GameObject)LuaScriptMgr.GetUnityObjectSelf(L, 1, "GameObject");

        if (obj != null)
        {
            var animation = obj.GetComponent<Animation>();
            foreach (AnimationState anim in animation)
            {
                if (animation.IsPlaying(anim.name))
                {
                    LuaScriptMgr.Push(L, anim.name);
                    return 1;
                }
            }
        }

        LuaScriptMgr.Push(L, string.Empty);
        return 1;
    }


    [MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int GetComponentInChildren(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 2);
		GameObject obj = (GameObject)LuaScriptMgr.GetUnityObjectSelf(L, 1, "GameObject");

        var classId = (int)LuaScriptMgr.GetNumber(L, 2);
        var t = WrapClassID.GetClassType(classId);
        if (t == null)
        {
            LuaDLL.luaL_error(L, "invalid arguments to method: GameObject.GetComponent");
            return 0;
        }
        else
        {
            Component o = obj.GetComponentInChildren(t);
            LuaScriptMgr.Push(L, o);
            return 1;
        }
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int GetComponents(IntPtr L)
	{
		int count = LuaDLL.lua_gettop(L);

		if (count == 2)
		{
            GameObject obj = (GameObject)LuaScriptMgr.GetUnityObjectSelf(L, 1, "GameObject");
            var classId = (int)LuaScriptMgr.GetNumber(L, 2);
            var t = WrapClassID.GetClassType(classId);
            if (t == null)
            {
                LuaDLL.luaL_error(L, "invalid arguments to method: GameObject.GetComponent");
                return 0;
            }
            else
            {
                Component[] o = obj.GetComponents(t);
                LuaScriptMgr.PushArray(L, o);
                return 1;
            }
		}
		else
		{
			LuaDLL.luaL_error(L, "invalid arguments to method: GameObject.GetComponents");
		}

		return 0;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int GetComponentsInChildren(IntPtr L)
	{
		int count = LuaDLL.lua_gettop(L);

		if (count == 2)
		{
			GameObject obj = (GameObject)LuaScriptMgr.GetUnityObjectSelf(L, 1, "GameObject");
            var classId = (int)LuaScriptMgr.GetNumber(L, 2);
            var arg0 = WrapClassID.GetClassType(classId);
            Component[] o = obj.GetComponentsInChildren(arg0);
			LuaScriptMgr.PushArray(L, o);
			return 1;
		}
		else
		{
			LuaDLL.luaL_error(L, "invalid arguments to method: GameObject.GetComponentsInChildren");
		}

		return 0;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int SetActive(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 2);
		GameObject obj = (GameObject)LuaScriptMgr.GetUnityObjectSelf(L, 1, "GameObject");
		bool arg0 = LuaScriptMgr.GetBoolean(L, 2);
        if(obj.activeSelf != arg0)
            obj.SetActive(arg0);
		return 0;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int AddComponent(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 2);
		GameObject obj = (GameObject)LuaScriptMgr.GetUnityObjectSelf(L, 1, "GameObject");
        //Type arg0 = LuaScriptMgr.GetTypeObject(L, 2);
        var classId = (int)LuaScriptMgr.GetNumber(L, 2);
        var arg0 = WrapClassID.GetClassType(classId);
        Component o = obj.AddComponent(arg0);
		LuaScriptMgr.Push(L, o);
		return 1;
	}

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    static int SmartAddComponent(IntPtr L)
    {
        LuaScriptMgr.CheckArgsCount(L, 2);
        GameObject obj = (GameObject)LuaScriptMgr.GetUnityObjectSelf(L, 1, "GameObject");
        var classId = (int)LuaScriptMgr.GetNumber(L, 2);
        var arg0 = WrapClassID.GetClassType(classId);
        var o = obj.GetComponent(arg0);
        if(o == null)
            o = obj.AddComponent(arg0);
        LuaScriptMgr.Push(L, o);
        return 1;
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int Find(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 1);
		string arg0 = LuaScriptMgr.GetLuaString(L, 1);
		GameObject o = GameObject.Find(arg0);
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

