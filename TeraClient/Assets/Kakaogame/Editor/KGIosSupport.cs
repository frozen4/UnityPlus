#if PLATFORM_KAKAO
#if UNITY_IOS && (UNITY_5 || UNITY_2017_1_OR_NEWER)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using UnityEditor;
using UnityEngine;
using UnityEditor.iOS.Xcode;

namespace Kakaogame.SDK.Editor
{
	public class KGIosSupport
	{
		private static PBXProject proj { get; set; }
		private static PlistDocument plist { get; set; }
#if UNITY_2017_1_OR_NEWER
		private static ProjectCapabilityManager pcm { get; set; }
#endif
		private static string basePath = "./Unity-iPhone.xcodeproj/project.pbxproj";

		public static bool IsIOSSupport(string path)
		{
			if (File.Exists(path + "/Info.plist"))
				return true;

			return false;
		}

		public static void Begin(string path)
		{
			basePath = path;

			proj = new PBXProject();
			proj.ReadFromFile(PBXProject.GetPBXProjectPath(path));
			plist = new PlistDocument();
			plist.ReadFromFile(path + "/Info.plist");
#if UNITY_2017_1_OR_NEWER
			pcm = new ProjectCapabilityManager(PBXProject.GetPBXProjectPath(path), "unity.entitlements", PBXProject.GetUnityTargetName());
#endif 
		}

		public static void Apply()
		{
			//proj.WriteToFile(PBXProject.GetPBXProjectPath(basePath));
			plist.WriteToFile(basePath + "/Info.plist");
#if UNITY_2017_1_OR_NEWER
			pcm.WriteToFile();
#endif
			proj.WriteToFile(PBXProject.GetPBXProjectPath(basePath));

		}

		public static void RemoveAndroidPlugin()
		{
			//string fileGuid = proj.FindFileGuidByProjectPath(basePath + "/Libraries/Plugins/Android/KakaoGameSDK/res/drawable-xhdpi/login_img_ryan.png");
			//proj.RemoveFile(fileGuid);
		}

		public static void AddFileToBuild(string path, string projectPath)
		{
			string target = proj.TargetGuidByName(PBXProject.GetUnityTargetName());
			string file = proj.AddFile (path, projectPath, PBXSourceTree.Source);
			proj.AddFileToBuild (target, file);
		}

		public static void AddBuildProperty(string opt, string value)
		{
			string target = proj.TargetGuidByName(PBXProject.GetUnityTargetName());
			proj.AddBuildProperty(target, opt, value);
		}
		public static void AddLinkerFlag(string opt)
		{
			string target = proj.TargetGuidByName(PBXProject.GetUnityTargetName());
			proj.AddBuildProperty(target, "OTHER_LDFLAGS", opt);
		}
		public static void AddFramework(string framework)
		{
			string target = proj.TargetGuidByName(PBXProject.GetUnityTargetName());
			proj.AddFrameworkToProject(target, framework, false);
		}

#if UNITY_2017_1_OR_NEWER
		public static void MoveEntitlementsFile(string buildPath)
		{
			// move entitlements file
			string srcFilePath = buildPath + "/unity.entitlements";
			string dstFilePath = buildPath + "/Unity-iPhone/unity.entitlements";
			if (!File.Exists(dstFilePath))
			{
				FileUtil.MoveFileOrDirectory(srcFilePath, dstFilePath);
				proj.AddFile("Unity-iPhone/unity.entitlements", "unity.entitlements");
			}
		}

		public static void AddPushNotificationCapability(string buildPath)
		{
			//MoveEntitlementsFile(buildPath);

			string targetGuid = proj.TargetGuidByName(PBXProject.GetUnityTargetName());
			bool pushCapability = proj.AddCapability(targetGuid, PBXCapabilityType.PushNotifications, "unity.entitlements");
			if (pushCapability)
				Debug.Log("Push Notification capabilities added successfully.");

			pcm.AddPushNotifications(true);
		}

		public static void AddKeychainSharingCapability(string buildPath)
		{
			//MoveEntitlementsFile(buildPath);

			string target = proj.TargetGuidByName(PBXProject.GetUnityTargetName());
			bool keychainCapability = proj.AddCapability(target, PBXCapabilityType.KeychainSharing);
			if (keychainCapability)
				Debug.Log("Keychain Sharing capabilities added successfully.");

			string appIdentifierPrefix = "$(AppIdentifierPrefix)";
			string bundleIdentifier = "";
#if UNITY_5_6_OR_NEWER
			bundleIdentifier = PlayerSettings.applicationIdentifier;
#else
			bundleIdentifier = PlayerSettings.bundleIdentifier;
#endif
			pcm.AddKeychainSharing(new string[] { appIdentifierPrefix + bundleIdentifier, appIdentifierPrefix + "KakaoGameAccessGroup" });
		}

		public static void AddMarketingIcon(string buildPath)
		{
			string marketingIconSrcFilePath = "Assets/Resources/Kakaogame/AppIcon-1024.png";
			string marketingIconDstFilePath = buildPath + "/Unity-iPhone/Images.xcassets/AppIcon.appiconset/Icon-1024.png";

			FileUtil.CopyFileOrDirectory(marketingIconSrcFilePath, marketingIconDstFilePath);
		}
#endif

		public static void AddConfig(string key, Dictionary<string, object> values)
		{
			if (values == null) return;
			var dic = plist.root.CreateDict(key);
			AddConfig(dic, values);
		}
		public static void AddConfig(string key, List<object> values)
		{
			if (values == null) return;
			var ary = plist.root.CreateArray(key);
			AddConfig(ary, values);
		}
		public static void AddConfig(string key, List<string> values)
		{
			if (values == null) 
				return;
			AddConfig(key, values.Select(x => (object)x).ToList());
		}
		public static void AddConfig(string key, string value)
		{
			plist.root.SetString(key, value);
		}
		public static void AddConfig(string key, bool value)
		{
			plist.root.SetBoolean(key, value);
		}
		public static void AddConfig(string key, int value)
		{
			plist.root.SetInteger(key, value);
		}

		public static void MergeConfig(string key, Dictionary<string, object> values)
		{
			if (values == null) return;

			if (plist.root.values.ContainsKey(key) == false)
				AddConfig(key, values);
			else
			{
				var dic = plist.root.values[key].AsDict();
				AddConfig(dic, values);
			}
		}

		public static void MergeConfig(string key, List<object> values)
		{
			if (values == null) return;

			if (plist.root.values.ContainsKey(key) == false)
				AddConfig(key, values);
			else
			{
				var ary = plist.root.values[key].AsArray();
				AddConfig(ary, values);
			}
		}

		public static void AddQueryScheme(string scheme)
		{
			MergeConfig("LSApplicationQueriesSchemes", new List<object>()
				{
					scheme
				});
		}

		public static void AddQueryScheme(List<object> schemes)
		{
			MergeConfig("LSApplicationQueriesSchemes", schemes);
		}

		public static void AddURLScheme(string id, string scheme)
		{
			MergeConfig("CFBundleURLTypes",
				new List<object>()
				{
					new Dictionary<string, object>()
					{
						{"CFBundleURLName", id },
						{"CFBundleURLSchemes", new List<object>()
							{
								scheme
							}}
					}
				});
		}

		private static void AddConfig(PlistElementDict dic, Dictionary<string, object> values)
		{
			foreach (var pair in values)
			{
				if (pair.Value is string)
					dic.SetString(pair.Key, (string)pair.Value);
				else if (pair.Value is int)
					dic.SetInteger(pair.Key, (int)pair.Value);
				else if (pair.Value is bool)
					dic.SetBoolean(pair.Key, (bool)pair.Value);
				else if (pair.Value is Dictionary<string, object>)
				{
					var parent = dic.CreateDict(pair.Key);
					AddConfig(parent, (Dictionary<string, object>)pair.Value);
				}
				else if (pair.Value is List<object>)
				{
					var parent = dic.CreateArray(pair.Key);
					AddConfig(parent, (List<object>)pair.Value);
				}
			}
		}

		private static void AddConfig(PlistElementArray ary, List<object> values)
		{
			foreach (var v in values)
			{
				if (v is int)
					ary.AddInteger((int)v);
				else if (v is string)
					ary.AddString((string)v);
				else if (v is bool)
					ary.AddBoolean((bool)v);
				else if (v is Dictionary<string, object>)
				{
					var parent = ary.AddDict();
					AddConfig(parent, (Dictionary<string, object>)v);
				}
			}
		}
	}
}

#endif
#endif