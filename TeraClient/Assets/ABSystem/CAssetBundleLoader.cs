#if USE_OBSOLETE
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityObject = UnityEngine.Object;


class CAssetBundleLoader
{
    private static TaskQueue _TaskQueue = new TaskQueue("AssetBundleLoader");
    public static TaskQueue GetTaskQueue()
    {
        _TaskQueue.CheckInit();
        return _TaskQueue;
    }

    public static void AsyncLoadAsset(UnityObject bundle, string path, Action<UnityObject> onFinish)
    {
        GetTaskQueue().AddCoroutineInMainThread(AsyncLoadAssetCoroutine(bundle, onFinish, path));
    }

    public static void AsyncLoadBundle(string path, Action<UnityObject> onFinish)
    {
        GetTaskQueue().AddCoroutineInMainThread(AsyncLoadBundleCoroutine(onFinish, path));
    }

    private static IEnumerator AsyncLoadBundleCoroutine(Action<UnityObject> onFinish, string path)
    {
        var bundle = AssetBundle.LoadFromFile(path);
        if (bundle == null)
        {
            if (onFinish != null)
                onFinish(null);
            yield break;
        }

        if (onFinish != null)
            onFinish(bundle);

        yield return 1;
    }

    private static IEnumerator AsyncLoadAssetCoroutine(UnityObject bundle, Action<UnityObject> onFinish, string path)
    {
        AssetBundle ab = bundle as AssetBundle;

        if (ab == null || (path != "" && !ab.Contains(path)))
        {
            if (onFinish != null)
                onFinish(null);
            yield break;
        }

        if (path != "")
        {
//#if false
            //UnityObject asset = ab.LoadAsset(path);
            //if (onFinish != null)
            //    onFinish(asset);
//#else
            // 在设备上，LoadAsync同时开启两个以上，就会导致loading.lockpersistentmanager，出现卡顿
            var request = ab.LoadAssetAsync(path);

            while (!request.isDone)
                yield return null;

            if (request != null && request.isDone)
            {
                UnityObject asset = request.asset;
                if (onFinish != null)
                    onFinish(asset);
            }
//#endif
        }
        else
        {
            if (onFinish != null)
                onFinish(ab);
        }
    }
}
#endif