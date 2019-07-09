// Cinema Suite
using CinemaDirector.Helpers;
using UnityEngine;
using System.Collections.Generic;

namespace CinemaDirector
{
    /// <summary>
    /// A simple event for Enabling a Game Object
    /// </summary>
    [CutsceneItemAttribute("Game Object", "Enable Game Object", CutsceneItemGenre.GlobalItem)]
    public class EnableGameObjectGlobal : CinemaGlobalEvent, IRevertable
    {
        // The target GameObject
        public GameObject target;

        // Options for reverting in editor.
        [SerializeField]
        private RevertMode editorRevertMode = RevertMode.Revert;

        // Options for reverting during runtime.
        [SerializeField]
        private RevertMode runtimeRevertMode = RevertMode.Revert;

        // Keep track of the GameObject's previous state when calling Trigger.
        private bool previousState;

        /// <summary>
        /// Cache the initial state of the target GameObject's active state.
        /// </summary>
        /// <returns>The Info necessary to revert this event.</returns>
        public List<RevertInfo> CacheState()
        {
            List<RevertInfo> reverts = new List<RevertInfo>();
            if (target != null)
            {
                reverts.Add(new RevertInfo(this, target, target.activeInHierarchy, RevertType.GameObject, RevertValueType.Active));
            }

            return reverts;
        }

        /// <summary>
        /// Trigger this event and set the given GameObject to enabled.
        /// </summary>
        public override void Trigger()
        {
            if (target != null)
            {
                previousState = target.activeInHierarchy;
                target.SetActive(true);
            }
        }

        /// <summary>
        /// Reverse this Event and put the GameObject into its' previous state.
        /// </summary>
        public override void Reverse()
        {
            if (target != null)
            {
                target.SetActive(previousState);
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