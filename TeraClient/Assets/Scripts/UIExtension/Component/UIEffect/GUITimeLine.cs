using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using DG.Tweening;
using LuaInterface;
using EZCameraShake;

namespace GNewUI.Timeline
{
    public enum UEEventType
    {
        PlayDotween,
        UIFx,
        SetActive,
        ShakeScreen,
        PlayAnim,
        Lua,
        UISound,
    }

    #region evt

    public class UEvtAnim : TLEvent
    {
        public Animation Anim;
        public string Path;

        public UEvtAnim() { EvtType = UEEventType.PlayAnim; }
    }

    public class UEvtDot : TLEvent
    {
        public DOTweenPlayer DotPlayer;
        public string Id;     //fxPath, dotID

        public UEvtDot() { EvtType = UEEventType.PlayDotween; }
    }

    public class UEvtActive : TLEvent
    {
        public GameObject Go;
        public bool ActiveState;

        public UEvtActive() { EvtType = UEEventType.SetActive; }
    }

    public class UEvtUIFx : TLEvent
    {
        public string Path;
        public Transform Holder;
        public Transform Target;
        public GameObject Clipper;
        public float LifeTime;     //fxPath, dotID
        public int OrderOffset;

        public UEvtUIFx() { EvtType = UEEventType.UIFx; }
    }

    public class UEvtShake : TLEvent
    {
        public float Mag;
        public float LifeTime;

        public UEvtShake() { EvtType = UEEventType.ShakeScreen; }
    }

    public class UEvtLua : TLEvent
    {
        public LuaFunction LuaFunc;
        public UEvtLua() { EvtType = UEEventType.Lua; }
    }

    public class UEvtUISound : TLEvent
    {
        public string Path;
        public UEvtUISound() { EvtType = UEEventType.UISound; }
    }

    #endregion

    public class TLEvent
    {
        public float StartTime;
        public int KeyID;
        public UEEventType EvtType;
        public string KeyName;
    }

    public class GUISchedule : System.Object
    {
        public static int uniID = 0;
        public List<TLEvent> _AllEvents;
        public const int COUNT_LIMIT = 999;
        private string _Tag;

        public GUISchedule()
        {
            _AllEvents = new List<TLEvent>();
        }

        public void SetTag(string s_tag)
        {
            _Tag = s_tag;
        }

        public void ExcEvent(TLEvent evt)
        {
            if (evt.EvtType == UEEventType.PlayAnim)
            {
                UEvtAnim block = evt as UEvtAnim;
                if (block != null && block.Anim != null)
                {
                    block.Anim.Play(block.Path);
                }
            }
            else if (evt.EvtType == UEEventType.PlayDotween)
            {
                UEvtDot block = evt as UEvtDot;
                if (block != null && block.DotPlayer != null)
                {
                    block.DotPlayer.Restart(block.Id);
                }
            }
            else if (evt.EvtType == UEEventType.UIFx)
            {
                UEvtUIFx block = evt as UEvtUIFx;
                if (block != null)
                {
                    GNewUITools.PlayFx(block.Holder, block.Target, block.Clipper, block.Path, block.LifeTime, true, block.OrderOffset);
                }
            }
            else if (evt.EvtType == UEEventType.SetActive)
            {
                UEvtActive block = evt as UEvtActive;
                if (block != null && block.Go != null)
                {
                    block.Go.SetActive(block.ActiveState);
                }
            }
            else if (evt.EvtType == UEEventType.ShakeScreen)
            {
                UEvtShake block = evt as UEvtShake;
                if (block != null)
                {
                    CameraShaker cs = Main.PanelRoot.GetComponent<CameraShaker>();
                    if (cs != null)
                    {
                        cs.ShakeOnce(block.Mag, 10, 0.15f, 0.15f, block.LifeTime, "ui");
                    }
                }
            }
            else if (evt.EvtType == UEEventType.Lua)
            {
                UEvtLua block = evt as UEvtLua;
                if (block != null && block.LuaFunc != null)
                {
                    block.LuaFunc.Call();
                }
            }
            else if (evt.EvtType == UEEventType.UISound)
            {
                UEvtUISound block = evt as UEvtUISound;
                if (block != null)
                {
#if IN_GAME
                    ISoundMan soundMan = EntryPoint.Instance.SoundMan;
                    if (soundMan != null)
                    {
                        soundMan.Play2DAudio(block.Path);
                    }
#endif
                }
            }
        }

        public void Clear()
        {
            _AllEvents.Clear();
        }

        public int AddEvent(TLEvent evt)
        {
            if (_AllEvents.Count > COUNT_LIMIT)
            {
                Common.HobaDebuger.LogWarning("Schedule on " + _Tag + " exceeds limit: " + COUNT_LIMIT);
            }

            if (uniID == int.MaxValue)
            {
                uniID = int.MinValue;
            }

            uniID += 1;
            evt.KeyID = uniID;
            _AllEvents.Add(evt);

            return uniID;
        }

        public void Tick(float delta_time)
        {
            for (int i = 0; i < _AllEvents.Count; i++)
            {
                _AllEvents[i].StartTime -= delta_time;
                if (_AllEvents[i].StartTime <= 0)
                {
                    ExcEvent(_AllEvents[i]);

                    if (i >= _AllEvents.Count)
                    {
                        break;
                    }
                    _AllEvents.RemoveAt(i);
                    i -= 1;
                }
            }
        }

        public int AddPlayAnim(string s_key, float f_time, Animation a_anim, string s_path)
        {
            if (a_anim != null)
            {
                TLEvent tl = new UEvtAnim() { StartTime = f_time, KeyName = s_key, Anim = a_anim, Path = s_path };
                return AddEvent(tl);
            }
            return -1;
        }

        public int AddPlayDotween(string s_key, float f_time, DOTweenPlayer dot_player, string s_id)
        {
            if (dot_player != null)
            {
                TLEvent tl = new UEvtDot() { StartTime = f_time, KeyName = s_key, DotPlayer = dot_player, Id = s_id };
                return AddEvent(tl);
            }
            return -1;
        }

        public int AddSetActive(string s_key, float f_time, GameObject g_go, bool b_activeState)
        {
            if (g_go != null)
            {
                TLEvent tl = new UEvtActive() { StartTime = f_time, KeyName = s_key, Go = g_go, ActiveState = b_activeState };
                return AddEvent(tl);
            }
            return -1;
        }

        public int AddLuaCB(string s_key, float f_time, LuaFunction lf_func)
        {
            if (lf_func != null)
            {
                TLEvent tl = new UEvtLua() { StartTime = f_time, KeyName = s_key, LuaFunc = lf_func };
                return AddEvent(tl);
            }
            return -1;
        }

        public int AddPlayFx(string s_key, float f_time, string s_path, Transform t_hook, Transform t_target, GameObject g_clipper, float f_lifeTime, int i_order)
        {
            TLEvent tl = new UEvtUIFx() { StartTime = f_time, KeyName = s_key, Path = s_path, Holder = t_hook, Target = t_target, Clipper = g_clipper, LifeTime = f_lifeTime, OrderOffset = i_order };
            return AddEvent(tl);
        }

        public int AddPlaySound(string s_key, float f_time, string s_path)
        {
            TLEvent tl = new UEvtUISound() { StartTime = f_time, KeyName = s_key, Path = s_path };
            return AddEvent(tl);
        }

        public int AddShake(string s_key, float f_time, float f_mag, float f_lifeTime)
        {
            TLEvent tl = new UEvtShake() { StartTime = f_time, KeyName = s_key, Mag = f_mag, LifeTime = f_lifeTime };
            return AddEvent(tl);
        }

        public void CloseSchedule(string key)
        {
            for (int i = 0; i < _AllEvents.Count; i++)
            {
                if (_AllEvents[i].KeyName == key)
                {
                    _AllEvents.RemoveAt(i);
                    i -= 1;
                }
            }
        }

        public void CloseEvent(int i_key)
        {
            for (int i = 0; i < _AllEvents.Count; i++)
            {
                if (_AllEvents[i].KeyID == i_key)
                {
                    _AllEvents.RemoveAt(i);
                    break;
                }
            }

        }

    }
}
