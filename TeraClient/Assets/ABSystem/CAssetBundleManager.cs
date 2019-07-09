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
    public string BundleName { get; private set; }
    public AssetBundle Bundle { get; private set; }

    public LoadedAssetBundle(AssetBundle bundle, string name)
    {
        BundleName = name;
        Bundle = bundle;
    }
}

public struct LoadedAsset
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

    public AssetLoaderRef(string url, int refcount)
    {
        url = url.Replace("file://", "");
        DownloadRequest = AssetBundle.LoadFromFileAsync(url);
        RefCount = refcount;
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
    static Dictionary<string, LoadedAssetBundle> _LoadedAssetBundles = new Dictionary<string, LoadedAssetBundle>();
    /// 完成加载，在管理器缓存的Asset列表 (仅缓存ICON资源，其余类型都有自己的缓存机制)
    static Dictionary<string, LoadedAsset> _LoadedAssetsCache = new Dictionary<string, LoadedAsset>();
    /// 正在处理中的AB加载请求列表
    static Dictionary<string, AssetLoaderRef> _ABLoadRequests = new Dictionary<string, AssetLoaderRef>();
    /// 加载出错资源信息列表
    static Dictionary<string, string> _DownloadingErrors = new Dictionary<string, string>();

    List<string> _KeysToRemove = new List<string>();

#if DEBUG_DELAY
    // For Debug
    public static float ResLoadDelay = 0f;
#endif

    public IEnumerable Init(string gameResPath, string assetBundlePath)
    {
        StartCoroutine(TaskCoroutine());
        yield return null;

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

        //HobaDebuger.LogWarningFormat("BaseAssetBundleURL: {0}", _BaseAssetBundleURL);
        //HobaDebuger.LogWarningFormat("UpdateAssetBundleURL: {0}", _UpdateAssetBundleURL);
        yield return null;

        foreach (var v in LoadPathIDDataCoroutine())
            yield return v;

        yield return null;

        //base manifest
        {
            string url;
            string assetBundleName = platformFolderForAssetBundles;
            if (IsUpdateFileExist(assetBundleName))
            {
                url = HobaText.Format("{0}{1}", _UpdateAssetBundleURL, assetBundleName);
            }
            else
            {
                url = HobaText.Format("{0}{1}", _BaseAssetBundleURL, assetBundleName);
            }

            var manifestAssetBundle = SyncLoadAssetBundle(assetBundleName);
            if (manifestAssetBundle != null)
            {
                var manifest = manifestAssetBundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
                if (manifest != null)
                {
                    var abs = manifest.GetAllAssetBundles();
                    foreach (var ab in abs)
                    {
                        var dependencies = manifest.GetAllDependencies(ab);
                        if (dependencies.Length > 0 && !_Dependencies.ContainsKey(ab))
                            _Dependencies.Add(ab, dependencies);
                    }
                }
                else
                {
                    HobaDebuger.LogErrorFormat("Failed to load AssetBundleManifest file!!! {0}", url);
                }

                UnloadBundle(assetBundleName);
            }
            else
            {
                HobaDebuger.LogErrorFormat("Failed to load Base AssetBundleManifest Bundle!!! {0}", url);
            }
        }
        yield return null;

        //update manifest
        {
            string assetBundleName = platformFolderForAssetBundles + "Update";
            var manifestAssetBundle = SyncLoadAssetBundleInUpdate(assetBundleName);
            if (manifestAssetBundle != null)
            {
                var manifest = manifestAssetBundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
                if (manifest != null)
                {
                    var abs = manifest.GetAllAssetBundles();
                    foreach (var ab in abs)
                    {
                        var dependencies = manifest.GetAllDependencies(ab);
                        if (dependencies.Length > 0 && !_Dependencies.ContainsKey(ab))
                            _Dependencies.Add(ab, dependencies);
                    }
                }
                else
                {
                    HobaDebuger.LogErrorFormat("Failed to load Update AssetBundleManifest file!!! {0}", assetBundleName);
                }

                UnloadBundle(assetBundleName);
            }
            else
            {
                HobaDebuger.LogWarningFormat("Failed to load Update AssetBundleManifest Bundle!!! {0}", assetBundleName);
            }
        }
        yield return null;        
    }

    private IEnumerable LoadPathIDDataCoroutine()
    {
        var filePath = HobaText.Format("{0}{1}", IsUpdateFileExist("PATHID.dat") ? _UpdateAssetBundleURL : _GameResBasePath, "PATHID.dat");
        var pathidContent = Util.ReadFile(filePath);
        yield return null;

        if (pathidContent != null && pathidContent.Length > 0)
        {
            using (StreamReader sr = new StreamReader(new MemoryStream(pathidContent)))
            {
                var strLine = sr.ReadLine();
                var processedCount = 0;
                while (strLine != null)
                {
                    string[] assetTemp = strLine.Split(',');
                    if (assetTemp.Length >= 2 && !_AssetPath2BundleMap.ContainsKey(assetTemp[1]))
                        _AssetPath2BundleMap.Add(assetTemp[1], assetTemp[0]);

                    // 每帧对多处理250条
                    processedCount++;
                    if (processedCount % 250 == 0)
                        yield return null;

                    strLine = sr.ReadLine();
                };

                sr.Close();
            }
        }
    }

    public static bool IsAssetBundleLoading()
    {
        return _InProgressOperations.Count > 0;
    }

    public static int GetAssetBundleLoadingCount()
    {
        return _InProgressOperations.Count;
    }

    public static AssetBundle SyncLoadAssetBundle(string bundleName)
    {
        bool isAlReadyProcessed = SyncLoadAssetBundleInternal(bundleName);
        if (!isAlReadyProcessed)
        {
            string[] dependencies;
            if (_Dependencies.TryGetValue(bundleName, out dependencies))
            {
                foreach (var v in dependencies)
                    SyncLoadAssetBundleInternal(v);
            }
        }

        LoadedAssetBundle bundle = null;
        if (_LoadedAssetBundles.TryGetValue(bundleName, out bundle))
            return bundle.Bundle;

        return null;
    }

    private static bool SyncLoadAssetBundleInternal(string assetBundleName)
    {
        if (assetBundleName.Length == 0)
        {
            Debug.LogWarning("can not load bundle with empty name");
            return true;
        }
        // Already loaded.
        LoadedAssetBundle bundle = null;
        if (_LoadedAssetBundles.TryGetValue(assetBundleName, out bundle) && bundle != null)
        {
            return true;
        }

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

        string key = assetBundleName;
        var loadedAssetBundle = AssetBundle.LoadFromFile(url);
        if (loadedAssetBundle == null)
        {
            var errMsg = HobaText.Format("failed to load AssetBundle {0}", key);
            if (!_DownloadingErrors.ContainsKey(key))
                _DownloadingErrors.Add(key, errMsg);
            HobaDebuger.LogWarning(errMsg);
            return false;
        }

        var lab = new LoadedAssetBundle(loadedAssetBundle, key);
        _LoadedAssetBundles.Add(key, lab);
        return false;
    }

    private static AssetBundle SyncLoadAssetBundleInUpdate(string assetBundleName)
    {
        if (assetBundleName.Length == 0)
        {
            Debug.LogWarning("can not load bundle with empty name");
            return null;
        }
        // Already loaded.
        LoadedAssetBundle bundle = null;
        if (_LoadedAssetBundles.TryGetValue(assetBundleName, out bundle) && bundle != null)
        {
            return bundle.Bundle;
        }

        //判断更新目录下的assetbundle是否存在
        string url;
        if (IsUpdateFileExist(assetBundleName))
            url = HobaText.Format("{0}{1}", _UpdateAssetBundleURL, assetBundleName);
        else
            return null;

        string key = assetBundleName;
        var loadedAssetBundle = AssetBundle.LoadFromFile(url);
        if (loadedAssetBundle == null)
        {
            var errMsg = HobaText.Format("failed to load AssetBundle {0}", key);
            if (!_DownloadingErrors.ContainsKey(key))
                _DownloadingErrors.Add(key, errMsg);
            HobaDebuger.LogWarning(errMsg);
            return null;
        }

        var lab = new LoadedAssetBundle(loadedAssetBundle, key);
        _LoadedAssetBundles.Add(key, lab);
        return loadedAssetBundle;
    }

    public static void AsyncLoadBundle(string bundleName, Action<UnityObject> onLoadFinish)
    {
        Hoba.Action<UnityObject> callback = null;
        if (onLoadFinish != null)
            callback = delegate (UnityObject o)
            {
                onLoadFinish(o);
                return true;
            };
        AsyncLoadResourceInternal("", bundleName, callback, false);
    }

    public static void UnloadBundle(string bundleName)
    {
        LoadedAssetBundle lab;
        if (_LoadedAssetBundles.TryGetValue(bundleName, out lab))
        {
            lab.Bundle.Unload(false);
            _LoadedAssetBundles.Remove(bundleName);
            HobaDebuger.LogFormat("Unload unuesd bundles: {0}", bundleName);
        }
    }

    public void UnloadBundleOfAsset(string assetName)
    {
        string bundleName;
        if (string.IsNullOrEmpty(assetName) || !_AssetPath2BundleMap.TryGetValue(assetName, out bundleName))
            return;

        UnloadBundle(bundleName);
    }

    public static void AsyncLoadResource(string assetName, Action<UnityObject> onLoadFinish, bool needInstantiate, string bundleName = null)
    {
        if (string.IsNullOrEmpty(bundleName))
            bundleName = Instance.GetBundleName(assetName);

        if (string.IsNullOrEmpty(bundleName))
        {
            onLoadFinish(null);
            return;
        }

        Hoba.Action<UnityObject> callback = null;
        if(onLoadFinish != null)
            callback = delegate(UnityObject o) {
                onLoadFinish(o);
                return true;
            };
        AsyncLoadResourceInternal(assetName, bundleName, callback, needInstantiate);
    }

    public static void AsyncLoadResource(string assetName, Hoba.Action<UnityObject> onLoadFinish, bool needInstantiate, string bundleName = null)
    {
        if (string.IsNullOrEmpty(bundleName))
            bundleName = Instance.GetBundleName(assetName);

        if (string.IsNullOrEmpty(bundleName))
        {
            onLoadFinish(null);
            return;
        }

        AsyncLoadResourceInternal(assetName, bundleName, onLoadFinish, needInstantiate);
    }

    public static T SyncLoadAssetFromBundle<T>(string assetName, string bundleName = null, bool addToCache = false) where T : UnityObject
    {
        if (string.IsNullOrEmpty(assetName))
            return null;

        if (addToCache)
        {
            LoadedAsset cache;
            if (_LoadedAssetsCache.TryGetValue(assetName, out cache))
            {
                cache.BornTime = Time.time;
                T result = cache.Asset as T;
                return result;
            }
        }

        if (string.IsNullOrEmpty(bundleName))
            bundleName = Instance.GetBundleName(assetName);

        if (string.IsNullOrEmpty(bundleName))
        {
            HobaDebuger.LogWarningFormat("{0} cant be load,because it cant find bundle name", assetName);
            return null;
        }

        var error = String.Empty;
        var bundle = GetLoadedAssetBundle(bundleName, out error);
        if (null == bundle)
        {
            var url = IsUpdateFileExist(bundleName) ? Path.Combine(_UpdateAssetBundleURL, bundleName) : Path.Combine(_BaseAssetBundleURL, bundleName);
            var ab = AssetBundle.LoadFromFile(url);
            if (ab == null)
            {
                HobaDebuger.LogWarningFormat("AssetBundle {0} has not been loaded first", bundleName);
                return null;
            }

            bundle = new LoadedAssetBundle(ab, bundleName);
            _LoadedAssetBundles.Add(bundleName, bundle);
        }

        var asset = bundle.Bundle.LoadAsset<T>(assetName);

        if (addToCache && asset != null)
        {
            if (!_LoadedAssetsCache.ContainsKey(assetName))
                _LoadedAssetsCache.Add(assetName, new LoadedAsset(asset));
        }

        return asset;
    }

    public static LoadedAssetBundle GetLoadedAssetBundle(string assetBundleName, out string error)
    {
        if (_DownloadingErrors.TryGetValue(assetBundleName, out error))
            return null;

        LoadedAssetBundle bundle = null;
        _LoadedAssetBundles.TryGetValue(assetBundleName, out bundle);
        if (bundle == null)
            return null;

        // No dependencies are recorded, only the bundle itself is required.
        string[] dependencies = null;
        if (!_Dependencies.TryGetValue(assetBundleName, out dependencies))
            return bundle;

        if (_DownloadingErrors.TryGetValue(assetBundleName, out error))
            return bundle;

        // Make sure all dependencies are loaded
        foreach (var dependency in dependencies)
        {
            // Wait all the dependent assetBundles being loaded.
            if (!_LoadedAssetBundles.ContainsKey(dependency))
                return null;
        }

        return bundle;
    }

    private string GetBundleName(string assetName)
    {
        if (string.IsNullOrEmpty(assetName)) return null;

        string bundleName = string.Empty;
        if (_AssetPath2BundleMap.TryGetValue(assetName, out bundleName))
            return bundleName;

#if UNITY_EDITOR || UNITY_STANDALONE
        HobaDebuger.LogErrorFormat("Failed to find bundle when load asset {0}", assetName);
#endif
        return null;
    }

    private static IEnumerator DelayCallOnLoadFinish(float delay, Action<UnityEngine.Object> callback, UnityEngine.Object asset)
    {
        yield return new WaitForSeconds(delay);
        callback(asset);
    }

    protected static void LoadAssetBundle(string assetBundleName)
    {
        // Check if the bundle has already been processed.
        bool isAlreadyProcessed = LoadAssetBundleInternal(assetBundleName);

        // Load dependencies.
        if (!isAlreadyProcessed)
        {
            string[] dependencies;
            if (_Dependencies.TryGetValue(assetBundleName, out dependencies))
            {
                foreach (var v in dependencies)
                    LoadAssetBundleInternal(v);
            }
        }
    }

    protected static bool LoadAssetBundleInternal(string assetBundleName)
    {
        if (assetBundleName.Length == 0)
        {
            Debug.LogWarning("can not load bundle with empty name");
            return true;
        }
        // Already loaded.
        LoadedAssetBundle bundle = null;
        if (_LoadedAssetBundles.TryGetValue(assetBundleName, out bundle) && bundle != null)
        {
            return true;
        }

        // AssetLoaderRefs
        AssetLoaderRef assetLoaderRef = null;
        if (_ABLoadRequests.TryGetValue(assetBundleName, out assetLoaderRef) && assetLoaderRef != null)
        {
            assetLoaderRef.RefCount++;
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

            AssetLoaderRef r = new AssetLoaderRef(url, 1);
            _ABLoadRequests.Add(assetBundleName, r);
            return false;
        }
    }

    private IEnumerator TaskCoroutine()
    {
        while (true)
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
                        var lab = new LoadedAssetBundle(downloadRequest.assetBundle, key);
                        _LoadedAssetBundles.Add(key, lab);
                        _KeysToRemove.Add(key);

                        if (downloadRequest.assetBundle == null)
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

            // 每次处理1个Operation
            AssetBundleLoadOperation loadedOne = null;
            foreach (var v in _InProgressOperations)
            {
                if (!v.Update())
                {
                    loadedOne = v;
                    break;
                }
            }

            if (loadedOne != null)
            {
                var obj = loadedOne.GetObject();
                if (obj != null)
                {
                    if (loadedOne.Type == LoadType.Asset && loadedOne.InstantiateWhenLoaded)
                    {
                        var prefab = obj as GameObject;
                        if (prefab != null)
                        {
                            var go = UnityEngine.Object.Instantiate(prefab);
                            yield return null;

                            if (loadedOne.Callback != null)
                                loadedOne.Callback(go);
                        }
                    }
                    else
                    {
                        if (loadedOne.Callback != null)
                            loadedOne.Callback(obj);
                    }
                }

                _InProgressOperations.Remove(loadedOne);
            }

            yield return null;
        }
    }

    public void ClearCaches(bool bClearAll)        //bClealAll清除所有，否则需要保留在lifetime内的asset
    {
        if (bClearAll)
        {
            _LoadedAssetsCache.Clear();
            return;
        }

        _KeysToRemove.Clear();
        float curTime = Time.time;
        foreach (var kv in _LoadedAssetsCache)
        {
            if (curTime - kv.Value.BornTime > 120)
                _KeysToRemove.Add(kv.Key);
        }

        foreach (var v in _KeysToRemove)
            _LoadedAssetsCache.Remove(v);

        _KeysToRemove.Clear();
    }

    private void LoadPathIDData()
    {
        if (Instance == null)
            throw new Exception("Failed to Initialize");

        var filePath = HobaText.Format("{0}{1}", IsUpdateFileExist("PATHID.dat") ? _UpdateAssetBundleURL : _GameResBasePath, "PATHID.dat");
        var pathidContent = Util.ReadFile(filePath);
        
        if (pathidContent != null && pathidContent.Length > 0)
        {
            using (StreamReader sr = new StreamReader(new MemoryStream(pathidContent)))
            {
                var strLine = sr.ReadLine();
                while (strLine != null)
                {
                    string[] assetTemp = strLine.Split(',');
                    if (assetTemp.Length >= 2 && !_AssetPath2BundleMap.ContainsKey(assetTemp[1]))
                        _AssetPath2BundleMap.Add(assetTemp[1], assetTemp[0]);

                    strLine = sr.ReadLine();
                };

                sr.Close();
            }
        }

        var lines = File.ReadAllLines(filePath);
        foreach (var v in lines)
        {
            string[] assetTemp = v.Split(',');
            if (assetTemp.Length >= 2 && !_AssetPath2BundleMap.ContainsKey(assetTemp[1]))
                _AssetPath2BundleMap.Add(assetTemp[1], assetTemp[0]);
        }
    }

    private static void AsyncLoadResourceInternal(string assetName, string bundleName, Hoba.Action<UnityObject> onLoadFinish, bool needInstantiate)
    {
        if (string.IsNullOrEmpty(bundleName))
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
        //assetName = assetName.ToLower();
        bundleName = bundleName.ToLower();
        LoadResourceInternalImp(assetName, bundleName, cb, needInstantiate);
    }

    private static void LoadResourceInternalImp(string assetName, string bundleName, Hoba.Action<UnityObject> onLoadFinish, bool needInstantiate)
    {
        LoadAssetBundle(bundleName);

        AssetBundleLoadOperation operation = null;
        if (string.IsNullOrEmpty(assetName))
            operation = new AssetBundleLoadBundleOperation(bundleName, onLoadFinish);
        else
            operation = new AssetBundleLoadAssetOperation(assetName, bundleName, onLoadFinish, needInstantiate);

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