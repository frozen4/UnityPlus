using System;
using UnityEngine;
using Common;
using UnityEngine.UI;


public class VideoMovieManager : MonoBehaviourSingleton<VideoMovieManager>, IVideoManager
{
    private GameObject _VideoManagerObj = null;
    private string _VideoName = "";

    private MediaPlayerCtrl _MediaCtrl = null;
    private Texture2D _AlphaTexture = null;
    private bool _IsLoop = false;
    private SizeMode _SizeMode = SizeMode.CG;

    private GameObject _DefaluImgVideoRoot = null;
    private GameObject _DefaluImgVideo = null;

    private bool _IsPauseOnFirstFrame = false;
    private bool _StopOnClick = false;

    private Action _OnStartCallback = null;
    private Action _OnEndCallback = null;
    
    public bool IsPlaying
    {
        get
        {
            if (_MediaCtrl != null)
                return _MediaCtrl.GetCurrentState() == MediaPlayerCtrl.MEDIAPLAYER_STATE.PLAYING;
            return false;
        }
    }

    void Awake()
    {
        this.enabled = false;
    }

    public void Init()
    {
        var panel = Resources.Load<GameObject>("UI/Panel_Video");
        if (panel == null) return;

        // 生成背景视频UI
        {
            _DefaluImgVideoRoot = GameObject.Instantiate(panel);
            _DefaluImgVideoRoot.transform.SetParent(Main.PanelRoot);
            var rcTransform = _DefaluImgVideoRoot.GetComponent<RectTransform>();
            rcTransform.offsetMin = Vector2.zero;
            rcTransform.offsetMax = Vector2.zero;
            rcTransform.anchoredPosition3D = Vector3.zero;
            _DefaluImgVideoRoot.transform.localScale = Vector3.one;
            _DefaluImgVideo = _DefaluImgVideoRoot.transform.Find("Img_Video").gameObject;
        }
        _DefaluImgVideoRoot.SetActive(false);

        //
        if (_VideoManagerObj == null)
        {
            UnityEngine.Object prefab = Resources.Load("VideoMovieManager", typeof(GameObject));
            if (prefab != null)
            {
                _VideoManagerObj = Instantiate(prefab) as GameObject;
                _VideoManagerObj.name = "VideoManagerCtrl";
                _VideoManagerObj.transform.parent = this.transform;

                _VideoManagerObj.AddComponent<AudioListener>();

                _MediaCtrl = _VideoManagerObj.GetComponent<MediaPlayerCtrl>();
            }
        }

        _AlphaTexture = Resources.Load<Texture2D>("AlphaTexture");
    }

    void OnEnable()
    {
        if (Main.Main3DCamera != null)
            Main.Main3DCamera.enabled = false;

        if (_VideoManagerObj != null)
        {
            this.Resize();

            _MediaCtrl = _VideoManagerObj.GetComponent<MediaPlayerCtrl>();
            if (_MediaCtrl == null)
            {
                _MediaCtrl = _VideoManagerObj.AddComponent<MediaPlayerCtrl>();
            }
            _MediaCtrl.m_strFileName = _VideoName;
            _MediaCtrl.m_bFullScreen = true; //全屏
            _MediaCtrl.m_bLoop = _IsLoop; //是否循环播放
            _MediaCtrl.m_bAutoPlay = true; //自动播放
            _MediaCtrl.OnReady = OnVideoReady;
            _MediaCtrl.OnEnd = OnVideoEnd;
            if (_IsPauseOnFirstFrame)
                _MediaCtrl.OnVideoFirstFrameReady = OnVideoFirstFrameReady;
            else
                _MediaCtrl.OnVideoFirstFrameReady = null;
            _MediaCtrl.DeleteVideoTexture();

            _MediaCtrl.Load(_MediaCtrl.m_strFileName);

            _VideoManagerObj.SetActive(true);
        }
    }

    public bool HasError()
    {
        if (_MediaCtrl != null)
        {
            return _MediaCtrl.GetCurrentState() == MediaPlayerCtrl.MEDIAPLAYER_STATE.ERROR;
        }
        return false;
    }


    public void Tick(float dt)
    {
        if (IsPlaying && _StopOnClick)
        {
            if (HasError() || InputManager.HasTouch())
            {
                StopVideo();
            }
        }
    }

    void OnDisable()
    {
        if (Main.Main3DCamera != null)
            Main.Main3DCamera.enabled = true;

        if (_DefaluImgVideoRoot != null && _DefaluImgVideoRoot.activeSelf)
            _DefaluImgVideoRoot.SetActive(false);
        PreprareRawImage(_DefaluImgVideo);

        if (_MediaCtrl != null)
        {
            _MediaCtrl.Stop();
            _MediaCtrl.DeleteVideoTexture();
            _MediaCtrl.UnLoad();
        }

        if (_VideoManagerObj != null)
        {
            _VideoManagerObj.SetActive(false);
        }

        _VideoName = "";
        _OnEndCallback = null;
        _StopOnClick = false;
    }

    void OnDestroy()
    {
        if (_MediaCtrl != null)
        {
            _MediaCtrl.Stop();
            _MediaCtrl.DeleteVideoTexture();
            _MediaCtrl.UnLoad();
        }

        if (_VideoManagerObj != null)
        {
            _VideoManagerObj.SetActive(false);
            _VideoManagerObj = null;
        }
    }

    void OnVideoReady()
    {
        this.Resize();
        _VideoManagerObj.SetActive(true);

        if (_OnStartCallback != null)
        {
            _OnStartCallback.Invoke();
            _OnStartCallback = null;
        }
    }

    void OnVideoEnd()
    {
        var endCallback = _OnEndCallback;
        if (_MediaCtrl != null && !_MediaCtrl.m_bLoop)
            this.enabled = false;

        if (endCallback != null)
            endCallback.Invoke();
    }

    void OnVideoError(MediaPlayerCtrl.MEDIAPLAYER_ERROR errorCode,MediaPlayerCtrl.MEDIAPLAYER_ERROR errorCodeExtra)
    {
        HobaDebuger.LogErrorFormat("VideoPlayer got error: {0}, {1}", errorCode, errorCodeExtra);

        StopVideo();
    }

    public void PlayVideo(string videoPath, RawImage destRawImage, Action onStartAction, Action onEndAction,
        bool isLoop = false, bool isPauseOnFirstFrame = false, bool stopOnClick = false, SizeMode sm = SizeMode.CG)
    {
        if (string.IsNullOrEmpty(videoPath))
            return;

        if (IsPlaying)
            StopVideo();

        if (destRawImage == null)
        {
            if (_DefaluImgVideoRoot == null || _DefaluImgVideo == null) return;

            if (!_DefaluImgVideoRoot.activeSelf)
                _DefaluImgVideoRoot.SetActive(true);

            _MediaCtrl.m_TargetMaterial[0] = _DefaluImgVideo;
        }
        else
        {
            if (_DefaluImgVideoRoot != null && _DefaluImgVideoRoot.activeSelf)
                _DefaluImgVideoRoot.SetActive(false);

            if (_DefaluImgVideo != null)
            {
                RawImage rawImage = _DefaluImgVideo.GetComponent<RawImage>();
                if (rawImage != null)
                    rawImage.texture = null;
            }

            _MediaCtrl.m_TargetMaterial[0] = destRawImage.gameObject;
        }

        _OnStartCallback = onStartAction;
        _OnEndCallback = onEndAction;
        _StopOnClick = stopOnClick;
        _IsPauseOnFirstFrame = isPauseOnFirstFrame;

        _VideoName = ProcessVideoPath(videoPath);
        _IsLoop = isLoop;

        float fVolume = 0.5f;
        if (EntryPoint.Instance.GamePersistentConfigParams.ReadFromFile())
            fVolume = EntryPoint.Instance.GamePersistentConfigParams.BGMVolume;
        _MediaCtrl.SetVolume(fVolume);

        PreprareRawImage(_MediaCtrl.m_TargetMaterial[0]);

        this.enabled = true;
    }

    string ProcessVideoPath(string videoPath)
    {
        string ret = videoPath;

        string streamingPath = Application.streamingAssetsPath;
        if (streamingPath.EndsWith("/") || streamingPath.EndsWith("\\"))
        {
            if (videoPath.StartsWith(streamingPath))
                ret = videoPath.Replace(streamingPath, "");
            else if (!videoPath.StartsWith("file://"))
                ret = "file://" + videoPath;
        }
        else
        {
            string str1 = streamingPath + "/";
            string str2 = streamingPath + "\\";

            if (videoPath.StartsWith(str1))
                ret = videoPath.Replace(str1, "");
            else if (videoPath.StartsWith(str2))
                ret = videoPath.Replace(str2, "");
            else if (!videoPath.StartsWith("file://"))
                ret = "file://" + videoPath;
        }

        return ret;
    }

    void PreprareRawImage(GameObject go)
    {
        if (go == null)
            return;

        RawImage rawImage = go.GetComponent<RawImage>();
        if (rawImage != null)
        {
            rawImage.texture = _AlphaTexture;
#if UNITY_IPHONE  || UNITY_TVOS || UNITY_EDITOR || UNITY_STANDALONE
            rawImage.uvRect = new Rect(0, 1, 1, -1);        //翻转
#else
            rawImage.uvRect = new Rect(0, 0, 1, 1);
#endif
        }

        int width = _SizeMode == SizeMode.CG ? VideoSizeConfig.STANDARD_VIDEO_WIDTH : VideoSizeConfig.STANDARD_VIDEO_WIDTH_UI;
        int height = _SizeMode == SizeMode.CG ? VideoSizeConfig.STANDARD_VIDEO_HEIGHT : VideoSizeConfig.STANDARD_VIDEO_HEIGHT_UI;
        GNewUITools.SetAspectMode(go, (float)width / (float)height, (int)AspectRatioFitter.AspectMode.EnvelopeParent);
    }

    public void ContinueVideo()
    {
        if (_MediaCtrl == null || IsPlaying) return;

        float fVolume = 0.5f;
        if (EntryPoint.Instance.GamePersistentConfigParams.ReadFromFile())
            fVolume = EntryPoint.Instance.GamePersistentConfigParams.BGMVolume;
        _MediaCtrl.SetVolume(fVolume);

        _MediaCtrl.Play();
    }

    public void StopVideo()
    {
        var endCallback = _OnEndCallback;
        _VideoName = "";
        this.enabled = false;

        if (endCallback != null)
            endCallback.Invoke();
    }

    private void Resize()
    {
        if (_VideoManagerObj != null)
        {
            int width = Screen.width;
            int height = Screen.height;
            float fRatio = (float)height / (float)width;

            _VideoManagerObj.transform.localScale = new Vector3(10.0f / fRatio, 10.0f, 1.0f);

#if !UNITY_WEBGL
            _VideoManagerObj.transform.GetComponent<MediaPlayerCtrl>().Resize();
#endif
        }
    }

    // 暂停视频开始播放的第一帧
    private void OnVideoFirstFrameReady()
    {
        if (_MediaCtrl != null)
        {
            _MediaCtrl.Pause();
            _MediaCtrl.OnVideoFirstFrameReady = null;
        }
    }
}