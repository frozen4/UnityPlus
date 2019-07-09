using System;
using System.IO;
using Common;

public partial class EntryPoint
{
    /// 版本号获取
    static string _ABVersion = "";
    static string _ClientVersion = "";

    public string ABVersion
    {
        get { return _ABVersion; }
    }

    public string ClientVersion
    {
        get { return _ClientVersion; }
    }

    private void LoadVersionData()
    {
        if (Instance == null)
            throw new Exception("Failed to Initialize");

        byte[] abVersionContent;
        if (CAssetBundleManager.IsUpdateFileExist("abversion.txt"))         //使用更新目录下的abversion.txt
        {
            string dirAssetBundles = CAssetBundleManager.UpdateAssetBundleURL;
            abVersionContent = Util.ReadFile(dirAssetBundles + "abversion.txt");

            HobaDebuger.LogFormat("Use abversion.txt from Update Dir: {0}", dirAssetBundles);
        }
        else
        {
            string dirAssetBundles = CAssetBundleManager.GameResBasePath;
            abVersionContent = Util.ReadFile(dirAssetBundles + "abversion.txt");

            HobaDebuger.LogFormat("Use abversion.txt from Base Dir: {0}", dirAssetBundles);
        }

        if (null != abVersionContent)
        {
            StreamReader sr = new StreamReader(new MemoryStream(abVersionContent));
            string strLine = sr.ReadLine();
            if (strLine != null)
                _ABVersion = strLine;
            sr.Close();
        }
        else
        {
            _ABVersion = "0";  //所有文件夹下都没有对应的版本信息。默认为0
        }
        //HobaDebuger.LogFormat("Use abversion.txt ReadLine: {0}", _ABVersion);

        byte[] clientVersionContent;
        if (CAssetBundleManager.IsUpdateFileExist("clientversion.txt"))         //使用更新目录下的clientversion.txt
        {
            string dirAssetBundles = CAssetBundleManager.UpdateAssetBundleURL;
            clientVersionContent = Util.ReadFile(dirAssetBundles + "clientversion.txt");

            HobaDebuger.LogFormat("Use clientversion.txt from Update Dir: {0}", dirAssetBundles);
        }
        else
        {
            string dirAssetBundles = CAssetBundleManager.GameResBasePath;
            clientVersionContent = Util.ReadFile(dirAssetBundles + "clientversion.txt");

            HobaDebuger.LogFormat("Use clientversion.txt from Base Dir: {0}", dirAssetBundles);
        }

        if (null != clientVersionContent)
        {
            StreamReader sr = new StreamReader(new MemoryStream(clientVersionContent));
            string strLine = sr.ReadLine();
            if (strLine != null)
                _ClientVersion = strLine;
            sr.Close();
        }
        else
        {
            _ClientVersion = "0";  //所有文件夹下都没有对应的版本信息。默认为0
        }
        //HobaDebuger.LogFormat("Use clientversion.txt ReadLine: {0}", _ClientVersion);
    }
}
