#define HOT_UPDATE
//#define LOG_WWISE
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;

#if false

public class WwiseSoundMan : MonoBehaviourSingleton<WwiseSoundMan>, ISoundMan
{
    private const int MaxEffect3DChannel = 12;                //最大的技能音效
    private const int MaxAttachedEffect3DChannel = 12;          //最大的子物体音效
    private const int MaxEffect2DChannel = 4;                //最大的UI音效
    private const int MaxStaticEffectChannel = 2;           //最大的静态音效
    private const int MaxFootStepEffectChannel = 5;            //最大的脚步声音效
    private const int MaxNpcVoiceEffectChannel = 1;
    private const int MaxNpcShoutEffectChannel = 3;
    private const int MaxHeartBeatEffectChannel = 1;
    public GameObject BanksLoaded;
    public GameObject AudioItems;
    private const float MaxEffectAudioDistSqr = 10000;
    private const float MaxStaticObjectDistSqr = 2500;

    private EffectAudioSourceItem BGEffectAudioItem = null;

    public enum EffectSoundType
    {
        Effect3D = 0,
        AttachEffect3D,
        Effect2D,
        Static3D,
        FootStep3D,
        NpcVoice3D,
        NpcShout3D,
        Heartbeat2D,
    }

    public class CSoundEntry
    {
        public EffectAudioSourceItem Item = new EffectAudioSourceItem();
        public bool IsEnabled = true;

        public void Reset()
        {
            if (Item.attachedObj != null)
            {
                Item.Stop();
                GameObject.Destroy(Item.attachedObj);
                Item.attachedObj = null;
            }
            Item.Reset();
        }
    }

    public class EffectAudioSourceItem
    {
        public int id = -1;
        public int priority;
        public string audioName = string.Empty;
        public string switchGroup = string.Empty;
        public string switchState = string.Empty;
        public GameObject attachedObj;          //event attached to
        public Transform attachedPos;
        public string playingId = string.Empty;

        public void Reset()
        {
            id = -1;
            priority = 0;
            audioName = string.Empty;
            attachedObj = null;
            attachedPos = null;
            playingId = string.Empty;
        }

        public bool IsPlaying()
        {
            return !string.IsNullOrEmpty(playingId);
        }

        public bool HaveSwitch()
        {
            return !string.IsNullOrEmpty(switchGroup) && !string.IsNullOrEmpty(switchState);
        }

        private void BgmEventCallback(object in_cookie, AkCallbackType in_type, AkCallbackInfo in_info)
        {
            EffectAudioSourceItem item = in_cookie as EffectAudioSourceItem;

            if (in_type == AkCallbackType.AK_EndOfEvent)
            {
                item.playingId = string.Empty;

                if (this == WwiseSoundMan.Instance.BGEffectAudioItem)
                    WwiseSoundMan.Instance.BGEffectAudioItem = null;
            }
        }

        public void Play(string audio_name)
        {
            //if (audio_name == EntryPoint.Instance.WwiseSoundConfigParams.WwiseBGEffectSound)
            //    WwiseSoundMan.Instance.BGEffectAudioItem = this;

            audioName = audio_name;
            switchGroup = string.Empty;
            switchState = string.Empty;
            uint id = AkSoundEngine.PostEvent(audioName, attachedObj, (uint)AkCallbackType.AK_EndOfEvent, BgmEventCallback, this);
            playingId = (id == 0 ? string.Empty : id.ToString());
        }

        public void PlayWithSwitch(string audio_name, string switch_group, string switch_state)
        {
            switchGroup = switch_group;
            switchState = switch_state;

            if (!string.IsNullOrEmpty(switchGroup) && !string.IsNullOrEmpty(switchState))
                AkSoundEngine.SetSwitch(switch_group, switchState, attachedObj);

            if (audio_name != audioName)
            {
                // #if LOG_WWISE && UNITY_EDITOR
                //                 Common.HobaDebuger.LogWarningFormat("PostEvent: {0}", audio_name);
                // #endif
                audioName = audio_name;
                uint id = AkSoundEngine.PostEvent(audioName, attachedObj, (uint)AkCallbackType.AK_EndOfEvent, BgmEventCallback, this);
                playingId = (id == 0 ? string.Empty : id.ToString());
            }
        }

        public void Stop()
        {
            AkSoundEngine.StopAll(attachedObj);
            playingId = string.Empty;

            if (this == WwiseSoundMan.Instance.BGEffectAudioItem)
                WwiseSoundMan.Instance.BGEffectAudioItem = null;

            audioName = string.Empty;
            switchGroup = string.Empty;
            switchState = string.Empty;
        }
    };

    private enum AudioType
    {
        S2D,        //2d sound
        S3D,        //3d sound
        AttS3D, //attached 3d sound
    };

    private enum PlayState
    {
        Resume,
        Pause,
        Stop,
    };

    private AkAudioListener _AudioListener; //听筒 
    private Transform _ListenerRoot = null; //听筒挂载点 

    private Vector3 ListenerPosition
    {
        get
        {
            if (_AudioListener == null)
                return Vector3.zero;
            return _AudioListener.transform.position;
        }
    }

    private bool _IsInWorld = false;

    private bool _IsInGameWorld = false;

    private bool _IsLoadingShow = false;

    public bool CanPlayEffect
    {
        get { return (_IsInWorld && !_IsLoadingShow) || !_IsInGameWorld; }
    }

    public float _Volume = 1.0f; ///0 - 100
    ///
    private float _LastTime = 0;

    //BGM相关  BackgroundMusic
    private CSoundEntry _BgmSoundEntry = new CSoundEntry();

    //Environment相关 EnvironmentMusic
    private CSoundEntry _EnvironmentSoundEntry = new CSoundEntry();

    //Weather相关 WeatherMusic
    private CSoundEntry _WeatherSoundEntry = new CSoundEntry();

    //SFX
    private List<EffectAudioSourceItem> _effect3DAudioSourceList = new List<EffectAudioSourceItem>();
    private List<EffectAudioSourceItem> _attachedEffect3DAudioSourceList = new List<EffectAudioSourceItem>();
    private List<EffectAudioSourceItem> _effect2DAudioSourceList = new List<EffectAudioSourceItem>();
    private List<EffectAudioSourceItem> _static3DAudioSourceList = new List<EffectAudioSourceItem>();
    private List<EffectAudioSourceItem> _footStep3DAudioSourceList = new List<EffectAudioSourceItem>();
    private List<EffectAudioSourceItem> _npcVoice3DAudioSourceList = new List<EffectAudioSourceItem>();
    private List<EffectAudioSourceItem> _npcShout3DAndioSourceList = new List<EffectAudioSourceItem>();
    private List<EffectAudioSourceItem> _heartbeat2DAudioSourceList = new List<EffectAudioSourceItem>();

    // 加载的bank
    //private List<AkBank> _SoundBankList = new List<AkBank>();

    // 音效音乐开关
    private bool _IsEffectAudioEnabled = true;

    public float Volume
    {
        get { return _Volume; }
    }

    public float BGMSysVolume { get; set; }
    public float BGMVolume { get; set; }
    public float BGMFinalVolume { get { return BGMVolume * BGMSysVolume; } }

    public float EffectSysVolume { get; set; }
    public float EffectVolume { get; set; }
    private float EffectFinalVolume { get { return EffectSysVolume * EffectVolume; } }

    public float CutsceneSysVolume { get; set; }
    public float CutsceneVolume { get; set; }
    private float CutsceneFinalVolume { get { return CutsceneSysVolume * CutsceneVolume; } }

    public float UISysVolume { get; set; }
    public float UIVolume { get; set; }
    private float UIFinalVolume { get { return UISysVolume * UIVolume; } }

    public bool IsBGMEnabled { get { return _BgmSoundEntry.IsEnabled; } }
    public bool IsEffectAudioEnabled { get { return _IsEffectAudioEnabled; } }

    public void Reset()
    {
        _IsLoadingShow = false;

        _BgmSoundEntry.Reset();

        _EnvironmentSoundEntry.Reset();

        _WeatherSoundEntry.Reset();


        List<EffectAudioSourceItem>[] effectSoundListArray = { 
                                                                 _effect3DAudioSourceList,
                                                                 _attachedEffect3DAudioSourceList,
                                                                 _effect2DAudioSourceList,
                                                                 _footStep3DAudioSourceList,
                                                                 _static3DAudioSourceList,                                                                
                                                                 _npcVoice3DAudioSourceList,
                                                                 _npcShout3DAndioSourceList,
                                                                _heartbeat2DAudioSourceList };
        for (int n = 0; n < effectSoundListArray.Length; ++n)
        {
            List<EffectAudioSourceItem> soundList = effectSoundListArray[n];
            int nCount = soundList.Count;
            for (int i = 0; i < nCount; ++i)
            {
                EffectAudioSourceItem item = soundList[i];
                if (item.attachedObj != null)
                {
                    item.Stop();
                    GameObject.Destroy(item.attachedObj);
                    item.attachedObj = null;
                }
                item.Reset();
            }
            soundList.Clear();
        }

        CheckAudioListener();
    }

    //检查听筒 
    private void CheckAudioListener()
    {
        if (_AudioListener == null)
        {
            GameObject go = new GameObject("AkListener");
            _AudioListener = go.AddComponent<AkAudioListener>();

            if (_ListenerRoot == null)
            {
                _ListenerRoot = transform;
            }
            go.transform.SetParent(_ListenerRoot, false);
        }
    }

    public void EnableBackgroundMusic(bool bOn)
    {
        _BgmSoundEntry.IsEnabled = bOn;
        UpdateAudioState(_BgmSoundEntry.Item, bOn ? PlayState.Resume : PlayState.Pause, true);

        _EnvironmentSoundEntry.IsEnabled = bOn;
        UpdateAudioState(_EnvironmentSoundEntry.Item, bOn ? PlayState.Resume : PlayState.Pause, true);

        _WeatherSoundEntry.IsEnabled = bOn;
        UpdateAudioState(_WeatherSoundEntry.Item, bOn ? PlayState.Resume : PlayState.Pause, true);
    }

    public void EnableEffectAudio(bool bOn)
    {
        _IsEffectAudioEnabled = bOn;

        if (_IsEffectAudioEnabled)
        {
            for (var i = 0; i < _effect3DAudioSourceList.Count; ++i)
            {
                UpdateAudioState(_effect3DAudioSourceList[i], PlayState.Stop, false);
            }
            _effect3DAudioSourceList.Clear();

            for (var i = 0; i < _attachedEffect3DAudioSourceList.Count; ++i)
            {
                UpdateAudioState(_attachedEffect3DAudioSourceList[i], PlayState.Stop, false);
            }
            _attachedEffect3DAudioSourceList.Clear();

            for (var i = 0; i < _effect2DAudioSourceList.Count; ++i)
            {
                UpdateAudioState(_effect2DAudioSourceList[i], PlayState.Stop, false);
            }
            _effect2DAudioSourceList.Clear();

            for (var i = 0; i < _static3DAudioSourceList.Count; ++i)
            {
                UpdateAudioState(_static3DAudioSourceList[i], PlayState.Stop, false);
            }
            _static3DAudioSourceList.Clear();


            for (var i = 0; i < _footStep3DAudioSourceList.Count; ++i)
            {
                UpdateAudioState(_footStep3DAudioSourceList[i], PlayState.Stop, false);
            }
            _footStep3DAudioSourceList.Clear();

        }
    }

    private void UpdateAudioState(EffectAudioSourceItem item, PlayState ps, bool useSwitch)
    {
        if (item == null)
            return;

        if (ps == PlayState.Resume)
        {
            if (item.attachedObj != null && !item.IsPlaying())
            {
                if (useSwitch)
                    item.PlayWithSwitch(item.audioName, item.switchGroup, item.switchState);
                else
                    item.Play(item.audioName);
            }
        }
        else if (ps == PlayState.Stop || ps == PlayState.Pause)
        {
            if (item.attachedObj != null)
            {
                item.Stop();
            }
        }
    }

    private void SafeInitGameObject(CSoundEntry soundEntry, string name)
    {
        if (soundEntry.Item.attachedObj == null)
        {
            GameObject bgm_GameObject = new GameObject(name);
            bgm_GameObject.transform.SetParent(AudioItems.transform, false);
            soundEntry.Item.attachedObj = bgm_GameObject;
        }
    }

    private void SafeInitSFX()
    {
        if (_effect3DAudioSourceList.Count == 0)
        {
            for (int i = 0; i < MaxEffect3DChannel; ++i)
            {
                GameObject go = new GameObject(HobaText.Format("effect3d{0}", i));
                go.transform.parent = AudioItems.transform;
                EffectAudioSourceItem m = new EffectAudioSourceItem();
                m.id = i;
                m.attachedObj = go;
                m.priority = 0;
                _effect3DAudioSourceList.Add(m);
            }
        }

        if (_attachedEffect3DAudioSourceList.Count == 0)
        {
            for (int i = 0; i < MaxAttachedEffect3DChannel; ++i)
            {
                GameObject go = new GameObject(HobaText.Format("attachedeffect3d{0}", i));
                go.transform.parent = AudioItems.transform;
                EffectAudioSourceItem m = new EffectAudioSourceItem();
                m.id = i;
                m.attachedObj = go;
                m.priority = 0;
                _attachedEffect3DAudioSourceList.Add(m);
            }
        }

        if (_effect2DAudioSourceList.Count == 0)
        {
            for (int i = 0; i < MaxEffect2DChannel; ++i)
            {
                GameObject go = new GameObject(HobaText.Format("effect2d{0}", i));
                go.transform.parent = AudioItems.transform;
                EffectAudioSourceItem m = new EffectAudioSourceItem();
                m.id = i;
                m.attachedObj = go;
                m.priority = 0;
                _effect2DAudioSourceList.Add(m);
            }
        }

        if (_static3DAudioSourceList.Count == 0)
        {
            for (int i = 0; i < MaxStaticEffectChannel; ++i)
            {
                GameObject go = new GameObject(HobaText.Format("static{0}", i));
                go.transform.parent = AudioItems.transform;
                EffectAudioSourceItem m = new EffectAudioSourceItem();
                m.id = i;
                m.attachedObj = go;
                m.priority = 0;
                _static3DAudioSourceList.Add(m);
            }
        }

        if (_footStep3DAudioSourceList.Count == 0)
        {
            for (int i = 0; i < MaxFootStepEffectChannel; ++i)
            {
                GameObject go = new GameObject(string.Format("footstep{0}", i));
                go.transform.parent = AudioItems.transform;
                EffectAudioSourceItem m = new EffectAudioSourceItem();
                m.id = i;
                m.attachedObj = go;
                m.priority = 0;
                _footStep3DAudioSourceList.Add(m);
            }
        }

        if (_npcVoice3DAudioSourceList.Count == 0)
        {
            for (int i = 0; i < MaxNpcVoiceEffectChannel; ++i)
            {
                GameObject go = new GameObject(string.Format("npcvoice3d{0}", i));
                go.transform.parent = AudioItems.transform;
                EffectAudioSourceItem m = new EffectAudioSourceItem();
                m.id = i;
                m.attachedObj = go;
                m.priority = 0;
                _npcVoice3DAudioSourceList.Add(m);
            }
        }

        if (_npcShout3DAndioSourceList.Count == 0)
        {
            for (int i = 0; i < MaxNpcShoutEffectChannel; ++i)
            {
                GameObject go = new GameObject(string.Format("npcshout3d{0}", i));
                go.transform.parent = AudioItems.transform;
                EffectAudioSourceItem m = new EffectAudioSourceItem();
                m.id = i;
                m.attachedObj = go;
                m.priority = 0;
                _npcShout3DAndioSourceList.Add(m);
            }
        }

        if (_heartbeat2DAudioSourceList.Count == 0)
        {
            for (int i = 0; i < MaxHeartBeatEffectChannel; ++i)
            {
                GameObject go = new GameObject(string.Format("heartbeat{0}", i));
                go.transform.parent = AudioItems.transform;
                EffectAudioSourceItem m = new EffectAudioSourceItem();
                m.id = i;
                m.attachedObj = go;
                m.priority = 0;
                _heartbeat2DAudioSourceList.Add(m);
            }
        }
    }


    public bool PlayBackgroundMusic(string audioName, float fadeInTime)
    {
        string eventName = string.Empty;
        string switchGroup = string.Empty;
        string switchState = string.Empty;

        if (CUnityUtil.IsWwiseAudioName(audioName))
        {
            CUnityUtil.GetWwiseAudioName(audioName, out eventName, out switchGroup, out switchState);
        }
        else
        {
            return false;
        }

#if LOG_WWISE && UNITY_EDITOR
        Common.HobaDebuger.LogWarningFormat("[UnityAudio] Playing [Wwise] Event: {0} SwitchGroup: {1} SwitchState: {2}", 
            eventName, switchGroup, switchState);    
#endif

        if (_BgmSoundEntry.IsEnabled)
        {
            CheckAudioListener();

            SafeInitGameObject(_BgmSoundEntry, "background");

            LoadAndPlayMusic(_BgmSoundEntry, eventName, switchGroup, switchState);
        }
        return true;
    }

    public bool PlayEnvironmentMusic(string audioName, float fadeInTime)
    {
        string eventName = string.Empty;
        string switchGroup = string.Empty;
        string switchState = string.Empty;

        if (CUnityUtil.IsWwiseAudioName(audioName))
        {
            CUnityUtil.GetWwiseAudioName(audioName, out eventName, out switchGroup, out switchState);
        }
        else
        {
            return false;
        }

#if LOG_WWISE && UNITY_EDITOR
        Common.HobaDebuger.LogWarningFormat("[UnityAudio] Playing [Wwise] Event: {0} SwitchGroup: {1} SwitchState: {2}",
            eventName, switchGroup, switchState);
#endif

        if (_EnvironmentSoundEntry.IsEnabled)
        {
            CheckAudioListener();

            SafeInitGameObject(_EnvironmentSoundEntry, "environment");

            LoadAndPlayMusic(_EnvironmentSoundEntry, eventName, switchGroup, switchState);
        }
        return true;
    }

    public bool PlayWeatherMusic(string audioName, float fadeInTime)
    {
        string eventName = string.Empty;
        string switchGroup = string.Empty;
        string switchState = string.Empty;

        if (CUnityUtil.IsWwiseAudioName(audioName))
        {
            CUnityUtil.GetWwiseAudioName(audioName, out eventName, out switchGroup, out switchState);
        }
        else
        {
            return false;
        }

#if false //LOG_WWISE && UNITY_EDITOR
        Common.HobaDebuger.LogWarningFormat("[UnityAudio] Playing [Wwise] Event: {0} SwitchGroup: {1} SwitchState: {2}",
            eventName, switchGroup, switchState); 
#endif

        if (_WeatherSoundEntry.IsEnabled)
        {
            CheckAudioListener();

            SafeInitGameObject(_WeatherSoundEntry, "weather");

            LoadAndPlayMusic(_WeatherSoundEntry, eventName, switchGroup, switchState);
        }
        return true;
    }

    private void LoadAndPlayMusic(CSoundEntry soundEntry, string eventName, string switchGroup, string switchState)
    {
        if (eventName != soundEntry.Item.audioName || switchGroup != soundEntry.Item.switchGroup || switchState != soundEntry.Item.switchState)
        {
            if (string.IsNullOrEmpty(eventName))
            {
                soundEntry.Item.Stop();
            }
            else
            {
                if (eventName != soundEntry.Item.audioName)
                {
                    soundEntry.Item.Stop();
                }
                soundEntry.Item.PlayWithSwitch(eventName, switchGroup, switchState);
            }
        }
    }

    public void Stop3DAudio(string audioName, string audio_source)
    {
        if (string.IsNullOrEmpty(audioName))
            return;

        string eventName = Util.FixPathString(audioName);

        for (int i = 0; i < _effect3DAudioSourceList.Count; ++i)
        {
            EffectAudioSourceItem item = _effect3DAudioSourceList[i];
            if (item.attachedObj != null && (string.IsNullOrEmpty(audio_source) || item.playingId == audio_source))
            {
                if (eventName == item.audioName)
                {
                    UpdateAudioState(item, PlayState.Stop, item.HaveSwitch());
                }
            }
        }

        for (int i = 0; i < _attachedEffect3DAudioSourceList.Count; ++i)
        {
            EffectAudioSourceItem item = _attachedEffect3DAudioSourceList[i];
            if (item.attachedObj != null && (string.IsNullOrEmpty(audio_source) || item.playingId == audio_source))
            {
                if (eventName == item.audioName)
                {
                    UpdateAudioState(item, PlayState.Stop, item.HaveSwitch());
                }
            }
        }
    }

    public void Stop2DAudio(string audioName, string audio_source)
    {
        if (string.IsNullOrEmpty(audioName))
            return;

        string eventName = Util.FixPathString(audioName);

        for (int i = 0; i < _effect2DAudioSourceList.Count; ++i)
        {
            EffectAudioSourceItem item = _effect2DAudioSourceList[i];
            if (item.attachedObj != null && (string.IsNullOrEmpty(audio_source) || item.playingId == audio_source))
            {
                if (eventName == item.audioName)
                {
                    UpdateAudioState(item, PlayState.Stop, false);
                }
            }
        }
    }

    //static audio
    public string PlayStatic3DAudio(string audioName, Vector3 pos, int sortId = 0, bool isLoop = false)
    {
        string eventName = audioName;

#if false //LOG_WWISE && UNITY_EDITOR
        Common.HobaDebuger.LogWarningFormat("[UnityAudio] Playing [Wwise] Event: {0}", eventName);
#endif

        EffectAudioSourceItem item = _PlaySFX(eventName, AudioType.S3D, null, pos, sortId, EffectSoundType.Static3D);
        return item != null ? item.playingId : string.Empty;
    }

    private void StopStatic3DAudio(string audioName, string audio_source)
    {
        if (string.IsNullOrEmpty(audioName))
            return;

        string eventName = audioName;

        for (int i = 0; i < _static3DAudioSourceList.Count; ++i)
        {
            EffectAudioSourceItem item = _static3DAudioSourceList[i];
            if (item.attachedObj != null && (string.IsNullOrEmpty(audio_source) || item.playingId == audio_source))
            {
                if (eventName == item.audioName)
                {
                    UpdateAudioState(item, PlayState.Pause, item.HaveSwitch());
                    break;
                }
            }
        }
    }

    private bool IsStaticAudioPlaying(string audioName)
    {
        for (int i = 0; i < _static3DAudioSourceList.Count; ++i)
        {
            EffectAudioSourceItem item = _static3DAudioSourceList[i];
            if (item.audioName == audioName && item.IsPlaying())
                return true;
        }
        return false;
    }

    public string Play3DAudio(string audioName, Vector3 pos, int sortId = 0, bool isLoop = false)
    {
        if (Main.HostPalyer != null && Util.SquareDistanceH(Main.HostPalyer.position, pos) > MaxEffectAudioDistSqr)
        {
            //Common.HobaDebuger.LogErrorFormat("Play3DAudio: {0}", audioName);
            return string.Empty;
        }

        string eventName = Util.FixPathString(audioName);

#if LOG_WWISE && UNITY_EDITOR
        Common.HobaDebuger.LogWarningFormat("[UnityAudio] Playing [Wwise] Event: {0}", eventName);
#endif

        EffectAudioSourceItem item = _PlaySFX(eventName, AudioType.S3D, null, pos, sortId, EffectSoundType.Effect3D);
        return item != null ? item.playingId : string.Empty;
    }

    public string PlayAttached3DAudio(string audioName, Transform trans, int sortId = 0, bool isLoop = false)
    {
        string eventName = Util.FixPathString(audioName);

#if LOG_WWISE && UNITY_EDITOR
        Common.HobaDebuger.LogWarningFormat("[UnityAudio] Playing [Wwise] Event: {0}", eventName);
#endif

        EffectAudioSourceItem item = _PlaySFX(eventName, AudioType.AttS3D, trans, Vector3.zero, sortId, EffectSoundType.AttachEffect3D);
        return item != null ? item.playingId : string.Empty;
    }

    public string Play3DNpcVoice(string audioName, Vector3 pos, int sortId = 0, bool isLoop = false)
    {
        string eventName = Util.FixPathString(audioName);

#if LOG_WWISE && UNITY_EDITOR
        Common.HobaDebuger.LogWarningFormat("[UnityAudio] Playing [Wwise] Event: {0}", eventName);
#endif

        if (string.IsNullOrEmpty(eventName))
        {
            for (var i = 0; i < _npcVoice3DAudioSourceList.Count; ++i)
            {
                UpdateAudioState(_npcVoice3DAudioSourceList[i], PlayState.Stop, false);
            }
            return string.Empty;
        }

        StopAllNpcShout();

        EffectAudioSourceItem item = _PlaySFX(eventName, AudioType.S3D, null, pos, sortId, EffectSoundType.NpcVoice3D);
        return item != null ? item.playingId : string.Empty;
    }

    public string Play3DNpcShout(string audioName, Vector3 pos, int sortId = 0, bool isLoop = false)
    {
        if (IsNpcVoicePlaying())
            return string.Empty;

        string eventName = Util.FixPathString(audioName);

#if LOG_WWISE && UNITY_EDITOR
        Common.HobaDebuger.LogWarningFormat("[UnityAudio] Playing [Wwise] Event: {0}", eventName);
#endif

//         if (string.IsNullOrEmpty(eventName))
//         {
//             for (var i = 0; i < _npcShout3DAndioSourceList.Count; ++i)
//             {
//                 UpdateAudioState(_npcShout3DAndioSourceList[i], PlayState.Stop, false);
//             }
//             return string.Empty;
//         }

        EffectAudioSourceItem item = _PlaySFX(eventName, AudioType.S3D, null, pos, sortId, EffectSoundType.NpcShout3D);
        return item != null ? item.playingId : string.Empty;
    }

    public bool IsNpcVoicePlaying()
    {
        bool bPlaying = false;
        for (var i = 0; i < _npcVoice3DAudioSourceList.Count; ++i)
        {
            if (_npcVoice3DAudioSourceList[i] != null && _npcVoice3DAudioSourceList[i].IsPlaying())
            {
                bPlaying = true;
                break;
            }
        }
        return bPlaying;
    }

    public void StopAllNpcShout()
    {
        for (var i = 0; i < _npcShout3DAndioSourceList.Count; ++i)
        {
            UpdateAudioState(_npcShout3DAndioSourceList[i], PlayState.Stop, false);
        }
    }

    public string Play2DAudio(string audioName, int sortId = 0, bool isLoop = false)
    {
        string eventName = Util.FixPathString(audioName);

#if LOG_WWISE && UNITY_EDITOR
        Common.HobaDebuger.LogWarningFormat("[UnityAudio] Playing [Wwise] Event: {0}", eventName);
#endif

        EffectAudioSourceItem item = _PlaySFX(eventName, AudioType.S2D, null, Vector3.zero, sortId, EffectSoundType.Effect2D);
        return item != null ? item.playingId : string.Empty;
    }

    public string Play2DHeartBeat(string audioName, int sortId = 0, bool isLoop = false)
    {
        string eventName = Util.FixPathString(audioName);

#if LOG_WWISE && UNITY_EDITOR
        Common.HobaDebuger.LogWarningFormat("[UnityAudio] Playing [Wwise] Event: {0}", eventName);
#endif

        if (string.IsNullOrEmpty(eventName))
        {
            for (var i = 0; i < _heartbeat2DAudioSourceList.Count; ++i)
            {
                UpdateAudioState(_heartbeat2DAudioSourceList[i], PlayState.Stop, false);
            }
            return string.Empty;
        }

        EffectAudioSourceItem item = _PlaySFX(eventName, AudioType.S2D, null, Vector3.zero, sortId, EffectSoundType.Heartbeat2D);
        return item != null ? item.playingId : string.Empty;

    }

    public string PlayFootStepAudio(string audioName, Vector3 pos, int sortId = 0, bool isLoop = false)
    {
        string eventName = audioName;

#if LOG_WWISE && UNITY_EDITOR
        Common.HobaDebuger.LogWarningFormat("[UnityAudio] Playing [Wwise] Event: {0}", eventName);
#endif

        EffectAudioSourceItem item = _PlaySFX(eventName, AudioType.S3D, null, pos, sortId, EffectSoundType.FootStep3D);
        return item != null ? item.playingId : string.Empty;
    }

    private EffectAudioSourceItem _PlaySFX(string audioName, AudioType aType, Transform point, Vector3 pos, int sortId, EffectSoundType type)
    {
        if (string.IsNullOrEmpty(audioName)) return null;

        if (_IsEffectAudioEnabled)
        {
            CheckAudioListener();

            SafeInitSFX();

            EffectAudioSourceItem item_arranged = GetUsableItem(aType, sortId, pos, type);

            if (item_arranged != null)
            {
                item_arranged.audioName = audioName;
                if (item_arranged.id != -1 && item_arranged.attachedObj != null)
                {
                    if (type == EffectSoundType.Effect3D || type == EffectSoundType.AttachEffect3D || type == EffectSoundType.Effect2D || type == EffectSoundType.FootStep3D)
                    {
                        if (item_arranged.IsPlaying())
                            item_arranged.Stop();
                    }
                    else
                    {
                        item_arranged.Stop();
                    }

                    GetProperSettingOfItem(item_arranged, aType, audioName, sortId);

                    if (aType == AudioType.AttS3D)
                    {
                        item_arranged.attachedPos = point;
                        item_arranged.attachedObj.transform.position = point.position;
                    }
                    if (aType == AudioType.S3D)
                    {
                        item_arranged.attachedObj.transform.position = pos;
                    }

                    if (item_arranged.attachedObj != null)
                    {
                        item_arranged.Play(audioName);
                    }

                    return item_arranged;
                }
                }
            }

        return null;
    }

    private int DistanceComparison(Vector3 pos1, Vector3 pos2)
    {
        float dist1 = Util.SquareDistance(ListenerPosition, pos1);
        float dist2 = Util.SquareDistance(ListenerPosition, pos2);
        if (Math.Abs(dist1 - dist2) < 0.000001f)
            return 0;

        return dist1 < dist2 ? 1 : -1;
    }

    //3D 音效按优先级和距离排序 
    private int AudioSourceComparison(EffectAudioSourceItem a, EffectAudioSourceItem b)
    {
        // 是否在播放中 
        if (!a.IsPlaying() && b.IsPlaying())
            return -1;
        else if (a.IsPlaying() && !b.IsPlaying())
            return 1;
        else if (a.IsPlaying() && b.IsPlaying())
        {
            // 按照播放优先级, 小在前 
            if (a.priority != b.priority)
                return a.priority > b.priority ? 1 : -1;

            // 按照距离远近 
            return DistanceComparison(a.attachedObj.transform.position, b.attachedObj.transform.position);
        }

        return 0;
    }

    private int AudioSourceComparison2D(EffectAudioSourceItem a, EffectAudioSourceItem b)
    {
        // 是否在播放中 
        if (!a.IsPlaying() && b.IsPlaying())
            return -1;
        else if (a.IsPlaying() && !b.IsPlaying())
            return 1;
        else if (a.IsPlaying() && b.IsPlaying())
        {
            // 按照播放优先级, 小在前 
            if (a.priority != b.priority)
                return a.priority > b.priority ? 1 : -1;

            // 按照距离远近 
            return 0;
        }

        return 0;
    }

    private EffectAudioSourceItem GetFirstItem(List<EffectAudioSourceItem> soundList, bool b3D)
    {
        if (soundList == null || soundList.Count <= 0)
            return null;

        //选一个最小项
        EffectAudioSourceItem item = soundList[0];
        if (b3D)
        {
            for (int i = 1; i < soundList.Count; ++i)
            {
                if (AudioSourceComparison(item, soundList[i]) > 0)
                    item = soundList[i];
            }
        }
        else
        {
            for (int i = 1; i < soundList.Count; ++i)
            {
                if (AudioSourceComparison2D(item, soundList[i]) > 0)
                    item = soundList[i];
            }
        }

        return item;
    }

    private EffectAudioSourceItem GetUsableItem(AudioType aType, int priority, Vector3 pos, EffectSoundType type)
    {
        List<EffectAudioSourceItem> soundList = null;
        bool bSkipCompare = false;
        switch (type)
        {
            case EffectSoundType.Effect3D:
                soundList = _effect3DAudioSourceList;
                break;
            case EffectSoundType.AttachEffect3D:
                soundList = _attachedEffect3DAudioSourceList;
                break;
            case EffectSoundType.Effect2D:
                soundList = _effect2DAudioSourceList;
                break;
            case EffectSoundType.Static3D:
                soundList = _static3DAudioSourceList;
                break;
            case EffectSoundType.FootStep3D:
                soundList = _footStep3DAudioSourceList;
                break;
            case EffectSoundType.NpcVoice3D:
                soundList = _npcVoice3DAudioSourceList;
                bSkipCompare = true;
                break;
            case EffectSoundType.NpcShout3D:
                soundList = _npcShout3DAndioSourceList;
                bSkipCompare = true;
                break;
            case EffectSoundType.Heartbeat2D:
                soundList = _heartbeat2DAudioSourceList;
                bSkipCompare = true;
                break;
            default:
                break;
        }

        if (soundList != null && soundList.Count > 0)
        {
            EffectAudioSourceItem item = GetFirstItem(soundList, aType != AudioType.S2D);
            if (item != null && item.attachedObj != null)
            {
                if (bSkipCompare)
                {
                    return item;
                }
                else if (item.IsPlaying())
                {
                    if (item.priority > priority)
                    {
                        return null;
                    }
                    else if (item.priority == priority && aType != AudioType.S2D)
                    {
                        if (DistanceComparison(item.attachedObj.transform.position, pos) > 0)
                            return null;
                    }
                }
            }
            return item;
        }

        return null;
    }

    private void UpdateSoundPos()
    {
        int nCount = _attachedEffect3DAudioSourceList.Count;
        for (int i = 0; i < nCount; ++i)
        {
            EffectAudioSourceItem item = _attachedEffect3DAudioSourceList[i];
            if (item.attachedObj != null && null != item.attachedPos)
            {
                item.attachedObj.transform.position = item.attachedPos.position;
            }
        }
    }

    private void UpdateBGEffect()
    {
        if (BGEffectAudioItem != null && BGEffectAudioItem.attachedObj != null)
        {
            float dist = Util.DistanceH(transform.position, BGEffectAudioItem.attachedObj.transform.position);
            ChangeBgEffect(dist);
        }
    }

    public IEnumerable Init()
    {
        if (BanksLoaded == null)
        {
            BanksLoaded = new GameObject("Banks");
            BanksLoaded.transform.parent = this.transform;
        }

        if (AudioItems == null)
        {
            AudioItems = new GameObject("AudioItems");
            AudioItems.transform.parent = this.transform;
        }

        if (gameObject.GetComponent<AkInitializer>() == null)
            gameObject.AddComponent<AkInitializer>();

        if (gameObject.GetComponent<AkTerminator>() == null)
            gameObject.AddComponent<AkTerminator>();
        yield return null;

        CheckAudioListener();
        yield return null;

        var listAlways = EntryPoint.Instance.WwiseBankConfigParams.AlwaysLoadingBankList;
        foreach (var v in listAlways)
        {
            CWwiseBankMan.Instance.LoadBank(v.Name, v.Localized);
            yield return null;
        }

        var listNonMap = EntryPoint.Instance.WwiseBankConfigParams.NonMapBankList;
        foreach (var v in listNonMap)
        {
            CWwiseBankMan.Instance.LoadBank(v.Name, v.Localized);
            yield return null;
        }
    }

    public void SetLanguage(string language)
    {
        AkInitializer initializer = gameObject.GetComponent<AkInitializer>();
        if (initializer != null)
            initializer.language = language;
    }

    public void Tick(float dt)
    {
        if (Main.HostPalyer != null)
            transform.position = Main.HostPalyer.transform.position;
        else
            transform.position = Vector3.zero;

        if (Main.Main3DCamera != null)
            transform.forward = Main.Main3DCamera.transform.forward;
        else
            transform.forward = Vector3.forward;

        UpdateSoundPos();
        UpdateBGEffect();

        float now = Time.time;
        if (now - _LastTime > 2)
        {
            _LastTime = now;

            //设置位置，方向
            UpdateStaticObjectSound();
        }
    }

    private void UpdateStaticObjectSound()
    {
        if (Main.HostPalyer == null)
            return;

        var soundList = ScenesManager.Instance.GetStaticObjectAudioList();
        //ScenesObjectManager.Instance._LoadedStaticObjectAudioList;
        var itor = soundList.GetEnumerator();
        var hpPos = Main.HostPalyer.position;
        while (itor.MoveNext())
        {
            var soundObj = itor.Current.Value;
            Vector3 pos = soundObj.transform.position;

            if (Util.SquareDistanceH(hpPos, pos) < MaxStaticObjectDistSqr)
            {
                if (!IsStaticAudioPlaying(soundObj.AudioName))
                {
                    PlayStatic3DAudio(soundObj.AudioName, pos);
                }
            }
            else
            {
                if (IsStaticAudioPlaying(soundObj.AudioName))
                {
                    StopStatic3DAudio(soundObj.AudioName, "");
                }
            }
        }
        itor.Dispose();
    }

    /*
    private void UpdateWeatherSound()
    {
        if (Main.HostPalyer == null)
            return;

        if (LuaScriptMgr.Instance.IsInNoWeatherRegion())
        {
            PlayWeatherMusic("", 0);
            return;
        }

        WeatherAudioType type = WeatherAudioType.None;
        int effectFlag = ScenesManager.Instance.CurrentEffectFlag;
        ScenesManager.WeatherType weatherType = ScenesManager.Instance.CurrentWeather;
        if (weatherType == ScenesManager.WeatherType.Rain)
        {
            type = WeatherAudioType.Rain;
        }
        else if(weatherType == ScenesManager.WeatherType.Snow)
        {
            type = WeatherAudioType.Snow;
        }
        else if(effectFlag >= 0)
        {
            float currentLerpValue = ScenesManager.Instance.GetLerpValue();
            float time = effectFlag * 6 + 6 * currentLerpValue;
            SetTimeOfDay(time);

            if (effectFlag == 3)
                type = WeatherAudioType.Night;
            else
                type = WeatherAudioType.Day;
        }

        string sound = EntryPoint.Instance.WwiseSoundConfigParams.GetWeatherAudio(type);
        PlayWeatherMusic(sound, 0);
    }
     * */


    public AkAudioListener GetAudioListener()
    {
        CheckAudioListener();
        return _AudioListener;
    }

    private void GetProperSettingOfItem(EffectAudioSourceItem item, AudioType aType, string audioName, int priority = 0)
    {
        if (item != null && !string.IsNullOrEmpty(audioName))
        {
            item.audioName = audioName;
            item.priority = priority;
            item.attachedPos = null;

            if (item.attachedObj != null)
                item.attachedObj.transform.localPosition = Vector3.zero;
        }
    }

#if !HOT_UPDATE             //热更的bank由CWwiseBankMan加载

    public void LoadSoundBank(string bankName)
    {
        for(int i = 0; i < _SoundBankList.Count; ++i)       //不重复加载
        {
            if (_SoundBankList[i] != null &&
                _SoundBankList[i].name == bankName)
                return;
        }

        GameObject go = new GameObject(bankName);

        AkBank akBank = go.AddComponent<AkBank>();
        akBank.bankName = bankName;

        go.transform.parent = BanksLoaded.transform;

        _SoundBankList.Add(akBank);
    }

    public void UnloadSoundBank(string bankName)
    {
        for (int i = 0; i < _SoundBankList.Count; ++i)
        {
            if (_SoundBankList[i] != null &&
                _SoundBankList[i].gameObject != null &&
                _SoundBankList[i].name == bankName)
            {
                GameObject.Destroy(_SoundBankList[i].gameObject);
                _SoundBankList.RemoveAt(i);
                break;
            }
        }
    }

    public void UnloadAllSoundBanks()
    {
        for (int i = 0; i < _SoundBankList.Count; ++i)
        {
            if (_SoundBankList[i] != null && _SoundBankList[i].gameObject != null)
                GameObject.Destroy(_SoundBankList[i].gameObject);
        }
        _SoundBankList.Clear();
    }

#endif


    public void ChangeBGMVolume(bool immediately = false)
    {
        uint nBGMVolume = EntryPoint.Instance.WwiseSoundConfigParams.WwiseBGMVolume;
        float volume = BGMFinalVolume * 100.0f;
        AkSoundEngine.SetRTPCValue(nBGMVolume, volume);
    }

    public void ChangeEffectVolume()
    {
        uint nSFXVolume = EntryPoint.Instance.WwiseSoundConfigParams.WwiseSFXVolume;
        float volume = EffectFinalVolume * 100.0f;
        AkSoundEngine.SetRTPCValue(nSFXVolume, volume);
    }

    public void ChangeCutSceneVolume()
    {
        uint nCutSceneVolume = EntryPoint.Instance.WwiseSoundConfigParams.WwiseCutsceneVolume;
        float volume = CutsceneFinalVolume * 100.0f;
        AkSoundEngine.SetRTPCValue(nCutSceneVolume, volume);
    }

    public void ChangeUIVolume()
    {
        uint nUIVolume = EntryPoint.Instance.WwiseSoundConfigParams.WwiseUIVolume;
        float volume = UIFinalVolume * 100.0f;
        AkSoundEngine.SetRTPCValue(nUIVolume, volume);
    }

    public void ChangeBgEffect(float nValue)
    {
        uint nBGEffect = EntryPoint.Instance.WwiseSoundConfigParams.WwiseBGEffect;
        AkSoundEngine.SetRTPCValue(nBGEffect, nValue);
    }

    public void OnEnterGameWorld()
    {
        _IsInGameWorld = true;

        var listNonMap = EntryPoint.Instance.WwiseBankConfigParams.NonMapBankList;
        for (int i = 0; i < listNonMap.Count; ++i)
        {
            CWwiseBankMan.Instance.UnloadBank(listNonMap[i].Name);
        }
    }

    public void OnLeaveGameWorld()
    {
        _IsInGameWorld = false;

        var enumerator = EntryPoint.Instance.WwiseBankConfigParams.MapBankList.GetEnumerator();
        while (enumerator.MoveNext())
        {
            string name = enumerator.Current.Name;
            CWwiseBankMan.Instance.UnloadBank(name);
        }

        var listNonMap = EntryPoint.Instance.WwiseBankConfigParams.NonMapBankList;
        for (int i = 0; i < listNonMap.Count; ++i)
        {
            CWwiseBankMan.Instance.LoadBank(listNonMap[i].Name, listNonMap[i].Localized);
        }
    }

    private Dictionary<string, bool> BankToLoad = new Dictionary<string, bool>();
    public void OnFinishEnterWorld(string navmeshName)
    {
        _IsInWorld = true;

        BankToLoad.Clear();

        //卸载
        {
            var enumerator = EntryPoint.Instance.WwiseBankConfigParams.MapBankList.GetEnumerator();
            while (enumerator.MoveNext())
            {
                CBankConfigEntry entry = enumerator.Current;

                string navmesh = entry.NavmeshName;
                string name = entry.Name;
                bool localize = entry.Localized;

                if (navmesh == navmeshName)
                {
                    if (!BankToLoad.ContainsKey(name))
                        BankToLoad.Add(name, localize);
                }
                else
                {
                    if (!BankToLoad.ContainsKey(name))
                    {
                        CWwiseBankMan.Instance.UnloadBank(name);
                    }
                }
            }
        }

        //加载
        {
            var enumerator = BankToLoad.GetEnumerator();
            while (enumerator.MoveNext())
            {
                string name = enumerator.Current.Key;
                bool localized = enumerator.Current.Value;
                CWwiseBankMan.Instance.LoadBank(name, localized);
            }
        }
    }

    public void OnWorldRelease()
    {
        _IsInWorld = false;

        List<EffectAudioSourceItem>[] effectSoundListArray = { 
                                                                 _effect3DAudioSourceList,
                                                                 _attachedEffect3DAudioSourceList,
                                                                 _effect2DAudioSourceList,
                                                                 _footStep3DAudioSourceList,
                                                                 _static3DAudioSourceList,                                                                
                                                                 _npcVoice3DAudioSourceList,
                                                                 _npcShout3DAndioSourceList,
                                                                _heartbeat2DAudioSourceList };
        for (int n = 0; n < effectSoundListArray.Length; ++n)
        {
            List<EffectAudioSourceItem> soundList = effectSoundListArray[n];
            int nCount = soundList.Count;
            for (int i = 0; i < nCount; ++i)
            {
                EffectAudioSourceItem item = soundList[i];
                if (item.attachedObj != null)
                    item.Stop();
            }
        }
    }

    public void OnLoadingShow(bool show)
    {
        _IsLoadingShow = show;
    }
}

#endif