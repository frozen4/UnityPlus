using Common;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using EntityComponent;
using UnityEngine.Events;
using System.Collections.Generic;

public struct MouseTouch
{
    public int fingerId;
    public Vector2 position;
    public Vector2 deltaPosition;
    public float deltaTime;
    public int tapCount;
    public TouchPhase phase;
}

public struct CommonTouch
{
    public bool isMoushTouch;
    public MouseTouch mouseTouch;
    public Touch fingerTouch;

    public int fingerId { get { return isMoushTouch ? mouseTouch.fingerId : fingerTouch.fingerId; } }
    public Vector2 position { get { return isMoushTouch ? mouseTouch.position : fingerTouch.position; } }
    public Vector2 deltaPosition { get { return isMoushTouch ? mouseTouch.deltaPosition : fingerTouch.deltaPosition; } }
    public float deltaTime { get { return isMoushTouch ? mouseTouch.deltaTime : fingerTouch.deltaTime; } }
    public int tapCount { get { return isMoushTouch ? mouseTouch.tapCount : fingerTouch.tapCount; } }
    public TouchPhase phase { get { return isMoushTouch ? mouseTouch.phase : fingerTouch.phase; } }
}

public class TouchState
{
    public CommonTouch _ThisTouch;
    public CommonTouch _LastTouch;

    public CommonTouch _BeginTouch;
    public CommonTouch _EndTouch;

    public bool _HasMoved = false;

    public bool Update(CommonTouch touch)
    {
        _LastTouch = _ThisTouch;
        _ThisTouch = touch;

        switch (_ThisTouch.phase)
        {
            case TouchPhase.Began:
                _BeginTouch = _ThisTouch;
                _HasMoved = false;
                break;

            case TouchPhase.Moved:
                if (!_HasMoved && !IsInScope(_BeginTouch.position, _ThisTouch.position))
                    _HasMoved = true;
                break;

            case TouchPhase.Stationary:
                break;

            case TouchPhase.Ended:
                _EndTouch = _ThisTouch;
                break;
        }
        return true;
    }

    protected bool IsInScope(Vector2 ptOrigin, Vector2 ptPos)
    {
        return Vector2.Distance(ptOrigin, ptPos) <= 10f;
    }
}



public class InputManager : Singleton<InputManager>, GameLogic.ITickLogic
{
    public const int MaxTouchCount = 5;

    private NewStandaloneInputModule _inputModule;
    public NewStandaloneInputModule inputModule
    {
        get
        {
            if (_inputModule == null) { _inputModule = EventSystem.current.currentInputModule as NewStandaloneInputModule; }
            return _inputModule;
        }
    }

    public delegate void OnClickGround(Vector3 clickpos);
    public OnClickGround ClickHandle = null;

    private TouchState[] _TouchStates = new TouchState[MaxTouchCount];
    public TouchState[] TouchStates { get { return _TouchStates; } }

    private int _TouchCount;
    public int TouchCount { get { return _TouchCount; } }

    private bool _Inited = false;

    private bool _isMultiDragingStarted = false;       //tell whether a new 2 f drag has began
    public bool IsMultiDragingStarted { get { return _isMultiDragingStarted; } }

    public bool IsInCmdsInputting = false;

    public bool Init()
    {
        for (int i = 0; i < MaxTouchCount; ++i)
            _TouchStates[i] = new TouchState();

        if (inputModule != null)
        {
            inputModule.beforeClickCallBack = HandleClickOnTips;
            //inputModule.OnProcessFinish = EventProcessCB;
        }

        _Inited = true;
        return true;
    }

    public void Tick(float dt)
    {
        if (CGManager.Instance.IsForbidInput())
            return;

        if (!_Inited)
            return;

        UpdateTouchStates(dt);

        UpdateToHandleInput();

        UpdateSleepingMode(dt);
    }

    //private void EventProcessCB()
    //{
    //    float dt = Time.deltaTime;
    //}

    public void LateTick(float dt)
    {
        if (!CGManager.Instance.IsForbidInput())
        {
            if (!_Inited)
                return;

#if UNITY_EDITOR || (!UNITY_IPHONE && !UNITY_ANDROID)
            if (!IsInCmdsInputting)
            {
                UpdateKeyboardInput();
                if (Time.frameCount % 5 == 0)
                    UpdateKeyboardInput2();
            }
#endif
        }

        if (!(CGManager.Instance.IsForbidInput() && CGManager.Instance.CanSkip))
        {
            UpdateESCKey(dt);
        }
    }

    //Esc键处理
    private const float EscCountDownLimit = 0.25f;
    private int _EscCount = 0;
    private float _EscCountDown = 0;
    void UpdateESCKey(float delta)
    {
        if (_EscCount > 0)
        {
            _EscCountDown += delta;
            if (_EscCountDown > EscCountDownLimit)
            {
                //2 = down and up once, 4 =...
                if (_EscCount >= 4)
                {
                    LuaScriptMgr.Instance.CallOnDoubleInputKeyCode((int)KeyCode.Escape);
                }
                else if (_EscCount >= 2)
                {
                    LuaScriptMgr.Instance.CallOnInputKeyCode((int)KeyCode.Escape);
                }

                else if (_EscCount == 1)
                {
                    Debug.LogWarning("ESC too slow");
                }

                _EscCount = 0;
                _EscCountDown = 0;
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (_EscCount % 2 == 0)
            {
                _EscCount += 1;
                //Debug.Log("ESC KeyDown + 1 _EscCount=" + _EscCount);
            }
        }

        if (Input.GetKeyUp(KeyCode.Escape))
        {
            if (_EscCount % 2 == 1)
            {
                _EscCount += 1;
                //Debug.Log("ESC KeyUp + 1 _EscCount=" + _EscCount);
            }
        }
    }

    #region power_saving

    //Sleeping
    //float _SleepCounts = 999999;
    private float _SleepCountDown = 0;
    private bool _IsSleepingCD = false;

    private float _PowerSavingCount = 180; //{ get { return EntryPoint.Instance.GameCustomConfigParams.PowerSavingCount; } }

    public void SetSleepingCD(float cd)
    {
        _PowerSavingCount = cd;
    }

    private void UpdateSleepingMode(float delta)
    {
        if (_IsSleepingCD)
        {
#if UNITY_IPHONE || UNITY_ANDROID
        bool has_touch = Input.touchCount > 0;
#else
            bool has_touch = Input.anyKey || Input.anyKeyDown;
#endif

            if (!has_touch)
            {
                _SleepCountDown -= delta;
                if (_SleepCountDown <= 0)
                {
                    //HobaDebuger.Log("PowerSaving Sleep Begin.");
                    _IsSleepingCD = false;
                    _SleepCountDown = _PowerSavingCount;

                    LuaScriptMgr.Instance.CallLuaBeginSleeping();
                }
            }
            else
            {
                _SleepCountDown = _PowerSavingCount;
            }
        }
    }

    //public void SetSleepCounts(float count_down)
    //{
    //    _SleepCounts = count_down;
    //}

    public void ResetSleepCD()
    {
        _SleepCountDown = _PowerSavingCount;
    }

    public void EnableSleepCD(bool is_enable)
    {
        if (_IsSleepingCD != is_enable)
        {
            _IsSleepingCD = is_enable;
        }
    }

    #endregion power_saving

    bool UpdateTouchStates(float fDeltaTime)
    {
        _TouchCount = 0;
#if UNITY_IPHONE || UNITY_ANDROID
        int count = Input.touchCount;
        for (int i = 0; i < count; ++i)
        {
            if (_TouchCount >= MaxTouchCount)
                break;

            Touch touch = Input.GetTouch(i);

            CommonTouch ct = new CommonTouch();
            ct.isMoushTouch = false;
            ct.fingerTouch = touch;

            _TouchStates[_TouchCount].Update(ct);

            ++_TouchCount;
        }
#else

        bool has_mouse_touch = Input.GetMouseButton(0) || Input.GetMouseButton(1) || Input.GetMouseButtonDown(0) || Input.GetMouseButtonUp(0) || Input.GetMouseButtonDown(1) || Input.GetMouseButtonUp(1);
        if (!has_mouse_touch) return true;

        CommonTouch touch2 = new CommonTouch();
        touch2.isMoushTouch = true;
        touch2.mouseTouch.fingerId = -1;
        touch2.mouseTouch.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);

        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
            touch2.mouseTouch.phase = TouchPhase.Began;
        else if (Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1))
            touch2.mouseTouch.phase = TouchPhase.Ended;
        else
            touch2.mouseTouch.phase = TouchPhase.Moved;

        if (_TouchCount < MaxTouchCount)
        {
            _TouchStates[_TouchCount].Update(touch2);
            ++_TouchCount;
        }

        //_IsPointerOverUI = IsPointerOverUIObject(touch2);
#endif
        return true;
    }

    public static bool HasTouch()
    {
        if (EventSystem.current.currentSelectedGameObject != null)
            return false;

#if UNITY_IPHONE || UNITY_ANDROID
        int count = Input.touchCount;
        return count > 0;
#else

        //bool has_mouse_touch = Input.GetMouseButton(0) || Input.GetMouseButton(1) || Input.GetMouseButtonDown(0) || Input.GetMouseButtonUp(0) || Input.GetMouseButtonDown(1) || Input.GetMouseButtonUp(1);
        //if (!has_mouse_touch)
        //    return false;

        if (Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1))
            return true;
#endif
        return false;
    }

    public static bool HasTouchOne()
    {
#if UNITY_IPHONE || UNITY_ANDROID
		int count = Input.touchCount;
		return count > 0;
#else

        bool has_mouse_touch = Input.GetMouseButton(0) || Input.GetMouseButton(1) || Input.GetMouseButtonDown(0) || Input.GetMouseButtonUp(0) || Input.GetMouseButtonDown(1) || Input.GetMouseButtonUp(1);
        return has_mouse_touch;

#endif
    }

    //    public static bool HasAnyTouch()
    //    {
    //#if UNITY_IPHONE || UNITY_ANDROID
    //        return Input.touchCount > 0;
    //#else
    //        return Input.anyKey || Input.anyKeyDown;
    //#endif
    //    }

    //#if UNITY_IPHONE || UNITY_ANDROID
    public bool IsPointerOverUIObject(CommonTouch touch)
    {
        if (inputModule != null)
        {
            return inputModule.IsHoveredUI(touch.fingerId);
        }

        return EventSystem.current.IsPointerOverGameObject(touch.fingerId) || EventSystem.current.currentSelectedGameObject != null;
    }
    //#endif

    public GameObject GetPointerOverUI(int touchID)
    {
        if (inputModule != null)
        {
            return inputModule.GetHoveredUI(touchID);
        }

        return null;
    }

    public GameObject GetPointerPressUI(int touchID)
    {
        if (inputModule != null)
        {
            return inputModule.GetPressedUI(touchID);
        }

        return null;
    }

    private bool IsThisUIAllowedDrag(int touchID)
    {
        var go = GetPointerOverUI(touchID);
        if (go == null) return false;
        if (go.tag == CUnityUtil.Tag_UIDrag)
            return true;
        else
            return false;
    }

    #region Game Control

    void UpdateToHandleInput()
    {
        CAM_CTRL_MODE mode = CCamCtrlMan.Instance.GetCurCamCtrlMode();

        if (mode == CAM_CTRL_MODE.LOGIN)
        {
            UpdateToHandleInput_LoginMode();
        }
        else if (mode == CAM_CTRL_MODE.GAME)
        {
            UpdateToHandleInput_Game();
        }
        else if (mode == CAM_CTRL_MODE.EXTERIOR || mode == CAM_CTRL_MODE.NEAR)
        {
            // 停止移动
            if (IsJoystickDraging())
            {
                Joystick.Instance.OnSimulateDrag(0, 0);
            }
            UpdateToHandleInput_Game();
        }
        else if (mode == CAM_CTRL_MODE.BOSS)
        {
            // 停止移动
            if (IsJoystickDraging())
            {
                Joystick.Instance.OnSimulateDrag(0, 0);
            }
        }
    }

    void UpdateToHandleInput_LoginMode()
    {
        if (_TouchCount == 1)
        {
            TouchState touch_state = _TouchStates[0];
            if (IsPointerOverUIObject(touch_state._ThisTouch))
                return;
            if (touch_state._ThisTouch.phase != TouchPhase.Began)
            {
                float delta = touch_state._ThisTouch.position.x - touch_state._LastTouch.position.x;
                if (Mathf.Abs(delta) > 0)
                    LuaScriptMgr.Instance.CallLuaOnSingleDragFunc(delta);
            }
        }
        else if (_TouchCount == 2)
        {
            float delta = DealWithTwoFingers(_TouchStates[0], _TouchStates[1]);
            if (Mathf.Abs(delta) > 0)
            {
                LuaScriptMgr.Instance.CallLuaOnTwoFingersDragFunc(delta);
            }
        }
    }

    //private bool _IsPointerOverUI = true;
    private bool _LastMultiTouch = false;

    private void UpdateToHandleInput_Game()
    {
        if (_TouchCount == 0)
        {
            _LastMultiTouch = false;
            _isMultiDragingStarted = false;
        }
        else if (_TouchCount == 1)
        {
//             if (IsJoystickDraging())
//                 return;

            if (!_LastMultiTouch)
                DealWithOneFinger(_TouchStates[0]);

            _LastMultiTouch = false;
            _isMultiDragingStarted = false;
        }
#if UNITY_IPHONE || UNITY_ANDROID
        else if (_TouchCount == 2)
        {
            int fingerId;
            if (IsJoystickDraging(out fingerId))
            {
                if(_TouchStates[0]._ThisTouch.fingerId == fingerId)
                    DealWithOneFinger(_TouchStates[1]);
                else
                    DealWithOneFinger(_TouchStates[0]);
            }
            else
            {
                DealWithTwoFingers(_TouchStates[0], _TouchStates[1]);
            }

            _LastMultiTouch = true;
        }
        else if(_TouchCount >= 3)
        {
            if (IsJoystickDraging())
                DealWithTwoFingers(_TouchStates[1], _TouchStates[2]);
            else
                DealWithTwoFingers(_TouchStates[0], _TouchStates[1]);

            _LastMultiTouch = true;
        }
#endif
        else
        {
            _isMultiDragingStarted = false;
        }
    }

#if !UNITY_IPHONE && !UNITY_ANDROID
    private KeyCode lastKey = KeyCode.X;
#endif
    private int lastX = 0, lastY = 0;
    void UpdateKeyboardInput()
    {

        KeyCode curKey = KeyCode.X;
        if (Input.GetKey(KeyCode.UpArrow))
        {
            CCamCtrlMan.Instance.ManualAdjustCamera(CCamCtrlMan.ADJUST_STYLE.PITCH, 0.25f);
            curKey = KeyCode.UpArrow;
        }
        else if (Input.GetKey(KeyCode.DownArrow))
        {
            CCamCtrlMan.Instance.ManualAdjustCamera(CCamCtrlMan.ADJUST_STYLE.PITCH, -0.25f);
            curKey = KeyCode.DownArrow;
        }
        else if (Input.GetKey(KeyCode.LeftArrow))
        {
            CCamCtrlMan.Instance.ManualAdjustCamera(CCamCtrlMan.ADJUST_STYLE.YAW, 0.25f);
            curKey = KeyCode.LeftArrow;
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            CCamCtrlMan.Instance.ManualAdjustCamera(CCamCtrlMan.ADJUST_STYLE.YAW, -0.25f);
            curKey = KeyCode.RightArrow;
        }
        else if (Input.GetKey(KeyCode.Home))
        {
#if !UNITY_IPHONE && !UNITY_ANDROID
            if (lastKey == KeyCode.Home)
                _isMultiDragingStarted = true;
            else
                _isMultiDragingStarted = false;
#endif
            CCamCtrlMan.Instance.ManualAdjustCamera(CCamCtrlMan.ADJUST_STYLE.DISTANCE, -0.25f);
            curKey = KeyCode.Home;
        }
        else if (Input.GetKey(KeyCode.End))
        {
#if !UNITY_IPHONE && !UNITY_ANDROID
            if (lastKey == KeyCode.End)
                _isMultiDragingStarted = true;
            else
                _isMultiDragingStarted = false;
#endif
            CCamCtrlMan.Instance.ManualAdjustCamera(CCamCtrlMan.ADJUST_STYLE.DISTANCE, 0.25f);
            curKey = KeyCode.End;
        }
        else
        {
            float delta = Input.GetAxis("Mouse ScrollWheel");
            if (delta != 0.0f)
            {
                CCamCtrlMan.Instance.ManualAdjustCamera(CCamCtrlMan.ADJUST_STYLE.DISTANCE, -delta * 10);
            }

        }

#if !UNITY_IPHONE && !UNITY_ANDROID
        lastKey = curKey;
#endif

        int x = 0, y = 0;
        if (Input.GetKey(KeyCode.A))
            x = -1;
        else if (Input.GetKey(KeyCode.D))
            x = 1;

        if (Input.GetKey(KeyCode.W))
            y = 1;
        else if (Input.GetKey(KeyCode.S))
            y = -1;

        if (x != 0 || y != 0 || lastX != 0 || lastY != 0)
        {
            if (Joystick.Instance != null)
                Joystick.Instance.OnSimulateDrag(x, y);
            lastX = x;
            lastY = y;
        }
    }

    void UpdateKeyboardInput2()
    {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
        if (!EntryPoint.Instance.DebugSettingParams.ShortCut)
            return;
#endif

        if (Input.GetKey(KeyCode.Alpha0))
        {
            LuaScriptMgr.Instance.CallOnInputKeyCode((int)KeyCode.Alpha0);
        }
        else if (Input.GetKey(KeyCode.Alpha1))
        {
            LuaScriptMgr.Instance.CallOnInputKeyCode((int)KeyCode.Alpha1);
        }
        else if (Input.GetKey(KeyCode.Alpha2))
        {
            LuaScriptMgr.Instance.CallOnInputKeyCode((int)KeyCode.Alpha2);
        }
        else if (Input.GetKey(KeyCode.Alpha3))
        {
            LuaScriptMgr.Instance.CallOnInputKeyCode((int)KeyCode.Alpha3);
        }
        else if (Input.GetKey(KeyCode.Alpha4))
        {
            LuaScriptMgr.Instance.CallOnInputKeyCode((int)KeyCode.Alpha4);
        }
        else if (Input.GetKey(KeyCode.Alpha5))
        {
            LuaScriptMgr.Instance.CallOnInputKeyCode((int)KeyCode.Alpha5);
        }
        else if (Input.GetKey(KeyCode.Alpha6))
        {
            LuaScriptMgr.Instance.CallOnInputKeyCode((int)KeyCode.Alpha6);
        }
        else if (Input.GetKey(KeyCode.Alpha7))
        {
            LuaScriptMgr.Instance.CallOnInputKeyCode((int)KeyCode.Alpha7);
        }
        else if (Input.GetKey(KeyCode.Alpha8))
        {
            LuaScriptMgr.Instance.CallOnInputKeyCode((int)KeyCode.Alpha8);
        }
        else if (Input.GetKey(KeyCode.Alpha9))
        {
            LuaScriptMgr.Instance.CallOnInputKeyCode((int)KeyCode.Alpha9);
        }
        else if (Input.GetKey(KeyCode.Space))
        {
            LuaScriptMgr.Instance.CallOnInputKeyCode((int)KeyCode.Space);
        }
        else if (Input.GetKey(KeyCode.LeftAlt))
        {
            LuaScriptMgr.Instance.CallOnInputKeyCode((int)KeyCode.LeftAlt);
        }
        else if (Input.GetKey(KeyCode.F1))
        {
            LuaScriptMgr.Instance.CallOnInputKeyCode((int)KeyCode.F1);
        }
        else if (Input.GetKey(KeyCode.BackQuote))
        {
            LuaScriptMgr.Instance.CallOnInputKeyCode((int)KeyCode.BackQuote);

        }
    }

    private bool IsJoystickDraging()
    {
        bool ret = false;

        Joystick joystick = Joystick.Instance;
        if (joystick != null)
            ret = joystick.IsDragging();

        return ret;
    }

#if UNITY_IPHONE || UNITY_ANDROID
    private bool IsJoystickDraging(out int fingerId)
    {
        Joystick joystick = Joystick.Instance;

        bool ret = false;
        fingerId = 0;

        if (joystick != null)
        {
            ret = joystick.IsDragging();
            fingerId = joystick.DragFingerId();
        }

        return ret;
    }
#endif

    public bool IsPointerBeginOnUI
    {
        get { return _IsPointerBeginOnUI; }
    }

    public Vector2 CurrentTouchPosition
    {
        get { return _CurrentTouchPosition; }
    }

    private bool _IsPointerBeginOnUI = false;
    private Vector2 _CurrentTouchPosition = Vector2.zero;
    private int _OneFingerId = 0;
    private bool _IsAllowUIDrag = false;

    private void DealWithOneFinger(TouchState touch_state)
    {
        //HobaDebuger.LogWarningFormat("DealWithOneFinger fingerId: {0}", touch_state._ThisTouch.fingerId);
        _CurrentTouchPosition = touch_state._ThisTouch.position;

        if (touch_state._ThisTouch.phase == TouchPhase.Began)//点击或者触摸
        {
            _IsPointerBeginOnUI = IsPointerOverUIObject(touch_state._ThisTouch); // IsPointerOverUIObject(touch_state._ThisTouch);
            _OneFingerId = touch_state._ThisTouch.fingerId;
            _IsAllowUIDrag = IsThisUIAllowedDrag(touch_state._ThisTouch.fingerId);
        }
        else if (touch_state._ThisTouch.phase == TouchPhase.Moved)//点击 -> 按住
        {
            if ((_IsPointerBeginOnUI && !_IsAllowUIDrag) || _OneFingerId != touch_state._ThisTouch.fingerId)
            {
                //Common.HobaDebuger.LogWarningFormat("_IsPointerBeginOnUI {0}, {1}", touch_state._ThisTouch.position.x, touch_state._ThisTouch.position.y);
                return;
                // if (EventSystem.current.IsPointerOverGameObject(touch_state._ThisTouch.fingerId)) return;
            }

            float distx = touch_state._ThisTouch.position.x - touch_state._LastTouch.position.x;
            float disty = touch_state._ThisTouch.position.y - touch_state._LastTouch.position.y;

            // #if UNITY_STANDALONE_WIN
            //             float mag = (touch_state._ThisTouch.position - touch_state._LastTouch.position).sqrMagnitude;
            //             if (mag < 2.0f)
            //                 return;
            // #endif

            if (Mathf.Abs(distx) > 0.5f)
            {
                CCamCtrlMan.Instance.DragScreenStart();
                CCamCtrlMan.Instance.ManualAdjustCamera(CCamCtrlMan.ADJUST_STYLE.YAW, distx / 5);
            }

            if (Mathf.Abs(disty) > 0.6f)
            {
                CCamCtrlMan.Instance.DragScreenStart();
                CCamCtrlMan.Instance.ManualAdjustCamera(CCamCtrlMan.ADJUST_STYLE.PITCH, -disty / 6);
            }
        }
        else if (touch_state._ThisTouch.phase == TouchPhase.Ended)//抬起 -> 检测是否属于点击操作
        {
            CCamCtrlMan.Instance.DragScreenEnd();

            //Click Ground
            bool is_pointerOverUI = IsPointerOverUIObject(touch_state._ThisTouch);
            if (is_pointerOverUI || touch_state._HasMoved || _OneFingerId != touch_state._ThisTouch.fingerId)
                return;

            CAM_CTRL_MODE mode = CCamCtrlMan.Instance.GetCurCamCtrlMode();
            HandleClick(touch_state._ThisTouch.position, mode != CAM_CTRL_MODE.EXTERIOR && mode != CAM_CTRL_MODE.NEAR);
        }
    }

    private float DealWithTwoFingers(TouchState touch_state1, TouchState touch_state2)
    {
        //若点击了UI而且UI不允许拖拽，则返回
        if ((IsPointerOverUIObject(touch_state1._ThisTouch) && !IsThisUIAllowedDrag(touch_state1._ThisTouch.fingerId)) ||
            (IsPointerOverUIObject(touch_state2._ThisTouch) && !IsThisUIAllowedDrag(touch_state2._ThisTouch.fingerId)))
            return 0.0f;

        if (IsJoystickDraging())
            return 0.0f;

        if (touch_state1._ThisTouch.phase != TouchPhase.Began && touch_state2._ThisTouch.phase != TouchPhase.Began)
        {
            float distlast = (touch_state2._LastTouch.position - touch_state1._LastTouch.position).magnitude;
            float distthis = (touch_state2._ThisTouch.position - touch_state1._ThisTouch.position).magnitude;

            float fDistanceDelta = distlast - distthis;

            if (distlast - distthis > 10 || distlast - distthis < -10)
            {
                CCamCtrlMan.Instance.ManualAdjustCamera(CCamCtrlMan.ADJUST_STYLE.DISTANCE, fDistanceDelta / 20);

                _isMultiDragingStarted = true;
            }
            return fDistanceDelta;
        }
        return 0.0f;
    }

    public void HandleClick(Vector2 scr_pos, bool can_touch_ground)
    {
        Camera main_camera = Main.Main3DCamera;
        if (main_camera != null)
        {
            Vector3 scr_pos3 = new Vector3(scr_pos.x, scr_pos.y);
            RaycastHit hit_info;
            Ray ray = main_camera.ScreenPointToRay(scr_pos3);

            int ray_mask;
            if (can_touch_ground)
                ray_mask = CUnityUtil.LayerMaskEntity | CUnityUtil.LayerMaskClickable | CUnityUtil.LayerMaskTerrainBuilding;
            else
                ray_mask = CUnityUtil.LayerMaskHostPlayer;
            if (Physics.Raycast(ray, out hit_info, Mathf.Infinity, ray_mask))
            {
                Collider cd = hit_info.collider;
                if (cd == null)
                    return;

                if (cd.gameObject.layer == CUnityUtil.Layer_Terrain)
                {
                    Vector3 pos = hit_info.point;
                    LuaScriptMgr.Instance.CallLuaOnClickGroundFunc(pos);
                }
                else if (cd.gameObject.layer == CUnityUtil.Layer_Building)
                {
                    Vector3 pos = hit_info.point;
                    pos.y = CUnityUtil.GetMapHeight(pos);
                    LuaScriptMgr.Instance.CallLuaOnClickGroundFunc(pos);
                }
                else if (cd.gameObject.layer != CUnityUtil.Layer_Unblockable)
                {
                    ObjectBehaviour oc = cd.gameObject.GetComponentInParent<ObjectBehaviour>();
                    if (oc != null) oc.OnClick();
                }
            }
        }
    }

    #endregion Game Control

    #region Tips Funcs
    public class TipsInfo
    {
        public GameObject Target;
        public UnityAction<GameObject> CallBack;
    }

    public List<TipsInfo> _AllTips = new List<TipsInfo>(8);

    public int FindTipIndex(GameObject g_target)
    {
        for (int i = 0; i < _AllTips.Count; i++)
        {
            if (_AllTips[i].Target == g_target)
            {
                return i;
            }
        }
        return -1;
    }

    public void SweepTips()
    {
        for (int i = 0; i < _AllTips.Count; i++)
        {
            if (_AllTips[i].Target == null)
            {
                _AllTips.RemoveAt(i);
                i -= 1;
            }
        }
    }

    private void HandleClickOnTips(GameObject g_clicked)
    {
        for (int i = 0; i < _AllTips.Count; i++)
        {
            if (_AllTips[i].Target != null)
            {
                _AllTips[i].CallBack(g_clicked);
            }
        }
    }

    public void RegisterTip(GameObject g_target)
    {
        if (g_target != null)
        {
            SweepTips();

            UIEventListener ui_el = g_target.GetComponent<UIEventListener>();
            if (ui_el != null)
            {
                if (FindTipIndex(g_target) == -1)
                {
                    _AllTips.Add(new TipsInfo() { Target = g_target, CallBack = ui_el.HandleClick4Tip });
                }
            }
        }
    }

    public void UnregisterTip(GameObject g_target)
    {
        if (g_target)
        {
            int i_pos = FindTipIndex(g_target);
            if (i_pos != -1)
            {
                _AllTips.RemoveAt(i_pos);
            }
        }
    }

    #endregion Tips Funcs


}