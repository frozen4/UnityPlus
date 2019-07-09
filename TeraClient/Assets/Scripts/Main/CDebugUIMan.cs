using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CDebugUIMan : MonoBehaviourSingleton<CDebugUIMan>, GameLogic.ITickLogic
{
    public bool IsDebugInfoDisplay = true;

    public GameObject PanelRoot;
    public Text FPSLabel;
    public Text PingLabel;

    private float _LastFPSTime = 0;
    private int _CurFrameCount;
    private float _FPS = 0;

    public void Tick(float delta)
    {
        if (!IsDebugInfoDisplay || FPSLabel == null) return;

        if (_LastFPSTime == 0)
        {
            _LastFPSTime = Time.unscaledTime;
            _CurFrameCount = Time.frameCount;
            FPSLabel.text = string.Empty;
			_FPS = 0;
        }
        else
        {
            float curTime = Time.unscaledTime;

            if (curTime - _LastFPSTime >= 1.0f)
            {
                int nFrameCount = Time.frameCount;

                if (FPSLabel.enabled)
                {
                    float fps = nFrameCount - _CurFrameCount;
                    float dt = curTime - _LastFPSTime;
                    _FPS = (dt == 0 ? 0 : fps / dt);
                    FPSLabel.text = HobaText.Format("{0:.00}", _FPS);
                    _CurFrameCount = nFrameCount;

                    if (fps < 20)
                        FPSLabel.color = Color.red;
                    else if (fps < 25)
                        FPSLabel.color = Color.yellow;
                    else
                        FPSLabel.color = Color.green;
                }
                _LastFPSTime = curTime;
            }
        }
    }

    public void UpdatePing(float ping)
    {
        if (!IsDebugInfoDisplay || PingLabel == null) return;

        var pingStr = HobaText.Format("{0:f2}", ping);
        PingLabel.text = pingStr;
    }

    public void EnableDisplay(bool enable)
    {
        IsDebugInfoDisplay = enable;

        if(PanelRoot != null && enable != PanelRoot.activeSelf)
            PanelRoot.SetActive(enable);
    }
}