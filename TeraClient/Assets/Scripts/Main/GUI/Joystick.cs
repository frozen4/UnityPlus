using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using LuaInterface;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UnityEngine.UI
{
    public delegate void OnJoystickDragCallback();

    [AddComponentMenu("UI/Joystick", 36), RequireComponent(typeof(RectTransform))]
    public class Joystick : UIBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler, IPointerUpHandler, IPointerDownHandler
    {
        private static Joystick instance;
        public static Joystick Instance { get { return instance; } }

        Vector2 m_OriginalPos = Vector2.zero;
        //float m_fDeltaHeight = 0;
        bool m_bPinStick = false;//拖拽joy
        bool m_bClickGroud = false;//模拟点地

        [SerializeField, Tooltip("The child graphic that will be moved around")]
        RectTransform _JoystickGroup = null;
        RectTransform _JoystickPole = null;

        public RectTransform JoystickGroup
        {
            get { return _JoystickGroup; }
            set
            {
                _JoystickGroup = value;
            }
        }

        //[SerializeField]
        Vector2 _DisplayAxis;

        [SerializeField, Tooltip("How fast the joystick will go back to the center")]
        float _SpringFactor = 25;
        public float SpringFactor
        {
            get { return _SpringFactor; }
            set { _SpringFactor = value; }
        }

        [SerializeField, Tooltip("How close to the center that the axis will be output as 0")]
        float _DeadZone = 0.1f;
        public float DeadZone
        {
            get { return _DeadZone; }
            set { _DeadZone = value; }
        }

        [SerializeField, Tooltip("Click Screen(LimitedX) Move The Joystick")]
        public float m_fMoveLimitedX = 220.0f;
        [SerializeField, Tooltip("Click Screen(LimitedY) Move The Joystick")]
        public float m_fMoveLimitedY = 220.0f;

        [Tooltip("Customize the output that is sent in OnValueChange")]
        public AnimationCurve _OutputCurve = new AnimationCurve(new Keyframe(0, 0, 1, 1), new Keyframe(1, 1, 1, 1));

        [System.Serializable]
        public class JoystickMoveEvent : UnityEvent<Vector2> { }
        public JoystickMoveEvent OnValueChange;

        Vector2 _InputAxis = Vector2.zero;
        public Vector2 JoystickAxis
        {
            get
            {
                return _InputAxis;
            }
            set { SetAxis(value); }
        }

        RectTransform _RectTransform;
        public RectTransform rectTransform
        {
            get
            {
                if (!_RectTransform) _RectTransform = transform as RectTransform;

                return _RectTransform;
            }
        }

        private bool _IsDragging;
        private int _DragFingerId = -1;
        private LuaFunction _JoyStickCallbackFunc;

        public OnJoystickDragCallback JoystickDragEndCb = null;

        [HideInInspector]
        bool _DontCallEvent;

        public Vector3 MoveDir
        {
            get
            {
                if (JoystickAxis == Vector2.zero)
                    return Vector3.zero;

                Vector3 vCamForward = CCamCtrlMan.Instance.MainCameraDir;
                vCamForward.y = 0;
                vCamForward = Vector3.Normalize(vCamForward);

                Vector3 vCamRight = CCamCtrlMan.Instance.MainCameraRight;
                vCamRight.y = 0;
                vCamRight = Vector3.Normalize(vCamRight);

                return (vCamForward * JoystickAxis.y + vCamRight * JoystickAxis.x);
            }
        }

        //Vector2 _LastClickPos = Vector2.zero;
        //float _LastClickTime = 0;

        private float _LastSyncTime = 0;

        public bool IsDragging()
        {
            return _IsDragging;
        }

        public int DragFingerId()
        {
            return _DragFingerId;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!IsActive() || eventData == null)
                return;

            if (!_IsDragging)
            {
                if (eventData.pointerPress == gameObject)
                {
                    if (!m_bPinStick)       //Pin the stick group to that position
                    {
                        MoveJoyStickGroup(eventData);
                        SetAxis(Vector2.zero);
                        m_bPinStick = true;
                    }
                }

                _IsDragging = true;
                _DragFingerId = eventData.pointerId;
                _DontCallEvent = true;
                SendMsgJoystickPressEvent();
            }
            m_bClickGroud = false;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (_IsDragging && (eventData == null || eventData.pointerId == _DragFingerId))
            {
                _IsDragging = false;
                _DragFingerId = -1;
                m_bPinStick = false;
                SendMsgJoystickReleaseEvent();

                if (JoystickDragEndCb != null)
                     JoystickDragEndCb();

                if (_JoystickGroup != null)
                {
                    _JoystickGroup.anchoredPosition = m_OriginalPos;
                }
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (_IsDragging && (eventData == null || eventData.pointerId == _DragFingerId))
            {
                Vector2 newAxis = eventData.position - eventData.pressPosition;

                newAxis.x /= rectTransform.sizeDelta.x * .5f;
                newAxis.y /= rectTransform.sizeDelta.y * .5f;

                SetAxis(newAxis);
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (_JoystickGroup != null)
            {
                _JoystickGroup.anchoredPosition = m_OriginalPos;
            }

            ////模拟OnClickGround
            //if (m_bClickGroud)
            //{
            //    InputManager.Instance.HandleClick(eventData.position, true);
            //}
            //m_bClickGroud = false;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            ////if (eventData.pointerCurrentRaycast.gameObject.name == "Image")
            ////if (eventData.pointerCurrentRaycast.gameObject != JoystickGraphic.gameObject)
            ////if (eventData.pointerPress == gameObject)
            ////{
            //    m_bClickGroud = true;
            ////}
        }

        public void OnSimulateDrag(int x, int y)
        {
            if (!IsActive())
                return;

            if (x == 0 && y == 0)
            {
                OnEndDrag(null);
                return;
            }

            if (_DisplayAxis.sqrMagnitude > 1e-3f)
            {
                _IsDragging = true;
                _DragFingerId = -1;
                _DontCallEvent = true;
                SendMsgJoystickPressEvent();
            }

            _DisplayAxis.x = x;
            _DisplayAxis.y = y;

            SetAxis(_DisplayAxis);

            _DontCallEvent = true;
        }

        public void SetAxis(Vector2 axis)
        {
            _DisplayAxis = axis;

            float fMagnitude = _DisplayAxis.magnitude;
            if (fMagnitude > 1f)
            {
                _DisplayAxis = _DisplayAxis / fMagnitude;
            }

            if (fMagnitude > _DeadZone)
            {
                _InputAxis = _DisplayAxis;
                _InputAxis *= _OutputCurve.Evaluate(fMagnitude);
            }
            else
            {
                _InputAxis = Vector2.zero;
            }

            if (!_DontCallEvent && OnValueChange != null)
                OnValueChange.Invoke(_InputAxis);

            UpdateJoystickGraphic();
        }

        void OnDeselect()
        {
            _IsDragging = false;
            _DragFingerId = -1;
        }

        void SendMsgJoystickPressEvent()
        {
            if (_JoyStickCallbackFunc == null)
                _JoyStickCallbackFunc = LuaScriptMgr.Instance.GetLuaFunction("OnJoystickPressEvent");

            if (_JoyStickCallbackFunc != null)
                _JoyStickCallbackFunc.Call(JoystickAxis.x, JoystickAxis.y);
        }

        void SendMsgJoystickReleaseEvent()
        {
            if (_JoyStickCallbackFunc == null)
                _JoyStickCallbackFunc = LuaScriptMgr.Instance.GetLuaFunction("OnJoystickPressEvent");

            if (_JoyStickCallbackFunc != null)
                _JoyStickCallbackFunc.Call(0, 0);
        }

        protected override void Awake()
        {
            instance = this;

            if (_JoystickGroup != null)
            {
                m_OriginalPos = _JoystickGroup.anchoredPosition;
                //m_RectTFimgMove = gameObject.FindChild("Img_Move").GetComponent<RectTransform>();

                if (_JoystickPole == null)
                {
                    _JoystickPole = _JoystickGroup.Find("Img_Pole") as RectTransform;
                }
            }
        }

        void Update()
        {
            if (_IsDragging && !_DontCallEvent && Time.time - _LastSyncTime > 0.2f)
            {
                SendMsgJoystickPressEvent();
                _LastSyncTime = Time.time;
            }
        }

        void LateUpdate()
        {
            if (!_IsDragging && _DisplayAxis != Vector2.zero)
            {

                Vector2 newAxis = _DisplayAxis * Mathf.Max(0.0f, (1.0f - Time.unscaledDeltaTime * _SpringFactor));

                if (newAxis.sqrMagnitude <= 0.0001f)
                    newAxis = Vector2.zero;

                SetAxis(newAxis);
            }

            _DontCallEvent = false;
        }

        void MoveJoyStickGroup(PointerEventData eventData)
        {
            if (_JoystickGroup != null)
            {
                Vector2 click_pos;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, eventData.position, eventData.pressEventCamera, out click_pos);
                _JoystickGroup.anchoredPosition = click_pos;
            }
        }

        void UpdateJoystickGraphic()
        {
            if (_JoystickPole != null)
                _JoystickPole.anchoredPosition = new Vector2(_DisplayAxis.x * rectTransform.sizeDelta.x, _DisplayAxis.y * rectTransform.sizeDelta.y) * 0.5f;
        }


    }
}

#if UNITY_EDITOR
static class JoystickGameObjectCreator
{
    [MenuItem("GameObject/UI/Joystick", false, 2000)]
    static void Create()
    {
        GameObject go = new GameObject("Joystick", typeof(Joystick));

        Canvas canvas = Selection.activeGameObject ? Selection.activeGameObject.GetComponent<Canvas>() : null;

        Selection.activeGameObject = go;

        if (!canvas)
            canvas = Object.FindObjectOfType<Canvas>();

        if (!canvas)
        {
            canvas = new GameObject("Canvas", typeof(Canvas), typeof(RectTransform), typeof(GraphicRaycaster)).GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        }

        if (canvas)
            go.transform.SetParent(canvas.transform, false);

        GameObject area = new GameObject("Img_Area", typeof(Image));
        GameObject background = new GameObject("Img_BG", typeof(Image));
        GameObject pole = new GameObject("Img_Pole", typeof(Image));

        Image img_area = area.GetComponent<Image>();
        img_area.raycastTarget = true;

        Image img_pole = pole.GetComponent<Image>();
        img_pole.raycastTarget = true;

        area.transform.SetParent(go.transform, false);
        background.transform.SetParent(go.transform, false);
        pole.transform.SetParent(background.transform, false);

        background.GetComponent<Image>().color = new Color(1, 1, 1, .86f);

        RectTransform backgroundTransform = background.transform as RectTransform;
        RectTransform poleTransform = pole.transform as RectTransform;

        poleTransform.sizeDelta = backgroundTransform.sizeDelta * .5f;

        Joystick joystick = go.GetComponent<Joystick>();
        joystick.JoystickGroup = backgroundTransform;
    }
}
#endif