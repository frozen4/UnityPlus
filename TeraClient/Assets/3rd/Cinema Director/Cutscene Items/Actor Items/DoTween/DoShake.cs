using UnityEngine;
using DG.Tweening;
using CinemaDirector;
using CinemaDirector.Helpers;
using System.Collections.Generic;

[CutsceneItem("DoTween", "DoShake", CutsceneItemGenre.ActorItem)]
public class DoShake : CinemaActorAction, IRevertable
{
    [Header("震动频率")]
    public int vibrato = 10;
    [Header("震动随机值(0-180)")]
    public float randomness = 90;
    [Header("true:自动平滑过渡")]
    public bool fadeOut = true;
    [Header("位置震动")]
    public Vector3 shakePosition = Vector3.zero;
    [Header("角度震动")]
    public Vector3 shakeRotation = Vector3.zero;
    [Header("大小震动")]
    public Vector3 shakeScale = Vector3.zero;

    // Options for reverting in editor.
    [SerializeField]
    private RevertMode editorRevertMode = RevertMode.Revert;

    // Options for reverting during runtime.
    [SerializeField]
    private RevertMode runtimeRevertMode = RevertMode.Revert;

    public RevertMode EditorRevertMode
    {
        get { return editorRevertMode; }
        set { editorRevertMode = value; }
    }

    public RevertMode RuntimeRevertMode
    {
        get { return runtimeRevertMode; }
        set { runtimeRevertMode = value; }
    }

    public List<RevertInfo> CacheState()
    {
        List<RevertInfo> reverts = new List<RevertInfo>();

        List<Transform> actors = new List<Transform>();
        GetActors(ref actors);

        for (int i = 0; i < actors.Count; i++)
        {
            Transform go = actors[i];
            if (go != null)
            {
                Transform t = go.GetComponent<Transform>();
                if (t != null)
                {
                    reverts.Add(new RevertInfo(this, t, t.localPosition, RevertType.Transform, RevertValueType.LocalPosition));
                }
            }
        }

        return reverts;
    }

    public override void Trigger(GameObject Actor)
    {
        if (Actor != null)
        {
            //Position与Rotation,涉及到Camera需要特殊处理
            if (shakePosition != Vector3.zero)
            {
                if (Actor.GetComponent<Camera>())
                {
                    Actor.GetComponent<Camera>().DOShakePosition(Duration, shakePosition, vibrato, randomness, fadeOut);
                }
                else
                {
                    Actor.transform.DOShakePosition(Duration, shakePosition, vibrato, randomness, fadeOut);
                }
            }
            if (shakeRotation != Vector3.zero)
            {
                if (Actor.GetComponent<Camera>())
                {
                    Actor.GetComponent<Camera>().DOShakeRotation(Duration, shakeRotation, vibrato, randomness, fadeOut);
                }
                else
                {
                    Actor.transform.DOShakeRotation(Duration, shakeRotation);
                }
            }
            if (shakeScale != Vector3.zero)
                Actor.transform.DOShakeScale(Duration, shakeScale, vibrato, randomness, fadeOut);
        }
    }

    public override void End(GameObject Actor)
    {
        
    }

}
