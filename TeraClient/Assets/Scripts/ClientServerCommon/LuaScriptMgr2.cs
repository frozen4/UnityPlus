using System;
using System.Collections.Generic;
using LuaInterface;
using UnityEngine;
using System.Diagnostics;
using System.Collections;
using Common;

public partial class LuaScriptMgr
{
     //get, push 相关，这些独立于ScriptMgr逻辑，可能用XLua等其他实现替换的部分

    public static double GetNumber(IntPtr L, int stackPos)
    {
        if (LuaDLL.lua_isnumber(L, stackPos))
        {
            return LuaDLL.lua_tonumber(L, stackPos);
        }

        LuaDLL.luaL_typerror(L, stackPos, "number");
        return 0;
    }

    public static bool GetBoolean(IntPtr L, int stackPos)
    {
        if (LuaDLL.lua_isboolean(L, stackPos))
        {
            return LuaDLL.lua_toboolean(L, stackPos);
        }

        LuaDLL.luaL_typerror(L, stackPos, "boolean");
        return false;
    }

    public static string GetString(IntPtr L, int stackPos)
    {
        string str = GetLuaString(L, stackPos);

        if (str == null)
        {
            LuaDLL.luaL_typerror(L, stackPos, "string");
        }

        return str;
    }

    private static LuaFunction GetFunction(IntPtr L, int stackPos)
    {
        LuaTypes luatype = LuaDLL.lua_type(L, stackPos);

        if (luatype != LuaTypes.LUA_TFUNCTION)
        {
            return null;
        }

        LuaDLL.lua_pushvalue(L, stackPos);
        return new LuaFunction(LuaDLL.luaL_ref(L, LuaIndexes.LUA_REGISTRYINDEX), GetTranslator(L).interpreter);
    }

    public static LuaFunction ToLuaFunction(IntPtr L, int stackPos)
    {
        LuaDLL.lua_pushvalue(L, stackPos);
        return new LuaFunction(LuaDLL.luaL_ref(L, LuaIndexes.LUA_REGISTRYINDEX), GetTranslator(L).interpreter);
    }

    public static LuaFunction GetLuaFunction(IntPtr L, int stackPos)
    {
        LuaFunction func = GetFunction(L, stackPos);
        LuaTypes luatype = LuaDLL.lua_type(L, stackPos);

        if (func == null && luatype != LuaTypes.LUA_TNIL)
        {
            LuaDLL.luaL_typerror(L, stackPos, "function");
            return null;
        }

        return func;
    }

    static LuaTable ToLuaTable(IntPtr L, int stackPos)
    {
        LuaDLL.lua_pushvalue(L, stackPos);
        return new LuaTable(LuaDLL.luaL_ref(L, LuaIndexes.LUA_REGISTRYINDEX), GetTranslator(L).interpreter);
    }

    static LuaTable GetTable(IntPtr L, int stackPos)
    {
        LuaTypes luatype = LuaDLL.lua_type(L, stackPos);

        if (luatype != LuaTypes.LUA_TTABLE)
        {
            return null;
        }

        LuaDLL.lua_pushvalue(L, stackPos);
        return new LuaTable(LuaDLL.luaL_ref(L, LuaIndexes.LUA_REGISTRYINDEX), GetTranslator(L).interpreter);
    }

    public static LuaTable GetLuaTable(IntPtr L, int stackPos)
    {
        LuaTable table = GetTable(L, stackPos);

        if (table == null)
        {
            LuaDLL.luaL_typerror(L, stackPos, "table");
            return null;
        }

        return table;
    }

    //注册到lua中的object类型对象, 存放在ObjectTranslator objects 池中
    public static object GetLuaObject(IntPtr L, int stackPos)
    {
        return GetTranslator(L).getRawNetObject(L, stackPos);
    }

    //System object类型匹配正确, 只需判断会否为null. 获取对象本身时使用
    public static object GetNetObjectSelf(IntPtr L, int stackPos, string type)
    {
        object obj = GetTranslator(L).getRawNetObject(L, stackPos);

        if (obj == null)
        {
            LuaDLL.luaL_argerror(L, stackPos, HobaText.Format("{0} expected, got nil", type));
            return null;
        }

        return obj;
    }

    //Unity object类型匹配正确, 只需判断会否为null. 获取对象本身时使用
    public static object GetUnityObjectSelf(IntPtr L, int stackPos, string type)
    {
        object obj = GetTranslator(L).getRawNetObject(L, stackPos);
        UnityEngine.Object uObj = (UnityEngine.Object)obj;

        if (uObj == null)
        {
            LuaDLL.luaL_argerror(L, stackPos, HobaText.Format("{0} expected, got nil", type));
            return null;
        }

        return obj;
    }

    public static object GetTrackedObjectSelf(IntPtr L, int stackPos, string type)
    {
        object obj = GetTranslator(L).getRawNetObject(L, stackPos);
        UnityEngine.TrackedReference uObj = (UnityEngine.TrackedReference)obj;

        if (uObj == null)
        {
            LuaDLL.luaL_argerror(L, stackPos, HobaText.Format("{0} expected, got nil", type));
            return null;
        }

        return obj;
    }

    public static T GetNetObject<T>(IntPtr L, int stackPos)
    {
        return (T)GetNetObject(L, stackPos, typeof(T));
    }

    public static object GetNetObject(IntPtr L, int stackPos, Type type)
    {
        if (LuaDLL.lua_isnil(L, stackPos))
        {
            return null;
        }

        object obj = GetLuaObject(L, stackPos);


        if (obj == null)
        {
            LuaDLL.luaL_argerror(L, stackPos, HobaText.Format("{0} expected, got nil", type.Name));
            return null;
        }

        Type objType = obj.GetType();

        if (type == objType || type.IsAssignableFrom(objType))
        {
            return obj;
        }

        LuaDLL.luaL_argerror(L, stackPos, HobaText.Format("{0} expected, got {1}", type.Name, objType.Name));
        return null;
    }

    public static T GetUnityObject<T>(IntPtr L, int stackPos) where T : UnityEngine.Object
    {
        return (T)GetUnityObject(L, stackPos, typeof(T));
    }

    public static UnityEngine.Object GetUnityObject(IntPtr L, int stackPos, Type type)
    {
        if (LuaDLL.lua_isnil(L, stackPos))
        {
            return null;
        }

        object obj = GetLuaObject(L, stackPos);

        if (obj == null)
        {
            LuaDLL.luaL_argerror(L, stackPos, HobaText.Format("{0} expected, got nil", type.Name));
            return null;
        }

        UnityEngine.Object uObj = (UnityEngine.Object)obj; // as UnityEngine.Object;        

        if (uObj == null)
        {
            LuaDLL.luaL_argerror(L, stackPos, HobaText.Format("{0} expected, got nil", type.Name));
            return null;
        }

        Type objType = uObj.GetType();

        if (type == objType || objType.IsSubclassOf(type))
        {
            return uObj;
        }

        LuaDLL.luaL_argerror(L, stackPos, HobaText.Format("{0} expected, got {1}", type.Name, objType.Name));
        return null;
    }

    public static T GetTrackedObject<T>(IntPtr L, int stackPos) where T : UnityEngine.TrackedReference
    {
        return (T)GetTrackedObject(L, stackPos, typeof(T));
    }

    public static UnityEngine.TrackedReference GetTrackedObject(IntPtr L, int stackPos, Type type)
    {
        if (LuaDLL.lua_isnil(L, stackPos))
        {
            return null;
        }

        object obj = GetLuaObject(L, stackPos);

        if (obj == null)
        {
            LuaDLL.luaL_argerror(L, stackPos, HobaText.Format("{0} expected, got nil", type.Name));
            return null;
        }

        UnityEngine.TrackedReference uObj = obj as UnityEngine.TrackedReference;

        if (uObj == null)
        {
            LuaDLL.luaL_argerror(L, stackPos, HobaText.Format("{0} expected, got nil", type.Name));
            return null;
        }

        Type objType = obj.GetType();

        if (type == objType || objType.IsSubclassOf(type))
        {
            return uObj;
        }

        LuaDLL.luaL_argerror(L, stackPos, HobaText.Format("{0} expected, got {1}", type.Name, objType.Name));
        return null;
    }

    public static Type GetTypeObject(IntPtr L, int stackPos)
    {
        object obj = GetLuaObject(L, stackPos);

        if (obj == null || obj.GetType() != monoType)
        {
            LuaDLL.luaL_argerror(L, stackPos, HobaText.Format("Type expected, got {0}", obj == null ? "nil" : obj.GetType().Name));
        }

        return (Type)obj;
    }

    //压入一个object变量
    public static void PushVarObject(IntPtr L, object o)
    {
        if (o == null)
        {
            LuaDLL.lua_pushnil(L);
            return;
        }

        Type t = o.GetType();

        if (t.IsValueType)
        {
            if (t == typeof(bool))
            {
                bool b = (bool)o;
                LuaDLL.lua_pushboolean(L, b);
            }
            else if (t.IsEnum)
            {
                Push(L, (System.Enum)o);
            }
            else if (t.IsPrimitive)
            {
                double d = Convert.ToDouble(o);
                LuaDLL.lua_pushnumber(L, d);
            }
            else if (t == typeof(Vector3))
            {
                Push(L, (Vector3)o);
            }
            else if (t == typeof(Vector2))
            {
                Push(L, (Vector2)o);
            }
            else if (t == typeof(Vector4))
            {
                Push(L, (Vector4)o);
            }
            else if (t == typeof(Quaternion))
            {
                Push(L, (Quaternion)o);
            }
            else if (t == typeof(Color))
            {
                Push(L, (Color)o);
            }
            else if (t == typeof(RaycastHit))
            {
                Push(L, (RaycastHit)o);
            }
            else if (t == typeof(Touch))
            {
                Push(L, (Touch)o);
            }
            else if (t == typeof(Ray))
            {
                Push(L, (Ray)o);
            }
            else
            {
                PushValue(L, o);
            }
        }
        else
        {
            if (t.IsArray)
            {
                PushArray(L, (System.Array)o);
            }
            else if (t.IsSubclassOf(typeof(Delegate)))
            {
                Push(L, (Delegate)o);
            }
            else if (IsClassOf(t, typeof(IEnumerator)))
            {
                Push(L, (IEnumerator)o);
            }
            else if (t == typeof(string))
            {
                string str = (string)o;
                LuaDLL.lua_pushstring(L, str);
            }
            else if (t == typeof(LuaStringBuffer))
            {
                LuaStringBuffer lsb = (LuaStringBuffer)o;
                LuaDLL.lua_pushlstring(L, lsb.buffer, lsb.buffer.Length);
            }
            else if (t.IsSubclassOf(typeof(UnityEngine.Object)))
            {
                UnityEngine.Object obj = (UnityEngine.Object)o;

                if (obj == null)
                {
                    LuaDLL.lua_pushnil(L);
                }
                else
                {
                    PushObject(L, o);
                }
            }
            else if (t == typeof(LuaTable))
            {
                ((LuaTable)o).push(L);
            }
            else if (t == typeof(LuaFunction))
            {
                ((LuaFunction)o).push(L);
            }
            else if (t == typeof(LuaCSFunction))
            {
                GetTranslator(L).pushFunction(L, (LuaCSFunction)o);
            }
            else if (t == monoType)
            {
                Push(L, (Type)o);
            }
            else if (t.IsSubclassOf(typeof(TrackedReference)))
            {
                UnityEngine.TrackedReference obj = (UnityEngine.TrackedReference)o;

                if (obj == null)
                {
                    LuaDLL.lua_pushnil(L);
                }
                else
                {
                    PushObject(L, o);
                }
            }
            else
            {
                PushObject(L, o);
            }
        }
    }

    //压入一个从object派生的变量
    public static void PushObject(IntPtr L, object o)
    {
        GetTranslator(L).PushObject(L, o, "luaNet_metatable");
    }

    public static void Push(IntPtr L, UnityEngine.Object obj)
    {
        PushObject(L, obj == null ? null : obj);
    }

    public static void Push(IntPtr L, TrackedReference obj)
    {
        PushObject(L, obj == null ? null : obj);
    }

    static void PushMetaObject(IntPtr L, ObjectTranslator translator, object o, int metaRef)
    {
        if (o == null)
        {
            LuaDLL.lua_pushnil(L);
            return;
        }

        int weakTableRef = translator.weakTableRef;
        int index = -1;
        bool found = translator.objectsBackMap.TryGetValue(o, out index);

        if (found)
        {
            if (LuaDLL.tolua_pushudata(L, weakTableRef, index))
            {
                return;
            }

            translator.collectObject(index);
        }

        index = translator.addObject(o, false);
        LuaDLL.tolua_pushnewudata(L, metaRef, weakTableRef, index);
    }

    public static void Push(IntPtr L, Type o)
    {
        LuaScriptMgr mgr = GetMgrFromLuaState(L);
        ObjectTranslator translator = mgr.GetLuaState().translator;
        PushMetaObject(L, translator, o, mgr.typeMetaRef);
    }

    public static void Push(IntPtr L, Delegate o)
    {
        LuaScriptMgr mgr = GetMgrFromLuaState(L);
        ObjectTranslator translator = mgr.GetLuaState().translator;
        PushMetaObject(L, translator, o, mgr.delegateMetaRef);
    }

    public static void Push(IntPtr L, IEnumerator o)
    {
        LuaScriptMgr mgr = GetMgrFromLuaState(L);
        ObjectTranslator translator = mgr.GetLuaState().translator;
        PushMetaObject(L, translator, o, mgr.iterMetaRef);
    }

    public static void PushArray(IntPtr L, System.Array o)
    {
        LuaScriptMgr mgr = GetMgrFromLuaState(L);
        ObjectTranslator translator = mgr.GetLuaState().translator;
        PushMetaObject(L, translator, o, mgr.arrayMetaRef);
    }

    public static void PushValue(IntPtr L, object obj)
    {
        GetTranslator(L).PushValueResult(L, obj);
    }

    public static void Push(IntPtr L, bool b)
    {
        LuaDLL.lua_pushboolean(L, b);
    }

    public static void Push(IntPtr L, string str)
    {
        LuaDLL.lua_pushstring(L, str);
    }

    public static void Push(IntPtr L, char d)
    {
        LuaDLL.lua_pushinteger(L, d);
    }

    public static void Push(IntPtr L, sbyte d)
    {
        int i = Convert.ToInt32(d);
        LuaDLL.lua_pushinteger(L, i);
    }

    public static void Push(IntPtr L, byte d)
    {
        int i = Convert.ToInt32(d);
        LuaDLL.lua_pushinteger(L, i);
    }

    public static void Push(IntPtr L, short d)
    {
        int i = Convert.ToInt32(d);
        LuaDLL.lua_pushinteger(L, i);
    }

    public static void Push(IntPtr L, ushort d)
    {
        int i = Convert.ToInt32(d);
        LuaDLL.lua_pushinteger(L, i);
    }

    public static void Push(IntPtr L, int d)
    {
        LuaDLL.lua_pushinteger(L, d);
    }

    public static void Push(IntPtr L, uint d)
    {
        LuaDLL.lua_pushnumber(L, d);
    }

    public static void Push(IntPtr L, long d)
    {
        LuaDLL.lua_pushnumber(L, d);
    }

    public static void Push(IntPtr L, ulong d)
    {
        LuaDLL.lua_pushnumber(L, d);
    }

    public static void Push(IntPtr L, float d)
    {
        double dbl = Convert.ToDouble(d);
        LuaDLL.lua_pushnumber(L, dbl);
    }

    public static void Push(IntPtr L, decimal d)
    {
        double dbl = Convert.ToDouble(d);
        LuaDLL.lua_pushnumber(L, dbl);
    }

    public static void Push(IntPtr L, double d)
    {
        LuaDLL.lua_pushnumber(L, d);
    }

    public static void Push(IntPtr L, IntPtr p)
    {
        LuaDLL.lua_pushlightuserdata(L, p);
    }

    public static void Push(IntPtr L, ILuaGeneratedType o)
    {
        if (o == null)
        {
            LuaDLL.lua_pushnil(L);
        }
        else
        {
            LuaTable table = o.__luaInterface_getLuaTable();
            table.push(L);
        }
    }

    public static void Push(IntPtr L, LuaTable table)
    {
        if (table == null)
        {
            LuaDLL.lua_pushnil(L);
        }
        else
        {
            table.push(L);
        }
    }

    public static void Push(IntPtr L, LuaFunction func)
    {
        if (func == null)
        {
            LuaDLL.lua_pushnil(L);
        }
        else
        {
            func.push();
        }
    }

    public static void Push(IntPtr L, LuaCSFunction func)
    {
        if (func == null)
        {
            LuaDLL.lua_pushnil(L);
            return;
        }

        GetTranslator(L).pushFunction(L, func);
    }

    public static object[] GetParamsObject(IntPtr L, int stackPos, int count)
    {
        //ObjectTranslator translator = GetTranslator(L);
        List<object> list = new List<object>();
        object obj = null;

        while (count > 0)
        {
            obj = GetVarObject(L, stackPos);

            ++stackPos;
            --count;

            if (obj != null)
            {
                list.Add(obj);
            }
            else
            {
                LuaDLL.luaL_argerror(L, stackPos, "object expected, got nil");
                break;
            }
        }

        return list.ToArray();
    }

    public static T[] GetParamsObject<T>(IntPtr L, int stackPos, int count)
    {
        List<T> list = new List<T>();
        T obj = default(T);

        while (count > 0)
        {
            object tmp = GetLuaObject(L, stackPos);

            ++stackPos;
            --count;

            if (tmp != null && tmp.GetType() == typeof(T))
            {
                obj = (T)tmp;
                list.Add(obj);
            }
            else
            {
                LuaDLL.luaL_argerror(L, stackPos, HobaText.Format("{0} expected, got nil", typeof(T).Name));
                break;
            }
        }

        return list.ToArray();
    }

    public static T[] GetArrayObject<T>(IntPtr L, int stackPos)
    {
        //ObjectTranslator translator = GetTranslator(L);
        LuaTypes luatype = LuaDLL.lua_type(L, stackPos);

        if (luatype == LuaTypes.LUA_TTABLE)
        {
            int index = 1;
            T val = default(T);
            List<T> list = new List<T>();
            LuaDLL.lua_pushvalue(L, stackPos);
            Type t = typeof(T);

            do
            {
                LuaDLL.lua_rawgeti(L, -1, index);
                luatype = LuaDLL.lua_type(L, -1);

                if (luatype == LuaTypes.LUA_TNIL)
                {
                    LuaDLL.lua_pop(L, 1);
                    break;
                }
                else if (!CheckType(L, t, -1))
                {
                    LuaDLL.lua_pop(L, 1);
                    break;
                }

                val = (T)GetVarObject(L, -1);
                list.Add(val);
                LuaDLL.lua_pop(L, 1);
                ++index;
            } while (true);

            return list.ToArray();
        }
        else if (luatype == LuaTypes.LUA_TUSERDATA)
        {
            T[] ret = GetNetObject<T[]>(L, stackPos);

            if (ret != null)
            {
                return (T[])ret;
            }
        }
        else if (luatype == LuaTypes.LUA_TNIL)
        {
            return null;
        }

        LuaDLL.luaL_error(L, HobaText.Format("invalid arguments to method: {0}, pos {1}", GetErrorFunc(1), stackPos));
        return null;
    }

    static string GetErrorFunc(int skip)
    {
        StackFrame sf = null;
        string file = string.Empty;
        StackTrace st = new StackTrace(skip, true);
        int pos = 0;

        do
        {
            sf = st.GetFrame(pos++);
            file = sf.GetFileName();
        } while (!file.Contains("Wrap"));

        int index1 = file.LastIndexOf('\\');
        int index2 = file.LastIndexOf("Wrap");
        string className = file.Substring(index1 + 1, index2 - index1 - 1);
        return HobaText.Format("{0}.{1}", className, sf.GetMethod().Name);
    }

    public static string GetLuaString(IntPtr L, int stackPos)
    {
        LuaTypes luatype = LuaDLL.lua_type(L, stackPos);
        string retVal = null;

        if (luatype == LuaTypes.LUA_TSTRING)
        {
            retVal = LuaDLL.lua_tostring(L, stackPos);
        }
        else if (luatype == LuaTypes.LUA_TUSERDATA)
        {
            object obj = GetLuaObject(L, stackPos);

            if (obj == null)
            {
                LuaDLL.luaL_argerror(L, stackPos, "string expected, got nil");
                return string.Empty;
            }

            if (obj.GetType() == typeof(string))
            {
                retVal = (string)obj;
            }
            else
            {
                retVal = obj.ToString();
            }
        }
        else if (luatype == LuaTypes.LUA_TNUMBER)
        {
            double d = LuaDLL.lua_tonumber(L, stackPos);
            retVal = d.ToString();
        }
        else if (luatype == LuaTypes.LUA_TBOOLEAN)
        {
            bool b = LuaDLL.lua_toboolean(L, stackPos);
            retVal = b.ToString();
        }
        else if (luatype == LuaTypes.LUA_TNIL)
        {
            return retVal;
        }
        else
        {
            LuaDLL.lua_getglobal(L, "tostring");
            LuaDLL.lua_pushvalue(L, stackPos);
            LuaDLL.lua_call(L, 1, 1);
            retVal = LuaDLL.lua_tostring(L, -1);
            LuaDLL.lua_pop(L, 1);
        }

        return retVal;
    }

    public static string[] GetParamsString(IntPtr L, int stackPos, int count)
    {
        List<string> list = new List<string>();
        string obj = null;

        while (count > 0)
        {
            obj = GetLuaString(L, stackPos);
            ++stackPos;
            --count;

            if (obj == null)
            {
                LuaDLL.luaL_argerror(L, stackPos, "string expected, got nil");
                break;
            }

            list.Add(obj);
        }

        return list.ToArray();
    }

    public static string[] GetArrayString(IntPtr L, int stackPos)
    {
        LuaTypes luatype = LuaDLL.lua_type(L, stackPos);

        if (luatype == LuaTypes.LUA_TTABLE)
        {
            int index = 1;
            string retVal = null;
            List<string> list = new List<string>();
            LuaDLL.lua_pushvalue(L, stackPos);

            while (true)
            {
                LuaDLL.lua_rawgeti(L, -1, index);
                luatype = LuaDLL.lua_type(L, -1);

                if (luatype == LuaTypes.LUA_TNIL)
                {
                    LuaDLL.lua_pop(L, 1);
                    break;
                }
                else
                {
                    retVal = GetLuaString(L, -1);
                }

                list.Add(retVal);
                LuaDLL.lua_pop(L, 1);
                ++index;
            }

            LuaDLL.lua_pop(L, 1);
            return list.ToArray();
        }
        else if (luatype == LuaTypes.LUA_TUSERDATA)
        {
            string[] ret = GetNetObject<string[]>(L, stackPos);

            if (ret != null)
            {
                return (string[])ret;
            }
        }

        LuaDLL.luaL_error(L, HobaText.Format("invalid arguments to method: {0}, pos {1}", GetErrorFunc(1), stackPos));
        return null;
    }

    public static T[] GetArrayNumber<T>(IntPtr L, int stackPos)
    {
        LuaTypes luatype = LuaDLL.lua_type(L, stackPos);

        if (luatype == LuaTypes.LUA_TTABLE)
        {
            int index = 1;
            T ret = default(T);
            List<T> list = new List<T>();
            LuaDLL.lua_pushvalue(L, stackPos);

            while (true)
            {
                LuaDLL.lua_rawgeti(L, -1, index);
                luatype = LuaDLL.lua_type(L, -1);

                if (luatype == LuaTypes.LUA_TNIL)
                {
                    LuaDLL.lua_pop(L, 1);
                    break;
                }
                else if (luatype != LuaTypes.LUA_TNUMBER)
                {
                    LuaDLL.lua_pop(L, 1);
                    break;
                }

                ret = (T)Convert.ChangeType(LuaDLL.lua_tonumber(L, -1), typeof(T));
                list.Add(ret);
                LuaDLL.lua_pop(L, 1);
                ++index;
            }

            LuaDLL.lua_pop(L, 1);
            return list.ToArray();
        }
        else if (luatype == LuaTypes.LUA_TUSERDATA)
        {
            T[] ret = GetNetObject<T[]>(L, stackPos);

            if (ret != null)
            {
                return (T[])ret;
            }
        }

        LuaDLL.luaL_error(L, HobaText.Format("invalid arguments to method: {0}, pos {1}", GetErrorFunc(1), stackPos));
        return null;
    }

    public static bool[] GetArrayBool(IntPtr L, int stackPos)
    {
        LuaTypes luatype = LuaDLL.lua_type(L, stackPos);

        if (luatype == LuaTypes.LUA_TTABLE)
        {
            int index = 1;
            List<bool> list = new List<bool>();
            LuaDLL.lua_pushvalue(L, stackPos);

            while (true)
            {
                LuaDLL.lua_rawgeti(L, -1, index);
                luatype = LuaDLL.lua_type(L, -1);

                if (luatype == LuaTypes.LUA_TNIL)
                {
                    LuaDLL.lua_pop(L, 1);
                    break;
                }
                else if (luatype != LuaTypes.LUA_TBOOLEAN)
                {
                    LuaDLL.lua_pop(L, 1);
                    break;
                }

                bool ret = LuaDLL.lua_toboolean(L, -1);
                list.Add(ret);
                LuaDLL.lua_pop(L, 1);
                ++index;
            }

            LuaDLL.lua_pop(L, 1);
            return list.ToArray();
        }
        else if (luatype == LuaTypes.LUA_TUSERDATA)
        {
            bool[] ret = GetNetObject<bool[]>(L, stackPos);

            if (ret != null)
            {
                return (bool[])ret;
            }
        }

        LuaDLL.luaL_error(L, HobaText.Format("invalid arguments to method: {0}, pos {1}", GetErrorFunc(1), stackPos));
        return null;
    }

    public static LuaStringBuffer GetStringBuffer(IntPtr L, int stackPos)
    {
        LuaTypes luatype = LuaDLL.lua_type(L, stackPos);

        if (luatype == LuaTypes.LUA_TNIL)
        {
            return null;
        }
        else if (luatype != LuaTypes.LUA_TSTRING)
        {
            LuaDLL.luaL_typerror(L, stackPos, "string");
            return null;
        }

        int len = 0;
        IntPtr buffer = LuaDLL.lua_tolstring(L, stackPos, out len);
        return new LuaStringBuffer(buffer, (int)len);
    }

    public static void SetValueObject(IntPtr L, int pos, object obj)
    {
        GetTranslator(L).SetValueObject(L, pos, obj);
    }

    public static void CheckArgsCount(IntPtr L, int count)
    {
        int c = LuaDLL.lua_gettop(L);

        if (c != count)
        {
            string str = HobaText.Format("no overload for method '{0}' takes '{1}' arguments", GetErrorFunc(1), c);
            LuaDLL.luaL_error(L, str);
        }
    }

    public static object GetVarTable(IntPtr L, int stackPos)
    {
        object o = null;
        int oldTop = LuaDLL.lua_gettop(L);
        LuaDLL.lua_pushvalue(L, stackPos);
        LuaDLL.lua_pushstring(L, "class");
        LuaDLL.lua_gettable(L, -2);

        if (LuaDLL.lua_isnil(L, -1))
        {
            LuaDLL.lua_settop(L, oldTop);
            LuaDLL.lua_pushvalue(L, stackPos);
            o = new LuaTable(LuaDLL.luaL_ref(L, LuaIndexes.LUA_REGISTRYINDEX), GetTranslator(L).interpreter);
        }
        else
        {
            string cls = LuaDLL.lua_tostring(L, -1);
            LuaDLL.lua_settop(L, oldTop);

            stackPos = stackPos > 0 ? stackPos : stackPos + oldTop + 1;

            if (cls == "Vector3")
            {
                o = GetVector3(L, stackPos);
            }
            else if (cls == "Vector2")
            {
                o = GetVector2(L, stackPos);
            }
            else if (cls == "Quaternion")
            {
                o = GetQuaternion(L, stackPos);
            }
            else if (cls == "Color")
            {
                o = GetColor(L, stackPos);
            }
            else if (cls == "Vector4")
            {
                o = GetVector4(L, stackPos);
            }
            else if (cls == "Ray")
            {
                o = GetRay(L, stackPos);
            }
            else if (cls == "Bounds")
            {
                o = GetBounds(L, stackPos);
            }
            else
            {
                LuaDLL.lua_pushvalue(L, stackPos);
                o = new LuaTable(LuaDLL.luaL_ref(L, LuaIndexes.LUA_REGISTRYINDEX), GetTranslator(L).interpreter);
            }
        }

        //LuaDLL.lua_settop(L, oldTop);
        return o;
    }

    //读取object类型，object为万用类型, 能读取所有从lua传递的参数
    public static object GetVarObject(IntPtr L, int stackPos)
    {
        LuaTypes type = LuaDLL.lua_type(L, stackPos);

        switch (type)
        {
            case LuaTypes.LUA_TNUMBER:
                return LuaDLL.lua_tonumber(L, stackPos);
            case LuaTypes.LUA_TSTRING:
                return LuaDLL.lua_tostring(L, stackPos);
            case LuaTypes.LUA_TUSERDATA:
                {
                    int udata = LuaDLL.luanet_rawnetobj(L, stackPos);

                    if (udata != -1)
                    {
                        object obj = null;
                        GetTranslator(L).objects.TryGetValue(udata, out obj);
                        return obj;
                    }
                    else
                    {
                        return null;
                    }
                }
            case LuaTypes.LUA_TBOOLEAN:
                return LuaDLL.lua_toboolean(L, stackPos);
            case LuaTypes.LUA_TTABLE:
                return GetVarTable(L, stackPos);
            case LuaTypes.LUA_TFUNCTION:
                LuaDLL.lua_pushvalue(L, stackPos);
                return new LuaFunction(LuaDLL.luaL_ref(L, LuaIndexes.LUA_REGISTRYINDEX), GetTranslator(L).interpreter);
            default:
                return null;
        }
    }

    //枚举是值类型
    public static void Push(IntPtr L, System.Enum e)
    {
        LuaScriptMgr mgr = GetMgrFromLuaState(L);
        ObjectTranslator translator = mgr.GetLuaState().translator;
        int weakTableRef = translator.weakTableRef;
        object obj = GetEnumObj(e);

        int index = -1;
        bool found = translator.objectsBackMap.TryGetValue(obj, out index);

        if (found)
        {
            if (LuaDLL.tolua_pushudata(L, weakTableRef, index))
            {
                return;
            }

            translator.collectObject(index);
        }

        index = translator.addObject(obj, false);
        LuaDLL.tolua_pushnewudata(L, mgr.enumMetaRef, weakTableRef, index);
    }

    public static void Push(IntPtr L, LuaStringBuffer lsb)
    {
        if (lsb != null && lsb.buffer != null)
        {
            LuaDLL.lua_pushlstring(L, lsb.buffer, lsb.buffer.Length);
        }
        else
        {
            LuaDLL.lua_pushnil(L);
        }
    }

    public static LuaScriptMgr GetMgrFromLuaState(IntPtr L)
    {

        return LuaScriptMgr.Instance;
    }

    public static void ThrowLuaException(IntPtr L)
    {
        string err = LuaDLL.lua_tostring(L, -1);
        if (err == null) err = "Unknown Lua Error";
        throw new LuaScriptException(err.ToString(), "");
    }

    //无缝兼容原生写法 transform.position = v3    
    public static Vector3 GetVector3(IntPtr L, int stackPos)
    {
        LuaScriptMgr luaMgr = GetMgrFromLuaState(L);
        float x = 0, y = 0, z = 0;
        LuaDLL.tolua_getfloat3(L, luaMgr.unpackVec3, stackPos, ref x, ref y, ref z);
        return new Vector3(x, y, z);
    }

    public static void SetVector3FromLua(IntPtr L, int stackPos, ref Vector3 v)
    {
        LuaScriptMgr luaMgr = GetMgrFromLuaState(L);
        float x = 0, y = 0, z = 0;
        LuaDLL.tolua_getfloat3(L, luaMgr.unpackVec3, stackPos, ref x, ref y, ref z);
        v.x = x;
        v.y = y;
        v.z = z;
    }

    public static void Push(IntPtr L, Vector3 v3)
    {
        LuaScriptMgr luaMgr = GetMgrFromLuaState(L);
        LuaDLL.tolua_pushfloat3(L, luaMgr.packVec3, v3.x, v3.y, v3.z);
    }

    public static void Push(IntPtr L, Quaternion q)
    {
        LuaScriptMgr luaMgr = GetMgrFromLuaState(L);
        LuaDLL.tolua_pushfloat4(L, luaMgr.packQuat, q.x, q.y, q.z, q.w);
    }

    public static Quaternion GetQuaternion(IntPtr L, int stackPos)
    {
        LuaScriptMgr luaMgr = GetMgrFromLuaState(L);
        float x = 0, y = 0, z = 0, w = 1;
        LuaDLL.tolua_getfloat4(L, luaMgr.unpackQuat, stackPos, ref x, ref y, ref z, ref w);
        return new Quaternion(x, y, z, w);
    }

    public static void SetQuaternionFromLua(IntPtr L, int stackPos, ref Quaternion q)
    {
        LuaScriptMgr luaMgr = GetMgrFromLuaState(L);
        float x = 0, y = 0, z = 0, w = 1;
        LuaDLL.tolua_getfloat4(L, luaMgr.unpackQuat, stackPos, ref x, ref y, ref z, ref w);
        q.x = x;
        q.y = y;
        q.z = z;
        q.w = w;
    }

    public static Vector2 GetVector2(IntPtr L, int stackPos)
    {
        LuaScriptMgr luaMgr = GetMgrFromLuaState(L);
        float x = 0, y = 0;
        LuaDLL.tolua_getfloat2(L, luaMgr.unpackVec2, stackPos, ref x, ref y);
        return new Vector2(x, y);
    }

    public static void Push(IntPtr L, Vector2 v2)
    {
        LuaScriptMgr luaMgr = GetMgrFromLuaState(L);
        LuaDLL.tolua_pushfloat2(L, luaMgr.packVec2, v2.x, v2.y);
    }

    public static Vector4 GetVector4(IntPtr L, int stackPos)
    {
        LuaScriptMgr luaMgr = GetMgrFromLuaState(L);
        float x = 0, y = 0, z = 0, w = 0;
        LuaDLL.tolua_getfloat4(L, luaMgr.unpackVec4, stackPos, ref x, ref y, ref z, ref w);
        return new Vector4(x, y, z, w);
    }

    public static void Push(IntPtr L, Vector4 v4)
    {
        LuaScriptMgr luaMgr = GetMgrFromLuaState(L);
        LuaDLL.tolua_pushfloat4(L, luaMgr.packVec4, v4.x, v4.y, v4.z, v4.w);
    }

    public static Rect GetRect(IntPtr L, int stackPos)
    {
        LuaScriptMgr luaMgr = GetMgrFromLuaState(L);
        float x = 0, y = 0, w = 0, h = 0;
        LuaDLL.tolua_getfloat4(L, luaMgr.unpackVec4, stackPos, ref x, ref y, ref w, ref h);
        return new Rect(x, y, w, h);
    }

    public static void Push(IntPtr L, Rect r)
    {
        LuaScriptMgr luaMgr = GetMgrFromLuaState(L);
        LuaDLL.tolua_pushfloat4(L, luaMgr.packRect, r.x, r.y, r.width, r.height);
    }

    public static void Push(IntPtr L, RaycastHit hit)
    {
        LuaScriptMgr luaMgr = GetMgrFromLuaState(L);
        luaMgr.packRaycastHit.push(L);

        Push(L, hit.collider);
        Push(L, hit.distance);
        Push(L, hit.normal);
        Push(L, hit.point);
        Push(L, hit.rigidbody);
        Push(L, hit.transform);

        LuaDLL.lua_call(L, 6, -1);
    }

    public static void Push(IntPtr L, Ray ray)
    {
        LuaScriptMgr luaMgr = GetMgrFromLuaState(L);
        LuaDLL.lua_getref(L, luaMgr.packRay);

        LuaDLL.tolua_pushfloat3(L, luaMgr.packVec3, ray.direction.x, ray.direction.y, ray.direction.z);
        LuaDLL.tolua_pushfloat3(L, luaMgr.packVec3, ray.origin.x, ray.origin.y, ray.origin.z);

        LuaDLL.lua_call(L, 2, -1);
    }

    public static Ray GetRay(IntPtr L, int stackPos)
    {
        LuaScriptMgr luaMgr = GetMgrFromLuaState(L);
        float x = 0, y = 0, z = 0;
        float x1 = 0, y1 = 0, z1 = 0;
        LuaDLL.tolua_getfloat6(L, luaMgr.unpackRay, stackPos, ref x, ref y, ref z, ref x1, ref y1, ref z1);
        Vector3 origin = new Vector3(x, y, z);
        Vector3 dir = new Vector3(x1, y1, z1);
        return new Ray(origin, dir);
    }

    public static Bounds GetBounds(IntPtr L, int stackPos)
    {
        LuaScriptMgr luaMgr = GetMgrFromLuaState(L);
        float x = 0, y = 0, z = 0;
        float x1 = 0, y1 = 0, z1 = 0;
        LuaDLL.tolua_getfloat6(L, luaMgr.unpackBounds, stackPos, ref x, ref y, ref z, ref x1, ref y1, ref z1);
        Vector3 center = new Vector3(x, y, z);
        Vector3 size = new Vector3(x1, y1, z1);
        return new Bounds(center, size);
    }

    public static Color GetColor(IntPtr L, int stackPos)
    {
        LuaScriptMgr luaMgr = GetMgrFromLuaState(L);
        float r = 0, g = 0, b = 0, a = 0;
        LuaDLL.tolua_getfloat4(L, luaMgr.unpackColor, stackPos, ref r, ref g, ref b, ref a);
        return new Color(r, g, b, a);
    }

    public static void Push(IntPtr L, Color clr)
    {
        LuaScriptMgr luaMgr = GetMgrFromLuaState(L);
        LuaDLL.tolua_pushfloat4(L, luaMgr.packColor, clr.r, clr.g, clr.b, clr.a);
    }

    public static void Push(IntPtr L, Touch touch)
    {
        LuaScriptMgr luaMgr = GetMgrFromLuaState(L);
        luaMgr.packTouch.push(L);

        LuaDLL.lua_pushinteger(L, touch.fingerId);
        LuaDLL.tolua_pushfloat2(L, luaMgr.packVec2, touch.position.x, touch.position.y);
        LuaDLL.tolua_pushfloat2(L, luaMgr.packVec2, touch.rawPosition.x, touch.rawPosition.y);
        LuaDLL.tolua_pushfloat2(L, luaMgr.packVec2, touch.deltaPosition.x, touch.deltaPosition.y);
        LuaDLL.lua_pushnumber(L, touch.deltaTime);
        LuaDLL.lua_pushinteger(L, touch.tapCount);
        LuaDLL.lua_pushinteger(L, (int)touch.phase);

        LuaDLL.lua_call(L, 7, -1);
    }

    public static void Push(IntPtr L, Bounds bound)
    {
        LuaScriptMgr luaMgr = GetMgrFromLuaState(L);
        LuaDLL.lua_getref(L, luaMgr.packBounds);

        Push(L, bound.center);
        Push(L, bound.size);

        LuaDLL.lua_call(L, 2, -1);
    }


    public static void PushTraceBack(IntPtr L)
    {
        if (traceback == null)
        {
            LuaDLL.lua_getglobal(L, "traceback");
            return;
        }

        traceback.push();
    }
}
