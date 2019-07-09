#if USING_FABRIC
using UnityEngine;
using System;
using System.Runtime.InteropServices;

public class FabricManager {
    //Fabric Configs
    public static readonly string ApiKey = "2917662e2b1bc3112d038c0c337bc2c9462613c7";
    public static readonly string BuildSecret = "c9a7ae415d80481b0d0fbce1b942491a6f906f3dcafdd08cf6d27f2bc58127f4";
    public static readonly string Initialization = "Manual";
    public static readonly string CrashlyticsKit = "Fabric.Internal.Crashlytics.CrashlyticsInit.RegisterExceptionHandlers";
    public static readonly System.Version Version = new System.Version("1.2.8");

    //Fabric PostBuildPaths
    public static readonly string SDKPath = System.IO.Path.Combine(UnityEngine.Application.dataPath, "../../SDK/Fabric/Frameworks");
    public static readonly string[] Frameworks =
    {
        "Crashlytics.framework",
        "Fabric.framework"
    };

#if UNITY_IOS
	[DllImport("__Internal")]
	private static extern bool CLUIsInitialized();
#endif

    public static string UserId { get; set; }
    public static string RoleInfo { get; set; }

    private FabricManager() { }

    public static void Initialize()
    {
        try
        {
            Common.HobaDebuger.LogWarning("[Fabric]FabricManager Initialize");
            Fabric.Runtime.Fabric.Initialize();

            RegisterExceptionHandlers();
        }
        catch (Exception e)
        {
            Common.HobaDebuger.LogWarningFormat("[Fabric]FabricManager Initialize failed, exception:{0}", e.ToString());
        }
    }

    public static void SetUserId(string id)
    {
        if (!IsSDKInitialized()) return;

        Common.HobaDebuger.LogWarning("[Fabric]SetUserIdentifier: " + id);
        Fabric.Crashlytics.Crashlytics.SetUserIdentifier(id);
        UserId = id;
    }

    public static void SetRoleInfo(string info)
    {
        if (!IsSDKInitialized()) return;

        Common.HobaDebuger.LogWarning("[Fabric]SetRoleInfo: " + info);
        RoleInfo = info;
    }

    //清楚缓存数据
    public static void Reset()
    {
        UserId = null;
        RoleInfo = null;
    }

    private static void RegisterExceptionHandlers()
    {
        if (IsSDKInitialized())
        {
            Common.HobaDebuger.LogWarning("[Fabric]Registering exception handlers");

            AppDomain.CurrentDomain.UnhandledException += HandleException;
            
            Application.logMessageReceived += HandleLog;
        }
        else
        {
            Common.HobaDebuger.LogWarning("[Fabric]Did not register exception handlers: Crashlytics SDK was not initialized");
        }
    }

    private static void HandleException(object sender, UnhandledExceptionEventArgs eArgs)
    {
        Exception e = (Exception)eArgs.ExceptionObject;
        HandleLog(e.Message.ToString(), e.StackTrace.ToString(), LogType.Exception);
    }

    private static void HandleLog(string message, string stackTraceString, LogType type)
    {
        if (type == LogType.Exception || type == LogType.Error || type == LogType.Warning)
            Fabric.Crashlytics.Crashlytics.Log(string.Format("Time: {0}\n{1}\n{2}", System.DateTime.Now.ToString(), message, stackTraceString));
        if (type == LogType.Exception || type == LogType.Error)
        {
            Fabric.Crashlytics.Crashlytics.SetKeyValue("Package", CPlatformConfig.GetLocale());
            Fabric.Crashlytics.Crashlytics.SetKeyValue("Version", EntryPoint.Instance.CurrentVersion);

            if (!string.IsNullOrEmpty(UserId))
                Fabric.Crashlytics.Crashlytics.SetKeyValue("UserId", UserId);

            if (!string.IsNullOrEmpty(RoleInfo))
                Fabric.Crashlytics.Crashlytics.SetKeyValue("RoleInfo", RoleInfo);

            Fabric.Crashlytics.Crashlytics.RecordCustomException(type.ToString(), message, stackTraceString);
        }
    }

    private static bool IsSDKInitialized()
    {
#if UNITY_IOS
			return CLUIsInitialized ();
#elif UNITY_ANDROID
			AndroidJavaObject crashlyticsInstance = null;
			try {
			    var crashlyticsClass = new AndroidJavaClass("com.crashlytics.android.Crashlytics");
				crashlyticsInstance = crashlyticsClass.CallStatic<AndroidJavaObject>("getInstance");
			}
			catch {
				crashlyticsInstance = null;
			}
			return crashlyticsInstance != null;
#else
        return false;
#endif
    }
}
#endif
