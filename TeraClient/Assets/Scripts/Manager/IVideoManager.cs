using System;
using UnityEngine.UI;

public enum SizeMode
{
    CG = 0,
    UI = 1
}

public static class VideoSizeConfig
{
    public const int STANDARD_VIDEO_WIDTH = 1280;
    public const int STANDARD_VIDEO_HEIGHT = 720;

    public const int STANDARD_VIDEO_WIDTH_UI = 1280;
    public const int STANDARD_VIDEO_HEIGHT_UI = 960;
}

public interface IVideoManager : GameLogic.ITickLogic
{
    bool IsPlaying { get; }

    void Init();

    void PlayVideo(string videoPath, RawImage destRawImage, Action onStartAction, Action onEndAction,
        bool isLoop = false, bool isPauseOnFirstFrame = false, bool stopOnClick = false, SizeMode sm = SizeMode.CG);

    void StopVideo();

    void ContinueVideo();
}