using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Common;

public partial class EntryPoint : MonoBehaviour
{
    static EntryPoint _Instance;

    public static EntryPoint Instance
    {
        get
        {
            if (_Instance == null)
                _Instance = GameObject.FindObjectOfType(typeof(EntryPoint)) as EntryPoint;
            if (_Instance == null)
                _Instance = new GameObject("EntryPoint").AddComponent<EntryPoint>();
            return _Instance;
        }
    }

    public LogLevel _WriteLogLevel;
    public string _ResPath = string.Empty;
    private string _AssetBundlePath = string.Empty;

    private bool _IsInited = false;

    public bool IsSyncLoadBundle(string bundleName)
    {
        return false;
    }

	void Awake()
	{
		DontDestroyOnLoad(gameObject);
	}
	
	void Start()
    {
        SetupPath();

        HobaDebuger.GameLogLevel = _WriteLogLevel;

        StartCoroutine(InitGameInternal().GetEnumerator());
	}

    void SetupPath()
    {
#if UNITY_IPHONE
        _ResPath = Application.streamingAssetsPath + "/res_base";
        _AssetBundlePath = _ResPath;
#elif (UNITY_EDITOR_WIN || UNITY_EDITOR_OSX) && !UNITY_ANDROID
        if (string.IsNullOrEmpty(_ResPath))
        {
            _ResPath = Application.streamingAssetsPath;
        }
        else if (_ResPath.StartsWith("./") || _ResPath.StartsWith("../"))
        {
            _ResPath = Application.dataPath + "/" + _ResPath;
        }
        _AssetBundlePath = _ResPath;
#elif UNITY_ANDROID
        _AssetBundlePath = Application.dataPath + "!assets/res_base";           //android直接从手机内存中读取代替 www
        _ResPath = Application.persistentDataPath + "/res_base";
#else
		if (string.IsNullOrEmpty(_ResPath))
		{
			_ResPath = Application.streamingAssetsPath;
		}

		if (Application.platform == RuntimePlatform.WindowsPlayer)
        {
            _ResPath = Application.dataPath + "/../GameRes";
        }
        _AssetBundlePath = _ResPath;
#endif
    }

    IEnumerable InitGameInternal()
    {
        CAssetBundleManager.Instance.Init(_ResPath, _AssetBundlePath);
        while (!CAssetBundleManager.IsOK)
            yield return null;

        var bundleName = "shader";
        var shaderListPath = "Assets/Outputs/Shader/ShaderList.prefab";
        ShaderManager.Instance.Init(bundleName, shaderListPath);

        _IsInited = true;
        yield return null;
    }

    void Update()
    {
        if (!_IsInited) return;
    }
}