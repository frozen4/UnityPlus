using System;
using UnityEngine;

public static class AndroidUtil
{
    //private static int DesignWidth = 1920;
    //private static int DesignHeight = 1080;

    //²Î¿¼applauncher¹¤³Ì PermissionUtils
    public static int CODE_RECORD_AUDIO = 0;
    public static int CODE_GET_ACCOUNTS = 1;
    public static int CODE_READ_PHONE_STATE = 2;
    public static int CODE_CALL_PHONE = 3;
    public static int CODE_CAMERA = 4;
    public static int CODE_ACCESS_FINE_LOCATION = 5;
    public static int CODE_ACCESS_COARSE_LOCATION = 6;
    public static int CODE_READ_EXTERNAL_STORAGE = 7;
    public static int CODE_WRITE_EXTERNAL_STORAGE = 8;

    public static long CopyAssetFileToPath(String relativeFileName, String sourceDir, String destDir)
    {
#if UNITY_ANDROID
        long bytesCopy = -1;
        AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject currentActitivy = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

        AndroidJavaClass CopyAssetDirectory = new AndroidJavaClass("com.meteoritestudio.applauncher.AndroidWrapper");

        bytesCopy = CopyAssetDirectory.CallStatic<long>("CopyAssetFileToPath",
            currentActitivy,
            relativeFileName,
            sourceDir,
            destDir);
        return bytesCopy;
#else
        return -1;
#endif
    }

    public static bool HasNotchInScreen()
    {
        AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject currentActitivy = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

        AndroidJavaClass NotchScreen = new AndroidJavaClass("com.meteoritestudio.applauncher.Screen");

        bool bRet = NotchScreen.CallStatic<bool>("HasNotchInScreen",
            currentActitivy);

        return bRet;
    }

    public static bool IgnoreNotchScreen()
    {
        AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject currentActitivy = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

        AndroidJavaClass NotchScreen = new AndroidJavaClass("com.meteoritestudio.applauncher.Screen");

        bool bRet = NotchScreen.CallStatic<bool>("IgnoreNotchScreen",
            currentActitivy);

        return bRet;
    }

    public static float GetDensity()
    {
        AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject currentActitivy = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

        AndroidJavaClass NotchScreen = new AndroidJavaClass("com.meteoritestudio.applauncher.Screen");

        float fRet = NotchScreen.CallStatic<float>("GetDensity",
            currentActitivy);

        return fRet;
    }

    public static float GetDpi()
    {
        AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject currentActitivy = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

        AndroidJavaClass NotchScreen = new AndroidJavaClass("com.meteoritestudio.applauncher.Screen");

        float fRet = NotchScreen.CallStatic<float>("GetDpi",
            currentActitivy);

        return fRet;
    }

    public static float GetDpiX()
    {
        AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject currentActitivy = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

        AndroidJavaClass NotchScreen = new AndroidJavaClass("com.meteoritestudio.applauncher.Screen");

        float fRet = NotchScreen.CallStatic<float>("GetDpiX",
            currentActitivy);

        return fRet;
    }

    public static float GetDpiY()
    {
        AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject currentActitivy = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

        AndroidJavaClass NotchScreen = new AndroidJavaClass("com.meteoritestudio.applauncher.Screen");

        float fRet = NotchScreen.CallStatic<float>("GetDpiY",
            currentActitivy);

        return fRet;
    }

    public static float GetWidth()
    {
        AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject currentActitivy = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

        AndroidJavaClass NotchScreen = new AndroidJavaClass("com.meteoritestudio.applauncher.Screen");

        float fRet = NotchScreen.CallStatic<float>("GetWidth",
            currentActitivy);

        return fRet;
    }

    public static float GetHeight()
    {
        AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject currentActitivy = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

        AndroidJavaClass NotchScreen = new AndroidJavaClass("com.meteoritestudio.applauncher.Screen");

        float fRet = NotchScreen.CallStatic<float>("GetHeight",
            currentActitivy);

        return fRet;
    }

    public static void OpenUrl(string url)
    {
        AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject currentActitivy = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

        AndroidJavaClass AndroidWrapper = new AndroidJavaClass("com.meteoritestudio.applauncher.AndroidWrapper");
        AndroidWrapper.CallStatic("OpenUrl", currentActitivy, url);
    }

    public static void CopyTextToClipboard(string str)
    {
        AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject currentActitivy = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

        AndroidJavaClass Clipboard = new AndroidJavaClass("com.meteoritestudio.applauncher.Clipboard");
        Clipboard.CallStatic("CopyTextToClipboard", currentActitivy, str);
    }

    public static string GetOpenUDID()
    {
        AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject currentActitivy = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

        AndroidJavaClass OpenUDID = new AndroidJavaClass("com.meteoritestudio.applauncher.OpenUDID");

        string udid = OpenUDID.CallStatic<string>("GetOpenUDID",
            currentActitivy);

        return udid;
    }

    public static string GetSysVersion()
    {
        string os = UnityEngine.SystemInfo.operatingSystem;
        int idx = os.IndexOf('/');
        if (idx >= 0)
            os = os.Substring(0, idx);
        os = os.Trim(new char[] { ' ' });
        return os;
    }

    public static string GetMACString()
    {
        AndroidJavaClass PermissionsUtils = new AndroidJavaClass("com.meteoritestudio.prom1.MainActivity");
        string macString = PermissionsUtils.CallStatic<string>("getMAC");
        macString = macString.ToLower();
        macString = macString.Replace("-", "");
        macString = macString.Replace(":", "");
        return macString;
    }

    public static bool OpenCamera()
    {
        AndroidJavaClass mainActivity = new AndroidJavaClass("com.meteoritestudio.prom1.MainActivity");
        return mainActivity.CallStatic<bool>("TakeCamera");
    }

    public static bool OpenPhoto()
    {
        AndroidJavaClass mainActivity = new AndroidJavaClass("com.meteoritestudio.prom1.MainActivity");
        return mainActivity.CallStatic<bool>("TakePhoto");
    }

    public static bool SavePhoto(string path)
    {
        AndroidJavaClass mainActivity = new AndroidJavaClass("com.meteoritestudio.prom1.MainActivity");
        return mainActivity.CallStatic<bool>("SavePhoto", path);
    }

    public static void DoRestartImmediate()
    {
        AndroidJavaClass mainActivity = new AndroidJavaClass("com.meteoritestudio.prom1.MainActivity");
        mainActivity.CallStatic("DoRestartImmediate");
    }

    public static void DoRestart(int ms)
    {
        AndroidJavaClass mainActivity = new AndroidJavaClass("com.meteoritestudio.prom1.MainActivity");
        mainActivity.CallStatic("DoRestart", ms);
    }

    public static void RequestPermission(int requestCode)
    {
        AndroidJavaClass mainActivity = new AndroidJavaClass("com.meteoritestudio.prom1.MainActivity");
        mainActivity.CallStatic("requestPermission", requestCode);
    }

    public static bool HasPermission(int requestCode)
    {
        AndroidJavaClass mainActivity = new AndroidJavaClass("com.meteoritestudio.prom1.MainActivity");
        bool hasPermission = mainActivity.CallStatic<bool>("hasPermission", requestCode);
        return hasPermission;
    }

    public static int GetLargeMemoryLimit()
    {
        AndroidJavaClass mainActivity = new AndroidJavaClass("com.meteoritestudio.prom1.MainActivity");
        int limit = mainActivity.CallStatic<int>("GetLargeMemoryLimit");
        return limit;
    }

    public static int GetMemoryLimit()
    {
        AndroidJavaClass mainActivity = new AndroidJavaClass("com.meteoritestudio.prom1.MainActivity");
        int limit = mainActivity.CallStatic<int>("GetMemoryLimit");
        return limit;
    }

    public static int getTotalPss()
    {
        AndroidJavaClass mainActivity = new AndroidJavaClass("com.meteoritestudio.prom1.MainActivity");
        int totalPss = mainActivity.CallStatic<int>("getTotalPss");
        return totalPss;
    }

    public static string getMemotryStats()
    {
        AndroidJavaClass mainActivity = new AndroidJavaClass("com.meteoritestudio.prom1.MainActivity");
        string ret = mainActivity.CallStatic<string>("getMemotryStats");
        return ret;
    }
}