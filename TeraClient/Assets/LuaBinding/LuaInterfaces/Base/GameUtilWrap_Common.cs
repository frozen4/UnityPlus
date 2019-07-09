using Common;
using DG.Tweening;
using LuaInterface;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

public static partial class GameUtilWrap
{
    private static bool bLogProtocol = false;

//     [MonoPInvokeCallback(typeof(LuaCSFunction))]
//     public static int SetMapLayers(IntPtr L)
//     {
//         int count = LuaDLL.lua_gettop(L);
//         const int nRet = 0;
// 
//         CUnityUtil.SupportedRaycastMapLayerMask = CUnityUtil.LayerMaskTerrain;
//         return CheckReturnNum(L, count, nRet);
// 
//         if (count == 1)
//         {
//             LuaTable layers = LuaScriptMgr.GetLuaTable(L, 1);
//             if (layers != null)
//             {
//                 int key = 1;
//                 while (layers[key] != null)
//                 {
//                     int layer = (int)layers[key];
//                     CUnityUtil.SupportedRaycastMapLayerMask |= (1 << layer);
//                     key++;
//                 }
//             }
//             return CheckReturnNum(L, count, nRet);
//         }
//         else
//         {
//             LogParamError("SetMapLayers", count);
//             return CheckReturnNum(L, count, nRet);
//         }
//     }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int FindChild(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 1;

        if (count == 2 && LuaScriptMgr.CheckTypes(L, 1, typeof(GameObject), typeof(string)))
        {
            GameObject obj = LuaScriptMgr.GetUnityObject<GameObject>(L, 1);
            if (obj == null)
            {
                LuaScriptMgr.Instance.CallOnTraceBack();
                HobaDebuger.LogError("FindChild: param 1 must be GameObject");
                LuaDLL.lua_pushnil(L);
                return CheckReturnNum(L, count, nRet);
            }
            string childname = LuaScriptMgr.GetString(L, 2);
#if UNITY_EDITOR
            //HobaDebuger.LogFormat("Lua FindChild {0}", childname);
#endif
            GameObject result = null;
            if (childname.Contains("/"))
            {
                Transform cur_node = obj.transform;

                //Split
                var sl = HobaText.GetStringList();
                var sb = HobaText.GetStringBuilder();
                for (int i = 0; i < childname.Length; ++i)
                {
                    char c = childname[i];
                    if (c == '/')
                    {
                        sl.Add(sb.ToString());      //use
                        sb.Length = 0;
                    }
                    else
                    {
                        sb.Append(c);
                    }
                }
                if (sb.Length > 0)
                {
                    sl.Add(sb.ToString());      //use
                    sb.Length = 0;
                }

                //Use
                for (int i = 0; i < sl.Count; i++)
                {
                    cur_node = cur_node.Find(sl[i]);
                    if (cur_node == null)
                    {
                        result = null;
                        break;
                    }
                    if (i == sl.Count - 1)
                        result = cur_node.gameObject;
                }
            }
            else
            {
                result = Util.FindChildDirect(obj, childname, true);
            }

            LuaScriptMgr.Push(L, result);
            return CheckReturnNum(L, count, nRet);
        }
        else
        {
            LogParamError("FindChild", count);
            LuaDLL.lua_pushnil(L);
            return CheckReturnNum(L, count, nRet);
        }
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int SetCurLayerVisible(IntPtr L)
    {
        Camera cam = Main.Main3DCamera;
        if (cam == null)
            return 0;

        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if ((count == 2 && LuaScriptMgr.CheckTypes(L, 1, typeof(int), typeof(bool))))
        {
            var layer = (int)LuaScriptMgr.GetNumber(L, 1);
            if (layer < 0)
            {
                HobaDebuger.LogWarning("SetCurLayerVisible: Layer can not be a negative number");
                return CheckReturnNum(L, count, nRet);
            }
            var isOn = LuaScriptMgr.GetBoolean(L, 2);
            var curMask = cam.cullingMask;
            var layerMask = 1 << layer;
            var newMask = isOn ? (curMask | layerMask) : (curMask & (~layerMask));
            cam.cullingMask = newMask;
        }
        else
        {
            LogParamError("SetCurLayerVisible", count);
        }

        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int SetLayerRecursively(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if ((count == 2 && LuaScriptMgr.CheckTypes(L, 1, typeof(GameObject), typeof(int))))
        {
            GameObject obj = LuaScriptMgr.GetUnityObject<GameObject>(L, 1);
            if (obj == null)
            {
                LuaScriptMgr.Instance.CallOnTraceBack();
                HobaDebuger.LogError("SetLayerRecursively: param 1 must be GameObject");
                return CheckReturnNum(L, count, nRet);
            }

            int layer = (int)LuaScriptMgr.GetNumber(L, 2);
            Util.SetLayerRecursively(obj, layer);
        }
        else
        {
            LogParamError("SetLayerRecursively", count);
        }

        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int GetMapHeight(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 1;

        if ((count == 1 && LuaScriptMgr.CheckTypes(L, 1, typeof(LuaTable))))
        {
            Vector3 pos = LuaScriptMgr.GetVector3(L, 1);
            float heigth = CUnityUtil.GetMapHeight(pos);
            LuaScriptMgr.Push(L, heigth);
        }
        else
        {
            LogParamError("GetMapHeight", count);
            LuaScriptMgr.Push(L, 0);
        }
        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int GetModelHeight(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 1;

        if ((count == 1 && LuaScriptMgr.CheckTypes(L, 1, typeof(GameObject))))
        {
            var m = LuaScriptMgr.GetUnityObject<GameObject>(L, 1);
            if(m != null)
            {
                float heigth = CUnityUtil.GetModelHeight(m);
                LuaScriptMgr.Push(L, heigth);
            }
            else
            {
                HobaDebuger.LogWarning("GetModelHeight param 1 is null");
                LuaScriptMgr.Push(L, 0);
            }
        }
        else if (count == 2 && LuaScriptMgr.CheckTypes(L, 1, typeof(GameObject), typeof(bool)))
        {
            var m = LuaScriptMgr.GetUnityObject<GameObject>(L, 1);
            if(m != null)
            {
                bool inchild = LuaScriptMgr.GetBoolean(L, 2);
                float heigth = CUnityUtil.GetModelHeight(m, inchild);
                LuaScriptMgr.Push(L, heigth);
            }
            else
            {
                HobaDebuger.LogWarning("GetModelHeight param 1 is null");
                LuaScriptMgr.Push(L, 0);
            }
        }
        else
        {
            LogParamError("GetModelHeight", count);
            LuaScriptMgr.Push(L, 0);
        }

        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int IsBlockedByObstacle(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 1;

        if (count == 2 && LuaScriptMgr.CheckTypes(L, 1, typeof(LuaTable), typeof(LuaTable)))
        {
            var pos1 = LuaScriptMgr.GetVector3(L, 1);
            pos1.y += 0.85f;
            var pos2 = LuaScriptMgr.GetVector3(L, 2);
            pos2.y += 0.85f;

            float raycast_distance = Vector3.Distance(pos1, pos2);
            Vector3 raycast_dir = pos2 - pos1;
            bool is_blocked = Physics.Raycast(pos1, raycast_dir, raycast_distance, CUnityUtil.LayerMaskTerrainBuilding);
            LuaScriptMgr.Push(L, is_blocked);
        }
        else
        {
            LogParamError("IsBlockedByObstacle", count);
            LuaScriptMgr.Push(L, false);
        }
        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int IsValidPosition(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 1;

        if (count == 1 && LuaScriptMgr.CheckTypes(L, 1, typeof(LuaTable)))
        {
            var pos1 = LuaScriptMgr.GetVector3(L, 1);
            //var pos2 = LuaScriptMgr.GetVector3(L, 2);

            bool is_valid = NavMeshManager.Instance.IsValidPositionStrict(pos1);
            LuaScriptMgr.Push(L, is_valid);
        }
        else
        {
            LogParamError("IsValidPosition", count);
            LuaScriptMgr.Push(L, false);
        }
        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int GetNearestValidPosition(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 1;

        if (count == 2 && LuaScriptMgr.CheckTypes(L, 1, typeof(LuaTable), typeof(float)))
        {
            var pos = LuaScriptMgr.GetVector3(L, 1);
            float searchScale = (float)LuaScriptMgr.GetNumber(L, 2);

            Vector3 nearest = pos; 
            bool is_valid = NavMeshManager.Instance.GetNearestValidPosition(pos, ref nearest, searchScale);
            if (is_valid)
                LuaScriptMgr.Push(L, nearest);
            else
                LuaDLL.lua_pushnil(L);
        }
        else
        {
            LogParamError("GetNearestValidPosition", count);
            LuaScriptMgr.Push(L, false);
        }
        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int IsValidPositionXZ(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 1;

        if (count == 2 && LuaScriptMgr.CheckTypes(L, 1, typeof(float), typeof(float)))
        {
            var x = (float)LuaScriptMgr.GetNumber(L, 1);
            var z = (float)LuaScriptMgr.GetNumber(L, 2);

            bool is_valid = NavMeshManager.Instance.IsValidPositionStrict(new Vector3(x, 0, z));
            LuaScriptMgr.Push(L, is_valid);
        }
        else
        {
            LogParamError("IsValidPosition", count);
            LuaScriptMgr.Push(L, false);
        }
        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int GenHmacMd5(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;
        if (count == 4)
        {
            var account = LuaScriptMgr.GetString(L, 1);
            var salt = LuaScriptMgr.GetString(L, 2);
            var password = LuaScriptMgr.GetString(L, 3);
            //LuaTypes luatype = LuaDLL.lua_type(L, 4);

            var lsb = LuaScriptMgr.GetStringBuffer(L, 4);

            //var lsb2 = LuaScriptMgr.GetString(L, 4);
            var all = account + salt + password;
            var md5Hasher = MD5.Create();
            var accountSaltPasswordMd5 = md5Hasher.ComputeHash(new UTF8Encoding(false).GetBytes(all));
            var hmacMd5Hasher = new HMACMD5(accountSaltPasswordMd5);
            var hmacMd5 = hmacMd5Hasher.ComputeHash(lsb.buffer);
            LuaDLL.lua_pushlstring(L, hmacMd5, hmacMd5.Length);
            return CheckReturnNum(L, count, nRet);
        }
        else
        {
            LogParamError("GenHmacMd5", count);
            return CheckReturnNum(L, count, nRet);
        }
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int MD5ComputeHash(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 1;

        if (count == 1)
        {
            var content = LuaScriptMgr.GetString(L, 1);
            var md5Hasher = MD5.Create();
            byte[] md5_content = md5Hasher.ComputeHash(new UTF8Encoding(false).GetBytes(content));
            LuaDLL.lua_pushlstring(L, md5_content, md5_content.Length);
        }
        else
        {
            LogParamError("MD5ComputeHash", count);
            LuaDLL.lua_pushnil(L);
        }
        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int HMACMD5ComputeHash(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 1;

        if (count == 2)
        {
            var key = LuaScriptMgr.GetString(L, 1);
            int len = 0;
            IntPtr pContent = LuaDLL.lua_tolstring(L, 2, out len);
            byte[] buffer = new byte[len];
            Marshal.Copy(pContent, buffer, 0, len);
            var hmacMd5Hasher = new HMACMD5(Encoding.UTF8.GetBytes(key));
            var hmacMd5 = hmacMd5Hasher.ComputeHash(buffer);
            LuaDLL.lua_pushlstring(L, hmacMd5, hmacMd5.Length);
        }
        else
        {
            LogParamError("HMACMD5ComputeHash", count);
            LuaDLL.lua_pushnil(L);
        }
        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int ComputeRNGCryptoNonce(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 1;

        if (count == 1)
        {
            int len = 0;
            IntPtr pContent = LuaDLL.lua_tolstring(L, 1, out len);
            byte[] buffer = new byte[len];
            Marshal.Copy(pContent, buffer, 0, len);
            var rng = new RNGCryptoServiceProvider();
            rng.GetBytes(buffer);
            LuaDLL.lua_pushlstring(L, buffer, buffer.Length);
        }
        else
        {
            LogParamError("ComputeRNGCryptoNonce", count);
            LuaDLL.lua_pushnil(L);
        }

        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int EnableRotate(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        int nRet = 0;

        if (count == 1 && LuaScriptMgr.CheckTypes(L, 1, typeof(GameObject)))
        {
            GameObject obj = LuaScriptMgr.GetUnityObject<GameObject>(L, 1);
            if (obj != null)
            {
                var comp = obj.GetComponent<ModelRotationComponent>();
                if (comp == null)
                    obj.AddComponent<ModelRotationComponent>();
            }
        }
        else if (count == 2 && LuaScriptMgr.CheckTypes(L, 1, typeof(GameObject), typeof(bool)))
        {
            GameObject obj = LuaScriptMgr.GetUnityObject<GameObject>(L, 1);
            bool bEnable = (bool)LuaScriptMgr.GetBoolean(L, 2);
            if (obj != null)
            {
                var comp = obj.GetComponent<ModelRotationComponent>();
                if (bEnable)
                {
                    if (comp == null)
                        obj.AddComponent<ModelRotationComponent>();
                }
                else
                {
                    if (comp != null)
                        UnityEngine.Object.Destroy(comp);
                }
            }
        }
        else
        {
            LogParamError("EnableRotate", count);
        }
        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int DoMove(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count ==6 && LuaScriptMgr.CheckTypes(L, 1, typeof(GameObject), typeof(LuaTable), typeof(float), typeof(int), typeof(float),typeof(LuaFunction)))
        {
            var obj = LuaScriptMgr.GetUnityObject<GameObject>(L, 1);
            if (obj == null)
            {
                HobaDebuger.LogError("GameUtil :: DoMove -> the GameObject is null.");
                return CheckReturnNum(L, count, nRet);
            }

            DoTweenComponent doTween = obj.GetComponent<DoTweenComponent>();
            if (doTween == null)
                doTween = obj.AddComponent<DoTweenComponent>();

            var toPos = LuaScriptMgr.GetVector3(L, 2);
            var interval = (float)LuaScriptMgr.GetNumber(L, 3);
            Ease easeType = (Ease)LuaScriptMgr.GetNumber(L, 4);
            var fDelay = (float)LuaScriptMgr.GetNumber(L, 5);
            LuaFunction callbackRef = LuaScriptMgr.GetLuaFunction(L, 6);
            doTween.DoMove(toPos, interval, easeType, fDelay, callbackRef);
        }
        else
        {
            LogParamError("DoMove", count);
        }

        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int DoLocalMove(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 5 && LuaScriptMgr.CheckTypes(L, 1, typeof(GameObject), typeof(LuaTable), typeof(float), typeof(int), typeof(LuaFunction)))
        {
            GameObject obj = LuaScriptMgr.GetUnityObject<GameObject>(L, 1);
            if (obj == null)
            {
                //LuaScriptMgr.Instance.CallOnTraceBack();
                HobaDebuger.LogError("GameUtil :: DoLocalMove -> the GameObject is null.");
                return CheckReturnNum(L, count, nRet);
            }

            DoTweenComponent doTween = obj.GetComponent<DoTweenComponent>();
            if (doTween == null)
                doTween = obj.AddComponent<DoTweenComponent>();

            var toPos = LuaScriptMgr.GetVector3(L, 2);
            float interval = (float)LuaScriptMgr.GetNumber(L, 3);
            Ease easeType = (Ease)LuaScriptMgr.GetNumber(L, 4);
            LuaFunction callbackRef = LuaScriptMgr.GetLuaFunction(L, 5);

            //Debug.Log("DoLocalMove " + obj.name+" to "+ toPos);
            doTween.DoLocalMove(toPos, interval, easeType, callbackRef);
        }
        else
        {
            LogParamError("DoLocalMove", count);
        }
        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int DoLocalRotateQuaternion(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 5 && LuaScriptMgr.CheckTypes(L, 1, typeof(GameObject), typeof(LuaTable), typeof(float), typeof(int), typeof(LuaFunction)))
        {
            GameObject obj = LuaScriptMgr.GetUnityObject<GameObject>(L, 1);
            if (obj == null)
            {
                HobaDebuger.LogError("GameUtil :: DoLocalRotateQuaternion -> the GameObject is null.");
                return CheckReturnNum(L, count, nRet);
            }

            DoTweenComponent doTween = obj.GetComponent<DoTweenComponent>();
            if (doTween == null)
                doTween = obj.AddComponent<DoTweenComponent>();

            var toPos = LuaScriptMgr.GetQuaternion(L, 2);
            float interval = (float)LuaScriptMgr.GetNumber(L, 3);
            Ease easeType = (Ease)LuaScriptMgr.GetNumber(L, 4);

            LuaFunction callbackRef = LuaScriptMgr.GetLuaFunction(L, 5);

            doTween.DoLocalRotateQuaternion(toPos, interval, easeType, callbackRef);
        }
        else
        {
            LogParamError("DOLocalRotateQuaternion", count);
        }
        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int DoLoopRotate(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 3 && LuaScriptMgr.CheckTypes(L, 1, typeof(GameObject), typeof(LuaTable), typeof(float)))
        {
            GameObject obj = LuaScriptMgr.GetUnityObject<GameObject>(L, 1);
            if (obj == null)
            {
                HobaDebuger.LogError("GameUtil :: DoRotation -> the GameObject is null.");
                return CheckReturnNum(L, count, nRet);
            }

            DoTweenComponent doTween = obj.GetComponent<DoTweenComponent>();
            if (doTween == null)
                doTween = obj.AddComponent<DoTweenComponent>();

            var endValue = LuaScriptMgr.GetVector3(L, 2);
            float interval = (float)LuaScriptMgr.GetNumber(L, 3);
            doTween.DoLoopRotate(endValue, interval);
        }
        else
        {
            LogParamError("DoLoopRotate", count);
        }

        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int DoScale(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 5 && LuaScriptMgr.CheckTypes(L, 1, typeof(GameObject), typeof(LuaTable), typeof(float), typeof(int), typeof(LuaFunction)))
        {
            GameObject obj = LuaScriptMgr.GetUnityObject<GameObject>(L, 1);
            if (obj == null)
            {
                HobaDebuger.LogError("GameUtil :: DoScale -> the GameObject is null.");
                return CheckReturnNum(L, count, nRet);
            }

            DoTweenComponent doTween = obj.GetComponent<DoTweenComponent>();
            if (doTween == null)
                doTween = obj.AddComponent<DoTweenComponent>();

            var endScale = LuaScriptMgr.GetVector3(L, 2);
            float interval = (float)LuaScriptMgr.GetNumber(L, 3);
            Ease easeType = (Ease)LuaScriptMgr.GetNumber(L, 4);

            LuaFunction callbackRef = LuaScriptMgr.GetLuaFunction(L, 5);

            doTween.DoScale(endScale, interval, easeType, callbackRef);
        }
        else
        {
            LogParamError("DoScale", count);
        }

        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int DoAlpha(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 4 && LuaScriptMgr.CheckTypes(L, 1, typeof(GameObject), typeof(float), typeof(float), typeof(LuaFunction)))
        {
            GameObject obj = LuaScriptMgr.GetUnityObject<GameObject>(L, 1);
            if (obj == null)
            {
                HobaDebuger.LogError("GameUtil :: DoAlpha -> the GameObject is null.");
                return CheckReturnNum(L, count, nRet);
            }

            DoTweenComponent doTween = obj.GetComponent<DoTweenComponent>();
            if (doTween == null)
                doTween = obj.AddComponent<DoTweenComponent>();

            float endValue = (float)LuaScriptMgr.GetNumber(L, 2);
            float interval = (float)LuaScriptMgr.GetNumber(L, 3);

            LuaFunction callbackRef = LuaScriptMgr.GetLuaFunction(L, 4);

            doTween.DoAlpha(endValue, interval, callbackRef);
        }
        else
        {
            LogParamError("DoAlpha", count);
        }

        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int DoKill(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 1 && LuaScriptMgr.CheckTypes(L, 1, typeof(GameObject)))
        {
            GameObject obj = LuaScriptMgr.GetUnityObject<GameObject>(L, 1);
            if (obj == null)
            {
                HobaDebuger.LogError("GameUtil :: DoKill -> the GameObject is null.");
                return CheckReturnNum(L, count, nRet);
            }
            //Debug.Log("Kill " + obj.name);
            DoTweenComponent doTween = obj.GetComponent<DoTweenComponent>();
            if (doTween != null)
            {
                doTween.KillAll();
            }

            DOTween.Kill(obj);
        }
        else
        {
            LogParamError("DoKill", count);
        }

        return CheckReturnNum(L, count, nRet);
    }
    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int ChangeSceneWeather(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 1 && LuaScriptMgr.CheckTypes(L, 1, typeof(int)))
        {
            //int weatherType = (int)LuaScriptMgr.GetNumber(L, 1);
            //ScenesManager.Instance.ChangeWeather((WeatherType)weatherType);
            int weatherId = (int)LuaScriptMgr.GetNumber(L, 1);
            ScenesManager.Instance.ChangeWeatherByEffectID(weatherId);
        }
        else
        {
            LogParamError("ChangeSceneWeather", count);
        }
        return CheckReturnNum(L, count, nRet);
    }

     [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int ChangeSceneWeatherByMemory(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 1 && LuaScriptMgr.CheckTypes(L, 1, typeof(int)))
        {
            int effectID = (int)LuaScriptMgr.GetNumber(L, 1);
            ScenesManager.Instance.ChangeWeatherByMemory(effectID);
        }
        else
        {
            LogParamError("ChangeSceneWeather", count);
        }
        return CheckReturnNum(L, count, nRet);
    }



   
    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int OnHostPlayerPosChange(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 2 && LuaScriptMgr.CheckTypes(L, 1, typeof(float), typeof(float)))
        {
            var posx = (float)LuaScriptMgr.GetNumber(L, 1);
            var posz = (float)LuaScriptMgr.GetNumber(L, 2);
           
            var pos = new Vector3(posx, 0, posz);
            ScenesManager.Instance.UpdateObjects(pos);
        }
        else
        {
            LogParamError("OnHostPlayerPosChange", count);
        }

        return CheckReturnNum(L, count, nRet);
    }
    
    /// <summary>
    /// 获取指定字符串的长度
    /// 以字符计
    /// </summary>
    /// <param name="L"></param>
    /// <returns></returns>
    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int GetStringLength(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 1;

        if ((count == 1 && LuaScriptMgr.CheckTypes(L, 1, typeof(string))))
        {
            string content = LuaDLL.lua_tostring(L, 1);
            int length = 0;
            for (int i = 0; i < content.Length; i++)
            {
                if ((content[i] >= 0 && content[i] <= 127) || (content[i] >= 194 && content[i] <= 244))
                {
                    length++;
                }
                else
                {
                    length = length + 2;
                }
            }
            LuaDLL.lua_pushinteger(L, length);
        }
        else
        {
            LogParamError("GetStringLength", count);
            LuaDLL.lua_pushnil(L);
        }
        return CheckReturnNum(L, count, nRet);
    }

    /// <summary>
    /// 设定指定字符串的长度
    /// 以字符计
    /// </summary>
    /// <param name="L"></param>
    /// <returns></returns>
    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int SetStringLength(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 1;

        if (count == 2 && LuaScriptMgr.CheckTypes(L, 1, typeof(string)))
        {
            string content = LuaDLL.lua_tostring(L, 1);
            int needLength = int.Parse(LuaDLL.lua_tostring(L, 2));
            if (needLength < 1)
            {
                HobaDebuger.LogWarning("SetStringLength should not be fushu:" + needLength);
                LuaDLL.lua_pushnil(L);
                return CheckReturnNum(L, count, nRet);
            }
            int countLength = 0;
            for (int i = 0; i < content.Length; i++)
            {
                if ((content[i] >= 0 && content[i] <= 127) || (content[i] >= 194 && content[i] <= 244))
                {
                    countLength++;
                }
                else
                {
                    countLength = countLength + 2;
                }
                if (countLength == needLength)
                {
                    LuaDLL.lua_pushstring(L, content.Substring(0, i + 1));
                    return CheckReturnNum(L, count, nRet);
                }
                else if (countLength > needLength)
                {
                    LuaDLL.lua_pushstring(L, content.Substring(0, i));
                    return CheckReturnNum(L, count, nRet);
                }
            }
            HobaDebuger.LogWarning("SetStringLength Unknown:" + needLength);
            LuaDLL.lua_pushnil(L);
            return CheckReturnNum(L, count, nRet);
        }
        else
        {
            LogParamError("SetStringLength", count);
            LuaDLL.lua_pushnil(L);
            return CheckReturnNum(L, count, nRet);
        }
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int DoScaleFrom(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 5 && LuaScriptMgr.CheckTypes(L, 1, typeof(GameObject), typeof(LuaTable), typeof(float), typeof(int), typeof(LuaFunction)))
        {
            GameObject obj = LuaScriptMgr.GetUnityObject<GameObject>(L, 1);
            if (obj == null)
            {
                HobaDebuger.LogError("GameUtil :: DoScaleFrom -> the GameObject is null.");
                return CheckReturnNum(L, count, nRet);
            }

            DoTweenComponent doTween = obj.GetComponent<DoTweenComponent>();
            if (doTween == null)
                doTween = obj.AddComponent<DoTweenComponent>();

            Vector3 fromScale = LuaScriptMgr.GetVector3(L, 2);
            float interval = (float)LuaScriptMgr.GetNumber(L, 3);
            Ease easeType = (Ease)LuaScriptMgr.GetNumber(L, 4);

            LuaFunction callbackRef = LuaScriptMgr.GetLuaFunction(L, 5);
            doTween.DoScaleFrom(fromScale, interval, easeType, callbackRef);
        }
        else
        {
            LogParamError("DoScaleFrom", count);
        }

        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int IsGameObjectInCamera(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 1;

        if (count == 2 && LuaScriptMgr.CheckTypes(L, 1, typeof(GameObject), typeof(LuaTable)))
        {
            GameObject obj = LuaScriptMgr.GetUnityObject<GameObject>(L, 1);
            if (obj == null)
            {
                HobaDebuger.LogError("IsGameObjectInCamera: param 1 must be GameObject");
                LuaScriptMgr.Push(L, false);
                return CheckReturnNum(L, count, nRet);
            }

            Vector3 size = LuaScriptMgr.GetVector3(L, 2);

            bool isIn = CCamCtrlMan.Instance.IsGameObjectInCamera(obj.transform, size);
            LuaScriptMgr.Push(L, isIn);
            return CheckReturnNum(L, count, nRet);
        }
        else
        {
            LogParamError("IsGameObjectInCamera", count);
            LuaScriptMgr.Push(L, false);
            return CheckReturnNum(L, count, nRet);
        }
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int SubUnicodeString(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 1;

        if ((count == 3 && LuaScriptMgr.CheckTypes(L, 1, typeof(string), typeof(int), typeof(int))) ||
            (count == 2 && LuaScriptMgr.CheckTypes(L, 1, typeof(string), typeof(int))))
        {
            string str = LuaScriptMgr.GetString(L, 1);
            if (string.IsNullOrEmpty(str))
            {
                HobaDebuger.LogWarning("SubUnicodeString string is null or empty");
                LuaScriptMgr.Push(L, "");
                return CheckReturnNum(L, count, nRet);
            }
            int startIndex = (int)LuaScriptMgr.GetNumber(L, 2) - 1;
            try
            {
                if (count == 2)
                    str = str.Substring(startIndex);
                else
                {
                    int length = (int)LuaScriptMgr.GetNumber(L, 3);
                    str = str.Substring(startIndex, length);
                }
                LuaScriptMgr.Push(L, str);
                return CheckReturnNum(L, count, nRet);
            }
            catch(Exception e)
            {
                HobaDebuger.LogErrorFormat("SubUnicodeString Exception: {0}", e.Message);
                LuaScriptMgr.Push(L, "");
                return CheckReturnNum(L, count, nRet);
            }
        }
        else
        {
            LogParamError("SubUnicodeString", count);
            LuaScriptMgr.Push(L, "");
            return CheckReturnNum(L, count, nRet);
        }
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int GetUnicodeStrLength(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 1;

        if (count == 1 && LuaScriptMgr.CheckTypes(L, 1, typeof(string)))
        {
            string str = LuaScriptMgr.GetString(L, 1);
            if (string.IsNullOrEmpty(str))
            {
                HobaDebuger.LogWarningFormat("GetUnicodeStrLength string is null or empty");
                LuaScriptMgr.Push(L, 0);
                return CheckReturnNum(L, count, nRet);
            }
            int length = str.Length;
            LuaScriptMgr.Push(L, length);
            return CheckReturnNum(L, count, nRet);
        }
        else
        {
            LogParamError("GetUnicodeStrLength", count);
            LuaScriptMgr.Push(L, 0);
            return CheckReturnNum(L, count, nRet);
        }
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int CheckName_ContainMainWord(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 1;

        if ((count == 1 && LuaScriptMgr.CheckTypes(L, 1, typeof(string))))
        {
            string name = LuaScriptMgr.GetString(L, 1);
            bool ret = Utilities.ContainMainWord(name);
            LuaScriptMgr.Push(L, ret);
        }
        else
        {
            LogParamError("CheckName_ContainMainWord", count);
            LuaScriptMgr.Push(L, false);
        }
        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int CheckName_IsValidWord(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 1;

        if ((count == 1 && LuaScriptMgr.CheckTypes(L, 1, typeof(string))))
        {
            string name = LuaScriptMgr.GetString(L, 1);
            bool ret = Utilities.IsValidWord(name);
            LuaScriptMgr.Push(L, ret);
        }
        else
        {
            LogParamError("CheckName_IsValidWord", count);
            LuaScriptMgr.Push(L, false);
        }
        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int CheckName_IsValidWord_KR(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 1;

        if ((count == 1 && LuaScriptMgr.CheckTypes(L, 1, typeof(string))))
        {
            string name = LuaScriptMgr.GetString(L, 1);
            bool ret = Utilities.IsValidWord_KR(name);
            LuaScriptMgr.Push(L, ret);
        }
        else
        {
            LogParamError("CheckName_IsValidWord_KR", count);
            LuaScriptMgr.Push(L, false);
        }
        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int GetScenePath(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 1;

        if (count == 1 && (LuaScriptMgr.CheckTypes(L, 1, typeof(GameObject))))
        {
            var go = LuaScriptMgr.GetUnityObject<GameObject>(L, 1);
            if (go == null)
            {
                HobaDebuger.LogError("GetScenePath: GameObject got null");
                LuaDLL.lua_pushstring(L, string.Empty);
                return CheckReturnNum(L, count, nRet);
            }

            Transform trans = go.transform;
            string s = "";
            while (trans != null)
            {
                s = trans.name + "/" + s;
                trans = trans.parent;
            }

            LuaDLL.lua_pushstring(L, s);
        }
        else
        {
            LuaDLL.lua_pushstring(L, string.Empty);
            LogParamError("GetScenePath", count);
        }
        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int CheckUserDataDir(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 0)
        {
            string dir = EntryPoint.Instance.UserDataDir;
            if (!System.IO.Directory.Exists(dir))
                System.IO.Directory.CreateDirectory(dir);
        }
        else
        {
            LogParamError("CheckUserDataDir", count);
        }
        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int QuitGame(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;
        HobaDebuger.Log("Manually Quited by Dounble Tapping the ESC Key!");

#if UNITY_EDITOR
        if (UnityEditor.EditorApplication.isPlaying)
        {
            UnityEditor.EditorApplication.isPlaying = false;
        }
#else
        EntryPoint.ExitGame();
#endif

        return CheckReturnNum(L, count, nRet);
    }
    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int ShowAlertView(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if ((count == 2 && LuaScriptMgr.CheckTypes(L, 1, typeof(string), typeof(string))))
        {
            string title = LuaScriptMgr.GetString(L, 1);
            string content = LuaScriptMgr.GetString(L, 2);
            OSUtility.ShowAlertView(title, content);
        }
        else
        {
            LogParamError("ShowAlertView", count);
        }
        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int CleanLocalNotification(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;
        Notification.LocalNotification.CleanNotification();

        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int RegisterLocalNotificationPermission(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;
        Notification.LocalNotification.RegisterPermission();

        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int RegisterLocalNotificationMessage(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;
        if (count == 5 && LuaScriptMgr.CheckTypes(L, 1, typeof(string), typeof(string), typeof(int), typeof(int), typeof(bool)))
        {
            string title = LuaScriptMgr.GetString(L, 1);
            string message = LuaScriptMgr.GetString(L, 2);
            int hour = (int)LuaScriptMgr.GetNumber(L, 3);
            int minute = (int)LuaScriptMgr.GetNumber(L, 4);
            bool isRepeatDay = LuaScriptMgr.GetBoolean(L, 5);
            Notification.LocalNotification.RegisterNotificationMessage(title, message, hour, minute, isRepeatDay);
        }
        else
        {
            LogParamError("RegisterNotificationMessage", count);
        }

        return CheckReturnNum(L, count, nRet);
    }
}