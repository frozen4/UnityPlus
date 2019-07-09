using UnityEngine;
using System.Collections;

public class PPChainHolder : MonoBehaviour
{
    public PostProcessChain PostProcessChain;

    void Start()
    {
        EnablePostProcessChain();
    }

    // TODO: 整合进动态效果管理器  added by lijian

    public void EnablePostProcessChain()
    {
        //Debug.LogError("EnablePostProcessChain " + gameObject.name);

        if (PostProcessChain != null)
        {
            PostProcessChain.enabled = true;
            PostProcessChain.EnableBloomHD = true;
            PostProcessChain.EnableHsvAdjust = true;
        }
    }
}
