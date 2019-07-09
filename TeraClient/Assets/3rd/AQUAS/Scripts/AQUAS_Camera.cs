// Needs to be attached to Camera to enable depth rendering in forward lighting (required for some platforms)

using UnityEngine;
using System.Collections;

[AddComponentMenu("AQUAS/AQUAS Camera")]
[RequireComponent(typeof(Camera))]
public class AQUAS_Camera : MonoBehaviour {
#if UNITY_EDITOR
	void OnDrawGizmos(){
		Set();
	}
#endif
	void Start () {
	}

	void Set()
    {
        Camera cam = GetComponent<Camera>();
		if(cam && cam.depthTextureMode == DepthTextureMode.None)
			cam.depthTextureMode = DepthTextureMode.Depth;
	}

    void OnEnable()
    {
        Camera cam = GetComponent<Camera>();
        
        if (cam != null)
            cam.depthTextureMode |= DepthTextureMode.Depth;
    }

    void OnDisable()
    {
        Camera cam = GetComponent<Camera>();

        if (cam != null)
            cam.depthTextureMode &= (~DepthTextureMode.Depth);
    }
}
