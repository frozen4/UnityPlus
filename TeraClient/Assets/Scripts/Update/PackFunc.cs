using System;
using LuaInterface;
using System.Text;

//更新的基本定义，变量
public static class PackFunc
{
    private static bool Inited = false;

    public static bool PackInitialize(bool bCreate)
    {
        if (Inited)
            return true;

        if (!LuaDLL.PackInitialize(bCreate))
            return false;

        Inited = true;
        return true;
    }

    public static void PackFinalize(bool bForce)
    {
        if (!Inited)
            return;

        LuaDLL.PackFinalize(bForce);

        Patcher.Instance.WritePacking(false);
        Inited = false;
    }

    public static void FlushWritePack()
    {
        LuaDLL.FlushWritePack();
    }

    public static bool SaveAndOpenUpdatePack()
    {
        return LuaDLL.SaveAndOpenUpdatePack();
    }

    public static bool IsFileInPack(string filename)
    {
        return LuaDLL.IsFileInPack(filename);
    }

    public static bool IsFileInAssetBundles(string filename)
    {
        if (filename.IndexOf("AssetBundles/", 0) == 0)
            return true;
        return false;
    }

    public static string MakeShortAssetBundlesFileName(string filename)
    {
        if (filename.IndexOf("AssetBundles/", 0) == 0)
        { 
            string shortFileName = System.IO.Path.GetFileName(filename);
            return "AssetBundles/" + shortFileName;
        }
        else
        {
            return filename;
        }
    }

    public static string CalcPackFileMd5(string filename)
    {
        return LuaDLL.CalcPackFileMd5String(filename);
    }

    public static bool UncompressToSepFile(string filename, IntPtr pData, int dataSize)
    {
        return LuaDLL.UncompressToSepFile(filename, pData, dataSize);
    }

    public static bool AddCompressedDataToPack(string filename, IntPtr pData, int dataSize)
    {
        if (!LuaDLL.AddCompressedDataToPack(filename, pData, dataSize))
            return false;

        Patcher.Instance.WritePacking(true);
        return true;
    }
}
