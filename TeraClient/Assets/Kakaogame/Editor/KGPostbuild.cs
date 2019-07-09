#if PLATFORM_KAKAO
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using System.Xml;

using Kakaogame.Base.Pathfinding.Serialization.JsonFx;

namespace Kakaogame.SDK.Editor {	
	public class Postbuild : MonoBehaviour {
		#pragma warning disable 0414
		private static readonly string configPath = KGPackage.sharedDataPath;
		#pragma warning restore 0414

		private static readonly string injectIdpInfoPath = "Assets/Kakaogame/Editor/inject_idp_info.rb";
		#if UNITY_IPHONE
		private static readonly String modiXcodePath = "Assets/Kakaogame/Editor/modixcode.rb";
		#endif
		#if UNITY_ANDROID
		private static readonly string modiAndroidPath = "Assets/Kakaogame/Editor/modiandroid.rb";
		#endif
		private static readonly string injectConfigPath = "Assets/Kakaogame/Editor/inject_configs.rb";

		/// <summary>
		/// Dictionary형태로 된 옵션들을 조합해서 String형태의 args를 만든다
		/// </summary>
		/// <example>
		/// var args = new Dictionary<String,String>(){
		/// 	{"foo", "bar"}, {"hi", "hello"}
		/// };
		/// 
		/// /* --foo bar --hi hello */
		/// var argv = AssembleArgs(args);
		/// </example>
		private static string AssembleArgs(Dictionary<string, string> args){
			string result = "";

			if (args != null) {
				foreach (var arg in args) {
					result += string.Format (
						"--{0} {1} ", arg.Key, arg.Value);
				}
			}

			return result;
		}
		private static void RunScript(string path, Dictionary<string, string> _args){
			var args = AssembleArgs (_args);

			UnityEngine.Debug.Log ("Run PostbuildScript / " + path + " / " + args);

			Process p = new Process ();

			p.StartInfo.FileName = "ruby";
			p.StartInfo.Arguments = path + " " + args;
			p.StartInfo.UseShellExecute = false;
			p.StartInfo.RedirectStandardError = true;
			p.StartInfo.RedirectStandardOutput = true;
			p.StartInfo.CreateNoWindow = false;

			p.Start (); p.WaitForExit ();

			UnityEngine.Debug.Log ("stderr : " + p.StandardError.ReadToEnd ());
			UnityEngine.Debug.Log ("stdout : " + p.StandardOutput.ReadToEnd ());
		}

		private static void ReloadExternalConfig(){
			/* does nothing */
		}

		#if UNITY_IOS
		// for Unity 5
		[PostProcessBuild(100)]
		public static void OnPostprocessBuild(BuildTarget buildTarget, string buildPath)
		{
			if (!KGIosSupport.IsIOSSupport(buildPath))
				return;
			
			UnityEngine.Debug.Log("---KakaogameSDK::OnIosPostbuild---");
			UnityEngine.Debug.Log(buildTarget + "  /  " + buildPath);

			KGIosSupport.Begin(buildPath);

			// Delete Android Plugin
			KGIosSupport.RemoveAndroidPlugin();

			// Add LinkerFlags
#region LINKER_FLAGS
			UnityEngine.Debug.Log("Add LinkerFlags");
			KGIosSupport.AddLinkerFlag("-all_load");
			KGIosSupport.AddLinkerFlag("-lz");
			KGIosSupport.AddLinkerFlag("-lxml2");
#endregion

			// Add Frameworks
#region FRAMEWORK_DEPS
			UnityEngine.Debug.Log("Add Frameworks");
            AddKakaoFrameworks(buildPath);
#endregion

			// URL SCHEME
#region URL_SCHEMES
			KGIosSupport.AddQueryScheme("com.kakaogames.sdk");
			KGIosSupport.AddURLScheme("KakaoGameSDK", "kakaogame" + KGSharedData.Configuration.appId);
#endregion

#region ADD_OR_MERGE_PLIST
			// DISABLE BITCODE
			KGIosSupport.AddBuildProperty("ENABLE_BITCODE", "NO");

			UnityEngine.Debug.Log("Add KakaoGame Configurations");

			KGIosSupport.AddConfig("UIViewControllerBasedStatusBarAppearance", false);

			//KGIosSupport.AddConfig("NSCameraUsageDescription", "고객센터 파일 첨부 용도로 사용합니다.");
			//KGIosSupport.AddConfig("NSPhotoLibraryUsageDescription", "고객센터 파일 첨부 용도로 사용합니다.");

            KGSharedData.Configuration.appVersion = PlayerSettings.bundleVersion;
			KGIosSupport.AddConfig("KakaoGameConfiguration",
				new Dictionary<string, object>()
				{
					{"AppId", KGSharedData.Configuration.appId},
					{"AppSecret", KGSharedData.Configuration.appSecret},
					{"AppVersion", KGSharedData.Configuration.appVersion},
					{"DebugLevel", KGSharedData.Configuration.debugLevel.ToString().ToLower()},
					{"ServerType", KGSharedData.Configuration.serverType.ToLower()}
				});

			KGIosSupport.MergeConfig("UIRequiredDeviceCapabilities", new List<object>() { "gamekit" });

			if (KGSharedData.instance.sdkDevelopConfiguration.useIOSTestAppAutoSetting == true)
			{
				KGIosSupport.AddConfig("ITSAppUsesNonExemptEncryption", false); 
			}

			KGIosSupport.AddConfig("CFBundleAllowMixedLocalizations", true);

#endregion

			var enabledComponents = new List<Component.KGComponentBase>();
			///if (KGSharedData.UseForKakao2)
			enabledComponents.Add(KGBuildAPI.GetComponent(KGBuildAPI.KGComponent.Kakao));
			if (KGSharedData.UseGoogle)
				enabledComponents.Add(KGBuildAPI.GetComponent(KGBuildAPI.KGComponent.Google));
			if (KGSharedData.UseFacebook)
				enabledComponents.Add(KGBuildAPI.GetComponent(KGBuildAPI.KGComponent.Facebook));

            // Add capabilities
#if UNITY_2017_1_OR_NEWER
			UnityEngine.Debug.Log("Add Capabailities");
			KGIosSupport.AddPushNotificationCapability(buildPath);
			KGIosSupport.AddKeychainSharingCapability(buildPath);
            KGIosSupport.AddMarketingIcon(buildPath);
#else
            AddCapabilities(buildPath);
#endif

			UnityEngine.Debug.Log("PostbuildProcess for enabled components....");
			foreach (var c in enabledComponents)
			{
				UnityEngine.Debug.Log("EnabledComponent : " + c.GetType().Name);
				c.OnIOSPostprocess();
			}

			KGIosSupport.Apply();
        }

        private static readonly string[] _SystemFrameworks = {
			//For Kakao Game SDK
			"Foundation.framework",
            "SystemConfiguration.framework",
            "UIKit.framework",
            "libstdc++.tbd",
            "libz.tbd",

			//For Google SDK
			"CoreLocation.framework",
            "CoreGraphics.framework",
            "CoreText.framework",
            "MediaPlayer.framework",

			//For Facebook SDK
			"AVFoundation.framework",
            "CoreMedia.framework",

            //For Apple Game Center
            "GameKit.framework",

			//Common
			"CoreMotion.framework",
            "SafariServices.framework",
            "AssetsLibrary.framework",
            "AddressBook.framework",
            "Security.framework",
            "StoreKit.framework",
            "CoreTelephony.framework",
            "AdSupport.framework"
        };

        private static readonly string _FrameworkRoot = Path.Combine(Application.dataPath, "../../SDK/Kakao/Frameworks");
        private static readonly string[] _FolderPaths =
        {
            "IDPFramework/Google.plugin",
            "KakaoGameFramework",
            "FacebookFramework"
        };

        private static void AddKakaoFrameworks(string buildPath)
        {
            foreach (var framework in _SystemFrameworks)
            {
                KGIosSupport.AddFramework(framework);
            }

            string frameworkFullPath = BuildTools.GetXcodeFrameworkFullPath(buildPath);
            foreach (var folderPath in _FolderPaths)
            {
                string folderFullPath = Path.Combine(_FrameworkRoot, folderPath);
                if (Directory.Exists(folderFullPath))
                {
                    foreach(var fileFullPath in Directory.GetDirectories(folderFullPath))
                    {
                        string fileName = Path.GetFileName(fileFullPath);
                        if (fileName.EndsWith(".framework") || fileName.EndsWith(".bundle"))
                        {
                            string dstFullPath = Path.Combine(frameworkFullPath, fileName);
                            FolderUtil.CopyAndReplace(fileFullPath, dstFullPath);
                            string xcodePath = Path.Combine(BuildTools.FrameworkXcodeRoot, fileName);                       //添加到Xcode工程的文件路径（相对路径）
                            KGIosSupport.AddFileToBuild(xcodePath, xcodePath);
                        }
                    }
                }
            }
        }

        private static readonly string _EntitlementsFileName = "TERA.entitlements";
        private static readonly string _KeyChainSharePrefix = "$(AppIdentifierPrefix)";
        private static void AddCapabilities(string buildPath)
        {
            string fullPath = Path.Combine(BuildTools.GetXcodeEntitlementsRoot(buildPath), _EntitlementsFileName);
            if (File.Exists(fullPath))
                File.Delete(fullPath);
            
            //Create
            XmlDocument xmlDoc = new XmlDocument();
            XmlDeclaration declaration = xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", "yes");
            xmlDoc.AppendChild(declaration);
            XmlDocumentType docType = xmlDoc.CreateDocumentType("plist", @"-//Apple//DTD PLIST 1.0//EN", @"http://www.apple.com/DTDs/PropertyList-1.0.dtd", null);
            xmlDoc.AppendChild(docType);

            XmlElement plist = xmlDoc.CreateElement("plist");
            plist.SetAttribute("version", "1.0");
            xmlDoc.AppendChild(plist);

            XmlElement dict = xmlDoc.CreateElement("dict");
            {
                //Set KeyChainShare
                XmlElement key = xmlDoc.CreateElement("key");
                key.InnerText = "keychain-access-groups";
                dict.AppendChild(key);

                XmlElement array = xmlDoc.CreateElement("array");
                //App itself
                XmlElement group_1 = xmlDoc.CreateElement("string");
                group_1.InnerText = _KeyChainSharePrefix + PlayerSettings.applicationIdentifier;
                array.AppendChild(group_1);
                //Kakao
                XmlElement group_2 = xmlDoc.CreateElement("string");
                group_2.InnerText = _KeyChainSharePrefix + "KakaoGameAccessGroup";
                array.AppendChild(group_2);

                dict.AppendChild(array);
            }
            plist.AppendChild(dict);

            xmlDoc.Save(fullPath);
            KGIosSupport.AddBuildProperty("CODE_SIGN_ENTITLEMENTS", fullPath);
        }

#endif // IOS
    }
}
#endif