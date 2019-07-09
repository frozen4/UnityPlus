using System;
using System.IO;
using System.Runtime.InteropServices;
using LuaInterface;
using System.Collections.Generic;


public class FileImage
{
    private IntPtr FilePointer;

    public FileImage()
    {
        FilePointer = IntPtr.Zero;
    }

    public bool IsOpened()
    {
        return FilePointer != IntPtr.Zero;
    }

    public bool Open(string filename, bool bText)
    {
        FilePointer = LuaDLL.FileImage_Open(filename, bText);
        return FilePointer != IntPtr.Zero;
    }

    public void Close()
    {
        if (FilePointer != IntPtr.Zero)
        {
            LuaDLL.FileImage_Close(FilePointer);
            FilePointer = IntPtr.Zero;
        }
    }

    public bool Read(byte[] pBuffer, uint bufferLength)
    {
        if (FilePointer == IntPtr.Zero)
            return false;
        return LuaDLL.FileImage_Read(FilePointer, pBuffer, bufferLength);
    }

    public int GetFileLength()
    {
        if (FilePointer == IntPtr.Zero)
            return 0;
        return LuaDLL.FileImage_GetFileLength(FilePointer);
    }

    public bool Seek(int iOffset)
    {
        if (FilePointer == IntPtr.Zero)
            return false;
        return LuaDLL.FileImage_Seek(FilePointer, iOffset);
    }

    public int GetPos()
    {
        if (FilePointer == IntPtr.Zero)
            return -1;
        return LuaDLL.FileImage_GetPos(FilePointer);
    }

    public static bool IsExist(string filename)
    {
        return LuaDLL.FileImage_IsExist(filename);
    }

    public struct FileBytesInfo
    {
        public int Length;
        public string FileName;

        public FileBytesInfo(string file, int len)
        {
            Length = len;
            FileName = file;
        }
    }

    //static List<FileBytesInfo> _AllFilesRecords = new List<FileBytesInfo>();
    public static byte[] ReadAllBytes(string filename)
    {
        byte[] data = null;
        IntPtr file = LuaDLL.FileImage_Open(filename, false);
        if (file != IntPtr.Zero)
        {
            int len = LuaDLL.FileImage_GetFileLength(file);
            //_AllFilesRecords.Add(new FileBytesInfo(filename, len));
            data = new byte[len];
            if (!LuaDLL.FileImage_Read(file, data, (uint)len))
            {
                data = null;
            }
            LuaDLL.FileImage_Close(file);
        }
        return data;
    }

//     public static void ShowDiagnostics()
//     {
//         string content = "";
//         foreach( var v in _AllFilesRecords)
//         {
//             var item = string.Format("{0},{1},\n", v.FileName, v.Length);
//             content = content + item;
//         }
//         Common.HobaDebuger.LogWarning(content);
//         _AllFilesRecords.Clear();
//     }
}

