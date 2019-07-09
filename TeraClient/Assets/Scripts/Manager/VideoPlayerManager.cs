using System;
using UnityEngine.Video;
using UnityEngine;
using Common;
using UnityEngine.UI;

public class VideoPlayerManager : MonoBehaviourSingleton<VideoPlayerManager>, IVideoManager
{
    private VideoPlayer _VideoPlayer = null;
    private AudioSource _AudioSource = null;
    //private AudioListener _AudioListener = null;
    private RenderTexture _RenderTexture = null;

    private GameObject _DefaluImgVideoRoot = null;
    private RawImage _DefaluImgVideo = null;

    private bool _IsPauseOnFirstFrame = false;
    private bool _StopOnClick = false;

    private Action _OnStartCallback = null;
    private Action _OnEndCallback = null;
    //private Action _OnErrorCallback = null;

    public bool IsPlaying
    {
        get
        {
            if (_VideoPlayer != null)
                return _VideoPlayer.isPlaying;

            return false;
        }
    }

    public static void InitVideoPlayer(VideoPlayer videoPlayer)
    {
        if (videoPlayer == null) return;

        videoPlayer.playOnAwake = false;
        videoPlayer.waitForFirstFrame = false;
        videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
        videoPlayer.renderMode = VideoRenderMode.RenderTexture;
        videoPlayer.aspectRatio = VideoAspectRatio.NoScaling;
        videoPlayer.source = VideoSource.Url;
    }

    public static RenderTexture SetupRenderTexture(RenderTexture rt = null, SizeMode sm = SizeMode.CG)
    {
        int width = sm == SizeMode.CG ? VideoSizeConfig.STANDARD_VIDEO_WIDTH : VideoSizeConfig.STANDARD_VIDEO_WIDTH_UI;
        int height = sm == SizeMode.CG ? VideoSizeConfig.STANDARD_VIDEO_HEIGHT : VideoSizeConfig.STANDARD_VIDEO_HEIGHT_UI;

        RenderTexture ret = rt;
        if (rt == null || rt.width != width || rt.height != height)
        {
            if(rt != null)
                HobaDebuger.LogWarning("VideoPlayerRT Size is not uniform");

            if (rt != null)
                RenderTexture.ReleaseTemporary(rt);
            ret = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.RGB565);
            ret.name = "VideoPlayerRT" + ret.GetHashCode();
            ret.autoGenerateMips = false;
        }
        return ret;
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
            _DefaluImgVideo = _DefaluImgVideoRoot.transform.Find("Img_Video").GetComponent<RawImage>();
        }
        _DefaluImgVideoRoot.SetActive(false);

        _VideoPlayer = gameObject.AddComponent<VideoPlayer>();
        _AudioSource = gameObject.AddComponent<AudioSource>();
        /*_AudioListener = */gameObject.AddComponent<AudioListener>();

        // Should setup the follow first, or the AudioSource won't work and there will be no sound.
        _AudioSource.playOnAwake = false;
        _AudioSource.Pause();

        InitVideoPlayer(_VideoPlayer);
        _VideoPlayer.SetTargetAudioSource(0, _AudioSource);

        _RenderTexture = SetupRenderTexture();

        //_VideoPlayer.targetTexture = _RenderTexture;
        //_Img_Video.texture = _RenderTexture;

        _VideoPlayer.prepareCompleted += Prepared;
        _VideoPlayer.loopPointReached += EndReached;
        _VideoPlayer.errorReceived += OnVideoError;
        _VideoPlayer.started += OnStarted;

        //public event EventHandler frameDropped;
        //public event FrameReadyEventHandler frameReady;
        //public event EventHandler seekCompleted;
    }

    public void Tick(float dt)
    {
        if (IsPlaying && _StopOnClick)
        {
            if (InputManager.HasTouch())
            {
                StopVideo();
            }
        }
    }

    public void PlayVideo(string videoPath, RawImage destRawImage, Action onStartAction, Action onEndAction,
        bool isLoop = false, bool isPauseOnFirstFrame = false, bool stopOnClick = false, SizeMode sm = SizeMode.CG)
    {
        if (_VideoPlayer == null) return;

        if (_RenderTexture == null)
        {
            HobaDebuger.LogWarning("VidePlayer enable failed, RenderTexture got null");
            return;
        }

        _RenderTexture = SetupRenderTexture(_RenderTexture, sm);

        if (_VideoPlayer.isPlaying)
            StopVideo();

        if (destRawImage == null)
        {
            if (_DefaluImgVideoRoot == null || _DefaluImgVideo == null) return;

            if (!_DefaluImgVideoRoot.activeSelf)
                _DefaluImgVideoRoot.SetActive(true);

            _DefaluImgVideo.texture = _RenderTexture;
        }
        else
        {
            if (_DefaluImgVideoRoot != null && _DefaluImgVideoRoot.activeSelf)
                _DefaluImgVideoRoot.SetActive(false);

            if (_DefaluImgVideo != null)
                _DefaluImgVideo.texture = null;

            destRawImage.texture = _RenderTexture;
        }

        _OnStartCallback = onStartAction;
        _OnEndCallback = onEndAction;
        _StopOnClick = stopOnClick;
        _IsPauseOnFirstFrame = isPauseOnFirstFrame;

        _VideoPlayer.url = videoPath;
        _VideoPlayer.isLooping = isLoop;

        float fVolume = 0.5f;
        if (EntryPoint.Instance.GamePersistentConfigParams.ReadFromFile())
            fVolume = EntryPoint.Instance.GamePersistentConfigParams.BGMVolume;
        _AudioSource.volume = fVolume;
        //_VideoPlayer.SetDirectAudioVolume(0, volume);

        _VideoPlayer.targetTexture = _RenderTexture;
        SetAspectMode(destRawImage);
        _VideoPlayer.Prepare();
    }

    void SetAspectMode(RawImage destImage)
    {
        if (destImage == null)
        {
            destImage = _DefaluImgVideo;
        }
        GNewUITools.SetAspectMode(destImage != null ? destImage.gameObject : null, _RenderTexture.width / (float)_RenderTexture.height, (int)AspectRatioFitter.AspectMode.EnvelopeParent);
    }

    ////mode 0 cg, 1 ui
    //public void SetAspectMode(GameObject destGameObject, int mode)
    //{
    //    if (destGameObject == null)
    //    {
    //        destGameObject = _DefaluImgVideoRoot != null ? _DefaluImgVideoRoot.gameObject : null;
    //    }

    //    GNewUITools.SetAspectMode(destGameObject, STANDARD_VIDEO_WIDTH / STANDARD_VIDEO_HEIGHT, mode);
    //}

    public void ContinueVideo()
    {
        if (_VideoPlayer == null || !_VideoPlayer.isPrepared || _VideoPlayer.isPlaying) return;

        float fVolume = 0.5f;
        if (EntryPoint.Instance.GamePersistentConfigParams.ReadFromFile())
            fVolume = EntryPoint.Instance.GamePersistentConfigParams.BGMVolume;
        _AudioSource.volume = fVolume;
        //_VideoPlayer.SetDirectAudioVolume(0, volume);

        _VideoPlayer.Play();
    }

    public void StopVideo()
    {
        if (_VideoPlayer != null && _VideoPlayer.isPlaying)
        {
            _VideoPlayer.Stop();
            OnVideoStop();
        }
        if (_OnEndCallback != null)
        {
            _OnEndCallback.Invoke();
            _OnEndCallback = null;
        }
    }

    private void OnVideoStop()
    {
        if (Main.Main3DCamera != null)
            Main.Main3DCamera.enabled = true;

        if (_DefaluImgVideoRoot != null && _DefaluImgVideoRoot.activeSelf)
            _DefaluImgVideoRoot.SetActive(false);
        if (_DefaluImgVideo != null)
            _DefaluImgVideo.texture = null;

        if (_RenderTexture != null)
        {
            RenderTexture rt = UnityEngine.RenderTexture.active;
            UnityEngine.RenderTexture.active = _RenderTexture;
            GL.Clear(false, true, Color.clear);
            UnityEngine.RenderTexture.active = rt;
        }
    }

    private void OnDestroy()
    {
        if (_RenderTexture != null)
        {
            RenderTexture.ReleaseTemporary(_RenderTexture);
            _RenderTexture = null;
        }
    }

    #region VideoPlayer回调
    private void Prepared(VideoPlayer player)
    {
        player.Play();
        if (_OnStartCallback != null)
        {
            _OnStartCallback.Invoke();
            _OnStartCallback = null;
        }

        if (_IsPauseOnFirstFrame)
        {
            player.Pause();
        }
    }

    private void EndReached(VideoPlayer player)
    {
        if (!player.isLooping)
            OnVideoStop();
        if (_OnEndCallback != null)
        {
            _OnEndCallback.Invoke();
            _OnEndCallback = null;
        }
    }

    private void OnVideoError(VideoPlayer player, string message)
    {
        HobaDebuger.LogErrorFormat("VideoPlayer got error: {0}", message);

        StopVideo();
    }

    private void OnStarted(VideoPlayer player)
    {
        if (Main.Main3DCamera != null)
            Main.Main3DCamera.enabled = false;
    }

    public static void PauseVideoUnitOnFirstFrame(VideoPlayer player, long frameIdx)
    {
        player.Pause();
        player.sendFrameReadyEvents = false;
        player.frameReady -= PauseVideoUnitOnFirstFrame;
    }
    #endregion
}