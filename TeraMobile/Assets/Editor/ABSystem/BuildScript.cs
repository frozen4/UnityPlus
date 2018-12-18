//#define MANUAL_MARK_METHOD

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

public class MeshMats
{
    public string meshname;
    public List<string> names;

    public MeshMats()
    {
        names = new List<string>();
    }
}

public class BuildScript
{
    private const string AssetBundlesOutputPath = "/Export/AssetBundles/";

    private static string bundle_path_file = "PATHID.dat";
    private static string[] no_encrypt_paths = new string[] { "Outputs/Scenes/" };

    private static string build_md5_file = "BuildRecord.dat";

    private static string basic_bundle_name_path_file = "Basic_bundle_name_file.dat";
    private static string monsterBundleName = "Monsters";
    private static string bossBundleName = "Boss";
    private static string outwardBundleName = "OutWard";
    private static string blockDenpendenciesName = "scenecommonres";
    private static SortedDictionary<string, string> _buildMD5List = null;
    private static SortedDictionary<string, string> _buildPathIDsList = null;

    private enum BuildType
    {
        All = 0,
        ADD = 1,
    }

    private static List<int> _UnusedIDs = new List<int>();

    [MenuItem("Hoba Tools/AssetBundles/Check Missing Scripts")]
    public static void CheckErrorPrefabs()
    {
        AssetBundleBuild[] prefabs = CollectPrefabBuildInfo(BuildType.All);
        bool isScriptMissing = false;

        if (prefabs.Length > 0)
        {
            for (int i = 0; i < prefabs.Length; i++)
            {
                GameObject m_gameObj = AssetDatabase.LoadAssetAtPath(prefabs[i].assetNames[0], typeof(GameObject)) as GameObject;
                Component[] cs = m_gameObj.GetComponents<Component>();

                for (int j = 0; j < cs.Length; j++)
                {
                    if (cs[j] == null)
                    {
                        isScriptMissing = true;

                        EditorUtility.DisplayDialog("Error 文件缺失", "Check File Dir : \n" +
                            prefabs[i].assetNames[0] +
                            "\n\n" +
                            "Obj Name : \n[    " +
                            cs[j].name +
                            "    ]", "OK");
                    }
                    else
                    {
                        //TEST ERROR OUTPUT
                        /*
                        if (i == 0 && j == 0)
                        {
                            EditorUtility.DisplayDialog("Error 文件缺失", "Check File Dir : \n" +
                                                                       prefabs[i].assetNames[0] +
                                                                       "\n\n" +
                                                                       "Obj Name : \n[    " +
                                                                       cs[j].name +
                                                                       "    ]", "OK");

                            isScriptMissing = true;
                        }
                        */
                    }
                }//j end
            }//i end
        }

        if (isScriptMissing)
        {
            EditorUtility.DisplayDialog("Error!", "File error!\n\nPlease Check!", "OK");
        }
        else
        {
            EditorUtility.DisplayDialog("检查完毕", "\n\n\nWell Done", "OK");
        }
    }

    [MenuItem("Hoba Tools/AssetBundles/Check Prefab Elements")]
    public static void CheckPrefab()
    {
        for (int iType = 0; iType < (int)BuildScript.EPREFAB_ELEMENTS_TYPE.EPREFAB_ELEMENTS_TYPE_MAX; iType++)
        {
            CheckPrefabElements((BuildScript.EPREFAB_ELEMENTS_TYPE)iType);
        }

        if (bFileError)
        {
            EditorUtility.DisplayDialog("Error!", "File error!\n\nPlease Check!", "OK");
        }
        else
        {
            EditorUtility.DisplayDialog("检查完毕", "\n\n\nWell Done", "OK");
        }
    }

    /********************************************************************************
                                    Jenkins Build Api
    ********************************************************************************/

    private static void Switch4Android()
    {
#if !UNITY_ANDROID
        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTarget.Android);
#endif
    }

    private static void Switch4iOS()
    {
#if !UNITY_IPHONE
        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTarget.iOS);
#endif
    }

    private static void Switch4Windows()
    {
#if !UNITY_STANDALONE_WIN
		EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTarget.StandaloneWindows64);
#endif
    }

    private static void Switch4Windows_OSX()
    {
        UnityEngine.Debug.Log("OSX::Switch platform type = " + EditorUserBuildSettings.activeBuildTarget);
#if !UNITY_STANDALONE_OSX
        UnityEngine.Debug.Log("OSX::Switch function in!");
        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTarget.StandaloneOSXIntel);
#else
		UnityEngine.Debug.Log("OSX::It's the same type! Should not switch");
#endif
    }

    private static void WriteAutoBuildTimestamp(string timestampPath)
    {
        try
        {
            StreamWriter sw = new StreamWriter(timestampPath, false);
            sw.WriteLine("AutoBuildTimestamp");

            sw.Flush();
            sw.Close();
        }
        catch (Exception e) { }
    }

    //重打资源包
    private static void AutoBuildBasicAssetBundle4Windows()
    {
        string[] commands = System.Environment.GetCommandLineArgs();
        string timestampPath = commands[commands.Length - 1];

        ReimportUnityEngineUI.ReimportUI();
        BuildBasicAssetBundleByPlatform(BuildTarget.StandaloneWindows64, true);

        WriteAutoBuildTimestamp(timestampPath);
    }

    private static void AutoBuildBasicAssetBundle4iOS()
    {
        string[] commands = System.Environment.GetCommandLineArgs();
        string timestampPath = commands[commands.Length - 1];

        ReimportUnityEngineUI.ReimportUI();
        BuildBasicAssetBundleByPlatform(BuildTarget.iOS, true);

        WriteAutoBuildTimestamp(timestampPath);
    }

    private static void AutoBuildBasicAssetBundle4Android()
    {
        string[] commands = System.Environment.GetCommandLineArgs();
        string timestampPath = commands[commands.Length - 1];

        ReimportUnityEngineUI.ReimportUI();
        BuildBasicAssetBundleByPlatform(BuildTarget.Android, true);

        WriteAutoBuildTimestamp(timestampPath);
    }

    //增量资源包
    private static void AutoBuildUpdateBasicAssetBundle4Windows()
    {
        string[] commands = System.Environment.GetCommandLineArgs();
        string timestampPath = commands[commands.Length - 1];

        ReimportUnityEngineUI.ReimportUI();
        BuildBasicAssetBundleByPlatform(BuildTarget.StandaloneWindows64, false);

        WriteAutoBuildTimestamp(timestampPath);
    }

    private static void AutoBuildUpdateBasicAssetBundle4iOS()
    {
        string[] commands = System.Environment.GetCommandLineArgs();
        string timestampPath = commands[commands.Length - 1];

        ReimportUnityEngineUI.ReimportUI();
        BuildBasicAssetBundleByPlatform(BuildTarget.iOS, false);

        WriteAutoBuildTimestamp(timestampPath);
    }

    private static void AutoBuildUpdateBasicAssetBundle4Android()
    {
        string[] commands = System.Environment.GetCommandLineArgs();
        string timestampPath = commands[commands.Length - 1];

        ReimportUnityEngineUI.ReimportUI();
        BuildBasicAssetBundleByPlatform(BuildTarget.Android, false);

        WriteAutoBuildTimestamp(timestampPath);
    }

#if UI_COMMAND
    [MenuItem("UI/批量操作（请谨慎）/重新生成所有UI")]
    static public void GenerateAllUI()
    {
        string[] allGuids = AssetDatabase.FindAssets("t:Prefab", new string[] { "Assets/Prefabs/UI" });

        //foreach (string guid in allGuids)
        List<GameObject> objs = new List<GameObject>();
        for (int i = 0; i < allGuids.Length; i++)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(allGuids[i]);
            if (assetPath.EndsWith(".prefab"))
            {
                GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                if (null == go) continue;
                objs.Add(go);
            }
        }
        UIGeneratePrefab.GenAllUIPrefab(objs.ToArray());
        AssetDatabase.Refresh();
        AssetDatabase.SaveAssets();
        EditorUtility.ClearProgressBar();
        EditorUtility.DisplayDialog("生成完成", "生成结束！", "OK");
    }
#endif

    static public void AutoBuildDevelopAssetBundle(GameObject go)
    {
        GameObject curSelectedGo = go;
        if (curSelectedGo == null)
        {
            EditorUtility.DisplayDialog("提示", "请先选择要打包的Prefab", "OK");
            return;
        }
        string sourcePath = AssetDatabase.GetAssetPath(curSelectedGo);
        //if (!(sourcePath.Contains("Assets/DevelopInterfaces") || sourcePath.Contains("Assets/Outputs/CG")))
        //{
        //    EditorUtility.DisplayDialog("提示", "只有DevelopInterfaces的测试资源和CG才可以单打", "OK");
        //    return;
        //}

        if (sourcePath.Contains("Assets/DevelopInterfaces"))
        {
            string dic = sourcePath.Replace("DevelopInterfaces", "Outputs/Interfaces");
            GameObject temp = PrefabUtility.CreatePrefab(dic, curSelectedGo, ReplacePrefabOptions.ReplaceNameBased);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            if (null != temp)
            {
                curSelectedGo = temp;
            }
        }

        string cur_path = AssetDatabase.GetAssetPath(curSelectedGo);
        //if(cur_path.Contains("Assets/DevelopInterfaces") || cur_path.Contains("Assets/Outputs/CG"))
        {
            BuildTarget bt = EditorUserBuildSettings.activeBuildTarget;

            string outputPath = Application.dataPath + "/" + ".." + AssetBundlesOutputPath + GetPlatformFolderForAssetBundles(bt);

            DirectoryInfo dir = new DirectoryInfo(outputPath);

            if (!dir.Exists)
            {
                Directory.CreateDirectory(outputPath);
            }
            else
            {
                string ClientPath = Path.Combine(outputPath + "/", "ClientPath.ini");

                if (File.Exists(ClientPath))
                {
                    string line;
                    StreamReader sr = new StreamReader(ClientPath);
                    if ((line = sr.ReadLine()) != null)
                    {
                        ClientPath = line;
                    }
                    sr.Dispose();
                    outputPath = ClientPath;
                }
            }

            AssetBundleBuild[] buildMaps = new AssetBundleBuild[1];
            string dirAssetBundles = Application.dataPath + "/" + ".." + AssetBundlesOutputPath;
            string path_in_database = cur_path;
            //string assetName = path_in_database.Replace("\\", "/").Trim().Replace("DevelopInterfaces", "Outputs/Interfaces");
            string assetName = path_in_database.Replace("\\", "/").Trim();
            buildMaps[0].assetBundleName = AssetDatabase.AssetPathToGUID(path_in_database).ToLower();
            buildMaps[0].assetNames = new string[] { assetName };

            ReadBundleInfo(outputPath, false);

            //string desName = assetName.Replace("DevelopInterfaces", "Outputs/Interfaces");

            if (_buildPathIDsList.ContainsKey(assetName))
            {
                _buildPathIDsList[assetName] = AssetDatabase.AssetPathToGUID(path_in_database).ToLower();
            }
            else
            {
                _buildPathIDsList.Add(assetName, AssetDatabase.AssetPathToGUID(path_in_database).ToLower());
            }
            UpdatePathIDFile(outputPath, _buildPathIDsList);

            BuildAssetBundleOptions options = BuildAssetBundleOptions.ForceRebuildAssetBundle;
            BuildPipeline.BuildAssetBundles(outputPath, buildMaps, options, bt);
            AssetDatabase.Refresh();
            if (1 == _buildPathIDsList.Count)
            {
                EditorUtility.DisplayDialog("提示", "PathID文件只有一条记录请手动复制", "OK");
            }

            //EditorUtility.DisplayDialog("提示", "操作完成", "OK");
            bool needShow = EditorUtility.DisplayDialog("提示", "打包已经完成,是否打开文件夹校验？", "打开", "就不");

            if (needShow)
            {
                OpenExplorer(outputPath + "/" + AssetDatabase.AssetPathToGUID(path_in_database).ToLower());
            }
            //FileUtil.ReplaceFile()
        }

        AssetDatabase.Refresh(); AssetDatabase.Refresh(); AssetDatabase.Refresh(); AssetDatabase.Refresh(); AssetDatabase.Refresh(); AssetDatabase.Refresh();
    }

    [MenuItem("Assets/资源单独打包（仅支持outputs下预设）")]
    static public void BuildDevelopAssetBundle()
    {
        GameObject curSelectedGo = Selection.activeGameObject;
        if (curSelectedGo == null)
        {
            EditorUtility.DisplayDialog("提示", "请先选择要打包的Prefab", "OK");
            return;
        }

        AutoBuildDevelopAssetBundle(curSelectedGo);
    }

    private static void OpenExplorer(string outputPath)
    {
        string str = "";
        for (int i = 0; i < outputPath.Length; ++i)
        {
            char c = outputPath[i];
            if (c == '/')
                c = '\\';
            str += c;
        }
        Utility.ShellExecute(
            IntPtr.Zero,
            "open",
            "Explorer.exe",
            "/select, " + str,
            "",
            Utility.ShowCommands.SW_NORMAL);
    }

    static public void BuildOneAssetToOneAssetBundle()
    {
        GameObject curSelectedGo = Selection.activeGameObject;
        if (curSelectedGo == null)
        {
            EditorUtility.DisplayDialog("提示", "请先选择要打包的Prefab", "OK");
            return;
        }

        string cur_path = AssetDatabase.GetAssetPath(curSelectedGo);

        if (!cur_path.Contains("Assets/DevelopInterfaces"))
        {
            EditorUtility.DisplayDialog("提示", "只有DevelopInterfaces的测试资源才可以单打", "OK");
            return;
        }

        BuildTarget bt = EditorUserBuildSettings.activeBuildTarget;

        string outputPath = Application.dataPath + "/" + ".." + AssetBundlesOutputPath + GetPlatformFolderForAssetBundles(bt);

        AssetBundleBuild[] buildMaps = new AssetBundleBuild[1];

        buildMaps[0].assetBundleName = AssetDatabase.AssetPathToGUID(cur_path).ToLower();

        string dirAssetBundles = Application.dataPath + "/" + ".." + AssetBundlesOutputPath;
        if (null != _buildMD5List)
        {
            _buildMD5List.Clear();
        }

        if (null != _buildPathIDsList)
        {
            _buildPathIDsList.Clear();
        }

        ReadMD5File(outputPath, false);
        ReadBundleInfo(outputPath, false);
        string path_in_database = cur_path;

        string assetName = path_in_database.Replace("\\", "/").Trim();
        ////修正md5
        string md5 = GetMD5HashFromFile(Application.dataPath + cur_path.Replace("Assets", ""));
        if (_buildMD5List.ContainsKey(assetName))
        {
            buildMaps[0].assetNames = new string[] { assetName };
            _buildMD5List[assetName] = md5;
        }
        else
        {
            buildMaps[0].assetBundleName = AssetDatabase.AssetPathToGUID(path_in_database).ToLower();
            buildMaps[0].assetNames = new string[] { assetName };
            _buildMD5List.Add(assetName, md5);
        }

        /////修正PathID
        if (_buildPathIDsList.ContainsKey(assetName))
        {
            _buildPathIDsList[assetName] = AssetDatabase.AssetPathToGUID(path_in_database).ToLower();
        }
        else
        {
            _buildPathIDsList.Add(assetName, AssetDatabase.AssetPathToGUID(path_in_database).ToLower());
        }
        UpdatePathIDFile(outputPath, _buildPathIDsList);
        UpdateMD5File(outputPath, _buildMD5List);
        BuildAssetBundleOptions options = BuildAssetBundleOptions.ForceRebuildAssetBundle;
        BuildPipeline.BuildAssetBundles(outputPath, buildMaps, options, bt);
        EditorUtility.DisplayDialog("提示", "打包完成！！！", "OK");
        AssetDatabase.Refresh();
    }

    public static string GetPlatformFolderForAssetBundles(BuildTarget target)
    {
        switch (target)
        {
            case BuildTarget.Android:
                return "Android";

            case BuildTarget.iOS:
                return "iOS";

            case BuildTarget.StandaloneWindows:
            case BuildTarget.StandaloneWindows64:
                return "Windows";

            case BuildTarget.StandaloneOSXIntel:
                return "Windows";

            case BuildTarget.StandaloneOSXIntel64:
            case BuildTarget.StandaloneOSXUniversal:
                return "OSX";
            // Add more build targets for your own.
            // If you add more targets, don't forget to add the same platforms to GetPlatformFolderForAssetBundles(RuntimePlatform) function.
            default:
                return null;
        }
    }

    private static SortedDictionary<string, string> _buildNewPathIDList = null;

    public static void ForceBuildAssetBundle(BuildTarget bt, bool bRebuild)
    {
        ExportEquipInfos();
        //ExportBasicEquipInfo();
        string dirAssetBundles = Application.dataPath + "/" + ".." + AssetBundlesOutputPath;
        string outputPath = dirAssetBundles + GetPlatformFolderForAssetBundles(bt);
        // 统计原有资源id信息

        if (null == _buildNewPathIDList)
        {
            _buildNewPathIDList = new SortedDictionary<string, string>();
        }
        // 是否清理原有资源
        DirectoryInfo dir = new DirectoryInfo(outputPath);

        if (dir.Exists)
        {
            if (bRebuild)
            {
                dir.Delete(true);
                Directory.CreateDirectory(outputPath);
            }
        }
        else
        {
            Directory.CreateDirectory(outputPath);
        }
        //UnityEngine.Debug.Log("BuildAssetBundles to " + outputPath);
#if MANUAL_MARK_METHOD
		BuildPipeline.BuildAssetBundles(outputPath, 0, EditorUserBuildSettings.activeBuildTarget);
#else
        GenPathIDFile(dirAssetBundles, null, bRebuild);
        //UnityEngine.Debug.Log("AssetBundle Building Is Done");
#endif
    }

    private static string[] CommonAssetPath =
	{
		//"Assets/Surfaces/Atlas/New_Common.png",
		"Assets/Surfaces/Panel_Common.png",
		//"Assets/Surfaces/Atlas/New_MainCity.png",
		"Assets/Fonts/UIFONT.TTF",
		//"Assets/Fonts/HuaKangYT.TTF",
	};

    private static string[] Cubemaps =
	{
		"Assets/Characters/cube/OverCast1_CM.cubemap",
		"Assets/Characters/cube/Sunny3_CM 1.cubemap",
		"Assets/Characters/cube/MoonShine_CM.cubemap",
	};

    private static AssetBundleBuild[] CollectPrefabBuildInfo(BuildType buildType)
    {
        string path = Path.Combine(Application.dataPath, "Outputs");
        if (!path.EndsWith("/")) path = path + "/";
#if UNITY_EDITOR_WIN
        string path2 = path.Replace("/", @"\");
#else
		string path2 = path;
#endif

        string[] fileList = Directory.GetFiles(path2, "*.prefab", SearchOption.AllDirectories);

        List<string> toBuildList = new List<string>();
        string assetPath = null;
        for (int i = 0; i < fileList.Length; i++)
        {
            assetPath = fileList[i].Replace(path2, "").Replace(@"\", "/");

            if (buildType == BuildType.ADD && _buildPathIDsList != null && _buildPathIDsList.ContainsValue(assetPath))
            {
                // 增量build时，已Build的资源部重复打包
                continue;
            }
            toBuildList.Add(assetPath);
        }

        var toBuildCount = toBuildList.Count + CommonAssetPath.Length + Cubemaps.Length;
        if (toBuildCount > 0)
        {
            int curIdx = 0;
            AssetBundleBuild[] buildMaps = new AssetBundleBuild[toBuildCount];
            for (int i = 0; i < toBuildList.Count; i++)
            {
                string path_in_database = "Assets/Outputs/" + toBuildList[i];
                buildMaps[i].assetBundleName = AssetDatabase.AssetPathToGUID(path_in_database).ToLower();
                buildMaps[i].assetNames = new string[] { path_in_database };
            }
            curIdx = toBuildList.Count;

            for (int j = 0; j < CommonAssetPath.Length; j++)
            {
                buildMaps[curIdx + j].assetBundleName = AssetDatabase.AssetPathToGUID(CommonAssetPath[j]).ToLower();
                buildMaps[curIdx + j].assetNames = new string[] { CommonAssetPath[j] };
            }
            curIdx += CommonAssetPath.Length;

            for (int j = 0; j < Cubemaps.Length; j++)
            {
                buildMaps[curIdx + j].assetBundleName = AssetDatabase.AssetPathToGUID(Cubemaps[j]).ToLower();
                buildMaps[curIdx + j].assetNames = new string[] { Cubemaps[j] };
            }

            return buildMaps;
        }

        return null;
    }

    [MenuItem("Hoba Tools/导出胶囊体")]
    public static void ExportCapsule()
    {
        string exportFileName = "CapsuleInfo.dat";
        Dictionary<string, float> capsuleInfoDic = new Dictionary<string, float>();

        string[] allGuids = AssetDatabase.FindAssets("t:Prefab", new string[] { "Assets/Outputs/Characters/Boss"
			,"Assets/Outputs/Characters/NPCs"
			,"Assets/Outputs/Characters/Monsters"
			,"Assets/Outputs/Characters/Players"
			,"Assets/Outputs/Characters/Ride"
			,"Assets/Outputs/Characters/Pets"
		});

        foreach (string guid in allGuids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            GameObject _GO = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (null == _GO) continue;
            CapsuleCollider capColider = _GO.GetComponent<CapsuleCollider>();
            if (null != capColider)
            {
                if (!capsuleInfoDic.ContainsKey(assetPath))
                {
                    capsuleInfoDic.Add(assetPath, capColider.radius);
                }
            }
        }

        string dirAssetBundles = Application.dataPath + "/" + ".." + "/Export/";
        string outputPath = dirAssetBundles;
        string path_id_file = Path.Combine(outputPath + "/", exportFileName);
        using (FileStream fs = new FileStream(path_id_file, FileMode.Create))
        {
            if (!fs.CanWrite)
            {
                UnityEngine.Debug.LogError(string.Format("The {0} Is Locked", path_id_file));
            }
            StreamWriter sw = new StreamWriter(fs);
            foreach (KeyValuePair<string, float> kv in capsuleInfoDic)
            {
                sw.WriteLine(string.Format("{0} {1}", kv.Key, kv.Value));
            }
            sw.Dispose();
            sw.Close();
            fs.Dispose();
            fs.Close();
        }
    }

    [MenuItem("Assets/动画相关/导出选择角色的基础装")]
    public static void ExportSingleBasicEquipInfo()
    {
        var go = Selection.activeGameObject;
        if (null == go) return;

        string srcPath = "Outputs/Characters/Players";
        string srcAssetPath = "Assets/" + srcPath;
        string dstPath = "Assets/Outputs/Characters/Outward/Basic";

        string path = Path.Combine(Application.dataPath, srcPath);
#if UNITY_EDITOR_WIN
        string path2 = path.Replace("/", @"\");
#else
		string path2 = path;
#endif
        var prefabName = go.name;
        string mediatePath = Path.GetDirectoryName(prefabName).Replace(srcAssetPath, "");
        string exportDictionaryName = dstPath + mediatePath.Replace("\\", "/") + "/";
        if (!Directory.Exists(Path.GetDirectoryName(exportDictionaryName)))
            Directory.CreateDirectory(Path.GetDirectoryName(exportDictionaryName));

        string[] basicInfo = new string[] { "face", "body", "hair" };

        for (int basicIndex = 0; basicIndex < basicInfo.Length; basicIndex++)
        {
            var childTran = go.transform.Find(basicInfo[basicIndex]);
            if (null == childTran)
            {
                UnityEngine.Debug.LogError(prefabName + "的 " + basicInfo[basicIndex] + "缺失" + "，请重新导出角色预设后，再次导出基础装");
                return;
            }
            else
            {
                SkinnedMeshRenderer smr = childTran.GetComponent<SkinnedMeshRenderer>();
                if (null == smr)
                {
                    UnityEngine.Debug.LogError(childTran + "的 " + "SkinnedMeshRenderer缺失" + "，请重新导出角色预设后，再次导出基础装");
                    return;
                }
            }
        }
        for (int basicIndex = 0; basicIndex < basicInfo.Length; basicIndex++)
        {
            var childTran = go.transform.Find(basicInfo[basicIndex]);
            if (null != childTran)
            {
                SkinnedMeshRenderer smr = childTran.GetComponent<SkinnedMeshRenderer>();
                if (smr != null)
                {
                    var newPrefabName = exportDictionaryName + prefabName + basicInfo[basicIndex] + ".prefab";
                    var prefab = PrefabUtility.CreatePrefab(newPrefabName, childTran.gameObject, ReplacePrefabOptions.ReplaceNameBased);
                    var info = prefab.AddComponent<OutwardInfo>();
                    info.Mesh = smr.sharedMesh;
                    info.Bones = new string[smr.bones.Length];
                    for (var j = 0; j < smr.bones.Length; j++)
                    {
                        string tmpNodePath = UnityUtil.GetNodePath(smr.bones[j]);
                        string[] nodes = Regex.Split(tmpNodePath, "/");
                        tmpNodePath = tmpNodePath.Replace(nodes[0] + "/", "");
                        info.Bones[j] = tmpNodePath;
                    }
                    info.Material = smr.sharedMaterial;
                    Material mat = smr.sharedMaterial;
                    if (mat != null)
                    {
                        if (mat.HasProperty("_RimColor"))
                        {
                            info.RimColor = mat.GetColor("_RimColor");
                            info.RimPower = mat.GetFloat("_RimPower");
                        }
                        else
                        {
                            UnityEngine.Debug.LogWarningFormat("Material {0} does not have property RimColor", mat.name);
                        }
                        if (mat.HasProperty("_FlakeColorR"))
                        {
                            info.FlakeColorR = mat.GetColor("_FlakeColorR");
                            info.FlakeColorG = mat.GetColor("_FlakeColorG");
                            info.FlakeColorB = mat.GetColor("_FlakeColorB");
                            info.FlakeColorA = mat.GetColor("_FlakeColorA");
                        }
                        else
                        {
                            UnityEngine.Debug.LogWarningFormat("Material {0} does not have property FlakeColor", mat.name);
                        }
                    }
                    ///暂时隐藏该处理，正式调试时需打开
                    smr.sharedMesh = null;
                    smr.sharedMaterial = null;
                }
            }
        }
        AssetDatabase.Refresh();
        AssetDatabase.SaveAssets();
    }

    [MenuItem("Hoba Tools/导出基础装")]
    public static void ExportBasicEquipInfo()
    {
        string srcPath = "Outputs/Characters/Players";
        string srcAssetPath = "Assets/" + srcPath;
        string dstPath = "Assets/Outputs/Characters/Outward/Basic";

        string path = Path.Combine(Application.dataPath, srcPath);
#if UNITY_EDITOR_WIN
        string path2 = path.Replace("/", @"\");
#else
		string path2 = path;
#endif
        string[] wavList = Directory.GetFiles(path2, "*.prefab", SearchOption.AllDirectories);
        List<string> toBuildList = new List<string>();
        string assetPath = null;
        for (int i = 0; i < wavList.Length; i++)
        {
            assetPath = srcAssetPath + wavList[i].Replace(path2, "");
            toBuildList.Add(assetPath);
        }

        for (int i = 0; i < toBuildList.Count; i++)
        {
            //"Assets/Outputs/Characters/Players/Aileen\\Mage\\alimage_f.prefab"
            string prefabName = toBuildList[i];
            string filename = Path.GetFileNameWithoutExtension(prefabName);
            GameObject go = AssetDatabase.LoadAssetAtPath(prefabName, typeof(GameObject)) as GameObject;
            if (go == null)
            {
                UnityEngine.Debug.Log(string.Format("Can't load : {0}", prefabName));
                continue;
            }

            string mediatePath = Path.GetDirectoryName(prefabName).Replace(srcAssetPath, "");
            string exportDictionaryName = dstPath + mediatePath.Replace("\\", "/") + "/";
            if (!Directory.Exists(Path.GetDirectoryName(exportDictionaryName)))
                Directory.CreateDirectory(Path.GetDirectoryName(exportDictionaryName));

            string[] basicInfo = new string[] { "face", "body", "hair" };

            for (int basicIndex = 0; basicIndex < basicInfo.Length; basicIndex++)
            {
                var childTran = go.transform.Find(basicInfo[basicIndex]);
                if (null == childTran)
                {
                    UnityEngine.Debug.LogError(prefabName + "的 " + basicInfo[basicIndex] + "缺失" + "，请重新导出角色预设后，再次导出基础装");
                    return;
                }
                else
                {
                    SkinnedMeshRenderer smr = childTran.GetComponent<SkinnedMeshRenderer>();
                    if (null == smr)
                    {
                        UnityEngine.Debug.LogError(childTran + "的 " + "SkinnedMeshRenderer缺失" + "，请重新导出角色预设后，再次导出基础装");
                        return;
                    }
                }
            }
            for (int basicIndex = 0; basicIndex < basicInfo.Length; basicIndex++)
            {
                var childTran = go.transform.Find(basicInfo[basicIndex]);
                if (null != childTran)
                {
                    SkinnedMeshRenderer smr = childTran.GetComponent<SkinnedMeshRenderer>();
                    if (smr != null)
                    {
                        var newPrefabName = exportDictionaryName + filename + basicInfo[basicIndex] + ".prefab";
                        var prefab = PrefabUtility.CreatePrefab(newPrefabName, childTran.gameObject, ReplacePrefabOptions.ReplaceNameBased);
                        var info = prefab.AddComponent<OutwardInfo>();
                        info.Mesh = smr.sharedMesh;
                        info.Bones = new string[smr.bones.Length];
                        for (var j = 0; j < smr.bones.Length; j++)
                        {
                            string tmpNodePath = UnityUtil.GetNodePath(smr.bones[j]);
                            string[] nodes = Regex.Split(tmpNodePath, "/");
                            tmpNodePath = tmpNodePath.Replace(nodes[0] + "/", "");
                            info.Bones[j] = tmpNodePath;
                        }
                        info.Material = smr.sharedMaterial;
                        Material mat = smr.sharedMaterial;
                        if (mat != null)
                        {
                            if (mat.HasProperty("_RimColor"))
                            {
                                info.RimColor = mat.GetColor("_RimColor");
                                info.RimPower = mat.GetFloat("_RimPower");
                            }
                            else
                            {
                                UnityEngine.Debug.LogWarningFormat("Material {0} does not have property RimColor", mat.name);
                            }
                            if (mat.HasProperty("_FlakeColorR"))
                            {
                                info.FlakeColorR = mat.GetColor("_FlakeColorR");
                                info.FlakeColorG = mat.GetColor("_FlakeColorG");
                                info.FlakeColorB = mat.GetColor("_FlakeColorB");
                                info.FlakeColorA = mat.GetColor("_FlakeColorA");
                            }
                            else
                            {
                                UnityEngine.Debug.LogWarningFormat("Material {0} does not have property FlakeColor", mat.name);
                            }
                        }
                        ///暂时隐藏该处理，正式调试时需打开
                        smr.sharedMesh = null;
                        smr.sharedMaterial = null;
                    }
                }
            }
        }

        AssetDatabase.Refresh();
        AssetDatabase.SaveAssets();
    }

    [MenuItem("Hoba Tools/导出换装资源")]
    static public void ExportEquipInfos()
    {
        string srcPath = "Characters/Outward";
        string srcAssetPath = "Assets/" + srcPath;
        string dstPath = "Assets/Outputs/Characters/Outward";

        string path = Path.Combine(Application.dataPath, srcPath);
#if UNITY_EDITOR_WIN
        string path2 = path.Replace("/", @"\");
#else
		string path2 = path;
#endif
        if (!Directory.Exists(path2))
            return;

        string[] wavList = Directory.GetFiles(path2, "*.prefab", SearchOption.AllDirectories);
        List<string> toBuildList = new List<string>();
        string assetPath = null;
        for (int i = 0; i < wavList.Length; i++)
        {
            assetPath = srcAssetPath + wavList[i].Replace(path2, "");
            toBuildList.Add(assetPath);
        }

        for (int i = 0; i < toBuildList.Count; i++)
        {
            //"Assets/Outputs/Characters/Players/Aileen\\Mage\\alimage_f.prefab"
            string prefabName = toBuildList[i];
            string filename = Path.GetFileNameWithoutExtension(prefabName);
            GameObject go = AssetDatabase.LoadAssetAtPath(prefabName, typeof(GameObject)) as GameObject;
            if (go == null)
            {
                UnityEngine.Debug.Log(string.Format("Can't load : {0}", prefabName));
                continue;
            }

            string mediatePath = Path.GetDirectoryName(prefabName).Replace(srcAssetPath, "");
            string exportDictionaryName = dstPath + mediatePath.Replace("\\", "/") + "/";
            if (!Directory.Exists(Path.GetDirectoryName(exportDictionaryName)))
                Directory.CreateDirectory(Path.GetDirectoryName(exportDictionaryName));

            SkinnedMeshRenderer smr = go.GetComponentInChildren<SkinnedMeshRenderer>();
            if (smr != null)
            {
                var newPrefabName = exportDictionaryName + filename + ".prefab";
                var prefab = PrefabUtility.CreatePrefab(newPrefabName, go, ReplacePrefabOptions.ReplaceNameBased);

                var info = prefab.AddComponent<OutwardInfo>();
                info.Mesh = smr.sharedMesh;
                info.Bones = new string[smr.bones.Length];
                for (var j = 0; j < smr.bones.Length; j++)
                {
                    string tmpNodePath = UnityUtil.GetNodePath(smr.bones[j]);

                    string[] nodes = Regex.Split(tmpNodePath, "/");
                    tmpNodePath = tmpNodePath.Replace(nodes[0] + "/", "");
                    info.Bones[j] = tmpNodePath;
                }

                info.Material = smr.sharedMaterial;

                Material mat = smr.sharedMaterial;
                if (mat != null)
                {
                    if (mat.HasProperty("_RimColor"))
                    {
                        info.RimColor = mat.GetColor("_RimColor");
                        info.RimPower = mat.GetFloat("_RimPower");
                    }
                    else
                    {
                        UnityEngine.Debug.LogWarningFormat("Material {0} does not have property RimColor", mat.name);
                    }

                    if (mat.HasProperty("_FlakeColorR"))
                    {
                        info.FlakeColorR = mat.GetColor("_FlakeColorR");
                        info.FlakeColorG = mat.GetColor("_FlakeColorG");
                        info.FlakeColorB = mat.GetColor("_FlakeColorB");
                        info.FlakeColorA = mat.GetColor("_FlakeColorA");
                    }
                    else
                    {
                        UnityEngine.Debug.LogWarningFormat("Material {0} does not have property FlakeColor", mat.name);
                    }
                }
            }
        }

        AssetDatabase.Refresh();
        AssetDatabase.SaveAssets();
    }

    public static string GetBuildTargetName(BuildTarget target)
    {
        switch (target)
        {
            case BuildTarget.Android:
                return "/test.apk";

            case BuildTarget.StandaloneWindows:
            case BuildTarget.StandaloneWindows64:
                return "/test.exe";

            case BuildTarget.StandaloneOSXIntel:
            case BuildTarget.StandaloneOSXIntel64:
            case BuildTarget.StandaloneOSXUniversal:
                return "/test.app";
            // Add more build targets for your own.
            default:
                UnityEngine.Debug.Log("Target not implemented.");
                return null;
        }
    }

    private static void CopyAssetBundlesTo(string outputPath)
    {
        // Clear streaming assets folder.
        FileUtil.DeleteFileOrDirectory(Application.streamingAssetsPath);
        Directory.CreateDirectory(outputPath);

        string outputFolder = GetPlatformFolderForAssetBundles(EditorUserBuildSettings.activeBuildTarget);

        // Setup the source folder for assetbundles.
        var source = Path.Combine(Path.Combine(System.Environment.CurrentDirectory, AssetBundlesOutputPath), outputFolder);
        if (!System.IO.Directory.Exists(source))
            UnityEngine.Debug.Log("No assetBundle output folder, try to build the assetBundles first.");

        // Setup the destination folder for assetbundles.
        var destination = System.IO.Path.Combine(outputPath, outputFolder);
        if (System.IO.Directory.Exists(destination))
            FileUtil.DeleteFileOrDirectory(destination);

        FileUtil.CopyFileOrDirectory(source, destination);
    }

    private static string[] GetLevelsFromBuildSettings()
    {
        List<string> levels = new List<string>();
        for (int i = 0; i < EditorBuildSettings.scenes.Length; ++i)
        {
            if (EditorBuildSettings.scenes[i].enabled)
                levels.Add(EditorBuildSettings.scenes[i].path);
        }

        return levels.ToArray();
    }

    private static void GenerateEquipResLua()
    {
        string[] wavList = Directory.GetFiles("Export/Lua/Equip/equipConfig/", "*.lua", SearchOption.AllDirectories);
        StringBuilder configBuilder = new StringBuilder();
        configBuilder.AppendLine("return {");

        for (int i = 0; i < wavList.Length; i++)
        {
            string realPath = "Configs/equipConfig/" + Path.GetFileName(wavList[i]);
            configBuilder.AppendLine(string.Format("\"{0}\",", realPath));
        }

        configBuilder.AppendLine("}");
        var utf8WithoutBom = new System.Text.UTF8Encoding(false);
        File.WriteAllText("Export/Lua/Equip/equip_res_map.lua", configBuilder.ToString(), utf8WithoutBom);
    }

    //------------------------------------------------Check Prefab------------------------------------------------
    public enum EPREFAB_ELEMENTS_TYPE
    {
        EPREFAB_ELEMENTS_MESH = 0,
        EPREFAB_ELEMENTS_ATTACH,
        EPREFAB_ELEMENTS_HURT,

        EPREFAB_ELEMENTS_TYPE_MAX,
    };

    private static bool RecursiveCheckElement(GameObject childObject, string elementName)
    {
        bool bRet = false;
        Transform childNext = childObject.transform.Find(elementName);

        if (childNext)
        {
            return true;
        }
        else
        {
            for (int j = 0; j < childObject.transform.childCount; j++)
            {
                GameObject childStep2 = childObject.transform.GetChild(j).gameObject;

                if (childStep2.transform.childCount > 0)
                    if (!RecursiveCheckElement(childStep2, elementName))
                    {
                        continue;
                    }
                    else
                    {
                        bRet = true;
                        break;
                    }
            }
        }

        return bRet;
    }

    public static bool bFileError = false;

    public static void CheckPrefabElements(BuildScript.EPREFAB_ELEMENTS_TYPE type)
    {
        string[] strElementMesh =
		{
			"body",
		};
        string[] strElementAttach =
		{
			"HangPoint_Weapon1",
			"HangPoint_Weapon2",
		};
        string[] strElementHurt =
		{
			"HangPoint_Hurt",
		};

        List<string[]> m_listElements = new List<string[]>()
		{
			strElementMesh,
			strElementAttach,
			strElementHurt,
		};

        AssetBundleBuild[] prefabs = CollectPrefabForType(type);

        bool bGotElementChild = false;

        if (prefabs.Length > 0)
        {
            for (int i = 0; i < prefabs.Length; i++)
            {
                GameObject m_gameObj = AssetDatabase.LoadAssetAtPath(prefabs[i].assetNames[0], typeof(GameObject)) as GameObject;

                for (int m = 0; m < m_listElements[(int)type].Length; m++)
                {
                    bGotElementChild = BuildScript.FindChildElement(m_gameObj, m_listElements[(int)type][m]);

                    if (!bGotElementChild)
                    {
                        bFileError = true;
                    }
                    bGotElementChild = false;
                }
            }//i end
        }
        else
        {
            EditorUtility.DisplayDialog("Error!", "请检查路径!!!", "OK");
        }
    }

    private static AssetBundleBuild[] CollectPrefabForType(BuildScript.EPREFAB_ELEMENTS_TYPE type)
    {
        string[] _path =
		{
			"Characters/",
			"Characters/Players/",
			"Characters/",
		};

        string path = Path.Combine(Application.dataPath, "Outputs/" + _path[(int)type]);
        if (!path.EndsWith("/")) path = path + "/";
#if UNITY_EDITOR_WIN
        string path2 = path.Replace("/", @"\");
#else
		string path2 = path;
#endif

        string[] fileList = Directory.GetFiles(path2, "*.prefab", SearchOption.AllDirectories);

        if (fileList.Length == 0)
        {
            EditorUtility.DisplayDialog("Error 文件不存在!", "请检查路径 : \n" + path2, "OK");
            return null;
        }

        List<string> m_list = new List<string>();

        for (int i = 0; i < fileList.Length; i++)
        {
            if ((type == BuildScript.EPREFAB_ELEMENTS_TYPE.EPREFAB_ELEMENTS_MESH ||
                type == BuildScript.EPREFAB_ELEMENTS_TYPE.EPREFAB_ELEMENTS_HURT) && fileList[i].IndexOf("Weapons") > -1)
            {
                continue;
            }
            m_list.Add(fileList[i]);
        }

        List<string> toBuildList = new List<string>();
        string assetPath = null;
        for (int i = 0; i < m_list.Count; i++)
        {
            assetPath = m_list[i].Replace(path2, "").Replace(@"\", "/");

            toBuildList.Add(assetPath);
        }

        if (toBuildList.Count > 0)
        {
            AssetBundleBuild[] buildMaps = new AssetBundleBuild[toBuildList.Count];
            for (int i = 0; i < toBuildList.Count; i++)
            {
                string path_in_database = "Assets/Outputs/" + _path[(int)type] + toBuildList[i];
                buildMaps[i].assetBundleName = AssetDatabase.AssetPathToGUID(path_in_database).ToLower();
                buildMaps[i].assetNames = new string[] { path_in_database };
            }

            return buildMaps;
        }

        return null;
    }

    private static bool FindChildElement(GameObject gameObj, string childName)
    {
        bool bRet = false;

        if (RecursiveCheckElement(gameObj, childName))
        {
            bRet = true;
        }
        else
        {
            EditorUtility.DisplayDialog("Error 文件缺失", "Check Prefab : " +
                gameObj.name +
                "\nElement : " +
                childName, "OK");
        }

        return bRet;
    }

    #region 菜单栏

    [MenuItem("Hoba Tools/AssetBundles/强制重新导出基础资源包/Windows")]
    static public void BuildBasicAssetBundleForWindows()
    {
        //bSyncInterfaces = false;
        ReimportUnityEngineUI.ReimportUI();
        BuildBasicAssetBundleByPlatform(BuildTarget.StandaloneWindows64, true);
    }

    [MenuItem("Hoba Tools/AssetBundles/强制重新导出基础资源包/IOS")]
    static public void BuildBasicAssetBundleForiOS()
    {
        //bSyncInterfaces = false;
        ReimportUnityEngineUI.ReimportUI();
        BuildBasicAssetBundleByPlatform(BuildTarget.iOS, true);
    }

    [MenuItem("Hoba Tools/AssetBundles/强制重新导出基础资源包/Android")]
    static public void BuildBasicAssetBundleForAndroid()
    {
        //bSyncInterfaces = false;
        ReimportUnityEngineUI.ReimportUI();
        BuildBasicAssetBundleByPlatform(BuildTarget.Android, true);
    }

    public static bool bSyncInterfaces = false;

    //[MenuItem("Hoba Tools/AssetBundles/强制重新导出基础资源包/Windows(本地化同步)")]
    static public void BuildBasicAssetBundleForWindowsWithSync()
    {
        ReimportUnityEngineUI.ReimportUI();
        //bSyncInterfaces = true;
        BuildBasicAssetBundleByPlatform(BuildTarget.StandaloneWindows64, true);
    }

    //[MenuItem("Hoba Tools/AssetBundles/强制重新导出基础资源包/IOS(本地化同步)")]
    static public void BuildBasicAssetBundleForiOSWithSync()
    {
        ReimportUnityEngineUI.ReimportUI();
        //bSyncInterfaces = true;
        BuildBasicAssetBundleByPlatform(BuildTarget.iOS, true);
    }

    // [MenuItem("Hoba Tools/AssetBundles/强制重新导出基础资源包/Android(本地化同步)")]
    static public void BuildBasicAssetBundleForAndroidWithSync()
    {
        ReimportUnityEngineUI.ReimportUI();
        //bSyncInterfaces = true;
        BuildBasicAssetBundleByPlatform(BuildTarget.Android, true);
    }

    [MenuItem("Hoba Tools/AssetBundles/增量导出基础资源包/Windows")]
    static public void BuildUpdateBasicAssetBundleForWindows()
    {
        //bSyncInterfaces = true;
        ReimportUnityEngineUI.ReimportUI();
        BuildBasicAssetBundleByPlatform(BuildTarget.StandaloneWindows64, false);
    }

    [MenuItem("Hoba Tools/AssetBundles/增量导出基础资源包/IOS")]
    static public void BuildUpdateBasicAssetBundleForiOS()
    {
        //bSyncInterfaces = false;
        ReimportUnityEngineUI.ReimportUI();
        BuildBasicAssetBundleByPlatform(BuildTarget.iOS, false);
    }

    [MenuItem("Hoba Tools/AssetBundles/增量导出基础资源包/Android")]
    static public void BuildUpdateBasicAssetBundleForAndroid()
    {
        //bSyncInterfaces = false;
        ReimportUnityEngineUI.ReimportUI();
        BuildBasicAssetBundleByPlatform(BuildTarget.Android, false);
    }

    [MenuItem("Hoba Tools/Warn(仅限打包机)/重新导出基础资源包")]
    static public void BuildBasicAssetBundleActiveTarget()
    {
        //bSyncInterfaces = false;
        ReimportUnityEngineUI.ReimportUI();
        BuildBasicAssetBundleByPlatform(EditorUserBuildSettings.activeBuildTarget, true);
    }

    [MenuItem("Hoba Tools/Warn(仅限打包机)/增量导出基础资源包")]
    static public void BuildUpdateBasicAssetBundleActiveTarget()
    {
        //bSyncInterfaces = false;
        ReimportUnityEngineUI.ReimportUI();
        BuildBasicAssetBundleByPlatform(EditorUserBuildSettings.activeBuildTarget, false);
    }

    #endregion 菜单栏

    static public void BuildBasicAssetBundleByPlatform(BuildTarget target, bool bRebuild)
    {
        EditorUserBuildSettings.activeBuildTargetChanged = delegate()
        {
            BuildBasicBundle(EditorUserBuildSettings.activeBuildTarget, bRebuild);
        };

        if (target == EditorUserBuildSettings.activeBuildTarget)
        {
            BuildBasicBundle(target, bRebuild);
            //EditorUtility.DisplayDialog("提示", "打包完成！", "OK");
        }
        else
        {
            EditorUserBuildSettings.SwitchActiveBuildTarget(target);
        }
    }

    public static List<string> basicBundleNameList = new List<string>();

    public static void BuildBasicBundle(BuildTarget bt, bool bRebuild)
    {
        ForceBuildBasicBundle(bt, bRebuild);
    }

    /// <summary>
    /// 清理备份资源
    /// </summary>
    /// <param name="bt"></param>
    public static void CleanUpBasicBundle(BuildTarget bt)
    {
        string dirAssetBundles = Application.dataPath + "/" + ".." + AssetBundlesOutputPath;
        string outputPath = dirAssetBundles + GetPlatformFolderForAssetBundles(bt);
        string backupPath = outputPath + "/" + "../" + GetPlatformFolderForAssetBundles(bt) + "backup";
        DirectoryInfo dir = new DirectoryInfo(backupPath);
        if (!dir.Exists)
        {
            Directory.CreateDirectory(backupPath);
        }
        else
        {
            foreach (FileInfo fi in dir.GetFiles())
            {
                fi.Delete();
            }
        }
    }

    [MenuItem("Assets/TestExportAnimations")]
    public static void EXportAnimastiondss()
    {
        var clips = AssetDatabase.FindAssets("t:Prefab", new string[] { "Assets/Outputs/Characters/Players" });
        foreach (var item in clips)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(item);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (null == prefab) continue;
            AnimationInfo info = prefab.GetComponent<AnimationInfo>();
            if (null == info) continue;

            foreach (var path in info.animationPaths)
            {
                if (File.Exists("Assets/Characters/" + path))
                {
                    AssetBundleBuild bundle = new AssetBundleBuild();
                    bundle.assetBundleName = "animations";
                    bundle.assetNames = new string[] { "Assets/Characters/" + path };
                }
            }
        }
    }

    public static List<string> commonPaths = new List<string>();
    public static List<string> AssetbundleNameArray = new List<string>();
    public static List<string> tmpArray = new List<string>();

    public static void ForceBuildBasicBundle(BuildTarget bt, bool bRebuild = true)
    {
        AssetDatabase.RemoveUnusedAssetBundleNames();
        string dirAssetBundles = Application.dataPath + "/" + ".." + AssetBundlesOutputPath;
        string outputPath = dirAssetBundles + GetPlatformFolderForAssetBundles(bt);

        string updatePath = dirAssetBundles + GetPlatformFolderForAssetBundles(bt) + "/Update";
        TextLogger.Path = Path.Combine(dirAssetBundles, "BuildBundle日志.log");
        File.Delete(TextLogger.Path);

        TextLogger.Instance.WriteLine("ExportEquipInfos...");       //重新导出装备信息
        if (bRebuild)
        {
            ExportEquipInfos();
        }

        //创建目录
        DirectoryInfo dir = new DirectoryInfo(outputPath);
        if (dir.Exists)
        {
            if (bRebuild)
            {
                dir.Delete(true);
                Directory.CreateDirectory(outputPath);
            }
        }
        else
        {
            Directory.CreateDirectory(outputPath);
        }

        DirectoryInfo dirUpdatePath = new DirectoryInfo(updatePath);
        if (!dirUpdatePath.Exists)
        {
            Directory.CreateDirectory(updatePath);
        }

        TextLogger.Instance.WriteLine("ReadMD5File...");
        ReadMD5File(outputPath, bRebuild);

        TextLogger.Instance.WriteLine("ReadBundleInfo...");
        ReadBundleInfo(outputPath, bRebuild);

        string rootPath = Application.dataPath + "/Outputs";
        string[] directoryEntries = Directory.GetFileSystemEntries(rootPath);
        AssetbundleNameArray.Clear();
        TextLogger.Instance.WriteLine(string.Format("directories in {0}:", rootPath));
        foreach (string item in directoryEntries)
        {
            TextLogger.Instance.WriteLine(string.Format("\t{0}", item));
            //if (!item.Contains("."))
            if (Path.GetExtension(item) == "")
            {
#if UNITY_EDITOR_WIN
                string[] folderName = item.Split('\\');
                AssetbundleNameArray.Add(folderName[1]);
#else
				//UnityEngine.Debug.LogError(item);
				string[] folderName = item.Split('/');
				int length = folderName.Length;
				AssetbundleNameArray.Add(folderName[length - 1 ].ToString());
#endif
            }
        }

        TextLogger.Instance.WriteLine("assetbundleNameArray:");
        foreach (string name in AssetbundleNameArray)
        {
            TextLogger.Instance.WriteLine(string.Format("\t{0}", name));
        }

        //把block依赖项都加到 scenecommonres
        List<AssetBundleBuild> scenesCommonRes = new List<AssetBundleBuild>();
        if (bRebuild)
        {
            scenesCommonRes.Clear();
            commonPaths.Clear();
            string blocksDenpendenciesPath = Application.dataPath + "/../Export/BlocksDenpendencies" + ".txt";
            if (File.Exists(blocksDenpendenciesPath))
            {
                string line;
                StreamReader pathReader = new StreamReader(blocksDenpendenciesPath);
                while ((line = pathReader.ReadLine()) != null)
                {
                    if (File.Exists(line))
                    {
                        AssetBundleBuild bundle = new AssetBundleBuild();

                        bundle.assetBundleName = blockDenpendenciesName;
                        bundle.assetNames = new string[] { line };
                        scenesCommonRes.Add(bundle);
                        commonPaths.Add(line.ToLower());
                    }
                }
                pathReader.Dispose();
            }
        }

        List<AssetBundleBuild> bundleList = new List<AssetBundleBuild>();
        for (int index = 0; index < AssetbundleNameArray.Count; index++)
        {
            AssetBundleBuild[] temp = CollectBasicBuildInfo(AssetbundleNameArray[index], bRebuild);
            for (int innerIndex = 0; innerIndex < temp.Length; innerIndex++)
            {
                if (!bundleList.Contains(temp[innerIndex]))
                {
                    bundleList.Add(temp[innerIndex]);
                }
            }
            for (int i = 0; i < scenesCommonRes.Count; i++)
            {
                if (!bundleList.Contains(scenesCommonRes[i]))
                {
                    bundleList.Add(scenesCommonRes[i]);
                }
            }
        }
        foreach (var item in tmpArray)
        {
            AssetbundleNameArray.Add(item);
        }
     
        //把Characters中的animation打包到 animations
        var clips = AssetDatabase.FindAssets("t:Prefab", new string[] { "Assets/Outputs/Characters" });
        //var clips = AssetDatabase.FindAssets("t:Prefab", new string[] { "Assets/Outputs/Characters/Players" });
        List<string> tmpPaths = new List<string>();
        foreach (var item in clips)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(item);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (null == prefab) continue;
            AnimationInfo info = prefab.GetComponent<AnimationInfo>();
            if (null == info) continue;

            foreach (var path in info.animationPaths)
            {
                var realPath = "Assets/Characters/" + path;
                if (File.Exists(realPath) && !tmpPaths.Contains(realPath))
                {
                    tmpPaths.Add(realPath);
                    AssetBundleBuild bundle = new AssetBundleBuild();
                    bundle.assetBundleName = "animations";
                    bundle.assetNames = new string[] { "Assets/Characters/" + path };
                    bundleList.Add(bundle);
                }
            }
        }

        AssetBundleBuild[] buildMaps = bundleList.ToArray();
        BuildAssetBundleOptions options = BuildAssetBundleOptions.ChunkBasedCompression;
        if (bRebuild)
        {
            options = BuildAssetBundleOptions.ForceRebuildAssetBundle | BuildAssetBundleOptions.ChunkBasedCompression | BuildAssetBundleOptions.DeterministicAssetBundle;
            //options = BuildAssetBundleOptions.ForceRebuildAssetBundle | BuildAssetBundleOptions.ChunkBasedCompression ;
        }
        List<AssetBundleBuild> finallyBuildList = new List<AssetBundleBuild>();
        finallyBuildList.AddRange(bundleList);

        AssetBundleBuild[] finallyBuildMaps = finallyBuildList.ToArray();

        TextLogger.Instance.WriteLine(string.Format("准备BuildAssetBundles Output: {0}", outputPath));
        TextLogger.Instance.WriteLine("finallyBuildMaps:");
        foreach (var build in finallyBuildMaps)
        {
            TextLogger.Instance.WriteLine(string.Format("\tname: {0}, asset: {1}", build.assetBundleName, build.assetNames.Length > 0 ? build.assetNames[0] : "<null>"));
        }
        TextLogger.Instance.WriteLine(string.Format("options: {0}", (int)options));

        BuildPipeline.BuildAssetBundles(outputPath, finallyBuildMaps, options, bt);

        TextLogger.Instance.WriteLine(string.Format("BuildAssetBundles完毕 Output: {0}", outputPath));

        if (bRebuild)
        {
            GenMD5File(outputPath, bRebuild);
        }
        GenPathIDFile(outputPath, buildMaps, bRebuild);
        if (bRebuild)
        {
            AssetbundleNameArray.Add(monsterBundleName);
            AssetbundleNameArray.Add(outwardBundleName);
            AssetbundleNameArray.Add("animations");
            AssetbundleNameArray.Add("scenecommonres");
            //AssetbundleNameArray.Remove("Scenes");
            GenBasicBundleNameFile(outputPath, AssetbundleNameArray.ToArray());
        }
        else
        {
            FileInfo[] files = dir.GetFiles("*", SearchOption.AllDirectories);
            List<string> basicBundleNameList = ReadBasicBundleNameFile(outputPath);
            string destinationFile = "";

            for (int i = 0; i < files.Length; i++)
            {
                //UnityEngine.Debug.LogError(files[i].FullName);
                //   if (files[i].FullName.EndsWith("manifest") || files[i].Name.EndsWith("dat") || files[i].DirectoryName.Contains("Update")
                if (files[i].FullName.EndsWith("manifest") || files[i].DirectoryName.Contains("Update")
                    || files[i].Name.EndsWith(GetPlatformFolderForAssetBundles(bt)) || basicBundleNameList.Contains(files[i].Name.ToLower()))
                {
                    continue;
                }
                else
                {
                    destinationFile = files[i].DirectoryName + "\\Update\\" + files[i].Name;

#if UNITY_EDITOR_WIN
                    string destinationPath = destinationFile;
#else
					string dictoryName = files [i].DirectoryName;
					string destinationPath = dictoryName + "/Update/" + files [i].Name;

#endif

                    files[i].CopyTo(destinationPath, true);
                    //if (!files[i].FullName.EndsWith("dat"))
                    //    files[i].Delete();
                }
                //copy
            }
        }
        // wirte svn local version
        if (bt == BuildTarget.iOS)
            WriteLocalSvnVersion("iOS");
        else if (bt == BuildTarget.Android)
            WriteLocalSvnVersion("Android");
        else
            WriteLocalSvnVersion("Windows");
    }

    public static void GenMD5File(string outputPath, bool bRebuild = false)
    {
        string bundle_md5_info = Path.Combine(outputPath + "/", build_md5_file);
        using (FileStream fs = new FileStream(bundle_md5_info, FileMode.Create))
        {
            if (!fs.CanWrite)
            {
                UnityEngine.Debug.LogError(string.Format("The {0} Is Locked", bundle_md5_info));
            }
            StreamWriter sw = new StreamWriter(fs);
            foreach (KeyValuePair<string, string> kv in _buildMD5List)
            {
                sw.WriteLine(string.Format("{0},{1}", kv.Key, kv.Value));
            }
            sw.Dispose();
            sw.Close();
            fs.Dispose();
            fs.Close();
        }
    }

    public static void ReadBundleInfo(string path, bool bRebuild = false)
    {
        if (null == _buildPathIDsList)
        {
            _buildPathIDsList = new SortedDictionary<string, string>();
        }
        _buildPathIDsList.Clear();

        if (bRebuild) return;
        string bundle_index_path = Path.Combine(path + "/", bundle_path_file);

        if (File.Exists(bundle_index_path))
        {
            string line;
            string[] temp;

            StreamReader sr = new StreamReader(bundle_index_path);
            while ((line = sr.ReadLine()) != null)
            {
                temp = line.Split(',');
                if (!_buildPathIDsList.ContainsKey(temp[1]))
                {
                    _buildPathIDsList.Add(temp[1], temp[0]);
                }
            }
            sr.Dispose();
        }
    }

    public static List<string> ReadBasicBundleNameFile(string path)
    {
        List<string> basicBundleNameList = new List<string>();
        string path_bundle_name_path = Path.Combine(path, basic_bundle_name_path_file);
        if (File.Exists(path_bundle_name_path))
        {
            string line;
            string[] temp;

            StreamReader sr = new StreamReader(path_bundle_name_path);
            while ((line = sr.ReadLine()) != null)
            {
                temp = line.Split(',');
                basicBundleNameList.Add(temp[0].ToLower());
            }
            sr.Dispose();
        }
        return basicBundleNameList;
    }

    public static void ReadMD5File(string path, bool bRebuild = false)
    {
        if (null == _buildMD5List)
        {
            _buildMD5List = new SortedDictionary<string, string>();
        }
        _buildMD5List.Clear();

        if (bRebuild) return;
        string build_record_path = Path.Combine(path + "/", build_md5_file);
        //UnityEngine.Debug.LogError(build_record_path);
        if (File.Exists(build_record_path))
        {
            string line;
            string[] temp;

            StreamReader sr = new StreamReader(build_record_path);
            while ((line = sr.ReadLine()) != null)
            {
                temp = line.Split(',');
                if (!_buildMD5List.ContainsKey(temp[0]))
                {
                    _buildMD5List.Add(temp[0], temp[1]);
                }
            }
            sr.Dispose();
        }
        else
        {
            UnityEngine.Debug.LogError("DDDD");
        }
    }

    private static void CollectAnimations()
    {
    }

    private static string GetMD5HashFromFile(string fileName)
    {
        try
        {
            FileStream file = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
            System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            byte[] retVal = md5.ComputeHash(file);
            file.Close();

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < retVal.Length; i++)
            {
                sb.Append(retVal[i].ToString("x2"));
            }
            return sb.ToString();
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogError(fileName);
            throw new Exception("GetMD5HashFromFile() fail,error:" + ex.Message);
        }
    }

    private static AssetBundleBuild[] CollectBasicBuildInfo(string assetBundleName, bool bRebuild = true)
    {
        string path = Path.Combine(Application.dataPath, "Outputs/" + assetBundleName + "/");
#if UNITY_EDITOR_WIN
        string path2 = path.Replace("/", @"\");
#else
		string path2 = path;
#endif
        string[] fileList = Directory.GetFiles(path2, "*.*", SearchOption.AllDirectories);
        List<string> temp = new List<string>();

        List<string> toBuildList = new List<string>();
        string assetPath = null;

        for (int i = 0; i < fileList.Length; i++)
        {
            if (fileList[i].EndsWith("prefab") ||
                fileList[i].EndsWith("png") ||
                fileList[i].EndsWith("shader") ||
                fileList[i].EndsWith("WAV") || 
                fileList[i].EndsWith("shadervariants") ||
                fileList[i].EndsWith("asset") ||
                fileList[i].EndsWith("wav") ||
                fileList[i].EndsWith("mp3") || 
                fileList[i].EndsWith("MP3") ||
                fileList[i].EndsWith("mat") ||
                fileList[i].EndsWith("TTF") ||
                fileList[i].EndsWith("xml") ||
                fileList[i].EndsWith("cubemap"))
            {
                assetPath = fileList[i].Replace(path2, "");

                toBuildList.Add(assetPath);
                //EditorUtility.DisplayCancelableProgressBar("资源计算", string.Format("{0}/{1}", i, fileList.Length), (float)i / (float)fileList.Length);
            }
        }

        AssetBundleBuild[] assetNameList = new AssetBundleBuild[toBuildList.Count];
        List<AssetBundleBuild> finallyBuildMap = new List<AssetBundleBuild>();

        for (int i = 0; i < toBuildList.Count; i++)
        {
            string path_in_database = "Assets/Outputs/" + assetBundleName + "/" + toBuildList[i];
            string assetName = path_in_database.Replace("\\", "/").Trim();

            string md5 = "";
            if (toBuildList[i].Contains("Outward") && toBuildList[i].EndsWith("prefab"))
            {   //如果是換裝資源的話。拿原始路徑來做MD5對比。
                //md5 = GetMD5HashFromFile(Application.dataPath + "/" + assetBundleName + "/" + toBuildList[i]);

                if (File.Exists(Application.dataPath + "/" + assetBundleName + "/" + toBuildList[i]))
                {
                    md5 = GetMD5HashFromFile(Application.dataPath + "/" + assetBundleName + "/" + toBuildList[i]);
                }
                else
                {
                    md5 = GetMD5HashFromFile(Application.dataPath + "/Outputs/" + assetBundleName + "/" + toBuildList[i]);
                }
            }
            else
            {
                md5 = GetMD5HashFromFile(Application.dataPath + "/Outputs/" + assetBundleName + "/" + toBuildList[i]);
            }

            if (bRebuild)
            {
                if (!commonPaths.Contains(assetName.ToLower()))
                {
                    AssetBundleBuild bundle = new AssetBundleBuild();

                    if ("Characters" == assetBundleName && assetName.Contains("Outputs/Characters/Monsters"))
                    {
                        bundle.assetBundleName = monsterBundleName.Replace(@"\", "/").ToLower();
                    }
                    else if ("Characters" == assetBundleName && assetName.Contains("Outputs/Characters/Outward"))
                    {
                        bundle.assetBundleName = outwardBundleName.ToLower();
                    }
                    else if ("Scenes" == assetBundleName && assetName.Contains("Outputs/Scenes") && assetName.EndsWith(".prefab")
                        && !assetName.Contains("SkySphere"))
                    {
                        bundle.assetBundleName = toBuildList[i].Replace(".prefab", "").ToLower();
                        tmpArray.Add(toBuildList[i].Replace(".prefab", "").ToLower());
                    }
                    else
                    {
                        bundle.assetBundleName = assetBundleName.Replace(@"\", "/").ToLower();
                    }

                    bundle.assetNames = new string[] { assetName };
                    finallyBuildMap.Add(bundle);
                    _buildMD5List.Add(assetName, md5);
                }
            }
            else
            {
                AssetBundleBuild bundle = new AssetBundleBuild();
                if (_buildMD5List.ContainsKey(assetName))
                {
                    if (_buildMD5List[assetName] != md5)
                    {
                        bundle.assetBundleName = AssetDatabase.AssetPathToGUID(path_in_database).ToLower();
                    }
                    else
                    {
                        if (_buildPathIDsList.ContainsKey(assetName))
                        {
                            bundle.assetBundleName = _buildPathIDsList[assetName];
                        }
                        else
                        {
                            UnityEngine.Debug.LogError("更新包有误");
                            bundle.assetBundleName = assetBundleName.Replace(@"\", "/").ToLower();
                        }
                    }
                    bundle.assetNames = new string[] { assetName };
                    finallyBuildMap.Add(bundle);
                }
                else
                {
                    bundle.assetBundleName = AssetDatabase.AssetPathToGUID(path_in_database).ToLower();
                    bundle.assetNames = new string[] { assetName };
                    finallyBuildMap.Add(bundle);
                }
            }
        }

        return finallyBuildMap.ToArray();
    }

    public static void GenPathIDFile(string outputPath, AssetBundleBuild[] buildMaps, bool bRebuild = false)
    {
        string path_id_file = Path.Combine(outputPath + "/", bundle_path_file);
        using (FileStream fs = new FileStream(path_id_file, FileMode.Create))
        {
            if (!fs.CanWrite)
            {
                UnityEngine.Debug.LogError(string.Format("The {0} Is Locked", path_id_file));
            }
            StreamWriter sw = new StreamWriter(fs);
            foreach (AssetBundleBuild item in buildMaps)
            {
                sw.WriteLine(string.Format("{0},{1}", item.assetBundleName, item.assetNames[0]));
            }
            sw.Dispose();
            sw.Close();
            fs.Dispose();
            fs.Close();
        }
    }

    public static void GenBasicBundleNameFile(string outputPath, string[] bundleNames)
    {
        string basic_bundle_name_file = Path.Combine(outputPath, basic_bundle_name_path_file);
        using (FileStream fs = new FileStream(basic_bundle_name_file, FileMode.Create))
        {
            if (!fs.CanWrite)
            {
                UnityEngine.Debug.LogError(string.Format("The {0} Is Locked", basic_bundle_name_file));
            }
            StreamWriter sw = new StreamWriter(fs);
            foreach (string item in bundleNames)
            {
                sw.WriteLine(string.Format("{0}", item.ToLower()));
            }
            sw.Dispose();
            sw.Close();
            fs.Dispose();
            fs.Close();
        }
    }

    public static void UpdatePathIDFile(string outputPath, SortedDictionary<string, string> pathInfo)
    {
        string path_id_file = Path.Combine(outputPath + "/", bundle_path_file);
        using (FileStream fs = new FileStream(path_id_file, FileMode.Create))
        {
            if (!fs.CanWrite)
            {
                UnityEngine.Debug.LogError(string.Format("The {0} Is Locked", path_id_file));
            }
            StreamWriter sw = new StreamWriter(fs);
            foreach (KeyValuePair<string, string> kv in pathInfo)
            {
                sw.WriteLine(string.Format("{0},{1}", kv.Value, kv.Key));
            }
            sw.Dispose();
            sw.Close();
            fs.Dispose();
            fs.Close();
        }
    }

    public static void UpdateMD5File(string outputPath, SortedDictionary<string, string> md5Info)
    {
        string path_id_file = Path.Combine(outputPath + "/", build_md5_file);
        using (FileStream fs = new FileStream(path_id_file, FileMode.Create))
        {
            if (!fs.CanWrite)
            {
                UnityEngine.Debug.LogError(string.Format("The {0} Is Locked", build_md5_file));
            }
            StreamWriter sw = new StreamWriter(fs);
            foreach (KeyValuePair<string, string> kv in md5Info)
            {
                sw.WriteLine(string.Format("{0},{1}", kv.Key, kv.Value));
            }
            sw.Dispose();
            sw.Close();
            fs.Dispose();
            fs.Close();
        }
    }

    [MenuItem("Hoba Tools/TEST/TestCreatePrefab")]
    public static void TestCreatePrefab()
    {
        string path = "Assets/Outputs/zheshiyigeceshi.prefab";
        if (null != Selection.activeGameObject)
        {
            GameObject go = Selection.activeGameObject;
            GameObject tempGo = (GameObject)AssetDatabase.LoadAssetAtPath(path, typeof(GameObject)) as GameObject;
            var prefab = go;
            if (null == tempGo)
            {
                prefab = PrefabUtility.CreatePrefab(path, go, ReplacePrefabOptions.ReplaceNameBased);
                UnityEngine.Debug.LogError("CreatePrefab");
            }
            else
            {
                prefab = PrefabUtility.ReplacePrefab(go, tempGo);
                UnityEngine.Debug.LogError("ReplacePrefab");
            }
            string prefabPath = AssetDatabase.GetAssetPath(prefab);
            string md5 = GetMD5HashFromFile(prefabPath);
            UnityEngine.Debug.LogError(md5);
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private static void executeCommand(string exe, string param, bool wait = true)
    {
        using (Process process = Process.Start(exe, param))
        {
            if (wait)
                process.WaitForExit();
        }
    }

    private static void WriteLocalSvnVersion(string paltform)
    {
        string cmdPath = Application.dataPath + "/../../Tools/get_local_version.cmd";
        executeCommand(cmdPath, paltform);
    }
}