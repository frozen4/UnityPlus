using System;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Collections;
using System.Text;
using System.Security;

//update只在客户端使用
namespace LuaInterface
{
#if !UNITY_IPHONE
    [SuppressUnmanagedCodeSecurity]
#endif
    public partial class LuaDLL
    {
        //SevenZReader
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr SevenZReader_Init([MarshalAs(UnmanagedType.LPStr)]string archiveName);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void SevenZReader_Destroy(IntPtr reader);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int SevenZReader_GetFileCount(IntPtr reader);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr SevenZReader_GetFileName(IntPtr reader, int iFile);

        public static string SevenZReader_GetFileNameString(IntPtr reader, int iFile)
        {
            IntPtr str = SevenZReader_GetFileName(reader, iFile);

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
        [return: MarshalAsAttribute(UnmanagedType.I1)]
        public static extern bool SevenZReader_ExtractFile(IntPtr reader, int iFile, out IntPtr ppData, out int dataSize);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAsAttribute(UnmanagedType.I1)]
        public static extern bool SevenZReader_IsDir(IntPtr reader, int iFile);


        //PackFunc
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAsAttribute(UnmanagedType.I1)]
        public static extern bool PackInitialize([MarshalAsAttribute(UnmanagedType.I1)]bool bCreate);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void PackFinalize([MarshalAsAttribute(UnmanagedType.I1)]bool bForce);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void FlushWritePack();

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAsAttribute(UnmanagedType.I1)]
        public static extern bool SaveAndOpenUpdatePack();

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAsAttribute(UnmanagedType.I1)]
        public static extern bool IsFileInPack([MarshalAs(UnmanagedType.LPStr)]string filename);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr CalcPackFileMd5([MarshalAs(UnmanagedType.LPStr)]string filename);

        public static string CalcPackFileMd5String(string filename)
        {
            IntPtr str = CalcPackFileMd5(filename);

            if (str != IntPtr.Zero)
                return Marshal.PtrToStringAnsi(str);
            else
                return string.Empty;
        }

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr CalcFileMd5([MarshalAs(UnmanagedType.LPStr)]string filename);

        public static string CalcFileMd5String(string filename)
        {
            IntPtr str = CalcFileMd5(filename);

            if (str != IntPtr.Zero)
                return Marshal.PtrToStringAnsi(str);
            else
                return string.Empty;
        }

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr CalcMemMd5(IntPtr pData, int dataSize);

        public static string CalcMemMd5String(IntPtr pData, int dataSize)
        {
            IntPtr str = CalcMemMd5(pData, dataSize);

            if (str != IntPtr.Zero)
                return Marshal.PtrToStringAnsi(str);
            else
                return string.Empty;
        }

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAsAttribute(UnmanagedType.I1)]
        public static extern bool AddCompressedDataToPack([MarshalAs(UnmanagedType.LPStr)]string filename, IntPtr pData, int dataSize);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAsAttribute(UnmanagedType.I1)]
        public static extern bool UncompressToSepFile([MarshalAs(UnmanagedType.LPStr)]string filename, IntPtr pData, int dataSize);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAsAttribute(UnmanagedType.I1)]
        public static extern bool MakeCompressedFile([MarshalAs(UnmanagedType.LPStr)]string srcFileName, [MarshalAs(UnmanagedType.LPStr)]string destFileName);

    }
}
