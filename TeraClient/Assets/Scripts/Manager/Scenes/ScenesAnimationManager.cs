using Common;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScenesAnimationManager : Singleton<ScenesAnimationManager>
{

    //private List<Animation> _AnimationComponentList = new List<Animation>();

    private Dictionary<string, GameObject> objectDic = new Dictionary<string, GameObject>();

    private bool _IsNight = false;

    private const string CLIP_NAME = "lighton";
    //public void AddLightAnimation(GameObject go)
    //{
        
    //    var animations = go.GetComponentsInChildren<Animation>(true);
  
    //    for (int i = 0; i < animations.Length; i++)
    //    {
    //        _AnimationComponentList.Add(animations[i]);
    //        if (_IsNight && null != animations[i])
    //        {
    //            animations[i].Play();
    //        }
    //    }
    //}

    public void RemoveAnimationObj(string name)
    {
        if (objectDic.ContainsKey(name))
        {
            objectDic[name] = null;
            //objectDic.Remove(name);
        }
    }

    public void AddAnimaitonObj(string name, GameObject go)
    {
        if (null == go )return;
        if (objectDic.ContainsKey(name))
        {
            objectDic[name] = go;
        }
        else
        {
            objectDic.Add(name, go);
        }

        if (_IsNight)
        {
            var animations = go.GetComponentsInChildren<Animation>();
            for (int i = 0; i < animations.Length; ++i)
            {
                var animationComp = animations[i];
                if (null != animationComp[CLIP_NAME])
                {
                    animationComp[CLIP_NAME].speed = 1f;
                    animationComp.Play(CLIP_NAME);
                }
            }
        }

    }

    public void Init(bool isNight)
    {
        _IsNight = isNight;
    }
    public void OpenLightAnimaiotn()
    {

        var itor = objectDic.GetEnumerator();
        while (itor.MoveNext())
        {
            var go = itor.Current.Value;
            if (null == go) continue;
            var animations = go.GetComponentsInChildren<Animation>();
            for (int i = 0; i < animations.Length; ++i)
            {
                var animationComp = animations[i];
                if (null != animationComp[CLIP_NAME])
                {
                    animationComp[CLIP_NAME].speed = 1f;
                    animationComp.Play(CLIP_NAME);
                }
            }
        }
        itor.Dispose();

        
        //for (int i = 0; i < objectDic.Count; i++)
        //{
        //   var go = objectDic[i];

        //   var animations = go.GetComponentsInChildren<Animation>();

            
        //   if (null!= animationComp[CLIP_NAME])
        //   {
        //       animationComp[CLIP_NAME].speed = 1f;
        //       animationComp.Play(CLIP_NAME);
        //   }
           
        //}
    }

    public void CloseAnimaiontion()
    {
        //for (int i = 0; i < _AnimationComponentList.Count; i++)
        //{
        //    var animationComp = _AnimationComponentList[i];
        //    if (null != animationComp[CLIP_NAME])
        //    {
        //        animationComp[CLIP_NAME].time = animationComp[CLIP_NAME].length;
        //        animationComp[CLIP_NAME].speed = -1f;
        //        animationComp.Play(CLIP_NAME);
        //    }
        //}

        var itor = objectDic.GetEnumerator();
        while (itor.MoveNext())
        {
            var go = itor.Current.Value;
            if (null == go) continue;
            var animations = go.GetComponentsInChildren<Animation>();
            for (int i = 0; i < animations.Length; ++i)
            {
                var animationComp = animations[i];
                if (null != animationComp[CLIP_NAME])
                {
                    animationComp[CLIP_NAME].time = animationComp[CLIP_NAME].length;
                    animationComp[CLIP_NAME].speed = -1f;
                    animationComp.Play(CLIP_NAME);
                }
            }
        }
        itor.Dispose();
    }
 
    public void Clear()
    {
        objectDic.Clear();
    }

}
