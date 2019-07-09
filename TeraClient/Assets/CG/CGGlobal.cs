using UnityEngine;
using System;

public class CGGlobal : MonoBehaviour
{
    public enum CameraType
    {
        CGCamera,
        GameCamera                      //该模式下只允许一个摄像机，其余摄像机自动删除
    }
    //游戏内摄像机模式
    public CameraType cameraType = CameraType.CGCamera;
    
    //天气效果ID
    public int WeatherId = 0;

    //是否使用UI摄像机
    public bool UseUICamera = false;

    private Camera _CurrentCamera = null;
    public Camera Current
    {
        get
        {
            if (_CurrentCamera == null)
                _CurrentCamera = Main.Main3DCamera;

            return _CurrentCamera;
        }
        set
        {
            _CurrentCamera = value;            
        }
    }
}
