using UnityEngine;
using System;
using LuaInterface;
using EntityComponent;
using Common;
using EntityVisualEffect;

public static partial class GameUtilWrap
{
    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int AddObjectComponent(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count >= 5 && LuaScriptMgr.CheckTypes(L, 1, typeof(LuaTable), typeof(GameObject), typeof(int), typeof(int), typeof(double)))
        {
            LuaDLL.lua_pushvalue(L, 1);
            int ObjRef = LuaDLL.luaL_ref(L, LuaIndexes.LUA_REGISTRYINDEX);

            GameObject obj = LuaScriptMgr.GetUnityObject<GameObject>(L, 2);
            if (obj == null)
            {
                HobaDebuger.LogError("AddObjectBehaviour: param 1 must be GameObject");
                return CheckReturnNum(L, count, nRet);
            }

            int id32 = (int)LuaScriptMgr.GetNumber(L, 3);
            ObjectBehaviour com = obj.GetComponent<ObjectBehaviour>();
            if(com == null)
                com = obj.AddComponent<ObjectBehaviour>();

            CLogicObjectMan<ObjectBehaviour>.Instance.Add(com);

            ObjectBehaviour.OBJ_TYPE obj_type = (ObjectBehaviour.OBJ_TYPE)LuaScriptMgr.GetNumber(L, 4);
            if (obj_type == ObjectBehaviour.OBJ_TYPE.HOSTPLAYER || obj_type == ObjectBehaviour.OBJ_TYPE.ELSEPLAYER
                || obj_type == ObjectBehaviour.OBJ_TYPE.NPC || obj_type == ObjectBehaviour.OBJ_TYPE.MONSTER)
            {
                GameObject entity = obj.transform.GetChild(0).gameObject;
                if (entity != null)
                {
                    CPhysicsEventTransfer pet = entity.GetComponent<CPhysicsEventTransfer>();
                    if (pet == null) pet = entity.AddComponent<CPhysicsEventTransfer>();
                    pet.Link(com);
                }
            }
            if (obj_type == ObjectBehaviour.OBJ_TYPE.HOSTPLAYER)
                com.NeedSyncPos = true;

            com.ObjType = obj_type;
           
            float radius = (float)LuaScriptMgr.GetNumber(L, 5);
            com.SetInfo(ObjRef, id32, radius);
        }
        else
        {
            LogParamError("AddObjectComponent", count);
        }

        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int ChangeAttach(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 3 && LuaScriptMgr.CheckTypes(L, 1, typeof(GameObject), typeof(int), typeof(int)))
        {
            var go = LuaScriptMgr.GetUnityObject<GameObject>(L, 1);
            if (go == null)
            {
                HobaDebuger.LogError("ChangeAttach: param 1 must be GameObject");
                return CheckReturnNum(L, count, nRet);
            }
            var holder = go.GetComponent<HangPointHolder>();
            if (holder == null)
            {
                HobaDebuger.LogError("ChangeAttach: param 1 must be has HangPointHolder");
                return CheckReturnNum(L, count, nRet);
            }

            var lhpid = LuaScriptMgr.GetNumber(L, 2);
            var rhpid = LuaScriptMgr.GetNumber(L, 3);

            var srchp = holder.GetHangPoint((HangPointHolder.HangPointType)lhpid);
            var dsthp = holder.GetHangPoint((HangPointHolder.HangPointType)rhpid);

            if (srchp != null && dsthp != null && srchp.transform != null && srchp.transform.childCount > 0)
            {
                var attach = srchp.transform.GetChild(0);
                attach.parent = dsthp.transform;
                attach.localPosition = Vector3.zero;
                attach.localRotation = Quaternion.identity;
            }
        }
        else
        {
            HobaDebuger.LogError("invalid arguments to method: GameUtilWrap.ResetGameObject");
        }

        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int ChangePartMesh(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;
        if (count == 3 && LuaScriptMgr.CheckTypes(L, 1, typeof(GameObject), typeof(int), typeof(bool)))
        {
            var go = LuaScriptMgr.GetUnityObject<GameObject>(L, 1);
            if (go == null)
            {
                HobaDebuger.LogError("ChangePartMesh: param 1 must be GameObject");
                return CheckReturnNum(L, count, nRet);
            }
            var collector = go.GetComponent<BodyPartCollector>();
            if (collector == null)
            {
                HobaDebuger.LogError("shot of script BodyPartCollector");
                return CheckReturnNum(L, count, nRet);
            }
            var partid = LuaScriptMgr.GetNumber(L, 2);
            var state = LuaScriptMgr.GetBoolean(L, 3);
            var rd = collector.GetRenderById((BodyPartEnum)partid);
            if (rd != null)
            {
                rd.enabled = state;
            }
        }
        else
        {
            HobaDebuger.LogError("invalid arguments to method: GameUtilWrap.ChangePartMesh");
        }
        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int GetHangPoint(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 1;

        if (count == 2 && LuaScriptMgr.CheckTypes(L, 1, typeof(GameObject), typeof(int)))
        {
            var obj = LuaScriptMgr.GetUnityObject<GameObject>(L, 1);
            if (obj == null)
            {
                LuaScriptMgr.Instance.CallOnTraceBack();
                HobaDebuger.LogError("GetHangPoint: param 1 must be GameObject");
                LuaDLL.lua_pushnil(L);
                return CheckReturnNum(L, count, nRet);
            }
            var hangPointId = (int)LuaScriptMgr.GetNumber(L, 2);
            GameObject result = null;

            var holder = obj.GetComponent<HangPointHolder>();
            if (holder != null)
                result = holder.GetHangPoint((HangPointHolder.HangPointType)hangPointId);

            LuaScriptMgr.Push(L, result);
            return CheckReturnNum(L, count, nRet);
        }
        else
        {
            LogParamError("GetHangPoint", count);
            LuaDLL.lua_pushnil(L);
            return CheckReturnNum(L, count, nRet);
        }
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int ResizeCollider(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 4 && LuaScriptMgr.CheckTypes(L, 1, typeof(GameObject), typeof(double), typeof(double), typeof(double)))
        {
            GameObject obj = LuaScriptMgr.GetUnityObject<GameObject>(L, 1);
            if (obj == null)
            {
                LuaScriptMgr.Instance.CallOnTraceBack();
                HobaDebuger.LogError("ResizeCollider: param 1 must be GameObject");
                return CheckReturnNum(L, count, nRet);
            }

            float x = (float)LuaScriptMgr.GetNumber(L, 2);
            float y = (float)LuaScriptMgr.GetNumber(L, 3);
            float z = (float)LuaScriptMgr.GetNumber(L, 4);

            BoxCollider bc = obj.GetComponent<BoxCollider>();
            if (bc == null)
            {
                bc = obj.AddComponent<BoxCollider>();
                bc.center = Vector3.zero;
            }
            bc.enabled = true;
            bc.size = new Vector3(x, y, z);
        }
        else
        {
            LogParamError("ResizeCollider", count);
        }

        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int DisableCollider(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 1 && LuaScriptMgr.CheckTypes(L, 1, typeof(GameObject)))
        {
            GameObject obj = LuaScriptMgr.GetUnityObject<GameObject>(L, 1);
            if (obj == null)
            {
                return CheckReturnNum(L, count, nRet);
            }

            BoxCollider bc = obj.GetComponent<BoxCollider>();
            if (bc != null)
                bc.enabled = false;
        }
        else
        {
            LogParamError("DisableCollider", count);
        }

        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int SetupDynamicBones(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 1 && LuaScriptMgr.CheckTypes(L, 1, typeof(GameObject)))
        {
            GameObject obj = LuaScriptMgr.GetUnityObject<GameObject>(L, 1);
            var dynamicBones = obj.GetComponentsInChildren<DynamicBone>();
            for (int i = 0; i < dynamicBones.Length; i++)
            {
                dynamicBones[i].Setup();
            }
        }
        else
        {
            LogParamError("SetupDynamicBones", count);
        }
        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int RotateByAngle(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 1;

        if (count == 2 && LuaScriptMgr.CheckTypes(L, 1, typeof(double), typeof(GameObject)))
        {
            float angle = (float)LuaScriptMgr.GetNumber(L, 1);
            GameObject obj = LuaScriptMgr.GetUnityObject<GameObject>(L, 2);
            if (obj != null)
            {
                Vector3 dir = Quaternion.AngleAxis(angle, Vector3.up) * obj.transform.forward;                
                LuaScriptMgr.Push(L, dir);
            }
            else
            {
                LuaScriptMgr.Push(L, Vector3.zero);
                LuaScriptMgr.Instance.CallOnTraceBack();
                HobaDebuger.LogError("RotateByAngle: param 2 must be GameObject");
            }
        }
        else
        {
            LuaScriptMgr.Push(L, Vector3.zero);
            LogParamError("RotateByAngle", count);
        }
        return CheckReturnNum(L, count, nRet);
    }


    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int RefreshObjectEffect(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        GameObject change = LuaScriptMgr.GetUnityObject<GameObject>(L, 1);
        if (change == null)
        {
            LuaScriptMgr.Instance.CallOnTraceBack();
            HobaDebuger.LogError("RefreshObjectEffect: param change must be GameObject");
            return CheckReturnNum(L, count, nRet);
        }

        GameObject original = LuaScriptMgr.GetUnityObject<GameObject>(L, 2);
        if (original == null)
        {
            LuaScriptMgr.Instance.CallOnTraceBack();
            HobaDebuger.LogError("RefreshObjectEffect: param original must be GameObject");
            return CheckReturnNum(L, count, nRet);
        }
        var oriEffectComp = original.GetComponent<EntityEffectComponent>();
        if (null != oriEffectComp)
        {
            var targetEffectComp = change.GetComponent<EntityEffectComponent>();
            if (null == targetEffectComp)
            {
                targetEffectComp = change.AddComponent<EntityEffectComponent>();
                targetEffectComp.Init();
            }
            targetEffectComp.enabled = true;
            targetEffectComp.CopyEntityEffect(ref oriEffectComp);
        }
        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int AddObjectEffect(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        GameObject obj = LuaScriptMgr.GetUnityObject<GameObject>(L, 1);
        if (obj == null)
        {
            LuaScriptMgr.Instance.CallOnTraceBack();
            HobaDebuger.LogError("AddObjectDieEffect: param 1 must be GameObject");
            return CheckReturnNum(L, count, nRet);
        }

        GameObject go = obj;
        var entityClientEffect = go.GetComponent<EntityEffectComponent>();
        if (entityClientEffect == null)
        {
            entityClientEffect = go.AddComponent<EntityEffectComponent>();
            entityClientEffect.Init();
        }
        CLogicObjectMan<EntityEffectComponent>.Instance.Add(entityClientEffect);

        int effectType = (int)LuaScriptMgr.GetNumber(L, 2);

        if (effectType == (int)EffectType.HitTwinkleWhite)
        {
            var duration = (float)LuaScriptMgr.GetNumber(L, 3);
            float r = (float)LuaScriptMgr.GetNumber(L, 4);
            float g = (float)LuaScriptMgr.GetNumber(L, 5);
            float b = (float)LuaScriptMgr.GetNumber(L, 6);
            float a = (float)LuaScriptMgr.GetNumber(L, 7);
            float power = (float)LuaScriptMgr.GetNumber(L, 8);
            entityClientEffect.StartTwinkleWhite(duration, r, g, b, a, power);
        }
        else if (effectType == 3) //清除闪白效果
        {
            entityClientEffect.StopTwinkleWhite();
        }
        else if (effectType == (int)EffectType.HitFlawsColor)  // 破绽闪红，逻辑控制闪红的开关
        {
            var enable = LuaScriptMgr.GetBoolean(L, 3);
            if (enable)
            {
                var r = (float)LuaScriptMgr.GetNumber(L, 4);
                var g = (float)LuaScriptMgr.GetNumber(L, 5);
                var b = (float)LuaScriptMgr.GetNumber(L, 6);
                var power = (float)LuaScriptMgr.GetNumber(L, 7);
                entityClientEffect.StartHitFlawsColor(r, g, b, power);
            }
            else
            {
                entityClientEffect.StopHitFlawsColor();
            }
        }
        else if (effectType == (int)EffectType.EliteBornColor)  // 精英出生 设置边缘
        {
            float r = (float)LuaScriptMgr.GetNumber(L, 3);
            float g = (float)LuaScriptMgr.GetNumber(L, 4);
            float b = (float)LuaScriptMgr.GetNumber(L, 5);
            float power = (float)LuaScriptMgr.GetNumber(L, 6);
            entityClientEffect.EnableEliteBornColor(r, g, b, power);
        }
        else if (effectType == (int)EffectType.Frozen)
        {
            var enabled = LuaScriptMgr.GetBoolean(L, 3);
            entityClientEffect.EnableFrozenEffect(enabled);
        }
        else if (effectType == 12)  // 隐身
        {
            var on = (bool)LuaScriptMgr.GetBoolean(L, 3);
            // "Model" "ForwardPointer" "Shadow"
            Renderer[] renders = obj.GetComponentsInChildren<Renderer>();
            for (int i = 0; i < renders.Length; i++)
            {
                renders[i].enabled = !on;
            }

            Transform pointer = obj.transform.Find("ForwardPointer");
            if (pointer != null)
            {
                pointer.gameObject.SetActive(!on);
            }
        }
        else if (effectType == 13)  // 溶解
        {
            float duration = (float)LuaScriptMgr.GetNumber(L, 3);
            float r = (float)LuaScriptMgr.GetNumber(L, 4);
            float g = (float)LuaScriptMgr.GetNumber(L, 5);
            float b = (float)LuaScriptMgr.GetNumber(L, 6);
            float a = (float)LuaScriptMgr.GetNumber(L, 7);
            entityClientEffect.DissoveToDeath(r, g, b, a, duration);
        }
        else if (effectType == 14)  // 刀光
        {
            var on = (bool)LuaScriptMgr.GetBoolean(L, 3);
            var renders = obj.GetComponentsInChildren<CustomTrailRenderer>();
            for (int i = 0; i < renders.Length; i++)
            {
                renders[i].enabled = on;
            }
        }
        else if (effectType == 15)  // 子物体相机模糊
        {
            var on = (bool)LuaScriptMgr.GetBoolean(L, 3);
            float fade_in = (float)LuaScriptMgr.GetNumber(L, 4);
            float duration = (float)LuaScriptMgr.GetNumber(L, 5);
            float fade_out = (float)LuaScriptMgr.GetNumber(L, 6);
            float level = (float)LuaScriptMgr.GetNumber(L, 7);
            float radius = (float)LuaScriptMgr.GetNumber(L, 8);
            if(on)
            {
                var fxone = obj.GetComponent<CFxOne>();
                if(fxone != null)
                {
                    if (fxone.RadialBlurBoot == null)
                        fxone.RadialBlurBoot = new RadialBlurBootInfo();
                    fxone.RadialBlurBoot.Set(fade_in, duration, fade_out, level, radius);
                    fxone.EnableRadialBlur();
                }
            }
        }
        else if (effectType == 16)  
        {
            string hp = (string)LuaScriptMgr.GetString(L, 3);
            float fade_in = (float)LuaScriptMgr.GetNumber(L, 4);
            float duration = (float)LuaScriptMgr.GetNumber(L, 5);
            float fade_out = (float)LuaScriptMgr.GetNumber(L, 6);
            float level = (float)LuaScriptMgr.GetNumber(L, 7);
            float radius = (float)LuaScriptMgr.GetNumber(L, 8);
            var hangpoint = obj;
            if (! string.IsNullOrEmpty(hp))
                hangpoint = obj.FindChildRecursively(hp);

            if (hangpoint != null)
            {
                var blur = hangpoint.GetComponent<RadialBlurBoot>();
                if (null == blur)
                    blur = hangpoint.AddComponent<RadialBlurBoot>();
                blur.StartEffect(fade_in, duration, fade_out, level, radius);
            }            
        }
        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int EnableHostPosSyncWhenMove(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 1 && LuaScriptMgr.CheckTypes(L, 1, typeof(bool)))
        {
            var hostGo = Main.HostPalyer;
            if(hostGo == null)
                return CheckReturnNum(L, count, nRet);

            var comp = hostGo.GetComponent<ObjectBehaviour>();
            if (comp == null)
                return CheckReturnNum(L, count, nRet);

            comp.NeedSyncPos = LuaScriptMgr.GetBoolean(L, 1);
        }
        else
        {
            LogParamError("EnableHostPosSyncWhenMove", count);
        }

        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int AddMoveBehavior(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 5 && LuaScriptMgr.CheckTypes(L, 1, typeof(GameObject), typeof(LuaTable), typeof(double), typeof(LuaFunction), typeof(bool)))
        {
            GameObject obj = LuaScriptMgr.GetUnityObject<GameObject>(L, 1);
            if (obj == null)
            {
                LuaScriptMgr.Instance.CallOnTraceBack();
                HobaDebuger.LogError("AddMoveBehavior: param 1 must be GameObject");
                return CheckReturnNum(L, count, nRet);
            }

            ObjectBehaviour comp = obj.GetComponent<ObjectBehaviour>();
            if (comp == null)
            {
                HobaDebuger.LogError("Can not AddMoveBehavior to a GameObject without ObjectBehaviour");
                return CheckReturnNum(L, count, nRet);
            }

            Vector3 target_pos = LuaScriptMgr.GetVector3(L, 2);
            float speed = (float)LuaScriptMgr.GetNumber(L, 3);
            LuaFunction cb = LuaScriptMgr.GetLuaFunction(L, 4);
            bool changeDir = LuaScriptMgr.GetBoolean(L, 5);

            comp.RemoveBehavior(BehaviorType.Follow);
            //comp.RemoveBehavior(BehaviorType.WanderMove);
            MoveBehavior mb = comp.AddBehavior(BehaviorType.Move) as MoveBehavior;

            mb.SetData(target_pos, speed, cb, comp.ID32Bit, 0, false);
            mb.IsDirChanged = changeDir;
            mb.IgnoreCollisionDetect = false;
        }
        else if (count == 8 && LuaScriptMgr.CheckTypes(L, 1, typeof(GameObject), typeof(LuaTable), typeof(double), typeof(double), typeof(LuaFunction), typeof(bool), typeof(bool), typeof(bool)))
        {
            GameObject obj = LuaScriptMgr.GetUnityObject<GameObject>(L, 1);
            if (obj == null)
            {
                HobaDebuger.LogError("AddMoveBehavior: param 1 must be GameObject");
                return CheckReturnNum(L, count, nRet);
            }

            ObjectBehaviour comp = obj.GetComponent<ObjectBehaviour>();
            if (comp == null)
            {
                HobaDebuger.LogError("Can not AddMoveBehavior to a GameObject without ObjectBehaviour");
                return CheckReturnNum(L, count, nRet);
            }

            Vector3 target_pos = LuaScriptMgr.GetVector3(L, 2);
            float speed = (float)LuaScriptMgr.GetNumber(L, 3);
            float fOffset = (float)LuaScriptMgr.GetNumber(L, 4);

            LuaFunction cb = LuaScriptMgr.GetLuaFunction(L, 5);

            //bool need_sync = LuaScriptMgr.GetBoolean(L, 6);
            bool changeDir = LuaScriptMgr.GetBoolean(L, 6);
            bool autopathing = LuaScriptMgr.GetBoolean(L, 7);
            bool ignoreCollision = LuaScriptMgr.GetBoolean(L, 8);

            comp.RemoveBehavior(BehaviorType.Follow);
            //comp.RemoveBehavior(BehaviorType.WanderMove);
            MoveBehavior mb = comp.AddBehavior(BehaviorType.Move) as MoveBehavior;

            mb.SetData(target_pos, speed, cb, comp.ID32Bit, fOffset > 0 ? fOffset : 0, autopathing);
            mb.IsDirChanged = changeDir;
            mb.IgnoreCollisionDetect = ignoreCollision;
        }
        else
        {
            LogParamError("AddMoveBehavior", count);
        }

        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int SetMoveBehaviorSpeed(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 1;

        if (count == 2 && LuaScriptMgr.CheckTypes(L, 1, typeof(GameObject), typeof(double)))
        {
            GameObject obj = LuaScriptMgr.GetUnityObject<GameObject>(L, 1);
            if (obj == null)
            {
                LuaScriptMgr.Instance.CallOnTraceBack();
                HobaDebuger.LogError("SetMoveBehaviorSpeed: param 1 must be GameObject");

                LuaDLL.lua_pushboolean(L, false);
                return CheckReturnNum(L, count, nRet);
            }

            ObjectBehaviour comp = obj.GetComponent<ObjectBehaviour>();
            if (comp == null)
            {
                HobaDebuger.LogError("Can not SetMoveBehaviorSpeed to a GameObject without ObjectBehaviour");

                LuaDLL.lua_pushboolean(L, false);
                return CheckReturnNum(L, count, nRet);
            }

            MoveBehavior moveBehavior = comp.GetActiveBehavior(BehaviorType.Move) as MoveBehavior;
            if (moveBehavior == null)
            {
                LuaDLL.lua_pushboolean(L, false);
                return CheckReturnNum(L, count, nRet);
            }

            float speed = (float)LuaScriptMgr.GetNumber(L, 2);

            moveBehavior.MoveSpeed = speed;
            LuaDLL.lua_pushboolean(L, true);
        }
        else
        {
            LogParamError("SetMoveBehaviorSpeed", count);
            LuaDLL.lua_pushboolean(L, false);
        }

        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int AddJoyStickMoveBehavior(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 3 && LuaScriptMgr.CheckTypes(L, 1, typeof(GameObject), typeof(double), typeof(bool)))
        {
            GameObject obj = LuaScriptMgr.GetUnityObject<GameObject>(L, 1);
            if (obj == null)
            {
                HobaDebuger.LogError("AddJoyStickMoveBehavior: param 1 must be GameObject");
                return CheckReturnNum(L, count, nRet);
            }

            ObjectBehaviour comp = obj.GetComponent<ObjectBehaviour>();
            if (comp == null)
            {
                HobaDebuger.LogError("Can not AddJoyStickMoveBehavior to a GameObject without ObjectBehaviour");
                return CheckReturnNum(L, count, nRet);
            }

            float speed = (float)LuaScriptMgr.GetNumber(L, 2);
            bool changeDir = LuaScriptMgr.GetBoolean(L, 3);

            comp.NeedSyncPos = true;
            comp.RemoveBehavior(BehaviorType.Move);
            comp.RemoveBehavior(BehaviorType.Follow);
            //comp.RemoveBehavior(BehaviorType.WanderMove);

            JoyStickBehavior mb = comp.AddBehavior(BehaviorType.JoyStickMove) as JoyStickBehavior;
            if (mb != null)
                mb.SetData(speed, changeDir);
        }
        else
        {
            LogParamError("AddJoyStickMoveBehavior", count);
        }

        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int AddFollowBehavior(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if ((count == 6 && LuaScriptMgr.CheckTypes(L, 1, typeof(GameObject), typeof(GameObject), typeof(double), typeof(double), typeof(double), typeof(bool)))
            || (count == 7 && LuaScriptMgr.CheckTypes(L, 1, typeof(GameObject), typeof(GameObject), typeof(double), typeof(double), typeof(double), typeof(bool), typeof(LuaFunction))))
        {
            GameObject obj = LuaScriptMgr.GetUnityObject<GameObject>(L, 1);
            if (obj == null)
            {
                LuaScriptMgr.Instance.CallOnTraceBack();
                HobaDebuger.LogError("AddFollowBehavior: param 1 must be GameObject");
                return CheckReturnNum(L, count, nRet);
            }

            ObjectBehaviour comp = obj.GetComponent<ObjectBehaviour>();
            if (comp == null)
            {
                HobaDebuger.LogError("Can not AddFollowBehavior to a GameObject without ObjectBehaviour");
                return CheckReturnNum(L, count, nRet);
            }

            GameObject target = LuaScriptMgr.GetUnityObject<GameObject>(L, 2);
            if (target == null)
            {
                HobaDebuger.LogError("AddFollowBehavior: param 2 must be GameObject");
                return CheckReturnNum(L, count, nRet);
            }

            float speed = (float)LuaScriptMgr.GetNumber(L, 3);
            float maxDis = (float)LuaScriptMgr.GetNumber(L, 4);
            float minDis = (float)LuaScriptMgr.GetNumber(L, 5);
            bool isOnce = LuaScriptMgr.GetBoolean(L, 6);
            LuaFunction cbref = LuaScriptMgr.GetLuaFunction(L, 7);

            comp.RemoveBehavior(BehaviorType.Move);
            //comp.RemoveBehavior(BehaviorType.WanderMove);
            FollowBehavior mb = comp.AddBehavior(BehaviorType.Follow) as FollowBehavior;
            mb.SetData(target, speed, maxDis, minDis, isOnce, cbref);
        }
        else
        {
            LogParamError("AddFollowBehavior", count);
        }

        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int AddTurnBehavior(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;
        if (count == 6 && LuaScriptMgr.CheckTypes(L, 1, typeof(GameObject), typeof(LuaTable), 
            typeof(float), typeof(LuaFunction), typeof(bool), typeof(float)))
        {
            GameObject obj = LuaScriptMgr.GetUnityObject<GameObject>(L, 1);
            if (obj == null)
            {
                LuaScriptMgr.Instance.CallOnTraceBack();
                HobaDebuger.LogError("AddTurnBehavior: param 1 must be GameObject");
                return CheckReturnNum(L, count, nRet);
            }

            ObjectBehaviour comp = obj.GetComponent<ObjectBehaviour>();
            if (comp == null)
            {
                HobaDebuger.LogError("AddTurnBehavior: param 1 must have ObjectBehaviour");
                return CheckReturnNum(L, count, nRet);
            }
            var faceDir = LuaScriptMgr.GetVector3(L, 2);
            var speed = (float)LuaScriptMgr.GetNumber(L, 3);
            LuaFunction cb = LuaScriptMgr.GetLuaFunction(L, 4);
            var is_continue = LuaScriptMgr.GetBoolean(L, 5);
            var anlge = (float)LuaScriptMgr.GetNumber(L, 6);
            TurnBehavior tb = comp.AddBehavior(BehaviorType.Turn) as TurnBehavior;
            tb.SetData(faceDir, speed, cb, is_continue, anlge);
        }
        else
        {
            LogParamError("AddTurnBehavior", count);
        }

        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int AddDashBehavior(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 5 && LuaScriptMgr.CheckTypes(L, 1, typeof(GameObject), typeof(LuaTable), typeof(float), typeof(bool), typeof(bool)))
        {
            GameObject obj = LuaScriptMgr.GetUnityObject<GameObject>(L, 1);
            if (obj == null)
            {
                LuaScriptMgr.Instance.CallOnTraceBack();
                HobaDebuger.LogError("AddChargeBehavior: param 1 must be GameObject");
                return CheckReturnNum(L, count, nRet);
            }
            ObjectBehaviour comp = obj.GetComponent<ObjectBehaviour>();
            if (comp == null)
            {
                HobaDebuger.LogError("AddChargeBehavior: param 1 must have ObjectBehaviour");
                return CheckReturnNum(L, count, nRet);
            }

            comp.RemoveBehavior(BehaviorType.Move);
            comp.RemoveBehavior(BehaviorType.Follow);

            Vector3 dest = LuaScriptMgr.GetVector3(L, 2);
            float time = (float)LuaScriptMgr.GetNumber(L, 3);
            bool canPierce = LuaScriptMgr.GetBoolean(L, 4);
            var killedGoOn = LuaScriptMgr.GetBoolean(L,5);

            DashBehavior cb = comp.AddBehavior(BehaviorType.Dash) as DashBehavior;
            cb.SetData(dest, time, null, canPierce, killedGoOn);
        }
        else if (count == 7 && LuaScriptMgr.CheckTypes(L, 1, typeof(GameObject), typeof(LuaTable), typeof(float), typeof(bool), typeof(bool), typeof(bool)))
        {
            GameObject obj = LuaScriptMgr.GetUnityObject<GameObject>(L, 1);
            if (obj == null)
            {
                LuaScriptMgr.Instance.CallOnTraceBack();
                HobaDebuger.LogError("AddChargeBehavior: param 1 must be GameObject");
                return CheckReturnNum(L, count, nRet);
            }
            ObjectBehaviour comp = obj.GetComponent<ObjectBehaviour>();
            if (comp == null)
            {
                HobaDebuger.LogError("AddChargeBehavior: param 1 must have ObjectBehaviour");
                return CheckReturnNum(L, count, nRet);
            }

            comp.RemoveBehavior(BehaviorType.Move);
            comp.RemoveBehavior(BehaviorType.Follow);

            Vector3 dest = LuaScriptMgr.GetVector3(L, 2);
            float time = (float)LuaScriptMgr.GetNumber(L, 3);
            bool canPierce = LuaScriptMgr.GetBoolean(L, 4);
            var killedGoOn = LuaScriptMgr.GetBoolean(L, 5);
            var onlyCollideTarget = LuaScriptMgr.GetBoolean(L, 6);
            var canChangeDir = LuaScriptMgr.GetBoolean(L, 7);

            DashBehavior cb = comp.AddBehavior(BehaviorType.Dash) as DashBehavior;
            cb.SetData(dest, time, null, canPierce, killedGoOn, onlyCollideTarget, canChangeDir);
        }
        else
        {
            LogParamError("AddDashBehavior", count);
        }

        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int AddAdsorbEffect(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 4 && LuaScriptMgr.CheckTypes(L, 1, typeof(GameObject), typeof(int), typeof(float), typeof(LuaTable)))
        {
            GameObject obj = LuaScriptMgr.GetUnityObject<GameObject>(L, 1);
            if (obj == null)
            {
                LuaScriptMgr.Instance.CallOnTraceBack();
                HobaDebuger.LogError("AddAdsorbBehavior: param 1 must be GameObject");
                return CheckReturnNum(L, count, nRet);
            }
            ObjectBehaviour comp = obj.GetComponent<ObjectBehaviour>();
            if (comp == null)
            {
                HobaDebuger.LogError("AddAdsorbBehavior: param 1 must have ObjectBehaviour");
                return CheckReturnNum(L, count, nRet);
            }

            var originId = (int)LuaScriptMgr.GetNumber(L, 2);
            var speed = (float)LuaScriptMgr.GetNumber(L, 3);
            var position = LuaScriptMgr.GetVector3(L, 4);

            var behavior = comp.AddBehavior(BehaviorType.Adsorb) as AdsorbBehavior;
            if(behavior != null)
                behavior.AddAdsorbData(originId, speed, position);
        }
        else
        {
            LogParamError("AddAdsorbBehavior", count);
        }

        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int RemoveAdsorbEffect(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 2 && LuaScriptMgr.CheckTypes(L, 1, typeof(GameObject), typeof(int)))
        {
            GameObject obj = LuaScriptMgr.GetUnityObject<GameObject>(L, 1);
            if (obj == null)
            {
                LuaScriptMgr.Instance.CallOnTraceBack();
                HobaDebuger.LogError("AddAdsorbBehavior: param 1 must be GameObject");
                return CheckReturnNum(L, count, nRet);
            }
            ObjectBehaviour comp = obj.GetComponent<ObjectBehaviour>();
            if (comp == null)
            {
                HobaDebuger.LogError("AddAdsorbBehavior: param 1 must have ObjectBehaviour");
                return CheckReturnNum(L, count, nRet);
            }

            comp.RemoveBehavior(BehaviorType.Adsorb);
        }
        else
        {
            LogParamError("RemoveOneAdsorb", count);
        }

        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int RemoveBehavior(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 2 && LuaScriptMgr.CheckTypes(L, 1, typeof(GameObject), typeof(int)))
        {
            GameObject obj = LuaScriptMgr.GetUnityObject<GameObject>(L, 1);
            if (obj == null)
            {
                LuaScriptMgr.Instance.CallOnTraceBack();
                HobaDebuger.LogError("RemoveBehavior: param 1 must be GameObject");
                return CheckReturnNum(L, count, nRet);
            }

            ObjectBehaviour comp = obj.GetComponent<ObjectBehaviour>();
            if (comp == null)
            {
                HobaDebuger.LogError("Can not use RemoveBehavior to GameObject without ObjectBehaviour");
                return CheckReturnNum(L, count, nRet);
            }

            int type_id = (int)LuaScriptMgr.GetNumber(L, 2);
            if (type_id > (int)BehaviorType.Invalid && type_id < (int)BehaviorType.Count)
            {

                comp.RemoveBehavior((BehaviorType)type_id);
            }
            else
            {
                HobaDebuger.LogErrorFormat("RemoveBehavior invalid type_id: {0}", type_id);
            }
        }
        else
        {
            LogParamError("RemoveBehavior", count);
        }

        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int HasBehavior(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 1;

        bool has = false;
        if (count == 2 && LuaScriptMgr.CheckTypes(L, 1, typeof(GameObject), typeof(int)))
        {
            GameObject obj = LuaScriptMgr.GetUnityObject<GameObject>(L, 1);
            if (obj == null)
            {
                LuaScriptMgr.Instance.CallOnTraceBack();
                HobaDebuger.LogError("HasBehavior: param 1 must be GameObject");
                LuaScriptMgr.Push(L, has);
                return CheckReturnNum(L, count, nRet);
            }

            ObjectBehaviour comp = obj.GetComponent<ObjectBehaviour>();
            if (comp == null)
            {
                HobaDebuger.LogError("HasBehavior's GameObject must have ObjectBehaviour");
                LuaScriptMgr.Push(L, has);
                return CheckReturnNum(L, count, nRet);
            }

            int type_id = (int)LuaScriptMgr.GetNumber(L, 2);
            if (type_id > (int)BehaviorType.Invalid && type_id < (int)BehaviorType.Count)
                has = comp.HasBehavior((BehaviorType)type_id);
        }
        else
        {
            LogParamError("HasBehavior", count);
        }

        LuaScriptMgr.Push(L, has);
        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int ChangeOutward(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count >= 3 && LuaScriptMgr.CheckTypes(L, 1, typeof(GameObject), typeof(int), typeof(string)))
        {
            GameObject obj = LuaScriptMgr.GetUnityObject<GameObject>(L, 1);
            if (obj == null)
            {
                LuaScriptMgr.Instance.CallOnTraceBack();
                HobaDebuger.LogError("ChangeOutward: param 1 must be GameObject");
                return CheckReturnNum(L, count, nRet);
            }

            var man = obj.GetComponent<OutwardComponent>();
            if (man == null)
                man = obj.AddComponent<OutwardComponent>();
            OutwardComponent.RegisterToMan(man);

            int partId = (int)LuaScriptMgr.GetNumber(L, 2);
            string assetPath = LuaScriptMgr.GetString(L, 3);

            LuaFunction callback = null;

            if (count == 4)
                callback = LuaScriptMgr.GetLuaFunction(L, 4);

            man.ChangeOutward((OutwardComponent.OutwardPart)partId, assetPath, callback);
        }
        else
        {
            LogParamError("ChangeOutward", count);
        }

        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int ChangeHairColor(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 4 && LuaScriptMgr.CheckTypes(L, 1, typeof(GameObject), typeof(int), typeof(int), typeof(int)))
        {
            GameObject obj = LuaScriptMgr.GetUnityObject<GameObject>(L, 1);
            if (obj == null)
            {
                LuaScriptMgr.Instance.CallOnTraceBack();
                HobaDebuger.LogError("ChangeHairColor: param 1 must be GameObject");
                return CheckReturnNum(L, count, nRet);
            }

            var man = obj.GetComponent<OutwardComponent>();
            if (man == null)
                man = obj.AddComponent<OutwardComponent>();
            OutwardComponent.RegisterToMan(man);

            int colorR = (int)LuaScriptMgr.GetNumber(L, 2);
            int colorG = (int)LuaScriptMgr.GetNumber(L, 3);
            int colorB = (int)LuaScriptMgr.GetNumber(L, 4);

            Color color = new Color(colorR / 255.0f, colorG / 255.0f, colorB / 255.0f);
            man.ChangeHairColor(color);
        }
        else
        {
            LogParamError("ChangeHairColor", count);
        }

        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int ChangeSkinColor(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 4 && LuaScriptMgr.CheckTypes(L, 1, typeof(GameObject), typeof(int), typeof(int), typeof(int)))
        {
            GameObject obj = LuaScriptMgr.GetUnityObject<GameObject>(L, 1);
            if (obj == null)
            {
                LuaScriptMgr.Instance.CallOnTraceBack();
                HobaDebuger.LogError("ChangeSkinColor: param 1 must be GameObject");
                return CheckReturnNum(L, count, nRet);
            }

            var man = obj.GetComponent<OutwardComponent>();
            if (man == null)
                man = obj.AddComponent<OutwardComponent>();
            OutwardComponent.RegisterToMan(man);

            int colorR = (int)LuaScriptMgr.GetNumber(L, 2);
            int colorG = (int)LuaScriptMgr.GetNumber(L, 3);
            int colorB = (int)LuaScriptMgr.GetNumber(L, 4);

            Color color = new Color(colorR / 255.0f, colorG / 255.0f, colorB / 255.0f);
            man.ChangeSkinColor(color);
        }
        else
        {
            LogParamError("ChangeSkinColor", count);
        }

        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int EnableGroundNormal(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 2 && LuaScriptMgr.CheckTypes(L, 1, typeof(GameObject), typeof(bool)))
        {
            GameObject obj = LuaScriptMgr.GetUnityObject<GameObject>(L, 1);
            if (obj == null)
            {
                LuaScriptMgr.Instance.CallOnTraceBack();
                HobaDebuger.LogError("SetGameObjectUseGroundNormal: param 1 must be GameObject");
                return CheckReturnNum(L, count, nRet);
            }

            bool bUseNormal = LuaScriptMgr.GetBoolean(L, 2);

            //bool bSet = false;
            ObjectBehaviour comp = obj.GetComponent<ObjectBehaviour>();
            if (comp != null)
                comp.SetUseGroundNormal(bUseNormal);

//             if (!bSet)      //没有被behavior应用，计算
//             {
//                 var trans = obj.transform;
// 
//                 if (bUseNormal)
//                     trans.rotation = CMapUtil.GetMapNormalRotation(trans.position, trans.forward, 0.75f);
//                 else
//                     trans.rotation = CMapUtil.GetUpNormalRotation(trans.forward);
//             }
        }
        else
        {
            LogParamError("SetGameObjectUseGroundNormal", count);
        }

        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int SetGameObjectYOffset(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 2 && LuaScriptMgr.CheckTypes(L, 1, typeof(GameObject), typeof(bool)))
        {
            GameObject obj = LuaScriptMgr.GetUnityObject<GameObject>(L, 1);
            if (obj == null)
            {
                LuaScriptMgr.Instance.CallOnTraceBack();
                HobaDebuger.LogError("SetGameObjectYOffset: param 1 must be GameObject");
                return CheckReturnNum(L, count, nRet);
            }

            float yOffset = (float)LuaScriptMgr.GetNumber(L, 2);

            bool bSet = false;
            ObjectBehaviour comp = obj.GetComponent<ObjectBehaviour>();
            if (comp != null)
            {
                bSet = comp.SetYOffset(yOffset);
            }

            if (!bSet)      //没有被behavior应用，计算
            {
                var trans = obj.transform;
                Vector3 cur_pos = trans.position;
                cur_pos.y = CUnityUtil.GetMapHeight(cur_pos) + yOffset;
                trans.position = cur_pos; 
            }
        }
        else
        {
            LogParamError("SetGameObjectYOffset", count);
        }

        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int EnablePhysicsCollision(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if ((count == 3 && LuaScriptMgr.CheckTypes(L, 1, typeof(GameObject), typeof(bool), typeof(int))))
        {
            GameObject obj = LuaScriptMgr.GetUnityObject<GameObject>(L, 1);
            if (obj == null)
            {
                LuaScriptMgr.Instance.CallOnTraceBack();
                HobaDebuger.LogError("EnablePhysicsCollision: param 1 must be GameObject");
                return CheckReturnNum(L, count, nRet);
            }

            bool enable = LuaScriptMgr.GetBoolean(L, 2);
            BoxCollider bc = obj.GetComponent<BoxCollider>();
            if (bc != null) bc.enabled = enable;

            if (enable)
            {
                int id = (int)LuaScriptMgr.GetNumber(L, 3);
                CWeaponOwnerInfo woi = obj.GetComponent<CWeaponOwnerInfo>();
                if (woi == null) woi = obj.AddComponent<CWeaponOwnerInfo>();
                woi.Owner32BitID = id;
            }
        }
        else
        {
            LogParamError("EnablePhysicsCollision", count);
        }

        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int OnEntityModelChanged(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 1 && LuaScriptMgr.CheckTypes(L, 1, typeof(GameObject)))
        {
            GameObject obj = LuaScriptMgr.GetUnityObject<GameObject>(L, 1);
            if (obj == null)
            {
                HobaDebuger.LogWarning("OnEntityModelChanged: param 1 must be GameObject");
                return CheckReturnNum(L, count, nRet);
            }

            var man = obj.GetComponent<EntityEffectComponent>();
            if (man != null)
                man.OnModelChanged();

            return CheckReturnNum(L, count, nRet);
        }
        else
        {
            LogParamError("OnEntityModelChanged", count);
            return CheckReturnNum(L, count, nRet);
        }
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int ChangeDressColor(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count >= 4 && LuaScriptMgr.CheckTypes(L, 1, typeof(GameObject), typeof(string), typeof(int), typeof(LuaTable)))
        {
            GameObject obj = LuaScriptMgr.GetUnityObject<GameObject>(L, 1);
            if (obj == null)
            {
                HobaDebuger.LogError("GameUtil :: ChangeNormalDressColor -> the GameObject is null.");
                return CheckReturnNum(L, count, nRet);
            }

            string name = LuaScriptMgr.GetString(L, 2);
            int part = (int)LuaScriptMgr.GetNumber(L, 3);
            var color = LuaScriptMgr.GetColor(L, 4);
            var man = obj.GetComponent<OutwardComponent>();
            if (man == null)
                man = obj.AddComponent<OutwardComponent>();
            bool bSetColor = true;
            if (count == 5 && LuaScriptMgr.CheckTypes(L, 5, typeof(bool)))
                bSetColor = LuaScriptMgr.GetBoolean(L, 5);

            //是否设置染色，false为还原
            man.ChangeDressColor(name, part, color, bSetColor);
        }
        else
        {
            LogParamError("ChangeNormalDressColor", count);
        }

        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int ChangeDressEmbroidery(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 2 && LuaScriptMgr.CheckTypes(L, 1, typeof(GameObject), typeof(string)))
        {
            GameObject obj = LuaScriptMgr.GetUnityObject<GameObject>(L, 1);
            if (obj == null)
            {
                HobaDebuger.LogError("GameUtil :: ChangeDressEmbroidery -> the GameObject is null.");
                return CheckReturnNum(L, count, nRet);
            }

            var path = LuaScriptMgr.GetString(L, 2);

            var man = obj.GetComponent<OutwardComponent>();
            if (man == null)
                man = obj.AddComponent<OutwardComponent>();
            man.ChangeArmorEmbroidery(path);
        }
        else
        {
            LogParamError("ChangeDressEmbroidery", count);
        }

        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int EnableLockWingYZRotation(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 3 && (LuaScriptMgr.CheckTypes(L, 1, typeof(bool), typeof(GameObject), typeof(GameObject))))
        {
            var go = LuaScriptMgr.GetUnityObject<GameObject>(L, 2);
            if (go == null)
            {
                HobaDebuger.LogError("EnableLockWingYZRotation: param 2 GameObject got null");
                return CheckReturnNum(L, count, nRet);
            }
            var comp = go.GetComponent<LockRotationComponent>();
            bool enable = LuaScriptMgr.GetBoolean(L, 1);
            if (enable)
            {
                var root = LuaScriptMgr.GetUnityObject<GameObject>(L, 3);
                if (root == null)
                {
                    HobaDebuger.LogError("EnableLockWingYZRotation: param 3 GameObject got null");
                    return CheckReturnNum(L, count, nRet);
                }
                if (comp == null)
                    comp = go.AddComponent<LockRotationComponent>();

                comp.Root = root.transform;
                comp.enabled = true;
            }
            else
            {
                if (comp != null)
                    comp.enabled = false;
                
                go.transform.localRotation = Quaternion.identity;
            }
        }
        else
        {
            LogParamError("EnableLockWingYZRotation", count);
        }
        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int EnableAnimationBulletTime(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 3 && (LuaScriptMgr.CheckTypes(L, 1, typeof(GameObject), typeof(bool), typeof(float))))
        {
            var go = LuaScriptMgr.GetUnityObject<GameObject>(L, 1);
            if (go == null)
            {
                HobaDebuger.LogError("EnableAnimationBulletTime: param 1 GameObject got null");
                return CheckReturnNum(L, count, nRet);
            }

            var bulletTimeComp = go.GetComponent<CBulletTimeAnimation>();
            var isOn = LuaScriptMgr.GetBoolean(L, 2);

            if (isOn)
            {
                if(bulletTimeComp == null)
                    bulletTimeComp = go.AddComponent<CBulletTimeAnimation>();
                var speed = (float)LuaScriptMgr.GetNumber(L, 3);
                bulletTimeComp.OnStart(speed);
            }
            else
            {
                if (bulletTimeComp != null)
                    bulletTimeComp.OnFinish();
            }
        }
        else
        {
            LogParamError("EnableAnimationBulletTime", count);
        }
        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int SetEntityColliderRadius(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 2 && (LuaScriptMgr.CheckTypes(L, 1, typeof(GameObject), typeof(float))))
        {
            var go = LuaScriptMgr.GetUnityObject<GameObject>(L, 1);
            if (go == null)
            {
                HobaDebuger.LogError("SetEntityColliderRadius: param 1 GameObject got null");
                return CheckReturnNum(L, count, nRet);
            }

            var collider = go.GetComponent<CapsuleCollider>();
            if (collider == null)
            {
                HobaDebuger.LogError("SetEntityColliderRadius: param 1 GameObject got no CapsuleCollider");
                return CheckReturnNum(L, count, nRet);
            }
            float radius = (float)LuaScriptMgr.GetNumber(L, 2);
            collider.radius = radius;
        }
        else
        {
            LogParamError("SetEntityColliderRadius", count);
        }
        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int EnableDressUnderSfx(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 2 && (LuaScriptMgr.CheckTypes(L, 1, typeof(GameObject), typeof(bool))))
        {
            var go = LuaScriptMgr.GetUnityObject<GameObject>(L, 1);
            if (go == null)
            {
                HobaDebuger.LogError("EnableDressUnderSfx: param 1 GameObject got null");
                return CheckReturnNum(L, count, nRet);
            }

            var man = go.GetComponent<OutwardComponent>();
            if (man == null)
            {
                HobaDebuger.LogError("EnableDressUnderSfx: param 1 GameObject got no OutwardManager");
                return CheckReturnNum(L, count, nRet);
            }
            bool enable = LuaScriptMgr.GetBoolean(L, 2);
            man.EnableRootSfx(enable);
        }
        else
        {
            LogParamError("EnableDressUnderSfx", count);
        }
        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int EnableOutwardPart(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 3 && (LuaScriptMgr.CheckTypes(L, 1, typeof(GameObject), typeof(int), typeof(bool))))
        {
            var go = LuaScriptMgr.GetUnityObject<GameObject>(L, 1);
            if (go == null)
            {
                HobaDebuger.LogError("EnableOutwardPart: param 1 GameObject got null");
                return CheckReturnNum(L, count, nRet);
            }

            var man = go.GetComponent<OutwardComponent>();
            if (man == null)
            {
                HobaDebuger.LogError("EnableOutwardPart: param 1 GameObject got no OutwardManager");
                return CheckReturnNum(L, count, nRet);
            }
            int partId = (int)LuaScriptMgr.GetNumber(L, 2);
            bool enable = LuaScriptMgr.GetBoolean(L, 3);
            man.EnableOutwardPart((OutwardComponent.OutwardPart)partId, enable);
        }
        else
        {
            LogParamError("EnableOutwardPart", count);
        }
        return CheckReturnNum(L, count, nRet);
    }

}
