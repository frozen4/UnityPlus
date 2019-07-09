/*
 * CG跳跃播放功能
 * 
 * 孟令康
 * 
 * 2016年12月27日
 * 
*/

using UnityEngine;
using CinemaDirector;
using CinemaDirector.Helpers;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Events;

namespace CinemaDirector
{
    [CutsceneItem("uGUI", "SetSkip", CutsceneItemGenre.ActorItem)]
    public class SetSkipEvent : CinemaActorAction, IRevertable
    {
        [Header("设置运行时间")]
        public float setRunTime = 0F;

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

            return reverts;
        }
        private UnityAction unityAction = null;
        public override void Trigger(GameObject Actor)
        {
            if (Actor != null)
            {
                if (Actor.GetComponent<RectTransform>())
                {
                    unityAction = delegate () { SkipEvent(Actor); };
                    Actor.GetComponent<Button>().onClick.AddListener(unityAction);
                }
                else
                {
                    Debug.Log("Error Actor:None RectTransform");
                }
            }
        }
        private void SkipEvent(GameObject Actor)
        {
            GetCutScene().SetRunningTime(setRunTime);
            Actor.GetComponent<Button>().onClick.RemoveListener(unityAction);
        }

        public override void End(GameObject Actor)
        {
            Actor.GetComponent<Button>().onClick.RemoveListener(unityAction);
        }
    }
}