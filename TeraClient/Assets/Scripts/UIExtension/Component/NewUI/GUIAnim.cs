using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

//1 only at the same time
//will call OnAnimEvent func in lua


namespace GNewUI
{
    public enum AimEventType
    {
        Anim = 0,
        Sound,
        Fx,
        AnimTransition,
        Total
    }


    [System.Serializable]
    public class EventBlock : System.Object
    {
        public string Name;
    }

    [System.Serializable]
    public class AnimEventBlock : EventBlock
    {
        public Animation Anim;
        public string AnimName;
        public float CrossFade;
    }

    [System.Serializable]
    public class SoundEventBlock : EventBlock
    {
        public string SndPath;
        public GameObject Node;
        public float LifeTime;
        public bool Is2D;
    }

    [System.Serializable]
    public class FxEventBlock : EventBlock
    {
        public string FxPath;
        public bool IsUI;
        public Transform Anchor;
        public Transform Hook;
        public float LifeTime;
    }

    [System.Serializable]
    public class AnimTransition : EventBlock
    {
        public Animation Anim;
        public string[] Froms;
        public float[] CrossFadeTimes;
    }

    public class GUIAnim : GNewUIBase
    {
        [NoToLua]
        [HideInInspector]
        public float TimeMark = 0;

        [NoToLua]
        public List<AnimEventBlock> AnimEvents = new List<AnimEventBlock>();
        [NoToLua]
        public List<SoundEventBlock> SoundEvents = new List<SoundEventBlock>();
        [NoToLua]
        public List<FxEventBlock> FxEvents = new List<FxEventBlock>();
        [NoToLua]
        public List<AnimTransition> AnimTrans = new List<AnimTransition>(); //override anim fade


        [NoToLua]
        [System.NonSerialized]
        public UnityEngine.Events.UnityAction<GameObject, string> OnSequenceEvent, OnSequenceEnd;

        #region privates

        protected Transform _CachedT;
        protected Animation _Animation;

        protected AnimationState _CurState = null;
        protected float _CurTime = 0;

        [NoToLua]
        public bool IsPlaying { get { return _Animation == null ? false : _Animation.isPlaying; } }
        [NoToLua]
        public string CurSequence { get { return IsPlaying ? _CurState.name : string.Empty; } }
        [NoToLua]
        public float CurTime { get { return IsPlaying ? _CurTime : 0; } }

        private void _PlayAnim(Animation anim, string ani_name, float cross_fade)
        {
            if (anim != null)
            {
                float fade = GetAnimTransitionFade(anim, ani_name);
                if (fade > 0)
                {
                    anim.CrossFade(ani_name, fade);
                }
                else
                {
                    //if(anim.IsPlaying(ani_name){}??? not sure fade between the same clip
                    anim.CrossFade(ani_name, cross_fade);
                }
#if ART_USE
                Debug.Log("PlayAnim " + anim.gameObject.name + ", " + ani_name);
#endif
            }
        }

        //        private void _PlayFx(Transform hook, Transform anchor, string fx_path, float life_time, bool is_ui)
        //        {
        //            if (!is_ui)
        //            {
        //                if (hook == null) hook = _CachedT;
        //#if ART_USE && UNITY_EDITOR
        //                GameObject obj = UnityEditor.AssetDatabase.LoadAssetAtPath(fx_path, typeof(GameObject)) as GameObject;
        //                if (obj != null)
        //                {
        //                    obj = Instantiate<GameObject>(obj);
        //                    Transform t = obj.transform;
        //                    t.SetParent(hook, false);
        //                    obj.SetActive(true);
        //                    Util.SetLayerRecursively(obj, LayerMask.NameToLayer("UIScene"));

        //                    //re-position
        //                    if (anchor != null && anchor != hook)
        //                    {
        //                        t.position = anchor.position;
        //                    }

        //                    if (life_time < 0.0001f)
        //                    {
        //                        FxDuration fxd = obj.GetComponent<FxDuration>();
        //                        if (fxd != null) life_time = fxd.duration;
        //                    }

        //                    Destroy(obj, life_time > 0.0001f ? life_time : 5);

        //                    Debug.Log("PlayFx " + hook.name + ", " + fx_path + ", " + life_time);
        //                }

        //#else
        //                //Debug.Log("TODO: PlayFx");
        //#if IN_Game
        //                 int fxId = 0;
        //                 CFxOne fx_one = CFxCacheMan.Instance.RequestFxOne(fx_path, -1, ref fxId);
        //                 if (fx_one != null)
        //                 {
        //                     Transform t_fx=fx_one.transform;
        //                     GameObject g_fx = fx_one.gameObject;
        //                     t_fx.SetParent(hook, false);

        //                     //re-position
        //                    if (anchor != null && anchor != hook)
        //                     {
        //                         t_fx.position = anchor.position;
        //                     }

        //                     //g_fx.SetActive(true);
        //                     Util.SetLayerRecursively(g_fx, LayerMask.NameToLayer("UIScene"));

        //                     fx_one.Play(life_time > 0.0001f ? life_time : 5);
        //                 }
        //#endif
        //#endif
        //            }
        //            else
        //            {
        //                UISfxBehaviour.Play(fx_path, anchor.gameObject, hook.gameObject, null, life_time > 0.0001f ? life_time : 5);
        //            }
        //        }

        private void _PlaySound(GameObject go, string snd_path, float life_time, bool is_2D)
        {
            if (go == null) return;
#if ART_USE && UNITY_EDITOR
            AudioClip clip = UnityEditor.AssetDatabase.LoadAssetAtPath(snd_path, typeof(AudioClip)) as AudioClip;
            if (clip != null)
            {
                AudioSource source = go.AddComponent<AudioSource>();
                source.clip = clip;
                source.spatialBlend = is_2D ? 0 : 1;
                source.Play();

                Destroy(source, life_time > 0.0001f ? life_time : clip.length);
            }

            Debug.Log("PlaySound " + go.name + ", " + snd_path);
#elif IN_GAME
            //Debug.Log("TODO: PlaySound");

            ISoundMan soundMan = EntryPoint.Instance.SoundMan;
            if (soundMan != null)
            {
                if (is_2D)
                {
                    soundMan.Play2DAudio(snd_path);
                }
                else
                {
                    soundMan.PlayAttached3DAudio(snd_path, go.transform);
                }
            }
#endif
        }

        private T FindEventBlock<T>(string name, List<T> list, int from = 0) where T : EventBlock
        {
            for (int i = from; i < list.Count; i++)
            {
                if (list[i].Name == name)
                {
                    return list[i];
                }
            }
            return null;
        }

        float GetAnimTransitionFade(Animation c_anim, string name_to)
        {
            float fade = 0;
            if (AnimTrans == null) return fade;
            if (!c_anim.isPlaying) return fade;

            //List<AnimTransition> at = AnimTrans.FindAll((x) => { return x.Anim == c_anim; });
            //if (at==null) return fade;

            for (int i = 0; i < AnimTrans.Count; i++)
            {
                AnimTransition a_item = AnimTrans[i];
                if (a_item.Anim == c_anim && AnimTrans[i].Name == name_to)
                {
                    if (a_item.Froms.Length != a_item.CrossFadeTimes.Length) return fade;

                    for (int k = 0; k < a_item.Froms.Length; k++)
                    {
                        if (c_anim.IsPlaying(a_item.Froms[k]))
                        {
                            fade = a_item.CrossFadeTimes[k];
                            return fade;
                        }
                    }
                }
            }

            return fade;
        }

        #endregion privates

        protected override void OnSafeInit()
        {
            base.OnSafeInit();
            _CachedT = transform;
            _Animation = GetComponent<Animation>();
        }

        protected virtual void Update()
        {
            if (_CurState != null)
            {
                _CurTime += Time.deltaTime;

                if ((_CurState.wrapMode != WrapMode.Loop && _CurState.wrapMode != WrapMode.PingPong) && _CurTime > _CurState.length)
                {
                    string st_name = _CurState.name;
                    _CurState = null;
                    if (OnSequenceEnd != null)
                    {
                        OnSequenceEnd.Invoke(this.gameObject, st_name);
                        //Debug.Log("Sequence End " + st_name + " at " + Time.time);
                    }
                }
            }
        }

        [NoToLua]
        public Animation GetAnimation()
        {
            SafeInit();

            return _Animation;
        }

        public float PlaySequence(string seq_name)
        {
            SafeInit();

            float ret = 0;

            if (_Animation)
            {
                try
                {
                    AnimationState ast = _Animation[seq_name];
                    _CurState = ast;
                    _CurTime = 0;
                    ret = _CurState.length;

                    _Animation.Play(seq_name);

#if ART_USE
                    Debug.Log("PlaySequence " + seq_name + " at " + Time.time);
#endif
                }
                catch
                {
                    _CurState = null;
                    _CurTime = 0;
                    ret = -1;
                }

            }
            return ret;
        }

        public void StopSequence()
        {
            if (_Animation)
            {
                if (_CurState != null)
                {
                    _Animation.Stop();
                    _CurState = null;
                    _CurTime = 0;

                    //Debug.Log("StopSequence");
                }
            }
        }

        [NoToLua]
        public void Event_PlayAnim(string evt_name)
        {
            SafeInit();

            AnimEventBlock aeb = FindEventBlock<AnimEventBlock>(evt_name, AnimEvents);
            if (aeb != null)
            {
                _PlayAnim(aeb.Anim, aeb.AnimName, aeb.CrossFade);
            }
        }

        [NoToLua]
        public void Event_PlaySound(string evt_name)
        {
            SafeInit();
            SoundEventBlock seb = FindEventBlock<SoundEventBlock>(evt_name, SoundEvents);
            if (seb != null)
            {
                _PlaySound(seb.Node, seb.SndPath, seb.LifeTime, seb.Is2D);
            }
        }

        [NoToLua]
        public void Event_PlayFx(string evt_name)
        {
            SafeInit();
            FxEventBlock feb = FindEventBlock<FxEventBlock>(evt_name, FxEvents);
            if (feb != null)
            {
                GNewUITools.PlayFx(feb.Hook, feb.Anchor, null, feb.FxPath, feb.LifeTime, feb.IsUI);
            }
        }

        [NoToLua]
        public void Event_RaiseMsg(string evt_name)
        {
            SafeInit();
            if (OnSequenceEvent != null)
            {
#if ART_USE
                Debug.Log("RaiseEvent " + evt_name + " at " + Time.time);
#endif

                OnSequenceEvent.Invoke(gameObject, evt_name);
            }
        }

        #region Editor Surport
#if UNITY_EDITOR
        [NoToLua]
        public void AddEventBlock(AimEventType evt_type)
        {
            switch (evt_type)
            {
                case AimEventType.Anim:
                    AnimEvents.Add(new AnimEventBlock());
                    break;
                case AimEventType.Fx:
                    FxEvents.Add(new FxEventBlock());
                    break;
                case AimEventType.Sound:
                    SoundEvents.Add(new SoundEventBlock());
                    break;
                case AimEventType.AnimTransition:
                    AnimTrans.Add(new AnimTransition());
                    break;
            }
        }

        [NoToLua]
        public bool IsEventBlockExisted(string name, int id, AimEventType evt_type)
        {
            switch (evt_type)
            {
                case AimEventType.Anim:
                    return FindEventBlock<AnimEventBlock>(name, AnimEvents, id + 1) != null;
                case AimEventType.Fx:
                    return FindEventBlock<FxEventBlock>(name, FxEvents, id + 1) != null;
                case AimEventType.Sound:
                    return FindEventBlock<SoundEventBlock>(name, SoundEvents, id + 1) != null;
                case AimEventType.AnimTransition:
                    return FindEventBlock<AnimTransition>(name, AnimTrans, id + 1) != null;
            }
            return false;
        }
#endif
        #endregion
    }
}
