using System.Collections;
using UnityEngine;
    
public interface ISoundMan : GameLogic.ITickLogic
{
    float Volume { get; }
    float BGMSysVolume { set;  get; }
    float BGMVolume { get; set; }
    bool IsBGMEnabled { get; }

    float EffectSysVolume { set; get; }
    float EffectVolume { set; get; }

    bool IsEffectAudioEnabled { get; }

    float CutsceneSysVolume { get; set; }
    float CutsceneVolume { get; set; }

    float UISysVolume { get; set; }
    float UIVolume { get; set; }

    bool CanPlayEffect { get; }

    IEnumerable Init();

    void SetLanguage(string language);

    void EnableBackgroundMusic(bool bOn);
    void EnableEffectAudio(bool bOn);
    void Reset();

    void ChangeBGMVolume(bool immediately = false);

    void ChangeEffectVolume();

    void ChangeCutSceneVolume();

    void ChangeUIVolume();

    bool PlayBackgroundMusic(string bgmName, float fadeInTime);

    bool PlayEnvironmentMusic(string envName, float fadeInTime);

    string Play3DAudio(string audioName, Vector3 pos, int sortId = 0, bool isLoop = false);
    string PlayAttached3DAudio(string audioName, Transform trans, int sortId = 0, bool isLoop = false);

    string Play3DNpcVoice(string audioName, Vector3 pos, int sortId = 0, bool isLoop = false);

    string Play3DNpcShout(string audioName, Vector3 pos, int sortId = 0, bool isLoop = false);

    string Play2DAudio(string audioName, int sortId = 0, bool isLoop = false);
    void Stop3DAudio(string audioName, string audio_source);

    void Stop2DAudio(string audioName, string audio_source);

    string Play2DHeartBeat(string audioName, int sortId = 0, bool isLoop = false);

    string PlayFootStepAudio(string audioName, Vector3 pos, int sortId = 0, bool isLoop = false);

    void OnEnterGameWorld();

    void OnLeaveGameWorld();

    void OnFinishEnterWorld(string navmeshName);

    void OnWorldRelease();

    void OnLoadingShow(bool show);
}

