using UnityEngine;
using System.Collections;

public class UnityJniWrapper
{
#if UNITY_ANDROID

    static private AndroidJavaObject m_UnityJniWrapper = null;
    static public AndroidJavaObject Instance()
    {
        if (m_UnityJniWrapper == null)
        {
            AndroidJavaClass launcherClass = new AndroidJavaClass("com.meteoritestudio.applauncher.UnityJniWrapper");
            m_UnityJniWrapper = launcherClass.CallStatic<AndroidJavaObject>("Instance");
        }

        return m_UnityJniWrapper;
    }
#endif

}