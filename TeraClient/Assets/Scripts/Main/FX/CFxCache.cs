using UnityEngine;
using System.Collections.Generic;
using System;

public class CFxCache
{
    public static Transform CachedFxsRoot = null;
    public static bool LoadAssetButNotPlay = false;

    private GameObject _Asset = null;
    private bool _IsLoading = false;
    private int _ActiveFxCount = 0;

    public float DeactiveTime { get; set; }
    public bool IsAutoClear = true;

    // CFxOne缓存
    private readonly List<GameObject> _CachedFxs = new List<GameObject>();

    // 加载过程中得到的CFxOne请求
    private readonly List<CFxOne> _DeferedGotFx = new List<CFxOne>();

    public void Init(string name)
    {
        if (_IsLoading || _Asset != null) return;

        Action<UnityEngine.Object> callback = (asset) =>
        {
            _Asset = asset as GameObject;
            _IsLoading = false;

            if (_Asset == null)
            {
                for (int i = 0; i < _DeferedGotFx.Count; ++i)
                {
                    var fxone = _DeferedGotFx[i];
                    fxone.Stop();
                }

                _DeferedGotFx.Clear();
            }
            else
            {
                for (int i = 0; i < _DeferedGotFx.Count; ++i)
                {
                    var fxone = _DeferedGotFx[i];
                    if (fxone.IsPlaying)
                    {
                        var fx = UnityEngine.Object.Instantiate(_Asset) as GameObject;
                        Transform trans = fx.transform;
                        trans.parent = fxone.transform;
                        trans.localPosition = Vector3.zero;
                        trans.localRotation = Quaternion.identity;
                        trans.localScale = Vector3.one;
                        Util.SetLayerRecursively(fx, fxone.gameObject.layer);
                        fxone.SetFxGameObject(fx);
                        fxone.DoRealPlay();
                    }
                    else
                        fxone.Stop();
                }
                _DeferedGotFx.Clear();
            }
        };
        _IsLoading = true;
        CAssetBundleManager.AsyncLoadResource(name, callback, false, "sfx");
    }

    public bool Touch(CFxOne fxone)
    {
        fxone.OnPlay = null;
        fxone.OnStop = null;

        if (!_IsLoading && _Asset == null)
            return false;

        ++_ActiveFxCount;
        DeactiveTime = -1;
        fxone.OnPlay = OnFxPlay;
        fxone.OnStop = OnFxStop;

        return true;
    }

    public void Clear()
    {
        _Asset = null;

        for(var i = 0; i < _CachedFxs.Count; i++)
            GameObject.Destroy(_CachedFxs[i]);
        _CachedFxs.Clear();

        _DeferedGotFx.Clear();
    }

    private void OnFxPlay(CFxOne fxone)
    {
        if (LoadAssetButNotPlay) return;

        if (_IsLoading)
        {
            _DeferedGotFx.Add(fxone);
        }
        else
        {
            GameObject fx = null;
            if (_CachedFxs.Count > 0)
            {
                fx = _CachedFxs[0];
                _CachedFxs.RemoveAt(0);
            }
            else
            {
                fx = UnityEngine.Object.Instantiate(_Asset) as GameObject;
            }
            Transform trans = fx.transform;
            trans.parent = fxone.transform;
            trans.localPosition = Vector3.zero;
            trans.localRotation = Quaternion.identity;
            trans.localScale = Vector3.one;
            //Util.SetLayerRecursively(fx, LayerMask.NameToLayer("Fx"));
            Util.SetLayerRecursively(fx, fxone.gameObject.layer);

            fxone.SetFxGameObject(fx);
        }
    }

    private void OnFxStop(CFxOne fxone)
    {
        if (_IsLoading)
            _DeferedGotFx.Remove(fxone);

        --_ActiveFxCount;

        if(_ActiveFxCount == 0)
            DeactiveTime = Time.time;

        // 将干净的RealFxGameObject缓存处理
        // 自身清理在CFxOne的OnStop
        var fx = fxone.GetFxGameObject();
        if (fx != null)
        {
            fx.transform.parent = CachedFxsRoot;
            fx.transform.localPosition = Vector3.zero;
            _CachedFxs.Add(fx);           
            fxone.SetFxGameObject(null);
        }

        // 清理FxOne，回收FxOne组件
        CFxCacheMan.Instance.RecycleFx(fxone);
    }
}
