using System;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Collections;
using System.Text;
using System.Security;

namespace LuaInterface
{
    public enum LuaTypes
    {
        LUA_TNONE = -1,
        LUA_TNIL = 0,
        LUA_TNUMBER = 3,
        LUA_TSTRING = 4,
        LUA_TBOOLEAN = 1,
        LUA_TTABLE = 5,
        LUA_TFUNCTION = 6,
        LUA_TUSERDATA = 7,
        LUA_TTHREAD = 8,
        LUA_TLIGHTUSERDATA = 2
    }

    public enum LuaGCOptions
    {
        LUA_GCSTOP = 0,
        LUA_GCRESTART = 1,
        LUA_GCCOLLECT = 2,
        LUA_GCCOUNT = 3,
        LUA_GCCOUNTB = 4,
        LUA_GCSTEP = 5,
        LUA_GCSETPAUSE = 6,
        LUA_GCSETSTEPMUL = 7,
    }

    public enum LuaThreadStatus
    {
        LUA_YIELD = 1,
        LUA_ERRRUN = 2,
        LUA_ERRSYNTAX = 3,
        LUA_ERRMEM = 4,
        LUA_ERRERR = 5,
    }

    public sealed class LuaIndexes
    {
        public static int LUA_REGISTRYINDEX = -10000;
        public static int LUA_ENVIRONINDEX = -10001;
        public static int LUA_GLOBALSINDEX = -10002;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ReaderInfo
    {
        public String chunkData;
        public bool finished;
    }

#if !UNITY_IPHONE
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
#endif
    public delegate int LuaCSFunction(IntPtr luaState);

    public delegate string LuaChunkReader(IntPtr luaState, ref ReaderInfo data, ref uint size);

    public delegate int LuaFunctionCallback(IntPtr luaState);

    #if !UNITY_IPHONE
    [SuppressUnmanagedCodeSecurity]
#endif
    public partial class LuaDLL
    {
        public static void lua_stackdump(IntPtr L)
        {
            var top = lua_gettop(L);
            var dump = "----lua stackdump begin----\n";
            for(var i = 0; i < top; i++)
            {
                var t = lua_type(L, i);
                switch(t)
                {
                    case LuaTypes.LUA_TSTRING:
                        dump += lua_tostring(L, i);
                        break;
                    case LuaTypes.LUA_TBOOLEAN:
                        dump += (lua_toboolean(L, i)? "true" : "false");
                        break;
                    case LuaTypes.LUA_TNUMBER:
                        dump += lua_tonumber(L, i);
                        break;
                    default:
                        dump += lua_typename(L, t);
                        break;
                }
                dump += "\n";
            }
            dump += "----lua stackdump end----";
#if !SERVER_USE
            Common.HobaDebuger.LogWarning(dump);
#endif
        }

        //proto buffer
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int luaopen_pb(IntPtr L);

        //bit
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int luaopen_bit(IntPtr L);

        //lfs
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int luaopen_lfs(IntPtr L);

        //tolua
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int tolua_openlibs(IntPtr L);

        //profiler
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int luaopen_profiler(IntPtr L);

        //UInt64
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int luaopen_LuaUInt64(IntPtr L);

        //SkillCollision
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int luaopen_SkillCollision(IntPtr L);

        //cbinary
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int luaopen_cbinary(IntPtr L);

        //socket
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int luaopen_socket_core(IntPtr L);

        //mime
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int luaopen_mime_core(IntPtr L);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int luaopen_snapshot(IntPtr L);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void setup_luastate(IntPtr L);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void clear_luastate();

        public static int lua_upvalueindex(int i)
        {
            return LuaIndexes.LUA_GLOBALSINDEX - i;
        }

        // Thread Funcs
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl, EntryPoint = "HOBA_lua_tothread")]
        public static extern int lua_tothread(IntPtr L, int index);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl, EntryPoint = "HOBA_lua_xmove")]
        public static extern int lua_xmove(IntPtr from, IntPtr to, int n);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl, EntryPoint = "HOBA_lua_yield")]
        public static extern int lua_yield(IntPtr L, int nresults);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl, EntryPoint = "HOBA_lua_newthread")]
        public static extern IntPtr lua_newthread(IntPtr L);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl, EntryPoint = "HOBA_lua_resume")]
        public static extern int lua_resume(IntPtr L, int narg);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl, EntryPoint = "HOBA_lua_status")]
        public static extern int lua_status(IntPtr L);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl, EntryPoint = "HOBA_lua_pushthread")]
        public static extern int lua_pushthread(IntPtr L);

        public static int luaL_getn(IntPtr luaState, int i)
        {
            return (int)LuaDLL.lua_objlen(luaState, i);
        }

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl, EntryPoint = "HOBA_lua_gc")]
        public static extern int lua_gc(IntPtr luaState, LuaGCOptions what, int data);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl, EntryPoint = "HOBA_lua_typename")]
        public static extern IntPtr HOBA_lua_typename(IntPtr luaState, LuaTypes type);

        public static string lua_typename(IntPtr luaState, LuaTypes type)
        {
            IntPtr ptr = HOBA_lua_typename(luaState, type);
            if (ptr != IntPtr.Zero)
            {
                return Marshal.PtrToStringAnsi(ptr);
            }
            return null;
        }

        public static string luaL_typename(IntPtr luaState, int stackPos)
        {
            return LuaDLL.lua_typename(luaState, LuaDLL.lua_type(luaState, stackPos));
        }

        public static bool lua_isfunction(IntPtr luaState, int stackPos)
        {
            return lua_type(luaState, stackPos) == LuaTypes.LUA_TFUNCTION;
        }

        public static bool lua_islightuserdata(IntPtr luaState, int stackPos)
        {
            return lua_type(luaState, stackPos) == LuaTypes.LUA_TLIGHTUSERDATA;
        }

        public static bool lua_istable(IntPtr luaState, int stackPos)
        {
            return lua_type(luaState, stackPos) == LuaTypes.LUA_TTABLE;
        }

        public static bool lua_isthread(IntPtr luaState, int stackPos)
        {
            return lua_type(luaState, stackPos) == LuaTypes.LUA_TTHREAD;
        }

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl, EntryPoint = "HOBA_luaL_error")]
        public static extern void luaL_error(IntPtr luaState, [MarshalAs(UnmanagedType.LPStr)]string message);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl, EntryPoint = "HOBA_luaL_gsub")]
        [return: MarshalAs(UnmanagedType.LPStr)]
        public static extern string luaL_gsub(IntPtr luaState, [MarshalAs(UnmanagedType.LPStr)]string str, [MarshalAs(UnmanagedType.LPStr)]string pattern, [MarshalAs(UnmanagedType.LPStr)]string replacement);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl, EntryPoint = "HOBA_lua_getfenv")]
        public static extern void lua_getfenv(IntPtr luaState, int stackPos);


        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl, EntryPoint = "HOBA_lua_isuserdata")]
        public static extern int lua_isuserdata(IntPtr luaState, int stackPos);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl, EntryPoint = "HOBA_lua_lessthan")]
        public static extern int lua_lessthan(IntPtr luaState, int stackPos1, int stackPos2);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl, EntryPoint = "HOBA_lua_rawequal")]
        public static extern int lua_rawequal(IntPtr luaState, int stackPos1, int stackPos2);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl, EntryPoint = "HOBA_lua_setfenv")]
        public static extern int lua_setfenv(IntPtr luaState, int stackPos);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl, EntryPoint = "HOBA_lua_setfield")]
        public static extern void lua_setfield(IntPtr luaState, int stackPos, [MarshalAs(UnmanagedType.LPStr)]string name);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl, EntryPoint = "HOBA_luaL_callmeta")]
        public static extern int luaL_callmeta(IntPtr luaState, int stackPos, [MarshalAs(UnmanagedType.LPStr)]string name);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl, EntryPoint = "HOBA_luaL_newstate")]
        public static extern IntPtr luaL_newstate();

        /// <summary>DEPRECATED - use luaL_newstate() instead!</summary>
        public static IntPtr lua_open()
        {
            return LuaDLL.luaL_newstate();
        }

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl, EntryPoint = "HOBA_lua_close")]
        public static extern void lua_close(IntPtr luaState);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl, EntryPoint = "HOBA_luaL_openlibs")]
        public static extern void luaL_openlibs(IntPtr luaState);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl, EntryPoint = "HOBA_lua_objlen")]
        public static extern int lua_objlen(IntPtr luaState, int stackPos);

        /// <summary>DEPRECATED - use lua_objlen(IntPtr luaState, int stackPos) instead!</summary>
        public static int lua_strlen(IntPtr luaState, int stackPos)
        {
            return lua_objlen(luaState, stackPos);
        }

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl, EntryPoint = "HOBA_luaL_loadstring")]
        public static extern int luaL_loadstring(IntPtr luaState, [MarshalAs(UnmanagedType.LPStr)]string chunk);

        public static int luaL_dostring(IntPtr luaState, string chunk)
        {
            int result = LuaDLL.luaL_loadstring(luaState, chunk);
            if (result != 0)
                return result;

            return LuaDLL.lua_pcall(luaState, 0, -1, 0);
        }
        /// <summary>DEPRECATED - use luaL_dostring(IntPtr luaState, string chunk) instead!</summary>
        public static int lua_dostring(IntPtr luaState, string chunk)
        {
            return LuaDLL.luaL_dostring(luaState, chunk);
        }

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl, EntryPoint = "HOBA_lua_createtable")]
        public static extern void lua_createtable(IntPtr luaState, int narr, int nrec);

        public static void lua_newtable(IntPtr luaState)
        {
            LuaDLL.lua_createtable(luaState, 0, 0);
        }
        public static int luaL_dofile(IntPtr luaState, string fileName)
        {
            int result = LuaDLL.luaL_loadfile(luaState, fileName);
            if (result != 0)
                return result;

            return LuaDLL.lua_pcall(luaState, 0, -1, 0);
        }
        public static void lua_getglobal(IntPtr luaState, string name)
        {
            LuaDLL.lua_pushstring(luaState, name);
            LuaDLL.lua_gettable(luaState, LuaIndexes.LUA_GLOBALSINDEX);
        }
        public static void lua_setglobal(IntPtr luaState, string name)
        {
            //LuaDLL.lua_pushstring(luaState,name);
            //LuaDLL.lua_insert(luaState,-2);
            //LuaDLL.lua_settable(luaState,LuaIndexes.LUA_GLOBALSINDEX);
            LuaDLL.lua_setfield(luaState, LuaIndexes.LUA_GLOBALSINDEX, name);
        }
        public static void lua_rawglobal(IntPtr luaState, string name)
        {
            LuaDLL.lua_pushstring(luaState, name);
            LuaDLL.lua_rawget(luaState, LuaIndexes.LUA_GLOBALSINDEX);
        }

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl, EntryPoint = "HOBA_lua_settop")]
        public static extern void lua_settop(IntPtr luaState, int newTop);

        public static void lua_pop(IntPtr luaState, int amount)
        {
            LuaDLL.lua_settop(luaState, -(amount) - 1);
        }
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl, EntryPoint = "HOBA_lua_insert")]
        public static extern void lua_insert(IntPtr luaState, int newTop);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl, EntryPoint = "HOBA_lua_remove")]
        public static extern void lua_remove(IntPtr luaState, int index);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl, EntryPoint = "HOBA_lua_gettable")]
        public static extern void lua_gettable(IntPtr luaState, int index);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl, EntryPoint = "HOBA_lua_rawget")]
        public static extern void lua_rawget(IntPtr luaState, int index);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl, EntryPoint = "HOBA_lua_settable")]
        public static extern void lua_settable(IntPtr luaState, int index);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl, EntryPoint = "HOBA_lua_rawset")]
        public static extern void lua_rawset(IntPtr luaState, int index);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl, EntryPoint = "HOBA_lua_setmetatable")]
        public static extern void lua_setmetatable(IntPtr luaState, int objIndex);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl, EntryPoint = "HOBA_lua_getmetatable")]
        public static extern int lua_getmetatable(IntPtr luaState, int objIndex);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl, EntryPoint = "HOBA_lua_equal")]
        public static extern int lua_equal(IntPtr luaState, int index1, int index2);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl, EntryPoint = "HOBA_lua_pushvalue")]
        public static extern void lua_pushvalue(IntPtr luaState, int index);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl, EntryPoint = "HOBA_lua_replace")]
        public static extern void lua_replace(IntPtr luaState, int index);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl, EntryPoint = "HOBA_lua_gettop")]
        public static extern int lua_gettop(IntPtr luaState);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl, EntryPoint = "HOBA_lua_type")]
        public static extern LuaTypes lua_type(IntPtr luaState, int index);

        public static bool lua_isnil(IntPtr luaState, int index)
        {
            return (LuaDLL.lua_type(luaState, index) == LuaTypes.LUA_TNIL);
        }

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl, EntryPoint = "HOBA_lua_isnumber")]
        public static extern bool lua_isnumber(IntPtr luaState, int index);

        public static bool lua_isboolean(IntPtr luaState, int index)
        {
            return LuaDLL.lua_type(luaState, index) == LuaTypes.LUA_TBOOLEAN;
        }

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl, EntryPoint = "HOBA_luaL_ref")]
        public static extern int luaL_ref(IntPtr luaState, int registryIndex);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl, EntryPoint = "HOBA_lua_rawgeti")]
        public static extern void lua_rawgeti(IntPtr luaState, int tableIndex, int index);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl, EntryPoint = "HOBA_lua_rawseti")]
        public static extern void lua_rawseti(IntPtr luaState, int tableIndex, int index);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl, EntryPoint = "HOBA_lua_newuserdata")]
        public static extern IntPtr lua_newuserdata(IntPtr luaState, int size);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl, EntryPoint = "HOBA_lua_touserdata")]
        public static extern IntPtr lua_touserdata(IntPtr luaState, int index);

        public static void lua_getref(IntPtr luaState, int reference)
        {
            LuaDLL.lua_rawgeti(luaState, LuaIndexes.LUA_REGISTRYINDEX, reference);
        }

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl, EntryPoint = "HOBA_luaL_unref")]
        public static extern void luaL_unref(IntPtr luaState, int registryIndex, int reference);

        public static void lua_unref(IntPtr luaState, int reference)
        {
            LuaDLL.luaL_unref(luaState, LuaIndexes.LUA_REGISTRYINDEX, reference);
        }

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl, EntryPoint = "HOBA_lua_isstring")]
        public static extern bool lua_isstring(IntPtr luaState, int index);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl, EntryPoint = "HOBA_lua_iscfunction")]
        public static extern bool lua_iscfunction(IntPtr luaState, int index);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl, EntryPoint = "HOBA_lua_pushnil")]
        public static extern void lua_pushnil(IntPtr luaState);

        public static void lua_pushstdcallcfunction(IntPtr luaState, LuaCSFunction function, int n = 0)
        {
            IntPtr fn = Marshal.GetFunctionPointerForDelegate(function);
            lua_pushcclosure(luaState, fn, n);
        }

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl, EntryPoint = "HOBA_lua_call")]
        public static extern void lua_call(IntPtr luaState, int nArgs, int nResults);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl, EntryPoint = "HOBA_lua_pcall")]
        public static extern int lua_pcall_raw(IntPtr luaState, int nArgs, int nResults, int errfunc);

        public static int lua_pcall(IntPtr luaState, int nArgs, int nResults, int errfunc)
        {
            // int len = System.Environment.StackTrace.Length;
            //string str = System.Environment.StackTrace.Substring(0, Math.Min(1024, len));
            
            //LuaDLL.HOBA_LogString(str);
            return lua_pcall_raw(luaState, nArgs, nResults, errfunc);
        }

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl, EntryPoint = "HOBA_lua_tocfunction")]
        public static extern IntPtr lua_tocfunction(IntPtr luaState, int index);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl, EntryPoint = "HOBA_lua_tonumber")]
        public static extern double lua_tonumber(IntPtr luaState, int index);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl, EntryPoint = "HOBA_lua_toboolean")]
        public static extern bool lua_toboolean(IntPtr luaState, int index);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl, EntryPoint = "HOBA_lua_tolstring")]
        public static extern IntPtr lua_tolstring(IntPtr luaState, int index, out int strLen);


        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr SC_CreateShape(int objType, float radius, float length, float angle, float[] pos, float[] dir);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void SC_DestroyShape(IntPtr pShape);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool SC_IsCollideWithShape(IntPtr pShape, float[] pos, float radius);

        static string AnsiToUnicode(IntPtr source, int strlen)
        {
            byte[] buffer = new byte[strlen];
            Marshal.Copy(source, buffer, 0, strlen);
            string str = Encoding.UTF8.GetString(buffer);
            return str;
        }

        public static string lua_tostring(IntPtr luaState, int index)
        {
            int strlen;
            IntPtr str = lua_tolstring(luaState, index, out strlen);

            if (str != IntPtr.Zero)
            {
                string ss = Marshal.PtrToStringAnsi(str, strlen);

                if (ss == null)
                {
                    return AnsiToUnicode(str, strlen);
                }

                return ss;
            }
            else
            {
                return null;
            }
        }

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl, EntryPoint = "HOBA_lua_atpanic")]
        public static extern void lua_atpanic(IntPtr luaState, LuaCSFunction panicf);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl, EntryPoint = "HOBA_lua_pushnumber")]
        public static extern void lua_pushnumber(IntPtr luaState, double number);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl, EntryPoint = "HOBA_lua_pushinteger")]
        public static extern void lua_pushinteger(IntPtr luaState, int number);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl, EntryPoint = "HOBA_lua_pushboolean")]
        public static extern void lua_pushboolean(IntPtr luaState, bool value);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl, EntryPoint = "HOBA_lua_pushlstring")]
        public static extern void lua_pushlstring(IntPtr luaState, byte[] str, int size);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl, EntryPoint = "HOBA_lua_pushstring")]
        public static extern void lua_pushstring(IntPtr luaState, [MarshalAs(UnmanagedType.LPStr)]string str);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl, EntryPoint = "HOBA_luaL_newmetatable")]
        public static extern int luaL_newmetatable(IntPtr luaState, [MarshalAs(UnmanagedType.LPStr)]string meta);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl, EntryPoint = "HOBA_lua_getfield")]
        public static extern void lua_getfield(IntPtr luaState, int stackPos, [MarshalAs(UnmanagedType.LPStr)]string meta);

        public static void luaL_getmetatable(IntPtr luaState, string meta)
        {
            LuaDLL.lua_getfield(luaState, LuaIndexes.LUA_REGISTRYINDEX, meta);
        }

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl, EntryPoint = "HOBA_luaL_checkudata")]
        public static extern IntPtr luaL_checkudata(IntPtr luaState, int stackPos, [MarshalAs(UnmanagedType.LPStr)]string meta);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl, EntryPoint = "HOBA_luaL_getmetafield")]
        public static extern LuaTypes luaL_getmetafield(IntPtr luaState, int stackPos, [MarshalAs(UnmanagedType.LPStr)]string field);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl, EntryPoint = "HOBA_lua_load")]
        public static extern int lua_load(IntPtr luaState, LuaChunkReader chunkReader, ref ReaderInfo data, [MarshalAs(UnmanagedType.LPStr)]string chunkName);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl, EntryPoint = "HOBA_luaL_loadbuffer")]
        public static extern int luaL_loadbuffer(IntPtr luaState, byte[] buff, int size, [MarshalAs(UnmanagedType.LPStr)]string name);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl, EntryPoint = "HOBA_luaL_loadfile")]
        public static extern int luaL_loadfile(IntPtr luaState, [MarshalAs(UnmanagedType.LPStr)]string filename);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl, EntryPoint = "HOBA_lua_error")]
        public static extern void lua_error(IntPtr luaState);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl, EntryPoint = "HOBA_lua_checkstack")]
        public static extern bool lua_checkstack(IntPtr luaState, int extra);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl, EntryPoint = "HOBA_lua_next")]
        public static extern int lua_next(IntPtr luaState, int index);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl, EntryPoint = "HOBA_lua_pushlightuserdata")]
        public static extern void lua_pushlightuserdata(IntPtr luaState, IntPtr udata);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl, EntryPoint = "HOBA_luaL_where")]
        public static extern void luaL_where(IntPtr luaState, int level);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl, EntryPoint = "HOBA_lua_pushcclosure")]
        public static extern void lua_pushcclosure(IntPtr luaState, IntPtr fn, int n);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl, EntryPoint = "HOBA_lua_getupvalue")]
        public static extern IntPtr HOBA_lua_getupvalue(IntPtr L, int funcindex, int n);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl, EntryPoint = "HOBA_lua_setupvalue")]
        public static extern IntPtr HOBA_lua_setupvalue(IntPtr L, int funcindex, int n);

        public static string lua_getupvalue(IntPtr L, int funcindex, int n)
        {
            IntPtr ptr = HOBA_lua_getupvalue(L, funcindex, n);
            if (ptr != IntPtr.Zero)
            {
                return Marshal.PtrToStringAnsi(ptr);
            }
            return null;
        }

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl, EntryPoint = "HOBA_luaL_typerror")]
        public static extern int luaL_typerror(IntPtr luaState, int narg, [MarshalAs(UnmanagedType.LPStr)]string tname);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl, EntryPoint = "HOBA_luaL_argerror")]
        public static extern int luaL_argerror(IntPtr luaState, int narg, [MarshalAs(UnmanagedType.LPStr)]string extramsg);

        //lua_wrap.c
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int luaL_checkmetatable(IntPtr luaState, int obj);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int luanet_tonetobject(IntPtr luaState, int obj);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void luanet_newudata(IntPtr luaState, int val);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int luanet_rawnetobj(IntPtr luaState, int obj);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int luanet_checkudata(IntPtr luaState, int obj, [MarshalAs(UnmanagedType.LPStr)]string meta);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr luanet_gettag();

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void tolua_getfloat2(IntPtr luaState, int reference, int stack, ref float x, ref float y);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void tolua_getfloat3(IntPtr luaState, int reference, int stack, ref float x, ref float y, ref float z);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void tolua_getfloat4(IntPtr luaState, int reference, int stack, ref float x, ref float y, ref float z, ref float w);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void tolua_getfloat6(IntPtr luaState, int reference, int stack, ref float x, ref float y, ref float z, ref float x1, ref float y1, ref float z1);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void tolua_pushfloat2(IntPtr luaState, int reference, float x, float y);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void tolua_pushfloat3(IntPtr luaState, int reference, float x, float y, float z);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void tolua_pushfloat4(IntPtr luaState, int reference, float x, float y, float z, float w);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAsAttribute(UnmanagedType.I1)]
        public static extern bool tolua_pushudata(IntPtr L, int reference, int index);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAsAttribute(UnmanagedType.I1)]
        public static extern bool tolua_pushnewudata(IntPtr L, int metaRef, int weakTableRef, int index);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void tolua_setindex(IntPtr L);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void tolua_setnewindex(IntPtr L);    

        //Unity专有函数
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAsAttribute(UnmanagedType.I1)]
        public static extern bool HOBA_Unity_Lua_PCall(IntPtr L, int nArgs, int nResults, int registryIndex, int errofFuncRef);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr HOBA_Unity_Lua_Call(IntPtr L, int nArgs, int registryIndex, int errofFuncRef);

        public static string Unity_Lua_Call(IntPtr L, int nArgs, int registryIndex, int errofFuncRef)
        {
            IntPtr str = HOBA_Unity_Lua_Call(L, nArgs, registryIndex, errofFuncRef);

            if (str != IntPtr.Zero)
            {
                string ss = Marshal.PtrToStringAnsi(str);
                return ss;
            }
            else
            {
                return null;
            }
        }
    }
}
