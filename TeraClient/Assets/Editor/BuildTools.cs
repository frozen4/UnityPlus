using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;

#pragma warning disable 0414

#if UNITY_IOS
//using UnityEditor.iOS.Xcode;
using UnityEditor.iOS.Xcode.Custom;
#endif

using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using System.Text;
using System.Diagnostics;
using System.Text.RegularExpressions;
public class BuildTools
{
    public static readonly string TopLevelManifestFilePath = Path.Combine(Application.dataPath, "Plugins/Android/AndroidManifest.xml");
    public static readonly string FrameworkXcodeRoot = "Frameworks"; //Xcode目录下的路径

    public static string GetXcodeFrameworkFullPath(string buildPath)
    {
        return Path.Combine(buildPath, FrameworkXcodeRoot);
    }

    public static string GetXcodeEntitlementsRoot(string buildPath)
    {
        return Path.Combine(buildPath, "Unity-iPhone");
    }

    //设置平台宏定义
    private static void SetScriptingDefineSymbols()
    {
        string[] commands = System.Environment.GetCommandLineArgs();
        string macroDefinition = commands[commands.Length - 1];

        BuildTarget activeTarget = EditorUserBuildSettings.activeBuildTarget;
        BuildTargetGroup curTarget;
        if (activeTarget == BuildTarget.Android)
        {
            curTarget = BuildTargetGroup.Android;
        }
        else if (activeTarget == BuildTarget.iOS)
        {
            curTarget = BuildTargetGroup.iOS;
        }
        else
        {
            DeviceLogger.Instance.WriteLog("SetScriptingDefineSymbols::Current PlatformType is Not a Mobile Type! Can not supported");
            return;
        }

        PlayerSettings.SetScriptingDefineSymbolsForGroup(curTarget, macroDefinition);
    }
    
    [MenuItem("Tools/本地Build前必点", false, 30)]
    private static void LocalBuild()
    {
        PrebuildClient();
        ResetKeystore();

        AssetDatabase.Refresh();
    }

    //设置宏定义完成后，修改配置文件等操作,在此函数块内完成。
    private static void PrebuildClient()
    {
        //默认bid 执行一次
        //PlayerSettings.applicationIdentifier = "com.kakaogames.tera.beta2";
        //Kakao的Prebuild会替换Manifeset文件，要首先执行
#if PLATFORM_KAKAO
        DeviceLogger.Instance.WriteLog("PreBuild Kakao start");
        KakaoPreBuild.Start();
#else
        DeviceLogger.Instance.WriteLog("PreBuild Kakao disable");
        KakaoPreBuild.Disable();
#endif

#if PLATFORM_LONGTU
        DeviceLogger.Instance.WriteLog("PreBuild Longtu start");
        LongtuPreBuild.Start();
#else
        DeviceLogger.Instance.WriteLog("PreBuild Longtu disable");
        LongtuPreBuild.Disable();
#endif

#if USING_FABRIC
        DeviceLogger.Instance.WriteLog("PreBuild Fabric start");
        FabricPreBuild.Start();
#endif

        DeviceLogger.Instance.WriteLog("PrebuildClient complete");

        UpdateKakaoConfig();
    }


    public static bool ReplaceAppVersion(string fileName, string appVersion)
    {
        string[] lines = null;
        try
        {
            lines = File.ReadAllLines(fileName);
        }
        catch (Exception e)
        {
            DeviceLogger.Instance.WriteLog("[ERROR]" + e.Message);
            return false;
        }

        if (lines == null)
            return false;

        for (int i = 0; i < lines.Length; ++i)
        {
            string strLine = lines[i];
            string key;
            string val;
            ParseTranslateValue_Keyword(strLine, "key", out key);
            if (key == "appVersion")
            {
                ParseTranslateValue_Keyword(strLine, "value", out val);
                string s0 = string.Format("\"{0}\"", val);
                string s1 = string.Format("\"{0}\"", appVersion);
                lines[i] = strLine.Replace(s0, s1);
                break;
            }
        }

        try
        {
            StreamWriter sw = new StreamWriter(fileName, false);
            for (int i = 0; i < lines.Length; ++i)
            {
                sw.Write(lines[i]);

                if (i + 1 != lines.Length)
                    sw.Write("\n");
            }

            sw.Flush();
            sw.Close();
        }
        catch (Exception e)
        {
            DeviceLogger.Instance.WriteLog("[ERROR]" + e.Message);
            return false;
        }
        return true;
    }

    private static void ParseTranslateValue_Keyword(string strLine, string keyword, out string val)
    {
        val = "";

        int nNameStr = strLine.IndexOf(keyword);
        if (nNameStr < 0)
            return;

        //Name = 前面不能是其他单词的一部分
        if (nNameStr > 0)
        {
            if (Char.IsLetter(strLine, nNameStr - 1) || Char.IsDigit(strLine, nNameStr - 1))
                return;
        }

        int nStart = IndexOfString(strLine, nNameStr);
        if (nStart < 0)
            return;

        int nEnd = IndexOfString(strLine, nStart + 1);
        if (nStart >= 0 && nEnd >= 0 && nEnd > nStart)
        {
            ++nStart;
            val = strLine.Substring(nStart, nEnd - nStart);
        }
    }

    private static int IndexOfString(string strLine, int nStart)
    {
        return strLine.IndexOf('\"', nStart);
    }

    //设置基础版本号
    private static void SetClientBaseVersion()
    {
        string[] commands = System.Environment.GetCommandLineArgs();
        string version = commands[commands.Length - 1];         // 版本号

        DeviceLogger.Instance.WriteLog("SetClientBaseVersion: " + version);

        // Version-Code & Version-String
        {
            try
            {
                string[] strArray = version.Split('.');
                int headVer = int.Parse(strArray[0]);
                int midVer = int.Parse(strArray[1]);
                int versionCode = headVer * 10000 + midVer;

                PlayerSettings.bundleVersion = version;
#if UNITY_ANDROID
                PlayerSettings.Android.bundleVersionCode = versionCode;
#elif UNITY_IOS
                PlayerSettings.iOS.buildNumber = versionCode.ToString();
#endif
            }
            catch (Exception e) {
                DeviceLogger.Instance.WriteLog("SetClientBaseVersion Exception1: " + e.Message);
            }
        }

        //FIXME with patch instance path
        {
            string strPath = Application.dataPath + "/Resources/BaseVersion.txt";
            try
            {
                StreamWriter sw = new StreamWriter(strPath, false);
                sw.WriteLine(version);

                sw.Flush();
                sw.Close();
            }
            catch (Exception e) {
                DeviceLogger.Instance.WriteLog("SetClientBaseVersion Exception2: " + e.Message);
            }
        }

        UpdateKakaoConfig();
    }

    private static void UpdateKakaoConfig()
    {
#if PLATFORM_KAKAO
        DeviceLogger.Instance.WriteLogFormat("UpdateKakaoConfig AppVersion: {0}", PlayerSettings.bundleVersion);
#else
        DeviceLogger.Instance.WriteLogFormat("UpdateKakaoConfig AppVersion: {0} Skip...", PlayerSettings.bundleVersion);
#endif

        //Write kakao_game_sdk_configuration
#if PLATFORM_KAKAO
#if UNITY_IOS
                   // Kakaogame.SDK.Editor.KGSharedData.Configuration.appVersion = PlayerSettings.bundleVersion;
#elif UNITY_ANDROID
                    string strPath = Application.dataPath + "/Plugins/Android/assets/kakao_game_sdk_configuration.xml";
                    if (ReplaceAppVersion(strPath, PlayerSettings.bundleVersion))
                    {
                        DeviceLogger.Instance.WriteLog("ReplaceAppVersion to: " + PlayerSettings.bundleVersion);
                    }
                    else
                    {
                        DeviceLogger.Instance.WriteLog("ReplaceAppVersion Failed!");
                    }
#endif
#endif
    }

    //设置渠道版本 关联更新服务器地址
    private static void SetComplatform()
    {
        string[] commands = System.Environment.GetCommandLineArgs();
        string complatform = commands[commands.Length - 1];         // 渠道名称 cn dev ...

        //FIXME with patch instance path
        {
            string strPath = Application.dataPath + "/Resources/Locale.txt";
            try
            {
                StreamWriter sw = new StreamWriter(strPath, false);
                sw.WriteLine(complatform);

                sw.Flush();
                sw.Close();
            }
            catch (Exception e) { }
        }
    }

    /*********************************************************************
    *                               切换平台
    **********************************************************************/
    private static void SwitchAndroid()
    {
#if !UNITY_ANDROID
        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTarget.Android);
#endif
    }

    private static void SwitchIos()
    {
#if !UNITY_IPHONE
        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTarget.iOS);
#endif
    }

    private static void SwitchWindows()
    {
#if !UNITY_STANDALONE_WIN
        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTarget.StandaloneWindows64);
#endif
    }

    /*********************************************************************
    *                           IOS 修改 Graphic Setting
    **********************************************************************/
    private static void FixGraphicSetting_iOS()
    {
        // string strSettingPath = Application.dataPath + "/../ProjectSettings/GraphicsSettings.asset";
        // DeviceLogger.Instance.WriteLog("GraphicsSettings : " + strSettingPath);

        // StreamReader sr = new StreamReader(strSettingPath);
        // List<string> list = new List<string>();
        // string str = "";
        // string strKey = "m_ScreenSpaceShadows: ([0-9])";
        // while ((str = sr.ReadLine()) != null)
        // {
        //     Regex regKey = new Regex(strKey);
        //     if (regKey.IsMatch(str))
        //     {
        //         Regex reg = new Regex("([0-9])");
        //         str = reg.Replace(str, "0");
        //     }

        //     list.Add(str);
        // }
        // sr.Close();

        // try
        // {
        //     StreamWriter sw = new StreamWriter(strSettingPath, false);

        //     foreach (var outLine in list)
        //     {
        //         sw.WriteLine(outLine);
        //     }

        //     sw.Flush();
        //     sw.Close();
        // }
        // catch (Exception e) { }

        // AssetDatabase.Refresh();

        DeviceLogger.Instance.WriteLog("FixGraphicSetting_iOS ok.");
    }

    //在这里找出你当前工程所有的场景文件，假设你只想把部分的scene文件打包 那么这里可以写你的条件判断 总之返回一个字符串数组。
    static string[] GetBuildScenes()
    {
        List<string> names = new List<string>();

        foreach (EditorBuildSettingsScene e in EditorBuildSettings.scenes)
        {
            if (e == null)
                continue;
            if (e.enabled)
                names.Add(e.path);
        }
        return names.ToArray();
    }

    private static void WriteMsg(string path, string msg)
    {
        //获取权限
        executeCommand("chmod ", "+x " + path);
        try
        {
            StreamWriter sw = new StreamWriter(path, false);
            if (sw != null)
            {
                sw.Write(msg);

                sw.Close();
                sw.Dispose();
            }
        }
        catch (Exception) { }
    }

    /*********************************************************************
    *                           Windows脚本打包
    **********************************************************************/

    /*
    private static void WritePlatformFile()
    {
        string strPlatform = "cn";

        switch (g_JenkinsParam.PlatformType)
        {
            case Platform.PlatformType.Korea:
                strPlatform = "korea";
                break;

            default:
                break;
        };

        string platformTextPath = "./Assets/Resources/Locale.txt";
        //获取权限
        WriteMsg(platformTextPath, strPlatform);
    }
    */
    private static void executeCommand(string exe, string param, bool wait = true)
    {
        using (Process process = Process.Start(exe, param))
        {
            if (wait)
                process.WaitForExit();
        }
    }

    static void WriteLocalSvnVersion(string paltform)
    {   
        string cmdPath = Application.dataPath + "/../../BuildTools/svn_scripts/get_local_version.cmd";
        executeCommand(cmdPath, paltform);
    }
    
    private static void ResetKeystore()
    {
#if PLATFORM_KAKAO
        DeviceLogger.Instance.WriteLog("ResetKeystore for Kakao");
        PlayerSettings.Android.keystoreName = Application.dataPath + "/../../SDK/Kakao/kakaogames.keystore";
        PlayerSettings.Android.keystorePass = "kgs5858";
        PlayerSettings.Android.keyaliasName = "kakaogames";
        PlayerSettings.Android.keyaliasPass = "kgs5858";
#elif PLATFORM_LONGTU
        DeviceLogger.Instance.WriteLog("ResetKeystore for Longtu");
        PlayerSettings.Android.keystoreName = Application.dataPath + "/../../BuildTools/tera_keystore/lt.keystore";
        PlayerSettings.Android.keystorePass = "771112";
        PlayerSettings.Android.keyaliasName = "520078";
        PlayerSettings.Android.keyaliasPass = "771112";
#else
        DeviceLogger.Instance.WriteLog("ResetKeystore for NoKakao");
        PlayerSettings.Android.keystoreName = Application.dataPath + "/../../BuildTools/tera_keystore/tera.keystore";
        PlayerSettings.Android.keystorePass = "123456";
        PlayerSettings.Android.keyaliasName = "tera";
        PlayerSettings.Android.keyaliasPass = "123456";
#endif
    }

    static void BuildForAndroid()
    {
        WriteLocalSvnVersion("Android");
#if PLATFORM_KAKAO
        EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Gradle;
#elif PLATFORM_LONGTU
        EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Internal;
#else
        EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Gradle;
#endif

        ResetKeystore();

        string[] commands = System.Environment.GetCommandLineArgs();
        string TERA_ALL_NAME = commands[commands.Length - 2];
        string path = commands[commands.Length - 1] + "/" + TERA_ALL_NAME;;
        //Application.dataPath + "/../../Package/" + TERA_ALL_NAME;

        BuildOptions buildOption = BuildOptions.None;
        string error = BuildPipeline.BuildPlayer(GetBuildScenes(), path, BuildTarget.Android, buildOption);
        if( string.IsNullOrEmpty(error))
            EditorApplication.Exit(0);
        else
        {
            DeviceLogger.Instance.WriteLogFormat("[ERROR] BuildForAndroid got error:{0}", error);
            EditorApplication.Exit(1);
        }
    }

    //得到项目的名称
    public static string projectName
    {
        get
        {
            //这里遍历所有参数，找到 project开头的参数， 然后把-符号 后面的字符串返回，
            foreach (string arg in System.Environment.GetCommandLineArgs())
            {
                if (arg.StartsWith("project"))
                {
                    return arg.Split("-"[0])[1];
                }
            }
            return "test";
        }
    }

    //shell脚本直接调用这个静态方法
    static void BuildForIPhone()
    {
        BuildOptions buildOption = BuildOptions.None;
        buildOption |= BuildOptions.Il2CPP; //是否支持 Il2CPP
        //buildOption |= BuildOptions.SymlinkLibraries;
        buildOption |= BuildOptions.AcceptExternalModificationsToPlayer;

        BuildPipeline.BuildPlayer(GetBuildScenes(), projectName, BuildTarget.iOS, buildOption);
    }

    static void BuildForWindows()
    {
        WriteLocalSvnVersion("Windows");
        string date = DateTime.Now.ToString("yyyy-MM-dd");
        //string path = Application.dataPath + "/../../Package/Tera-Win-" + date + ".exe";
		string[] commands = System.Environment.GetCommandLineArgs();
        string path = commands[commands.Length - 1] + "/Tera-Win-" + date + ".exe";

        BuildOptions buildOption = BuildOptions.None;
        buildOption |= BuildOptions.AcceptExternalModificationsToPlayer;

        string error = BuildPipeline.BuildPlayer(GetBuildScenes(), path, BuildTarget.StandaloneWindows64, buildOption);
        if( string.IsNullOrEmpty(error))
            EditorApplication.Exit(0);
        else
            EditorApplication.Exit(1);
    }

    static string AllowUseMicrophone = "Allow game to use Microphone?";
    static string AllowUsePhoto = "Allow game to use Photo?";
    static string AllowUseCamera = "Allow game to use Camera?";
    static string AllowAddToPhoto = "Allow add image to photo library?";
    static string AllowUseMicrophoneChinese = "是否允许使用麦克风？";
    static string AllowUsePhotoChinese = "是否允许使用相册？";
    static string AllowUseCameraChinese = "是否允许使用相机？";
    static string AllowAddToPhotoChinese = "是否允许添加照片到相册?";

    static string AllowUseMicrophoneKorean = "비디오 접근 권한 : 비디오 촬영(고객센터 첨부) 기능을 위해서 마이크 사용을 허용해주세요.";
    static string AllowUsePhotoKorean = "테라 클래식 앱에서 앨범에 접근하는 것을 허용하시겠습니까?";
    static string AllowUseCameraKorean = "카메라 접근 권한 : 스크린 샷 촬영(고객센터 첨부) 기능을 위해서 카메라 사용을 허용해주세요.";
    static string AllowAddToPhotoKorean = "사진첩 접근 권한 : 프로필 사진 설정과 스크린 샷 촬영(고객센터 첨부) 기능을 위한 사진첩 접근을 허용해주세요."; 


    static void SetPlistStrings()
    {
        /*
        SystemLanguage lan = Application.systemLanguage;
        if (lan == SystemLanguage.Korean)
        {
            AllowUseMicrophone = "테라 클래식 앱에서 마이크에 접근하는 것을 허용하시겠습니까?";
            AllowUsePhoto = "테라 클래식 앱에서 앨범에 접근하는 것을 허용하시겠습니까?";
            AllowUseCamera = "테라 클래식 앱에서 카메라에 접근하는 것을 허용하시겠습니까?";
            AllowAddToPhoto = "테라 클래식 앱에서 앨범에 접근하여 이미지를 저장하는 것을 허용하시겠습니까?";
        }
        else if (lan == SystemLanguage.Chinese || lan == SystemLanguage.ChineseSimplified)
        {
            AllowUseMicrophone = "是否允许Tera使用麦克风？";
            AllowUsePhoto = "是否允许Tera使用照片？";
            AllowUseCamera = "是否允许Tera使用相机？";
            AllowAddToPhoto = "是否允许照片添加到相册?";
        }
        else 
        {
            AllowUseMicrophone = "Allow game to use Microphone?";
            AllowUsePhoto = "Allow game to use Photo?";
            AllowUseCamera = "Allow game to use Camera?";
            AllowAddToPhoto = "Allow image add to photo library?";
        }
         * */
    }

#if UNITY_IOS
    // Line语音frameworks
    private static readonly string[] _LGVCFrameworks =
    {
        "Frameworks/LGVC/GroupCall/Plugins/iOS/AMPKit_4.5.27-13718c10c_iOS/AMPKit.framework",
		"Frameworks/LGVC/GroupCall/Plugins/iOS/AMPKit_4.5.27-13718c10c_iOS/MOSQKit.framework",
		"Frameworks/LGVC/SpeechRecognition/Plugins/iOS/NaverSpeech.framework",
    };


    // ios版本xcode工程维护代码  
    [PostProcessBuild(999)]
    public static void OnPostprocessBuild(BuildTarget BuildTarget, string path)
    {
        if (BuildTarget == BuildTarget.iOS)
        {
            SetPlistStrings();

            string projPath = PBXProject.GetPBXProjectPath(path);
            PBXProject proj = new PBXProject();
            proj.ReadFromString(File.ReadAllText(projPath));

            // 获取当前项目名字  
            string target = proj.TargetGuidByName(PBXProject.GetUnityTargetName());

            // 对所有的编译配置设置选项  
            proj.SetBuildProperty(target, "ENABLE_BITCODE", "NO");

            // 添加依赖库  
            proj.AddFrameworkToProject(target, "CoreTelephony.framework", false);
            proj.AddFrameworkToProject(target, "CoreAudio.framework", false);
            proj.AddFrameworkToProject(target, "Photos.framework", false);
            proj.AddFrameworkToProject(target, "AssetsLibrary.framework", false);
            //proj.AddFrameworkToProject(target, "libstdc++.6.0.9.tbd", false);
            proj.AddFrameworkToProject(target, "libsqlite3.tbd", false);
            proj.AddFrameworkToProject(target, "libz.tbd", false);
            proj.AddFrameworkToProject(target, "libc++.tbd", false);

            string frameworkSearchPath = string.Format("\"$(SRCROOT)/{0}\"", FrameworkXcodeRoot);
            proj.AddBuildProperty(target, "FRAMEWORK_SEARCH_PATHS", frameworkSearchPath);
#if USING_BUGLY
            //Bugly SDK
            proj.AddBuildProperty(target, "OTHER_LDFLAGS", "-ObjC"); // Other Linker Flags

			proj.AddFrameworkToProject(target, "JavaScriptCore.framework", false);
			proj.AddFrameworkToProject(target, "Security.framework", false);

            /*
            string desPath = FrameworkXcodeRoot;
            string srcPath = "../SDK/Bugly/Bugly.framework"; //Framework所在原路径
            string fullPath = string.Format("{0}/Bugly.framework", desPath);
            CopyAndReplaceDirectory(srcPath, Path.Combine(path, fullPath));
            proj.AddFileToBuild(target, proj.AddFile(fullPath, fullPath, PBXSourceTree.Source));
            */
            foreach (string framework in BuglyManager.Frameworks)
            {
                string srcPath = Path.Combine(BuglyManager.SDKPath, framework);                 //Framework所在原路径（绝对路径）
                string dstPath = Path.Combine(GetXcodeFrameworkFullPath(path), framework);      //复制Framework的目标路径（绝对路径）
                CopyAndReplaceDirectory(srcPath, dstPath);
                string fullPath = Path.Combine(FrameworkXcodeRoot, framework);                  //添加到Xcode工程的文件路径（相对路径）
                proj.AddFileToBuild(target, proj.AddFile(fullPath, fullPath, PBXSourceTree.Source));
            }
#endif

#if false //USING_LGVC
            //LGVC 添加动态库 参考 https://answers.unity.com/questions/1074471/is-it-possible-to-use-the-xcode-manipulation-api-t.html
            {
                proj.AddDynamicFrameworksToProject(target, _LGVCFrameworks);

                proj.AddBuildProperty(target, "LD_RUNPATH_SEARCH_PATHS", "@executable_path/Frameworks");
            }
#endif

            // 设置签名
            proj.SetBuildProperty(target, "DEVELOPMENT_TEAM", "6UDLK9RFG9");
            proj.SetBuildProperty(target, "CODE_SIGN_IDENTITY", "iPhone Developer: Xin Huang (396999LAWD)");
            proj.SetBuildProperty(target, "PROVISIONING_PROFILE_SPECIFIER", "tera-all");
            // objc exception
            proj.SetBuildProperty(target, "GCC_ENABLE_OBJC_EXCEPTIONS", "YES");

            // 保存工程  
            proj.WriteToFile(projPath);

#if false //USING_LGVC
            // LGVC额外的framework codesign
            {
                string contents = File.ReadAllText(projPath);
                foreach (string framework in _LGVCFrameworks)
                {
                    string filename = System.IO.Path.GetFileNameWithoutExtension(framework);
                    DeviceLogger.Instance.WriteLog("Add FrameWork and CodeSign: " + filename);

//                     contents = Regex.Replace(contents,
//                     "(?<=Embed Frameworks)(?:.*)(\\/\\* EXAMPLE\\.framework \\*\\/)(?=; };)",
//                     m => m.Value.Replace("/* EXAMPLE.framework */",
//                     "/* EXAMPLE.framework */; settings = {ATTRIBUTES = (CodeSignOnCopy, ); }"));

                    // Enable CodeSignOnCopy for the framework
                    contents = Regex.Replace(contents,
                        "(?<=Embed Frameworks)(?:.*)(\\/\\* " + filename + "\\.framework \\*\\/)(?=; };)",
                        m => m.Value.Replace("/* " + filename + ".framework */",
                        "/* " + filename + ".framework */; settings = {ATTRIBUTES = (CodeSignOnCopy, ); }"));
                }
                File.WriteAllText(projPath, contents);
            }
#endif

            // 修改plist  
            string plistPath = path + "/Info.plist";
            PlistDocument plist = new PlistDocument();
            plist.ReadFromString(File.ReadAllText(plistPath));
            PlistElementDict rootDict = plist.root;

            // 语音所需要的声明，iOS10必须  
            //rootDict.SetString("NSMicrophoneUsageDescription", AllowUseMicrophone);
            rootDict.SetString("NSPhotoLibraryUsageDescription", AllowUsePhotoKorean);
            rootDict.SetString("NSCameraUsageDescription", AllowUseCameraKorean);
            rootDict.SetBoolean("UIFileSharingEnabled", true);
            //rootDict.SetBoolean("UIApplicationExitsOnSuspend", false);

            // iOS11需要声明 NSPhotoLibraryAddUsageDescription
            rootDict.SetString("NSPhotoLibraryAddUsageDescription", AllowAddToPhotoKorean);

            // 保存plist  
            plist.WriteToFile(plistPath);
        }
    }

    internal static void CopyAndReplaceDirectory(string srcPath, string dstPath)
    {
        if (Directory.Exists(dstPath))
            Directory.Delete(dstPath);
        if (File.Exists(dstPath))
            File.Delete(dstPath);

        Directory.CreateDirectory(dstPath);

        foreach (var file in Directory.GetFiles(srcPath))
            File.Copy(file, Path.Combine(dstPath, Path.GetFileName(file)));

        foreach (var dir in Directory.GetDirectories(srcPath))
            CopyAndReplaceDirectory(dir, Path.Combine(dstPath, Path.GetFileName(dir)));
    }

#endif
}