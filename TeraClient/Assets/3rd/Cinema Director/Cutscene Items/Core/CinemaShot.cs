// Cinema Suite
using CinemaDirector.Helpers;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace CinemaDirector
{
    /// <summary>
    /// The representation of a Shot.
    /// </summary>
    [CutsceneItemAttribute("Shots", "Shot", CutsceneItemGenre.CameraShot)]
    public class CinemaShot : CinemaGlobalAction, IRevertable
    {
        public Camera shotCamera;
        //private bool cachedState;

        // Options for reverting in editor.
        [SerializeField]
        private RevertMode editorRevertMode = RevertMode.Revert;

        // Options for reverting during runtime.
        [SerializeField]
        private RevertMode runtimeRevertMode = RevertMode.Revert;

        public RevertMode EditorRevertMode
        {
            get
            {
                return editorRevertMode;
            }

            set
            {
                editorRevertMode = value;
            }
        }

        public RevertMode RuntimeRevertMode
        {
            get
            {
                return runtimeRevertMode;
            }

            set
            {
                runtimeRevertMode = value;
            }
        }

        public override void Initialize()
        {
            base.Initialize();

            if (shotCamera != null)
            {
                //cachedState = shotCamera.gameObject.activeInHierarchy;
            }
        }

        public override void Trigger()
        {
            if (this.shotCamera != null)
            {
                this.shotCamera.gameObject.SetActive(true);
                GetComponentInParent<CGGlobal>().Current = this.shotCamera;
            }
        }

        public override void End()
        {
            if (this.shotCamera != null)
            {
                this.shotCamera.gameObject.SetActive(false);
            }
        }

        public List<RevertInfo> CacheState()
        {
            List<RevertInfo> reverts = new List<RevertInfo>();

            if (shotCamera != null)
            {
                GameObject go = shotCamera.gameObject;
                reverts.Add(new RevertInfo(this, go, go.activeSelf, RevertType.GameObject, RevertValueType.Active));
            }

            return reverts;
        }
    }
}