using UnityEngine;
using CinemaDirector.Helpers;
using System.Collections.Generic;
using UnityEngine.UI;

namespace CinemaDirector
{
    [CutsceneItem("uGUI", "SetStop", CutsceneItemGenre.ActorItem)]
    public class SetStopEvent : CinemaActorAction, IRevertable
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

        [Header("结束按钮")]
        public GameObject StopButton;

        //3秒内2次点击才会跳过
        private GameObject _Btn_Jump;
        private int _ClickCount = 0;
        private int _MaxCount = 2;
        private float _DisTime = 3F;

        void Awake()
        {
            var group = GetComponentInParent<ActorTrackGroup>();
            if (group == null)
                return;
            if (group.Actor == null)
                return;
            _Btn_Jump = group.Actor.gameObject;
        }

        public List<RevertInfo> CacheState()
        {
            List<RevertInfo> reverts = new List<RevertInfo>();

            return reverts;
        }

        public override void Trigger(GameObject Actor)
        {
            if (Actor != null)
            {
                HandleClickJumpListener(Actor, true);
            }
            if (StopButton != null)
            {
                HandleClickStopListener(StopButton, true);
            }
        }

        private GButton.OnButtonClick clickJumpEvent = null;
        private void HandleClickJumpListener(GameObject go, bool isAdd)
        {
            if (go == null) return;
            Button button = go.GetComponent<Button>();
            if (button != null)
            {
                if (isAdd)
                    button.onClick.AddListener(ClickJump);
                else
                    button.onClick.RemoveListener(ClickJump);
            }
            else
            {
                GButton gButton = go.GetComponent<GButton>();
                if (gButton != null)
                {
                    if (clickJumpEvent == null)
                        clickJumpEvent = (btn) => { ClickJump(); };

                    if (isAdd)
                        gButton.OnClick += clickJumpEvent;
                    else
                        gButton.OnClick -= clickJumpEvent;
                }
            }
        }

        private GButton.OnButtonClick clickStopEvent = null;
        private void HandleClickStopListener(GameObject go, bool isAdd)
        {
            if (go == null) return;
            Button button = go.GetComponent<Button>();
            if (button)
            {
                if (isAdd)
                    button.onClick.AddListener(ClickStop);
                else
                    button.onClick.RemoveListener(ClickStop);
            }
            else
            {

                GButton gButton = go.GetComponent<GButton>();
                if (gButton != null)
                {
                    if (clickStopEvent == null)
                        clickStopEvent = (btn) => { ClickStop(); };

                    if (isAdd)
                        gButton.OnClick += clickStopEvent;
                    else
                        gButton.OnClick -= clickStopEvent;
                }
            }
        }

        private void ClickJump()
        {
            _ClickCount++;
            if (StopButton != null)
            {
                StopButton.SetActive(true);
            }
            CancelInvoke("ResetState");
            Invoke("ResetState", _DisTime);
        }

        private void ClickStop()
        {
            if (_Btn_Jump != null)
            {
                HandleClickJumpListener(_Btn_Jump, false);
            }
            if (StopButton != null)
            {
                HandleClickStopListener(StopButton, false);
            }
            CancelInvoke("ResetState");
            _ClickCount = 0;
            GetCutScene().Stop();
        }

        private void ResetState()
        {
            _ClickCount = 0;
            if (StopButton != null && StopButton.activeSelf != false)
                StopButton.SetActive(false);
        }

        public override void End(GameObject Actor)
        {

        }

        void LateUpdate()
        {
            if (CGManager.Instance.IsForbidInput() && CGManager.Instance.CanSkip)
            {
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    if (_ClickCount == _MaxCount)
                        ClickStop();
                    else
                        ClickJump();
                }
            }
        }
    }
}
