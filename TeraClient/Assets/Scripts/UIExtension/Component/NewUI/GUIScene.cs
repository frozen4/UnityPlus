using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.Events;
using System;

//1 only at the same time
//will call OnAnimEvent func in lua


namespace GNewUI
{
    public class GUIScene : GNewUIBase
    {
        //[NoToLua]
        //public static GUIScene Instance { get { return _Instance; } }
        //private static GUIScene _Instance;

        private static List<GUIScene> AllScenes;

        #region serialization

        [NoToLua]
        public Camera MainCamera;
        [NoToLua]
        public Vector2 ViewportSize = new Vector2(-1, -1);
        [NoToLua]
        public GameObject Lights;
        [NoToLua]
        public GUIAnim[] UiAinm;

        public int EnvEffectID = -1;

        ////[NoToLua]
        ////private RawImage _Img;

        ////env color
        //public Color EnvSkyColor;
        //public Color EnvEquColor;
        //public Color EnvGrdColor;

        //public bool IsOverrideAmb = true;

        //public Cubemap SkyCubeMap;
        //[Range(0, 5)]
        //public float ReflectionIntensity = 1;

        //public bool IsOverrideReflection = false;

        [NoToLua]
        public UnityEvent OnRestart = new UnityEvent();

        #endregion

        private bool _IsVisible = true;
        RenderTexture RT2Release;
        //private Sun[] _allSuns;

        protected override void OnEnable()
        {
            base.OnEnable();

            if (AllScenes == null)
            {
                AllScenes = new List<GUIScene>();
                AllScenes.Add(this);
            }
            else
            {
                bool b_found = false;
                for (int i = 0; i < AllScenes.Count; i++)
                {
                    if (AllScenes[i] != null)
                    {
                        if (AllScenes[i] != this)
                        {
                            AllScenes[i].gameObject.SetActive(false);
                        }
                        else
                        {
                            b_found = true;
                        }
                    }
                    else
                    {
                        AllScenes.RemoveAt(i);
                        i -= 1;
                    }
                }

                if (!b_found)
                {
                    AllScenes.Add(this);
                }
            }

            OnVisible(_IsVisible);

        }

        protected override void OnDisable()
        {
            base.OnDisable();
            OnVisible(false);
        }

        protected override void OnDestroy()
        {
            if (MainCamera != null && MainCamera.targetTexture == RT2Release)
            {
                MainCamera.targetTexture = null;
            }

            if (RT2Release != null)
            {
                //RT2Release.Release();
                RenderTexture.ReleaseTemporary(RT2Release);
                RT2Release = null;
            }
        }

        protected override void OnSafeInit()
        {
            base.OnSafeInit();

            if (Lights == null)
            {
                Transform t_lights = Trans.Find("Lights");
                if (t_lights != null)
                {
                    Lights = t_lights.gameObject;
                }
            }
        }

        public void Init()
        {
            SafeInit();

            if (OnRestart != null)
            {
                OnRestart.Invoke();
            }
        }

        //public
        [NoToLua]
        public RenderTexture GetRenderTexture(int w, int h)
        {
            if (MainCamera != null)
            {
                if (MainCamera.targetTexture == null)
                {
                    if (RT2Release == null)
                    {
                        if (w <= 0) w = Screen.width;

                        if (h <= 0) h = Screen.height;

                        //if (Screen.width < Screen.height)
                        //{
                        //    w = Mathf.Min(1024, Screen.width);
                        //    h = w / Screen.width * Screen.height;
                        //}
                        //else
                        //{
                        //    h = Mathf.Min(1024, Screen.height);
                        //    w = h / Screen.height * Screen.width;
                        //}

                        RT2Release = RenderTexture.GetTemporary(w, h, 16, RenderTextureFormat.Default);
                        RT2Release.name = "UISceneRT:" + name;
                    }
                    MainCamera.targetTexture = RT2Release;
                }

                return MainCamera.targetTexture;
            }
            return null;
        }

        [NoToLua]
        public float PlaySequence(int id, string seq_name, UnityEngine.Events.UnityAction<GameObject, string> on_finish)
        {
            SafeInit();
            float ret = -1;

            if (id > -1 && id < UiAinm.Length)
            {
                GUIAnim g_anim = UiAinm[id];
                if (g_anim)
                {
                    g_anim.StopSequence();

                    g_anim.OnSequenceEnd = on_finish;
                    ret = g_anim.PlaySequence(seq_name);
                }
            }
            return ret;
        }

        [NoToLua]
        public void RegisterEventHandler(int id, UnityEngine.Events.UnityAction<GameObject, string> on_event)
        {
            for (int i = 0; i < UiAinm.Length; i++)
            {
                GUIAnim g_anim = UiAinm[id];
                if (g_anim != null)
                {
                    g_anim.OnSequenceEvent += on_event;
                }
            }
        }

        [NoToLua]
        public void UnregisterEventHandler(int id, UnityEngine.Events.UnityAction<GameObject, string> on_event)
        {
            for (int i = 0; i < UiAinm.Length; i++)
            {
                GUIAnim g_anim = UiAinm[id];
                if (g_anim != null)
                {
                    g_anim.OnSequenceEvent -= on_event;
                }
            }
        }

#if IN_GAME

        LuaInterface.LuaTable LuaObj;
        LuaInterface.LuaFunction OnFinish;
        LuaInterface.LuaFunction OnEvent;

        public void SetLuaHandler(LuaInterface.LuaTable lua_obj, LuaInterface.LuaFunction on_finish, LuaInterface.LuaFunction on_event)
        {
            LuaObj = lua_obj;
            OnFinish = on_finish;
            OnEvent = on_event;

            for (int i = 0; i < UiAinm.Length; i++)
            {
                UnregisterEventHandler(i, InternalOnEvent);
                RegisterEventHandler(i, InternalOnEvent);
            }
        }

        public float PlaySequence(int id, string seq_name)
        {
            return PlaySequence(id, seq_name, InternalOnFinished);
        }

        private void InternalOnFinished(GameObject go, string seq_name)
        {
            if (OnFinish != null)
            {
                OnFinish.Call(LuaObj, go.name, seq_name);
            }
        }

        private void InternalOnEvent(GameObject go, string evt_name)
        {
            if (OnEvent != null)
            {
                OnEvent.Call(LuaObj, evt_name);
            }
        }

#endif

        [NoToLua]
        public int GetChannelCount()
        {
            return UiAinm == null ? 0 : UiAinm.Length;
        }

        [NoToLua]
        public GUIAnim GetAnim(int id)
        {
            return UiAinm[id];
        }

        public void SetVisible(bool is_show)
        {
            _IsVisible = is_show;
            OnVisible(enabled && _IsVisible);
        }

        public void SetCameraDepth(int i_depth)
        {
            if (MainCamera != null)
            {
                MainCamera.depth = i_depth;
            }
        }

        void OnVisible(bool is_show)
        {
            //Debug.LogWarning("OnVisible " + enabled + ", " + _IsVisible);

#if IN_GAME
            if (MainCamera != null)
            {
                MainCamera.enabled = is_show;
            }

            //if (_allSuns != null)
            //{
            //    for (int i = 0; i < _allSuns.Length; i++)
            //    {
            //        _allSuns[i].enabled = is_show;
            //    }
            //}

            if (Lights != null)
            {
                Lights.SetActive(is_show);
            }

            //if (is_show && IsOverrideAmb)
            //    DynamicEffectManager.Instance.OpenSmithyUI(EnvEquColor, EnvGrdColor, EnvSkyColor);
            //else
            //    DynamicEffectManager.Instance.CloseSmithyUI();

            //if (is_show && IsReflection)
            //{
            //    DynamicEffectManager.Instance.DisableSkyAndSun();
            //}
            //else
            //{
            //    DynamicEffectManager.Instance.EnableSkyAndSun();
            //}

#endif
        }

        public void PossessImage(GameObject g, float alpha)
        {
            RawImage image = g.GetComponent<RawImage>();

            if (image != null && MainCamera != null)
            {
                image.texture = GetRenderTexture((int)ViewportSize.x, (int)ViewportSize.y);
                Color color = image.color;
                image.color = new Color(color.r, color.g, color.b, alpha);
            }
        }

    }
}
