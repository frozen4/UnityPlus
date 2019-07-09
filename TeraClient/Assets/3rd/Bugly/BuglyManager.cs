#if USING_BUGLY
using UnityEngine;
using System.Collections.Generic;

public class BuglyManager
{
    /// <summary>
    /// Your Bugly App ID. Every app has a special identifier that allows Bugly to associate error monitoring data with your app.
    /// Your App ID can be found on the "Setting" page of the app you are trying to monitor.
    /// </summary>
#if UNITY_IOS || UNITY_IPOINE
    private const string _BuglyAppID = "900026036";   //Bugly苹果项目ID
#elif UNITY_ANDROID
    private const string _BuglyAppID = "702d15cc9e";  //Bugly安卓项目ID
#else
    private const string _BuglyAppID = "";
#endif

    //Bugly PostBuildPaths
    public static readonly string SDKPath = System.IO.Path.Combine(Application.dataPath, "../../SDK/Bugly");
    public static readonly string[] Frameworks =
    {
        "Bugly.framework"
    };

    public static string BuglyAppID
    {
        get { return _BuglyAppID; }
    }

	public static void Initialize ()
	{
        //---- Enable the debug log print，Release下需要设为false
#if DEBUG
        BuglyAgent.ConfigDebugMode(true);
#else
        BuglyAgent.ConfigDebugMode(false);
#endif

        //---- Config default channel, version, user
        //BuglyAgent.ConfigDefault (null, null, null, 0);

        //---- Config auto report log level, default is LogSeverity.LogError, so the LogError, LogException log will auto report
        BuglyAgent.ConfigAutoReportLogLevel (LogSeverity.LogError);

        //---- Config auto quit the application make sure only the first one c# exception log will be report, please don't set TRUE if you do not known what are you doing.
        //BuglyAgent.ConfigAutoQuitApplication (false);

        //---- If you need register Application.RegisterLogCallback(LogCallback), you can replace it with this method to make sure your function is ok.
        //BuglyAgent.RegisterLogCallback (null);

        //---- Init the bugly sdk and enable the c# exception handler.
        BuglyAgent.InitWithAppId (_BuglyAppID);

        //---- TODO Required. If you do not need call 'InitWithAppId(string)' to initialize the sdk(may be you has initialized the sdk it associated Android or iOS project),
        //---- please call this method to enable c# exception handler only.
        //BuglyAgent.EnableExceptionHandler ();

        //---- TODO NOT Required. If you need to report extra data with exception, you can set the extra handler
        //BuglyAgent.SetLogCallbackExtrasHandler (MyLogCallbackExtrasHandler);

        BuglyAgent.PrintLog(LogSeverity.LogInfo, "Bugly init complete, time: {0}", System.DateTime.Now);
        //Debug.Log(HobaString.Format("Bugly init complete, time: {0}, verion: v{1}", System.DateTime.Now, BuglyAgent.PluginVersion));
    }

   // Extra data handler to packet data and report them with exception.
        // Please do not do hard work in this handler 
    private static Dictionary<string, string> MyLogCallbackExtrasHandler ()
    {
        // TODO Test log, please do not copy it
        BuglyAgent.PrintLog (LogSeverity.Log, "extra handler");
        
        // TODO Sample code, please do not copy it
        Dictionary<string, string> extras = new Dictionary<string, string> ();
        extras.Add ("ScreenSolution", HobaText.Format ("{0}x{1}", Screen.width, Screen.height));
        extras.Add ("deviceModel", SystemInfo.deviceModel);
        extras.Add ("deviceName", SystemInfo.deviceName);
        extras.Add ("deviceType", SystemInfo.deviceType.ToString ());
        
        extras.Add ("deviceUId", SystemInfo.deviceUniqueIdentifier);
        extras.Add ("gDId", HobaText.Format ("{0}", SystemInfo.graphicsDeviceID));
        extras.Add ("gDName", SystemInfo.graphicsDeviceName);
        extras.Add ("gDVdr", SystemInfo.graphicsDeviceVendor);
        extras.Add ("gDVer", SystemInfo.graphicsDeviceVersion);
        extras.Add ("gDVdrID", HobaText.Format ("{0}", SystemInfo.graphicsDeviceVendorID));
        
        extras.Add ("graphicsMemorySize", HobaText.Format ("{0}", SystemInfo.graphicsMemorySize));
        extras.Add ("systemMemorySize", HobaText.Format ("{0}", SystemInfo.systemMemorySize));
        extras.Add ("UnityVersion", Application.unityVersion);
        
        BuglyAgent.PrintLog (LogSeverity.LogInfo, "Package extra data");
        return extras;
    }
}

#endif
