using UnityEngine;
using System;
using LuaInterface;
using Common;
using EntityComponent;
using UnityEngine.UI;

public static partial class GameUtilWrap
{
    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int AsyncLoad(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count >= 2 && LuaScriptMgr.CheckTypes(L, 1, typeof(string), typeof(LuaFunction)))
        {
            string assetname = LuaScriptMgr.GetString(L, 1);
            //Debug.Log("C#====assetname" + assetname);
            if (string.IsNullOrEmpty(assetname))
            {
                HobaDebuger.LogError("GameUtilWrap.AsyncLoad's argument #1 can not be empty");
                return CheckReturnNum(L, count, nRet);
            }

            if (!LuaDLL.lua_isfunction(L, 2))
            {
                HobaDebuger.LogError("GameUtilWrap.AsyncLoad: second param must be function");
                return CheckReturnNum(L, count, nRet);
            }
            // assetname, cb,
            LuaDLL.lua_pushvalue(L, 2);  // assetname, cb, cb
            int callbackRef = LuaDLL.luaL_ref(L, LuaIndexes.LUA_REGISTRYINDEX); // assetname, cb

            var needInstantiate = LuaScriptMgr.GetBoolean(L, 3);
            DoAsyncLoadResource(L, assetname, callbackRef, needInstantiate);
            return CheckReturnNum(L, count, nRet);
        }
        else
        {
            LogParamError("AsyncLoad", count);
            return CheckReturnNum(L, count, nRet);
        }
    }

    private static void DoAsyncLoadResource(IntPtr L, string assetname, int callbackRef, bool needInstantiate)
    {
        Action<UnityEngine.Object> callback = (asset) =>
        {
            if (LuaScriptMgr.Instance.GetLuaState() == null)
                return;

            var oldTop = LuaDLL.lua_gettop(L);
            LuaDLL.lua_rawgeti(L, LuaIndexes.LUA_REGISTRYINDEX, callbackRef); // cb
            LuaDLL.luaL_unref(L, LuaIndexes.LUA_REGISTRYINDEX, callbackRef);
            if (LuaDLL.lua_isnil(L, -1))
            {
                LuaDLL.lua_settop(L, oldTop);
                return;
            }

            LuaDLL.lua_pushvalue(L, -1);    //-> cb, cb
            if (asset != null)
                LuaScriptMgr.Push(L, asset);
            else
                LuaDLL.lua_pushnil(L);

            if (!LuaScriptMgr.Instance.GetLuaState().PCall(1, 0)) //-> cb, [err]
            {
                HobaDebuger.LogLuaError(LuaDLL.lua_tostring(L, -1));
            }
            LuaDLL.lua_settop(L, oldTop);
        };
        CAssetBundleManager.AsyncLoadResource(assetname, callback, needInstantiate);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int UnloadBundle(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 1 && LuaScriptMgr.CheckTypes(L, 1, typeof(string)))
        {
            var bundleName = LuaScriptMgr.GetString(L, 1);
            CAssetBundleManager.UnloadBundle(bundleName);
        }
        
        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int UnloadBundleOfAsset(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 1 && LuaScriptMgr.CheckTypes(L, 1, typeof(string)))
        {
            var assetName = LuaScriptMgr.GetString(L, 1);
            CAssetBundleManager.Instance.UnloadBundleOfAsset(assetName);
        }

        return CheckReturnNum(L, count, nRet);
    }
    
    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int ClearAssetBundleCache(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 1 && LuaScriptMgr.CheckTypes(L, 1, typeof(bool)))
        {
            bool clearAll = LuaScriptMgr.GetBoolean(L, 1);
            CAssetBundleManager.Instance.ClearCaches(clearAll);
        }
        else
        {
            LogParamError("ClearAssetCaches", count);
            LuaDLL.lua_pushnil(L);
        }

        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int RequestFx(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 2;

        if (LuaDLL.lua_isstring(L, 1) == false)
        {
            HobaDebuger.LogError("RequestFx: param 1 must be string");
            LuaDLL.lua_pushnil(L);
            LuaScriptMgr.Push(L, 0);
            return CheckReturnNum(L, count, nRet);
        }
        if (LuaDLL.lua_isboolean(L, 2) == false)
        {
            HobaDebuger.LogError("RequestFx: param 2 must be boolean");
            LuaDLL.lua_pushnil(L);
            LuaScriptMgr.Push(L, 0);
            return CheckReturnNum(L, count, nRet);
        }
        string name = LuaScriptMgr.GetString(L, 1);
        bool fixRot = (bool)LuaScriptMgr.GetBoolean(L, 2);
        int priority = 50;
        if (count > 2)
            priority = (int)LuaScriptMgr.GetNumber(L, 3);

        int fxId = 0;
        CFxOne fxone = CFxCacheMan.Instance.RequestFxOne(name, priority, out fxId);
        if (fxone != null)
        {
            fxone.IsFixRot = fixRot;
            LuaScriptMgr.Push(L, fxone.gameObject);
            LuaScriptMgr.Push(L, fxId);
        }
        else
        {
            LuaDLL.lua_pushnil(L);
            LuaScriptMgr.Push(L, fxId);
        }

        return CheckReturnNum(L, count, nRet);
    }
    
    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int PreloadFxAsset(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (LuaDLL.lua_isstring(L, 1) == false)
        {
            HobaDebuger.LogError("PreLoadHostFx: param 1 must be string");
            LuaDLL.lua_pushnil(L);
            LuaScriptMgr.Push(L, 0);
            return CheckReturnNum(L, count, nRet);
        }
        string name = LuaScriptMgr.GetString(L, 1);
        CFxCacheMan.Instance.PreloadFxAsset(name);
        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int ClearFxManCache(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        CFxCacheMan.Instance.ClearCaches();

        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int FxCacheManCleanup(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        CFxCacheMan.Instance.Cleanup();

        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int SetFxScale(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 2 && LuaScriptMgr.CheckTypes(L, 1, typeof(GameObject), typeof(float)))
        {
            var go = LuaScriptMgr.GetUnityObject<GameObject>(L, 1);
            if (go != null)
            {
                var s = (float)LuaScriptMgr.GetNumber(L, 2);
                var fxOne = go.GetComponent<CFxOne>();
                fxOne.SetScale(s);
            }
        }
        else
        {
            HobaDebuger.LogError("invalid arguments to method: GameUtilWrap.ResetGameObject");
        }

        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int SetMineObjectScale(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 2 && LuaScriptMgr.CheckTypes(L, 1, typeof(GameObject), typeof(float)))
        {
            var go = LuaScriptMgr.GetUnityObject<GameObject>(L, 1);
            if (go != null)
            {
                var scale = (float) LuaScriptMgr.GetNumber(L, 2);
                go.transform.localScale = new Vector3(scale, scale, scale);

                var projectors = go.GetComponentsInChildren<Projector>();
                foreach (var v in projectors)
                    v.orthographicSize = scale * v.orthographicSize;

                var scaleCurves = go.GetComponentsInChildren<RFX4_ScaleCurves>();
                foreach (var v in scaleCurves)
                {
                    v.SetScaleFactor(scale);
                    v.SetActive(true);
                }
            }
        }
        else
        {
            HobaDebuger.LogError("invalid arguments to method: GameUtilWrap.SetMineFxScale");
        }

        return CheckReturnNum(L, count, nRet);
    }


    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int BluntAttachedFxs(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 3 && LuaScriptMgr.CheckTypes(L, 1, typeof(GameObject), typeof(float), typeof(float)))
        {
            var go = LuaScriptMgr.GetUnityObject<GameObject>(L, 1);
            if (go != null)
            {
                var lastTime = (float)LuaScriptMgr.GetNumber(L, 2);
                var fixedSpeed = (float)LuaScriptMgr.GetNumber(L, 3);
                var fxs = go.GetComponentsInChildren<CFxOne>();
                for(var i = 0; i < fxs.Length; i++)
                    fxs[i].Blunt(lastTime, fixedSpeed);
            }
        }
        else
        {
            HobaDebuger.LogError("invalid arguments to method: GameUtilWrap.BluntAttachedFxs");
        }

        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int RequestUncachedFx(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 1;

        if (count == 1 && LuaScriptMgr.CheckTypes(L, 1, typeof(string)))
        {
            string path = LuaScriptMgr.GetString(L, 1);
            var fxone = CFxCacheMan.Instance.RequestUncachedFx(path);
            LuaScriptMgr.Push(L, fxone.gameObject);
        }
        else if (count == 2 && LuaScriptMgr.CheckTypes(L, 1, typeof(string), typeof(bool)))
        {
            string path = LuaScriptMgr.GetString(L, 1);
            bool isSingleton = LuaScriptMgr.GetBoolean(L, 2);
            var fxone = CFxCacheMan.Instance.RequestUncachedFx(path, isSingleton);
            LuaScriptMgr.Push(L, fxone.gameObject);
        }
        else
        {
            LogParamError("RequestUncachedFx", count);
            LuaDLL.lua_pushnil(L);
        }

        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int RequestArcFx(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 2;

        if (LuaDLL.lua_isstring(L, 1) == false)
        {
            HobaDebuger.LogError("GetArcFx: param 1 must be string");
            LuaDLL.lua_pushnil(L);
            LuaScriptMgr.Push(L, 0);
            return CheckReturnNum(L, count, nRet);
        }

        string name = LuaScriptMgr.GetString(L, 1);
        var owner = LuaScriptMgr.GetUnityObject<GameObject>(L, 2);
        var target = LuaScriptMgr.GetUnityObject<GameObject>(L, 3);

        if (owner == null || target == null)
        {
            HobaDebuger.LogError("GetArcFx: param 2 or 3 is null");
            LuaDLL.lua_pushnil(L);
            LuaScriptMgr.Push(L, 0);
            return CheckReturnNum(L, count, nRet);
        }

        int priority = 0;
        if (count > 3)
            priority = (int)LuaScriptMgr.GetNumber(L, 4);

        int fxId = 0;
        CFxOne fxone = CFxCacheMan.Instance.RequestFxOne(name, priority, out fxId);
        if (fxone != null)
        {
            fxone.IsFixRot = false;
            if (fxone.ArcReactor == null)
                fxone.ArcReactor = new ArcReactorInfo();
            fxone.ArcReactor.Set(owner.transform, target.transform);

            LuaScriptMgr.Push(L, fxone.gameObject);
            LuaScriptMgr.Push(L, fxId);
        }
        else
        {
            LuaDLL.lua_pushnil(L);
            LuaScriptMgr.Push(L, fxId);
        }

        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int LoadSceneBlocks(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 3 && LuaScriptMgr.CheckTypes(L, 1, typeof(float), typeof(float), typeof(LuaFunction)))
        {
            var posx = (float)LuaScriptMgr.GetNumber(L, 1);
            var posz = (float)LuaScriptMgr.GetNumber(L, 2);
            LuaDLL.lua_pushvalue(L, 3);
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
            ScenesManager.Instance.LoadObjectsAtPos(posx, posz, callback);

            return CheckReturnNum(L, count, nRet);
        }
        else
        {
            LogParamError("OnWorldLoadedBlocks", count);
            return CheckReturnNum(L, count, nRet);
        }
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int FetchResFromCache(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 1;

        if (count == 1 && LuaScriptMgr.CheckTypes(L, 1, typeof(string)) || LuaScriptMgr.CheckTypes(L, 1, typeof(int)))
        {
            var assetPathId = 0;
            if (LuaDLL.lua_isnumber(L, 1))
                assetPathId = (int)LuaDLL.lua_tonumber(L, 1);
            else
            {
                string resPath = LuaDLL.lua_tostring(L, 1);
                assetPathId = resPath.GetHashCode();
            }

            ResCacheMan.CacheItem item;

            if (ResCacheMan.Instance.FetchResFromCache(assetPathId, out item))
            {
                LuaScriptMgr.Push(L, item.Model);
            }
            else
            {
                LuaDLL.lua_pushnil(L);
            }
        }
        else
        {
            LuaDLL.lua_pushnil(L);
            LogParamError("FetchResFromCache", count);
        }

        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int AddResToCache(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 2 && (LuaScriptMgr.CheckTypes(L, 1, typeof(int), typeof(GameObject)) || LuaScriptMgr.CheckTypes(L, 1, typeof(string), typeof(GameObject))))
        {
            var assetPathId = 0;
            if (LuaDLL.lua_isnumber(L, 1))
                assetPathId = (int)LuaDLL.lua_tonumber(L, 1);
            else
            {
                string resPath = LuaDLL.lua_tostring(L, 1);
                assetPathId = resPath.GetHashCode();
            }

            GameObject go = LuaScriptMgr.GetUnityObject<GameObject>(L, 2);
            if (null == go)
            {
                HobaDebuger.LogWarning("AddResToCache: param 2 must be GameObject");
                return CheckReturnNum(L, count, nRet);
            }
            else
            {
                ResCacheMan.Instance.AddResToCache(assetPathId, go);
            }
        }
        else
        {
            LogParamError("AddResToCache", count);
        }
        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int GetEntityBaseRes(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 1;

        if (count == 0)
        {
            GameObject res = ResCacheMan.Instance.GetEntityBaseRes();
            LuaScriptMgr.Push(L, res);
        }
        else
        {
            LuaDLL.lua_pushnil(L);
            LogParamError("GetEntityBaseRes", count);
        }

        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int RecycleEntityBaseRes(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 1 && LuaScriptMgr.CheckTypes(L, 1, typeof(GameObject)))
        {
            var go = LuaScriptMgr.GetUnityObject<GameObject>(L, 1);
            ResCacheMan.Instance.RecycleEntityBaseRes(go);
        }
        else
        {
            LogParamError("RecycleEntityBaseRes", count);
        }
        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int ClearEntityModelCache(IntPtr L)
    {
        ResCacheMan.Instance.Cleanup();
        return 0;
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int PlayEarlyWarningGfx(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 2;

        if (count == 6 && LuaScriptMgr.CheckTypes(L, 1, typeof(string), typeof(LuaTable), typeof(LuaTable), typeof(LuaTable), typeof(float),typeof(bool)))
        {
            float lifeTime = (float)LuaScriptMgr.GetNumber(L, 5);
            if (lifeTime < 0.01f)
            {
                LuaDLL.lua_pushnil(L);
                LuaScriptMgr.Push(L, 0);
                return CheckReturnNum(L, count, nRet);
            }

            string gfxName = LuaScriptMgr.GetString(L, 1);
            int fxId = 0;
            CFxOne fx = CFxCacheMan.Instance.RequestFxOne(gfxName, -1, out fxId);
            if (fx == null)
            {
                LuaDLL.lua_pushnil(L);
                LuaScriptMgr.Push(L, 0);
                return CheckReturnNum(L, count, nRet);
            }

            Vector3 pos = LuaScriptMgr.GetVector3(L, 2);
            Vector3 dir = LuaScriptMgr.GetVector3(L, 3);
            Vector3 scale = LuaScriptMgr.GetVector3(L, 4);
            bool doNotUseProjector = LuaScriptMgr.GetBoolean(L,6);


            Transform trans = fx.transform;
            trans.position = pos;
            trans.rotation = CMapUtil.GetMapNormalRotationWithDistance(pos, dir, Math.Max(scale.x, scale.z));   //法线为地面法线
            trans.localScale = Vector3.one;

            fx.IsFixRot = false;
            if (fx.EarlyWarning == null)
                fx.EarlyWarning = new EarlyWarningInfo();
            fx.EarlyWarning.Set(Time.time, lifeTime, scale, doNotUseProjector);

            fx.Play(lifeTime + 0.2f);

            LuaScriptMgr.Push(L, fx.gameObject);
            LuaScriptMgr.Push(L, fx.ID);
        }
        else
        {
            LogParamError("PlaySkillIndicatorGfx", count);
            LuaDLL.lua_pushnil(L);
            LuaScriptMgr.Push(L, 0);
        }
        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int StopGfx(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        if (count == 2 && LuaScriptMgr.CheckTypes(L, 1, typeof(GameObject), typeof(int)))
        {
            var fx = LuaScriptMgr.GetUnityObject<GameObject>(L, 1);
            var fxone = fx != null ? fx.GetComponent<CFxOne>() : null;
            if (fxone == null) return 0;

            var id = (int)LuaDLL.lua_tonumber(L, 2);
            if (fxone.ID == id)
                fxone.Stop();

            // 按照ID生成规则，绝大多数情况下复用时ID都会增长，故以此规则添加有效性检查
            if(fxone.ID < id && fxone.ID > 0)
            {
#if UNITY_STANDALONE_WIN
                HobaDebuger.LogErrorFormat("logic error occurs when call StopGfx: current id = {0} old id = {1}", fxone.ID, id);
#endif
            }
            //LuaScriptMgr.Push(L, fxone.ID != 0);
        }
        else
        {
            LogParamError("StopGfx", count);
            //LuaScriptMgr.Push(L, false);
        }

        return 0;
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int ChangeGfxPlaySpeed(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        if (count == 2 && LuaScriptMgr.CheckTypes(L, 1, typeof(GameObject), typeof(int)))
        {
            var fx = LuaScriptMgr.GetUnityObject<GameObject>(L, 1);
            var fxone = fx != null ? fx.GetComponent<CFxOne>() : null;
            if (fxone == null) return 0;

            var speed = (float)LuaDLL.lua_tonumber(L, 2);
            fxone.ChangePlaySpeed(speed);
        }
        else
        {
            LogParamError("ChangeGfxPlaySpeed", count);
        }

        return 0;
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int GetEmojiCount(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        if (count == 0)
        {
            var emotions = CEmojiManager.Instance.EmotionAssets;
            if (emotions != null)
                LuaScriptMgr.Push(L, emotions.EmojiCount);
            else
                LuaDLL.lua_pushinteger(L, 0);
        }
        else
        {
            LuaDLL.lua_pushinteger(L, 0);
            LogParamError("GetEmotionCount", count);
        }

        return 1;
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int SetEmojiSprite(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        if (count == 2 && LuaScriptMgr.CheckTypes(L, 1, typeof(GameObject), typeof(int)))
        {
            var gameObject = LuaScriptMgr.GetUnityObject<GameObject>(L, 1);
            if (gameObject != null)
            {
                var image = gameObject.GetComponent<Image>();
                if (image != null)
                {
                    var index = (int)LuaDLL.lua_tonumber(L, 2);
                    var emoji = CEmojiManager.Instance.GetEmotionAssetById(CEmojiManager.EmojiIndex2EmojiID(index));
                    if (emoji != null)
                        image.sprite = emoji.IconSprite;
                }
            }
        }
        else
        {
            LogParamError("SetEmojiSprite", count);
        }

        return 0;
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int InputEmoji(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        if (count == 2 && LuaScriptMgr.CheckTypes(L, 1, typeof(InputField), typeof(int)))
        {
            var inputField = LuaScriptMgr.GetUnityObject<InputField>(L, 1);
            if (inputField != null)
            {
                var index = (int)LuaDLL.lua_tonumber(L, 2);
                //var emoji = CEmojiManager.Instance.GetEmotionAssetById(index);
                //if (emoji != null)
                {
                    var oldContent = inputField.text;
                    inputField.text = HobaText.Format("{0}[e]{1}[-]", oldContent, CEmojiManager.EmojiIndex2EmojiID(index));
                }
            }
        }
        else
        {
            LogParamError("InputEmoji", count);
        }
        return 0;
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int ClearAllEmoji(IntPtr L)
    {
        CEmojiManager.Instance.Cleanup();
        return 0;
    }
}
