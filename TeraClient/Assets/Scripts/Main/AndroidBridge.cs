using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class AndroidBridge : MonoBehaviour {
    public void PlatformSDKLoginCallBack(string param)
    {
        LTPlatformBase.ShareInstance().DoLoginCallBack(param);
    }

    public void PlatformSDKLogoutCallBack(string param)
    {
        LTPlatformBase.ShareInstance().DoLogoutCallBack(param);
    }

    public void PlatformSDKExitCallBack(string param)
    {
        LTPlatformBase.ShareInstance().DoExitCallBack(param);
    }
}
