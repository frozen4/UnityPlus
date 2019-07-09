//#define ONGUI_DEBUG
using UnityEngine;
using System;
using LuaInterface;
using Common;
using UnityEngine.UI;
using SeasideResearch.LibCurlNet;
using EntityComponent;
using System.Collections;

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
    private string _DocPath = string.Empty;
    private string _LibPath = string.Empty;        //更新使用
    private string _TmpPath = string.Empty;       //更新使用
    private string _ConfigPath = string.Empty;     // ResPath下的配置目录，可热更新
    private string _LuaPath = string.Empty;
    private string _VoiceDir = string.Empty;      //暂存的语音聊天文件
    private string _CustomPicDir = string.Empty;   //暂存的自定义头像文件
    private string _UserLanguageFile = string.Empty;
    private string _UserBillingFile = string.Empty;
    private string _UserDataDir = string.Empty;

    public bool _IsToWriteLogFile = false;
    public bool _UseWwiseStreamingAssets = false;
    public bool _UseWwiseEngineLog = false;

    private bool _BeForArtTest = false;

    public static double _ServerTimeGap = 0;

    private int _DesignWidth = 0;           //分辨率缩放, 为0时不缩放 
    private int _DesignHeight = 0;

    internal readonly CTimerList _TimerList = new CTimerList();
    internal readonly CTimerList _LateTimerList = new CTimerList();

    private bool _IsInited = false;
    private bool _IsCurlInited = false;

    private bool _IsQuiting = false;
    public bool IsQuiting
    {
        get { return _IsQuiting; }
    }

#if UNITY_ANDROID
    public static float fCopyProgress = 0.0f;
    struct SFileEntry
    {
        public string filename;
        public int filesize;
    }
#endif
    //private bool IsCheckVersionFinished = false;
    //private bool bIsNewVersionPack = true;
    //string strNewVersion = "ErrorVersionCode";   
    //string strLockPath = "";
    private bool _IsInstallFinished = false;

    private GameObject _PanelMessageBox = null;

    public GameObject PanelMessageBox
    {
        get
        {
            if (_PanelMessageBox == null)
            {
                UnityEngine.Object panelMessageBox = Resources.Load("UI/Panel_MessageBox", typeof(GameObject));
                if (panelMessageBox != null)
                {
                    _PanelMessageBox = Instantiate(panelMessageBox) as GameObject;
                    _PanelMessageBox.transform.SetParent(Main.PanelRoot);
                    var rcTransform = _PanelMessageBox.GetComponent<RectTransform>();
                    rcTransform.offsetMin = Vector2.zero;
                    rcTransform.offsetMax = Vector2.zero;
                    rcTransform.anchoredPosition3D = Vector3.zero;
                    _PanelMessageBox.transform.localScale = Vector3.one;
                    _PanelMessageBox.FindChild("Frame_Message").SetActive(false);
                }
            }

            return _PanelMessageBox;
        }
    }

    private GameObject _PanelCirle = null;
    public GameObject PanelCirle
    {
        get
        {
            if (_PanelCirle == null)
            {
                UnityEngine.Object panelCirle = Resources.Load("UI/Panel_Circle", typeof(GameObject));
                if (panelCirle != null)
                {
                    _PanelCirle = Instantiate(panelCirle) as GameObject;
                    _PanelCirle.transform.SetParent(Main.PanelRoot);
                    RectTransform rcTransform = _PanelCirle.GetComponent<RectTransform>();
                    rcTransform.offsetMin = Vector2.zero;
                    rcTransform.offsetMax = Vector2.zero;
                    rcTransform.anchoredPosition3D = Vector3.zero;
                    _PanelCirle.transform.localScale = Vector3.one;
                    _PanelCirle.SetActive(false);
                }
            }
            return _PanelCirle;
        }
    }

    public GameObject PanelHotUpdate = null;

    private GameObject _PanelLogo = null;
    public GameObject PanelLogo
    {
        get
        {
            if (_PanelLogo == null)
            {
                GameObject prefab = Resources.Load("UI/Panel_Logo") as GameObject;
                if (prefab == null)
                {
                    HobaDebuger.LogError("PanelLogo open failed :: can not find Panel_Logo in Resources");
                }
                var logo = GameObject.Instantiate(prefab);
                logo.transform.SetParent(Main.PanelRoot);

                bool isCN = false;
                var img_english = logo.transform.Find("Img_Logo_EN");
                if (img_english != null)
                    img_english.gameObject.SetActive(!isCN);
                var img_chinese = logo.transform.Find("Img_Logo_CN");
                if (img_chinese != null)
                    img_chinese.gameObject.SetActive(isCN);

                RectTransform rcTransform = logo.GetComponent<RectTransform>();
                rcTransform.offsetMin = Vector2.zero;
                rcTransform.offsetMax = Vector2.zero;
                rcTransform.anchoredPosition3D = Vector3.zero;
                logo.transform.localScale = Vector3.one;
                logo.SetActive(false);

                _PanelLogo = logo;
            }
            return _PanelLogo;
        }
        private set { _PanelLogo = value; }
    }
    private GameObject _PanelFPS = null;
    private Image _ImgLogoMask = null;

    public Image ImgLogoMask
    {
        get
        {
            if (_ImgLogoMask == null)
            {
                if (PanelLogo != null)
                {
                    GameObject mask = PanelLogo.FindChild("Img_Mask");
                    if (mask != null)
                        _ImgLogoMask = mask.GetComponent<Image>();
                }
            }

            return _ImgLogoMask;
        }
    }

    private GameObject _PanelSplash = null;
    private GameObject _PanelMonition = null;
    //private Image _ImgSplash = null;
    private ISoundMan _SoundMan = null;
    private IVideoManager _VideoManager = null;

    public bool IsInited
    {
        get { return _IsInited; }
    }

    public bool IsInstallFinished
    {
        get { return _IsInstallFinished; }
        set { _IsInstallFinished = value; }
    }

    public string LuaPath
    {
        get { return _LuaPath; }
    }

    public string ConfigPath
    {
        get { return _ConfigPath; }
    }

    public string ResPath
    {
        get { return _ResPath; }
    }

    public string DocPath
    {
        get { return _DocPath; }
    }

    public string LibPath
    {
        get { return _LibPath; }
    }

    public string TmpPath
    {
        get { return _TmpPath; }
    }

    public string VoiceDir
    {
        get { return _VoiceDir; }
    }

    public string CustomPicDir
    {
        get { return _CustomPicDir; }
    }

    public string UserLanguageFile
    {
        get { return _UserLanguageFile; }
    }

    public string UserBillingFile
    {
        get { return _UserBillingFile; }
    }

    public string UserDataDir
    {
        get { return _UserDataDir; }
    }

    public bool IsToWriteLogFile
    {
        get { return _IsToWriteLogFile; }
    }

    public LogLevel WriteLogLevel
    {
        get { return _WriteLogLevel; }
    }

    public ISoundMan SoundMan
    {
        get { return _SoundMan; }
    }

    public IVideoManager VideoManager
    {
        get { return _VideoManager; }
    }

    public bool UseWwise
    {
        get { return true; }
    }

    protected static void CGCallLuaFunction(String luaFunction)
    {
        LuaScriptMgr.Instance.CallLuaFunction(luaFunction);
    }

    void Awake()
    {
        DeviceLogger.Instance.WriteLogFormat("EntryPoint Awake Begin...");

#if UNITY_IPHONE || UNITY_ANDROID
        Application.targetFrameRate = 30;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
#elif !UNITY_EDITOR && UNITY_STANDALONE_WIN
        Application.targetFrameRate = 60;
#endif

        DontDestroyOnLoad(gameObject);

        OSUtility.SetAudioAmbient();

        Application.lowMemory += OnLowMemory;
        DeviceLogger.Instance.WriteLogFormat("EntryPoint Awake End...");
    }

    void Start()
    {
#if UNITY_IPHONE || UNITY_ANDROID
        _WriteLogLevel = LogLevel.Warning;
#endif

#if AssetsUnloadTest
        //test "Assets/Outputs/Scenes/SelectChar.prefab"
        SetupPath();
        CAssetBundleManager.Instance.Init(_ResPath, _AssetBundlePath);
        return;
#endif
        /*
        {
            UnityEngine.Transform component = this.gameObject.transform;
            System.Reflection.PropertyInfo propertyInfo = CinemaSuite.Common.ReflectionHelper.GetProperty(typeof(UnityEngine.Transform), "localPosition");
            object value = propertyInfo.GetValue(component, null);
            //System.Reflection.MethodInfo mi = propertyInfo.GetGetMethod();
            //object value = mi.Invoke(component, null);
        }
         * */
        /*
        {
            GameObject obj = new GameObject("test1");
            Quaternion q = Quaternion.Euler(new Vector3(271.31f, 158.38f, 34.74f));
            Vector3 dir = q * Vector3.forward;
            
            obj.transform.forward = dir;
        }
         * */

        {
            GameObject go = GameObject.Find("UICamera");
            if (go != null)
                Main.UICamera = go.GetComponent<Camera>();
            go = GameObject.Find("UIRootCanvas");
            if (go != null)
            {
                Main.UIRootCanvas = go.transform;
                Main.PanelRoot = go.transform.Find("PanelRoot");
                Main.PanelHUD = go.transform.Find("Panel_HUD");
            }
            go = GameObject.Find("TopPateCanvas");
            {
                Main.TopPateCanvas = go.transform;
            }
            Main.BlockCanvas = Main.UIRootCanvas.Find("PanelRoot/BlockCanvas");
            Main.EnableBlockCanvas(false);
        }

        DeviceLogger.Instance.WriteLogFormat("EntryPoint Start Begin...");

        GFXConfig.CreateGFXConfig();

        // 隐藏FPS
        var transFPS = Main.UIRootCanvas.Find("Panel_FPS");
        if (transFPS != null)
        {
            _PanelFPS = transFPS.gameObject;
            _PanelFPS.SetActive(false);
        }

        //游戏Logo
        if (PanelLogo != null)
            PanelLogo.SetActive(false);

        //更新界面   _PanelUpdate 在 UpdateRoutine() 中控制显隐
        {
            UnityEngine.Object panelHotUpdate = Resources.Load("UI/Panel_HotUpdate", typeof(GameObject));
            if (panelHotUpdate != null)
            {
                PanelHotUpdate = Instantiate(panelHotUpdate) as GameObject;
                PanelHotUpdate.transform.SetParent(Main.PanelRoot);
                RectTransform rcTransform = PanelHotUpdate.GetComponent<RectTransform>();
                rcTransform.offsetMin = Vector2.zero;
                rcTransform.offsetMax = Vector2.zero;
                rcTransform.anchoredPosition3D = Vector3.zero;
                PanelHotUpdate.transform.localScale = Vector3.one;
                {
                    // TODO:加载更新图上的Tip和背景图更换
                }
                PanelHotUpdate.SetActive(false);
            }
            else
            {
                DeviceLogger.Instance.WriteLog("Failed to Load Panel_HotUpdate!");
            }
        }

        //Splash界面
        {
            UnityEngine.Object panelSplash = Resources.Load("UI/Panel_Splash", typeof(GameObject));
            if (panelSplash != null)
            {
                _PanelSplash = Instantiate(panelSplash) as GameObject;
                _PanelSplash.transform.SetParent(Main.PanelRoot);
                //GameObject img = _PanelSplash.FindChild("Img_Splash");
                RectTransform rcTransform = _PanelSplash.GetComponent<RectTransform>();
                rcTransform.offsetMin = Vector2.zero;
                rcTransform.offsetMax = Vector2.zero;
                rcTransform.anchoredPosition3D = Vector3.zero;
                _PanelSplash.transform.localScale = Vector3.one;

                _PanelSplash.SetActive(false);
            }
            else
            {
                DeviceLogger.Instance.WriteLog("Failed to Load Panel_Splash!");
            }
        }

        //游戏忠告界面
        {
            UnityEngine.Object panelMonition = Resources.Load("UI/Panel_Monition", typeof(GameObject));
            if (panelMonition != null)
            {
                _PanelMonition = Instantiate(panelMonition) as GameObject;
                _PanelMonition.transform.SetParent(Main.PanelRoot, false);
                //RectTransform rcTransform = _PanelMonition.GetComponent<RectTransform>();
                //rcTransform.offsetMin = Vector2.zero;
                //rcTransform.offsetMax = Vector2.zero;
                //rcTransform.anchoredPosition3D = Vector3.zero;
                //_PanelMonition.transform.localScale = Vector3.one;

                _PanelMonition.SetActive(false);
            }
            else
            {
                DeviceLogger.Instance.WriteLog("Failed to Load Panel_Monition!");
            }
        }

        try
        {
            _IsCurlInited = (CURLcode.CURLE_OK == Curl.GlobalInit((int)CURLinitFlag.CURL_GLOBAL_ALL));
            if (!_IsCurlInited)
                DeviceLogger.Instance.WriteLogFormat("Curl Init Failed: Curl.GlobalInit");
            else
                DeviceLogger.Instance.WriteLogFormat("Curl Init Succeed: {0}", Curl.Version);
        }
        catch (Exception e)
        {
            DeviceLogger.Instance.WriteLogFormat("Curl Init Failed: {0}", e.Message);
        }

        StartCoroutine(InitGameCoroutine().GetEnumerator());
        DeviceLogger.Instance.WriteLogFormat("EntryPoint Start End...");
    }

#if false
    private const int MAX_PROTO_PER_FRAME = 60;
    private const int SIMPLE_PROTO_PROCESS_THRESHOLD = 30;

    private IEnumerable ProtocolProcessCoroutine()
    {
        while (true)
        {
            var gameSession = CGameSession.Instance();

            if (gameSession.IsPaused) yield return null;

            var canProcess = true;
            var processedCount = 0;
            var count = gameSession.FetchProtocols();
            if (count > 0)
            {
                for (int i = 0; i < count; i++)
                {
                    if (!canProcess || processedCount >= MAX_PROTO_PER_FRAME)
                        break;

                    gameSession.ProcessOneProtocol(count > SIMPLE_PROTO_PROCESS_THRESHOLD);

                    processedCount++;

                    if (gameSession.IsPaused) canProcess = false;

                    // TODO: 增加限时，如果超出时间上限，yield return

                }
            }

            yield return null;
        }
    }
#endif

    private IEnumerable InitSoundMan()
    {
        /*
        if (UseWwise)
        {
            _SoundMan = WwiseSoundMan.Instance;
        }
        else
        {
            _SoundMan = CSoundMan.Instance;
        }

        foreach (var item in _SoundMan.Init())
        {
            yield return item;
        }
        */
        yield return null;
    }

    private void InitVideoManager()
    {
        if (true)
        {
            _VideoManager = VideoMovieManager.Instance;
        }
        else 
        {
            _VideoManager = VideoPlayerManager.Instance;
        }

        _VideoManager.Init();
    }

#if ONGUI_DEBUG
    bool _IsValidStart = false;
    bool _IsValidEnd = false;
    bool _IsConnected = false;
    bool _CanNavigateTo = false;
    bool _IsCollideByObstacle = false;
    bool _FindFirstConnectedPoint = false;
    Vector3 _FirstConnectedPoint = Vector3.zero;
#endif

    void Update()
    {
        if (!_IsInited) return;

#if ONGUI_DEBUG
        /*
        if (Main.HostPalyer != null)
        {
            Vector3 polyPickExt = new Vector3(1, 256, 1);

            //Vector3 pos = new Vector3(6.78f, 28.74f, 50.72f); 
            Vector3 pos = Main.HostPalyer.transform.position;
            //Vector3 target = new Vector3(6.78f, 28.74f, 47.72f);        //干尸
            //Vector3 target = new Vector3(-21.85f, 27.31f, 38.80f);
            //Vector3 target = new Vector3(77.5f, 46.0f, -69.3f);         //110地图某点
            //Vector3 target = pos + new Vector3(1, 2, 3);           //110二层楼高点

            Vector3 target = new Vector3(142.47f, 24.04f, -127.85f);

            Vector3 nearest = target;
            _IsValidStart = NavMeshManager.Instance.IsValidPositionStrict(pos);

            _IsValidEnd = NavMeshManager.Instance.IsValidPositionStrict(target);

            _IsConnected = PathFindingManager.Instance.IsConnected(pos, target);

            _CanNavigateTo = PathFindingManager.Instance.CanNavigateTo(pos, target, polyPickExt);

            _IsCollideByObstacle = PathFindingManager.Instance.IsCollideWithBlockable(pos, target);

            //_FindFirstConnectedPoint = PathFindingManager.Instance.FindFirstConnectedPoint(pos, target, polyPickExt, 1.0f, out _FirstConnectedPoint);
        }
         * */
#endif
        try
        {
            float dt = Time.deltaTime;

            _TimerList.Tick(true);

            Main.Tick(dt);
            InputManager.Instance.Tick(dt);
            CLogicObjectMan<ObjectBehaviour>.Instance.Tick(dt);
            CLogicObjectMan<EntityEffectComponent>.Instance.Tick(dt);
            LuaScriptMgr.Instance.Tick(dt);
            CGameSession.Instance().Tick(dt);
            UISfxBehaviour.Tick(dt);
            CFxCacheMan.Instance.Tick(dt);
            DynamicEffectManager.Instance.Tick(dt);

            //_SoundMan.Tick(dt);
            VideoManager.Tick(dt);

            CDebugUIMan.Instance.Tick(dt);
        }
        catch (LuaScriptException e)
        {
            HobaDebuger.LogErrorFormat("LuaScriptException: {0}", e.Message);
        }
        catch (Exception e)
        {
            HobaDebuger.LogErrorFormat("{0}\n{1}", e.Message, e.StackTrace);
        }
    }

    void LateUpdate()
    {
        if (!_IsInited) return;

        _LateTimerList.Tick(true);

        float dt = Time.deltaTime;
        CCamCtrlMan.Instance.Tick(dt);
        VoiceManager.Instance.Tick(dt);
        InputManager.Instance.LateTick(dt);
        CFxCacheMan.Instance.LateTick(dt);
        TimedPoolManager.Instance.LateTick(dt);
    }

    private IEnumerable TickCoroutine()
    {
        while (true)
        {
            if (!_IsInited) yield return null;

            foreach (var item in ResCacheMan.Instance.TickCoroutine())
                yield return item;

            foreach (var item in ScenesManager.Instance.TickCoroutine())
                yield return item;

            yield return null;
        }
    }

    void OnApplicationPause(bool pauseStatus)
    {
        SDK.PlatformControl.OnApplicationStateChange(pauseStatus);
        if (IsInited)
        {
            if (!pauseStatus)
            {
                if (_DesignWidth > 0 && _DesignHeight > 0)
                    OSUtility.SetDesignContentScale(_DesignWidth, _DesignHeight);
            }
            else
            {
                LuaScriptMgr.Instance.CallLuaFunction("PauseGame");
            }
        }
    }

    void OnApplicationQuit()
    {
        _IsQuiting = true;

        //CTimerList.ShowDiagnostics();
        //EntityVisualEffect.EffectObjectPool.ShowDiagnostics();
        //MaterialPool.Instance.ShowDiagnostics();
        //FileImage.ShowDiagnostics();
        if (_IsCurlInited)
        {
            Curl.GlobalCleanup();
            _IsCurlInited = false;
        }

        CGameSession.Instance().Close();

        if (_IsInited)
        {
            LuaScriptMgr.Instance.CallLuaFunction("ReleaseGame");
            LuaScriptMgr.Instance.Destroy();
            NavMeshManager.Instance.Release();
            LuaDLL.HOBA_Release();
            _IsInited = false;
        }

        _TimerList.Clear();
        _LateTimerList.Clear();

        CLogicObjectMan<ObjectBehaviour>.Instance.Cleanup();
        CLogicObjectMan<EntityEffectComponent>.Instance.Cleanup();
        ResCacheMan.Instance.Cleanup();
        CFxCacheMan.Instance.Cleanup();

        HobaDebuger.Log("Application Quit!");
    }

    public int AddTimer(float ttl, bool once, int cb, string debugInfo, bool bLateUpdate)
    {
        int id = 0;
        if (bLateUpdate)
        {
            id = _LateTimerList.AddTimer(ttl, once, cb, debugInfo);
        }
        else
        {
            id = _TimerList.AddTimer(ttl, once, cb, debugInfo);
        }
        return id;
    }

    public int AddTimer(float ttl, bool once, Action cb, bool bLateUpdate)
    {
        int id = 0;
        if (bLateUpdate)
        {
            id = _LateTimerList.AddTimer(ttl, once, cb);
        }
        else
        {
            id = _TimerList.AddTimer(ttl, once, cb);
        }
        return id;
    }

    public void RemoveTimer(int id)
    {
        if (id > 0)
        {
            _TimerList.RemoveTimer(id);
            _LateTimerList.RemoveTimer(id);
        }
    }

    public void ResetTimer(int id)
    {
        if (id > 0)
        {
            _TimerList.ResetTimer(id);
            _LateTimerList.ResetTimer(id);
        }
    }

    public static double GetClientTime()
    {
        return (DateTime.UtcNow - Main.DateTimeBegin).TotalMilliseconds;
    }

    public static double GetServerTime()
    {
        return GetClientTime() - _ServerTimeGap;
    }

    public static void ExitGame()
    {
        if (Application.isPlaying)
        {
            Application.Quit();
        }
    }

    private void OnLowMemory()
    {
        //System.GC.Collect();
       // Resources.UnloadUnusedAssets();

        long luamem = LuaScriptMgr.Instance.OnLowMemory();      //在lua中处理 
        HobaDebuger.LogErrorFormat("Game Memory Warning!!! dev: {0} SysMem: {1}, GraphicMem: {2}, LuaMem: {3}",
            SystemInfo.deviceModel,
            SystemInfo.systemMemorySize,
            SystemInfo.graphicsMemorySize,
            luamem);
    }

    ////UI适配 
    ////type : 0 = normal, 1 = Xleft, 2 = XRight
    //const int SAFE_ZONE_OFFSET = 88;    //in px
    //public void TuneUIAspect(int aspect_type)
    //{
    //    RectTransform rt_panelRoot = Main.UIRootCanvas.Find("PanelRoot") as RectTransform;
    //    Rect area;

    //    if (aspect_type == 1)
    //    {
    //        area = new Rect(/*x,y,w,h*/ SAFE_ZONE_OFFSET, 0, Screen.width - SAFE_ZONE_OFFSET, Screen.height);
    //    }
    //    else if (aspect_type == 2)
    //    {
    //        area = new Rect(/*x,y,w,h*/ 0, 0, Screen.width - SAFE_ZONE_OFFSET, Screen.height);
    //    }
    //    else
    //    {
    //        area = new Rect(/*x,y,w,h*/ 0, 0, Screen.width, Screen.height);
    //    }

    //    Vector2 anchorMin = area.position;
    //    Vector2 anchorMax = area.position + area.size;
    //    anchorMin.x /= Screen.width;
    //    anchorMin.y /= Screen.height;
    //    anchorMax.x /= Screen.width;
    //    anchorMax.y /= Screen.height;
    //    rt_panelRoot.anchorMin = anchorMin;
    //    rt_panelRoot.anchorMax = anchorMax;

    //    //IntPtr L = LuaScriptMgr.Instance.GetL();
    //    //if (_CurLuaState == null)
    //    //    _CurLuaState = LuaScriptMgr.Instance.GetLuaState();
    //    //LuaDLL.lua_getglobal(L, "OnTuneUIAspect");
    //    //LuaDLL.lua_pushnumber(L, aspect_type);
    //    //if (!_CurLuaState.PCall(1, 0))
    //    //{
    //    //    HobaDebuger.LogLuaError(LuaDLL.lua_tostring(L, -1), LuaStatic.GetTraceBackInfo(L));
    //    //}

    //    LuaScriptMgr.Instance.CallLuaFunction("OnTuneUIAspect", aspect_type);
    //}
}
