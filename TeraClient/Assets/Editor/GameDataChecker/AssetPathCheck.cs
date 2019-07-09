using Common;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Template;

namespace GameDataChecker
{
    public class AssetPathCheck : Singleton<AssetPathCheck>
    {
        protected const string AssetBundlesFolderName = "/AssetBundles/";

        // 基础路径设置
        private static string _GameResBasePath = "";
        private static string _BaseAssetBundleURL = "";
        private static string _UpdateAssetBundleURL = "";

        private static HashSet<string> _AssetSet = new HashSet<string>();
        private static List<string> _ErrorPathList = new List<string>();

        [MenuItem("Tools/游戏数据检查/AssetBundle路径检查", false, 1)]
        public static void Check()
        {
            EditorUtility.DisplayProgressBar("提示", "数据初始化", 0.3f);
            Instance.Init();
            EditorUtility.DisplayProgressBar("提示", "数据检测", 0.6f);
            Instance.CheckPath();
            EditorUtility.ClearProgressBar();
            bool bCheck = EditorUtility.DisplayDialog("提示", "检测完成,请查看对应txt文件-M1Client/CheckResult_GameData/AssetBundle路径检查.txt", "好的");
            if (bCheck)
            {
#if UNITY_EDITOR && UNITY_STANDALONE_WIN
                string logDir = System.IO.Path.Combine(Application.dataPath, "../../CheckResult_GameData/");
                if (Directory.Exists(logDir))
                    Util.OpenDir(logDir);
#endif
            }
        }

        public void Init()
        {
            string gameResPath = System.IO.Path.Combine(Application.dataPath, "../../GameRes/");

            //设置assetBundle路径
            string platformFolderForAssetBundles = GetPlatformFolderForAssetBundles();
            _GameResBasePath = gameResPath + AssetBundlesFolderName + platformFolderForAssetBundles + "/";
            _BaseAssetBundleURL = gameResPath + AssetBundlesFolderName + platformFolderForAssetBundles + "/";
            _UpdateAssetBundleURL = _BaseAssetBundleURL + "Update/";

            LoadPathIDData();
        }

        private static string GetPlatformFolderForAssetBundles()
        {
#if UNITY_EDITOR
            switch (EditorUserBuildSettings.activeBuildTarget)
            {
                case BuildTarget.Android:
                    return "Android";
                case BuildTarget.iOS:
                    return "iOS";
                case BuildTarget.StandaloneWindows:
                case BuildTarget.StandaloneWindows64:
                    return "Windows";
                default:
                    return "";
            }
#else
            switch(Application.platform)
		    {
		        case RuntimePlatform.Android:
			        return "Android";
		        case RuntimePlatform.IPhonePlayer:
			        return "iOS";
		        case RuntimePlatform.WindowsPlayer:
			        return "Windows";
		        default:
			        return "";
		    }
#endif
        }

        public static bool IsUpdateFileExist(string filename)
        {
            var fullPath = HobaText.Format("{0}/{1}", _UpdateAssetBundleURL, filename);
            return File.Exists(fullPath);
        }

        private static void LoadPathIDData()
        {
            byte[] pathidContent = null;
            if (IsUpdateFileExist("PATHID.dat"))         //使用更新目录下的pathid.dat
            {
                var filePath = HobaText.Format("{0}{1}", _UpdateAssetBundleURL, "PATHID.dat");
                pathidContent = Util.ReadFile(filePath);
            }
            else
            {
                var filePath = HobaText.Format("{0}{1}", _GameResBasePath, "PATHID.dat");
                pathidContent = Util.ReadFile(filePath);
            }

            if (pathidContent != null && pathidContent.Length > 0)
            {
                StreamReader sr = new StreamReader(new MemoryStream(pathidContent));

                var strLine = sr.ReadLine();
                while (strLine != null)
                {
                    string[] asset_temp = strLine.Split(',');
                    if (asset_temp.Length >= 2)
                    {
                        _AssetSet.Add(asset_temp[1]);
                    }

                    strLine = sr.ReadLine();
                };

                sr.Close();
            }
        }

        private void CheckPath()
        {
            foreach(var asset in _AssetSet)
            {
                if (asset.StartsWith("Assets/Outputs/Shader/"))
                    continue;

                if (asset.Contains(' ') || asset.Contains('\t') || asset.Contains('\n'))
                    _ErrorPathList.Add(asset);
            }

            LogReport("AssetBundle路径检查.txt");
        }

        private void LogReport(string filename)
        {
            //写log 
            string logDir = System.IO.Path.Combine(Application.dataPath, "../../CheckResult_GameData/");
            if (!Directory.Exists(logDir))
                Directory.CreateDirectory(logDir);
            string logFile = System.IO.Path.Combine(logDir, filename);
            TextLogger.Path = logFile;
            File.Delete(logFile);

            _ErrorPathList.Sort();

            if (_ErrorPathList.Count == 0)
            {
                TextLogger.Instance.WriteLine("AssetBundle路径检查!");
            }
            else
            {
                TextLogger.Instance.WriteLine("");
                TextLogger.Instance.WriteLine("检查Asset路径...\n\n");
                foreach (var path in _ErrorPathList)
                {
                    TextLogger.Instance.WriteLine(string.Format("Asset路径错误: {0}", path));
                }
            }
        }
    }
}