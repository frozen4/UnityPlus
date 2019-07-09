using Common;

#if USING_GCLOUDVOICE && UNITY_EDITOR

using gcloud_voice;
using LuaInterface;
using System;

public class Gcloud_RealTimeVoiceNational : IVoiceModule
{
    private VoiceManager _Manager = null;
    private VoiceMode _Mode;
    private bool _bIsSpeakerOpen = false;
    private bool _bIsMicOpen = false;
    private bool _bRoomJoint = false;
    private GCloudVoiceRole _Role = GCloudVoiceRole.AUDIENCE;
    private int _MemberID = -1;

    public Gcloud_RealTimeVoiceNational(VoiceManager manager, VoiceMode mode)
    {
        _Manager = manager;
        _Mode = mode;
    }

    public bool IsStarted { get; private set; }

    public string TeamRoomName
    {
        get;
        private set;
    }

    public bool IsSpeakerOpen
    {
        get { return _bIsSpeakerOpen; }
    }

    public bool IsMicOpen
    {
        get { return _bIsMicOpen; }
    }

    public bool IsRoomJoint
    {
        get { return _bRoomJoint; }
    }

    public int MemberID
    {
        get { return _MemberID; }
    }

    public GCloudVoiceRole Role
    {
        get { return _Role; }
    }

    public VoiceMode VoiceMode { get { return _Mode; } }

    private Action<string> LOG_STRING = HobaDebuger.Log;

    public bool Start()
    {
        if (!IsStarted)
        {
            IGCloudVoice engine = _Manager.Gcloud_VoiceMan.VoiceEngine;

            engine.OnJoinRoomComplete += OnJoinRoomComplete;
            engine.OnQuitRoomComplete += OnQuitRoomComplete;
            engine.OnMemberVoice += OnMemberVoice;

            IsStarted = true;
        }

        return IsStarted;
    }

    private void OnJoinRoomComplete(IGCloudVoice.GCloudVoiceCompleteCode code, string roomName, int memberID)
    {
        //LOG_STRING("OnJoinRoomComplete c# callback");
        //LOG_STRING("OnJoinRoomComplete ret=" + code + " roomName:" + roomName + " memberID:" + memberID);
        int retCode = 0;
        if (code == IGCloudVoice.GCloudVoiceCompleteCode.GV_ON_JOINROOM_SUCC)
        {
            _MemberID = memberID;
            TeamRoomName = roomName;
            _bRoomJoint = true;
        }
        else
        {
            retCode = (int)code;
            _MemberID = -1;
            TeamRoomName = "";
            _bRoomJoint = false;
        }

        //
        IntPtr L = LuaScriptMgr.Instance.GetL();
        if (L != IntPtr.Zero)
        {
            int oldTop = LuaDLL.lua_gettop(L);
            LuaDLL.lua_getglobal(L, "Voice_OnJoinRoomComplete");
            if (!LuaDLL.lua_isnil(L, -1))
            {

                LuaScriptMgr.Push(L, retCode);
                LuaScriptMgr.Push(L, roomName);
                LuaScriptMgr.Push(L, memberID);
                if (LuaDLL.lua_pcall(L, 3, 0, 0) != 0)
                {
                    HobaDebuger.LogLuaError(LuaDLL.lua_tostring(L, -1));
                }
            }
            LuaDLL.lua_settop(L, oldTop);
        }
    }

    private void OnQuitRoomComplete(IGCloudVoice.GCloudVoiceCompleteCode code, string roomName, int memberID)
    {
        //LOG_STRING("On Quit Room With " + code);

        int retCode = 0;
        if (code == IGCloudVoice.GCloudVoiceCompleteCode.GV_ON_QUITROOM_SUCC)
        {

        }
        else
        {
            retCode = (int)code;
        }

        //call lua
        IntPtr L = LuaScriptMgr.Instance.GetL();
        if (L != IntPtr.Zero)
        {
            int oldTop = LuaDLL.lua_gettop(L);
            LuaDLL.lua_getglobal(L, "Voice_OnQuitRoomComplete");
            if (!LuaDLL.lua_isnil(L, -1))
            {
                LuaScriptMgr.Push(L, retCode);
                LuaScriptMgr.Push(L, roomName);
                LuaScriptMgr.Push(L, memberID);
                if (LuaDLL.lua_pcall(L, 3, 0, 0) != 0)
                {
                    HobaDebuger.LogLuaError(LuaDLL.lua_tostring(L, -1));
                }
            }
            LuaDLL.lua_settop(L, oldTop);
        }

        _MemberID = -1;
        TeamRoomName = "";
        _bRoomJoint = false;
    }

    private void OnMemberVoice(int[] members, int count)
    {
    }

    public int OpenMic()
    {
        if (!_bIsMicOpen)
        {
            int ret = _Manager.Gcloud_VoiceMan.VoiceEngine.OpenMic();
            if (ret == 0)
                _bIsMicOpen = true;

            return ret;
        }
        return 0;
    }

    public int CloseMic()
    {
        if (_bIsMicOpen)
        {
            int ret = _Manager.Gcloud_VoiceMan.VoiceEngine.CloseMic();
            if (ret == 0)
                _bIsMicOpen = false;

            return ret;
        }
        return 0;
    }

    public int OpenSpeaker()
    {
        if (!_bIsSpeakerOpen)
        {
            int ret = _Manager.Gcloud_VoiceMan.VoiceEngine.OpenSpeaker();
            if (ret == 0)
                _bIsSpeakerOpen = true;

            return ret;
        }
        return 0;
    }

    public int CloseSpeaker()
    {
        if (_bIsSpeakerOpen)
        {
            int ret = _Manager.Gcloud_VoiceMan.VoiceEngine.CloseSpeaker();
            if (ret == 0)
                _bIsSpeakerOpen = false;

            return ret;
        }
        return 0;
    }

    public int SetSpeakerVolume(int volume)
    {
        if (_bIsSpeakerOpen)
            return _Manager.Gcloud_VoiceMan.VoiceEngine.SetSpeakerVolume(volume);
        return 0;
    }

    public int GetSpeakerVolume()
    {
        if (_bIsSpeakerOpen)
            return _Manager.Gcloud_VoiceMan.VoiceEngine.GetSpeakerLevel();
        return 0;
    }

    public int JointNationalRoom(string strTeamRoom, GCloudVoiceRole role, int timeout)
    {
        if (!_bRoomJoint)
        {
            _Role = role;
            return _Manager.Gcloud_VoiceMan.VoiceEngine.JoinNationalRoom(strTeamRoom, role, timeout);
        }
        return 0;
    }

    public int QuitNationalRoom(int timeout)
    {
        if (_bRoomJoint)
        {
            return _Manager.Gcloud_VoiceMan.VoiceEngine.QuitRoom(TeamRoomName, timeout);
        }
        return 0;
    }

    public void Update()
    {
        if (IsStarted)
            _Manager.Gcloud_VoiceMan.VoiceEngine.Poll();
    }

    public void Stop()
    {
        if (IsStarted)
        {
            QuitNationalRoom(6000);
            CloseSpeaker();
            _MemberID = -1;
            TeamRoomName = "";
            _bRoomJoint = false;

            IGCloudVoice engine = _Manager.Gcloud_VoiceMan.VoiceEngine;

            engine.OnJoinRoomComplete -= OnJoinRoomComplete;
            engine.OnQuitRoomComplete -= OnQuitRoomComplete;
            engine.OnMemberVoice -= OnMemberVoice;

            IsStarted = false;
        }
    }
}

#endif