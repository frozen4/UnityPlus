using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using LuaInterface;

public static class FileOperate
{
    public static bool IsFileExist(string filename)
    {
        return File.Exists(filename);
    }

    public static bool IsDirectoryExist(string dirname)
    {
        return Directory.Exists(dirname);
    }

    public static bool DeleteFile(string filename)
    {
        bool ret = true;
        try 
        {
            File.SetAttributes(filename, File.GetAttributes(filename) & ~FileAttributes.ReadOnly);
            File.Delete(filename);
        }
        catch(Exception)
        {
            ret = false;
        }
        return ret;
    }

    public static StreamReader OpenTextFile(string filename)
    {
        StreamReader reader = null;
        try
        {
            reader = File.OpenText(filename);
        }
        catch(IOException)
        {
            reader = null;
        }
        return reader;
    }

    public static StreamWriter CreateTextFile(string filename)
    {
        StreamWriter writer = null;
        try
        {
            writer = File.CreateText(filename);
        }
        catch(IOException)
        {
            writer = null;
        }
        return writer;
    }

    public static bool MakeDir(string dir, int r)
    {
        --r;
        while (r > 0 && dir[r] != '/' && dir[r] != '\\')
            --r;
        if (r == 0)
            return true;

        MakeDir(dir, r);

        string subDir = dir.Substring(0, r);

        try
        {
            Directory.CreateDirectory(subDir);
        }
        catch(IOException)
        {

        }
        return Directory.Exists(subDir);
    }

    public static bool MakeDir(string dir)
    {
       return MakeDir(dir, dir.Length);
    }

    /*
    public static String CalcFileMd5(String filePath)
    {
        Int64 fileSize;
        return CalcFileMd5AndSize(filePath, out fileSize);
    }

    public static String CalcFileMd5AndSize(String filePath, out Int64 fileSize)
    {
        try
        {
            var md5 = MD5.Create();
            using (var stream = File.OpenRead(filePath))
            {
                fileSize = stream.Length;
                Byte[] data = md5.ComputeHash(stream);
                return BitConverter.ToString(data).Replace("-", "").ToLower();
            }
        }
        catch (IOException)
        {
            fileSize = 0;
            return "";
        }
    }

    public static string CalcMemMd5(byte[] pData)
    {
        try
        {
            var md5 = MD5.Create();
            using (var stream = new MemoryStream(pData))
            {
 
                Byte[] data = md5.ComputeHash(stream);
                return BitConverter.ToString(data).Replace("-", "").ToLower();
            }
        }
        catch (IOException)
        {
            return "";
        }
    }
     * */

    public static String CalcFileMd5(String filePath)
    {
        return LuaDLL.CalcFileMd5String(filePath);
    }

    public static string CalcMemMd5(IntPtr pData, int dataSize)
    {
        return LuaDLL.CalcMemMd5String(pData, dataSize);
    }

    public static bool WriteToTextFile(string filename, char[] pBuffer, int dataSize)
    {
        StreamWriter writer = null;
        try
        {
            writer = File.CreateText(filename);
        }
        catch(IOException)
        {
            writer = null;
            return false;
        }

        if (writer == null)
            return false;

        writer.Write(pBuffer, 0, dataSize);
        writer.Close();

        return true;
    }
}

