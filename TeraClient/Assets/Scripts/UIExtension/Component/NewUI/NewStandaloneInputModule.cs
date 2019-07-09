using System;
using UnityEngine;
using UnityEngine.Serialization;
using System.Text;
using System.Collections.Generic;
using UnityEngine.Events;

namespace UnityEngine.EventSystems
{
    [AddComponentMenu("Event/New Standalone Input Module")]
    public class NewStandaloneInputModule : StandaloneInputModule
    {
        [NonSerialized]
        public UnityAction<GameObject> beforeClickCallBack;
        //[NonSerialized]
        //public UnityAction OnProcessFinish;

        protected override void OnDestroy()
        {
            base.OnDestroy();
            beforeClickCallBack = null;
            //OnProcessFinish = null;
        }

        public override void Process()
        {
            ClearEventData();

            bool usedEvent = SendUpdateEventToSelectedObject();

            if (eventSystem.sendNavigationEvents)
            {
                //18-01-03 no longer allow keyboard affect selected object.
                //if (!usedEvent)
                //    usedEvent |= SendMoveEventToSelectedObject();

                if (!usedEvent)
                    SendSubmitEventToSelectedObject();
            }

            // touch needs to take precedence because of the mouse emulation layer
            if (!ProcessTouchEvents() && input.mousePresent)
                ProcessMouseEvent();

            //if (OnProcessFinish != null)
            //    OnProcessFinish();
        }

        private bool ProcessTouchEvents()
        {
            for (int i = 0; i < input.touchCount; ++i)
            {
                Touch touch = input.GetTouch(i);

                if (touch.type == TouchType.Indirect)
                    continue;

                bool released;
                bool pressed;
                var pointer = GetTouchPointerEventData(touch, out pressed, out released);

                SaveEventData(pointer);

                ProcessTouchPress(pointer, pressed, released);

                if (!released)
                {
                    ProcessMove(pointer);
                    ProcessDrag(pointer);
                }
                else
                    RemovePointerData(pointer);
            }
            return input.touchCount > 0;
        }

        protected new void ProcessTouchPress(PointerEventData pointerEvent, bool pressed, bool released)
        {
            var currentOverGo = pointerEvent.pointerCurrentRaycast.gameObject;

            // PointerDown notification
            if (pressed)
            {
                pointerEvent.eligibleForClick = true;
                pointerEvent.delta = Vector2.zero;
                pointerEvent.dragging = false;
                pointerEvent.useDragThreshold = true;
                pointerEvent.pressPosition = pointerEvent.position;
                pointerEvent.pointerPressRaycast = pointerEvent.pointerCurrentRaycast;

                DeselectIfSelectionChanged(currentOverGo, pointerEvent);

                if (pointerEvent.pointerEnter != currentOverGo)
                {
                    // send a pointer enter to the touched element if it isn't the one to select...
                    HandlePointerExitAndEnter(pointerEvent, currentOverGo);
                    pointerEvent.pointerEnter = currentOverGo;
                }

                // search for the control that will receive the press
                // if we can't find a press handler set the press
                // handler to be what would receive a click.
                var newPressed = ExecuteEvents.ExecuteHierarchy(currentOverGo, pointerEvent, ExecuteEvents.pointerDownHandler);

                // didnt find a press handler... search for a click handler
                if (newPressed == null)
                    newPressed = ExecuteEvents.GetEventHandler<IPointerClickHandler>(currentOverGo);

                // Debug.Log("Pressed: " + newPressed);

                float time = Time.unscaledTime;

                if (newPressed == pointerEvent.lastPress)
                {
                    var diffTime = time - pointerEvent.clickTime;
                    if (diffTime < 0.3f)
                        ++pointerEvent.clickCount;
                    else
                        pointerEvent.clickCount = 1;

                    pointerEvent.clickTime = time;
                }
                else
                {
                    pointerEvent.clickCount = 1;
                }

                pointerEvent.pointerPress = newPressed;
                pointerEvent.rawPointerPress = currentOverGo;

                pointerEvent.clickTime = time;

                // Save the drag handler as well
                pointerEvent.pointerDrag = ExecuteEvents.GetEventHandler<IDragHandler>(currentOverGo);

                if (pointerEvent.pointerDrag != null)
                    ExecuteEvents.Execute(pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.initializePotentialDrag);
            }

            // PointerUp notification
            if (released)
            {
                // Debug.Log("Executing pressup on: " + pointer.pointerPress);
                ExecuteEvents.Execute(pointerEvent.pointerPress, pointerEvent, ExecuteEvents.pointerUpHandler);

                // Debug.Log("KeyCode: " + pointer.eventData.keyCode);

                // see if we mouse up on the same element that we clicked on...
                var pointerUpHandler = ExecuteEvents.GetEventHandler<IPointerClickHandler>(currentOverGo);

                // PointerClick and Drop events
                if (pointerEvent.pointerPress == pointerUpHandler && pointerEvent.eligibleForClick)
                {
#if !Debug_Proj
                    if (beforeClickCallBack != null) beforeClickCallBack(pointerEvent.pointerPress);
#endif
                    ExecuteEvents.Execute(pointerEvent.pointerPress, pointerEvent, ExecuteEvents.pointerClickHandler);
                }
                else if (pointerEvent.pointerDrag != null && pointerEvent.dragging)
                {
                    ExecuteEvents.ExecuteHierarchy(currentOverGo, pointerEvent, ExecuteEvents.dropHandler);
                }

                pointerEvent.eligibleForClick = false;
                pointerEvent.pointerPress = null;
                pointerEvent.rawPointerPress = null;

                if (pointerEvent.pointerDrag != null && pointerEvent.dragging)
                    ExecuteEvents.Execute(pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.endDragHandler);

                pointerEvent.dragging = false;
                pointerEvent.pointerDrag = null;

                //
                //                if (pointerEvent.pointerDrag != null)
                //                    ExecuteEvents.Execute(pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.endDragHandler);
                //
                //                pointerEvent.pointerDrag = null;
                //

                // send exit events as we need to simulate this on touch up on touch device
                ExecuteEvents.ExecuteHierarchy(pointerEvent.pointerEnter, pointerEvent, ExecuteEvents.pointerExitHandler);
                pointerEvent.pointerEnter = null;
            }
        }

        protected new void ProcessMouseEvent()
        {
            ProcessMouseEvent(0);
        }

        /// <summary>
        /// Process all mouse events.
        /// </summary>
        protected new void ProcessMouseEvent(int id)
        {
            var mouseData = GetMousePointerEventData(id);
            var leftButtonData = mouseData.GetButtonState(PointerEventData.InputButton.Left).eventData;

            //if (ForceAutoSelect())
            //    eventSystem.SetSelectedGameObject(leftButtonData.buttonData.pointerCurrentRaycast.gameObject, leftButtonData.buttonData);

            if (Input.GetMouseButtonUp(0) || Input.GetMouseButton(0) || Input.GetMouseButtonUp(1) || Input.GetMouseButton(1))
            {
                SaveEventData(mouseData.GetButtonState(PointerEventData.InputButton.Left).eventData.buttonData);
            }

            // Process the first mouse button fully
            ProcessMousePress(leftButtonData);
            ProcessMove(leftButtonData.buttonData);
            ProcessDrag(leftButtonData.buttonData);

            // Now process right / middle clicks
            ProcessMousePress(mouseData.GetButtonState(PointerEventData.InputButton.Right).eventData);
            ProcessDrag(mouseData.GetButtonState(PointerEventData.InputButton.Right).eventData.buttonData);
            //ProcessMousePress(mouseData.GetButtonState(PointerEventData.InputButton.Middle).eventData);
            //ProcessDrag(mouseData.GetButtonState(PointerEventData.InputButton.Middle).eventData.buttonData);

            if (!Mathf.Approximately(leftButtonData.buttonData.scrollDelta.sqrMagnitude, 0.0f))
            {
                var scrollHandler = ExecuteEvents.GetEventHandler<IScrollHandler>(leftButtonData.buttonData.pointerCurrentRaycast.gameObject);
                ExecuteEvents.ExecuteHierarchy(scrollHandler, leftButtonData.buttonData, ExecuteEvents.scrollHandler);
            }
        }

        /// <summary>
        /// Process the current mouse press.
        /// </summary>
        protected new void ProcessMousePress(MouseButtonEventData data)
        {
            var pointerEvent = data.buttonData;
            var currentOverGo = pointerEvent.pointerCurrentRaycast.gameObject;

            // PointerDown notification
            if (data.PressedThisFrame())
            {
                pointerEvent.eligibleForClick = true;
                pointerEvent.delta = Vector2.zero;
                pointerEvent.dragging = false;
                pointerEvent.useDragThreshold = true;
                pointerEvent.pressPosition = pointerEvent.position;
                pointerEvent.pointerPressRaycast = pointerEvent.pointerCurrentRaycast;

                DeselectIfSelectionChanged(currentOverGo, pointerEvent);

                // search for the control that will receive the press
                // if we can't find a press handler set the press
                // handler to be what would receive a click.
                var newPressed = ExecuteEvents.ExecuteHierarchy(currentOverGo, pointerEvent, ExecuteEvents.pointerDownHandler);

                // didnt find a press handler... search for a click handler
                if (newPressed == null)
                    newPressed = ExecuteEvents.GetEventHandler<IPointerClickHandler>(currentOverGo);

                // Debug.Log("Pressed: " + newPressed);

                float time = Time.unscaledTime;

                if (newPressed == pointerEvent.lastPress)
                {
                    var diffTime = time - pointerEvent.clickTime;
                    if (diffTime < 0.3f)
                        ++pointerEvent.clickCount;
                    else
                        pointerEvent.clickCount = 1;

                    pointerEvent.clickTime = time;
                }
                else
                {
                    pointerEvent.clickCount = 1;
                }

                pointerEvent.pointerPress = newPressed;
                pointerEvent.rawPointerPress = currentOverGo;

                pointerEvent.clickTime = time;

                // Save the drag handler as well
                pointerEvent.pointerDrag = ExecuteEvents.GetEventHandler<IDragHandler>(currentOverGo);

                if (pointerEvent.pointerDrag != null)
                    ExecuteEvents.Execute(pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.initializePotentialDrag);
            }

            // PointerUp notification
            if (data.ReleasedThisFrame()
                //Fix:release button when App loses focus
                || (!input.GetMouseButton(0) && (!input.GetMouseButton(1)) && pointerEvent.pointerPress != null))
            {
                // Debug.Log("Executing pressup on: " + pointer.pointerPress);
                ExecuteEvents.Execute(pointerEvent.pointerPress, pointerEvent, ExecuteEvents.pointerUpHandler);

                // Debug.Log("KeyCode: " + pointer.eventData.keyCode);

                // see if we mouse up on the same element that we clicked on...
                var pointerUpHandler = ExecuteEvents.GetEventHandler<IPointerClickHandler>(currentOverGo);

                // PointerClick and Drop events
                if (pointerEvent.pointerPress == pointerUpHandler && pointerEvent.eligibleForClick)
                {
#if !Debug_Proj
                    if (beforeClickCallBack != null) beforeClickCallBack(pointerEvent.pointerPress);
#endif
                    ExecuteEvents.Execute(pointerEvent.pointerPress, pointerEvent, ExecuteEvents.pointerClickHandler);
                }
                else if (pointerEvent.pointerDrag != null && pointerEvent.dragging)
                {
                    ExecuteEvents.ExecuteHierarchy(currentOverGo, pointerEvent, ExecuteEvents.dropHandler);
                }

                pointerEvent.eligibleForClick = false;
                pointerEvent.pointerPress = null;
                pointerEvent.rawPointerPress = null;

                if (pointerEvent.pointerDrag != null && pointerEvent.dragging)
                    ExecuteEvents.Execute(pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.endDragHandler);

                pointerEvent.dragging = false;
                pointerEvent.pointerDrag = null;

                // redo pointer enter / exit to refresh state
                // so that if we moused over somethign that ignored it before
                // due to having pressed on something else
                // it now gets it.
                if (currentOverGo != pointerEvent.pointerEnter)
                {
                    HandlePointerExitAndEnter(pointerEvent, null);
                    HandlePointerExitAndEnter(pointerEvent, currentOverGo);
                }
            }
        }

        //[System.NonSerialized]
        //public bool IsPointedUI = false;  //I have further use of this ! ;

        public struct PointerHist
        {
            public PointerEventData data;
            public bool notEmpty;
        }

        private List<PointerHist> _lastPoints = new List<PointerHist>();

        private void SaveEventData(PointerEventData evt)
        {
            PointerHist hist = new PointerHist();
            hist.data = evt;
            //hist.notEmpty = evt.pointerEnter != null;
            hist.notEmpty = evt.pointerCurrentRaycast.gameObject != null;

            _lastPoints.Add(hist);
        }

        //private void UpdateEventData(PointerEventData evt, bool is_handled)
        //{
        //    for (int i = 0; i < _lastPoints.Count; i++)
        //    {
        //        PointerHist hist = _lastPoints[i];
        //        if (hist.data == evt)
        //        {
        //             hist.notEmpty = hist.notEmpty || is_handled;
        //             _lastPoints[i] = hist;
        //        }
        //    }
        //}

        void ClearEventData()
        {
            _lastPoints.Clear();
        }

        public GameObject GetPressedUI(int id)
        {
            for (int i = 0; i < _lastPoints.Count; i++)
            {
                PointerHist hist = _lastPoints[i];
                if (hist.data.pointerId == id)
                {
                    return hist.data.pointerPress;
                }
            }
            return null;
        }

        public GameObject GetHoveredUI(int id)
        {
            for (int i = 0; i < _lastPoints.Count; i++)
            {
                PointerHist hist = _lastPoints[i];
                if (hist.data.pointerId == id)
                {
                    return hist.data.pointerEnter;
                }
            }
            return null;
        }

        public bool IsHoveredUI(int id)
        {
            for (int i = 0; i < _lastPoints.Count; i++)
            {
                PointerHist hist = _lastPoints[i];
                if (hist.data.pointerId == id)
                {
                    return hist.notEmpty; //|| hist.data.pointerEnter != null;
                }
            }
            return false;
        }

        #region DebugUI
        public class DebugParam
        {
            public int Id = -1;
            public bool isWantPos = false;
            public bool isWantHover = true;
            public bool isWantPress = true;
            public bool isWantDrag = false;
            public bool isWantRay = false;
        }

        public DebugParam debugParam = new DebugParam();

        public override string ToString()
        {
            var sb = HobaText.GetStringBuilder();
            sb.Append("<b>Pointer Input Module of type: </b>");
            sb.Append(GetType());
            sb.AppendLine();
            for (int i = 0; i < _lastPoints.Count; ++i)
            {
                //if (pointer.Value == null)
                //{
                //    continue;
                //}

                //PointerEventData point = pointer.Value;
                //int key=pointer.Key;
                var pointer = _lastPoints[i];
                PointerEventData point = pointer.data;
                int key = point.pointerId;

                if (debugParam.Id != -1 && point.pointerId != debugParam.Id)
                {
                    continue;
                }

                sb.AppendLine("<B>Pointer:</b> " + key);
                sb.AppendLine("<B>IsPointUI:</b> " + pointer.notEmpty);

                if (debugParam.isWantPos)
                {
                    sb.AppendLine("<b>Position</b>: " + point.position);
                    sb.AppendLine("<b>delta</b>: " + point.delta);
                }
                if (debugParam.isWantHover)
                {
                    if (point.pointerEnter != null) sb.AppendLine("<b>pointerEnter</b>: " + point.pointerEnter);
                    else sb.AppendLine("<b>pointerEnter</b>: Null");
                }
                if (debugParam.isWantPress)
                {
                    sb.AppendLine("<b>eligibleForClick</b>: " + point.eligibleForClick);

                    if (point.pointerPress != null) sb.AppendLine("<b>pointerPress</b>: " + point.pointerPress);
                    else sb.AppendLine("<b>pointerPress</b>: Null");
                    if (point.lastPress != null) sb.AppendLine("<b>lastPress</b>: " + point.lastPress);
                    else sb.AppendLine("<b>lastPress</b>: Null");
                }
                if (debugParam.isWantDrag)
                {
                    if (point.pointerDrag != null) sb.AppendLine("<b>pointerDrag</b>: " + point.pointerDrag);
                    else sb.AppendLine("<b>pointerDrag</b>: Null");

                    sb.AppendLine("<b>Use Drag Threshold</b>: " + point.useDragThreshold);
                }
                if (debugParam.isWantRay)
                {
                    sb.AppendLine("<b>Current Rayast:</b>");
                    sb.AppendLine(point.pointerCurrentRaycast.ToString());
                    sb.AppendLine("<b>Press Rayast:</b>");
                    sb.AppendLine(point.pointerPressRaycast.ToString());
                }
            }
            return sb.ToString();
        }
        #endregion
    }
}
