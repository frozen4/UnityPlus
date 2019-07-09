using LuaInterface;
using UnityEngine;
using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Collections.Generic;

public static class OSUtility
{
    public static void ShowAlertView(string title, string content)
    {
#if UNITY_IPHONE
        IOSUtil.showAlertView(title, content);
#elif UNITY_ANDROID

#endif
    }

    public static void SetDesignContentScale(int DesignWidth, int DesignHeight)
    {
#if UNITY_IPHONE || UNITY_ANDROID
        if (Screen.currentResolution.width > DesignWidth ||
            Screen.currentResolution.height > DesignHeight)
        {
            int scaleWidth = 0;
            int scaleHeight = 0;

            if (scaleWidth == 0 && scaleHeight == 0)
            {
                int width = Screen.currentResolution.width;
                int height = Screen.currentResolution.height;
                int designWidth = DesignWidth;
                int designHeight = DesignHeight;
                float s1 = (float)designWidth / (float)designHeight;
                float s2 = (float)width / (float)height;
                if (s1 < s2)
                {
                    designWidth = (int)Mathf.FloorToInt(designHeight * s2);
                }
                else if (s1 > s2)
                {
                    designHeight = (int)Mathf.FloorToInt(designWidth / s2);
                }
                float contentScale = (float)designWidth / (float)width;
                if (contentScale < 1.0f)
                {
                    scaleWidth = designWidth;
                    scaleHeight = designHeight;
                }
            }
            if (scaleWidth > 0 && scaleHeight > 0)
            {
                scaleWidth = scaleWidth / 2 * 2;
                scaleHeight = scaleHeight / 2 * 2;

                Screen.SetResolution(scaleWidth, scaleHeight, true);
                DeviceLogger.Instance.WriteLog(string.Format("SetDesignContentScale Screen SetResolution: {0} {1} From Screen Size: {2} {3}...",
                    scaleWidth, scaleHeight, Screen.width, Screen.height));
            }
        }
#endif
    }

    public static uint GetMilliSecond()
    {
        return LuaDLL.HOBA_GetMilliSecond();
    }

    //麦克风是否可用
    public static bool IsMicrophoneAvailable()
    {
#if UNITY_STANDALONE_WIN
        return Microphone.devices != null && Microphone.devices.Length > 0;
#else
        return false;
#endif
    }

    public static void OpenUrl(string url)
    {
#if UNITY_IOS
        IOSUtil.iosOpenUrl(url);
#elif UNITY_ANDROID
        AndroidUtil.OpenUrl(url);
#endif
    }

    public static void SetAudioAmbient()
    {  
#if UNITY_IOS
        IOSUtil.setAVAudioSessionCategory(0);
#endif
    }

    // 复制到粘贴板
    public static void CopyTextToClipboard(string str)
    {
#if UNITY_IOS
        IOSUtil.iosCopyTextToClipboard(str);
#elif UNITY_ANDROID
        AndroidUtil.CopyTextToClipboard(str);
#else
        TextEditor t = new TextEditor();
        t.text = str;
        t.OnFocus();
        t.Copy();
#endif
    }

    // UDID
    public static string GetOpenUDID()
    {
        string udid = string.Empty;
#if UNITY_IOS
        udid = IOSUtil.iosGetOpenUDID();
#elif UNITY_ANDROID
        udid = AndroidUtil.GetOpenUDID();
#endif
        if (string.IsNullOrEmpty(udid))
            udid = SystemInfo.deviceUniqueIdentifier;

        return udid;
    }

    /// 网络可用
    public static bool IsNetAvailable()
    {
        return Application.internetReachability != NetworkReachability.NotReachable;
    }

    /// 是否是无线
    public static bool IsWifi()
    {
        return Application.internetReachability == NetworkReachability.ReachableViaLocalAreaNetwork;
    }

    //是否有sd卡
    public static bool IsHaveSDCard()
    {
#if UNITY_ANDROID
        var jc = new AndroidJavaClass("android.os.Environment");
        if(jc.CallStatic<string>("getExternalStorageState") != "mounted")
            return false;

        return true;
#else
        return false;
#endif
    }

    //磁盘剩余空间 (byte)
    public static long GetFreeDiskSpace()
    {
#if UNITY_ANDROID
        var jc = new AndroidJavaClass("android.os.Environment");
        if(jc.CallStatic<string>("getExternalStorageState") != "mounted")
            return 0;

        var file = jc.CallStatic<AndroidJavaObject>("getExternalStorageDirectory");
        var path = file.Call<string>("getPath");

        var stat = new AndroidJavaObject("android.os.StatFs", path);
        var blocks = stat.Call<long>("getAvailableBlocksLong");
        var blockSize = stat.Call<long>("getBlockSizeLong");

        var freeSize = blocks * blockSize;
        return (long)freeSize;
#else
        return (long)LuaDLL.HOBA_GetFreeDiskSpace();
#endif
    }

    public static long GetVirtualMemoryUsedSize()
    {
#if UNITY_ANDROID
        var jc = new AndroidJavaClass("java.lang.Runtime");
        var runtime = jc.CallStatic<AndroidJavaObject>("getRuntime");
        long totalMem = runtime.Call<long>("totalMemory");
        return totalMem;
#else
        return (long)LuaDLL.HOBA_GetVirtualMemoryUsedSize();
#endif
    }

    public static long GetPhysMemoryUsedSize()
    {
#if UNITY_ANDROID
        var jc = new AndroidJavaClass("java.lang.Runtime");
        var runtime = jc.CallStatic<AndroidJavaObject>("getRuntime");
        long totalMem = runtime.Call<long>("totalMemory");
        long freeMem = runtime.Call<long>("freeMemory");
        return totalMem - freeMem;
#else
        return (long)LuaDLL.HOBA_GetPhysMemoryUsedSize();
#endif
    }

    public static string GetResponseDeviceString()
    {
#if UNITY_IOS
        return UnityEngine.SystemInfo.deviceModel; //IOSUtil.iosGetSysModel();
#elif UNITY_ANDROID
        return UnityEngine.SystemInfo.deviceModel;
#else
        return string.Empty;
#endif
    }

    public static string GetResponseOSVersionString()
    {
#if UNITY_IOS
        return UnityEngine.SystemInfo.operatingSystem; //IOSUtil.iosGetSysVersion();
#elif UNITY_ANDROID
        return AndroidUtil.GetSysVersion();
#else
        return string.Empty;
#endif
    }

    public static string GetResponseMACString()
    {
#if UNITY_IOS
        return UnityEngine.SystemInfo.deviceUniqueIdentifier;
        //return IOSUtil.iosGetOpenUDID();
#elif UNITY_ANDROID
        return AndroidUtil.GetMACString();
#else
        return string.Empty;
        
#endif
    }

    public static int GetLargeMemoryLimit()
    {
#if UNITY_ANDROID
        return AndroidUtil.GetLargeMemoryLimit();
#else
        return 0;
#endif
    }

    public static int GetMemoryLimit()
    {
#if UNITY_ANDROID
        return AndroidUtil.GetMemoryLimit();
#else
        return 0;
#endif
    }

    public static int getTotalPss()
    {
#if UNITY_ANDROID
        return AndroidUtil.getTotalPss();
#else
        return 0;
#endif
    }

    public static string getMemotryStats()
    {
#if UNITY_ANDROID
        return AndroidUtil.getMemotryStats();
#else
        long vm = GetVirtualMemoryUsedSize() / (1024 * 1024);
        long pm = GetPhysMemoryUsedSize() / (1024 * 1024);
        return string.Format("Virtual MEM: {0}M, Physics MEM: {1}M", vm, pm);
#endif
    }

    public static string GetSystemLanguageCode()
    {
        UnityEngine.SystemLanguage lan = Application.systemLanguage;

#if UNITY_IOS || UNITY_IPHONE
        //fix IOS9 以后的bug
        //http://blog.csdn.net/teng_ontheway/article/details/50277169
        if (lan == SystemLanguage.Chinese)
        {
            string name = LuaDLL.HOBA_IOSGetCurLanguageString();
            if (name.StartsWith("zh-Hans"))
                lan = SystemLanguage.ChineseSimplified;
            else
                lan = SystemLanguage.ChineseTraditional;
        }
#endif

        switch (lan)
        {
            case SystemLanguage.Chinese:
                return "CN";
            case SystemLanguage.ChineseSimplified:
                return "CN";

             case SystemLanguage.ChineseTraditional:
                return "CN";

            case SystemLanguage.Korean:
                return "KR";

             case SystemLanguage.English:
                return "KR";

            default:            //default cn
                return "KR";
        }
    }


    public static string Post(string url, Dictionary<string, string> dic)
    {
        string result = "";
        HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
        req.Method = "POST";
        req.ContentType = "application/x-www-form-urlencoded";

#region 添加Post 参数
        StringBuilder builder = new StringBuilder();
        int i = 0;
        foreach (var item in dic)
        {
            if (i > 0)
                builder.Append("&");
            builder.AppendFormat("{0}={1}", item.Key, item.Value);
            i++;
        }
        byte[] data = Encoding.UTF8.GetBytes(builder.ToString());
        req.ContentLength = data.Length;
        using (Stream reqStream = req.GetRequestStream())
        {
            reqStream.Write(data, 0, data.Length);
            reqStream.Close();
        }
#endregion

        HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
        Stream stream = resp.GetResponseStream();

        using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
        {
            result = reader.ReadToEnd();
        }

        return result;
    }

    public static bool HasRecordAudioPermission()
    {
#if UNITY_IOS
        return IOSUtil.HasPermission((int)IOSUtil.PermissionType.PermissionMicrophone);
#elif UNITY_ANDROID
        return AndroidUtil.HasPermission(AndroidUtil.CODE_RECORD_AUDIO);
#else
        return true;
#endif
    }

    public static void RequestRecordAudioPermission()
    {
#if UNITY_IOS
        IOSUtil.RequestPermission((int)IOSUtil.PermissionType.PermissionMicrophone);
#elif UNITY_ANDROID
        AndroidUtil.RequestPermission(AndroidUtil.CODE_RECORD_AUDIO);
#else

#endif
    }

    public static bool HasCameraPermission()
    {
#if UNITY_IOS
        return IOSUtil.HasPermission((int)IOSUtil.PermissionType.PermissionCamera);
#elif UNITY_ANDROID
        return AndroidUtil.HasPermission(AndroidUtil.CODE_CAMERA);
#else
        return true;
#endif
    }

    public static void RequestCameraPermission()
    {
#if UNITY_IOS
        IOSUtil.RequestPermission((int)IOSUtil.PermissionType.PermissionCamera);
#elif UNITY_ANDROID
        AndroidUtil.RequestPermission(AndroidUtil.CODE_CAMERA);
#else
        
#endif
    }

    public static bool HasPhotoPermission()
    {
#if UNITY_IOS
        return IOSUtil.HasPermission((int)IOSUtil.PermissionType.PermissionPhotoLibrary);
#elif UNITY_ANDROID
        return AndroidUtil.HasPermission(AndroidUtil.CODE_WRITE_EXTERNAL_STORAGE);
#else
        return true;
#endif
    }

    public static void RequestPhotoPermission()
    {
#if UNITY_IOS
        IOSUtil.RequestPermission((int)IOSUtil.PermissionType.PermissionPhotoLibrary);
#elif UNITY_ANDROID
        AndroidUtil.RequestPermission(AndroidUtil.CODE_WRITE_EXTERNAL_STORAGE);
#else

#endif
    }

    public static bool DNSResolve(int nTryTimes, string strHostName, out string strUrl)
    {
        strUrl = string.Empty;
        if (string.IsNullOrEmpty(strHostName))
            return false;

        IPAddress addr;
        if (IPAddress.TryParse(strHostName, out addr) && addr.AddressFamily ==  AddressFamily.InterNetwork)
        {
            strUrl = addr.ToString();
            return true;
        }

        bool bDNSResolved = false;
        int times = nTryTimes;
        do
        {
            --times;
            string ipStr = null;
            try
            {
                IPAddress[] ipAddresses = Dns.GetHostAddresses(strHostName);
                if (ipAddresses != null && ipAddresses.Length > 0)
                {
                    for (int i = 0; i < ipAddresses.Length; ++i)
                    {
                        if (ipAddresses[i].AddressFamily == AddressFamily.InterNetwork)
                        {
                            ipStr = ipAddresses[i].ToString();
                            break;
                        }
                    }

                    if (ipStr != null)
                    {
                        strUrl = ipStr;
                        bDNSResolved = true;
                    }
                }
            }
            catch (Exception e)
            {
                bDNSResolved = false;
                break;
            }

            if (bDNSResolved)
            {
                break;
            }

        } while (times > 0);

        return bDNSResolved;
    }
}