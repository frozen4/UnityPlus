using System;
using UnityEngine;
using CinemaDirector;
using LuaInterface;
using System.Collections.Generic;
using System.Text;
using Mono.Xml;
using System.Security;
using CG;
using UnityEngine.UI;
using System.Collections;

public class CGManager : MonoBehaviourSingleton<CGManager>
{
    // UI摄像机调整层级
    private const int CGGUI_CAMERA_DEPTH = 6;
    private const string VIDEO_CG_SUFFIX = ".mp4";
    private const string VIDEO_CG_COMMON_UI_PATH = "Assets/Outputs/CG/City01/CG_CommonVideo.prefab";

    public Action OnPlayCallback = null;
    public Action OnStopCallback = null;

    private readonly CommonCgConfig _CgXmlConfig = new CommonCgConfig();
    private CGGlobal _CurCGGlobal = null;
    private Cutscene _CurCutscene = null;

    // 视频CG通用UI框
    private GameObject _CommonVideoFrame = null;
    private bool _IsCommonVideoFrameLoading = false;
    private RawImage _VideoRawImage = null;
    private GameObject _DialogueRoot = null;
    private Text _DialogueNameText = null;
    private Text _DialogueContentText = null;
    private readonly List<int> _DialogueTimerList = new List<int>();

    private readonly CGMask _CGMask = new CGMask();

    public bool CanSkip
    {
        get { return !_CgXmlConfig.DontSkip; }
    }

    public Cutscene CurCutscene
    {
        get { return _CurCutscene; }
    }

    public CGGlobal CurCGGlobal
    {
        get { return _CurCGGlobal; }
    }

    public void PlayCG(string cgPath, int priority, Action callback, bool dontSkip)
    {
        if (_CgXmlConfig.Path == cgPath || _CgXmlConfig.Priority > priority)
            return;

        //分析CG名字
        var cgName = System.IO.Path.GetFileNameWithoutExtension(cgPath); 
        if (string.IsNullOrEmpty(cgName)) return;

        if (OnPlayCallback != null)
            OnPlayCallback();

        _CgXmlConfig.Reset();

        var isVideo = cgPath.Contains(VIDEO_CG_SUFFIX);
        _CgXmlConfig.Path = cgPath;
        _CgXmlConfig.Priority = priority;
        _CgXmlConfig.IsVideo = isVideo;
        _CgXmlConfig.StartCallback = callback;
        _CgXmlConfig.DontSkip = dontSkip;

        ParseCgConfigFile(cgName, isVideo);

        if (_CgXmlConfig.IsMaskShown)
        {
            _CGMask.Show(delegate
                {
                    if (_CgXmlConfig.IsVideo)
                        PlayVideoCG();
                    else
                        PlayCommonCG();
                });
        }
        else
        {
            if (_CgXmlConfig.IsVideo)
                PlayVideoCG();
            else
                PlayCommonCG();
        }
    }

    public void StopCG()
    {
        if (_CgXmlConfig.IsVideo)
            EntryPoint.Instance.VideoManager.StopVideo();

        if (_CurCutscene != null)
            _CurCutscene.Skip();

        if (OnStopCallback != null)
            OnStopCallback();
    }

    private void ParseCgConfigFile(string cgName, bool isVideo)
    {
        string configsDir = System.IO.Path.Combine(EntryPoint.Instance.ResPath, LuaScriptMgr.Instance.GetConfigsDir());

        var configPath = string.Empty;
        if(isVideo)
            configPath = System.IO.Path.Combine(configsDir, "VideoConfig.xml");
        else
            configPath = System.IO.Path.Combine(configsDir, "CgConfig.xml");

        var bytes = Util.ReadFile(configPath);
        if (bytes == null) return;

        try
        {
            string text = Encoding.UTF8.GetString(bytes);
            SecurityParser sp = Main.XMLParser;
            sp.LoadXml(text);
            SecurityElement root = sp.ToXml();
            foreach (SecurityElement se1 in root.Children)
            {
                if (se1.Tag != cgName) continue;

                foreach (SecurityElement se2 in se1.Children)
                {
                    if (se2.Tag == "Dialogue")
                    {
                        foreach (SecurityElement se3 in se2.Children)
                        {
                            var dialogue = new Dialogue();
                            dialogue.Name = se3.Attribute("name");
                            dialogue.Content = se3.Attribute("content");
                            var showStr = se3.Attribute("show");
                            dialogue.ShowTime = string.IsNullOrEmpty(showStr)? 0 : float.Parse(showStr);
                            var closeStr = se3.Attribute("close");
                            dialogue.HideTime = string.IsNullOrEmpty(closeStr) ? 0 : float.Parse(closeStr);

                            if (!_CgXmlConfig.Dialogues.ContainsKey(se3.Tag))
                                _CgXmlConfig.Dialogues.Add(se3.Tag, dialogue);
                        }
                    }
                    else if (se2.Tag == "Name")
                    {
                        foreach (SecurityElement se3 in se2.Children)
                        {
                            if (!_CgXmlConfig.Names.ContainsKey(se3.Tag))
                                _CgXmlConfig.Names.Add(se3.Tag, se3.Attribute("name"));
                        }
                    }
                    else if (se2.Tag == "CGMask")
                    {
                        _CgXmlConfig.IsMaskShown = bool.Parse(se2.Attribute("mask"));
                    }
                }
                break;
            }
            sp.Clear();
        }
        catch (Exception e)
        {
            Debug.LogError(HobaText.Format(e.Message));
        }
    }

    private void PlayCommonCG()
    {
        Action<UnityEngine.Object> callback = (asset) =>
        {
            // 异步加载中 可能存在其他操作（比如cg结束 切换成功）
            if (asset == null || _CgXmlConfig.Path == null || !_CgXmlConfig.Path.Contains(asset.name))
                return;

            var cg = CUnityUtil.Instantiate(asset) as GameObject;
            if (cg == null) return;

            var cgg = cg.GetComponent<CGGlobal>();
            if (cgg == null)
            {
                Destroy(cg);
                return;
            }

            _CurCGGlobal = cgg;
            _CurCutscene = cg.GetComponentInChildren<Cutscene>();
            _CurCutscene.CutsceneStarted += OnCutsceneStarted;
            _CurCutscene.CutsceneFinished += OnCutsceneFinished;

            if (_CgXmlConfig.StartCallback != null)
            {
                _CgXmlConfig.StartCallback();
                _CgXmlConfig.StartCallback = null;
            }

            if (_CgXmlConfig.IsMaskShown)
                _CGMask.Hide();

            if (_CurCutscene != null)
            {
                _CurCutscene.Optimize();
                _CurCutscene.Play();
            }
        };
        CAssetBundleManager.AsyncLoadResource(_CgXmlConfig.Path, callback, false);
    }

    private void PlayVideoCG()
    {
        if(_CommonVideoFrame == null)
        {
            if(_IsCommonVideoFrameLoading) return;

            Action<UnityEngine.Object> callback = (asset) =>
            {
                var cg = CUnityUtil.Instantiate(asset) as GameObject;
                if (cg == null) return;

                _CommonVideoFrame = cg;
                _VideoRawImage = cg.transform.Find("UICanvas/UI_Video/Img_Video").GetComponent<RawImage>();
                _DialogueRoot = cg.transform.Find("UICanvas/UIBasic/Down").gameObject;
                _DialogueNameText = _DialogueRoot.transform.Find("Name").GetComponent<Text>();
                _DialogueContentText = _DialogueRoot.transform.Find("Content").GetComponent<Text>();

                _IsCommonVideoFrameLoading = false;

                // 异步加载中 可能存在其他操作（比如cg结束 切换成功）
                if (asset == null || _CgXmlConfig.Path == null || !_CgXmlConfig.IsVideo)
                    return;

                PlayVideoImp();
            };
            _IsCommonVideoFrameLoading = true;
            CAssetBundleManager.AsyncLoadResource(VIDEO_CG_COMMON_UI_PATH, callback, false);
        }
        else
        {
            PlayVideoImp();
        }
    }

    private void PlayVideoImp()
    {
        if (_CommonVideoFrame == null) return;

        var cgg = _CommonVideoFrame.GetComponent<CGGlobal>();
        if (cgg == null) return;

        _CurCGGlobal = cgg;
        _CurCutscene = _CommonVideoFrame.GetComponentInChildren<Cutscene>();

        foreach (var v in _DialogueTimerList)
            EntryPoint.Instance.RemoveTimer(v);
        _DialogueTimerList.Clear();

        _DialogueRoot.SetActive(false);

        _CurCutscene.CutsceneStarted += OnCutsceneStarted;
        _CurCutscene.CutsceneFinished += OnCutsceneFinished;

        if (!string.IsNullOrEmpty(_CgXmlConfig.Path))
        {
            Action onStartFunc = () =>
            {
                if (_CgXmlConfig.StartCallback != null)
                {
                    _CgXmlConfig.StartCallback();
                    _CgXmlConfig.StartCallback = null;
                }

                if (_CurCutscene != null)
                {
                    _CurCutscene.Optimize();
                    _CurCutscene.Play();
                }

                if (_CgXmlConfig.IsMaskShown)
                    _CGMask.Hide();

                foreach (var v in _CgXmlConfig.Dialogues)
                {
                    if (v.Value != null && v.Value.ShowTime >= 0)
                    {
                        var timerId = EntryPoint.Instance.AddTimer(v.Value.ShowTime, true,
                            delegate
                            {
                                if (!_DialogueRoot.activeSelf)
                                    _DialogueRoot.SetActive(true);
                                _DialogueNameText.text = v.Value.Name;
                                _DialogueContentText.text = v.Value.Content;
                            }, false);

                        _DialogueTimerList.Add(timerId);
                    }

                    if (v.Value != null && Util.IsZero(v.Value.HideTime))
                    {
                        var timerId = EntryPoint.Instance.AddTimer(v.Value.HideTime, true,
                            delegate
                            {
                                if (_DialogueRoot.activeSelf)
                                    _DialogueRoot.SetActive(false);
                            }, false);

                        _DialogueTimerList.Add(timerId);
                    }
                }
            };
            Action onEndFunc = () =>
            {
                foreach (var v in _DialogueTimerList)
                    EntryPoint.Instance.RemoveTimer(v);
                _DialogueTimerList.Clear();

                if(_CurCutscene != null)
                    _CurCutscene.Skip();
            };

            var videoPath = EntryPoint.Instance.GetFullResPath("Video", _CgXmlConfig.Path);

            if (System.IO.File.Exists(videoPath))
            {
                EntryPoint.Instance.VideoManager.PlayVideo(videoPath, _VideoRawImage, onStartFunc, onEndFunc);
            }
            else
            {
                Common.HobaDebuger.LogWarningFormat("CgPath does not exist! {0}", videoPath);
                onStartFunc.Invoke();
                onEndFunc.Invoke();
            }
        }
    }

    /// CG开始播放
    public void OnCutsceneStarted(object sender, CutsceneEventArgs e)
    {
        Cutscene cs = sender as Cutscene;
        if (cs != _CurCutscene || _CurCutscene == null)
        {
            Common.HobaDebuger.LogWarningFormat("Logic Error: when call OnCutsceneStarted, another cg is playing");
            return;
        }

        var useCGCamera = _CurCGGlobal.cameraType == CGGlobal.CameraType.CGCamera;
        if (useCGCamera)
        {
            ChangeOtherSetting(false, _CurCGGlobal.UseUICamera);
            if (_CurCGGlobal.WeatherId > 0)
                DynamicEffectManager.Instance.EnterCGEffect(_CurCGGlobal.WeatherId, _CurCGGlobal.GetComponentsInChildren<PostProcessChain>(true));
        }

        {
            IntPtr L = LuaScriptMgr.Instance.GetL();
            try
            {
                var ls = LuaScriptMgr.Instance.GetLuaState();
                var oldTop = LuaDLL.lua_gettop(L);
                LuaDLL.lua_getglobal(L, "OnCGStart");
                LuaDLL.lua_pushboolean(L, useCGCamera);
                if (!ls.PCall(1, 0))
                    Common.HobaDebuger.LogLuaError(LuaDLL.lua_tostring(L, -1));
                LuaDLL.lua_settop(L, oldTop);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                Debug.LogError(LuaStatic.GetTraceBackInfo(L));
            }
        }

        _CurCutscene.CutsceneStarted -= OnCutsceneStarted;
    }

    /// CG播放完回调
    public void OnCutsceneFinished(object sender, CutsceneEventArgs e)
    {
        Cutscene cs = sender as Cutscene;
        if (cs != _CurCutscene || _CurCutscene == null)
        {
            Common.HobaDebuger.LogWarningFormat("Logic Error: when call OnCutsceneStarted, another cg is playing");
            return;
        }

        var useCGCamera = _CurCGGlobal.cameraType == CGGlobal.CameraType.CGCamera;
        if (useCGCamera)
        {
            ChangeOtherSetting(true, _CurCGGlobal.UseUICamera);
            DynamicEffectManager.Instance.EnterCGEffect(-1, null);
        }

        {
            IntPtr L = LuaScriptMgr.Instance.GetL();
            try
            {
                var ls = LuaScriptMgr.Instance.GetLuaState();
                var oldTop = LuaDLL.lua_gettop(L);
                LuaDLL.lua_getglobal(L, "OnCGFinish");
                LuaDLL.lua_pushboolean(L, useCGCamera);
                if (!ls.PCall(1, 0))
                    Common.HobaDebuger.LogLuaError(LuaDLL.lua_tostring(L, -1));
                LuaDLL.lua_settop(L, oldTop);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                Debug.LogError(LuaStatic.GetTraceBackInfo(L));
            }
        }

        _CurCutscene.CutsceneFinished -= OnCutsceneFinished;
        _CurCutscene = null;

        if (_CgXmlConfig.IsVideo)
            EntryPoint.Instance.VideoManager.StopVideo();

        Destroy(_CurCGGlobal.gameObject);
        _CurCGGlobal = null;

        _CgXmlConfig.Reset();
    }

    private void ChangeOtherSetting(bool beReset, bool useUiCamera)
    {
        Main.EnableMainCamera(beReset, Main.CameraEnableMask.VideoPlaying);
        Main.SetUIStateByCG(beReset, useUiCamera, CGGUI_CAMERA_DEPTH);
    }

    /// 是否屏蔽外来输入
    public bool IsForbidInput()
    {
        return (_CurCGGlobal != null && _CurCGGlobal.cameraType == CGGlobal.CameraType.CGCamera);
    }

    public Dialogue GetDialogueById(string id)
    {
        Dialogue dialog;
        _CgXmlConfig.Dialogues.TryGetValue(id, out dialog);

        return dialog;
    }
}

