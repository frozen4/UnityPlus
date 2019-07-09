using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;

public class GUISound : GNewUIBase, IPointerClickHandler, IPointerUpHandler
{
#if IN_GAME
    private static string[] _UISondClips;
    public static void ReadSoundResPath()
    {
        if (_UISondClips == null)
        {
            _UISondClips = new string[7];
            _UISondClips[(int)PlayType.Btn_Press] = LuaScriptMgr.Instance.GetResPath("GUISound_Btn_Press");
            _UISondClips[(int)PlayType.Tag_Press] = LuaScriptMgr.Instance.GetResPath("GUISound_Tab_Press");
            _UISondClips[(int)PlayType.Choose_Press] = LuaScriptMgr.Instance.GetResPath("GUISound_Choose_Press");
            _UISondClips[(int)PlayType.RollDown] = LuaScriptMgr.Instance.GetResPath("GUISound_RollDown");
            _UISondClips[(int)PlayType.RollUp] = LuaScriptMgr.Instance.GetResPath("GUISound_RollUp");
            _UISondClips[(int)PlayType.Btn_Back] = LuaScriptMgr.Instance.GetResPath("GUISound_Btn_Back");
            _UISondClips[(int)PlayType.Btn_Menu] = LuaScriptMgr.Instance.GetResPath("GUISound_Btn_Menu");
        }
    }
#else
    private const string Btn_Example = "Assets/Outputs/Sound/UI/Btn_Press.wav";
#endif

    public enum PlayType
    {
        Btn_Press = 0,
        Tag_Press = 1,
        Choose_Press = 2,
        RollDown = 3,
        RollUp = 4,
        Btn_Back = 5,
        Btn_Menu = 6,
        //total 7
        Custom = 999,
    };
    public enum EffectType
    {
        None,
        Click,
        PointUp,
        Enable,
        Disable,
        EnableNDisable,
    }

    public PlayType playType;
    public EffectType effectType;
    public string customClipName;
    private Selectable _selectable;

    protected override void OnSafeInit()
    {
        base.OnSafeInit();
        _selectable = GetComponent<Selectable>();
    }

    public void PlaySound()
    {
        if (null == _selectable || _selectable.IsInteractable())
        {
            if (playType == PlayType.Custom)
            {
                if (!string.IsNullOrEmpty(customClipName))
                {
                    PlaySoundClip(customClipName);
                }
            }
            else
            {
#if IN_GAME
                //if (_UISondClips == null)
                //{
                //    Debug.LogWarning("GUISound not Init res Path");
                //    return;
                //}

                //int i_pt = (int)playType;
                //if (i_pt > -1 && i_pt < _UISondClips.Length)
                //{
                //    PlaySoundClip(_UISondClips[i_pt]);
                //}
                //else
                //{
                //    Debug.LogWarning("GUISound PlaySound with unknown type " + i_pt);
                //}

                //if (playType == PlayType.Btn_Back || playType == PlayType.Btn_Menu)
                //{
                //    GUISound.PlaySoundClip(GUISound.PlayType.Btn_Press);
                //}
                //else
                //{
                    GUISound.PlaySoundClip(playType);
                //}

#else
                PlaySoundClip(Btn_Example);
#endif
            }
        }
    }

    public static void PlaySoundClip(string preMake)
    {
#if IN_GAME
        ISoundMan soundMan = EntryPoint.Instance.SoundMan;
        if (soundMan != null)
            soundMan.Play2DAudio(preMake);
#else
        /*
        if (Application.isPlaying)
        {
            GameObject g = GameObject.Find("AudioSources");
            if (g == null) g = new GameObject("AudioSources");

            AudioSource audioSource = g.GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = g.AddComponent<AudioSource>();
            }

            AudioClip clip = AssetDatabase.LoadAssetAtPath(preMake, typeof(AudioClip)) as AudioClip;
            audioSource.clip = clip;
            audioSource.volume = 1f;
            audioSource.spatialBlend = 0f;
            audioSource.Play();
        }
        */
#endif
    }

    public static void PlaySoundClip(PlayType p_type)
    {
        if (p_type != PlayType.Custom)
        {
#if IN_GAME
            if (_UISondClips != null && (int)p_type < _UISondClips.Length)
            {
                PlaySoundClip(_UISondClips[(int)p_type]);
            }
#else
            PlaySoundClip(Btn_Example);
#endif
        }
    }

    [NoToLua]
    public void OnPointerClick(PointerEventData eventData)
    {
        if (effectType == EffectType.Click)
        {
            PlaySound();
        }
    }

    [NoToLua]
    public void OnPointerUp(PointerEventData eventData)
    {
        if (effectType == EffectType.PointUp)
        {
            PlaySound();
        }
    }

    protected override void OnEnable()
    {
        if (effectType == EffectType.Enable || effectType == EffectType.EnableNDisable)
        {
            PlaySound();
        }
        base.OnEnable();
    }

    protected override void OnDisable()
    {
        if (effectType == EffectType.Disable || effectType == EffectType.EnableNDisable)
        {
            PlaySound();
        }
        base.OnDisable();
    }
}
