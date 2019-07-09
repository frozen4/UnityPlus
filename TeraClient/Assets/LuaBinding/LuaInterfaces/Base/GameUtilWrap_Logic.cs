using UnityEngine;
using System;
using System.Collections.Generic;
using LuaInterface;
using Common;
using UnityEngine.UI;
using EntityComponent;

public static partial class GameUtilWrap
{
    #region Sound
    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int AddFootStepTouch(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 1 && LuaScriptMgr.CheckTypes(L, 1, typeof(GameObject)))
        {
            GameObject obj = LuaScriptMgr.GetUnityObject<GameObject>(L, 1);
            if (obj == null)
            {
                HobaDebuger.LogError("AddFootStepTouch: param 1 must be GameObject");
                return CheckReturnNum(L, count, nRet);
            }

            if (obj.GetComponent<Footsteptouch>() == null)
                obj.AddComponent<Footsteptouch>();
        }
        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int RemoveFootStepTouch(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 1 && LuaScriptMgr.CheckTypes(L, 1, typeof(GameObject)))
        {
            GameObject obj = LuaScriptMgr.GetUnityObject<GameObject>(L, 1);
            if (obj == null)
            {
                HobaDebuger.LogError("RemoveFootStepTouch: param 1 must be GameObject");
                return CheckReturnNum(L, count, nRet);
            }

            Footsteptouch comp = obj.GetComponent<Footsteptouch>();
            if (comp != null)
                UnityEngine.Object.Destroy(comp);
        }
        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int GetBGMSysVolume(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 1;

        ISoundMan soundMan = EntryPoint.Instance.SoundMan;
        if (soundMan != null)
        {
            float volume = soundMan.BGMSysVolume;
            LuaScriptMgr.Push(L, volume);
        }
        else
        {
            LuaScriptMgr.Push(L, 0);
        }

        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int IsBackgroundMusicEnable(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 1;

        ISoundMan soundMan = EntryPoint.Instance.SoundMan;
        if (soundMan != null)
        {
            bool bEnable = soundMan.IsBGMEnabled;
            LuaScriptMgr.Push(L, bEnable);
        }
        else
        {
            LuaScriptMgr.Push(L, false);
        }

        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int IsEffectAudioEnable(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 1;

        ISoundMan soundMan = EntryPoint.Instance.SoundMan;
        if (soundMan != null)
        {
            bool bEnable = soundMan.IsEffectAudioEnabled;
            LuaScriptMgr.Push(L, bEnable);
        }
        else
        {
            LuaScriptMgr.Push(L, false);
        }

        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int PlayBackgroundMusic(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 1 && LuaScriptMgr.CheckTypes(L, 1, typeof(string)))
        {
            var bgmName = LuaScriptMgr.GetString(L, 1);

            ISoundMan soundMan = EntryPoint.Instance.SoundMan;
            if (soundMan != null)
                soundMan.PlayBackgroundMusic(bgmName, 0);
        }
        else if (count == 2 && LuaScriptMgr.CheckTypes(L, 1, typeof(string), typeof(float)))
        {
            var bgmName = LuaScriptMgr.GetString(L, 1);
            float fadeInTime = (float)LuaScriptMgr.GetNumber(L, 2);

            ISoundMan soundMan = EntryPoint.Instance.SoundMan;
            if (soundMan != null)
                soundMan.PlayBackgroundMusic(bgmName, fadeInTime);
        }
        else
        {
            LogParamError("PlayBackgroundMusic", count);
        }

        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int PlayEnvironmentMusic(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 1 && LuaScriptMgr.CheckTypes(L, 1, typeof(string)))
        {
            var envMusicName = LuaScriptMgr.GetString(L, 1);

            ISoundMan soundMan = EntryPoint.Instance.SoundMan;
            if (soundMan != null)
                soundMan.PlayEnvironmentMusic(envMusicName, 2);
        }
        else if (count == 2 && LuaScriptMgr.CheckTypes(L, 1, typeof(string), typeof(float)))
        {
            var envMusicName = LuaScriptMgr.GetString(L, 1);
            float fadeInTime = (float)LuaScriptMgr.GetNumber(L, 2);

            ISoundMan soundMan = EntryPoint.Instance.SoundMan;
            if (soundMan != null)
                soundMan.PlayEnvironmentMusic(envMusicName, fadeInTime);
        }
        else
        {
            LogParamError("PlayEnvironmentMusic", count);
        }

        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int Play3DAudio(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 1;

        if (count == 3 && LuaScriptMgr.CheckTypes(L, 1, typeof(string), typeof(LuaTable), typeof(int)))
        {
            var name = LuaScriptMgr.GetString(L, 1);
            var pos = LuaScriptMgr.GetVector3(L, 2);
            int sort = (int)LuaScriptMgr.GetNumber(L, 3);

            ISoundMan soundMan = EntryPoint.Instance.SoundMan;
            if (soundMan != null && soundMan.CanPlayEffect)
            {
                var as_name = soundMan.Play3DAudio(name, pos, sort);
                LuaScriptMgr.Push(L, as_name);
            }
            else
            {
                LuaScriptMgr.Push(L, "");
            }
        }
        else
        {
            LogParamError("Play3DAudio", count);
            LuaScriptMgr.Push(L, "");
        }

        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int PlayAttached3DAudio(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 1;

        if (count == 3 && LuaScriptMgr.CheckTypes(L, 1, typeof(string), typeof(GameObject), typeof(int)))
        {
            var name = LuaScriptMgr.GetString(L, 1);
            var go = LuaScriptMgr.GetUnityObject<GameObject>(L, 2);
            int sort = (int)LuaScriptMgr.GetNumber(L, 3);

            ISoundMan soundMan = EntryPoint.Instance.SoundMan;
            if (soundMan != null && soundMan.CanPlayEffect)
            {
                var as_name = soundMan.PlayAttached3DAudio(name, go != null ? go.transform : null, sort);
                LuaScriptMgr.Push(L, as_name);
            }
            else
            {
                LuaScriptMgr.Push(L, "");
            }
        }
        else
        {
            LogParamError("PlayAttch3DAudio", count);
            LuaScriptMgr.Push(L, "");
        }
        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int Stop3DAudio(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 2 && LuaScriptMgr.CheckTypes(L, 1, typeof(string), typeof(string)))
        {
            var name = LuaScriptMgr.GetString(L, 1);
            string as_name = LuaScriptMgr.GetString(L, 2);
            ISoundMan soundMan = EntryPoint.Instance.SoundMan;
            if (soundMan != null)
                soundMan.Stop3DAudio(name, as_name);
        }
        else
        {
            LogParamError("Stop3DAudio", count);
        }

        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int Stop2DAudio(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 2 && LuaScriptMgr.CheckTypes(L, 1, typeof(string), typeof(string)))
        {
            var name = LuaScriptMgr.GetString(L, 1);
            string as_name = LuaScriptMgr.GetString(L, 2);
            ISoundMan soundMan = EntryPoint.Instance.SoundMan;
            if (soundMan != null)
                soundMan.Stop2DAudio(name, as_name);
        }
        else
        {
            LogParamError("Stop2DAudio", count);
        }

        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int Play3DVoice(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 3 && LuaScriptMgr.CheckTypes(L, 1, typeof(string), typeof(LuaTable), typeof(int)))
        {
            var name = LuaScriptMgr.GetString(L, 1);
            var pos = LuaScriptMgr.GetVector3(L, 2);
            int sort = (int)LuaScriptMgr.GetNumber(L, 3);

            ISoundMan soundMan = EntryPoint.Instance.SoundMan;
            if (soundMan != null)
                soundMan.Play3DNpcVoice(name, pos, sort);
        }
        else
        {
            LogParamError("Play3DVoice", count);
        }

        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int Play3DShout(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 3 && LuaScriptMgr.CheckTypes(L, 1, typeof(string), typeof(LuaTable), typeof(int)))
        {
            var name = LuaScriptMgr.GetString(L, 1);
            var pos = LuaScriptMgr.GetVector3(L, 2);
            int sort = (int)LuaScriptMgr.GetNumber(L, 3);

            ISoundMan soundMan = EntryPoint.Instance.SoundMan;
            if (soundMan != null && soundMan.CanPlayEffect)
                soundMan.Play3DNpcShout(name, pos, sort);
        }
        else
        {
            LogParamError("Play3DShout", count);
        }

        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int Play2DAudio(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 2 && LuaScriptMgr.CheckTypes(L, 1, typeof(string), typeof(int)))
        {
            var name = LuaScriptMgr.GetString(L, 1);
            int sort = (int)LuaScriptMgr.GetNumber(L, 2);

            ISoundMan soundMan = EntryPoint.Instance.SoundMan;
            if (soundMan != null)
                soundMan.Play2DAudio(name, sort);
        }
        else
        {
            LogParamError("Play2DAudio", count);
        }

        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int Play2DHeartBeat(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 2 && LuaScriptMgr.CheckTypes(L, 1, typeof(string), typeof(int)))
        {
            var name = LuaScriptMgr.GetString(L, 1);
            int sort = (int)LuaScriptMgr.GetNumber(L, 2);

            ISoundMan soundMan = EntryPoint.Instance.SoundMan;
            if (soundMan != null)
                soundMan.Play2DHeartBeat(name, sort);
        }
        else
        {
            LogParamError("Play2DHeartBeat", count);
        }

        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int EnableBackgroundMusic(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 1 && LuaScriptMgr.CheckTypes(L, 1, typeof(bool)))
        {
            var bOn = LuaScriptMgr.GetBoolean(L, 1);

            ISoundMan soundMan = EntryPoint.Instance.SoundMan;
            if (soundMan != null)
                soundMan.EnableBackgroundMusic(bOn);
        }
        else
        {
            LogParamError("EnableBackgroundMusic", count);
        }
        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int ResetSoundMan(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        ISoundMan soundMan = EntryPoint.Instance.SoundMan;
        if (soundMan != null)
            soundMan.Reset();

        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int SetSoundBGMVolume(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 2 && LuaScriptMgr.CheckTypes(L, 1, typeof(float), typeof(bool)))
        {
            float volume = (float)LuaScriptMgr.GetNumber(L, 1);
            bool isImmediate = LuaScriptMgr.GetBoolean(L, 2);

            ISoundMan soundMan = EntryPoint.Instance.SoundMan;
            if (soundMan != null)
            {
                soundMan.BGMVolume = volume * soundMan.Volume;
                soundMan.ChangeBGMVolume(isImmediate);
            }
        }
        else
        {
            LogParamError("SetSoundBGMVolume", count);
        }

        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int SetBGMSysVolume(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 1 && LuaScriptMgr.CheckTypes(L, 1, typeof(float)))
        {
            float volume = (float)LuaScriptMgr.GetNumber(L, 1);

            ISoundMan soundMan = EntryPoint.Instance.SoundMan;
            if (soundMan != null)
            {
                soundMan.BGMSysVolume = volume;
                soundMan.ChangeBGMVolume(true);
            }
        }
        else
        {
            LogParamError("SetBGMSysVolume", count);
        }

        return CheckReturnNum(L, count, nRet);
    }


    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int SetSoundLanguage(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 1 && LuaScriptMgr.CheckTypes(L, 1, typeof(string)))
        {
            string language = LuaScriptMgr.GetString(L, 1);

            ISoundMan soundMan = EntryPoint.Instance.SoundMan;
            if (soundMan != null)
                soundMan.SetLanguage(language);
        }
        else
        {
            LogParamError("SetSoundLanguage", count);
        }

        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int SetSoundEffectVolume(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 1 && LuaScriptMgr.CheckTypes(L, 1, typeof(float)))
        {
            float volume = (float)LuaScriptMgr.GetNumber(L, 1);

            ISoundMan soundMan = EntryPoint.Instance.SoundMan;
            if (soundMan != null)
            {
                soundMan.EffectVolume = volume;
                soundMan.ChangeEffectVolume();
            }
        }
        else
        {
            LogParamError("SetSoundEffectVolume", count);
        }

        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int SetEffectSysVolume(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 1 && LuaScriptMgr.CheckTypes(L, 1, typeof(float)))
        {
            float volume = (float)LuaScriptMgr.GetNumber(L, 1);

            ISoundMan soundMan = EntryPoint.Instance.SoundMan;
            if (soundMan != null)
            {
                soundMan.EffectSysVolume = volume;
                soundMan.ChangeEffectVolume();
            }
        }
        else
        {
            LogParamError("SetEffectSysVolume", count);
        }

        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int GetEffectSysVolume(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 1;

        ISoundMan soundMan = EntryPoint.Instance.SoundMan;
        if (soundMan != null)
        {
            float volume = soundMan.EffectSysVolume;
            LuaScriptMgr.Push(L, volume);
        }
        else
        {
            LuaScriptMgr.Push(L, 0);
        }

        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int SetCutSceneVolume(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 1 && LuaScriptMgr.CheckTypes(L, 1, typeof(float)))
        {
            float volume = (float)LuaScriptMgr.GetNumber(L, 1);

            ISoundMan soundMan = EntryPoint.Instance.SoundMan;
            if (soundMan != null)
            {
                soundMan.CutsceneVolume = volume;
                soundMan.ChangeCutSceneVolume();
            }
        }
        else
        {
            LogParamError("SetCutSceneVolume", count);
        }

        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int SetCutSceneSysVolume(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 1 && LuaScriptMgr.CheckTypes(L, 1, typeof(float)))
        {
            float volume = (float)LuaScriptMgr.GetNumber(L, 1);

            ISoundMan soundMan = EntryPoint.Instance.SoundMan;
            if (soundMan != null)
            {
                soundMan.CutsceneSysVolume = volume;
                soundMan.ChangeCutSceneVolume();
            }
        }
        else
        {
            LogParamError("SetCutSceneSysVolume", count);
        }

        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int GetCutSceneSysVolume(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 1;

        ISoundMan soundMan = EntryPoint.Instance.SoundMan;
        if (soundMan != null)
        {
            float volume = soundMan.CutsceneSysVolume;
            LuaScriptMgr.Push(L, volume);
        }
        else
        {
            LuaScriptMgr.Push(L, 0);
        }

        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int SetUIVolume(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 1 && LuaScriptMgr.CheckTypes(L, 1, typeof(float)))
        {
            float volume = (float)LuaScriptMgr.GetNumber(L, 1);

            ISoundMan soundMan = EntryPoint.Instance.SoundMan;
            if (soundMan != null)
            {
                soundMan.UIVolume = volume;
                soundMan.ChangeUIVolume();
            }
        }
        else
        {
            LogParamError("SetUIVolume", count);
        }

        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int SetUISysVolume(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 1 && LuaScriptMgr.CheckTypes(L, 1, typeof(float)))
        {
            float volume = (float)LuaScriptMgr.GetNumber(L, 1);

            ISoundMan soundMan = EntryPoint.Instance.SoundMan;
            if (soundMan != null)
            {
                soundMan.UISysVolume = volume;
                soundMan.ChangeUIVolume();
            }
        }
        else
        {
            LogParamError("SetUISysVolume", count);
        }

        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int GetUISysVolume(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 1;

        ISoundMan soundMan = EntryPoint.Instance.SoundMan;
        if (soundMan != null)
        {
            float volume = soundMan.UISysVolume;
            LuaScriptMgr.Push(L, volume);
        }
        else
        {
            LuaScriptMgr.Push(L, 0);
        }

        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int EnableEffectAudio(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 1 && LuaScriptMgr.CheckTypes(L, 1, typeof(bool)))
        {
            var bOn = LuaScriptMgr.GetBoolean(L, 1);

            ISoundMan soundMan = EntryPoint.Instance.SoundMan;
            if (soundMan != null)
                soundMan.EnableEffectAudio(bOn);
        }
        else
        {
            LogParamError("EnableEffectAudio", count);
        }

        return CheckReturnNum(L, count, nRet);
    }

    #endregion

    #region Setting

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int SetPostProcessLevel(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 1 && LuaScriptMgr.CheckTypes(L, 1, typeof(int)))
        {
            int level = (int)LuaScriptMgr.GetNumber(L, 1);
            GFXConfig.Instance.PostProcessLevel = level;
            return CheckReturnNum(L, count, nRet);
        }
        else
        {
            LogParamError("SetPostProcessLevel", count);
            return CheckReturnNum(L, count, nRet);
        }
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int GetPostProcessLevel(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 1;

        int level = GFXConfig.Instance.PostProcessLevel;
        LuaScriptMgr.Push(L, level);

        return CheckReturnNum(L, count, nRet);
    }


    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int SetShadowLevel(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 1 && LuaScriptMgr.CheckTypes(L, 1, typeof(int)))
        {
            int level = (int)LuaScriptMgr.GetNumber(L, 1);
            GFXConfig.Instance.ShadowLevel = level;
            return CheckReturnNum(L, count, nRet);
        }
        else
        {
            LogParamError("SetShadowLevel", count);
            return CheckReturnNum(L, count, nRet);
        }
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int GetShadowLevel(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 1;

        int level = GFXConfig.Instance.ShadowLevel;
        LuaScriptMgr.Push(L, level);

        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int SetCharacterLevel(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 1 && LuaScriptMgr.CheckTypes(L, 1, typeof(int)))
        {
            int level = (int)LuaScriptMgr.GetNumber(L, 1);
            GFXConfig.Instance.CharacterLevel = level;
            return CheckReturnNum(L, count, nRet);
        }
        else
        {
            LogParamError("SetCharacterLevel", count);
            return CheckReturnNum(L, count, nRet);
        }
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int GetCharacterLevel(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 1;

        int level = GFXConfig.Instance.CharacterLevel;
        LuaScriptMgr.Push(L, level);

        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int SetSceneDetailLevel(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 1 && LuaScriptMgr.CheckTypes(L, 1, typeof(int)))
        {
            int level = (int)LuaScriptMgr.GetNumber(L, 1);
            GFXConfig.Instance.SceneDetailLevel = level;
            return CheckReturnNum(L, count, nRet);
        }
        else
        {
            LogParamError("SetSceneDetailLevel", count);
            return CheckReturnNum(L, count, nRet);
        }
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int GetSceneDetailLevel(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 1;

        int level = GFXConfig.Instance.SceneDetailLevel;
        LuaScriptMgr.Push(L, level);

        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int SetFxLevel(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 1 && LuaScriptMgr.CheckTypes(L, 1, typeof(int)))
        {
            int level = (int)LuaScriptMgr.GetNumber(L, 1);
            GFXConfig.Instance.FxLevel = level;
            return CheckReturnNum(L, count, nRet);
        }
        else
        {
            LogParamError("SetFxLevel", count);
            return CheckReturnNum(L, count, nRet);
        }
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int GetFxLevel(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 1;

        int level = GFXConfig.Instance.FxLevel;
        LuaScriptMgr.Push(L, level);

        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int IsUseDOF(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 1;

        bool bUse = GFXConfig.Instance.IsEnableDOF;
        LuaScriptMgr.Push(L, bUse);

        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int EnableDOF(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 1 && LuaScriptMgr.CheckTypes(L, 1, typeof(bool)))
        {
            bool bUse = LuaScriptMgr.GetBoolean(L, 1);
            GFXConfig.Instance.IsEnableDOF = bUse;
        }
        else
        {
            LogParamError("EnableDOF", count);
        }
        return CheckReturnNum(L, count, nRet);
    }


    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int IsUseWaterReflection(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 1;

        bool bUse = GFXConfig.Instance.IsEnableWaterReflection;
        LuaScriptMgr.Push(L, bUse);

        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int EnableWaterReflection(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 1 && LuaScriptMgr.CheckTypes(L, 1, typeof(bool)))
        {
            bool bUse = LuaScriptMgr.GetBoolean(L, 1);
            GFXConfig.Instance.IsEnableWaterReflection = bUse;
        }
        else
        {
            LogParamError("EnableWaterReflection", count);
        }
        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int IsUsePostProcessFog(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 1;

        bool bUse = GFXConfig.Instance.IsUsePostProcessFog;
        LuaScriptMgr.Push(L, bUse);

        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int EnablePostProcessFog(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 1 && LuaScriptMgr.CheckTypes(L, 1, typeof(bool)))
        {
            bool bUse = LuaScriptMgr.GetBoolean(L, 1);
            GFXConfig.Instance.IsUsePostProcessFog = bUse;
        }
        else
        {
            LogParamError("EnablePostProcessFog", count);
        }
        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int IsUseWeatherEffect(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 1;

        bool bUse = GFXConfig.Instance.IsUseWeatherEffect;
        LuaScriptMgr.Push(L, bUse);

        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int EnableWeatherEffect(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 1 && LuaScriptMgr.CheckTypes(L, 1, typeof(bool)))
        {
            bool bUse = LuaScriptMgr.GetBoolean(L, 1);
            GFXConfig.Instance.IsUseWeatherEffect = bUse;
        }
        else
        {
            LogParamError("EnableWeatherEffect", count);
        }
        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int IsUseDetailFootStepSound(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 1;

        bool bUse = GFXConfig.Instance.IsUseDetailFootStepSound;
        LuaScriptMgr.Push(L, bUse);

        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int EnableDetailFootStepSound(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 1 && LuaScriptMgr.CheckTypes(L, 1, typeof(bool)))
        {
            bool bUse = LuaScriptMgr.GetBoolean(L, 1);
            GFXConfig.Instance.IsUseDetailFootStepSound = bUse;
        }
        else
        {
            LogParamError("EnableDetailFootStepSound", count);
        }
        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int GetFPSLimit(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 1;

        int fps = GFXConfig.Instance.FPSLimit;
        LuaScriptMgr.Push(L, fps);

        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int SetFPSLimit(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 1 && LuaScriptMgr.CheckTypes(L, 1, typeof(int)))
        {
            int fps = (int)LuaScriptMgr.GetNumber(L, 1);
            GFXConfig.Instance.FPSLimit = fps;
        }
        else
        {
            LogParamError("SetFPSLimit", count);
        }
        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int SetSimpleBloomHDParams(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 2 && LuaScriptMgr.CheckTypes(L, 2, typeof(int)))
        {
            int level = (int)LuaScriptMgr.GetNumber(L, 1);
            int iteration = (int)LuaScriptMgr.GetNumber(L, 2);

            PostProcess.BloomHD._LEVEL = level;
            PostProcess.BloomHD._ITERATION = iteration;
        }
        else
        {
            LogParamError("SetSimpleBloomHDParams", count);
        }
        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int ApplyGfxConfig(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        GFXConfig.Instance.ApplyChange();

        return CheckReturnNum(L, count, nRet);
    }

    #endregion

    #region GraphicLOD

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int EnableLightShadow(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 2 && LuaScriptMgr.CheckTypes(L, 1, typeof(GameObject), typeof(bool)))
        {
            GameObject obj = LuaScriptMgr.GetUnityObject<GameObject>(L, 1);
            if (obj == null)
            {
                HobaDebuger.LogError("EnableLightShadow: param 1 must be GameObject");
                return CheckReturnNum(L, count, nRet);
            }

            Light light = obj.GetComponent<Light>();
            if (light == null)
            {
                HobaDebuger.LogError("EnableLightShadow: param 1 must have light component");
                return CheckReturnNum(L, count, nRet);
            }

            bool bShadow = LuaScriptMgr.GetBoolean(L, 2);

            if (bShadow)
                light.shadows = LightShadows.Hard;
            else
                light.shadows = LightShadows.None;
        }
        else
        {
            LogParamError("EnableLightShadow", count);
        }

        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int EnableBloomHD(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 2 && LuaScriptMgr.CheckTypes(L, 1, typeof(GameObject), typeof(bool)))
        {
            GameObject obj = LuaScriptMgr.GetUnityObject<GameObject>(L, 1);
            if (obj == null)
            {
                HobaDebuger.LogError("EnableBloomHD: param 1 must be GameObject");
                return CheckReturnNum(L, count, nRet);
            }

            Camera cam = obj.GetComponent<Camera>();
            if (cam == null)
            {
                HobaDebuger.LogError("EnableBloomHD: param 1 must have Camera component");
                return CheckReturnNum(L, count, nRet);
            }

            PostProcessChain chain = obj.GetComponent<PostProcessChain>();
            if (chain == null)
            {
                HobaDebuger.LogError("EnableBloomHD: param 1 must have PostProcessChain component");
                return CheckReturnNum(L, count, nRet);
            }

            bool bEnable = LuaScriptMgr.GetBoolean(L, 2);
            chain.EnableBloomHD = bEnable;
            cam.allowHDR = bEnable;
        }
        else
        {
            LogParamError("EnableBloomHD", count);
        }
        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int FixCameraSetting(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 1 && LuaScriptMgr.CheckTypes(L, 1, typeof(GameObject)))
        {
            GameObject obj = LuaScriptMgr.GetUnityObject<GameObject>(L, 1);
            if (obj == null)
            {
                HobaDebuger.LogError("FixCameraSetting: param 1 must be GameObject");
                return CheckReturnNum(L, count, nRet);
            }

            Camera cam = obj.GetComponent<Camera>();
            if (cam == null)
            {
                HobaDebuger.LogError("FixCameraSetting: param 1 must have Camera component");
                return CheckReturnNum(L, count, nRet);
            }

            cam.renderingPath = RenderingPath.Forward;

            int invisibleMask = LayerMask.GetMask("UI", "Invisible", "Blur");
            cam.cullingMask = (-1 & (~invisibleMask));
        }
        else
        {
            LogParamError("FixCameraSetting", count);
        }

        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int EnableCastShadows(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 2 && LuaScriptMgr.CheckTypes(L, 1, typeof(GameObject), typeof(bool)))
        {
            GameObject obj = LuaScriptMgr.GetUnityObject<GameObject>(L, 1);
            if (obj == null)
            {
                HobaDebuger.LogError("EnableCastShadows: param 1 must be GameObject");
                return CheckReturnNum(L, count, nRet);
            }

            bool bOn = LuaScriptMgr.GetBoolean(L, 2);

            CUnityUtil.EnableCastShadows(obj, bOn);
        }
        else
        {
            LogParamError("EnableCastShadows", count);
        }

        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int SetActiveFxMaxCount(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 1 && LuaScriptMgr.CheckTypes(L, 1, typeof(int)))
        {
            var maxCount = (int)LuaScriptMgr.GetNumber(L, 1);
            if (maxCount >= 0)
                CFxCacheMan.Instance.MaxActiveFxCount = maxCount;
        }
        else
        {
            LogParamError("SetActiveFxMaxCount", count);
        }

        return GameUtilWrap.CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int GetFxManUncachedFxsRoot(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 1;

        GameObject go = CFxCacheMan.Instance.UncachedFxsRootTrans.gameObject;
        if (go == null)
            LuaDLL.lua_pushnil(L);
        else
            LuaScriptMgr.Push(L, go);

        return GameUtilWrap.CheckReturnNum(L, count, nRet);
    }

    #endregion

    #region Callback

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int OnHostPlayerCreate(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 1 && LuaScriptMgr.CheckTypes(L, 1, typeof(GameObject)))
        {
            GameObject obj = LuaScriptMgr.GetUnityObject<GameObject>(L, 1);
            if (obj == null)
            {
                LuaScriptMgr.Instance.CallOnTraceBack();
                HobaDebuger.LogError("OnHostPlayerCreate: param 1 must be GameObject");
                return CheckReturnNum(L, count, nRet);
            }

            Main.HostPalyer = obj.transform;

            /*
            var skmr_list = obj.GetComponentsInChildren<SkinnedMeshRenderer>();
            if (skmr_list.Length > 0)
            {
                for (int i = 0; i < skmr_list.Length; i++)
                {
                    skmr_list[i].receiveShadows = false;
                }
            }
             * */
        }

        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int OnHostPlayerDestroy(IntPtr L)
    {
        Main.HostPalyer = null;
        return 0;
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int OnMainCameraCreate(IntPtr L)
    {
        //重置CCamCtrlMan的状态
        CCamCtrlMan.Instance.Reset();
        RemoveMainCameraEffect();

        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count != 0)
        {
            HobaDebuger.LogError("invalid arguments to method: OnMainCameraCreate");
            return CheckReturnNum(L, count, nRet);
        }

        Camera main = Camera.main;
        Main.Main3DCamera = main;
        if (main == null)
        {
            HobaDebuger.LogError("Failed to OnMainCameraCreate, because Camera.main is null");
            return CheckReturnNum(L, count, nRet);
        }

        main.renderingPath = RenderingPath.Forward;

        //if (main.depthTextureMode == DepthTextureMode.None)
        //    main.depthTextureMode = DepthTextureMode.Depth;

        //Add Clip planes here...
        //if (null == main.gameObject.GetComponent<VisibleTest>())
        //{
        //    main.gameObject.AddComponent<VisibleTest>();
        //}

        Transform com3dT = Main.Main3DCamera.GetComponent<Transform>();

        // 初始化TopPate Canvas相机
        Transform t_camTP = com3dT.Find("TopPateCamera");
        if (t_camTP != null)
        {
            Main.TopPateCamera = t_camTP.GetComponent<Camera>();

            GTopPateCam gtp_cam = t_camTP.gameObject.AddComponent<GTopPateCam>();
            gtp_cam.SetTarget(Main.Main3DCamera);

            GameObject toppate_root = GameObject.Find("HUDCanvas");
            if (toppate_root != null)
            {
                Canvas c = toppate_root.GetComponent<Canvas>();
                if (c != null)
                {
                    c.renderMode = RenderMode.WorldSpace;
                    c.worldCamera = main;
                }
            }
            int invisibleMask = LayerMask.GetMask("UI", "Invisible", "Blur", "TopPate");
            main.cullingMask = (-1 & (~invisibleMask));
        }
        else
        {
            int invisibleMask = LayerMask.GetMask("UI", "Invisible", "Blur");
            main.cullingMask = (-1 & (~invisibleMask));
        }

        // 角色光
        if (Main.PlayerLight == null)
        {
            Main.PlayerLight = new GameObject("PlayerLight");

            //PlayerLight.transform.parent = main.transform;
            Main.PlayerLight.transform.position = Vector3.zero;
            Main.PlayerLight.transform.rotation = Quaternion.Euler(56.67795f, 111.3001f, 310.5828f);
            Main.PlayerLight.transform.localScale = Vector3.one;
            var light = Main.PlayerLight.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = new Color(1, 1, 1, 1);
            light.intensity = 1f;
            light.bounceIntensity = 0f;

            if (GFXConfig.Instance.ShadowLevel == 0)
            {
                light.shadows = LightShadows.None;
                light.cullingMask = CUnityUtil.LayerMaskShadows_L0;
            }
            else if (GFXConfig.Instance.ShadowLevel == 1)
            {
                light.shadows = LightShadows.Hard;
                light.cullingMask = CUnityUtil.LayerMaskShadows_L0;
            }
            else
            {
                light.shadows = LightShadows.Hard;
                light.cullingMask = CUnityUtil.LayerMaskShadows_L2;
            }

            //var sun = Main.PlayerLight.AddComponent<Sun>();
            //sun.color = new Color(1, 1, 1, 1);
            //sun.AmbientColorIntensity = 1;
        }

        // UI光
        if (Main.UILight == null)
        {
            //Main.UILight = new GameObject("UILight");
            Main.UILight = GameObject.Find("UILight");

            //Main.UILight = new GameObject("UILight");
            //Transform ui_light_t = Main.UILight.transform;

            //ui_light_t.transform.position = Vector3.zero;
            //ui_light_t.transform.rotation = Quaternion.Euler(50f, 325f, 0f);
            //ui_light_t.transform.localScale = Vector3.one;
            Light light = Main.UILight.GetComponent<Light>();
            //light.type = LightType.Directional;
            //light.color = new Color(1, 1, 1, 1);
            //light.intensity = 1f;
            //light.bounceIntensity = 0f;
            //light.cullingMask = CUnityUtil.LayerMaskUI;
            light.shadows = GFXConfig.Instance.IsEnableRealTimeShadow ? LightShadows.Hard : LightShadows.None;
            //light.enabled = false;
        }

        // 相机后处理效果
        /*
        var bcg = main.gameObject.GetComponent<BrightnessContrastGamma>();
        if (bcg == null)
        {
            bcg = main.gameObject.AddComponent<BrightnessContrastGamma>();
            bcg.Shader = ShaderManager.Instance.FindShader("Hidden/Colorful/Brightness Contrast Gamma");
            bcg.Brightness = 12f;
            bcg.Contrast = 7f;
            bcg.ContrastCoeff.x = 0.8f;
            bcg.ContrastCoeff.y = 0.6f;
            bcg.ContrastCoeff.z = 1f;
            bcg.Gamma = 0.9f;
        }
        */
        //var bloom = main.gameObject.GetComponent<UnityStandardAssets.ImageEffects.Bloom>();
        //if(bloom == null)
        //{
        //    main.gameObject.AddComponent<UnityStandardAssets.ImageEffects.Bloom>();
        //}

        // 创建屏幕Blur相机
        //if (Main.RapidBlurCam == null)
        //{
        //    Main.RapidBlurCam = new GameObject("RapidBlurCam");
        //    Transform blurTrans = Main.RapidBlurCam.transform;
        //    blurTrans.parent = main.transform;
        //    blurTrans.localPosition = Vector3.zero;
        //    blurTrans.localRotation = Quaternion.identity;
        //    blurTrans.localScale = Vector3.one;
        //    var blurCam = Main.RapidBlurCam.AddComponent<Camera>();
        //    blurCam.clearFlags = CameraClearFlags.Depth;
        //    blurCam.cullingMask = LayerMask.GetMask("Blur");
        //    blurCam.depth = main.depth + 1;
        //    blurCam.fieldOfView = main.fieldOfView;
        //    blurCam.orthographic = main.orthographic;
        //    Main.RapidBlurCam.SetActive(false);
        //}

        //var blurComp = main.gameObject.AddComponent<RapidBlurEffect>();
        //blurComp.enabled = false;

        {
            //PostProcess.BloomHD bloom = main.gameObject.AddComponent<PostProcess.BloomHD>();
            //bloom.Threshold = 2;
            //bloom.Intensity = 1;
            //bloom.Radius = 1.5f;
            //bloom.Iteration = 10;
            //bloom.WithFlicker = false;
            //
            //if (GFXConfig.Instance.IsEnableBloomHD)
            //{
            //    main.allowHDR = true;
            //    bloom.enabled = true;
            //}
            //else
            //{
            //    main.allowHDR = false;
            //    bloom.enabled = false;
            //}
        }

        var sv = main.gameObject.AddComponent<PostProcessChain>();
        //main.gameObject.GetComponent<FrameRenderer>().EnableRenderDepth();

        sv.brightness_contrast_paramters.x = 1.1f;
        sv.brightness_contrast_paramters.y = 1.1f;
        sv.brightness_contrast_paramters.z = 0.3f;

        Main.MainPostProcessChain = sv;

        //开始天气系统
        //ScenesManager.Instance.StartInvoking();

        //通知音频系统
        ISoundMan soundMan = EntryPoint.Instance.SoundMan;
        if (soundMan != null)
            soundMan.OnEnterGameWorld();

        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int OnMainCameraDestroy(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        ISoundMan soundMan = EntryPoint.Instance.SoundMan;
        if (soundMan != null)
        {
            //停止声音
            soundMan.Reset();
            //通知音频系统
            soundMan.OnLeaveGameWorld();
        }

        //停止天气系统
        //DynamicEffectManager.Instance.enabled = false;
        ScenesManager.Instance.Release();
        DynamicEffectManager.Instance.Release();

        if (Main.PlayerLight != null)
        {
            GameObject.Destroy(Main.PlayerLight);
            Main.PlayerLight = null;
        }

        RemoveMainCameraEffect();

        //重置CCamCtrlMan的状态
        CCamCtrlMan.Instance.Reset();

        Main.MainPostProcessChain = null;
        Main.Main3DCamera = null;

        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int EnableMainCamera(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 2 && (LuaScriptMgr.CheckTypes(L, 1, typeof(bool), typeof(int))))
        {
            bool enable = LuaScriptMgr.GetBoolean(L, 1);
            var maskInt = (int)LuaScriptMgr.GetNumber(L, 2);
            var mask = (Main.CameraEnableMask) maskInt;
            Main.EnableMainCamera(enable, mask);
        }
        else
        {
            LogParamError("EnableMainCamera", count);
        }
        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int OnWorldLoaded(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 1)
        {
            GameObject obj = LuaScriptMgr.GetUnityObject<GameObject>(L, 1);
            if (null != obj)
            {
                ScenesManager.Instance.OnEnterScene(obj);
            }
            else
            {
                HobaDebuger.LogError("ScenesManger init failed!");
            }

            return CheckReturnNum(L, count, nRet);
        }
        else
        {
            LogParamError("OnWorldLoaded", count);
            return CheckReturnNum(L, count, nRet);
        }
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int OnFinishEnterWorld(IntPtr L)
    {
        //通知音频系统
        ISoundMan soundMan = EntryPoint.Instance.SoundMan;
        if (soundMan != null)
        {
            var navmesh_name = ScenesManager.Instance.GetCurrentNavmeshName();
            string name = System.IO.Path.GetFileNameWithoutExtension(navmesh_name);
            soundMan.OnFinishEnterWorld(name);
        }

        return 0;
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int OnLoadingShow(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 1 && LuaScriptMgr.CheckTypes(L, 1, typeof(bool)))
        {
            bool show = LuaScriptMgr.GetBoolean(L, 1);
            //通知音频系统
            ISoundMan soundMan = EntryPoint.Instance.SoundMan;
            if (soundMan != null)
            {
                soundMan.OnLoadingShow(show);
            }
        }
        else
        {
            LogParamError("OnLoadingShow", count);
            return CheckReturnNum(L, count, nRet);
        }
        

        return 0;
    }
    

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int OnWorldRelease(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        //通知音频系统
        ISoundMan soundMan = EntryPoint.Instance.SoundMan;
        if (soundMan != null)
        {
            soundMan.OnWorldRelease();
        }

        CEnvTransparentEffectMan.Instance.Cleanup();
        ScenesManager.Instance.Cleanup();
        DynamicEffectManager.Instance.Cleanup();

        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    static int PassLuaGuideMan(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 1 && LuaScriptMgr.CheckTypes(L, 1, typeof(LuaTable)))
        {
            LuaTable arg0 = LuaScriptMgr.GetLuaTable(L, 1);
            Main.LuaGuideMan = arg0;
        }
        else
        {
            LogParamError("SetLuaGuideMan", count);
        }
        return CheckReturnNum(L, count, nRet);
    }
    #endregion

    #region CG
    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int PlayCG(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;
        if (count == 4 && LuaScriptMgr.CheckTypes(L, 1, typeof(string), typeof(int), typeof(LuaFunction), typeof(bool)))
        {
            var cgPath = LuaScriptMgr.GetString(L, 1);
            var priority = (int)LuaScriptMgr.GetNumber(L, 2);
            Action callback = null;
            if (!LuaDLL.lua_isnil(L, 3))
            {
                LuaDLL.lua_pushvalue(L, 3);
                int callbackRef = LuaDLL.luaL_ref(L, LuaIndexes.LUA_REGISTRYINDEX);
                callback = () =>
                {
                    if (LuaScriptMgr.Instance.GetLuaState() == null || callbackRef == 0)
                        return;

                    var oldTop = LuaDLL.lua_gettop(L);
                    LuaDLL.lua_rawgeti(L, LuaIndexes.LUA_REGISTRYINDEX, callbackRef);
                    LuaDLL.luaL_unref(L, LuaIndexes.LUA_REGISTRYINDEX, callbackRef);
                    if (LuaDLL.lua_isnil(L, -1))
                    {
                        LuaDLL.lua_settop(L, oldTop);
                        return;
                    }
                    if (!LuaScriptMgr.Instance.GetLuaState().PCall(0, 0))
                    {
                        HobaDebuger.LogLuaError(LuaDLL.lua_tostring(L, -1));
                    }
                    LuaDLL.lua_settop(L, oldTop);
                };
            }
            var canSkip = LuaScriptMgr.GetBoolean(L, 4);
            CGManager.Instance.PlayCG(cgPath, priority, callback, !canSkip);
        }
        else
        {
            LogParamError("PlayCG", count);
        }
        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int StopCG(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 0)
            CGManager.Instance.StopCG();
        else
            LogParamError("StopCG", count);

        return CheckReturnNum(L, count, nRet);
    }
    #endregion

    #region Net
    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int SendProtocol(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 2)
        {
            int arg0 = (int)LuaScriptMgr.GetNumber(L, 1);
            LuaStringBuffer lsb = LuaScriptMgr.GetStringBuffer(L, 2);
            CGameSession.Instance().SendProtocol(arg0, lsb.buffer);
            if (bLogProtocol)
                HobaDebuger.LogWarningFormat("Sending Protocol: {0}", arg0);
        }
        else
        {
            LogParamError("SendProtocol", count);
        }

        return CheckReturnNum(L, count, nRet);
    }
    #endregion

    #region Video
    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int PlayVideo(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 2 && LuaScriptMgr.CheckTypes(L, 1, typeof(string), typeof(bool)))
        {
            var videoName = LuaScriptMgr.GetString(L, 1);
            if (string.IsNullOrEmpty(videoName))
            {
                HobaDebuger.LogWarning("PlayVideo: param 1 must be string");
                return GameUtilWrap.CheckReturnNum(L, count, nRet);
            }
            string videoPath = System.IO.Path.Combine(Application.streamingAssetsPath, videoName);
            bool isLoop = LuaScriptMgr.GetBoolean(L, 2);
            EntryPoint.Instance.VideoManager.PlayVideo(videoPath, null, null, null, isLoop);
        }
        else if (count == 4 && LuaScriptMgr.CheckTypes(L, 1, typeof(string), typeof(bool), typeof(bool), typeof(LuaFunction)))
        {
            var videoName = LuaScriptMgr.GetString(L, 1);
            if (string.IsNullOrEmpty(videoName))
            {
                HobaDebuger.LogWarning("PlayVideo: param 1 must be string");
                return GameUtilWrap.CheckReturnNum(L, count, nRet);
            }
            string videoPath = System.IO.Path.Combine(Application.streamingAssetsPath, videoName);
            bool isLoop = LuaScriptMgr.GetBoolean(L, 2);
            bool stopOnClick = LuaScriptMgr.GetBoolean(L, 3);

            LuaDLL.lua_pushvalue(L, 4);
            int callbackRef = LuaDLL.luaL_ref(L, LuaIndexes.LUA_REGISTRYINDEX);
            Action callback = () =>
            {
                if (LuaScriptMgr.Instance.GetLuaState() == null || callbackRef == 0)
                    return;

                var oldTop = LuaDLL.lua_gettop(L);
                LuaDLL.lua_rawgeti(L, LuaIndexes.LUA_REGISTRYINDEX, callbackRef);
                LuaDLL.luaL_unref(L, LuaIndexes.LUA_REGISTRYINDEX, callbackRef);
                if (LuaDLL.lua_isnil(L, -1))
                {
                    LuaDLL.lua_settop(L, oldTop);
                    return;
                }
                if (!LuaScriptMgr.Instance.GetLuaState().PCall(0, 0))
                {
                    HobaDebuger.LogLuaError(LuaDLL.lua_tostring(L, -1));
                }
                LuaDLL.lua_settop(L, oldTop);
            };

            EntryPoint.Instance.VideoManager.PlayVideo(videoPath, null, null, callback, isLoop, false, stopOnClick);
        }
        else if (count == 5 && LuaScriptMgr.CheckTypes(L, 1, typeof(GameObject), typeof(string), typeof(bool), typeof(bool), typeof(LuaFunction)))
        {
            GameObject obj = LuaScriptMgr.GetUnityObject<GameObject>(L, 1);
            RawImage rawImage = obj.GetComponent<RawImage>();
            if (obj == null)
            {
                HobaDebuger.LogWarning("PlayVideo: param 1 must be GameObject");
                return GameUtilWrap.CheckReturnNum(L, count, nRet);
            }
            if (rawImage == null)
            {
                HobaDebuger.LogWarning("PlayVideo: param 1 must be GameObject, And this GameObject must have a RawImage Component");
                return GameUtilWrap.CheckReturnNum(L, count, nRet);
            }
            var videoName = LuaScriptMgr.GetString(L, 2);
            if (string.IsNullOrEmpty(videoName))
            {
                HobaDebuger.LogWarning("PlayVideo: param 2 must be string");
                return GameUtilWrap.CheckReturnNum(L, count, nRet);
            }
            //string videoPath = System.IO.Path.Combine(Application.streamingAssetsPath, videoName);
            string videoPath = EntryPoint.Instance.GetFullResPath("Video", videoName);

            bool isLoop = LuaScriptMgr.GetBoolean(L, 3);
            bool stopOnClick = LuaScriptMgr.GetBoolean(L, 4);

            LuaDLL.lua_pushvalue(L, 5);
            int callbackRef = LuaDLL.luaL_ref(L, LuaIndexes.LUA_REGISTRYINDEX);
            Action callback = () =>
            {
                if (LuaScriptMgr.Instance.GetLuaState() == null || callbackRef == 0)
                    return;

                var oldTop = LuaDLL.lua_gettop(L);
                LuaDLL.lua_rawgeti(L, LuaIndexes.LUA_REGISTRYINDEX, callbackRef);
                LuaDLL.luaL_unref(L, LuaIndexes.LUA_REGISTRYINDEX, callbackRef);
                if (LuaDLL.lua_isnil(L, -1))
                {
                    LuaDLL.lua_settop(L, oldTop);
                    return;
                }
                if (!LuaScriptMgr.Instance.GetLuaState().PCall(0, 0))
                {
                    HobaDebuger.LogLuaError(LuaDLL.lua_tostring(L, -1));
                }
                LuaDLL.lua_settop(L, oldTop);
            };

            EntryPoint.Instance.VideoManager.PlayVideo(videoPath, rawImage, null, callback, isLoop, false, stopOnClick);
        }
        else
        {
            HobaDebuger.LogError("PlayVideo's param is WRONG");
        }
        return GameUtilWrap.CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int StopVideo(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 0)
        {
            EntryPoint.Instance.VideoManager.StopVideo();
        }
        else
        {
            HobaDebuger.LogError("StopVideo's param is WRONG");
        }
        return GameUtilWrap.CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int IsPlayingVideo(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 1;

        if (count == 0)
        {
            bool isPlaying = EntryPoint.Instance.VideoManager.IsPlaying;
            LuaDLL.lua_pushboolean(L, isPlaying);
        }
        else
        {
            HobaDebuger.LogError("StopVideo's param is WRONG");
        }
        return GameUtilWrap.CheckReturnNum(L, count, nRet);
    }

    public static void PauseVideoUnitOnFirstFrame(UnityEngine.Video.VideoPlayer player, long frameIdx)
    {
        player.Pause();
        player.sendFrameReadyEvents = false;
        player.frameReady -= PauseVideoUnitOnFirstFrame;
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int PrepareVideoUnit(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 2 && LuaScriptMgr.CheckTypes(L, 1, typeof(GameObject), typeof(string)))
        {
            var go = LuaScriptMgr.GetUnityObject<GameObject>(L, 1);
            if (go == null)
            {
                HobaDebuger.LogWarning("PrepareVideoUnit: param 1 GameObject got null");
                return GameUtilWrap.CheckReturnNum(L, count, nRet);
            }
            var videoPlayer = go.AddComponent<UnityEngine.Video.VideoPlayer>();
            VideoPlayerManager.InitVideoPlayer(videoPlayer);

            string videoName = LuaScriptMgr.GetString(L, 2);
            videoPlayer.url = EntryPoint.Instance.GetFullResPath("Video", videoName);
            videoPlayer.isLooping = true;
            videoPlayer.targetTexture = VideoPlayerManager.SetupRenderTexture();
            videoPlayer.sendFrameReadyEvents = true;
            videoPlayer.frameReady += PauseVideoUnitOnFirstFrame;
            videoPlayer.Play();
        }
        else
        {
            HobaDebuger.LogError("PrepareVideoUnit's param is WRONG");
        }
        return GameUtilWrap.CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int ActivateVideoUnit(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 2 && LuaScriptMgr.CheckTypes(L, 1, typeof(GameObject), typeof(GameObject)))
        {
            var go1 = LuaScriptMgr.GetUnityObject<GameObject>(L, 1);
            if (go1 == null)
            {
                HobaDebuger.LogWarning("ActivateVideoUnit: param 1 GameObject got null");
                return GameUtilWrap.CheckReturnNum(L, count, nRet);
            }
            var videoPlayer = go1.GetComponent<UnityEngine.Video.VideoPlayer>();
            if (videoPlayer == null)
            {
                HobaDebuger.LogWarning("ActivateVideoUnit: param 1 GameObject must has a VideoPlayer");
                return GameUtilWrap.CheckReturnNum(L, count, nRet);
            }
            var go2 = LuaScriptMgr.GetUnityObject<GameObject>(L, 2);
            if (go2 == null)
            {
                HobaDebuger.LogWarning("ActivateVideoUnit: param 2 GameObject got null");
                return GameUtilWrap.CheckReturnNum(L, count, nRet);
            }
            var rawImg = go2.GetComponent<RawImage>();
            if (rawImg == null)
            {
                HobaDebuger.LogWarning("ActivateVideoUnit: param 2 GameObject must has a RawImage");
                return GameUtilWrap.CheckReturnNum(L, count, nRet);
            }
            if (videoPlayer.isPrepared)
            {
                RenderTexture rt = videoPlayer.targetTexture;
                GNewUITools.SetAspectMode(go2, (float)rt.width / (float)rt.height, (int)AspectRatioFitter.AspectMode.EnvelopeParent);
                videoPlayer.Play();
                rawImg.texture = rt;
            }
        }
        else
        {
            HobaDebuger.LogError("ActivateVideoUnit's param is WRONG");
        }
        return GameUtilWrap.CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int DeactivateVideoUnit(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 1 && LuaScriptMgr.CheckTypes(L, 1, typeof(GameObject)))
        {
            var go = LuaScriptMgr.GetUnityObject<GameObject>(L, 1);
            if (go == null)
            {
                HobaDebuger.LogWarning("DeactivateVideoUnit: param 1 GameObject got null");
                return GameUtilWrap.CheckReturnNum(L, count, nRet);
            }
            var videoPlayer = go.GetComponent<UnityEngine.Video.VideoPlayer>();
            if (videoPlayer == null)
            {
                HobaDebuger.LogWarning("DeactivateVideoUnit: param 1 GameObject must has a VideoPlayer");
                return GameUtilWrap.CheckReturnNum(L, count, nRet);
            }
            //videoPlayer.frame = 1;
            videoPlayer.Pause();
        }
        else
        {
            HobaDebuger.LogError("DeactivateVideoUnit's param is WRONG");
        }
        return GameUtilWrap.CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int ReleaseVideoUnit(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 1 && LuaScriptMgr.CheckTypes(L, 1, typeof(GameObject)))
        {
            var go = LuaScriptMgr.GetUnityObject<GameObject>(L, 1);
            if (go == null)
            {
                HobaDebuger.LogWarning("ReleaseVideoUnit: param 1 GameObject got null");
                return GameUtilWrap.CheckReturnNum(L, count, nRet);
            }
            var videoPlayer = go.GetComponent<UnityEngine.Video.VideoPlayer>();
            if (videoPlayer == null)
            {
                HobaDebuger.LogWarning("ReleaseVideoUnit: param 1 GameObject must has a VideoPlayer");
                return GameUtilWrap.CheckReturnNum(L, count, nRet);
            }
            var targetTexture = videoPlayer.targetTexture;
            videoPlayer.Stop();
            videoPlayer.targetTexture = null;
            //GameObject.Destroy(videoPlayer);
            if (targetTexture != null)
            {
                RenderTexture rt = RenderTexture.active;
                RenderTexture.active = targetTexture;
                GL.Clear(false, true, Color.clear);
                RenderTexture.active = rt;
                RenderTexture.ReleaseTemporary(targetTexture);
            }
        }
        else
        {
            HobaDebuger.LogError("ReleaseVideoUnit's param is WRONG");
        }
        return GameUtilWrap.CheckReturnNum(L, count, nRet);
    }

    #endregion

    #region Template


    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int SetTemplatePath(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 3 && LuaScriptMgr.CheckTypes(L, 1, typeof(string), typeof(string), typeof(string)))
        {
            string basePath = LuaScriptMgr.GetString(L, 1);
            string binPath = LuaScriptMgr.GetString(L, 2);
            string localePath = LuaScriptMgr.GetString(L, 3);

            Template.Path.BasePath = basePath;
            Template.Path.BinPath = binPath;
            Template.Path.LocalePath = localePath;
        }
        else
        {
            LogParamError("SetTemplatePath", count);
        }

        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int AddSpecialTemplateDataPath(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 2 && LuaScriptMgr.CheckTypes(L, 1, typeof(string), typeof(LuaTable)))
        {
            string templateName = LuaScriptMgr.GetString(L, 1);
            string[] filePathArray = LuaScriptMgr.GetArrayString(L, 2);

            Template.TemplateDataManagerCollection.AddSpecialTemplateDataPath(templateName, filePathArray);
            return CheckReturnNum(L, count, nRet);
        }
        
        {
            LogParamError("AddSpecialTemplateDataPath", count);
            return CheckReturnNum(L, count, nRet);
        }
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int AddPreloadTemplateData(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 1 && LuaScriptMgr.CheckTypes(L, 1, typeof(string)))
        {
            string templateName = LuaScriptMgr.GetString(L, 1);

            Template.TemplateDataManagerCollection.AddPreloadTemplateDataPath(templateName);
            return CheckReturnNum(L, count, nRet);
        }

        {
            LogParamError("AddPreloadTemplateData", count);
            return CheckReturnNum(L, count, nRet);
        }
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int PreloadGameData(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;
        if (count == 0)
            EntryPoint.Instance.StartPrelaodDataCoroutine();

        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int GetTemplateData(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 1;

        if ((count == 2 && LuaScriptMgr.CheckTypes(L, 1, typeof(string), typeof(int))))
        {
            string templateSpace = LuaDLL.lua_tostring(L, 1);
            int tid = (int)LuaDLL.lua_tonumber(L, 2);

            Template.ITemplateDataManager man = Template.TemplateDataManagerCollection.GetTemplateDataManager(templateSpace);
            if (man != null)
            {
                if (templateSpace == "Actor")          //Actor特殊处理
                {
                    if (tid <= 0)
                    {
                        // tid == 0的Actor为空Actor,占位用
                        LuaDLL.lua_pushnil(L);
                        return CheckReturnNum(L, count, nRet);
                    }
                }

                byte[] data = man.GetTemplateData(tid);
                if (data != null)
                {
                    LuaDLL.lua_pushlstring(L, data, data.Length);
                    return CheckReturnNum(L, count, nRet);
                }
                else
                {
                    LuaDLL.lua_pushnil(L);
                    return CheckReturnNum(L, count, nRet);
                }
            }
            else
            {
                HobaDebuger.LogWarningFormat("Template space name is not defined :{0}", templateSpace);
                LuaDLL.lua_pushnil(L);
                return CheckReturnNum(L, count, nRet);
            }
        }
        else
        {
            LogParamError("GetTemplateData", count);
            LuaDLL.lua_pushnil(L);
            return CheckReturnNum(L, count, nRet);
        }
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int GetAllTid(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 1;

        if (count != 1 || !LuaScriptMgr.CheckTypes(L, 1, typeof(string)))
        {
            LogParamError("GetAllTid", count);

            LuaDLL.lua_pushnil(L);
            return CheckReturnNum(L, count, nRet);
        }

        var templateSpace = LuaScriptMgr.GetString(L, 1);

        //Debug.LogErrorFormat("GetAllTids {0} {1}", templateSpace, Time.frameCount);

        Template.ITemplateDataManager man = Template.TemplateDataManagerCollection.GetTemplateDataManager(templateSpace);

        var map = man.GetTemplateDataMap();
        if (map != null)
        {
            if (map.Count > 0)
            {
                LuaDLL.lua_newtable(L);

                using (var iter = map.GetEnumerator())
                {
                    int key = 1;
                    while (iter.MoveNext())
                    {
                        LuaScriptMgr.Push(L, key);
                        LuaScriptMgr.Push(L, iter.Current.Key);
                        LuaDLL.lua_settable(L, -3);
                        key++;
                    }
                }
            }
            else
            {
                LuaDLL.lua_pushnil(L);
            }
        }
        else
        {
            LuaDLL.lua_pushnil(L);
        }

        return CheckReturnNum(L, count, nRet);
    }

    #endregion

    #region Navmesh
    static readonly Vector3 _PolyPickExt = new Vector3(1, 512, 1);

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int CanNavigateToXYZ(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 1;

        if (count == 7 && LuaScriptMgr.CheckTypes(L, 2, typeof(float), typeof(float), typeof(float), typeof(float), typeof(float), typeof(float)))
        {
            var navmeshName = ScenesManager.Instance.GetCurrentNavmeshName();
            if (LuaDLL.lua_isstring(L, 1))
                navmeshName = LuaScriptMgr.GetString(L, 1);
            float startPosX = (float)LuaScriptMgr.GetNumber(L, 2);
            float startPosY = (float)LuaScriptMgr.GetNumber(L, 3);
            float startPosZ = (float)LuaScriptMgr.GetNumber(L, 4);
            float endPosX = (float)LuaScriptMgr.GetNumber(L, 5);
            float endPosY = (float)LuaScriptMgr.GetNumber(L, 6);
            float endPosZ = (float)LuaScriptMgr.GetNumber(L, 7);

            var vec0 = new Vector3(startPosX, startPosY, startPosZ);
            var vec1 = new Vector3(endPosX, endPosY, endPosZ);

            bool ret = NavMeshManager.Instance.CanNavigateTo(navmeshName, vec0, vec1, _PolyPickExt, 0.5f, 0.1f);
            LuaScriptMgr.Push(L, ret);
        }
        else if (count == 8 && LuaScriptMgr.CheckTypes(L, 2, typeof(float), typeof(float), typeof(float), typeof(float), typeof(float), typeof(float), typeof(float)))
        {
            var navmeshName = ScenesManager.Instance.GetCurrentNavmeshName();
            if (LuaDLL.lua_isstring(L, 1))
                navmeshName = LuaScriptMgr.GetString(L, 1);
            float startPosX = (float)LuaScriptMgr.GetNumber(L, 2);
            float startPosY = (float)LuaScriptMgr.GetNumber(L, 3);
            float startPosZ = (float)LuaScriptMgr.GetNumber(L, 4);
            float endPosX = (float)LuaScriptMgr.GetNumber(L, 5);
            float endPosY = (float)LuaScriptMgr.GetNumber(L, 6);
            float endPosZ = (float)LuaScriptMgr.GetNumber(L, 7);
            float step = (float)LuaScriptMgr.GetNumber(L, 8);

            var vec0 = new Vector3(startPosX, startPosY, startPosZ);
            var vec1 = new Vector3(endPosX, endPosY, endPosZ);

            bool ret = NavMeshManager.Instance.CanNavigateTo(navmeshName, vec0, vec1, _PolyPickExt, step, 0.1f);
            LuaScriptMgr.Push(L, ret);
        }
        else if (count == 9 && LuaScriptMgr.CheckTypes(L, 2, typeof(float), typeof(float), typeof(float), typeof(float), typeof(float), typeof(float), typeof(float), typeof(float)))
        {
            var navmeshName = ScenesManager.Instance.GetCurrentNavmeshName();
            if (LuaDLL.lua_isstring(L, 1))
                navmeshName = LuaScriptMgr.GetString(L, 1);
            float startPosX = (float)LuaScriptMgr.GetNumber(L, 2);
            float startPosY = (float)LuaScriptMgr.GetNumber(L, 3);
            float startPosZ = (float)LuaScriptMgr.GetNumber(L, 4);
            float endPosX = (float)LuaScriptMgr.GetNumber(L, 5);
            float endPosY = (float)LuaScriptMgr.GetNumber(L, 6);
            float endPosZ = (float)LuaScriptMgr.GetNumber(L, 7);
            float step = (float)LuaScriptMgr.GetNumber(L, 8);
            float slop = (float)LuaScriptMgr.GetNumber(L, 9);

            var vec0 = new Vector3(startPosX, startPosY, startPosZ);
            var vec1 = new Vector3(endPosX, endPosY, endPosZ);

            bool ret = NavMeshManager.Instance.CanNavigateTo(navmeshName, vec0, vec1, _PolyPickExt, step, slop);
            LuaScriptMgr.Push(L, ret);
        }
        else
        {
            LogParamError("CanNavigateToXYZ", count);
            LuaScriptMgr.Push(L, false);
        }
        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int GetPointInPath(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 1;

        if (count == 1 && LuaScriptMgr.CheckTypes(L, 1, typeof(float)))
        {
            var fDistanceCompleted = (float)LuaScriptMgr.GetNumber(L, 1);

            Vector3 vPos;
            bool ret = CHostPathFindingInfo.Instance.GetPointInPath(fDistanceCompleted, out vPos);

            if (ret)
                LuaScriptMgr.Push(L, vPos);
            else
                LuaDLL.lua_pushnil(L);

            return CheckReturnNum(L, count, nRet);
        }
        else
        {
            LogParamError("GetPointInPath", count);
            LuaDLL.lua_pushnil(L);
        }
        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int GetCurrentCompleteDistance(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 1;

        float fDistanceCompleted = Main.HostPalyer != null ? CHostPathFindingInfo.Instance.DistanceCompleted : 0.0f;
        LuaScriptMgr.Push(L, fDistanceCompleted);

        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int GetCurrentTotalDistance(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 1;

        float fDistanceTotal = Main.HostPalyer != null ? CHostPathFindingInfo.Instance.TotalDistance : 0.0f;
        LuaScriptMgr.Push(L, fDistanceTotal);

        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int GetCurrentTargetPos(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 1;

        if (Main.HostPalyer != null)
        {
            ObjectBehaviour comp = Main.HostPalyer.GetComponent<ObjectBehaviour>();
            if (comp == null)
            {
                //HobaDebuger.LogError("GetCurrentCompleteDistance's GameObject must have ObjectBehaviour");
                LuaDLL.lua_pushnil(L);
                return CheckReturnNum(L, count, nRet);
            }

            MoveBehavior moveBehavior = comp.GetActiveBehavior(BehaviorType.Move) as MoveBehavior;
            if (moveBehavior == null)
            {
                //HobaDebuger.LogError("GetCurrentCompleteDistance's ObjectBehaviour does not have MoveBehavior");
                LuaDLL.lua_pushnil(L);
                return CheckReturnNum(L, count, nRet);
            }

            LuaScriptMgr.Push(L, moveBehavior.TargetPos);
        }
        else
        {
            LuaDLL.lua_pushnil(L);
        }

        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int GetAllPointsInNavMesh(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 1;

        if (count == 5 && LuaScriptMgr.CheckTypes(L, 1, typeof(string), typeof(LuaTable), typeof(LuaTable), typeof(float), typeof(float)))
        {
            var navmesh_name = LuaScriptMgr.GetString(L, 1);
            var startPos = LuaScriptMgr.GetVector3(L, 2);
            var endPos = LuaScriptMgr.GetVector3(L, 3);
            var step = (float)LuaScriptMgr.GetNumber(L, 4);
            var slop = (float)LuaScriptMgr.GetNumber(L, 5);

            var tempAllPoints = NavMeshManager.TempAllPoints;
            int outVertNum = 0;
            if (NavMeshManager.Instance.GetWayPoints(navmesh_name, startPos, endPos, _PolyPickExt, step, slop, tempAllPoints, out outVertNum))
            {
                int vert_count = outVertNum;
                if (vert_count > 0)
                {
                    LuaDLL.lua_newtable(L);
                    for (int i = 0; i < vert_count; i++)
                    {
                        LuaScriptMgr.Push(L, i + 1);
                        LuaScriptMgr.Push(L, new Vector3(tempAllPoints[i * 3], tempAllPoints[i * 3 + 1], tempAllPoints[i * 3 + 2]));
                        LuaDLL.lua_settable(L, -3);
                    }
                }
                else
                {
                    HobaDebuger.LogWarningFormat("path point count less than 1.");
                    LuaDLL.lua_pushnil(L);
                }
            }
            else
            {
                HobaDebuger.LogWarningFormat("GetPathFindFollowPoints faild.");
                LuaDLL.lua_pushnil(L);
            }
        }
        else
        {
            LogParamError("GetAllPointsInNavMesh", count);
            LuaDLL.lua_pushnil(L);
        }
        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int GetNavDistOfTwoPoint(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 1;

        if (count == 2 && LuaScriptMgr.CheckTypes(L, 1, typeof(LuaTable), typeof(LuaTable)))
        {
            Vector3 startPos = LuaScriptMgr.GetVector3(L, 1);
            Vector3 endPos = LuaScriptMgr.GetVector3(L, 2);

            float fDistance;
            if (NavMeshManager.Instance.GetNavDistOfTwoPoint(out fDistance, startPos, endPos, _PolyPickExt, 0.5f, 0.1f))
            {
                LuaScriptMgr.Push(L, fDistance);
            }
            else
            {
                HobaDebuger.LogWarningFormat("calculate path faild. start pos is :" + startPos.ToString() + ",end pos is :" + endPos.ToString());
                LuaScriptMgr.Push(L, 0.0f);
            }
        }
        else if (count == 4 && LuaScriptMgr.CheckTypes(L, 1, typeof(LuaTable), typeof(LuaTable), typeof(float), typeof(float)))
        {
            Vector3 startPos = LuaScriptMgr.GetVector3(L, 1);
            Vector3 endPos = LuaScriptMgr.GetVector3(L, 2);
            float step = (float)LuaScriptMgr.GetNumber(L, 3);
            float slop = (float)LuaScriptMgr.GetNumber(L, 4);

            float fDistance;
            if (NavMeshManager.Instance.GetNavDistOfTwoPoint(out fDistance, startPos, endPos, _PolyPickExt, step, slop))
            {
                LuaScriptMgr.Push(L, fDistance);
            }
            else
            {
                HobaDebuger.LogWarningFormat("calculate path faild. start pos is :" + startPos.ToString() + ",end pos is :" + endPos.ToString());
                LuaScriptMgr.Push(L, 0.0f);
            }
        }
        else
        {
            LogParamError("GetNavDistOfTwoPoint", count);
            LuaScriptMgr.Push(L, 0.0f);
        }
        return CheckReturnNum(L, count, nRet);
    }
    #endregion

    #region PathFinding 判断navmesh可达和阻挡

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int IsCollideWithBlockable(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 1;

        if (count == 2 && LuaScriptMgr.CheckTypes(L, 1, typeof(LuaTable), typeof(LuaTable)))
        {
            Vector3 startPos = LuaScriptMgr.GetVector3(L, 1);
            Vector3 endPos = LuaScriptMgr.GetVector3(L, 2);

            bool ret = PathFindingManager.Instance.IsCollideWithBlockable(startPos, endPos);
            LuaDLL.lua_pushboolean(L, ret);
        }
        else
        {
            LogParamError("IsCollideWithBlockable", count);
            LuaDLL.lua_pushboolean(L, false);
        }
        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int PathFindingIsConnected(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 1;

        if (count == 2 && LuaScriptMgr.CheckTypes(L, 1, typeof(LuaTable), typeof(LuaTable)))
        {
            Vector3 startPos = LuaScriptMgr.GetVector3(L, 1);
            Vector3 endPos = LuaScriptMgr.GetVector3(L, 2);

            bool ret = PathFindingManager.Instance.IsConnected(startPos, endPos);
            LuaDLL.lua_pushboolean(L, ret);
        }
        else
        {
            LogParamError("PathFindingIsConnected", count);
            LuaDLL.lua_pushboolean(L, false);
        }
        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int PathFindingIsConnectedWithPoint(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 2;

        if (count == 2 && LuaScriptMgr.CheckTypes(L, 1, typeof(LuaTable), typeof(LuaTable)))
        {
            Vector3 startPos = LuaScriptMgr.GetVector3(L, 1);
            Vector3 endPos = LuaScriptMgr.GetVector3(L, 2);

            Vector3 hitPos;
            bool ret = PathFindingManager.Instance.IsConnected(startPos, endPos, out hitPos);
            LuaDLL.lua_pushboolean(L, ret);

            if (ret)
                LuaDLL.lua_pushnil(L);
            else
                LuaScriptMgr.Push(L, hitPos);
        }
        else
        {
            LogParamError("PathFindingIsConnected", count);
            LuaDLL.lua_pushboolean(L, false);
            LuaDLL.lua_pushnil(L);
        }
        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int PathFindingCanNavigateTo(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 1;

        if (count == 2 && LuaScriptMgr.CheckTypes(L, 1, typeof(LuaTable), typeof(LuaTable)))
        {
            Vector3 startPos = LuaScriptMgr.GetVector3(L, 1);
            Vector3 endPos = LuaScriptMgr.GetVector3(L, 2);

            bool ret = PathFindingManager.Instance.CanNavigateTo(startPos, endPos, _PolyPickExt);
            LuaDLL.lua_pushboolean(L, ret);
        }
        else if (count == 3 && LuaScriptMgr.CheckTypes(L, 1, typeof(LuaTable), typeof(LuaTable), typeof(float)))
        {
            Vector3 startPos = LuaScriptMgr.GetVector3(L, 1);
            Vector3 endPos = LuaScriptMgr.GetVector3(L, 2);
            float step = (float)LuaScriptMgr.GetNumber(L, 3);

            bool ret = PathFindingManager.Instance.CanNavigateTo(startPos, endPos, _PolyPickExt, step);
            LuaDLL.lua_pushboolean(L, ret);
        }
        else
        {
            LogParamError("PathFindingCanNavigateTo", count);
            LuaDLL.lua_pushboolean(L, false);
        }
        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int PathFindingCanNavigateToXYZ(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 1;

        if (count == 6 && LuaScriptMgr.CheckTypes(L, 1, typeof(float), typeof(float), typeof(float), typeof(float), typeof(float), typeof(float)))
        {
            float startPosX = (float)LuaScriptMgr.GetNumber(L, 1);
            float startPosY = (float)LuaScriptMgr.GetNumber(L, 2);
            float startPosZ = (float)LuaScriptMgr.GetNumber(L, 3);
            float endPosX = (float)LuaScriptMgr.GetNumber(L, 4);
            float endPosY = (float)LuaScriptMgr.GetNumber(L, 5);
            float endPosZ = (float)LuaScriptMgr.GetNumber(L, 6);

            var startPos = new Vector3(startPosX, startPosY, startPosZ);
            var endPos = new Vector3(endPosX, endPosY, endPosZ);

            bool ret = PathFindingManager.Instance.CanNavigateTo(startPos, endPos, _PolyPickExt);
            LuaDLL.lua_pushboolean(L, ret);
        }
        else if (count == 7 && LuaScriptMgr.CheckTypes(L, 1, typeof(float), typeof(float), typeof(float), typeof(float), typeof(float), typeof(float), typeof(float)))
        {
            float startPosX = (float)LuaScriptMgr.GetNumber(L, 1);
            float startPosY = (float)LuaScriptMgr.GetNumber(L, 2);
            float startPosZ = (float)LuaScriptMgr.GetNumber(L, 3);
            float endPosX = (float)LuaScriptMgr.GetNumber(L, 4);
            float endPosY = (float)LuaScriptMgr.GetNumber(L, 5);
            float endPosZ = (float)LuaScriptMgr.GetNumber(L, 6);
            float step = (float)LuaScriptMgr.GetNumber(L, 7);

            var startPos = new Vector3(startPosX, startPosY, startPosZ);
            var endPos = new Vector3(endPosX, endPosY, endPosZ);

            bool ret = PathFindingManager.Instance.CanNavigateTo(startPos, endPos, _PolyPickExt, step);
            LuaDLL.lua_pushboolean(L, ret);
        }
        else
        {
            LogParamError("PathFindingCanNavigateToXYZ", count);
            LuaDLL.lua_pushboolean(L, false);
        }
        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int FindFirstConnectedPoint(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 2;

        if (count == 2 && LuaScriptMgr.CheckTypes(L, 1, typeof(LuaTable), typeof(LuaTable)))
        {
            Vector3 startPos = LuaScriptMgr.GetVector3(L, 1);
            Vector3 endPos = LuaScriptMgr.GetVector3(L, 2);

            Vector3 selectPos;
            if (PathFindingManager.Instance.FindFirstConnectedPoint(startPos, endPos, _PolyPickExt, 1.0f, out selectPos))
            {
                LuaDLL.lua_pushboolean(L, true);
                LuaScriptMgr.Push(L, selectPos);
            }
            else
            {
                LuaDLL.lua_pushboolean(L, false);
                LuaDLL.lua_pushnil(L);
            }
        }
        else
        {
            LogParamError("FindFirstConnectedPoint", count);
            LuaDLL.lua_pushboolean(L, false);
            LuaDLL.lua_pushnil(L);
        }

        return CheckReturnNum(L, count, nRet);
    }


    #endregion

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int SetCurrentMapInfo(IntPtr L)
    {
        int sceneTid = (int)LuaScriptMgr.GetNumber(L, 1);
        int mapId = (int)LuaScriptMgr.GetNumber(L, 2);
        var navmeshNme = LuaScriptMgr.GetString(L, 3);
        ScenesManager.Instance.UpdataSceneInfo(sceneTid, mapId, navmeshNme);

        var navmesh_name = navmeshNme;
        if (!string.IsNullOrEmpty(navmesh_name))
        {
            // 加载NavMesh数据
            if (navmeshNme != NavMeshManager.Instance.NavMeshName && !NavMeshManager.Instance.Init(navmesh_name))
                HobaDebuger.LogErrorFormat("NavMeshMan failed to init: {0}", navmesh_name);

            // 加载其他Region数据
            var footstep_name = System.IO.Path.ChangeExtension(navmesh_name, "footstepregion");
            if (footstep_name != PathFindingManager.Instance.FootStepFileName && !PathFindingManager.Instance.Init(footstep_name))
                HobaDebuger.LogErrorFormat("PathFindingManager failed to init: {0}", footstep_name);
        }

        return 0;
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int GetCurrentMapId(IntPtr L)
    {
        int sceneTid = ScenesManager.Instance.GetCurrentMapID();
        LuaScriptMgr.Push(L, sceneTid);
        return 1;
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int GetCurrentSceneTid(IntPtr L)
    {
        int sceneTid = ScenesManager.Instance.GetCurrentSceneTid();
        LuaScriptMgr.Push(L, sceneTid);
        return 1;
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int EnableBackUICamera(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 1 && LuaScriptMgr.CheckTypes(L, 1, typeof(bool)))
        {
            bool bShow = LuaScriptMgr.GetBoolean(L, 1);

            GameObject go = GameObjectUtil.GetBackUICamera();
            if (go != null)
                go.SetActive(bShow);

        }
        else
        {
            LogParamError("EnableBackUICamera", count);
        }

        return GameUtilWrap.CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int GetChargeDistance(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 1;

        if (count == 3 && LuaScriptMgr.CheckTypes(L, 1, typeof(LuaTable), typeof(LuaTable), typeof(float)/*, typeof(float), typeof(float)*/))
        {
            var srcPos = LuaScriptMgr.GetVector3(L, 1);
            var dir = LuaScriptMgr.GetVector3(L, 2);
            var maxDis = (float)LuaScriptMgr.GetNumber(L, 3);
            var distance = Util.GetMaxValidDistance(srcPos, dir, maxDis);

            LuaDLL.lua_pushnumber(L, distance);
            return CheckReturnNum(L, count, nRet);
        }
        else
        {
            LogParamError("GetChargeDistance", count);
            LuaDLL.lua_pushnumber(L, 0);
            return CheckReturnNum(L, count, nRet);
        }
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int SetLayoutElementPreferredSize(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 3 && LuaScriptMgr.CheckTypes(L, 1, typeof(GameObject), typeof(float), typeof(float)))
        {
            GameObject obj = LuaScriptMgr.GetUnityObject<GameObject>(L, 1);
            if (obj == null)
                return CheckReturnNum(L, count, nRet);

            LayoutElement layoutElement = obj.GetComponent<LayoutElement>();
            if (layoutElement == null)
                return CheckReturnNum(L, count, nRet);

            float width = (float)LuaScriptMgr.GetNumber(L, 2);
            float height = (float)LuaScriptMgr.GetNumber(L, 3);

            if (width > 0)
                layoutElement.preferredWidth = width;

            if (height > 0)
                layoutElement.preferredHeight = height;

            return CheckReturnNum(L, count, nRet);
        }
        else
        {
            LogParamError("SetLoadDistScale", count);
        }

        return GameUtilWrap.CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int Test(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;
        var dateTime = System.DateTime.Now.AddSeconds(10);
        int hour = dateTime.Hour;
        int minute = dateTime.Minute;
        Notification.LocalNotification.RegisterNotificationMessage("测试标题", "What a fucking nice day.", hour, minute, true);

        return GameUtilWrap.CheckReturnNum(L, count, nRet);
    }
}
