#define USING_LOAD_FROM_FILE

using System;
using System.Collections;
using UnityEngine;
#if UNITY_EDITOR	
using UnityEditor;
#endif
using System.Collections.Generic;
using Common;
using UnityObject = UnityEngine.Object;
using System.IO;

public class LoadedAssetBundle
{
    public string BundleName;
    public AssetBundle Bundle;
    public int RefCount;
    public bool DoNotRelease;

    public LoadedAssetBundle(AssetBundle bundle, string name)
    {
        BundleName = name;
        Bundle = bundle;
        RefCount = 1;
        DoNotRelease = false;
    }
}

public class LoadedAsset
{
    public float BornTime;
    public UnityEngine.Object Asset;

    public LoadedAsset(UnityEngine.Object asset)
    {
        Asset = asset;
        BornTime = Time.time;
    }
}

public class AssetLoaderRef
{
    public AssetBundleCreateRequest DownloadRequest;
    public int RefCount;
    public bool DoNotReleaseBundle;

    public AssetLoaderRef(string url, int refcount, bool donot)
    {
        url = url.Replace("file://", "");
        DownloadRequest = AssetBundle.LoadFromFileAsync(url);
        RefCount = refcount;
        DoNotReleaseBundle = donot;
    }
};

public class CAssetBundleManager : MonoBehaviourSingleton<CAssetBundleManager>
{
    protected const string AssetBundlesFolderName = "/AssetBundles/";

    // 基础路径设置
    private static string _GameResBasePath = "";
    public static string GameResBasePath { get { return _GameResBasePath; } }

    private static string _BaseAssetBundleURL = "";
    public static string BaseAssetBundleURL { get { return _BaseAssetBundleURL; } }

    private static string _UpdateAssetBundleURL = "";
    public static string UpdateAssetBundleURL { get { return _UpdateAssetBundleURL; } }

#if USE_VARIANT
    static string[] _Variants =  {  };
#endif

    // PathId 信息
    private Dictionary<string, string> _AssetPath2BundleMap = new Dictionary<string, string>();

    // ab依赖信息
    static AssetBundleManifest _AssetBundleManifest = null;

    private static bool _IsOK = false;
    public static bool IsOK { get { return _IsOK; } }
    
    static Dictionary<string, string[]> _Dependencies = new Dictionary<string, string[]>();

    /// <summary>
    ///   加载流程
    ///   1、应用层发起加载请求 AssetBundleLoadOperation
    ///   2、在_LoadedAssetBundles寻找是否已经加载？如果已加载，直接返回
    ///   3、如果未在缓存，添加请求至_ABLoadRequests；
    ///   4、Tick中查询添加请求至_ABLoadRequests状态，成功，则将加载的Bundle放入_LoadedAssetBundles
    ///   5、Tick中查询_LoadedAssetBundles是否存在AssetBundleLoadOperation中需要的资源，如有Operation完成
    /// </summary>

    /// 应用层发起的资源加载操作请求列表
    static List<AssetBundleLoadOperation> _InProgressOperations = new List<AssetBundleLoadOperation>();
    /// 完成加载，在管理器缓存的AB列表
    static Dictionary<string, LoadedAssetBundle> _LoadedAssetBundles = new Dictionary<string, LoadedAssetBundle> ();
    /// 正在处理中的AB加载请求列表
    static Dictionary<string, AssetLoaderRef> _ABLoadRequests = new Dictionary<string, AssetLoaderRef>();
    /// 加载出错资源信息列表
    static Dictionary<string, string> _DownloadingErrors = new Dictionary<string, string> ();
    /// 完成加载，在管理器缓存的AB列表
    static Dictionary<string, LoadedAsset> _LoadedAssetsCache = new Dictionary<string, LoadedAsset>();
    /// 永不释放资源路径
    static HashSet<string> _DoNotReleaseBundleSet = new HashSet<string>();

    List<string> _KeysToRemove = new List<string>();

#if DEBUG_DELAY
    // For Debug
    public static float ResLoadDelay = 0f;
#endif
    public void Init(string gameResPath, string assetBundlePath)
    {
        //GetTaskQueue().AddLoopActionInMainThread(() => Tick());
        StartCoroutine(TaskCoroutine());

        // 计算所用到的路径
        string platformFolderForAssetBundles = GetPlatformFolderForAssetBundles();
        _GameResBasePath = gameResPath + AssetBundlesFolderName + platformFolderForAssetBundles + "/";
        _BaseAssetBundleURL = assetBundlePath + AssetBundlesFolderName + platformFolderForAssetBundles + "/";

        //Update
#if UNITY_EDITOR //临时使用本地目录
        _UpdateAssetBundleURL = _BaseAssetBundleURL + "Update/";
#else                //使用更新目录, 将 AssetBundle/<Plaform> 和 AssetBundle/<Plaform>/Update 整合成AssetBundle
        string strLibDir = EntryPoint.Instance.LibPath;
        _UpdateAssetBundleURL = strLibDir + AssetBundlesFolderName;
#endif

        HobaDebuger.LogWarningFormat("BaseAssetBundleURL: {0}", _BaseAssetBundleURL);
        HobaDebuger.LogWarningFormat("UpdateAssetBundleURL: {0}", _UpdateAssetBundleURL);

        LoadPathIDData();

        string manifestAssetBundleName = platformFolderForAssetBundles;
        LoadAssetBundle(manifestAssetBundleName, true);
        Action<UnityObject> onManifestOk = (asset) =>
        {
            AssetBundleManifest abm = asset as AssetBundleManifest;
            _AssetBundleManifest = abm;
            _IsOK = true;
        };
        var operation = new AssetBundleLoadManifestOperation(manifestAssetBundleName, "AssetBundleManifest", onManifestOk);
        _InProgressOperations.Add(operation);

    }

    public static bool IsAssetBundleLoading()
    {
        return _InProgressOperations.Count > 0;
    }

    public static int GetAssetBundleLoadingCount()
    {
        return _InProgressOperations.Count;
    }

    public static void AddDoNotReleaseBundle(List<string> bundleNames)
    {
        if(bundleNames == null) return;
        for(int i = 0; i < bundleNames.Count; i++)
            _DoNotReleaseBundleSet.Add(bundleNames[i]);
    }

    public static void AsyncLoadBundle(string bundleName, Action<UnityObject> onLoadFinish, bool doNotRelease)
    {
        AsyncLoadResourceInternal("", bundleName, onLoadFinish, doNotRelease);
    }

    public static void UnloadBundle(string bundleName)
    {
        LoadedAssetBundle lab;
        if (_LoadedAssetBundles.TryGetValue(bundleName, out lab))
        {
            lab.Bundle.Unload(false);
            _LoadedAssetBundles.Remove(bundleName);
            HobaDebuger.LogFormat("Unload unuesd bundles {0}", bundleName);
        }
    }

    public static void CleanupUncachedBundles()
    {
        var unloadList = new List<LoadedAssetBundle>();
        var itor = _LoadedAssetBundles.GetEnumerator();
        while (itor.MoveNext())
        {
            var lab = itor.Current.Value;
            if(!ShouldKeepItInCache(lab))
                unloadList.Add(lab);
        }
        itor.Dispose();

        for (int i = 0; i < unloadList.Count; i++)
        {
            var lab = unloadList[i];
            lab.Bundle.Unload(false);
            _LoadedAssetBundles.Remove(lab.BundleName);
            HobaDebuger.LogFormat("Unload unuesd bundles {0}", lab.BundleName);
        }
    }

    public static void AddAssetCache(string assetPath, LoadedAsset asset)
    {
        if (!_LoadedAssetsCache.ContainsKey(assetPath))
            _LoadedAssetsCache.Add(assetPath, asset);
    }

    public static void AsyncLoadResource(string assetName, Action<UnityObject> onLoadFinish, string bundleName = null)
    {
        LoadedAsset cache;
        if(_LoadedAssetsCache.TryGetValue(assetName, out cache))
        {
            cache.BornTime = Time.time;
            onLoadFinish(cache.Asset);
            return;
        }

        if(string.IsNullOrEmpty(bundleName))
            bundleName = Instance.GetBundleName(assetName);

        if (string.IsNullOrEmpty(bundleName))
        {
            onLoadFinish(null);
            return;
        }

        var isBundleAlwayCached = _DoNotReleaseBundleSet.Contains(bundleName);
        AsyncLoadResourceInternal(assetName, bundleName, onLoadFinish, isBundleAlwayCached);
    }

    public static T SyncLoadAssetFromBundle<T>(string assetName, string bundleName = null) where T : UnityObject
    {
        if (string.IsNullOrEmpty(assetName))
            return null;

        LoadedAsset cache;
        if (_LoadedAssetsCache.TryGetValue(assetName, out cache))
        {
            cache.BornTime = Time.time;
            T result = cache.Asset as T;
            return result;
        }

        if (string.IsNullOrEmpty(bundleName))
            bundleName = Instance.GetBundleName(assetName);

        if (string.IsNullOrEmpty(bundleName))
        {
            HobaDebuger.LogWarningFormat("{0} cant be load,because it cant find bundle name", assetName);
            return null;
        }

        string error = String.Empty;
        LoadedAssetBundle bundle = GetLoadedAssetBundle(bundleName,out error);
        if (null == bundle)
        {
            HobaDebuger.LogWarningFormat("AssetBundle {0} has not been loaded first", bundleName);
            return null;
        }
    
        var asset = bundle.Bundle.LoadAsset<T>(assetName);

        //var newCache = new LoadedAsset(asset);
        //CAssetBundleManager.AddAssetCache(assetName, newCache);

        return asset;
    }

    static public LoadedAssetBundle GetLoadedAssetBundle (string assetBundleName, out string error)
	{
		if (_DownloadingErrors.TryGetValue(assetBundleName, out error) )
			return null;
	
		LoadedAssetBundle bundle = null;
		_LoadedAssetBundles.TryGetValue(assetBundleName, out bundle);
		if (bundle == null)
			return null;
		
		// No dependencies are recorded, only the bundle itself is required.
		string[] dependencies = null;
		if (!_Dependencies.TryGetValue(assetBundleName, out dependencies) )
			return bundle;
		
		// Make sure all dependencies are loaded
		//foreach(var dependency in dependencies)
        for (int i = 0; i < dependencies.Length; ++i)
        {
            if (_DownloadingErrors.TryGetValue(assetBundleName, out error))
                return bundle;

            // Wait all the dependent assetBundles being loaded.
            LoadedAssetBundle dependentBundle;
            _LoadedAssetBundles.TryGetValue(dependencies[i], out dependentBundle);
            if (dependentBundle == null)
                return null;
        }

		return bundle;
	}

    private string GetBundleName(string assetName)
    {
        if (string.IsNullOrEmpty(assetName)) return null;

        string bundleName = string.Empty; ;
        if (_AssetPath2BundleMap.TryGetValue(assetName, out bundleName))
            return bundleName;

        var lowerName = assetName.ToLower();

        var itor = _DoNotReleaseBundleSet.GetEnumerator();
        while (itor.MoveNext())
        {
            var keywords = HobaText.Format("/{0}/", itor.Current);
            if (lowerName.Contains(keywords))
            {
                bundleName = itor.Current;
                break;
            }
        }
        itor.Dispose();

        if (string.IsNullOrEmpty(bundleName))
            HobaDebuger.LogErrorFormat("Failed to find bundle when load asset {0}", assetName);

        return bundleName;
    }

    static private IEnumerator DelayCallOnLoadFinish(float delay, Action<UnityEngine.Object> callback, UnityEngine.Object asset)
    {
        yield return new WaitForSeconds(delay);
        callback(asset);
    }

    static protected void LoadAssetBundle(string assetBundleName, bool isLoadingAssetBundleManifest = false, bool isBundleAlwayCached = false)
	{
		if (!isLoadingAssetBundleManifest)
			assetBundleName = RemapVariantName (assetBundleName);

        //暂时注掉
        //if (assetBundleName == "interfaces")
        //    assetBundleName = HobaString.Format("{0}{1}", assetBundleName, EntryPoint.Instance.GetUserLanguagePostfix(true));

		// Check if the bundle has already been processed.
		bool isAlreadyProcessed = LoadAssetBundleInternal(assetBundleName, isLoadingAssetBundleManifest, isBundleAlwayCached);

		// Load dependencies.
		if (!isAlreadyProcessed && !isLoadingAssetBundleManifest)
			LoadDependencies(assetBundleName);
	}
	
	static protected string RemapVariantName(string assetBundleName)
	{
#if USE_VARIANT
        string[] bundlesWithVariant = _AssetBundleManifest.GetAllAssetBundlesWithVariant();

		// If the asset bundle doesn't have variant, simply return.
		if (System.Array.IndexOf(bundlesWithVariant, assetBundleName) < 0 )
			return assetBundleName;

		string[] split = assetBundleName.Split('.');

		int bestFit = int.MaxValue;
		int bestFitIndex = -1;
		// Loop all the assetBundles with variant to find the best fit variant bundle.
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
#else
        return assetBundleName;
#endif
    }

	static protected bool LoadAssetBundleInternal (string assetBundleName, bool isLoadingAssetBundleManifest, bool isBundleAlwaysCached)
	{
	    if (assetBundleName.Length == 0)
	    {
	        Debug.LogWarning("can not load bundle with empty name");
	        return true;
	    }
		// Already loaded.
		LoadedAssetBundle bundle = null;
		if(_LoadedAssetBundles.TryGetValue(assetBundleName, out bundle) && bundle != null)
		{
			bundle.RefCount++;
		    bundle.DoNotRelease = isBundleAlwaysCached;
            return true;
		}

        // AssetLoaderRefs
        AssetLoaderRef assetLoaderRef = null;
	    if (_ABLoadRequests.TryGetValue(assetBundleName, out assetLoaderRef) && assetLoaderRef != null)
	    {
            assetLoaderRef.RefCount++;
            if (assetLoaderRef.DoNotReleaseBundle != isBundleAlwaysCached)
	        {
                assetLoaderRef.DoNotReleaseBundle = true;
                HobaDebuger.LogWarningFormat("The Bundle {0} has diffirent release-settings", assetBundleName);
	        }
            return true;
        }
	    else
	    {
            //判断更新目录下的assetbundle是否存在
            string url;
            if (IsUpdateFileExist(assetBundleName))
            {
                url = HobaText.Format("{0}{1}", _UpdateAssetBundleURL, assetBundleName);
            }
            else
            {
                url = HobaText.Format("{0}{1}", _BaseAssetBundleURL, assetBundleName);
            }

            AssetLoaderRef r = new AssetLoaderRef(url, 1, isBundleAlwaysCached);
            _ABLoadRequests.Add(assetBundleName, r);
            return false;
        }
    }

	static protected void LoadDependencies(string assetBundleName)
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

        if(!_Dependencies.ContainsKey(assetBundleName))
            _Dependencies.Add(assetBundleName, dependencies);

		for (int i=0;i<dependencies.Length;i++)
			LoadAssetBundleInternal(dependencies[i], false, true);
	}


    private IEnumerator TaskCoroutine()
    {
        while (true)
        {
            yield return null;

            Tick();
        }
    }

    private void Tick()
    {
        if (_ABLoadRequests.Count > 0)
        {
            _KeysToRemove.Clear();

            var loadEnumerator = _ABLoadRequests.GetEnumerator();
            while (loadEnumerator.MoveNext())
            {
                var keyValue = loadEnumerator.Current;
                
                var downloadRequest = keyValue.Value.DownloadRequest;
                string key = keyValue.Key;

                if (downloadRequest.isDone)
                {
                    LoadedAssetBundle lab = new LoadedAssetBundle(downloadRequest.assetBundle, key);
                    lab.RefCount = keyValue.Value.RefCount;
                    lab.DoNotRelease = keyValue.Value.DoNotReleaseBundle;
                    _LoadedAssetBundles.Add(key, lab);
                    _KeysToRemove.Add(key);

                    if(downloadRequest.assetBundle == null)
                    {
                        var errMsg = HobaText.Format("failed to load AssetBundle {0}", key);
                        if (!_DownloadingErrors.ContainsKey(key))
                            _DownloadingErrors.Add(key, errMsg);
                        HobaDebuger.LogWarning(errMsg);
                    }
                }
            }
            loadEnumerator.Dispose();

            for (int i = 0; i < _KeysToRemove.Count; i++)
            {
                string key = _KeysToRemove[i];
                _ABLoadRequests.Remove(key);
            }
        }

        // 分帧处理，每帧处理1个Operation
        for (int i = 0; i < _InProgressOperations.Count; i++)
        {
            if (!_InProgressOperations[i].Update())
            {
                _InProgressOperations.RemoveAt(i);
                break;
            }
        }

        // 清理缓存Asset，5 min不用，清理掉
        var itor = _LoadedAssetsCache.GetEnumerator();
        while(itor.MoveNext())
        {
            if(Time.time - itor.Current.Value.BornTime > 300f)
            {
                _LoadedAssetsCache.Remove(itor.Current.Key);
                break;
            }
        }
    }

    private static bool ShouldKeepItInCache(LoadedAssetBundle ab)
    {
        if (ab != null && ab.DoNotRelease)
            return true;
        if (_DoNotReleaseBundleSet.Contains(ab.BundleName))
            return true;

        return false;
    }

    private void LoadPathIDData()
    {
        if (Instance == null)
            throw new Exception("Failed to Initialize");

        byte[] pathidContent = null;
        if (IsUpdateFileExist("PATHID.dat"))         //使用更新目录下的pathid.dat
        {
            var filePath = HobaText.Format("{0}{1}", _UpdateAssetBundleURL, "PATHID.dat");
            pathidContent = Util.ReadFile(filePath);

            HobaDebuger.LogFormat("Use PATHID.dat from Update Dir: {0}", _UpdateAssetBundleURL);
        }
        else
        {
            var filePath = HobaText.Format("{0}{1}", _GameResBasePath, "PATHID.dat");
            pathidContent = Util.ReadFile(filePath);

            HobaDebuger.LogFormat("Use PATHID.dat from ResBase Dir: {0}", _GameResBasePath);
        }

        if (pathidContent != null && pathidContent.Length > 0)
        {
            StreamReader sr = new StreamReader(new MemoryStream(pathidContent));

            var strLine = sr.ReadLine();
            while (strLine != null)
            {
                string[] asset_temp = strLine.Split(',');
                if (asset_temp.Length >= 2 && !EntryPoint.Instance.IsSyncLoadBundle(asset_temp[0]))
                {
                    if (!_AssetPath2BundleMap.ContainsKey(asset_temp[1]))
                    {
                        _AssetPath2BundleMap.Add(asset_temp[1], asset_temp[0]);
                    }
                }

                strLine = sr.ReadLine();
            };

            sr.Close();
        }
    }

    private static void AsyncLoadResourceInternal(string assetName, string bundleName, Action<UnityObject> onLoadFinish, bool doNotRelease)
    {
        if(bundleName == null || bundleName == "")
        {
            HobaDebuger.LogWarningFormat("Can not load bundle with empty name, {0} - {1}", assetName, bundleName);
            return;
        }

        if (assetName == null)
            assetName = "";

        var cb = onLoadFinish;
#if DEBUG_DELAY
        if (ResLoadDelay > 0)
        {
            cb = (asset) =>
            {
                Instance.StartCoroutine(DelayCallOnLoadFinish(ResLoadDelay, onLoadFinish, asset));
            };
        }
#endif
        assetName = assetName.ToLower();
        bundleName = bundleName.ToLower();
        LoadResourceInternalImp(assetName, bundleName, cb, doNotRelease);
    }

    private static void LoadResourceInternalImp(string assetName, string bundleName, Action<UnityObject> onLoadFinish, bool doNotRelease)
    {
        LoadAssetBundle(bundleName, false, doNotRelease);

        AssetBundleLoadOperation operation = null;
        if (assetName == null || assetName == "")
            operation = new AssetBundleLoadBundleOperation(bundleName, onLoadFinish);
        else
            operation = new AssetBundleLoadAssetOperation(assetName, bundleName, onLoadFinish);

        _InProgressOperations.Add(operation);
    }

    public static bool IsUpdateFileExist(string filename)
    {
        var fullPath = HobaText.Format("{0}/{1}", _UpdateAssetBundleURL, filename);
        return File.Exists(fullPath);
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
}