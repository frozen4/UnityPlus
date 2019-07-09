using System;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Collections;
using System.Text;
using System.Security;

//fileimage只在客户端使用
/* 实例
 * {
		AFileImage* pFile = FileImage_Open("lua\\Lplus.lua", true);
		
        int len = FileImage_GetFileLength(pFile)
        
		FileImage_Close(pFile);
	}
 * */

namespace LuaInterface
{
    #if !UNITY_IPHONE
    [SuppressUnmanagedCodeSecurity]
#endif
    public partial class LuaDLL
    {
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr FileImage_Open([MarshalAs(UnmanagedType.LPStr)]string filename, [MarshalAs(UnmanagedType.I1)]bool bText);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void FileImage_Close(IntPtr pFile);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool FileImage_Read(IntPtr pFile, [MarshalAs(UnmanagedType.LPArray)]byte[] pBuffer, uint bufferLength);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int FileImage_GetFileLength(IntPtr pFile);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool FileImage_Seek(IntPtr pFile, int iOffset);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int FileImage_GetPos(IntPtr pFile);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool FileImage_IsExist([MarshalAs(UnmanagedType.LPStr)]string filename);
    }
}
