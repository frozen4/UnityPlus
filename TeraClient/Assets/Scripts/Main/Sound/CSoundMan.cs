using System;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class CSoundMan : MonoBehaviourSingleton<CSoundMan>, ISoundMan
{
    private const int MaxEffectChannel = 16;
    private const float BGMChangeVolumeTime = 1.5f;

    public class CSoundEntry
    {
        public EffectAudioSourceItem Item = new EffectAudioSourceItem();
        public string NewName = string.Empty;
        public BgmChangePhase CurPhase = BgmChangePhase.NONE;
        public bool IsEnabled = true;
        public bool IsChanging = false;
        public float FadeInTime = 2;

        public void Reset()
        {
            if (Item.audioSource != null)
            {
                Item.audioSource.Stop();
                GameObject.Destroy(Item.audioSource.gameObject);
                Item.audioSource = null;
            }
            Item.Reset();
            NewName = string.Empty;
            FadeInTime = 2;
        }
    }

    public class EffectAudioSourceItem
    {
        public int id = -1;

        public AudioSource audioSource;
        public int priority;
        public string audioName = string.Empty;
        public Transform attachedPos;
        public byte isLoading;

        public void Reset()
        {
            id = -1;
            audioSource = null;
            priority = 0;
            audioName = string.Empty;
            attachedPos = null;
            isLoading = 0;
        }
    };

    public enum BgmChangePhase
    {
        NONE = 0,
        OLD_FADE_OUT = 1,
        NEW_FADE_IN = 2,
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

    private AudioListener _AudioListener;   //听筒
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

    //BGM相关  BackgroundMusic
    private CSoundEntry _BgmSoundEntry = new CSoundEntry();

    //EVM相关 EnvironmentMusic
    private CSoundEntry _EnvironmentSoundEntry = new CSoundEntry();

    //Weather相关 WeatherMusic
    private CSoundEntry _WeatherSoundEntry = new CSoundEntry();

    private Dictionary<string, AudioClip> _ClipCache = new Dictionary<string, AudioClip>();

    //SFX
    private List<EffectAudioSourceItem> _effectAudioSourceList = new List<EffectAudioSourceItem>();

    // 音效音乐开关
    private bool _IsEffectAudioEnabled = true;

    private bool _IsInWorld = false;

    private bool _IsInGameWorld = false;

    private bool _IsLoadingShow = false;

    public bool CanPlayEffect
    {
        get { return (_IsInWorld && !_IsLoadingShow) || !_IsInGameWorld; }
    }

    public float Volume
    {
        get { return AudioListener.volume; }
    }

    public float BGMSysVolume { get; set; }
    public float BGMVolume { get; set; }
    private float BGMFinalVolume { get { return BGMVolume * BGMSysVolume; } }

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
        _BgmSoundEntry.Reset();

        _EnvironmentSoundEntry.Reset();

        _WeatherSoundEntry.Reset();

        {
            int nCount = _effectAudioSourceList.Count;
            for (int i = 0; i < nCount; ++i)
            {
                EffectAudioSourceItem item = _effectAudioSourceList[i];
                if (item.audioSource != null)
                {
                    // if(item.audioSource.isPlaying)
                    item.audioSource.Stop();
                    GameObject.Destroy(item.audioSource.gameObject);
                    item.audioSource = null;
                }
                item.Reset();
            }
            _effectAudioSourceList.Clear();
        }

        _ClipCache.Clear();

        CheckAudioListener();
    }

    public void EnableBackgroundMusic(bool bOn)
    {
        _BgmSoundEntry.IsEnabled = bOn;
        UpdateAudioState(_BgmSoundEntry.Item, bOn ? PlayState.Resume : PlayState.Pause);

        _EnvironmentSoundEntry.IsEnabled = bOn;
        UpdateAudioState(_EnvironmentSoundEntry.Item, bOn ? PlayState.Resume : PlayState.Pause);

        _WeatherSoundEntry.IsEnabled = bOn;
        UpdateAudioState(_WeatherSoundEntry.Item, bOn ? PlayState.Resume : PlayState.Pause);
    }

    public void EnableEffectAudio(bool bOn)
    {
        _IsEffectAudioEnabled = bOn;

        if (!_IsEffectAudioEnabled)
        {
            for (var i = 0; i < _effectAudioSourceList.Count; ++i)
            {
                UpdateAudioState(_effectAudioSourceList[i], PlayState.Stop);
            }
            _effectAudioSourceList.Clear();
        }
    }

    public bool PlayBackgroundMusic(string bgmName, float fadeInTime)
    {
        if (CUnityUtil.IsWwiseAudioName(bgmName))
            return false;

        if (_BgmSoundEntry.IsEnabled)
        {
            CheckAudioListener();

            SafeInitGameObject(_BgmSoundEntry, "background");

            LoadAndPlayMusic(_BgmSoundEntry, bgmName, fadeInTime);
        }

        return true;
    }

    public bool PlayEnvironmentMusic(string envName, float fadeInTime)
    {
        if (CUnityUtil.IsWwiseAudioName(envName))
            return false;

        if (_EnvironmentSoundEntry.IsEnabled)
        {
            CheckAudioListener();

            SafeInitGameObject(_EnvironmentSoundEntry, "environment");

            LoadAndPlayMusic(_EnvironmentSoundEntry, envName, fadeInTime);
        }

        return true;
    }

    public bool PlayWeatherMusic(string envName, float fadeInTime)
    {
        if (CUnityUtil.IsWwiseAudioName(envName))
            return false;

        if (_WeatherSoundEntry.IsEnabled)
        {
            CheckAudioListener();

            SafeInitGameObject(_WeatherSoundEntry, "weather");

            LoadAndPlayMusic(_WeatherSoundEntry, envName, fadeInTime);
        }

        return true;
    }

    private void SafeInitGameObject(CSoundEntry soundEntry, string name)
    {
        if (soundEntry.Item.audioSource == null)
        {
            soundEntry.Item.id = 0;
            GameObject bgm_GameObject = new GameObject(name);
            bgm_GameObject.transform.SetParent(transform, false);
            soundEntry.Item.audioSource = bgm_GameObject.AddComponent<AudioSource>();
        }
    }

    private void LoadAndPlayMusic(CSoundEntry soundEntry, string soundName, float fadeInTime)
    {
        if (!string.IsNullOrEmpty(soundName) && soundName == soundEntry.Item.audioName && soundEntry.CurPhase == BgmChangePhase.OLD_FADE_OUT)
        {
            soundEntry.NewName = soundName;
            soundEntry.CurPhase = BgmChangePhase.NEW_FADE_IN;
            soundEntry.FadeInTime = fadeInTime;
        }
        else if (soundEntry.Item.audioName != soundName)
        {
            soundEntry.NewName = soundName;
            soundEntry.CurPhase = BgmChangePhase.OLD_FADE_OUT;
            soundEntry.FadeInTime = fadeInTime;

            if (!string.IsNullOrEmpty(soundEntry.NewName) && !IsClipCached(soundEntry.NewName))
            {
                Action<UnityEngine.Object> callback = asset =>
                {
                    if (!soundEntry.IsEnabled) return;
                    CacheClipFromLoading(soundEntry.NewName, soundName, asset);
                };
                CAssetBundleManager.AsyncLoadResource(soundEntry.NewName, callback, false);
            }
        }
    }

    public void Stop3DAudio(string audioName, string audio_source)
    {
        if (string.IsNullOrEmpty(audioName)) return;

        //Debug.Log("Stop " + audioName + " " + audio_source);
        for (int i = 0; i < _effectAudioSourceList.Count; ++i)
        {
            EffectAudioSourceItem item = _effectAudioSourceList[i];

            AudioSource audioSource = item.audioSource;
            if (audioSource != null && (string.IsNullOrEmpty(audio_source) || audioSource.name == audio_source))
            {
                if (audioName == item.audioName)
                {
                    if (item.isLoading == 1)
                    {
                        item.isLoading = 0;
                    }
                    else
                    {
                        UpdateAudioState(item, PlayState.Pause);
                    }
                    break;
                }
            }
        }
    }

    public void Stop2DAudio(string audioName, string audio_source)
    {
        if (string.IsNullOrEmpty(audioName)) return;

        for (int i = 0; i < _effectAudioSourceList.Count; ++i)
        {
            EffectAudioSourceItem item = _effectAudioSourceList[i];

            AudioSource audioSource = item.audioSource;
            if (audioSource != null && (string.IsNullOrEmpty(audio_source) || audioSource.name == audio_source))
            {
                if (audioName == item.audioName)
                {
                    if (item.isLoading == 1)
                    {
                        item.isLoading = 0;
                    }
                    else
                    {
                        UpdateAudioState(item, PlayState.Pause);
                    }
                    break;
                }
            }
        }
    }

    public string Play3DAudio(string audioName, Vector3 pos, int sortId = 0, bool isLoop = false)
    {
        if (CUnityUtil.IsWwiseAudioName(audioName))
            return string.Empty;

        EffectAudioSourceItem item = _PlaySFX(audioName, AudioType.S3D, null, pos, sortId, isLoop);
        return item != null && item.audioSource != null ? item.audioSource.name : string.Empty;
    }

    public string PlayAttached3DAudio(string audioName, Transform trans, int sortId = 0, bool isLoop = false)
    {
        if (CUnityUtil.IsWwiseAudioName(audioName))
            return string.Empty;

        EffectAudioSourceItem item = _PlaySFX(audioName, AudioType.AttS3D, trans, Vector3.zero, sortId, isLoop);
        return item != null && item.audioSource != null ? item.audioSource.name : string.Empty;
    }

    public string Play3DNpcVoice(string audioName, Vector3 pos, int sortId = 0, bool isLoop = false)
    {
        return Play3DAudio(audioName, pos, sortId, isLoop);
    }

    public string Play3DNpcShout(string audioName, Vector3 pos, int sortId = 0, bool isLoop = false)
    {
        return Play3DAudio(audioName, pos, sortId, isLoop);
    }

    public string Play2DAudio(string audioName, int sortId = 0, bool isLoop = false)
    {
        if (CUnityUtil.IsWwiseAudioName(audioName))
            return string.Empty;

        EffectAudioSourceItem item = _PlaySFX(audioName, AudioType.S2D, null, Vector3.zero, sortId, isLoop);
        return item != null && item.audioSource != null ? item.audioSource.name : string.Empty;
    }

    public string Play2DHeartBeat(string audioName, int sortId = 0, bool isLoop = false)
    {
        return Play2DAudio(audioName, sortId, isLoop);
    }

    public string PlayFootStepAudio(string audioName, Vector3 pos, int sortId = 0, bool isLoop = false)
    {
        return string.Empty;
    }

    private EffectAudioSourceItem _PlaySFX(string audioName, AudioType aType, Transform point, Vector3 pos, int sortId, bool isLoop)
    {
        if (string.IsNullOrEmpty(audioName)) return null;

        if (_IsEffectAudioEnabled)
        {
            CheckAudioListener();

            SafeInitSFX();

            if (IsClipCached(audioName))
            {
                //PlayAttachAudio(_ClipCache[audioName], false, trans, audioName, sortId, IsLoop);
                EffectAudioSourceItem item = GetUsableItem(aType, sortId, pos);
                if (item != null && item.audioSource != null)
                {
                    AudioClip clip;
                    LoadCache(audioName, out clip);
                    GetProperSettingOfItem(item, aType, clip, audioName, sortId, isLoop);

                    if (aType == AudioType.AttS3D)
                    {
                        item.attachedPos = point;
                    }
                    if (aType == AudioType.S3D)
                    {
                        item.audioSource.transform.position = pos;
                    }

                    item.audioSource.Play();
                    return item;
                }
            }
            else
            {
                EffectAudioSourceItem item_arranged = GetUsableItem(aType, sortId, pos);

                if (item_arranged != null)
                {
                    Action<UnityEngine.Object> callback = asset =>
                    {
                        if (!_IsEffectAudioEnabled || asset == null)
                        {
                            if (item_arranged != null)
                            {
                                item_arranged.isLoading = 0;
                                item_arranged.audioName = "";
                            }
                            return;
                        }

                        if (item_arranged != null)
                        {
                            if (item_arranged.id != -1 && item_arranged.audioSource != null)
                            {
                                if (item_arranged.isLoading == 1 && audioName == item_arranged.audioName)
                                {
                                    //EffectAudioSourceItem item = FindInternalItem(item_arranged);

                                    item_arranged.isLoading = 0;

                                    GetProperSettingOfItem(item_arranged, aType, asset as AudioClip, audioName, sortId, isLoop);

                                    if (aType == AudioType.AttS3D)
                                    {
                                        item_arranged.attachedPos = point;
                                    }
                                    if (aType == AudioType.S3D)
                                    {
                                        item_arranged.audioSource.transform.position = pos;
                                    }

                                    if (item_arranged.audioSource != null)
                                    {
                                        item_arranged.audioSource.Play();
                                    }
                                }
                            }
                            item_arranged = null;
                        }
                    };

                    item_arranged.isLoading = 1;
                    item_arranged.audioName = audioName;
                    //Debug.Log("Ask " + audioName + " " + item_arranged.audioSource.name);
                    CAssetBundleManager.AsyncLoadResource(audioName, callback, false);
                    return item_arranged;
                }
            }
        }

        return null;
    }

    public void ChangeBGMVolume(bool immediately = false)
    {
        ChangeSoundVolume(_BgmSoundEntry, immediately, BGMFinalVolume);
        ChangeSoundVolume(_EnvironmentSoundEntry, immediately, BGMFinalVolume);
        ChangeSoundVolume(_WeatherSoundEntry, immediately, BGMFinalVolume);
    }

    public void ChangeEffectVolume()
    {

    }

    public void ChangeCutSceneVolume()
    {

    }

    public void ChangeUIVolume()
    {

    }

    private void ChangeSoundVolume(CSoundEntry soundEntry, bool immediately, float volume)
    {
        if (soundEntry.Item.audioSource != null)
        {
            if (immediately)
            {
                if (soundEntry.CurPhase == BgmChangePhase.NONE && !soundEntry.IsChanging)
                {
                    soundEntry.Item.audioSource.volume = volume;
                }
            }
            else
            {
                soundEntry.IsChanging = true;
            }
        }
    }

    public AudioListener GetAudioListener()
    {
        CheckAudioListener();
        return _AudioListener;
    }

    #region private

    private void CacheClipFromLoading(string cur_name, string request_name, UnityEngine.Object asset)
    {
        if (asset != null)
        {
            if (!string.IsNullOrEmpty(cur_name) && cur_name == request_name)
            {
                CacheClip(cur_name, asset as AudioClip);
            }
        }
    }

    private void CacheClip(string clip_name, AudioClip clip)
    {
        if (!string.IsNullOrEmpty(clip_name))
        {
            _ClipCache[clip_name] = clip;
        }
    }

    private void LoadCache(string clip_name, out AudioClip clip)
    {
        if (!string.IsNullOrEmpty(clip_name))
        {
            _ClipCache.TryGetValue(clip_name, out clip);
        }
        else
        {
            clip = null;
        }
    }

    private bool IsClipCached(string clip_name)
    {
        return _ClipCache.ContainsKey(clip_name);
    }

    public IEnumerable Init()
    {
        yield return null;
        CheckAudioListener();
    }

    public void SetLanguage(string language)
    {

    }

    private void Start()
    {
        CheckAudioListener();
    }

    public void Tick(float dt)
    {
        UpdateSoundVolumeChange(_BgmSoundEntry, BGMFinalVolume);

        UpdateSoundVolumeChange(_EnvironmentSoundEntry, BGMFinalVolume);

        UpdateSoundVolumeChange(_WeatherSoundEntry, BGMFinalVolume);

        UpdateSoundsPos();

        UpdateSoundFadeInOut(_BgmSoundEntry, BGMFinalVolume);

        UpdateSoundFadeInOut(_EnvironmentSoundEntry, BGMFinalVolume);

        UpdateSoundFadeInOut(_WeatherSoundEntry, BGMFinalVolume);
    }

    private void UpdateSoundVolumeChange(CSoundEntry soundEntry, float finalVolume)
    {
        if (soundEntry.IsChanging && soundEntry.Item.audioSource != null)
        {
            //播放时，更改背景音乐的过度
            if (soundEntry.Item.audioSource.volume > BGMFinalVolume)
            {
                soundEntry.Item.audioSource.volume -= Time.deltaTime / BGMChangeVolumeTime * finalVolume;
                if (soundEntry.Item.audioSource.volume <= finalVolume)
                {
                    soundEntry.Item.audioSource.volume = finalVolume;
                    soundEntry.IsChanging = false;
                }
            }
            else
            {
                soundEntry.Item.audioSource.volume += Time.deltaTime / BGMChangeVolumeTime * finalVolume;
                if (soundEntry.Item.audioSource.volume >= finalVolume)
                {
                    soundEntry.Item.audioSource.volume = finalVolume;
                    soundEntry.IsChanging = false;
                }
            }
        }
    }

    private void UpdateSoundFadeInOut(CSoundEntry soundEntry, float finalVolume)
    {
        if (soundEntry.IsEnabled && soundEntry.Item.audioSource != null)
        {
            if (soundEntry.CurPhase == BgmChangePhase.OLD_FADE_OUT)
            {
                if (soundEntry.Item.audioSource == null || string.IsNullOrEmpty(soundEntry.Item.audioName))
                {
                    soundEntry.CurPhase = BgmChangePhase.NEW_FADE_IN;
                }
                else
                {
                    if (soundEntry.FadeInTime <= 0)
                        soundEntry.Item.audioSource.volume = 0;
                    else
                        soundEntry.Item.audioSource.volume -= (Time.deltaTime / soundEntry.FadeInTime * finalVolume);

                    if (soundEntry.Item.audioSource.volume <= 0)
                    {
                        soundEntry.Item.audioSource.volume = 0;
                        soundEntry.Item.audioSource.Stop();

                        soundEntry.CurPhase = BgmChangePhase.NEW_FADE_IN;
                    }
                }
            }
            else if (soundEntry.CurPhase == BgmChangePhase.NEW_FADE_IN)
            {
                if (soundEntry.Item.audioSource == null)
                {
                    soundEntry.CurPhase = BgmChangePhase.NONE;
                }
                else
                {
                    if (string.IsNullOrEmpty(soundEntry.NewName))
                    {
                        soundEntry.Item.audioSource.clip = null;
                        soundEntry.Item.audioName = string.Empty;
                        soundEntry.CurPhase = BgmChangePhase.NONE;
                    }
                    else if (IsClipCached(soundEntry.NewName))
                    {
                        if (!soundEntry.Item.audioSource.isPlaying)
                        {
                            //PlayAudio(_ClipCache[_newBgmName], true, Vector3.zero, _newBgmName);
                            GetProperSettingOfItem(soundEntry, _ClipCache[soundEntry.NewName], soundEntry.NewName);
                            soundEntry.Item.audioSource.Play();
                            soundEntry.Item.audioName = soundEntry.NewName;
                        }

                        if (soundEntry.FadeInTime <= 0)
                            soundEntry.Item.audioSource.volume = finalVolume;
                        else
                            soundEntry.Item.audioSource.volume += (Time.deltaTime / soundEntry.FadeInTime * finalVolume);
                        if (soundEntry.Item.audioSource.volume >= finalVolume)
                        {
                            soundEntry.Item.audioSource.volume = finalVolume;
                            soundEntry.CurPhase = BgmChangePhase.NONE;
                            soundEntry.NewName = string.Empty;
                        }
                    }
                }
            }
        }
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
        if (a.isLoading != b.isLoading)
            return a.isLoading > b.isLoading ? 1 : -1;

        // 是否在播放中
        if (!a.audioSource.isPlaying && b.audioSource.isPlaying)
            return -1;
        else if (a.audioSource.isPlaying && !b.audioSource.isPlaying)
            return 1;
        else if (a.audioSource.isPlaying && b.audioSource.isPlaying)
        {
            // 按照播放优先级, 小在前
            if (a.priority != b.priority)
                return a.priority > b.priority ? 1 : -1;

            // 按照距离远近
            return DistanceComparison(a.audioSource.transform.position, b.audioSource.transform.position);
        }

        return 0;
    }

    //更新播放状态
    private void UpdateAudioState(EffectAudioSourceItem item, PlayState ps)
    {
        if (item == null)
            return;

        if (ps == PlayState.Resume)
        {
            if (item.audioSource != null && !item.audioSource.isPlaying)
                item.audioSource.Play();
        }
        else if (ps == PlayState.Stop || ps == PlayState.Pause)
        {
            if (item.audioSource != null && item.audioSource.isPlaying)
            {
                item.audioSource.Stop();

                if (ps == PlayState.Stop)
                {
                    _ClipCache.Remove(item.audioName);
                    item.audioSource.clip = null;
                }
            }
        }
    }

    //刷新Attach声源位置
    private void UpdateSoundsPos()
    {
        //if (_AudioListener == null && Camera.main != null)
        //{
        //    _AudioListener = Camera.main.gameObject.GetComponent<AudioListener>();
        //    if (_AudioListener == null)
        //        _AudioListener = Camera.main.gameObject.AddComponent<AudioListener>();
        //}

        int nCount = _effectAudioSourceList.Count;
        for (int i = 0; i < nCount; ++i)
        {
            EffectAudioSourceItem item = _effectAudioSourceList[i];
            if (item.audioSource != null && null != item.attachedPos)
            {
                item.audioSource.transform.position = item.attachedPos.position;
            }
        }
    }

    //检查听筒
    private void CheckAudioListener()
    {
        if (_AudioListener == null)
        {
            GameObject go = new GameObject("Listener");
            _AudioListener = go.AddComponent<AudioListener>();

            if (_ListenerRoot == null)
            {
                _ListenerRoot = transform;
            }
            go.transform.SetParent(_ListenerRoot, false);
        }
    }

    private void SafeInitSFX()
    {
        if (_effectAudioSourceList.Count == 0)
        {
            for (int i = 0; i < MaxEffectChannel; ++i)
            {
                GameObject go = new GameObject(HobaText.Format("effect{0}", i));
                go.transform.parent = transform;
                AudioSource src = go.AddComponent<AudioSource>();
                EffectAudioSourceItem m = new EffectAudioSourceItem();
                m.id = i;
                m.audioSource = src;
                m.priority = 0;
                m.audioSource.minDistance = 10;
                m.audioSource.maxDistance = 30;
                _effectAudioSourceList.Add(m);
            }
        }
    }

    private EffectAudioSourceItem GetFirstItem(List<EffectAudioSourceItem> soundList)
    {
        if (soundList == null || soundList.Count <= 0)
            return null;

        //选一个最小项
        EffectAudioSourceItem item = soundList[0];
        for (int i = 1; i < soundList.Count; ++i)
        {
            if (AudioSourceComparison(item, soundList[i]) > 0)
                item = soundList[i];
        }
        return item;
    }

    //按优先级及距离获取可用的Channel
    private EffectAudioSourceItem GetUsableItem(AudioType aType, int priority, Vector3 pos)
    {
        if (_effectAudioSourceList.Count > 0)
        {
            EffectAudioSourceItem item = GetFirstItem(_effectAudioSourceList);
            if (item != null && item.isLoading == 0)
            {
                if (item.audioSource != null && item.audioSource.isPlaying)
                {
                    if (item.priority > priority)
                    {
                        return null;
                    }
                    else if (item.priority == priority && aType == AudioType.S3D)
                    {
                        if (DistanceComparison(item.audioSource.transform.position, pos) > 0)
                            return null;
                    }
                }
                return item;
            }
        }
        return null;
    }

    private void GetProperSettingOfItem(EffectAudioSourceItem item, AudioType aType, AudioClip audioClip, string audioName, int priority = 0, bool IsLoop = false)
    {
        //if (audioClip.name.Contains("Battle01"))
        //{
        //    Debug.Log("aType " + aType);
        //}

        if (item != null && audioClip != null)
        {
            if (aType == AudioType.AttS3D || aType == AudioType.S2D || aType == AudioType.S3D)
            {
                AudioSource audioSource = item.audioSource;

                if (audioSource != null)
                {
                    /*
                        if (audioSource.clip != null && _ClipCache != null && !string.IsNullOrEmpty(item.audioName) && _ClipCache.ContainsKey(item.audioName))
                        {
                            var curClip = _ClipCache[item.audioName];
                            //Destroy(curClip);
                            _ClipCache.Remove(item.audioName);
                        }
                    */
                    item.audioName = audioName;
                    item.priority = priority;
                    item.attachedPos = null;
                    if (audioSource.isPlaying)
                    {
                        audioSource.Stop();
                    }

                    //audioSource.transform.position = Vector3.zero;
                    audioSource.transform.localPosition = Vector3.zero;
                    audioSource.clip = audioClip;
                    audioSource.volume = EffectFinalVolume;
                    if (AudioType.S3D == aType || AudioType.AttS3D == aType)
                    {
                        audioSource.spatialBlend = 1f;
                    }
                    else if (aType == AudioType.S2D)
                    {
                        audioSource.spatialBlend = 0f;
                    }

                    audioSource.loop = IsLoop;
                }
            }
        }
    }

    private void GetProperSettingOfItem(CSoundEntry soundEntry, AudioClip audioClip, string audioName)
    {
        EffectAudioSourceItem item = soundEntry.Item;
        if (item != null)
        {
            if (!string.IsNullOrEmpty(item.audioName))
            {
                //AudioClip curClip = _ClipCache[item.audioName];
                //Destroy(curClip);
                _ClipCache.Remove(item.audioName);
            }
            AudioSource audioSource = item.audioSource;
            if (audioSource != null)
            {
                audioSource.clip = audioClip;
                audioSource.loop = true;
                audioSource.priority = 0;
                audioSource.volume = 0;
            }
            item.audioName = audioName;
        }
    }

    #endregion private

    public void OnEnterGameWorld()
    {
        _IsInGameWorld = true;
    }

    public void OnLeaveGameWorld()
    {
        _IsInGameWorld = false;
    }

    public void OnFinishEnterWorld(string navmeshName)
    {
        _IsInWorld = true;
    }

    public void OnWorldRelease()
    {
        _IsInWorld = false;
    }

    public void OnLoadingShow(bool show)
    {
        _IsLoadingShow = show;
    }
}