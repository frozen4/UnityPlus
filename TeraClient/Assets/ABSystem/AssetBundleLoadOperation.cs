using System;
using UnityEngine;
using System.Collections;
using Common;
using Object = UnityEngine.Object;

public abstract class AssetBundleLoadOperation : IEnumerator
{
    protected string _BundleName;
    protected string _AssetName;
    protected string _DownloadingError;
    protected Action<Object> _Callback = null;

    protected AssetBundleLoadOperation(string bundleName, string assetName, Action<Object> cb)
    {
        _BundleName = bundleName;
        _AssetName = assetName;
        _Callback = cb;
    }

    public object Current
	{
		get{ return null; }
	}

	public bool MoveNext()
	{
		return !IsDone();
	}

    public void Reset() {}

    abstract public bool Update ();
	
	abstract public bool IsDone ();
}

public class AssetBundleLoadManifestOperation : AssetBundleLoadOperation
{
    protected Object _Asset = null;

    public AssetBundleLoadManifestOperation(string bundleName, string assetName, Action<Object> cb)
        : base(bundleName, assetName, cb)
	{
    }

    public override bool Update()
	{
        LoadedAssetBundle bundle = CAssetBundleManager.GetLoadedAssetBundle(_BundleName, out _DownloadingError);
        if (bundle != null)
        {
            bundle.DoNotRelease = true;
            _Asset = bundle.Bundle.LoadAsset(_AssetName);
            if (_Callback != null)
                _Callback(_Asset);

            return false;
        }

        return true;
	}

    public override bool IsDone()
    {
#if true
        return _Asset != null;
#else
        if (_Request == null && _DownloadingError != null)
        {
            HobaDebuger.LogError(_DownloadingError);
            return true;
        }

        return _Request != null && _Request.isDone;
#endif
    }
}

public class AssetBundleLoadBundleOperation : AssetBundleLoadOperation
{
    protected AssetBundle _Bundle = null;

    public AssetBundleLoadBundleOperation(string bundleName, Action<Object> cb)
        : base(bundleName, null, cb)
    {
    }

    public override bool Update()
    {
        if (_Bundle != null)
            return false;

        LoadedAssetBundle bundle = CAssetBundleManager.GetLoadedAssetBundle(_BundleName, out _DownloadingError);
        if (bundle != null)
        {
            _Bundle = bundle.Bundle;
            if (_Callback != null)
                _Callback(_Bundle);
            return false;
        }
        else
        {
            return true;
        }
    }

    public override bool IsDone()
    {
        if (_Bundle == null && _DownloadingError != null)
        {
            HobaDebuger.LogError(_DownloadingError);
            return true;
        }

        return _Bundle != null;
    }
}

public class AssetBundleLoadAssetOperation : AssetBundleLoadOperation
{
    protected AssetBundleRequest _Request = null;
    public AssetBundleLoadAssetOperation(string assetName, string bundleName, Action<Object> cb)
        : base(bundleName, assetName, cb)
    {
    }

    public override bool Update()
    {
        LoadedAssetBundle bundle = CAssetBundleManager.GetLoadedAssetBundle(_BundleName, out _DownloadingError);
        if (_DownloadingError != null)
        {
            if (_Callback != null)
                _Callback(null);

            return false;
        }

        if (bundle != null && _Request == null)
            _Request = bundle.Bundle.LoadAssetAsync(_AssetName);

        if (_Request != null && _Request.isDone)
        {
            // 有可能在asset异步请求中，bundle被释放掉
            if(bundle != null)
            {
                UnityEngine.Object asset = _Request.asset;
                if (_Callback != null)
                {
                    if (bundle.DoNotRelease)
                    {
                        var cache = new LoadedAsset(asset);
                        CAssetBundleManager.AddAssetCache(_AssetName, cache);
                    }

                    _Callback(asset);
                }
            }

            return false;
        }

        return true;
    }

    public override bool IsDone()
    {
        if (_DownloadingError != null)
        {
            HobaDebuger.LogError(_DownloadingError);
            return true;
        }

        return _Request != null && _Request.isDone;
    }
}
