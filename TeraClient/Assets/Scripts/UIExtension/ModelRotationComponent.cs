using UnityEngine;

public class ModelRotationComponent : MonoBehaviour 
{
    [SerializeField]
    private float speed = 10f;
    private Vector3 _RotationCenter = Vector3.up;

    private Camera _RoleCam;
    public Camera RoleCam
    {
        get
        {
            if (_RoleCam == null)
            {
                if (Camera.allCameras != null && Camera.allCameras.Length > 1)
                {
                    foreach (var cam in Camera.allCameras)
                    {
                        if (cam.gameObject.layer != CUnityUtil.Layer_UI)
                        {
                            _RoleCam = cam;
                            break;
                        }
                    }
                }
            }
            return _RoleCam;
        }
    }

    void Start()
    {
        var collider = GetComponent<CapsuleCollider>();
		if(collider == null)
		{
            collider = gameObject.AddComponent<CapsuleCollider>();
            collider.center = Vector3.up;
            collider.height = 2.05f;
        }
        //美术会设置旋转中轴偏移值
        _RotationCenter.x = collider.center.x;
        _RotationCenter.z = collider.center.z;
    }

    private bool _IsPointerBeginOnUI = false;
    private bool _IsPointerBeginOnThis = false;
    private int _OneFingerId = 0;
    void LateUpdate()
    {
        if (RoleCam == null) return;

        var inputMan = InputManager.Instance;
        if (inputMan == null) return;

        if (inputMan.TouchCount != 1) return;

        var touchState = inputMan.TouchStates[0];
        if (touchState == null) return;

        if (touchState._ThisTouch.phase == TouchPhase.Began)//点击或者触摸
        {
            _IsPointerBeginOnUI = inputMan.IsPointerOverUIObject(touchState._ThisTouch);
            _OneFingerId = touchState._ThisTouch.fingerId;

            RaycastHit hitInfo;
            Ray ray = RoleCam.ScreenPointToRay(new Vector3(touchState._ThisTouch.position.x, touchState._ThisTouch.position.y));
            if (Physics.Raycast(ray, out hitInfo))
            {
                if (hitInfo.collider != null && hitInfo.collider.gameObject == this.gameObject)
                    _IsPointerBeginOnThis = true;
                else
                    _IsPointerBeginOnThis = false;
            }
            else
                _IsPointerBeginOnThis = false;
        }
        else if (touchState._ThisTouch.phase == TouchPhase.Moved)//点击 -> 按住
        {
            if (!_IsPointerBeginOnThis || _IsPointerBeginOnUI || _OneFingerId != touchState._ThisTouch.fingerId) return;

            float distx = touchState._ThisTouch.position.x - touchState._LastTouch.position.x;
            if (Mathf.Abs(distx) > 0.5f)
                transform.Rotate(_RotationCenter, -distx / 10 * speed);
        }
    }
    
}
