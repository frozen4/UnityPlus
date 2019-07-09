using Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

#pragma warning disable 0414

public partial class GameDataCheckMan : Singleton<GameDataCheckMan>
{
    public bool bIsUpdateSucceed = false;

    private Text pCurrenVersion = null;
    private Text pServerVersion = null;
    private Text pTextDescription = null;
    private Slider pSliderPart = null;
    private Slider pSliderAll = null;
    public GameObject Frame_Message = null;
    public GameObject Frame_EnterGame = null;

    private Text pTextEnterGameTips = null;  //准备进入游戏
    private Text msgText = null;
    private Text msgTitle = null;

    public bool _AssetBundleInited = false;
    private bool _InCheckMissing = false;

    public GameDataCheckMan()
    {
        pCurrenVersion = GameDataCheck.Instance._PanelHotUpdate.transform.Find("Lab_Tips1").gameObject.GetComponent<Text>();

        pServerVersion = GameDataCheck.Instance._PanelHotUpdate.transform.Find("Lab_Tips2").gameObject.GetComponent<Text>();

        pTextDescription = GameDataCheck.Instance._PanelHotUpdate.transform.Find("Lab_Tips3").gameObject.GetComponent<Text>();

        pSliderPart = GameDataCheck.Instance._PanelHotUpdate.transform.Find("Sld_Part").gameObject.GetComponent<Slider>();

        pSliderAll = GameDataCheck.Instance._PanelHotUpdate.transform.Find("Sld_All").gameObject.GetComponent<Slider>();

        Frame_EnterGame = GameDataCheck.Instance._PanelHotUpdate.FindChild("Frame_EnterGame");
        pTextEnterGameTips = Frame_EnterGame.FindChild("Lab_EnterGameTips").GetComponent<Text>();
        Frame_EnterGame.SetActive(false);

        Frame_Message = GameDataCheck.Instance._PanelMessageBox.FindChild("Frame_Message");
        msgText = Frame_Message.FindChild("Img_MsgContentBG/Lay_Content/Lab_Message").GetComponent<Text>();
        msgTitle = Frame_Message.FindChild("Lab_MsgTitle").GetComponent<Text>();
        Frame_Message.SetActive(false);
    }

    private void SetAllProgress(float progress)
    {
        if (progress < 0.0f) return;

        float value = 0.0f;
        if (progress > 1.0f)
            value = 1.0f;
        else
            value = progress;

        pSliderAll.value = value;
    }

    public void SetPartProgress(float progress)
    {
        if (progress < 0.0f) return;

        float value = 0.0f;
        if (progress > 1.0f)
            value = 1.0f;
        else
            value = progress;

        pSliderPart.value = value;
    }

    public void SetDesc(string desc)
    {
        pTextDescription.text = desc;
    }

    private void SetUpdateSucceed()
    {
        bIsUpdateSucceed = true;
    }

    private void ClearLogFile(string filename)
    {
        //写log
        string logDir = System.IO.Path.Combine(Application.dataPath, "../../CheckResult_GameData/");
        if (!Directory.Exists(logDir))
            Directory.CreateDirectory(logDir);
        string logFile = System.IO.Path.Combine(logDir, filename);
        TextLogger.Path = logFile;
        File.Delete(logFile);
    }

    private void LogReport(string errString)
    {
        if (errString.Length == 0)
        {
            Debug.Log("检查通过!");
        }
        else
        {
            TextLogger.Instance.WriteLine(errString);
        }
    }

    private static void ProcessExceptionReport(string condition, string stackTrace, LogType type)
    {
        if (!GameDataCheckMan.Instance._InCheckMissing || !condition.StartsWith("The referenced script on"))
            return;

        TextLogger.Instance.WriteLine(string.Format("[Unity]: {0}", condition));
    }

    private float _TotalCheck = 13.0f;            //总的检查类型

    public IEnumerable UpdateRoutine()
    {
        GameDataCheck.Instance._PanelHotUpdate.SetActive(true);
        SetAllProgress(0.0f);

        //写log
        CUnityHelper.RegisterLogCallback(ProcessExceptionReport);

        //检查AssetBundle，同时加载所有的assetbundle
        foreach (var item in CheckAssetBundle(1, "AssetBundle资源检查.txt"))
        {
            yield return item;
        }
        yield return new WaitForSeconds(0.2f);
        _AssetBundleInited = true;

        if (GameDataCheck.Instance._bCheckSyncLoad)
        {
            foreach (var item in CheckSyncLoadAB(2, "SyncLoad检查.txt"))
            {
                yield return item;
            }
            yield return new WaitForSeconds(0.2f);
        }

        if (GameDataCheck.Instance._bCheckMissing)
        {
            _InCheckMissing = true;
            foreach (var item in CheckMissingComponent(3, "Missing检查.txt"))
            {
                yield return item;
            }
            _InCheckMissing = false;
            yield return new WaitForSeconds(0.2f);
        }

        if (GameDataCheck.Instance._bCheckAnimator)
        {
            foreach (var item in CheckAnimatorUsed(4, "非特效下的Animator使用.csv"))
            {
                yield return item;
            }
            yield return new WaitForSeconds(0.2f);

        }

        //检查UI prefab
        if (GameDataCheck.Instance._bCheckUIPrefab)
        {
            foreach (var item in CheckLuaPrefab(5, "LuaPrefab资源检查.txt", "UIPrefab使用.csv"))
            {
                yield return item;
            }
            yield return new WaitForSeconds(0.2f);
        }

        //检查Data
        if (GameDataCheck.Instance._bCheckData)
        {
            foreach (var item in CheckLuaData(6, "LuaData资源检查.txt"))
            {
                yield return item;
            }
        }

        yield return new WaitForSeconds(0.2f);

        //检查Quest
        if (GameDataCheck.Instance._bCheckQuest)
        {
            foreach (var item in CheckLuaQuest(7, "LuaQuest资源检查.txt"))
            {
                yield return item;
            }
        }

        yield return new WaitForSeconds(0.2f);

        //检查场景资源
        if (GameDataCheck.Instance._bCheckSceneResource)
        {
            foreach (var item in CheckSceneResource(8, "Scene资源检查.txt"))
            {
                yield return item;
            }
        }

        yield return new WaitForSeconds(0.2f);

        //检查模型资源
        if (GameDataCheck.Instance._bCheckCharacterResource)
        {
            foreach (var item in CheckCharacterResource(9, "Character资源检查.txt", "Character过大贴图.csv", "Character非标准贴图.csv", "Character过大贴图(只贴图).csv"))
            {
                yield return item;
            }
        }

        yield return new WaitForSeconds(0.2f);

        //检查sfx资源
        if (GameDataCheck.Instance._bCheckSfxResource)
        {
            foreach (var item in CheckSfxResource(10, "Sfx资源检查.txt"))
            {
                yield return item;
            }
        }

        yield return new WaitForSeconds(0.2f);

        //检查静态Mesh统计
        if (GameDataCheck.Instance._bCheckStaticMesh)
        {
            foreach (var item in CheckStaticMeshResource(11, "StaticMesh资源统计.txt", "StaticMesh资源统计.csv", "StaticMesh总量统计.csv"))
            {
                yield return item;
            }
        }

        yield return new WaitForSeconds(0.2f);

        //检查Shader统计
        if (GameDataCheck.Instance._bCheckShader)
        {
            foreach (var item in CheckShaderResource(12,
                "Shader被哪些使用_Standard.csv", "Shader被哪些使用_GrabPass.csv", "Shader被哪些使用_KriptoFXParticle.csv",
                "Shader被哪些使用_Distortion_L3.csv", "Shader被哪些使用_Distortion_非L3.csv", "Shader被哪些使用_DepthTexture.csv", "Shader被哪些使用_CutoutBorder.csv", "Shader被哪些使用_非Output下.csv",
                "Shader使用统计_Scene.csv", "Shader使用统计_Character.csv", "Shader使用统计_Sfx.csv", "Shader使用统计_ALL.csv"))
            {
                yield return item;
            }
        }

        yield return new WaitForSeconds(0.2f);

        CUnityHelper.UnregisterLogCallback(ProcessExceptionReport);

        SetAllProgress(1.0f);
        SetDesc("全部检查完毕!");

#if UNITY_EDITOR && UNITY_STANDALONE_WIN
        string logDir = System.IO.Path.Combine(Application.dataPath, "../../CheckResult_GameData/");
        if (Directory.Exists(logDir))
            Util.OpenDir(logDir);
#endif

        SetUpdateSucceed();
        yield return null;
    }

    private IEnumerable CheckSyncLoadAB(int step, string logFile)
    {
        ClearLogFile(logFile);
        SetPartProgress(0.0f);
        SetDesc("检查SyncLoadResource...");

        {
            List<string> allAssets = AssetBundleCheck.Instance.GetAllAssetNamesOfBundle("animations");
            int index = 0;
            foreach (string str in allAssets)
            {
                index++;
                SetDesc(string.Format("正在检查 预设： {0}", System.IO.Path.GetFileName(str)));
                SetPartProgress((float)index / allAssets.Count);
                yield return null;

                if (str.EndsWith(".anim"))
                {
                    if (null == AssetBundleCheck.Instance.SyncLoadAssetFromBundle<AnimationClip>(str))
                    {
                        LogReport(string.Format("animations 同步加载asset失败 AnimationClip： {0}\n", str));
                    }
                }
            }
        }

        {
            List<string> allAssets = AssetBundleCheck.Instance.GetAllAssetNamesOfBundle("animations");
            int index = 0;
            foreach (string str in allAssets)
            {
                index++;
                SetDesc(string.Format("正在检查 预设： {0}", System.IO.Path.GetFileName(str)));
                SetPartProgress((float)index / allAssets.Count);
                yield return null;

                if (str.EndsWith("tga") || str.EndsWith("png"))
                {
                    if (null == AssetBundleCheck.Instance.SyncLoadAssetFromBundle<Sprite>(str))
                    {
                        LogReport(string.Format("commonatlas 同步加载asset失败 Sprite： {0}\n", str));
                    }
                }
                else if (str.EndsWith(".mat"))
                {
                    if (null == AssetBundleCheck.Instance.SyncLoadAssetFromBundle<Material>(str))
                    {
                        LogReport(string.Format("commonatlas 同步加载asset失败 Material： {0}\n", str));
                    }
                }
                else if (str.EndsWith(".TTF"))
                {
                    if (null == AssetBundleCheck.Instance.SyncLoadAssetFromBundle<Font>(str))
                    {
                        LogReport(string.Format("commonatlas 同步加载asset失败 Font： {0}\n", str));
                    }
                }
            }
        }

        {
            List<string> allAssets = AssetBundleCheck.Instance.GetAllAssetNamesOfBundle("animations");
            int index = 0;
            foreach (string str in allAssets)
            {
                index++;
                SetDesc(string.Format("正在检查 预设： {0}", System.IO.Path.GetFileName(str)));
                SetPartProgress((float)index / allAssets.Count);
                yield return null;

                if (str.EndsWith(".mat"))
                {
                    if (null == AssetBundleCheck.Instance.SyncLoadAssetFromBundle<Material>(str))
                    {
                        LogReport(string.Format("commonatlas 同步加载asset失败 Material： {0}\n", str));
                    }
                }
            }
        }

        SetAllProgress(step / _TotalCheck);
        SetPartProgress(1.0f);
        SetDesc("SyncLoadResource完毕!");
        yield return null;
    }

    private IEnumerable CheckMissingComponent(int step, string logFile)
    {
        ClearLogFile(logFile);
        SetPartProgress(0.0f);
        SetDesc("检查CheckMissingComponet...");
        var dic = AssetBundleCheck.Instance.GetAllAssetPath();

        int index = 0;
        foreach (var key in dic.Keys)
        {
            index++;
            SetDesc(string.Format("正在检查 预设： {0}", System.IO.Path.GetFileName(key)));
            SetPartProgress((float)index / dic.Keys.Count);
            yield return null;

            if (key.EndsWith(".prefab"))
            {
                var asset = AssetBundleCheck.Instance.LoadAsset(key) as GameObject;
                if (null == asset)
                {
                    Debug.LogError("nothing with" + key);
                    continue;
                }
                if (AssetBundleCheck.Instance.CheckComponentMissing(asset))
                {
                    LogReport(string.Format("有脚本丢失的预设有： {0}\n", key));
                }
            }
        }

        SetAllProgress(step / _TotalCheck);
        SetPartProgress(1.0f);
        SetDesc("CheckMissingComponet完毕!");
        yield return null;
    }

    private IEnumerable CheckAnimatorUsed(int step, string csvFile)
    {
        SetPartProgress(0.0f);
        SetDesc("检查CheckAnimatorUsed...");
        var dic = AssetBundleCheck.Instance.GetAllAssetPath();
        List<string> sfxList = AssetBundleCheck.Instance.GetAllAssetNamesOfBundle("sfx");
        Dictionary<string, string> dicOutput = new Dictionary<string, string>();


        int index = 0;
        foreach (var key in dic.Keys)
        {
            if (sfxList.Contains(key))          //排除sfx目录 
                continue;

            index++;
            SetDesc(string.Format("正在检查 预设： {0}", System.IO.Path.GetFileName(key)));
            SetPartProgress((float)index / (dic.Keys.Count - sfxList.Count));
            yield return null;

            if (key.EndsWith(".prefab"))
            {
                var asset = AssetBundleCheck.Instance.LoadAsset(key) as GameObject;
                if (null == asset)
                {
                    Debug.LogError("nothing with" + key);
                    continue;
                }

                List<string> list = new List<string>();
                AssetBundleCheck.Instance.CheckAnimatorUsed(asset, list);

                if (list.Count > 0)
                {
                    string str = string.Empty;
                    for (int i = 0; i < list.Count; ++i)
                        str += (list[i] + "\t");

                    if (!dicOutput.ContainsKey(key))
                        dicOutput.Add(key, str);
                }
            }
        }

        WriteCsvFile(csvFile, dicOutput);

        SetAllProgress(step / _TotalCheck);
        SetPartProgress(1.0f);
        SetDesc("CheckAnimatorUsed完毕!");
        yield return null;
    }

    private IEnumerable CheckAssetBundle(int step, string logFile)
    {
        SetPartProgress(0.0f);
        SetDesc("检查AssetBundle...");
        yield return null;

        ClearLogFile(logFile);

        AssetBundleCheck.Instance.Init();

        //检查
        AssetBundleCheck.Instance._ErrorString = "";
        AssetBundleCheck.Instance.LoadPlatformAndManifest();

        //加载所有ab
        for (int i = 0; i < AssetBundleCheck.Instance._AssetBundleNames.Count; ++i)
        {
            string abName = AssetBundleCheck.Instance._AssetBundleNames[i];

            SetDesc(string.Format("正在检查 AssetBundle： {0}", abName));
            SetPartProgress((float)i / AssetBundleCheck.Instance._AssetBundleNames.Count);
            yield return null;

            AssetBundleCheck.Instance.LoadAssetBundle(abName, false);
        }

        //初始化shaderManager
        LoadShaderList();

        AssetBundleCheck.Instance.LogAssetBundleDependencies();

        if (!string.IsNullOrEmpty(AssetBundleCheck.Instance._ErrorString))
            LogReport(AssetBundleCheck.Instance._ErrorString);

        SetAllProgress(step / _TotalCheck);
        SetPartProgress(1.0f);
        SetDesc("检查AssetBundle完毕!");
        yield return null;
    }

    private void LoadShaderList()
    {
        var path = "Assets/Outputs/Shader/ShaderList.prefab";
        var asset = AssetBundleCheck.Instance.LoadAsset(path) as GameObject;
        BuildShaderList(asset);
    }

    private void BuildShaderList(GameObject shaderListObj)
    {
        //ShaderManager.Instance._ShaderMap.Clear();

        if (shaderListObj != null)
        {
            ShaderList compShaderList = shaderListObj.GetComponent<ShaderList>();
            if (compShaderList != null && compShaderList.Shaders != null)
            {
                try
                {
                    int nCount = compShaderList.Shaders.Length;
                    HobaDebuger.LogFormat("ShaderManager.Init, Shaders Count: {0}", nCount);
                    for (int i = 0; i < nCount; ++i)
                    {
                        Shader shader = compShaderList.Shaders[i];
                        if (shader == null)
                        {
                            Debug.LogErrorFormat("ShaderManager.Init, Shader {0} is missing!", i);
                            AssetBundleCheck.Instance._ErrorString += string.Format("ShaderManager.Init, Shader {0} is missing!", i);
                        }
                    }
                }
                catch (ArgumentException ae)
                {
                    HobaDebuger.LogError(ae.Message);
                }
            }
        }
    }

    public static bool IsPOT(uint x)
    {
        return (x > 0) && ((x & (x - 1)) == 0);
    }

    //
    private IEnumerable CheckLuaPrefab(int step, string logFile, string csvFile)
    {
        SetPartProgress(0.0f);
        SetDesc("检查LuaPrefab...");
        yield return null;

        ClearLogFile(logFile);

        LuaPrefabCheck.Instance.Init();

        //检查
        LuaPrefabCheck.Instance._ErrorString = "";

        //解析lua文件
        LuaPrefabCheck.Instance.CollectPrefabs();
        LuaPrefabCheck.Instance.CollectObjectCfgs();
        LuaPrefabCheck.Instance.CollectLuaClasses();

        //检测非UI prefab是否存在
        {
            int count = 0;
            int total = LuaPrefabCheck.Instance._ResPathNoUIPrefabSet.Count + LuaPrefabCheck.Instance._LuaPrefabSet.Count;
            foreach (string prefab in LuaPrefabCheck.Instance._ResPathNoUIPrefabSet)
            {
                ++count;
                SetDesc(string.Format("正在检查 prefab： {0}", prefab));
                SetPartProgress((float)count / total);
                yield return null;

                LuaPrefabCheck.Instance.LoadAsset(prefab);
            }

            foreach (string prefab in LuaPrefabCheck.Instance._LuaPrefabSet)
            {
                ++count;
                SetDesc(string.Format("正在检查 prefab： {0}", prefab));
                SetPartProgress((float)count / total);
                yield return null;

                LuaPrefabCheck.Instance.LoadAsset(prefab);
            }
        }

        //检测UI的prefab和ObjectCfg
        SortedList<string, LuaPrefabCheck.CLuaClass> list = new SortedList<string, LuaPrefabCheck.CLuaClass>();
        {
            string[] ignoredUINames = new string[] {
            };

            int count = 0;
            int total = LuaPrefabCheck.Instance._MapLuaClass.Count;
            foreach (var kv in LuaPrefabCheck.Instance._MapLuaClass)
            {
                ++count;
                SetDesc(string.Format("正在检查 UI： {0}", kv.Key));
                SetPartProgress((float)count / total);
                yield return null;

                bool bFind = false;
                foreach (var name in ignoredUINames)
                {
                    if (name == kv.Key)
                    {
                        bFind = true;
                        break;
                    }
                }
                if (bFind)
                    continue;

                string prefabName = LuaPrefabCheck.Instance.CheckUIClass(kv.Key, kv.Value);
                if (!string.IsNullOrEmpty(prefabName) && !list.ContainsKey(prefabName))
                    list.Add(prefabName, kv.Value);

                /*string prefabName1 =*/ LuaPrefabCheck.Instance.CheckUIParentClass(kv.Key, kv.Value);
                if (!string.IsNullOrEmpty(prefabName) && !list.ContainsKey(prefabName))
                    list.Add(prefabName, kv.Value);
            }

            SetDesc(string.Format("正在检查UI使用..."));
            yield return null;

            string dir = LuaPrefabCheck.Instance._LuaBasePath;
            List<string> luaFileList = new List<string>();
            Common.Utilities.ListFiles(new DirectoryInfo(dir), ".lua", luaFileList);

            count = 0;
            total = luaFileList.Count;
            for (int i = 0; i < luaFileList.Count; ++i)
            {
                ++count;
                SetDesc(string.Format("正在检查UI使用： {0}", luaFileList[i]));
                SetPartProgress((float)count / total);
                yield return null;

                LuaPrefabCheck.Instance.CheckReferenced(luaFileList[i], LuaPrefabCheck.Instance._MapLuaClass);
            }
            

            //输出luaUIClass
            /*
            string uiClassString = "";
            foreach(string uiName in LuaPrefabCheck.Instance._LuaUIClassList)
            {
                uiClassString += string.Format("{0} = ", uiName);
                uiClassString += "{},\n";
            }
            if (!string.IsNullOrEmpty(uiClassString))
                LogReport("LuaUI.txt", uiClassString);
             * */
        }

        System.GC.Collect();
        UnityEngine.Resources.UnloadUnusedAssets();

        WriteCsvFile(csvFile, list);

        if (!string.IsNullOrEmpty(LuaPrefabCheck.Instance._ErrorString))
            LogReport(LuaPrefabCheck.Instance._ErrorString);
        else
            LogReport("检查通过!");

        SetAllProgress(step / _TotalCheck);
        SetPartProgress(1.0f);
        SetDesc("检查LuaPrefab完毕!");

        yield return null;
    }

    //
    private IEnumerable CheckLuaData(int step, string logFile)
    {
        SetPartProgress(0.0f);
        SetDesc("检查Data...");
        yield return null;

        ClearLogFile(logFile);

        //检查
        LuaDataCheck.Instance._ErrorString = "";

        LuaDataCheck.Instance.Init();
        LuaDataCheck.Instance.InitData();

        SetDesc("正在检查Data模型数据");
        yield return null;
        foreach (var item in LuaDataCheck.Instance.CheckModelAssetCoroutine())
            yield return item;

        SetDesc("正在检查Data图标数据");
        yield return null;
        foreach (var item in LuaDataCheck.Instance.CheckIconPathCoroutine())
            yield return item;

        //写入检查数据
        LuaDataCheck.Instance.ShowDataCheckInfo();

        System.GC.Collect();
        UnityEngine.Resources.UnloadUnusedAssets();

        if (!string.IsNullOrEmpty(LuaDataCheck.Instance._ErrorString))
            LogReport(LuaDataCheck.Instance._ErrorString);
        else
            LogReport("检查通过!");

        SetAllProgress(step / _TotalCheck);
        SetPartProgress(1.0f);
        SetDesc("检查Data完毕!");
        yield return null;
    }

    private IEnumerable CheckLuaQuest(int step, string logFile)
    {
        SetPartProgress(0.0f);
        SetDesc("检查Quest...");
        yield return null;

        ClearLogFile(logFile);

        //检查
        LuaQuestCheck.Instance._ErrorString = "";

        LuaQuestCheck.Instance.Init();

        SetDesc("正在检查quest的npc数据");
        yield return null;
        foreach (var item in LuaQuestCheck.Instance.CheckQuestNpcCoroutine())
            yield return item;

        SetDesc("正在检查quest的monster数据");
        yield return null;
        foreach (var item in LuaQuestCheck.Instance.CheckQuestMonsterCoroutine())
            yield return item;

        SetDesc("正在检查quest的mine数据");
        yield return null;
        foreach (var item in LuaQuestCheck.Instance.CheckQuestMineCoroutine())
            yield return item;

        SetDesc("正在检查quest的主quest线数据");
        yield return null;
        foreach (var item in LuaQuestCheck.Instance.CheckQuestMainCoroutine())
            yield return item;

        System.GC.Collect();
        UnityEngine.Resources.UnloadUnusedAssets();

        if (!string.IsNullOrEmpty(LuaQuestCheck.Instance._ErrorString))
            LogReport(LuaQuestCheck.Instance._ErrorString);
        else
            LogReport("检查通过!");

        SetAllProgress(step / _TotalCheck);
        SetPartProgress(1.0f);
        SetDesc("检查Quest完毕!");
        yield return null;
    }

    //
    private IEnumerable CheckSceneResource(int step, string logFile)
    {
        SetPartProgress(0.0f);
        SetDesc("检查场景资源...");
        yield return null;

        SceneResourceCheck.Instance.Init(256, 1024);

        //检查
        SceneResourceCheck.Instance._ErrorString = "";
        //SceneResourceCheck.Instance._DicErrorString.Clear();

        //
        HashSet<string> sceneAssetPathSet = new HashSet<string>();
        foreach (var scene in SceneResourceCheck.Instance._SceneData.Values)
        {
            string assetPath = scene.AssetPath;
            if (!AssetBundleCheck.Instance.IsValidPath(assetPath))
            {
                SceneResourceCheck.Instance._ErrorString += string.Format("scene 错误的assetPath! assetTid: {0}\n", assetPath);
                continue;
            }
            string prefab = assetPath;
            sceneAssetPathSet.Add(prefab);
        }

        //添加选择角色和创建角色
        sceneAssetPathSet.Add("Assets/Outputs/Scenes/SelectChar.prefab");
        sceneAssetPathSet.Add("Assets/Outputs/Scenes/CreatCharacter.prefab");

        int count = 0;
        int total = sceneAssetPathSet.Count;
        foreach (string assetPath in sceneAssetPathSet)
        {
            ++count;
            SetDesc(string.Format("检查场景: {0}", assetPath));
            SetPartProgress((float)count / total);
            yield return null;

            //string name = System.IO.Path.GetFileNameWithoutExtension(assetPath);
            //string sceneLogFile = "场景资源检查_" + name + ".txt";
            //ClearLogFile(sceneLogFile);

            //SceneResourceCheck.Instance._DicErrorString.Add(name, "");

            foreach (var item in CheckSceneAssetCoroutine(assetPath))
                yield return item;

            //if (!string.IsNullOrEmpty(SceneResourceCheck.Instance._DicErrorString[name]))
            //    LogReport(sceneLogFile, SceneResourceCheck.Instance._DicErrorString[name]);
        }
        System.GC.Collect();
        UnityEngine.Resources.UnloadUnusedAssets();

        ClearLogFile(logFile);
        if (!string.IsNullOrEmpty(SceneResourceCheck.Instance._ErrorString))
            LogReport(SceneResourceCheck.Instance._ErrorString);
        else
            LogReport("检查通过!");

        SetAllProgress(step / _TotalCheck);
        SetPartProgress(1.0f);
        SetDesc("检查场景资源完毕!");
        yield return null;
    }

    //
    private IEnumerable CheckSceneAssetCoroutine(string assetPath)
    {
        SceneResourceCheck.Instance._ErrorString += "\n";
        string prefab = assetPath;
        var asset = AssetBundleCheck.Instance.LoadAsset(prefab);
        if (asset == null)
        {
            SceneResourceCheck.Instance._ErrorString += string.Format("asset 加载失败! assetPath: {0}\n", prefab);
            yield break;
        }

        //创建
        if (asset != null)
        {
            //TextLogger.Instance.WriteLine(string.Format("Checking Scene: {0}", prefab));
            var obj = GameObject.Instantiate(asset) as GameObject;

            foreach (var item in SceneResourceCheck.Instance.DoCheckSceneCoroutine(prefab, obj))
                yield return item;

            GameObject.DestroyImmediate(obj);
        }
    }

    //
    private IEnumerable CheckCharacterResource(int step, string logFile, string largeCsvFile, string noStandardCsvFile, string largeTextureCsvFile)
    {
        SetPartProgress(0.0f);
        SetDesc("检查角色资源...");
        yield return null;

        ClearLogFile(logFile);

        CharacterResourceCheck.Instance.Init(512);

        //检查
        CharacterResourceCheck.Instance._ErrorString = "";
        CharacterResourceCheck.Instance.LargeTextureList.Clear();
        CharacterResourceCheck.Instance.NoStandardTextureList.Clear();

        int count = 0;
        int total = CharacterResourceCheck.Instance._AssetPathList.Count;
        foreach (string prefab in CharacterResourceCheck.Instance._AssetPathList)
        {
            ++count;

            string shortName = Path.GetFileNameWithoutExtension(prefab);
            SetDesc(string.Format("检查Character: {0}", shortName));
            SetPartProgress((float)count / total);
            yield return null;

            foreach (var item in CheckCharacterAssetCoroutine(prefab))
                yield return item;
        }

        System.GC.Collect();
        UnityEngine.Resources.UnloadUnusedAssets();

        CharacterResourceCheck.Instance.SortListEntries(CharacterResourceCheck.Instance.LargeTextureList);
        CharacterResourceCheck.Instance.SortListEntries(CharacterResourceCheck.Instance.NoStandardTextureList);
        WriteCsvFile(largeCsvFile, CharacterResourceCheck.Instance.LargeTextureList);
        WriteCsvFile(noStandardCsvFile, CharacterResourceCheck.Instance.NoStandardTextureList);

        List<CharacterResourceCheck.CSimpleTextureInfo> simpleTextureList = CharacterResourceCheck.Instance.GetSimpleTextureList(CharacterResourceCheck.Instance.LargeTextureList);
        CharacterResourceCheck.Instance.SortListEntries(simpleTextureList);
        WriteCsvFile(largeTextureCsvFile, simpleTextureList);

        if (!string.IsNullOrEmpty(CharacterResourceCheck.Instance._ErrorString))
            LogReport(CharacterResourceCheck.Instance._ErrorString);
        else
            LogReport("检查通过!");

        SetAllProgress(step / _TotalCheck);
        SetPartProgress(1.0f);
        SetDesc("检查角色资源完毕!");
        yield return null;
    }

    private IEnumerable CheckCharacterAssetCoroutine(string assetPath)
    {
        var asset = AssetBundleCheck.Instance.LoadAsset(assetPath);
        if (asset == null)
        {
            CharacterResourceCheck.Instance._ErrorString += string.Format("asset 加载失败! assetPath: {0}\n", assetPath);
            yield break;
        }

        //创建
        if (asset != null)
        {
            //TextLogger.Instance.WriteLine(string.Format("Checking Scene: {0}", prefab));
            var obj = GameObject.Instantiate(asset) as GameObject;

            foreach (var item in CharacterResourceCheck.Instance.DoCheckSkinMeshRendererCoroutine(assetPath, obj))
                yield return item;

            GameObject.DestroyImmediate(obj);
        }
    }

    private IEnumerable CheckSfxResource(int step, string logFile)
    {
        SetPartProgress(0.0f);
        SetDesc("检查特效资源...");
        yield return null;

        ClearLogFile(logFile);

        SfxResourceCheck.Instance.Init(512);

        //检查
        SfxResourceCheck.Instance._ErrorString = "";

        int count = 0;
        int total = SfxResourceCheck.Instance._AssetPathList.Count;
        foreach (string prefab in SfxResourceCheck.Instance._AssetPathList)
        {
            ++count;

            string shortName = Path.GetFileNameWithoutExtension(prefab);
            SetDesc(string.Format("检查Sfx: {0}", shortName));
            SetPartProgress((float)count / total);
            yield return null;

            foreach (var item in CheckSfxAssetCoroutine(prefab))
                yield return item;
        }

        System.GC.Collect();
        UnityEngine.Resources.UnloadUnusedAssets();

        if (!string.IsNullOrEmpty(SfxResourceCheck.Instance._ErrorString))
            LogReport(SfxResourceCheck.Instance._ErrorString);
        else
            LogReport("检查通过!");

        SetAllProgress(step / _TotalCheck);
        SetPartProgress(1.0f);
        SetDesc("检查特效资源完毕!");
        yield return null;
    }

    private IEnumerable CheckSfxAssetCoroutine(string assetPath)
    {
        var asset = AssetBundleCheck.Instance.LoadAsset(assetPath);
        if (asset == null)
        {
            SfxResourceCheck.Instance._ErrorString += string.Format("asset 加载失败! assetPath: {0}\n", assetPath);
            yield break;
        }

        //创建
        if (asset != null)
        {
            //TextLogger.Instance.WriteLine(string.Format("Checking Scene: {0}", prefab));
            var obj = GameObject.Instantiate(asset) as GameObject;

            foreach (var item in SfxResourceCheck.Instance.DoCheckParticleSystemCoroutine(assetPath, obj))
                yield return item;

            //foreach (var item in SfxResourceCheck.Instance.DoCheckFxLOD(prefab, obj))
            //    yield return item;

            GameObject.DestroyImmediate(obj);
        }
    }

    private IEnumerable CheckStaticMeshResource(int step, string logFile, string csvFile, string csvFile2)
    {
        SetPartProgress(0.0f);
        SetDesc("检查场景Mesh...");
        yield return null;

        ClearLogFile(logFile);

        StaticMeshResourceCheck.Instance.Init(10000);

        //检查
        StaticMeshResourceCheck.Instance._ErrorString = "";
        StaticMeshResourceCheck.Instance._ListEntries.Clear();

        //
        HashSet<string> sceneAssetPathSet = new HashSet<string>();
        foreach (var scene in StaticMeshResourceCheck.Instance._SceneData.Values)
        {
            string assetPath = scene.AssetPath;
            if (!AssetBundleCheck.Instance.IsValidPath(assetPath))
            {
                StaticMeshResourceCheck.Instance._ErrorString += string.Format("scene 错误的assetPath! assetTid: {0}\n", assetPath);
                continue;
            }
            string prefab = assetPath;
            sceneAssetPathSet.Add(prefab);
        }

        //添加选择角色和创建角色
        sceneAssetPathSet.Add("Assets/Outputs/Scenes/SelectChar.prefab");
        sceneAssetPathSet.Add("Assets/Outputs/Scenes/CreatCharacter.prefab");

        int count = 0;
        int total = sceneAssetPathSet.Count;
        foreach (string assetPath in sceneAssetPathSet)
        {
            ++count;
            SetDesc(string.Format("检查场景: {0}", assetPath));
            SetPartProgress((float)count / total);
            yield return null;

            StaticMeshResourceCheck.Instance._ErrorString += string.Format("\n检查场景Prefab: {0}\n", assetPath);

            foreach (var item in CheckStaticMeshAssetCoroutine(assetPath))
                yield return item;
        }
        System.GC.Collect();
        UnityEngine.Resources.UnloadUnusedAssets();

        StaticMeshResourceCheck.Instance.SortListEntries();
        WriteCsvFile(csvFile, StaticMeshResourceCheck.Instance._ListEntries);

        WriteCsvFile(csvFile2, StaticMeshResourceCheck.Instance._DicPrefabMeshInfo, StaticMeshResourceCheck.Instance._DicBlockMeshInfo);

        if (!string.IsNullOrEmpty(StaticMeshResourceCheck.Instance._ErrorString))
            LogReport(StaticMeshResourceCheck.Instance._ErrorString);
        else
            LogReport("检查通过!");

        SetAllProgress(step / _TotalCheck);
        SetPartProgress(1.0f);
        SetDesc("检查场景Mesh完毕!");
        yield return null;
    }

    private IEnumerable CheckStaticMeshAssetCoroutine(string assetPath)
    {
        string prefab = assetPath;
        var asset = AssetBundleCheck.Instance.LoadAsset(prefab);
        if (asset == null)
        {
            StaticMeshResourceCheck.Instance._ErrorString += string.Format("asset 加载失败! assetPath: {0}\n", prefab);
            yield break;
        }

        //创建
        if (asset != null)
        {
            //TextLogger.Instance.WriteLine(string.Format("Checking Scene: {0}", prefab));
            var obj = GameObject.Instantiate(asset) as GameObject;

            foreach (var item in StaticMeshResourceCheck.Instance.DoCheckMeshCoroutine(prefab, obj))
                yield return item;

            //
            foreach (var kv in StaticMeshResourceCheck.Instance._DicBlockMeshInfo)
            {
                string prefabName = kv.Key;
                var meshInfo = kv.Value;

                StaticMeshResourceCheck.Instance._ErrorString += string.Format("\n\t静态Mesh检查: {0}\n", prefabName);

                StaticMeshResourceCheck.Instance._ErrorString += string.Format("\t\t静态Mesh总面数: {0}\n", meshInfo.CalcTotalFaces());

                foreach (var entry in meshInfo.meshList)
                {
                    if (entry.faces > StaticMeshResourceCheck.Instance.MeshFaceLimit)
                        StaticMeshResourceCheck.Instance._ErrorString += string.Format("\t\t\t静态Mesh： {0} 面数: {1}\n", entry.name, entry.faces);
                }
            }

            GameObject.DestroyImmediate(obj);
        }
    }

    private IEnumerable CheckShaderResource(int step,
        string logFile_Standard, string logFile_GrabPass, string logFile_KriptoFXParticle,
        string logFile_DistortionL3, string logFile_DistortionNoL3, string logFile_DepthTexture, string logFile_CutoutBorder, string logFile_NoOutput,
        string sceneCsv, string characterCsv, string sfxCsv, string csvFile)
    {
        SetPartProgress(0.0f);
        SetDesc("检查Shader...");
        yield return null;

        ShaderResourceCheck.Instance.Init();

        //检查
        ShaderResourceCheck.Instance.ClearAllShaderList();
        //SceneResourceCheck.Instance._DicErrorString.Clear();

        //检查场景
        {
            HashSet<string> sceneAssetPathSet = new HashSet<string>();
            foreach (var scene in ShaderResourceCheck.Instance._SceneData.Values)
            {
                string assetPath = scene.AssetPath;
                if (!AssetBundleCheck.Instance.IsValidPath(assetPath))
                {
                    continue;
                }
                string prefab = assetPath;
                sceneAssetPathSet.Add(prefab);
            }

            //添加选择角色和创建角色
            sceneAssetPathSet.Add("Assets/Outputs/Scenes/SelectChar.prefab");
            sceneAssetPathSet.Add("Assets/Outputs/Scenes/CreatCharacter.prefab");

            int count = 0;
            int total = sceneAssetPathSet.Count;
            foreach (string assetPath in sceneAssetPathSet)
            {
                ++count;
                SetDesc(string.Format("检查场景: {0}", assetPath));
                SetPartProgress((float)count / total);
                yield return null;

                foreach (var item in CheckShader_SceneAssetCoroutine(assetPath))
                    yield return item;
            }
        }

        //检查Character
        {
            int count = 0;
            int total = ShaderResourceCheck.Instance._CharacterAssetPathList.Count;
            foreach (string prefab in ShaderResourceCheck.Instance._CharacterAssetPathList)
            {
                ++count;

                string shortName = Path.GetFileNameWithoutExtension(prefab);
                SetDesc(string.Format("检查Character: {0}", shortName));
                SetPartProgress((float)count / total);
                yield return null;

                foreach (var item in CheckShader_CharacterAssetCoroutine(prefab))
                    yield return item;
            }
        }

        //检查Sfx
        {
            int count = 0;
            int total = ShaderResourceCheck.Instance._SfxAssetPathList.Count;
            foreach (string prefab in ShaderResourceCheck.Instance._SfxAssetPathList)
            {
                ++count;

                string shortName = Path.GetFileNameWithoutExtension(prefab);
                SetDesc(string.Format("检查Sfx: {0}", shortName));
                SetPartProgress((float)count / total);
                yield return null;

                foreach (var item in CheckShader_SfxAssetCoroutine(prefab))
                    yield return item;
            }
        }

        System.GC.Collect();
        UnityEngine.Resources.UnloadUnusedAssets();

        WriteCsvFile(sceneCsv, ShaderResourceCheck.Instance._SceneShaderDic);
        WriteCsvFile(characterCsv, ShaderResourceCheck.Instance._CharacterShaderDic);
        WriteCsvFile(sfxCsv, ShaderResourceCheck.Instance._SfxShaderDic);
        WriteCsvFile(csvFile, ShaderResourceCheck.Instance._AllShaderDic);

        WriteCsvFile(logFile_Standard, ShaderResourceCheck.Instance._UsedShaders_Standard);
        WriteCsvFile(logFile_GrabPass, ShaderResourceCheck.Instance._UsedShaders_NoUsedGrabPass);
        WriteCsvFile(logFile_KriptoFXParticle, ShaderResourceCheck.Instance._UsedShaders_KriptoFXParticle);
        WriteCsvFile(logFile_DistortionL3, ShaderResourceCheck.Instance._UsedShaders_Distortion_L3);
        WriteCsvFile(logFile_DistortionNoL3, ShaderResourceCheck.Instance._UsedShaders_Distortion_NoL3);
        WriteCsvFile(logFile_DepthTexture, ShaderResourceCheck.Instance._UsedShaders_DepthTexture);
        WriteCsvFile(logFile_CutoutBorder, ShaderResourceCheck.Instance._UsedShaders_CutoutBorder);
        WriteCsvFile(logFile_NoOutput, ShaderResourceCheck.Instance._UsedShaders_NoOutput);

        SetAllProgress(step / _TotalCheck);
        SetPartProgress(1.0f);
        SetDesc("检查Shader完毕!");
        yield return null;
    }

    private IEnumerable CheckShader_SceneAssetCoroutine(string assetPath)
    {
        string prefab = assetPath;
        var asset = AssetBundleCheck.Instance.LoadAsset(prefab);
        if (asset == null)
        {
            yield break;
        }

        //创建
        //TextLogger.Instance.WriteLine(string.Format("Checking Scene: {0}", prefab));
        var obj = GameObject.Instantiate(asset) as GameObject;

        foreach (var item in ShaderResourceCheck.Instance.DoCheckSceneCoroutine(prefab, obj))
            yield return item;

        GameObject.DestroyImmediate(obj);
    }

    private IEnumerable CheckShader_CharacterAssetCoroutine(string assetPath)
    {
        string prefab = assetPath;
        var asset = AssetBundleCheck.Instance.LoadAsset(prefab);
        if (asset == null)
        {
            yield break;
        }

        //创建
        //TextLogger.Instance.WriteLine(string.Format("Checking Scene: {0}", prefab));
        var obj = GameObject.Instantiate(asset) as GameObject;

        foreach (var item in ShaderResourceCheck.Instance.DoCheckCharacterCoroutine(prefab, obj))
            yield return item;

        GameObject.DestroyImmediate(obj);
    }

    private IEnumerable CheckShader_SfxAssetCoroutine(string assetPath)
    {
        string prefab = assetPath;
        var asset = AssetBundleCheck.Instance.LoadAsset(prefab);
        if (asset == null)
        {
            yield break;
        }

        //创建
        //TextLogger.Instance.WriteLine(string.Format("Checking Scene: {0}", prefab));
        var obj = GameObject.Instantiate(asset) as GameObject;

        foreach (var item in ShaderResourceCheck.Instance.DoCheckSfxCoroutine(prefab, obj))
            yield return item;

        GameObject.DestroyImmediate(obj);
    }
}