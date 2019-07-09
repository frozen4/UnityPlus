using System;
using System.IO;
using UnityEngine;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine.Profiling;

public partial class Util
{
    //将Byte转换为结构体类型
    public static byte[] StructToBytes(object structObj, int size)
    {
        byte[] bytes = new byte[size];
        IntPtr structPtr = Marshal.AllocHGlobal(size);
        //将结构体拷到分配好的内存空间
        Marshal.StructureToPtr(structObj, structPtr, false);
        //从内存空间拷贝到byte 数组
        Marshal.Copy(structPtr, bytes, 0, size);
        //释放内存空间
        Marshal.FreeHGlobal(structPtr);
        return bytes;

    }

    //将Byte转换为结构体类型
    public static object ByteToStruct(byte[] bytes, Type type)
    {
        int size = Marshal.SizeOf(type);
        if (size > bytes.Length)
        {
            return null;
        }
        //分配结构体内存空间
        IntPtr structPtr = Marshal.AllocHGlobal(size);
        //将byte数组拷贝到分配好的内存空间
        Marshal.Copy(bytes, 0, structPtr, size);
        //将内存空间转换为目标结构体
        object obj = Marshal.PtrToStructure(structPtr, type);
        //释放内存空间
        Marshal.FreeHGlobal(structPtr);
        return obj;
    }

    public static Color32 GetColorUInt32(UInt32 value)
    {
        byte r = (byte)((value >> 24) & 0xff);
        byte g = (byte)((value >> 16) & 0xff);
        byte b = (byte)((value >> 8) & 0xff);
        byte a = (byte)((value) & 0xff);

        return new Color32(r, g, b, a);
    }

    static byte[] _aBOM = new byte[] { 0xEF, 0xBB, 0xBF };
    public static byte[] GetBOMHeader()
    {
        return _aBOM;
    }

    public static bool HasBOMHeader(byte[] first3Bytes)
    {
        return first3Bytes[0] == _aBOM[0] && first3Bytes[1] == _aBOM[1] && first3Bytes[2] == _aBOM[2];
    }

    //
    public static object IntPtrToStruct(IntPtr structPtr, Type type)
    {
        object obj = Marshal.PtrToStructure(structPtr, type);
        return obj;
    }

    public static string ReadString(BinaryReader reader)
    {
        int len = reader.ReadInt32();
        byte[] utf8string = reader.ReadBytes(len);
        return System.Text.Encoding.UTF8.GetString(utf8string, 0, len);
    }

    public static void WriteString(BinaryWriter writer, string str)
    {
        byte[] utf8str = System.Text.Encoding.UTF8.GetBytes(str);
        int len = utf8str.GetLength(0);
        writer.Write(len);
        writer.Write(utf8str);
    }

    public static void ReadBin(BinaryReader br, ref float v)
    {
        v = br.ReadSingle();
    }

    public static void ReadBin(BinaryReader br, ref int v)
    {
        v = br.ReadInt32();
    }

    public static void ReadBin(BinaryReader br, ref bool v)
    {
        v = br.ReadBoolean();
    }

    public static void ReadBin(BinaryReader br, ref uint v)
    {
        v = br.ReadUInt32();
    }

    public static int GetStringByteSize(string str)
    {
        byte[] utf8str = System.Text.Encoding.UTF8.GetBytes(str);
        int len = utf8str.GetLength(0);
        return Marshal.SizeOf(len) + len;
    }

    public static int GetByteSize(bool v)
    {
        return 1;
    }

    public static int GetByteSize(int v)
    {
        return Marshal.SizeOf(v);
    }

    public static int GetByteSize(float v)
    {
        return Marshal.SizeOf(v);
    }

    public static int GetByteSize(uint v)
    {
        return Marshal.SizeOf(v);
    }

    public static void Align4Bytes(ref int x)
    {
        x = (x + 3) & ~3;
    }

    public static void AlignBR4Bytes(BinaryReader reader)
    {
        int pos = (int)reader.BaseStream.Position;
        int newpos = pos;
        Align4Bytes(ref newpos);
        for (int i = pos; i < newpos; ++i)
        {
            reader.ReadByte();
        }
    }

    public static void AlignBW4Bytes(BinaryWriter writer)
    {
        int pos = (int)writer.BaseStream.Position;
        int newpos = pos;
        Align4Bytes(ref newpos);
        for (int i = pos; i < newpos; ++i)
        {
            writer.Write(false);
        }
    }

    public static string gb2312_to_utf8(string text)
    {
        //声明字符集   
        System.Text.Encoding utf8, gb2312;
        //gb2312   
        gb2312 = System.Text.Encoding.GetEncoding("gb2312");
        //utf8   
        utf8 = System.Text.Encoding.GetEncoding("utf-8");
        byte[] gb;
        gb = gb2312.GetBytes(text);
        gb = System.Text.Encoding.Convert(gb2312, utf8, gb);
        //返回转换后的字符   
        return utf8.GetString(gb);
    }

    public static string unicode_to_utf8(string text)
    {
        byte[] buffer1 = Encoding.UTF8.GetBytes(text);
        byte[] buffer2 = Encoding.Convert(Encoding.Default, Encoding.UTF8, buffer1, 0, buffer1.Length);
        string strBuffer = Encoding.UTF8.GetString(buffer2, 0, buffer2.Length);
        return strBuffer;
    }

    /// <summary>
    /// UTF8转换成GB2312
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    public static string utf8_to_gb2312(string text)
    {
        //声明字符集   
        System.Text.Encoding utf8, gb2312;
        //utf8   
        utf8 = System.Text.Encoding.GetEncoding("utf-8");
        //gb2312   
        gb2312 = System.Text.Encoding.GetEncoding("gb2312");
        byte[] utf;
        utf = utf8.GetBytes(text);
        utf = System.Text.Encoding.Convert(utf8, gb2312, utf);
        //返回转换后的字符   
        return gb2312.GetString(utf);
    }

    public static string utf8_to_unicode(string text)
    {
        byte[] buffer1 = Encoding.Default.GetBytes(text);
        byte[] buffer2 = Encoding.Convert(Encoding.UTF8, Encoding.Default, buffer1, 0, buffer1.Length);
        string strBuffer = Encoding.Default.GetString(buffer2, 0, buffer2.Length);
        return strBuffer;
    }

    public static int ReadInt32(MemoryStream memstream)
    {
        byte[] lenBytes = new byte[4];
        memstream.Read(lenBytes, 0, 4);
        int len = (int)Util.ByteToStruct(lenBytes, typeof(int));
        return len;
    }

    public static uint ReadUInt32(MemoryStream memstream)
    {
        byte[] lenBytes = new byte[4];
        memstream.Read(lenBytes, 0, 4);
        uint len = (uint)Util.ByteToStruct(lenBytes, typeof(uint));
        return len;
    }

    public static float ReadSingle(MemoryStream memstream)
    {
        byte[] lenBytes = new byte[4];
        memstream.Read(lenBytes, 0, 4);
        float len = (float)Util.ByteToStruct(lenBytes, typeof(float));
        return len;
    }

    public static Vector3 ReadVector3(MemoryStream memStream)
    {
        int len = Marshal.SizeOf(typeof(Vector3));
        byte[] bytes = new byte[len];
        memStream.Read(bytes, 0, len);
        Vector3 v = (Vector3)Util.ByteToStruct(bytes, typeof(Vector3));
        return v;
    }

    public static Quaternion ReadQuaternion(MemoryStream memStream)
    {
        int len = Marshal.SizeOf(typeof(Quaternion));
        byte[] bytes = new byte[len];
        memStream.Read(bytes, 0, len);
        Quaternion v = (Quaternion)Util.ByteToStruct(bytes, typeof(Quaternion));
        return v;
    }

    public static void PrintCurrentMemoryInfo(string tag)
    {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
        var monoHeapSize = Profiler.GetMonoHeapSizeLong() / 1024 / 1024;
        var monoUsedSize = Profiler.GetMonoUsedSizeLong() / 1024 / 1024;
        var usedHeapSize = Profiler.usedHeapSizeLong / 1024 / 1024;

        Common.HobaDebuger.LogWarning(tag);
        Common.HobaDebuger.LogWarningFormat("当前Mono内存 {0}M，实际使用Mono内存 {1}M，总计使用堆内存 {2}M", monoHeapSize, monoUsedSize, usedHeapSize);
#endif
    }


    public static long GetUsedHeapSize()
    {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
        var usedHeapSize = Profiler.GetMonoUsedSizeLong();
        return usedHeapSize;
#else
        return 0;
#endif
    }

    public static bool IsChineseUnicode(char ch)
    {
        //http://www.qqxiuzi.cn/zh/hanzi-unicode-bianma.php
        if (ch >= 0x4E00 && ch < 0x9FA5)
            return true;

        if (ch >= 0x9FA6 && ch <= 0x9FCB)
            return true;

        return false;
    }

    public static bool IsChineseUnicode(string str)
    {
        for (int i = 0; i < str.Length; ++i)
        {
            if (IsChineseUnicode(str[i]))
                return true;
        }
        return false;
    }

    public static bool IsAllAscii(string str)
    {
        for (int i = 0; i < str.Length; ++i)
        {
            if (str[i] > 256)
                return false;
        }
        return true;
    }

    public static NetworkReachability GetNetworkStatus()
    {
        return Application.internetReachability;
    }

    public static float GetBatteryLevel()
    {
        return SystemInfo.batteryLevel;
    }

    public static BatteryStatus GetBatteryStatus()
    {
        return SystemInfo.batteryStatus;
    }
}