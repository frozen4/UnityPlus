using CinemaDirector.Helpers;
using System.Collections.Generic;
using UnityEngine;

namespace CinemaDirector
{
    /// <summary>
    /// An Event for enabling any behaviour that has an "enabled" property.
    /// </summary>
    [CutsceneItemAttribute("Game Object", "Enable Behaviour", CutsceneItemGenre.ActorItem)]
    public class EnableBehaviour : CinemaActorEvent, IRevertable
    {
        // The Component/Behaviour to Enable.
        //public Component Behaviour;

        // Options for reverting in editor.
        [SerializeField]
        private RevertMode editorRevertMode = RevertMode.Revert;

        // Options for reverting during runtime.
        [SerializeField]
        private RevertMode runtimeRevertMode = RevertMode.Revert;

        /// <summary>
        /// Cache the state of all actors related to this event.
        /// </summary>
        /// <returns>All the revert info related to this event.</returns>
        public List<RevertInfo> CacheState()
        {
            List<RevertInfo> reverts = new List<RevertInfo>();

            return reverts;
        }

        /// <summary>
        /// Trigger this event and Enable the chosen Behaviour.
        /// </summary>
        /// <param name="actor">The actor to perform the behaviour enable on.</param>
        public override void Trigger(GameObject actor)
        {
            //Component b = actor.GetComponent(Behaviour.GetType()) as Component;
            //if (b != null)
            //{
            //    PropertyInfo fieldInfo = ReflectionHelper.GetProperty(Behaviour.GetType(), "enabled");
            //    fieldInfo.SetValue(Behaviour, true, null);
            //}
            var b = actor.GetComponent<Behaviour>();
            if (null != b)
            {
                b.enabled = true;
            }
        }

        /// <summary>
        /// Reverse trigger this event and Disable the chosen Behaviour.
        /// </summary>
        /// <param name="actor">The actor to perform the behaviour disable on.</param>
        public override void Reverse(GameObject actor)
        {
            //Component b = actor.GetComponent(Behaviour.GetType()) as Component;
            //if (b != null)
            //{
            //    PropertyInfo fieldInfo = ReflectionHelper.GetProperty(Behaviour.GetType(), "enabled");
            //    fieldInfo.SetValue(b, false, null);
            //}
            var b = actor.GetComponent<Behaviour>();
            if (null != b)
            {
                b.enabled = false;
            }
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