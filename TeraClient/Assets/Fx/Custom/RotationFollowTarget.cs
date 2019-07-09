using UnityEngine;

public class RotationFollowTarget : ReusableFx
{
    private Camera _FollowCamera = null;

    public override void SetActive(bool active)
    {
        if (Main.Main3DCamera != null && Main.Main3DCamera.enabled)
            _FollowCamera = Main.Main3DCamera;
        else
            _FollowCamera = Camera.main != null ? Camera.main : Camera.current;

        base.SetActive(active);
    }


    public override void LateTick(float dt)
    {
        if (_FollowCamera != null)
            transform.rotation = Main.Main3DCamera.transform.rotation;
    }

    void LateUpdate()
    {
        LateTick(0);
    }
}