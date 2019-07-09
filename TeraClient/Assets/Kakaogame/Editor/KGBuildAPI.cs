#if PLATFORM_KAKAO
using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

using UnityEngine;
using UnityEditor;

namespace Kakaogame.SDK.Editor{
	using Component;
	using Base.Pathfinding.Serialization.JsonFx;

	/// <summary>
	/// 커스텀 빌드 환경 구성을 위한 API들을 제공하는 클래스
	/// </summary>
	public class KGBuildAPI {
		public enum KGComponent {
			Push,
			Kakao,
			Facebook,
			Google,
		}

		const string androidSDKManifestFilePath = "Assets/Kakaogame/AndroidPlugins/KakaoGameSDK.plugin/AndroidManifest.xml";
		const string androidSDKValuesFilePath = "Assets/Kakaogame/AndroidPlugins/KakaoGameSDK.plugin/res/values/kakao_game_sdk_values.xml";
		const string androidSDKKakaoAuthFilePath = "Assets/Kakaogame/AndroidPlugins/KakaoGameSDK.plugin/res/values/kakao_game_sdk_kakao_auth.xml";
		const string androidSDKGoogleAuthFilePath = "Assets/Kakaogame/AndroidPlugins/KakaoGameSDK.plugin/res/values/kakao_game_google_auth.xml";
		const string androidSDKFacebookAuthFilePath = "Assets/Kakaogame/AndroidPlugins/KakaoGameSDK.plugin/res/values/kakao_game_facebook_auth.xml";
		const string androidSDKConfigurationFilePath = "Assets/Plugins/Android/assets/kakao_game_sdk_configuration.xml";

		const string androidPluginPath = "Assets/Kakaogame/AndroidPlugins/KakaoGameSDK.plugin";

		static readonly string[] androidFacebookPluginPaths = new string[] {
			"Assets/FacebookSDK/Plugins/Android/Facebook.Unity.Android.dll",
			"Assets/FacebookSDK/Plugins/Android/Facebook.Unity.IOS.dll",
		};

		static readonly string[] iOSPluginPaths = new string[] {
			"Assets/Kakaogame/iOSPlugins/IDPFramework/Google.plugin",
			"Assets/Kakaogame/iOSPlugins/KakaoGameFramework/KakaoGame.framework",
			"Assets/Kakaogame/iOSPlugins/KakaoGameFramework/KakaoGameResources.bundle",
		};
		const string iOSGooglePlayPluginPath = "Assets/Kakaogame/iOSPlugins/IDPFramework/Google.plugin";
		const string iOSGooglePlayIdpPath = "Assets/Kakaogame/iOSPlugins/KakaoGameFramework/KakaoGameGoogle.framework";
		//		const string iOSFacebookPluginPath = "Assets/Kakaogame/iOSPlugins/IDPFramework/Facebook.plugin";
		static readonly string[] iOSFacebookPluginPaths = new string[] {
			"Assets/FacebookSDK/Plugins/iOS/Facebook.Unity.Android.dll",
			"Assets/FacebookSDK/Plugins/iOS/Facebook.Unity.IOS.dll",
			"Assets/FacebookSDK/Plugins/iOS/FBSDKCoreKit.framework",
			"Assets/FacebookSDK/Plugins/iOS/FBSDKLoginKit.framework",
			"Assets/FacebookSDK/Plugins/iOS/FBSDKShareKit.framework"
		};
		const string iOSFacebookIdpPath = "Assets/Kakaogame/iOSPlugins/KakaoGameFramework/KakaoGameFacebook.framework";

		//		const string editorFacebookPluginPath = "Assets/FacebookSDK/Plugins/Editor/Facebook.Unity.Editor.dll";
		//		static readonly string iOSEditorFacebookPluginPath = "Assets/FacebookSDK/SDK/Editor/iOS";

		public static KGSharedData.NZMutableConfiguration configuration
		{
			get {
				return KGSharedData.instance.configuration;
			}
		}
		public static KGSharedData.GoogleInjectionData google
		{
			get {
				return KGSharedData.instance.google;
			}
		}
		public static KGSharedData.FacebookInjectionData facebook
		{
			get
			{
				return KGSharedData.instance.facebook;
			}
		}

		#region INTERNAL_API
		/// <summary>
		/// INTERNAL API (do not call this API directly)
		/// </summary>
		public static void BuildAndroid()
		{
			SetCompatiblePluginsWithPlatform(BuildTarget.Android, "Kakaogame/AndroidPlugins");
			WaitAndApply ();

			PlayerSettings.Android.keyaliasPass = "android";
			PlayerSettings.Android.keystorePass = "android";

			BuildPipeline.BuildPlayer(
				EditorBuildSettings.scenes
				.Where(x => x.enabled)
				.Select(x => x.path).ToArray(),
				"UnityTestAndroid.apk",
				BuildTarget.Android,
				BuildOptions.None);
		}

		/// <summary>
		/// INTERNAL API (do not call this API directly)
		/// </summary>
		public static void BuildAndroidForGameShop()
		{
			SetCompatiblePluginsWithPlatform(BuildTarget.Android, "Kakaogame/AndroidPlugins");

			string defineSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android);
			UnityEngine.Debug.Log("current define Symbol : " + defineSymbols);
			if (!defineSymbols.Contains("GAMESHOP_PAYMENT"))
				defineSymbols += ";" + "GAMESHOP_PAYMENT";
			PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, defineSymbols);

			WaitAndApply ();

			PlayerSettings.Android.keyaliasPass = "android";
			PlayerSettings.Android.keystorePass = "android";

			BuildPipeline.BuildPlayer(
				EditorBuildSettings.scenes
				.Where(x => x.enabled)
				.Select(x => x.path).ToArray(),
				"UnityTestAndroidForGameShop.apk",
				BuildTarget.Android,
				BuildOptions.None);
		}

		/// <summary>
		/// INTERNAL API (do not call this API directly)
		/// </summary>
		public static void BuildAndroid_Gradle()
		{
			SetCompatiblePluginsWithPlatform(BuildTarget.Android, "Kakaogame/AndroidPlugins");
			WaitAndApply ();

			PlayerSettings.Android.keyaliasPass = "android";
			PlayerSettings.Android.keystorePass = "android";

			// Change build system to gradle
			EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Gradle;

			BuildPipeline.BuildPlayer(
				EditorBuildSettings.scenes
				.Where(x => x.enabled)
				.Select(x => x.path).ToArray(),
				"UnityTestAndroid.apk",
				BuildTarget.Android,
				BuildOptions.None);
		}

		/// <summary>
		/// INTERNAL API (do not call this API directly)
		/// </summary>
		public static void BuildiOS()
		{
			SetCompatiblePluginsWithPlatform(BuildTarget.iOS, "Kakaogame/iOSPlugins");
			WaitAndApply ();

			BuildPipeline.BuildPlayer(
				EditorBuildSettings.scenes
				.Where(x => x.enabled)
				.Select(x => x.path).ToArray(),
				"UnityTestIOS",
				BuildTarget.iOS,
				BuildOptions.AcceptExternalModificationsToPlayer);
		}

		/// <summary>
		/// INTERNAL API (do not call this API directly)
		/// </summary>
		public static void BuildiOSProject()
		{
			var parameters = System.Environment.GetCommandLineArgs();
			int i = 0;
			string version = "";
			string buildNo = "";
			foreach (var param in parameters)
			{
				if (parameters[i] == "-Version")
				{
					version = parameters[++i];
					Debug.Log("Version : " + version);
					PlayerSettings.bundleVersion = version;
					continue;
				}
				else if (parameters[i] == "-BuildNo")
				{
					buildNo = parameters[++i];
					Debug.Log("BuildNo : " + buildNo);
					PlayerSettings.iOS.buildNumber = buildNo;
					continue;
				}

				i++;
			}

			SetCompatiblePluginsWithPlatform(BuildTarget.iOS, "Kakaogame/iOSPlugins");
			WaitAndApply ();

			BuildPipeline.BuildPlayer(
				EditorBuildSettings.scenes
				.Where(x => x.enabled)
				.Select(x => x.path).ToArray(),
				"UnityTestIOS",
				BuildTarget.iOS,
				BuildOptions.AcceptExternalModificationsToPlayer);
		}

		/// <summary>
		/// INTERNAL API (do not call this API directly)
		/// </summary>
		public static void ExportPackage()
		{
			Debug.Log("Export Package");

			SetCompatiblePluginsWithPlatform(BuildTarget.Android, "Kakaogame/AndroidPlugins");
			SetCompatiblePluginsWithPlatform(BuildTarget.iOS, "Kakaogame/iOSPlugins");
			WaitAndApply ();

			var files = new List<string>();

			files.AddRange(Directory.GetDirectories("Assets/FacebookSDK/", "*", SearchOption.AllDirectories));
			files.AddRange(Directory.GetFiles("Assets/FacebookSDK/", "*.*", SearchOption.AllDirectories));

			files.AddRange(Directory.GetDirectories("Assets/Kakaogame/AndroidPlugins/", "*", SearchOption.AllDirectories));
			files.AddRange(Directory.GetDirectories("Assets/Kakaogame/iOSPlugins/", "*", SearchOption.AllDirectories));
			files.AddRange(Directory.GetFiles("Assets/Kakaogame/", "*.*", SearchOption.AllDirectories));
			files.AddRange(Directory.GetFiles("Assets/Plugins/", "*.*", SearchOption.AllDirectories));
			files.AddRange(Directory.GetFiles("Assets/Resources/", "*.*", SearchOption.AllDirectories));
			files.AddRange(Directory.GetDirectories("Assets/Plugins/", "*", SearchOption.AllDirectories));

			AssetDatabase.ExportPackage(files.ToArray(), "../KakaoGameSDK.unitypackage", ExportPackageOptions.Recurse | ExportPackageOptions.IncludeDependencies);
		}

		/// <summary>
		/// INTERNAL API (do not call this API directly)
		/// </summary>
		public static void ExportPackageForGameShopPayment()
		{
			Debug.Log("Export Package For GameShopPayment");

			SetCompatiblePluginsWithPlatform(BuildTarget.Android, "Kakaogame/AndroidPlugins");
			WaitAndApply ();

			var files = new List<string>();

			files.AddRange(Directory.GetDirectories("Assets/FacebookSDK/", "*", SearchOption.AllDirectories));
			files.AddRange(Directory.GetFiles("Assets/FacebookSDK/", "*.*", SearchOption.AllDirectories));

			files.AddRange(Directory.GetDirectories("Assets/Kakaogame/AndroidPlugins/", "*", SearchOption.AllDirectories));
			files.AddRange(Directory.GetFiles("Assets/Kakaogame/", "*.*", SearchOption.AllDirectories));
			files.AddRange(Directory.GetFiles("Assets/Plugins/", "*.*", SearchOption.AllDirectories));
			files.AddRange(Directory.GetFiles("Assets/Resources/", "*.*", SearchOption.AllDirectories));
			files.AddRange(Directory.GetDirectories("Assets/Plugins/", "*", SearchOption.AllDirectories));

			AssetDatabase.ExportPackage(files.ToArray(), "../KakaoGameSDK_PaymentOnly.unitypackage", ExportPackageOptions.Recurse | ExportPackageOptions.IncludeDependencies);
		}

		private static void WaitAndApply()
		{
			while(KGSharedData.instance == null) {
				System.Threading.Thread.Sleep (1000);
			}
			KGBuildAPI.Apply ();
		}

		private static void SetCompatiblePluginsWithPlatform(BuildTarget target, string containPath)
		{
			var importers = PluginImporter.GetAllImporters();

			foreach (var importer in importers)
			{
				if (importer.assetPath.Contains(containPath))
				{
					if (importer.GetCompatibleWithAnyPlatform())
						importer.SetCompatibleWithAnyPlatform(false);
					importer.SetCompatibleWithPlatform(target, true);
					importer.SaveAndReimport();
				}
			}

			AssetDatabase.Refresh();
			AssetDatabase.SaveAssets();
		}

		#endregion

		private static Type FindComponent(KGComponent component)
		{
			var componentType = Assembly.GetExecutingAssembly()
				.GetTypes().Where(type => type.Name == "KG" + component.ToString())
				.FirstOrDefault();

			if (componentType == null)
			{
				Debug.LogError("Component not found : " + component.ToString());
				return null;
			}

			return componentType;
		}
		public static KGComponentBase GetComponent(KGComponent component)
		{
			var type = FindComponent(component);
			if(type != null)
			{
				return (KGComponentBase)Activator.CreateInstance(type);
			}
			return null;
		}

		/// <summary>
		/// BuildAPI 사용을 초기화한다.
		/// 모든 BuildAPI를 사용하기 전 반드시 Begin을 호출해야 한다. 
		/// </summary>
		/// <returns>성공 여부</returns>
		public static bool Begin() {
			if (KGSharedData.instance == null)
			{
				if (File.Exists(KGPackage.sharedDataPath) == false)
				{
					try {
						File.Copy(KGPackage.emptySharedDataPath, KGPackage.sharedDataPath);
					}
					catch(Exception e)
					{
						Debug.LogException(e);
						return false;
					}
				}

				KGSharedData.LoadSharedData(false);
			}

			return true;
		}

		/// <summary>
		/// 컴포넌트를 활성화하거나 비활성화한다.
		/// </summary>
		/// <param name="component">변경할 컴포넌트</param>
		/// <param name="enable">활성화 또는 비활성화</param>
		/// <returns>성공 여부</returns>
		public static bool EnableComponent(KGComponent component, bool enable) {
			var handler = GetComponent(component);

			if (handler.isAvaliable)
				Debug.LogWarning("Component(" + component.ToString() + ") not avaliable for current package.");

			if(enable)
				handler.OnEnable();
			else
				handler.OnDisable();

			return true;
		}

		/// <summary>
		/// 컴포넌트의 네이티브 라이브러리를 설치하거나 제거한다.
		/// </summary>
		/// <param name="component">변경할 컴포넌트</param>
		/// <param name="install">설치 또는 제거 여부</param>
		/// <returns>성공 여부</returns>
		public static bool InstallNativeLibrary(KGComponent component, bool install)
		{
			var handler = GetComponent(component);

			if (install)
				handler.OnInstallNativeLibrary();
			else
				handler.OnUninstallNativeLibrary();

			return true;
		}

		public static void SetupFromJsonConfig(string json)
		{
			Begin();
			Clean();

			var obj = (Dictionary<string, object>)JsonReader.Deserialize(json);
			foreach (var _val in (object[])obj["components"])
			{
				var val = (KGComponent)Enum.Parse(typeof(KGComponent), (string)_val);
				KGBuildAPI.EnableComponent(val, true);
			}
			foreach (var _val in (object[])obj["native_deps"])
			{
				var val = (KGComponent)Enum.Parse(typeof(KGComponent), (string)_val);
				KGBuildAPI.InstallNativeLibrary(val, true);
			}

			var rawConfig = (Dictionary<string, object>)obj["config"];
			var config = KGBuildAPI.configuration;
			config.appId = (string)rawConfig["app_id"];
			config.appSecret = (string)rawConfig["app_secret"];
			config.appVersion = (string)rawConfig["app_version"];
			config.market = (string)rawConfig["market"];
			config.debugLevel = (KGConfiguration.KGDebugLevel)Enum.Parse(typeof(KGConfiguration.KGDebugLevel), (string)rawConfig["debug_level"]);
			config.serverType = (string)rawConfig["server_type"];

			Apply();
		}

		/// <summary>
		/// 모든 컴포넌트를 삭제하고,
		/// 설정을 초기값으로 되돌립니다.
		/// </summary>
		public static void Clean()
		{
			if (File.Exists(KGPackage.sharedDataPath))
				File.Delete(KGPackage.sharedDataPath);

			foreach(var _val in Enum.GetValues(typeof(KGComponent)))
			{
				var val = (KGComponent)_val;

				KGBuildAPI.EnableComponent(val, false);
				KGBuildAPI.InstallNativeLibrary(val, false);
			}

			RefreshAssetDatabase();
		}

		private static void GenerateXmlFromGoogleJson()
		{
			System.Diagnostics.Process p = new System.Diagnostics.Process();
			p.StartInfo.FileName = "python";
			p.StartInfo.Arguments = "generate_xml_from_google_services_json.py";

			// Pipe the output to itself - we will catch this later
			p.StartInfo.RedirectStandardError = true;
			p.StartInfo.RedirectStandardOutput = true;
			p.StartInfo.CreateNoWindow = true;

			// Where the script lives
			p.StartInfo.WorkingDirectory = "Assets/Plugins/Editor/";
			p.StartInfo.UseShellExecute = false;

			p.Start();
			// Read the output - this will show is a single entry in the console - you could get  fancy and make it log for each line - but thats not why we're here
			//Debug.Log( p.StandardOutput.ReadToEnd() );
			p.WaitForExit();
			p.Close();
		}

		// 3.4.0 이전 리소스 구조 제거
		// 3.4.0 ~ 3.6.6 리소스 구조 제거
		private static void CleanOldAndroidPackage()
		{
			// 3.4.0 이전 리소스 구조 제거
			if (Directory.Exists("Assets/Plugins/Android/android-support"))
				Directory.Delete("Assets/Plugins/Android/android-support", true);

			if (Directory.Exists("Assets/Plugins/Android/facebook"))
				Directory.Delete("Assets/Plugins/Android/facebook", true);

			if (Directory.Exists("Assets/Plugins/Android/google-play-services_lib"))
				Directory.Delete("Assets/Plugins/Android/google-play-services_lib", true);

			if (Directory.Exists("Assets/Plugins/Android/kakao_android_sdk_for_eclipse"))
				Directory.Delete("Assets/Plugins/Android/kakao_android_sdk_for_eclipse", true);

			if (Directory.Exists("Assets/Plugins/Android/KakaoGameSDK"))
				Directory.Delete("Assets/Plugins/Android/KakaoGameSDK", true);

			if (Directory.Exists("Assets/Plugins/Android/res"))
				Directory.Delete("Assets/Plugins/Android/res", true);

			// 3.4.0 ~ 3.6.6 리소스 구조 제거
			//if (Directory.Exists("Assets/FacebookSDK/Plugins/Android/libs"))
			//	Directory.Delete("Assets/FacebookSDK/Plugins/Android/libs", true);

			if (Directory.Exists("Assets/Kakaogame/AndroidPlugins/google-play-services_lib.plugin"))
				Directory.Delete("Assets/Kakaogame/AndroidPlugins/google-play-services_lib.plugin", true);

			if (Directory.Exists("Assets/Kakaogame/AndroidPlugins/kakao_android_sdk_for_eclipse.plugin"))
				Directory.Delete("Assets/Kakaogame/AndroidPlugins/kakao_android_sdk_for_eclipse.plugin", true);

			if (Directory.Exists("Assets/Kakaogame/AndroidPlugins/KakaoGameSDK.libs"))
				Directory.Delete("Assets/Kakaogame/AndroidPlugins/KakaoGameSDK.libs", true);

			if (Directory.Exists("Assets/Kakaogame/AndroidPlugins/KakaoGameSDK.plugin"))
			{
				if (Directory.Exists("Assets/Kakaogame/AndroidPlugins/KakaoGameSDK.plugin/assets"))
					Directory.Delete("Assets/Kakaogame/AndroidPlugins/KakaoGameSDK.plugin/assets", true);

				if (Directory.Exists("Assets/Kakaogame/AndroidPlugins/KakaoGameSDK.plugin/res"))
					Directory.Delete("Assets/Kakaogame/AndroidPlugins/KakaoGameSDK.plugin/res", true);
			}
		}

		/// <summary>
		/// 모든 변경 사항을 저장하여 반영한다.
		/// </summary>
		public static void Apply() {
			SaveSharedData();
			RefreshAssetDatabase();
			//CleanOldAndroidPackage();

			if (Directory.Exists ("Assets/Kakaogame/AndroidPlugins")) {
				/* MANIFEST */
				var manifest = new AndroidManifest ();
#if UNITY_5_6_OR_NEWER
				//manifest.packageName = PlayerSettings.applicationIdentifier;
				manifest.packageName = UnityEngine.Application.identifier;
#else
				manifest.packageName = UnityEngine.Application.bundleIdentifier;
#endif
				manifest.forKakaoClientId = KGSharedData.Configuration.appSecret;
				manifest.permissions.permissions = KGSharedData.Permissions;
				// Configuration의 프로퍼티 중 Use prefix를 가지고 있고, true 설정된 것들을 찾는다.
				manifest.platforms = ";";

				// install refferer receiver
				manifest.receiverNames = KGSharedData.instance.etcSDKConfiguration.receiverNames;
				manifest.receiverValues = KGSharedData.instance.etcSDKConfiguration.receiverValues;

				for (int i = 0; i < manifest.receiverNames.Count; i++) {

				}

				//if (string.Compare (KGSharedData.Configuration.market, "kakaogameShop", true) == 0 || KGPackage.isSdkDevelop == true)
				manifest.platforms = ";kakaogameshop;";
				typeof(KGSharedData).GetFields (BindingFlags.Static | BindingFlags.Public)
					.Where (m => m.Name.StartsWith ("Use"))
					.Where (m => (bool)m.GetValue (null))
					.Select (m => { 
						return manifest.platforms += m.Name.ToLower () + ";";
					}).ToArray (); // 단순 쿼리 실행용

				if (KGSharedData.instance.customManifest == false) {
					if (File.Exists (androidSDKManifestFilePath) == false) {
						using (FileStream fs = File.Create (androidSDKManifestFilePath)) {
						}
					}
					//File.WriteAllText (androidSDKManifestFilePath, manifest.TransformText ());
					Component.KGURLInjector.InjectKakaoManifest ();
				}

				/* IDP INFO */
				Directory.CreateDirectory ("Assets/Kakaogame/AndroidPlugins/KakaoGameSDK.plugin/res/");
				Directory.CreateDirectory ("Assets/Kakaogame/AndroidPlugins/KakaoGameSDK.plugin/res/values/");
				if (KGSharedData.Permissions != null) {
					var values = new ZinnyValues ();
					values.permissions = KGSharedData.Permissions;
					File.WriteAllText (androidSDKValuesFilePath, values.TransformText ());
					AssetDatabase.ImportAsset (androidSDKValuesFilePath);
				}
				if (KGSharedData.UseKakao) {
					var kakao = new IDPReachKakao ();
					kakao.appKey = KGSharedData.Configuration.appSecret.SafeString ();
					File.WriteAllText (androidSDKKakaoAuthFilePath, kakao.TransformText ());
					AssetDatabase.ImportAsset (androidSDKKakaoAuthFilePath);
				}
				if (KGSharedData.UseGoogle) {
					var google = new IDPGoogle ();
					google.appId = KGSharedData.Google.webappClientId.Split ('-') [0].SafeString ();
					google.webappClientId = KGSharedData.Google.webappClientId.SafeString ();
					google.permissions = KGSharedData.Google.permissions;
					File.WriteAllText (androidSDKGoogleAuthFilePath, google.TransformText ());
					AssetDatabase.ImportAsset (androidSDKGoogleAuthFilePath);
				}
				if (KGSharedData.UseFacebook) {
					var facebook = new IDPFacebook ();
					facebook.appKey = KGSharedData.Facebook.appKey.SafeString ();
					facebook.permissions = KGSharedData.Facebook.permissions;
					File.WriteAllText (androidSDKFacebookAuthFilePath, facebook.TransformText ());
					AssetDatabase.ImportAsset (androidSDKFacebookAuthFilePath);
				}

				if (File.Exists ("Assets/Kakaogame/AndroidPlugins/KakaoGameSDK.plugin/.project") == false) {
					FileUtil.CopyFileOrDirectory (KGPackage.projectPath, "Assets/Kakaogame/AndroidPlugins/KakaoGameSDK.plugin/.project");
				}

				/* CONFIGS */
				var configTemplate = new ZinnyConfigurationAndroid ();
				Directory.CreateDirectory ("Assets/Plugins/Android/assets/");
				configTemplate.config.appId = KGSharedData.Configuration.appId.SafeString ();
				configTemplate.config.appSecret = KGSharedData.Configuration.appSecret.SafeString ();
				configTemplate.config.appVersion = RefineVersion (PlayerSettings.bundleVersion);
				configTemplate.config.market = KGSharedData.Configuration.market.SafeString ().ToLowerFirstChar ();
				configTemplate.config.debugLevel = KGSharedData.Configuration.debugLevel.ToString ().ToLower ();    
				configTemplate.config.serverType = KGSharedData.Configuration.serverType.SafeString ().ToLower ();
				File.WriteAllText (androidSDKConfigurationFilePath, configTemplate.TransformText ());
				AssetDatabase.ImportAsset (androidSDKConfigurationFilePath);
			}

			// KakaoGame SDK requires > iOS 8.0
			if (PlayerSettings.iOS.targetOSVersion < iOSTargetOSVersion.iOS_8_0)
				PlayerSettings.iOS.targetOSVersion = iOSTargetOSVersion.iOS_8_0;

#if UNITY_ANDROID
			GenerateXmlFromGoogleJson();
#endif

			ApplyPlugins ();
		}

		public static void ApplyPlugins()
		{
			SetPluginCompatibleIdp (BuildTarget.Android, androidPluginPath, true);
			SetPluginCompatibleIdp (BuildTarget.iOS, iOSPluginPaths, true);

			// google play
			//SetPluginCompatibleIdp (BuildTarget.Android, androidGooglePlayPluginPath, true);	// google play service library is always link to android
			//SetPluginCompatibleIdp (BuildTarget.Android, androidGooglePlayIdpPath, KGSharedData.UsePush | KGSharedData.UseGoogle);

			SetPluginCompatibleIdp (BuildTarget.iOS, iOSGooglePlayPluginPath, KGSharedData.UseGoogle);
			SetPluginCompatibleIdp (BuildTarget.iOS, iOSGooglePlayIdpPath, KGSharedData.UseGoogle);


			// facebook
			SetPluginCompatibleIdp (BuildTarget.Android, androidFacebookPluginPaths, KGSharedData.UseFacebook);

			SetPluginCompatibleIdp (BuildTarget.iOS, iOSFacebookPluginPaths, KGSharedData.UseFacebook);
			SetPluginCompatibleIdp (BuildTarget.iOS, iOSFacebookIdpPath, KGSharedData.UseFacebook);
			//			SetPluginCompatibleIdp (BuildTarget.iOS, iOSEditorFacebookPluginPath, KGSharedData.UseFacebook);

			//			SetPluginCompatibleEditor (editorFacebookPluginPath, KGSharedData.UseFacebook);
		}

		private static void SetPluginCompatibleIdp(BuildTarget target, string[] paths, bool enable)
		{
			foreach (string path in paths) {
				SetPluginCompatibleIdp (target, path, enable);
			}
		}

		private static void SetPluginCompatibleIdp(BuildTarget target, string path, bool enable)
		{
			//			if (File.Exists (path) == true || Directory.Exists (path) == true) {
			//				FileAttributes attr = File.GetAttributes (path);
			//				string[] pathSplit = path.Split ('/');
			//				string name = pathSplit [pathSplit.Length - 1];
			//				if ((attr & FileAttributes.Directory) == FileAttributes.Directory && name.Contains (".") == false) {
			//					Debug.LogError ("directory : " + path);   
			//					string[] directoryPaths = Directory.GetDirectories (path, "*", SearchOption.TopDirectoryOnly);
			//					foreach (string directoryPath in directoryPaths) {
			//						SetPluginCompatibleIdp (target, directoryPath, enable);
			//					}
			//				} else {
			//					Debug.LogError ("file : " + path); 
			if (File.Exists (path) == true || Directory.Exists (path) == true) {
				PluginImporter pluginImporter = PluginImporter.GetAtPath (path) as PluginImporter;
				if (pluginImporter != null) {
					if (pluginImporter.GetCompatibleWithAnyPlatform () == true) {
						pluginImporter.SetCompatibleWithAnyPlatform (false);
					}
					if (pluginImporter.GetCompatibleWithPlatform (target) != enable) {
						pluginImporter.SetCompatibleWithPlatform (target, enable);
						pluginImporter.SaveAndReimport ();
					}
				}
			}
			//				}
			//			}
		}

		private static void SetPluginCompatibleEditor(string path, bool enable)
		{
			if (File.Exists (path) == true || Directory.Exists (path) == true) {
				PluginImporter pluginImporter = PluginImporter.GetAtPath (path) as PluginImporter;
				if (pluginImporter != null) {
					pluginImporter.SetCompatibleWithAnyPlatform (false);
					if (pluginImporter.GetCompatibleWithEditor () != enable) {
						pluginImporter.SetCompatibleWithEditor (enable);
						pluginImporter.SaveAndReimport ();
					}
				}
			}
		}

		private static void SetPluginCompatibleAnyPlatform(string[] paths, bool enable)
		{
			foreach (string path in paths) {
				SetPluginCompatibleAnyPlatform (path, enable);
			}
		}

		private static void SetPluginCompatibleAnyPlatform(string path, bool enable)
		{
			if (File.Exists (path) == true || Directory.Exists (path) == true) {
				PluginImporter pluginImporter = PluginImporter.GetAtPath (path) as PluginImporter;
				if (pluginImporter != null) {
					if (pluginImporter.GetCompatibleWithAnyPlatform () != enable) {
						pluginImporter.SetCompatibleWithAnyPlatform (enable);
						pluginImporter.SaveAndReimport ();
					}
				}
			}
		}

		public static string VersionString(string s)
		{
			int number;
			if (int.TryParse(s, out number))
				return number.ToString();

			return "0";
		}

		public static string RefineVersion(string oldVersion)
		{
			string[] verItems = oldVersion.Split('.');
			if (verItems.Length > 2)
			{
				return VersionString(verItems[0]) + "." + VersionString(verItems[1]) + "." + VersionString(verItems[2]);
			}
			else if (verItems.Length == 2)
			{
				return VersionString(verItems[0]) + "." + VersionString(verItems[1]) + ".0";
			}
			else if (verItems.Length == 1)
			{
				return VersionString(verItems[0])  + ".0.0";
			}

			return "0.0.0";
		}

		public static void RegenerateAndroidManifest()
		{
		}

		private static void SaveSharedData()
		{
			var fs = File.Create(KGPackage.sharedDataPath);
			var sb = new StringBuilder();

			JsonWriter writer = new JsonWriter(sb);
			writer.Settings.PrettyPrint = true;

			writer.Write((object)KGSharedData.instance);

			var data = Encoding.UTF8.GetBytes(sb.ToString());

			fs.Write(data, 0, data.Length);

			fs.Close();
		}

		/// <summary>
		/// 변경 내용을 유니티가 다시 로드하도록 합니다.
		/// </summary>
		private static void RefreshAssetDatabase()
		{
			try
			{
				AssetDatabase.Refresh();
			}
			catch { }
		}
	}
}
#endif
