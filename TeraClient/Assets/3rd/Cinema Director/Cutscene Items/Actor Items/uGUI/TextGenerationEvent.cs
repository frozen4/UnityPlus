using CinemaDirector.Helpers;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine.UI;
using UnityEngine;

namespace CinemaDirector
{
    [CutsceneItemAttribute("uGUI", "Text Generator", CutsceneItemGenre.ActorItem)]
    public class TextGenerationEvent : CinemaActorAction, IRevertable
    {
        public enum DialogueType
        {
            Name,
            Content,
            All
        }

        [TextArea(3, 10)]
        public string textValue;                //编辑内容
        [SerializeField]
        [HideInInspector]
        public string realTextValue;            //实际显示内容
        [SerializeField]
        [HideInInspector]
        public DialogueType dialogueType = DialogueType.Content;
        private string initialTextValue;

        // Options for reverting in editor.
        [SerializeField]
        private RevertMode editorRevertMode = RevertMode.Revert;

        // Options for reverting during runtime.
        [SerializeField]
        private RevertMode runtimeRevertMode = RevertMode.Revert;

        private Text _TextComp = null;
        /// <summary>
        /// Cache the state of all actors related to this event.
        /// </summary>
        /// <returns></returns>
        public List<RevertInfo> CacheState()
        {
            List<RevertInfo> reverts = new List<RevertInfo>();

            return reverts;
        }

        public override void Trigger(GameObject actor)
        {
            InitRealTextValue();
            actor.SetActive(true);
            _TextComp = actor.GetComponentInChildren<Text>();
            initialTextValue = _TextComp.text;
            _TextComp.text = "";
#if UNITY_EDITOR
            EditorUtility.SetDirty(_TextComp);
#endif
        }

        private void InitRealTextValue()
        {
            var dialogue = CGManager.Instance.GetDialogueById(textValue);
            if (dialogue == null) return;
            switch (dialogueType)
            {
                case DialogueType.Name:
                    realTextValue = dialogue.Name;
                    break;
                case DialogueType.Content:
                    realTextValue = dialogue.Content;
                    break;
                case DialogueType.All:
                    realTextValue = HobaText.Format("{0}{1}", dialogue.Name, dialogue.Content);
                    break;
                default:
                    realTextValue = string.Empty;
                    Debug.LogError("Error dialogueType!!!");
                    break;
            }
        }

        public override void ReverseTrigger(GameObject actor)
        {
            if (_TextComp == null)
            {
                Trigger(actor);
            }
            _TextComp.text = initialTextValue;
#if UNITY_EDITOR
            EditorUtility.SetDirty(_TextComp);
#endif
        }

        public override void SetTime(GameObject actor, float time, float deltaTime)
        {
            if (actor != null)
                if (time > 0 && time <= Duration)
                    UpdateTime(actor, time, deltaTime);
        }

        public override void UpdateTime(GameObject actor, float runningTime, float deltaTime)
        {
            if (_TextComp == null || realTextValue == null)
            {
                Trigger(actor);
            }
            if (realTextValue != null)
            {
                float transition = runningTime / Duration;
                var numericalValue = (int)Mathf.Round(Mathf.Lerp(0, realTextValue.Length, transition));
                _TextComp.text = realTextValue.Substring(0, numericalValue);
#if UNITY_EDITOR
                EditorUtility.SetDirty(_TextComp);
#endif
            }
        }

        public override void End(GameObject actor)
        {
            if(_TextComp == null || realTextValue == null)
            {
                Trigger(actor);
            }
            _TextComp.text = realTextValue;
#if UNITY_EDITOR
            EditorUtility.SetDirty(_TextComp);
#endif
        }

        /// <summary>
        /// Option for choosing when this Event will Revert to initial state in Editor.
        /// </summary>
        public RevertMode EditorRevertMode
        {
            get { return editorRevertMode; }
            set { editorRevertMode = value; }
        }

        /// <summary>
        /// Option for choosing when this Event will Revert to initial state in Runtime.
        /// </summary>
        public RevertMode RuntimeRevertMode
        {
            get { return runtimeRevertMode; }
            set { runtimeRevertMode = value; }
        }
    }
}