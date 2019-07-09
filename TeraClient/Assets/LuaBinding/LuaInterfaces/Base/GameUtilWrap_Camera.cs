using UnityEngine;
using System;
using LuaInterface;
using Common;

public static partial class GameUtilWrap
{
    #region Params
    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int SetCameraParams(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 1 && LuaScriptMgr.CheckTypes(L, 1, typeof(int)))
        {
            CAM_CTRL_MODE mode = (CAM_CTRL_MODE)LuaScriptMgr.GetNumber(L, 1);
            CCamCtrlMan.Instance.SetCurCamCtrl(mode, true);
        }
        else if (count == 6 && LuaScriptMgr.CheckTypes(L, 1, typeof(int), typeof(bool), typeof(GameObject), typeof(int), typeof(LuaTable), typeof(LuaFunction)))
        {
            //CAM_CTRL_MODE mode = (CAM_CTRL_MODE)LuaScriptMgr.GetNumber(L, 1);
            //var adjust_immediatelly = LuaScriptMgr.GetBoolean(L, 2);
            GameObject npc_obj = LuaScriptMgr.GetUnityObject<GameObject>(L, 3);
            int look_at_type = (int)LuaScriptMgr.GetNumber(L, 4);
            Vector3 destDir = LuaScriptMgr.GetVector3(L, 5);

            if (npc_obj == null)
            {
                HobaDebuger.LogError("the target object you wanna look at is null. => GameUtil.SetCameraParams");
                return CheckReturnNum(L, count, nRet);
            }

            LuaFunction callback = LuaScriptMgr.GetLuaFunction(L, 6);
            CCamCtrlMan.Instance.SetNpcInfo(destDir);
            CCamCtrlMan.Instance.SetCurCamCtrl(CAM_CTRL_MODE.NPC, true, npc_obj, look_at_type, callback);
        }
        else if (count == 4 && LuaScriptMgr.CheckTypes(L, 1, typeof(int), typeof(GameObject), typeof(int), typeof(LuaFunction)))
        {
            CAM_CTRL_MODE mode = (CAM_CTRL_MODE)LuaScriptMgr.GetNumber(L, 1);
            GameObject look_at_target = LuaScriptMgr.GetUnityObject<GameObject>(L, 2);
            int look_at_type = (int)LuaScriptMgr.GetNumber(L, 3);

            if (look_at_target == null)
            {
                HobaDebuger.LogError("the target object you wanna look at is null. => GameUtil.SetCameraParams");
                return CheckReturnNum(L, count, nRet);
            }

            LuaFunction callback = LuaScriptMgr.GetLuaFunction(L, 4);
            CCamCtrlMan.Instance.SetCurCamCtrl(mode, true, look_at_target, look_at_type, callback);
        }
        else if (count == 5 && LuaScriptMgr.CheckTypes(L, 1, typeof(int), typeof(bool), typeof(GameObject), typeof(int), typeof(LuaFunction)))
        {
            CAM_CTRL_MODE mode = (CAM_CTRL_MODE)LuaScriptMgr.GetNumber(L, 1);
            bool adjust_immediatelly = LuaScriptMgr.GetBoolean(L, 2);
            GameObject target = LuaScriptMgr.GetUnityObject<GameObject>(L, 3);
            int look_at_type = (int)LuaScriptMgr.GetNumber(L, 4);
            LuaFunction callback = LuaScriptMgr.GetLuaFunction(L, 5);

            CCamCtrlMan.Instance.SetCurCamCtrl(mode, adjust_immediatelly, target, look_at_type, callback);
        }
        else
        {
            LogParamError("SetCameraParams", count);
        }
        return CheckReturnNum(L, count, nRet);
    }

    //设置镜头额外参数，有需要在SetCameraParams之前调用
    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int SetCameraParamsEX(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count > 1 && LuaScriptMgr.CheckType(L, typeof(int), 1))
        {
            CAM_CTRL_MODE mode = (CAM_CTRL_MODE)LuaScriptMgr.GetNumber(L, 1);
            switch (mode)
            {
                case CAM_CTRL_MODE.DUNGEON:
                    if (count == 4 && LuaScriptMgr.CheckTypes(L, 2, typeof(Vector3), typeof(Vector3), typeof(float)))
                    {
                        Vector3 dest_pos = LuaScriptMgr.GetVector3(L, 2);
                        Vector3 dest_rot = LuaScriptMgr.GetVector3(L, 3);
                        float fov = (float)LuaScriptMgr.GetNumber(L, 4);
                        CCamCtrlMan.Instance.SetDungeonCamParam(dest_pos, dest_rot, fov);
                    }
                    else
                    {
                        LogParamError("SetCameraParamsEX DUNGEON should be with params of : pos, rot, fov. ", count);
                    }
                    break;
            }
        }
        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int SetGameCamHeightOffsetInterval(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 3 && LuaScriptMgr.CheckTypes(L, 1, typeof(float), typeof(float), typeof(bool)))
        {
            float fHeightOffsetMin = (float)LuaScriptMgr.GetNumber(L, 1);
            float fHeightOffsetMax = (float)LuaScriptMgr.GetNumber(L, 2);
            bool isImmediately = LuaScriptMgr.GetBoolean(L, 3);
            CCamCtrlMan.Instance.GetGameCamCtrl().SetHeightOffsetInterval(fHeightOffsetMin, fHeightOffsetMax, isImmediately);
        }
        else
        {
            LogParamError("SetGameCamHeightOffsetInterval", count);
        }
        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int SetGameCamOwnDestDistOffset(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 2 && LuaScriptMgr.CheckTypes(L, 1, typeof(float), typeof(bool)))
        {
            float fDistOffset = (float)LuaScriptMgr.GetNumber(L, 1);
            bool bImmediate = LuaScriptMgr.GetBoolean(L, 2);
            CCamCtrlMan.Instance.GetGameCamCtrl().SetOwnDestDistOffset(fDistOffset, bImmediate);
        }
        else
        {
            LogParamError("SetGameCamOwnDestDistOffset", count);
        }
        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int SetGameCamDefaultDestDistOffset(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 1 && LuaScriptMgr.CheckTypes(L, 1, typeof(bool)))
        {
            bool bImmediate = LuaScriptMgr.GetBoolean(L, 1);
            CCamCtrlMan.Instance.GetGameCamCtrl().SetDefaultDestDistOffset(bImmediate);
        }
        else
        {
            LogParamError("SetGameCamDefaultDestDistOffset", count);
        }
        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int SetGameCamDestDistOffset(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 2 && LuaScriptMgr.CheckTypes(L, 1, typeof(float), typeof(bool)))
        {
            float fDist = (float)LuaScriptMgr.GetNumber(L, 1);
            bool bImmediate = LuaScriptMgr.GetBoolean(L, 2);

            CCamCtrlMan.Instance.GetGameCamCtrl().SetDestDistOffset(fDist, bImmediate);
        }
        else
        {
            LogParamError("SetGameCamDestDistOffset", count);
        }
        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int SetDestDistOffsetAndDestPitchDeg(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 4 && LuaScriptMgr.CheckTypes(L, 1, typeof(float), typeof(float), typeof(float), typeof(float)))
        {
            float fDistOffset = (float)LuaScriptMgr.GetNumber(L, 1);
            float fPitchDeg = (float)LuaScriptMgr.GetNumber(L, 2);
            float fDistOffsetSpeed = (float)LuaScriptMgr.GetNumber(L, 3);
            float fPitchSpeed = (float)LuaScriptMgr.GetNumber(L, 4);
            CCamCtrlMan.Instance.GetGameCamCtrl().SetDestDistOffsetAndDestPitchDeg(fDistOffset, fPitchDeg, fDistOffsetSpeed, fPitchSpeed);
        }
        else
        {
            LogParamError("SetDestDistOffsetAndDestPitchDeg", count);
        }
        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int SetSkillActCamMode(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;
        if (count == 6 && LuaScriptMgr.CheckTypes(L, 1, typeof(GameObject), typeof(int), typeof(float), typeof(float), typeof(float), typeof(float)))
        {
            GameObject target = LuaScriptMgr.GetUnityObject<GameObject>(L, 1);
            //SKILL_CAM_ENUM look_at_type = (SKILL_CAM_ENUM)LuaScriptMgr.GetNumber(L, 2);
            float angle = (float)LuaScriptMgr.GetNumber(L, 2);
            float offset = (float)LuaScriptMgr.GetNumber(L, 3);
            float duration = (float)LuaScriptMgr.GetNumber(L, 4);
            float ori_angle = (float)LuaScriptMgr.GetNumber(L, 5);
            CCamCtrlMan.Instance.SetSkillActInfo(angle, offset, duration, ori_angle/*, look_at_type*/);
            CCamCtrlMan.Instance.SetCurCamCtrl(CAM_CTRL_MODE.SKILLACT, true, target, 1);
        }
        else
        {
            LogParamError("SetSkillActCamMode", count);
        }
        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int GetGameCamDestDistOffset(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 1;

        if (count == 0)
        {
            float dist = CCamCtrlMan.Instance.GetGameCamCtrl().DistOffsetDest;
            LuaDLL.lua_pushnumber(L, dist);
        }
        else
        {
            LogParamError("GetGameCamDestDistOffset", count);
            LuaDLL.lua_pushnumber(L, 0.0f);
        }
        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int GetGameCamCurDistOffset(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 1;

        if (count == 0)
        {
            float dist = CCamCtrlMan.Instance.GetGameCamCtrl().DistOffset;
            LuaDLL.lua_pushnumber(L, dist);
        }
        else
        {
            LogParamError("GetGameCamCurDistOffset", count);
            LuaDLL.lua_pushnumber(L, 0.0f);
        }
        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int SetGameCamCtrlParams(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 2 && LuaScriptMgr.CheckTypes(L, 1, typeof(bool), typeof(int)))
        {
            var isLockedDir = LuaScriptMgr.GetBoolean(L, 1);
            var priority = (int)LuaScriptMgr.GetNumber(L, 2);
            CCamCtrlMan.Instance.GetGameCamCtrl().SetCamParams(isLockedDir, priority);
        }
        else
        {
            LogParamError("SetGameCamCtrlParams", count);
        }
        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int AddOrSubForTest(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 2 && LuaScriptMgr.CheckTypes(L, 1, typeof(int), typeof(bool)))
        {
            int type = (int)LuaScriptMgr.GetNumber(L, 1);
            bool isAdd = LuaScriptMgr.GetBoolean(L, 2);
            CCamCtrlMan.Instance.AddOrSubForTest(type, isAdd);
        }
        else
        {
            LogParamError("AddOrSubForTest", count);
        }

        return GameUtilWrap.CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int SetExteriorDebugParams(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 4 && LuaScriptMgr.CheckTypes(L, 1, typeof(float), typeof(float), typeof(float), typeof(float)))
        {
            float yaw_deg = (float)LuaScriptMgr.GetNumber(L, 1);
            float pitch_deg = (float)LuaScriptMgr.GetNumber(L, 2);
            float distance = (float)LuaScriptMgr.GetNumber(L, 3);
            float height = (float)LuaScriptMgr.GetNumber(L, 4);
            CCamCtrlMan.Instance.SetExteriorDebugParams(yaw_deg, pitch_deg, distance, height);
        }
        else
        {
            LogParamError("SetExteriorDebugParams", count);
        }

        return GameUtilWrap.CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int SetExteriorCamParams(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 5 && LuaScriptMgr.CheckTypes(L, 1, typeof(float), typeof(float), typeof(float), typeof(float), typeof(float)))
        {
            float yaw_deg = (float)LuaScriptMgr.GetNumber(L, 1);
            float pitch_deg = (float)LuaScriptMgr.GetNumber(L, 2);
            float distance = (float)LuaScriptMgr.GetNumber(L, 3);
            float height = (float)LuaScriptMgr.GetNumber(L, 4);
            float min_distance = (float)LuaScriptMgr.GetNumber(L, 5);
            CCamCtrlMan.Instance.SetExteriorCamParams(yaw_deg, pitch_deg, distance, height, min_distance);
        }
        else
        {
            LogParamError("SetExteriorCamParams", count);
        }

        return GameUtilWrap.CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int SetExteriorCamHeightOffset(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 1 && LuaScriptMgr.CheckTypes(L, 1, typeof(float)))
        {
            float height = (float)LuaScriptMgr.GetNumber(L, 1);
            CCamCtrlMan.Instance.SetExteriorCamHeightOffset(height);
        }
        else
        {
            LogParamError("SetExteriorCamHeightOffset", count);
        }

        return GameUtilWrap.CheckReturnNum(L, count, nRet);
    }
    #endregion

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int SetGameCamCtrlMode(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 5 && LuaScriptMgr.CheckTypes(L, 1, typeof(int), typeof(Boolean), typeof(Boolean), typeof(Boolean), typeof(Boolean)))
        {
            var m = (int)LuaScriptMgr.GetNumber(L, 1);
            if (m < 0 || m > (int)CPlayerFollowCam.CTRL_MODE.FIX25D)
            {
                HobaDebuger.LogErrorFormat("SetGameCamCtrlMode: invalid mode {0}", m);
                return CheckReturnNum(L, count, nRet);
            }

            CPlayerFollowCam.CTRL_MODE mode = (CPlayerFollowCam.CTRL_MODE)m;
            bool bChangeYaw = LuaScriptMgr.GetBoolean(L, 2);
            bool bChangePitch = LuaScriptMgr.GetBoolean(L, 3);
            bool bChangeDist = LuaScriptMgr.GetBoolean(L, 4);
            bool isImmediately = LuaScriptMgr.GetBoolean(L, 5);

            CCamCtrlMan.Instance.GetGameCamCtrl().SetCameraCtrlMode(mode, bChangeYaw, bChangePitch, bChangeDist, isImmediately);
        }
        else
        {
            LogParamError("SetGameCamCtrlMode", count);
        }
        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int GetGameCamCtrlMode(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 1;

        int mode = (int)CCamCtrlMan.Instance.GetGameCamCtrl().DestCamCtrlMode;
        LuaScriptMgr.Push(L, mode);

        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int SetTransToPortal(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 1 && LuaScriptMgr.CheckTypes(L, 1, typeof(Boolean)))
        {
            bool enter = LuaScriptMgr.GetBoolean(L, 1);
            CCamCtrlMan.Instance.IsTransToPortal = enter;
        }
        else
        {
            LogParamError("SetTransToPortal", count);
        }

        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int SetProDefaultSpeed(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 1 && LuaScriptMgr.CheckTypes(L, 1, typeof(float)))
        {
            float defaultSpeed = (float)LuaScriptMgr.GetNumber(L, 1);
            CCamCtrlMan.Instance.CurProDefaultSpeed = defaultSpeed;
        }
        else
        {
            LogParamError("SetProDefaultSpeed", count);
        }

        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int SetCamToDefault(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 4 && LuaScriptMgr.CheckTypes(L, 1, typeof(bool), typeof(bool), typeof(bool), typeof(bool)))
        {
            bool bChangeYaw = LuaScriptMgr.GetBoolean(L, 1);
            bool bChangePitch = LuaScriptMgr.GetBoolean(L, 2);
            bool bChangeDist = LuaScriptMgr.GetBoolean(L, 3);
            bool isImmediately = LuaScriptMgr.GetBoolean(L, 4);

            CCamCtrlMan.Instance.GetGameCamCtrl().SetToDefaultCamera(bChangeYaw, bChangePitch, bChangeDist, isImmediately);
        }
        else
        {
            LogParamError("SetCamToDefault", count);
        }
        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int QuickRecoverCamToDest(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 2 && LuaScriptMgr.CheckTypes(L, 1, typeof(float), typeof(float)))
        {
            if (CPlayerFollowCam.CTRL_MODE.FIX25D != CCamCtrlMan.Instance.GetGameCamCtrl().DestCamCtrlMode)     //屏蔽2.5D模式
            {
                float x = (float)LuaScriptMgr.GetNumber(L, 1);
                float z = (float)LuaScriptMgr.GetNumber(L, 2);
                CCamCtrlMan.Instance.QuickRecoverCamToDest(x, z);
            }
        }
        else
        {
            LogParamError("QuickRecoverCamToDest", count);
        }
        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int SetCamLockState(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 1 && LuaScriptMgr.CheckTypes(L, 1, typeof(bool)))
        {
            bool isLock = LuaScriptMgr.GetBoolean(L, 1);

            CCamCtrlMan.Instance.GetGameCamCtrl().SetCamFightLockState(isLock);
        }
        else if (count == 2 && LuaScriptMgr.CheckTypes(L, 1, typeof(bool), typeof(GameObject)))
        {
            bool isLock = LuaScriptMgr.GetBoolean(L, 1);
            GameObject target = LuaScriptMgr.GetUnityObject<GameObject>(L, 2);

            CCamCtrlMan.Instance.GetGameCamCtrl().SetCamFightLockState(isLock, target);
        }
        else
        {
            LogParamError("SetCamLockOrNot", count);
        }
        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int SetCameraGreyOrNot(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 1 && LuaScriptMgr.CheckTypes(L, 1, typeof(bool)))
        {
            bool isSetGrey = LuaScriptMgr.GetBoolean(L, 1);
            DynamicEffectManager.Instance.EnablePlayerDeathEffect(isSetGrey);
        }
        else
        {
            HobaDebuger.LogError("SetCameraGreyOrNot's param is WRONG");
        }
        return GameUtilWrap.CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int OpenUIWithEffect(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 1 && LuaScriptMgr.CheckTypes(L, 1, typeof(int)))
        {
            int effectID = (int)LuaScriptMgr.GetNumber(L, 1);
            DynamicEffectManager.Instance.EnterUIDynamicEffect(effectID);
        }
        else
        {
            HobaDebuger.LogError("EnterUIdynamicEffect's param is WRONG");
        }
        return GameUtilWrap.CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int LeaveUIEffect(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        DynamicEffectManager.Instance.EnterUIDynamicEffect(0);
        return GameUtilWrap.CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int CameraLookAtNpc(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 1 && LuaScriptMgr.CheckTypes(L, 1, typeof(GameObject)))
        {
            GameObject npc = LuaScriptMgr.GetUnityObject<GameObject>(L, 1);
            if (npc == null)
            {
                HobaDebuger.Log("The npc object is null.");
                return CheckReturnNum(L, count, nRet);
            }

            GameObject hang_point = npc.FindChildRecursively("Bip001 Spine1");
            if (hang_point == null)
            {
                HobaDebuger.Log("The hang point object is null.");
                return CheckReturnNum(L, count, nRet);
            }

            Transform trans_target = hang_point.transform;
            Transform trans_camera = GameObject.Find("MainCameraRoot").transform;
            Vector3 p = new Vector3(npc.transform.forward.x, 0, npc.transform.forward.z);
            trans_camera.position = trans_target.position + p * 3;
            trans_camera.LookAt(trans_target);
        }
        else
        {
            LogParamError("CameraLookAtNpc", count);
        }

        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int SetGameCam2DHeightOffset(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 1 && LuaScriptMgr.CheckTypes(L, 1, typeof(float)))
        {
            float fHeightOffset = (float)LuaScriptMgr.GetNumber(L, 1);
            //CCamCtrlMan.Instance.SetExteriorCamHeightOffset(fHeightOffset);
            CCamCtrlMan.Instance.GetGameCamCtrl().SetHeightOffset2D(fHeightOffset);
        }
        else
        {
            LogParamError("SetGameCam2DHeightOffset", count);
        }
        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int ReadNearCameraProfConfig(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 1 && LuaScriptMgr.CheckTypes(L, 1, typeof(int)))
        {
            int prof = (int)LuaScriptMgr.GetNumber(L, 1);
            EntryPoint.Instance.ReadPlayerNearCameraConfig(prof);
        }
        else
        {
            LogParamError("ReadNearCameraProfConfig", count);
        }
        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int SetNearCamProfCfg(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 1 && LuaScriptMgr.CheckTypes(L, 1, typeof(bool)))
        {
            bool bRide = LuaScriptMgr.GetBoolean(L, 1);
            CCamCtrlMan.Instance.GetNearCamCtrl().SetProfConfig(bRide);
        }
        else
        {
            LogParamError("SetNearCamProfCfg", count);
        }
        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int SetNearCamRollSensitivity(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 1 && LuaScriptMgr.CheckTypes(L, 1, typeof(float)))
        {
            float value = (float)LuaScriptMgr.GetNumber(L, 1);
            CCamCtrlMan.Instance.GetNearCamCtrl().RollSensitivity = value;
        }
        else
        {
            LogParamError("SetNearCamRollSensitivity", count);
        }
        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int EnableNearCamLookIK(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 1 && LuaScriptMgr.CheckTypes(L, 1, typeof(bool)))
        {
            bool enable = LuaScriptMgr.GetBoolean(L, 1);
            CCamCtrlMan.Instance.GetNearCamCtrl().EnableLookIK(enable);
        }
        else
        {
            LogParamError("EnableNearCamLookIK", count);
        }
        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int GetGameCamDirXZ(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 2;

        if (count == 0)
        {
            var dir = CCamCtrlMan.Instance.GetGameCamCtrl().GetCamDir();
            LuaScriptMgr.Push(L, dir.x);
            LuaScriptMgr.Push(L, dir.z);
        }
        else
        {
            LogParamError("GetGameCamDirXZ", count);
            LuaScriptMgr.Push(L, 0);
            LuaScriptMgr.Push(L, 0);
        }
        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int StartBossCamMove(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 1 && LuaScriptMgr.CheckTypes(L, 1, typeof(LuaFunction)))
        {
            LuaFunction callback = LuaScriptMgr.GetLuaFunction(L, 1);
            CCamCtrlMan.Instance.StartBossCamMove(callback);
        }
        else if (count == 2 && LuaScriptMgr.CheckTypes(L, 1, typeof(float), typeof(LuaFunction)))
        {
            float duration = (float)LuaScriptMgr.GetNumber(L, 1);
            LuaFunction callback = LuaScriptMgr.GetLuaFunction(L, 2);
            CCamCtrlMan.Instance.StartBossCamMove(callback, duration);
        }
        else
        {
            LogParamError("StartBossCamMove", count);
        }
        return CheckReturnNum(L, count, nRet);
    }
}
