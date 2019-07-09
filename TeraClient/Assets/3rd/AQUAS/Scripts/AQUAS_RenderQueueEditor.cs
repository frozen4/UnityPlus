using UnityEngine;
using System.Collections;

//#if GAIA_PRESENT && UNITY_EDITOR


//[ExecuteInEditMode]
[AddComponentMenu("AQUAS/Render Queue Controller")]
public class AQUAS_RenderQueueEditor : MonoBehaviour
{
#if UNITY_EDITOR && ART_USE
    public int renderQueueIndex = -1;
    void Update()
    {
        gameObject.GetComponent<Renderer>().sharedMaterial.renderQueue = renderQueueIndex;
    }
#endif
}
