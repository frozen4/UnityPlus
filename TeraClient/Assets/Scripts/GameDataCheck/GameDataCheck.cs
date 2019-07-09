using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using LuaInterface;
using System.IO;
using Common;
using System.Text;
using UnityEngine.UI;

public partial class  GameDataCheck : MonoBehaviour
{
    static GameDataCheck _Instance;

    public static GameDataCheck Instance
	{
		get
		{
			if (_Instance == null)
                _Instance = GameObject.FindObjectOfType(typeof(GameDataCheck)) as GameDataCheck;
			if (_Instance == null)
                _Instance = new GameObject("GameDataChecker").AddComponent<GameDataCheck>();
			return _Instance;
		}
	}

    public GameObject _PanelMessageBox = null;
    public GameObject _PanelHotUpdate = null;
    private GameObject _PanelUpdating = null;
    private GameObject _PanelFPS = null;

    bool _bPreInitCompleted = false;
    private bool _IsInited = false;

    public bool _bCheckUIPrefab = false;
    public bool _bCheckData = false;
    public bool _bCheckQuest = false;

    public bool _bCheckSyncLoad = true;
    public bool _bCheckMissing = true;
    public bool _bCheckAnimator = true;
    public bool _bCheckSceneResource = true;
    public bool _bCheckCharacterResource = true;
    public bool _bCheckSfxResource = true;
    public bool _bCheckStaticMesh = true;
    public bool _bCheckShader = true;

    public float _fObjDestroyDelay = 0.0f;

    public void DestroyObjectX(UnityEngine.Object obj)
    {
        if (_fObjDestroyDelay == 0)
            GameObject.Destroy(obj);
        else
            GameObject.Destroy(obj, _fObjDestroyDelay);
    }

    void Awake()
    {
        //AndroidLogger.Instance.WriteToSDCard("EntryPoint Awake Begin...");

        DontDestroyOnLoad(gameObject);

        //strLockPath = Application.persistentDataPath + "/VersionCode.lock";

        //AndroidLogger.Instance.WriteToSDCard("EntryPoint Awake End...");
    }

    void Start()
    {
        //AndroidLogger.Instance.WriteToSDCard("EntryPoint Start Begin...");


        GameObject ui_root = GameObject.Find("UIRootCanvas");
        GFXConfig.CreateGFXConfig();

        // 隐藏FPS
        var transFPS = ui_root.transform.Find("Panel_FPS");
        if (transFPS != null)
            _PanelFPS = transFPS.gameObject;
        if (_PanelFPS)
            _PanelFPS.SetActive(false);

        //弹窗界面
        {
            UnityEngine.Object panelMessageBox = Resources.Load("UI/Panel_MessageBox", typeof(GameObject));
            if (panelMessageBox != null)
            {
                _PanelMessageBox = Instantiate(panelMessageBox) as GameObject;
                _PanelMessageBox.transform.SetParent(Main.PanelRoot);
                RectTransform rcTransform = _PanelMessageBox.GetComponent<RectTransform>();
                rcTransform.offsetMin = Vector2.zero;
                rcTransform.offsetMax = Vector2.zero;
                rcTransform.anchoredPosition3D = Vector3.zero;
                _PanelMessageBox.transform.localScale = Vector3.one;
                _PanelMessageBox.FindChild("Frame_Message").SetActive(false);
            }
        }

        //更新界面   _PanelUpdate 在 UpdateRoutine() 中控制显隐
        {
            UnityEngine.Object panelHotUpdate = Resources.Load("UI/Panel_HotUpdate", typeof(GameObject));
            if (panelHotUpdate != null)
            {
                _PanelHotUpdate = Instantiate(panelHotUpdate) as GameObject;
                _PanelHotUpdate.transform.SetParent(ui_root.transform);
                RectTransform rcTransform = _PanelHotUpdate.GetComponent<RectTransform>();
                rcTransform.offsetMin = Vector2.zero;
                rcTransform.offsetMax = Vector2.zero;
                rcTransform.anchoredPosition3D = Vector3.zero;
                _PanelHotUpdate.transform.localScale = Vector3.one;
                {
                    // TODO:加载更新图上的Tip和背景图更换
                }
                _PanelHotUpdate.SetActive(false);
            }
            else
            {
                //AndroidLogger.Instance.WriteToSDCard("Failed to Load Panel_HotUpdate!");
            }
        }


        StartCoroutine(PreInitGameCoroutine().GetEnumerator());

        StartCoroutine(InitGameCoroutine().GetEnumerator());

        //AndroidLogger.Instance.WriteToSDCard("EntryPoint Start End...");
    }

    IEnumerable PreInitGameCoroutine()
    {
        //AndroidLogger.Instance.WriteToSDCard("PreInitGameCoroutine Start ...");

        if (_PanelUpdating != null)
        {
            _PanelUpdating.SetActive(true);
            yield return null;
        }

        //AndroidLogger.Instance.WriteToSDCard("PreInitGameCoroutine Start End...");

        _bPreInitCompleted = true;
    }

    IEnumerable InitGameCoroutine()
    {
        while (!_bPreInitCompleted)
            yield return null;

        //AndroidLogger.Instance.WriteToSDCard("EntryPoint InitGameCoroutine Start...");

        if (_PanelFPS)
        {
            _PanelFPS.SetActive(true);
            yield return null;
        }

        foreach (var item in GameDataCheckMan.Instance.UpdateRoutine())
        {
            yield return item;
        }

        while (!GameDataCheckMan.Instance.bIsUpdateSucceed)
            yield return null;

        _IsInited = true;
    }

    void Update()
    {
        if (!_IsInited) return;
    }

    void OnApplicationQuit()
    {

    }
}
