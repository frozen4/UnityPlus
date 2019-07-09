using CinemaDirector.Helpers;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine.UI;
using UnityEngine;

namespace CinemaDirector
{
    [CutsceneItemAttribute("uGUI", "Text Bubble", CutsceneItemGenre.ActorItem)]
    public class TextBubbleEvent : CinemaActorAction, IRevertable
    {
        public TextBubble textBubble;

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
        /// <summary>
        /// Option for choosing when this Event will Revert to initial state in Editor.
        /// </summary>
        public RevertMode EditorRevertMode
        {
            get { return editorRevertMode; }
            set { editorRevertMode = value; }
        }
        // Options for reverting during runtime.
        [SerializeField]
        private RevertMode runtimeRevertMode = RevertMode.Revert;
        /// <summary>
        /// Option for choosing when this Event will Revert to initial state in Runtime.
        /// </summary>
        public RevertMode RuntimeRevertMode
        {
            get { return runtimeRevertMode; }
            set { runtimeRevertMode = value; }
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
            if(textBubble)
            {
                textBubble.OnInit(realTextValue.Length);
                Text txt = textBubble.text;
                initialTextValue = txt.text;
                txt.text = "";
#if UNITY_EDITOR
                EditorUtility.SetDirty(txt);
#endif
            }
        }

        public override void ReverseTrigger(GameObject actor)
        {
            if(textBubble)
            {
                Text txt = textBubble.text;
                txt.text = initialTextValue;
#if UNITY_EDITOR
                EditorUtility.SetDirty(txt);
#endif
            }
        }

        public override void SetTime(GameObject actor, float time, float deltaTime)
        {
            if (actor != null)
                if (time > 0 && time <= Duration)
                    UpdateTime(actor, time, deltaTime);
        }

        public override void UpdateTime(GameObject actor, float runningTime, float deltaTime)
        {
            float transition = runningTime / Duration;
            int numericalValue;

            if (realTextValue != null)
            {
                numericalValue = (int)Mathf.Round(Mathf.Lerp(0, realTextValue.Length, transition));
                Text txt = textBubble.text;
                txt.text = realTextValue.Substring(0, numericalValue);
#if UNITY_EDITOR
                EditorUtility.SetDirty(txt);
#endif
            }
        }

        public override void End(GameObject actor)
        {
            if(textBubble)
            {
                if (Util.IsZero(Duration))
                    textBubble.OnInit(realTextValue.Length);
                Text txt = textBubble.text;
                txt.text = realTextValue;
#if UNITY_EDITOR
                EditorUtility.SetDirty(txt);
#endif
            }
        }
    }
}
