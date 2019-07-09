using CinemaDirector.Helpers;
using System.Collections.Generic;
using UnityEngine;

namespace CinemaDirector
{
    /// <summary>
    /// Sets the transform of the Actor to that of another Game Object's Transform.
    /// </summary>
    [CutsceneItemAttribute("Transform", "Set Transform", CutsceneItemGenre.ActorItem, CutsceneItemGenre.TransformItem)]
    public class SetTransformEvent : CinemaActorEvent, IRevertable
    {
        public Transform Transform;

        // Options for reverting in editor.
        [SerializeField]
        private RevertMode editorRevertMode = RevertMode.Revert;

        // Options for reverting during runtime.
        [SerializeField]
        private RevertMode runtimeRevertMode = RevertMode.Revert;

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
            if (actor != null)
            {
                actor.transform.position = Transform.position;
                actor.transform.rotation = Transform.rotation;
                actor.transform.localScale = Transform.localScale;
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