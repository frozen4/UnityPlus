using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GTopPateCam : MonoBehaviour
{
    public Camera mainCamera;
    public Camera thisCamera;

	// Use this for initialization
	void Start ()
    {
        thisCamera = GetComponent<Camera>();
        thisCamera.clearFlags = CameraClearFlags.Nothing;
        thisCamera.cullingMask = LayerMask.GetMask("TopPate");
        thisCamera.useOcclusionCulling = false;
        thisCamera.allowHDR = false;
        thisCamera.allowMSAA = false;
        thisCamera.renderingPath = RenderingPath.Forward;

        //thisCamera.enabled = true;
	}

    void OnPreRender()
    {
        if (mainCamera != null && mainCamera.enabled)
        {
            //this.thisCamera.projectionMatrix = mainCamera.projectionMatrix;

            this.thisCamera.farClipPlane = mainCamera.farClipPlane;
            this.thisCamera.nearClipPlane = mainCamera.nearClipPlane;
            this.thisCamera.fieldOfView = mainCamera.fieldOfView;
        }
    }

    public void SetTarget(Camera target_cam)
    {
        mainCamera = target_cam;
    }
}
