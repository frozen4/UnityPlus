namespace LuaInterface
{
	using System;
	using System.Runtime.InteropServices;
	using System.Text;
    using System.Security;

	#pragma warning disable 414
    public class MonoPInvokeCallbackAttribute : System.Attribute
    {
        private Type type;
        public MonoPInvokeCallbackAttribute( Type t ) { type = t; }
    }
	#pragma warning restore 414
	
#if !UNITY_IPHONE
    [SuppressUnmanagedCodeSecurity]
#endif
	public partial class LuaDLL
	{
        public static int LUA_MULTRET = -1;
        public static int LUA_NOREF = -2;
#if UNITY_IPHONE
        const string LUADLL = "__Internal";
#else
#if !SERVER_USE
        const string LUADLL = "hoba";
#else
        const string LUADLL = "hobaserver";
#endif
#endif

#if !SERVER_USE
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr HOBA_GetDocumentDir();

        public static string HOBA_GetDocumentDirString()
        {
            IntPtr str = HOBA_GetDocumentDir();
            if (str != IntPtr.Zero)
            {
                return Marshal.PtrToStringAnsi(str);
            }
            else
            {
                return "";
            }
        }

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr HOBA_GetLibraryDir();

        public static string HOBA_GetLibraryDirString()
        {
            IntPtr str = HOBA_GetLibraryDir();
            if (str != IntPtr.Zero)
            {
                return Marshal.PtrToStringAnsi(str);
            }
            else
            {
                return "";
            }
        }

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]

        public static extern IntPtr HOBA_GetTmpDir();

        public static string HOBA_GetTmpDirString()
        {
            IntPtr str = HOBA_GetTmpDir();
            if (str != IntPtr.Zero)
            {
                return Marshal.PtrToStringAnsi(str);
            }
            else
            {
                return "";
            }
        }

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]

        public static extern IntPtr HOBA_IOSGetCurLanguage();

        public static string HOBA_IOSGetCurLanguageString()
        {
            IntPtr str = HOBA_IOSGetCurLanguage();
            if (str != IntPtr.Zero)
            {
                return Marshal.PtrToStringAnsi(str);
            }
            else
            {
                return "";
            }
        }

        //register c fuctions
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void HOBA_Init( 
            [MarshalAs(UnmanagedType.LPStr)]string baseDir, 
            [MarshalAs(UnmanagedType.LPStr)]string docDir,
            [MarshalAs(UnmanagedType.LPStr)]string libDir,
            [MarshalAs(UnmanagedType.LPStr)]string tmpDir);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void HOBA_Release();

        //[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        //public static extern void HOBA_GetMemStats(out int peakMemKB, out int curMemKB);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void HOBA_DumpMemoryStats([MarshalAs(UnmanagedType.LPStr)]string message);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAsAttribute(UnmanagedType.I1)]
        public static extern bool HOBA_InitPackages([MarshalAs(UnmanagedType.LPStr)]string resBaseDir);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void HOBA_LogString([MarshalAs(UnmanagedType.LPStr)]string strMsg);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAsAttribute(UnmanagedType.I1)]
        public static extern bool HOBA_DeleteFilesInDirectory([MarshalAs(UnmanagedType.LPStr)]string strDir);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAsAttribute(UnmanagedType.I1)]
        public static extern bool HOBA_HasFilesInDirectory([MarshalAs(UnmanagedType.LPStr)]string strDir);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern UInt64 HOBA_GetFreeDiskSpace();

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern UInt64 HOBA_GetVirtualMemoryUsedSize();

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern UInt64 HOBA_GetPhysMemoryUsedSize();

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern UInt32 HOBA_GetMilliSecond();

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern UInt64 HOBA_GetMicroSecond();

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void HOBA_Tick();

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern float HOBA_GetMPS();
#else
        //给服务器提供windows下崩溃dump支持
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void HOBA_BeginWinMiniDump();

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void HOBA_EndWinMiniDump();

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void HOBA_MemBeginCheckPoint();

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAsAttribute(UnmanagedType.I1)]
        public static extern bool HOBA_MemEndCheckPoint();

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void HOBA_MemSetBreakAlloc(int block);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void HOBA_MemDumpMemoryLeaks();

#endif

    }
}
