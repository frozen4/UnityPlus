using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using LuaInterface;
using Common;

public class CNpcSceneDialogCam : CECCamCtrl
{
    public void Start()
    {
        EnableCameraEffect(true);
    }

    public override void OnDisable()
    {
        EnableCameraEffect(false);
    }

    protected void EnableCameraEffect(bool enable)
    {
        EnableDOF(enable);
    }

    protected void EnableDOF(bool enable)
    {
//         float farValue = 0f;
//         if (enable)
//             farValue = (_StageCfg._DistanceLimit.y + DOF_DISTANCE_OFFSET) / Main.Main3DCamera.farClipPlane;

        DynamicEffectManager.Instance.EnableDepthOfField(enable, 0.01f, 0.01f, 1f);
    }

}