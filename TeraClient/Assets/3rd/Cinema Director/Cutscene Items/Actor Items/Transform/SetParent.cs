using CinemaDirector.Helpers;
using System.Collections.Generic;
using UnityEngine;

namespace CinemaDirector
{
    /// <summary>
    /// Attaches actor as child of target in hierarchy
    /// </summary>
    [CutsceneItemAttribute("Transform", "Set Parent", CutsceneItemGenre.ActorItem)]
    public class SetParent : CinemaActorEvent, IRevertable
    {
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
            List<Transform> actors = new List<Transform>(GetActors());
            List<RevertInfo> reverts = new List<RevertInfo>();
            for (int i = 0; i < actors.Count; i++)
            {
                Transform go = actors[i];
                if (go != null)
                {
                    Transform t = go.GetComponent<Transform>();
                    if (t != null)
                    {
                        reverts.Add(new RevertInfo(this, t, t.parent, RevertType.Transform, RevertValueType.Parent));
                    }
                }
            }

            return reverts;
        }

        public GameObject parent;
        public string child = string.Empty;
        public override void Trigger(GameObject actor)
        {
            if (actor != null && parent != null)
            {
                actor.transform.parent = parent.transform;
            }
        }

        public override void Reverse(GameObject actor)
        {
        }
    }
}