using System;
using System.Collections.Generic;
using System.Text;
using LuaInterface;
using System.Runtime.InteropServices;

public class SevenZReader
{
    IntPtr pointer = IntPtr.Zero;

    public bool Init(string archiveName)
    {
        pointer = LuaDLL.SevenZReader_Init(archiveName);

        return pointer != IntPtr.Zero;
    }

    public void Release()
    {
        if(pointer != IntPtr.Zero)
        {
            LuaDLL.SevenZReader_Destroy(pointer);
            pointer = IntPtr.Zero;
        }
    }

    public int GetFileCount()
    {
        if (pointer != IntPtr.Zero)
            return LuaDLL.SevenZReader_GetFileCount(pointer);
        return 0;
    }

    public string GetFileName(int iFile)
    {
        if (pointer != IntPtr.Zero)
            return LuaDLL.SevenZReader_GetFileNameString(pointer, iFile);
        return "";
    }

    public IntPtr ExtractFile(int iFile, out int dataSize)
    {
        dataSize = 0;
        IntPtr pData;
        if (pointer == IntPtr.Zero ||
            !LuaDLL.SevenZReader_ExtractFile(pointer, iFile, out pData, out dataSize))
            return IntPtr.Zero;

        return pData;
    }

    public bool IsDir(int iFile)
    {
        if (pointer != IntPtr.Zero)
            return LuaDLL.SevenZReader_IsDir(pointer, iFile);
        return false;
    }
}

public struct SFileIncEntry
{
    public string Md5;
    public string FileName;
}

public class FileIncList
{
    public FileIncList()
    {
        m_FileMap = new Dictionary<string, SFileIncEntry>();
    }

    public bool AddFile(string md5, string filename)
    {
        m_FileMap.Add(filename, new SFileIncEntry() { Md5 = md5, FileName = filename });
        return true;
    }

    public bool GetFileMd5(string filename, out string md5)
    {
        SFileIncEntry entry;
        if (!m_FileMap.TryGetValue(filename, out entry))
        {
            string sepFileName = "./" + filename;
            if (!m_FileMap.TryGetValue(sepFileName, out entry))
            {
                md5 = "";
                return false;
            }
        }

        md5 = entry.Md5;
        return true;
    }

    public void Clear()
    {
        m_FileMap.Clear();
    }

    private Dictionary<string, SFileIncEntry> m_FileMap;
}

