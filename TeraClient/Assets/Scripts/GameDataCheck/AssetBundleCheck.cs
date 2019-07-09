using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Common;


public class AssetBundleCheck : Singleton<AssetBundleCheck>
{
    string _AssetBundleBasePath = "";
    string _AssetBundleUpdatePath = "";
    public List<string> _AssetBundleNames = new List<string>();
    Dictionary<string, string> _AssetPath2BundleMap = new Dictionary<string, string>();
    Dictionary<string, List<string>> _Bundle2AssetPathsMap = new Dictionary<string, List<string>>();
    public string _ErrorString = "";


    //
    string[] _Variants = { };
    AssetBundleManifest _AssetBundleManifest = null;
    Dictionary<string, AssetBundle> _LoadedAssetBundles = new Dictionary<string, AssetBundle>();
    Dictionary<string, string[]> _Dependencies = new Dictionary<string, string[]>();

    private string _Platform = "";

    public void Init()
    {
        Release();

        _Platform = GetPlatformFolderForAssetBundles();

        _AssetBundleBasePath = Path.Combine(Application.dataPath, "../../GameRes/AssetBundles/");
        _AssetBundleBasePath = Path.Combine(_AssetBundleBasePath, _Platform + "/");
        _AssetBundleBasePath.NormalizeDir();

        _AssetBundleUpdatePath = Path.Combine(_AssetBundleBasePath, "Update/");
        _AssetBundleUpdatePath.NormalizeDir();

        //read pathid
        GetPathIdInfo();  
        GetAllAssetBundleName();
    }

    public Dictionary<string, string> GetAllAssetPath()
    {
        return _AssetPath2BundleMap;
    }
    private static string GetPlatformFolderForAssetBundles()
    {
#if UNITY_EDITOR
        switch (UnityEditor.EditorUserBuildSettings.activeBuildTarget)
        {
            case UnityEditor.BuildTarget.Android:
                return "Android";
            case UnityEditor.BuildTarget.iOS:
                return "iOS";
            case UnityEditor.BuildTarget.StandaloneWindows:
            case UnityEditor.BuildTarget.StandaloneWindows64:
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

    /*
     * characters
     * outward
     * monsters
     * sfx
     * animations
     * commonatlas
     * scenecommonres
     * */
    public List<string> GetAllAssetNamesOfBundle(string name)
    {
        List<string> ret;
        if (_Bundle2AssetPathsMap.TryGetValue(name, out ret))
            return ret;
        return new List<string>();
    }

    private bool IsUpdateFileExist(string filename)
    {
        string fullFileName = Path.Combine(_AssetBundleUpdatePath, filename);

        return System.IO.File.Exists(fullFileName);
    }

    private void GetAllAssetBundleName()
    {
        _AssetBundleNames.Clear();

        foreach (var name in _Bundle2AssetPathsMap.Keys)
            _AssetBundleNames.Add(name);
        /*
        string dirAssetBundles;
        if (IsUpdateFileExist("Basic_bundle_name_file.dat"))
            dirAssetBundles = _AssetBundleUpdatePath;
        else
            dirAssetBundles = _AssetBundleBasePath;

        byte[] pathid_content;
        pathid_content = File.ReadAllBytes(dirAssetBundles + "Basic_bundle_name_file.dat");
        if (null != pathid_content)
        {
            var ms = new MemoryStream(pathid_content);
            StreamReader sr = new StreamReader(ms);
            string strLine;
            strLine = sr.ReadLine();
            do
            {
                if (strLine != null)
                {
                    _AssetBundleNames.Add(strLine);
                }
                strLine = sr.ReadLine();
            } while (strLine != null);

            sr.Close();
        }
         * */

    }

    private void GetPathIdInfo()
    {
        _AssetPath2BundleMap.Clear();
        _Bundle2AssetPathsMap.Clear();

        string dirAssetBundles;
        if (IsUpdateFileExist("Basic_bundle_name_file.dat") && IsUpdateFileExist("PATHID.dat"))
            dirAssetBundles = _AssetBundleUpdatePath;
        else
            dirAssetBundles = _AssetBundleBasePath;

        var pathid_content = File.ReadAllBytes(dirAssetBundles + "PATHID.dat");
        if (null != pathid_content)
        {
            var ms = new MemoryStream(pathid_content);
            StreamReader sr = new StreamReader(ms);
            string strLine;
            strLine = sr.ReadLine();

            if (strLine != null)
            {
                do
                {
                    string[] asset_temp = strLine.Split(',');
                    if (asset_temp.Length >= 2)
                    {
                        asset_temp[0] = asset_temp[0].ToLower();

                        if (!_AssetPath2BundleMap.ContainsKey(asset_temp[1]))
                        {
                            _AssetPath2BundleMap.Add(asset_temp[1], asset_temp[0]);
                        }

                        if (!_Bundle2AssetPathsMap.ContainsKey(asset_temp[0]))
                        {
                            _Bundle2AssetPathsMap.Add(asset_temp[0], new List<string>());
                        }
                        _Bundle2AssetPathsMap[asset_temp[0]].Add(asset_temp[1]);
                    }

                    strLine = sr.ReadLine();
                } while (strLine != null);
            }

            sr.Close();
        }
    }

    public void Release()
    {
        ClearAllAssetBundles();

        _AssetBundleNames.Clear();
        _AssetPath2BundleMap.Clear();
        _Bundle2AssetPathsMap.Clear();
    }

    private void ClearAllAssetBundles()
    {
        //释放dependency
        foreach (var dependency in _Dependencies)
        {
            foreach (string name in dependency.Value)
            {
                AssetBundle ab;
                if (_LoadedAssetBundles.TryGetValue(name, out ab) && ab != null)
                {
                    ab.Unload(true);
                    _LoadedAssetBundles.Remove(name);
                }
            }
        }

        //释放assetbundle
        foreach (var kv in _LoadedAssetBundles)
        {
            AssetBundle ab = kv.Value;
            ab.Unload(true);
        }

        _Variants = new string[0];
        _AssetBundleManifest = null;
        _LoadedAssetBundles.Clear();
        _Dependencies.Clear();
    }

    public void LoadPlatformAndManifest()
    {
        LoadAssetBundle(_Platform, true);

        AssetBundle ab;
        if (_LoadedAssetBundles.TryGetValue(_Platform, out ab))
            _AssetBundleManifest = ab.LoadAsset<AssetBundleManifest>("AssetBundleManifest");

        if (_AssetBundleManifest == null)
            _ErrorString += string.Format("Load AssetBundleManifest failed from {0}!\n", _Platform);
    }

    public void LoadAssetBundle(string assetBundleName, bool isLoadingAssetBundleManifest = false)
    {
        if (!isLoadingAssetBundleManifest)
            assetBundleName = RemapVariantName(assetBundleName);

        //暂时注掉
        //if (assetBundleName == "interfaces")
        //    assetBundleName = string.Format("{0}{1}", assetBundleName, EntryPoint.Instance.GetUserLanguagePostfix(true));

        // Check if the assetBundle has already been processed.
        bool isAlreadyProcessed = LoadAssetBundleInternal(assetBundleName, isLoadingAssetBundleManifest);

        // Load dependencies.
        if (!isAlreadyProcessed && !isLoadingAssetBundleManifest)
            LoadDependencies(assetBundleName);
    }

    private string RemapVariantName(string assetBundleName)
    {
        string[] bundlesWithVariant = _AssetBundleManifest.GetAllAssetBundlesWithVariant();

        // If the asset bundle doesn't have variant, simply return.
        if (System.Array.IndexOf(bundlesWithVariant, assetBundleName) < 0)
            return assetBundleName;

        string[] split = assetBundleName.Split('.');

        int bestFit = int.MaxValue;
        int bestFitIndex = -1;
        // Loop all the assetBundles with variant to find the best fit variant assetBundle.
        for (int i = 0; i < bundlesWithVariant.Length; i++)
        {
            string[] curSplit = bundlesWithVariant[i].Split('.');
            if (curSplit[0] != split[0])
                continue;

            int found = System.Array.IndexOf(_Variants, curSplit[1]);
            if (found != -1 && found < bestFit)
            {
                bestFit = found;
                bestFitIndex = i;
            }
        }

        if (bestFitIndex != -1)
            return bundlesWithVariant[bestFitIndex];
        else
            return assetBundleName;
    }

    private bool LoadAssetBundleInternal(string assetBundleName, bool isLoadingAssetBundleManifest)
    {
        if (assetBundleName.Length == 0)
        {
            Debug.LogWarning("can not load assetBundle with empty name");
            return true;
        }
        // Already loaded.
        AssetBundle bundle = null;
        if (_LoadedAssetBundles.TryGetValue(assetBundleName, out bundle) && bundle != null)
        {
            return true;
        }

        string url;
        if (IsUpdateFileExist(assetBundleName))
            url = Path.Combine(_AssetBundleUpdatePath, assetBundleName);
        else
            url = Path.Combine(_AssetBundleBasePath, assetBundleName);
        bundle = AssetBundle.LoadFromFile(url);

        if (bundle == null)
        {
            _ErrorString += string.Format("failed to load AssetBundle {0}\n", assetBundleName);
        }
        else
        {
            _LoadedAssetBundles.Add(assetBundleName, bundle);
        }

        return false;
    }

    private void LoadDependencies(string assetBundleName)
    {
        if (_AssetBundleManifest == null)
        {
            Debug.LogError("Please initialize AssetBundleManifest by calling AssetBundleManager.Initialize()");
            return;
        }

        string[] dependencies = _AssetBundleManifest.GetAllDependencies(assetBundleName);
        if (dependencies.Length == 0)
            return;
        for (int i = 0; i < dependencies.Length; i++)
        {
            dependencies[i] = RemapVariantName(dependencies[i]);
            //HobaDebuger.LogWarning("load Dependencies " + dependencies[i]);
        }

        if (!_Dependencies.ContainsKey(assetBundleName))
            _Dependencies.Add(assetBundleName, dependencies);

        for (int i = 0; i < dependencies.Length; i++)
            LoadAssetBundleInternal(dependencies[i], false);
    }

    public Object LoadAsset(string assetName)
    {
        string abName;
        if (!_AssetPath2BundleMap.TryGetValue(assetName, out abName))
            return null;

        assetName = assetName.ToLower();
        abName = abName.ToLower();

        AssetBundle ab;
        if (!_LoadedAssetBundles.TryGetValue(abName, out ab) || ab == null)
            return null;

        Object obj = ab.LoadAsset(assetName);

        return obj;
    }

    public T SyncLoadAssetFromBundle<T>(string assetName) where T : UnityEngine.Object
    {
        if (string.IsNullOrEmpty(assetName))
            return null;

        string abName;
        if (!_AssetPath2BundleMap.TryGetValue(assetName, out abName))
            return null;

        assetName = assetName.ToLower();
        abName = abName.ToLower();

        AssetBundle ab;
        if (!_LoadedAssetBundles.TryGetValue(abName, out ab) || ab == null)
            return null;

        var asset = ab.LoadAsset<T>(assetName);
        return asset;
    }

    public bool IsValidPath(string path)
    {
        return _AssetPath2BundleMap.ContainsKey(path);
    }

    public void LogAssetBundleDependencies()
    {
        foreach(string assetBundleName in _AssetBundleNames)
        {
            _ErrorString += string.Format("\nDependencies of AssetBundle: {0}\n", assetBundleName);

            string[] dependencies = _AssetBundleManifest.GetAllDependencies(assetBundleName);
            if (dependencies.Length == 0)
                continue;
            for (int i = 0; i < dependencies.Length; i++)
            {
                dependencies[i] = RemapVariantName(dependencies[i]);
                //HobaDebuger.LogWarning("load Dependencies " + dependencies[i]);

                _ErrorString += string.Format("\t{0}\n", dependencies[i]);
            }
        }
    }

    public bool CheckComponentMissing(GameObject go)
    {
        bool isMissing = false;
        Component[] components = go.GetComponentsInChildren<Component>(true);
        foreach (var item in components)
        {
            if (null == item)
            {
                isMissing = true;
                break;
            }
        }
        return isMissing;
    }
  
    public void CheckAnimatorUsed(GameObject go, List<string> strListUsed)
    {
        strListUsed.Clear();
        Animator[] animators = go.GetComponentsInChildren<Animator>(true);
        foreach (var item in animators)
        {
            string name = item.gameObject.name;
            if (!strListUsed.Contains(name))
                strListUsed.Add(name);
        }
    }

}
