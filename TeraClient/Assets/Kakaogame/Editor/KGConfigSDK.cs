#if PLATFORM_KAKAO
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Diagnostics;
using System.Runtime.Serialization;

using Kakaogame.Base.Pathfinding.Serialization.JsonFx;

using UnityEngine;
using UnityEditor;

namespace Kakaogame.SDK.Editor
{
	using Kakaogame.SDK;
	using Kakaogame.SDK.Editor.Component;

	[Serializable]
	public class KGConfigSDK : UnityEditor.Editor
	{
		//// 실제 퍼미션 이름과 1:1 대응하는 이름
		//public enum AndroidPermission
		//{
		//    NONE,
		//    SEND_SMS,
		//    READ_SMS,
		//    RECEIVE_SMS,
		//    READ_EXTERNAL_STORAGE,
		//    WRITE_EXTERNAL_STORAGE,
		//    CAMERA,
		//    READ_CONTACTS,
		//    WRITE_CONTACTS,
		//    GET_ACCOUNTS
		//}

		public enum ServerType
		{
			ALPHA,
			REAL
		}

		private ServerType serverType;

		private static bool showConfiguration = true;
		private static bool showPushSetting = true;
		private static bool showIdpSetting = true;
		private static bool showEditorSetting = true;
		private static bool showEtcSetting = true;
		private static bool showUrlPromotionSetting = true;
		private static bool showSdkDevelopSetting = true;

		private static bool showExample = false;
		private static bool showErrorCode = false;
		private static bool showPermissions = false;
		private static bool showReceivers = true;

		private static bool showFacebookPcakageUI = true;
		private static bool showGooglePackageUI = true;

		private static KGGoogle google { get; set; }
		private static KGKakao kakao { get; set; }
		private static Component.KGPush push { get; set; }
		private static KGFacebook facebook { get; set; }

		const string facebookSDKSrcPath = "Assets/FacebookSDK";
		const string kakaoGameFacebookSDKSrcPath_ios = "Assets/Kakaogame/iOSPlugins/KakaoGameFramework/KakaoGameFacebook.framework";
		const string kakaoGameGoogleSDKSrcPath_ios = "Assets/Kakaogame/iOSPlugins/KakaoGameFramework/KakaoGameGoogle.framework";

		const string facebookPackagePath = "Assets/../FacebookPackage_Backup";
		const string facebookSDKDstPath = "Assets/../FacebookPackage_Backup/FacebookSDK";
		const string kakaoGameFacebookSDK_ios = "Assets/../FacebookPackage_Backup/iOS";
		const string kakaoGameFacebookSDKDstPath_ios = "Assets/../FacebookPackage_Backup/iOS/KakaoGameFacebook.framework";

		const string kakaoGameGoogleSDK_ios = "Assets/../GooglePackage_Backup/iOS";
		const string kakaoGameGoogleSDKDstPath_ios = "Assets/../GooglePackage_Backup/iOS/KakaoGameGoogle.framework";

		static KGConfigSDK()
		{
			google = new KGGoogle();
			push = new Component.KGPush();
			kakao = new KGKakao();
			facebook = new KGFacebook();
		}

		private static void DrawRegion(string msg)
		{
			var regionLabelStyle = new GUIStyle(GUI.skin.label);
			regionLabelStyle.fontSize = 15;
			regionLabelStyle.fontStyle = FontStyle.Bold;

			EditorGUILayout.Space();

			EditorGUILayout.TextArea("", GUI.skin.horizontalSlider);

			var lastRect = GUILayoutUtility.GetLastRect();
			lastRect.y -= 9;
			EditorGUI.LabelField(lastRect, msg, regionLabelStyle);
		}

		private bool checkConfigurationFailed = false;
		private string configurationFailReason = "";
		private bool showAppVersionTooltip = false;

		private void ConfigurationGUI()
		{
			showConfiguration = EditorGUILayout.Foldout(showConfiguration, "Configuration");
			if (showConfiguration)
			{
				EditorGUI.indentLevel++;

				try
				{
					var vdIdent = new Regex("^[a-zA-Z0-9_]+$");
					var vdVersion = new Regex("^[0-9\\.]+$");
					var config = KGSharedData.instance.configuration;
					var style = new GUIStyle();
					style.richText = true;

					var btnSkin = new GUIStyle(GUI.skin.button);
					btnSkin.richText = true;

					//DrawRegion("App");

					using (KGGUIHelper.DisableForSealed())
					{
						//var tfStyle = new GUIStyle(GUI.skin.textField);
						//tfStyle.fontSize = 14;
						//tfStyle.fixedHeight = 14;

						// App ID
						config.appId = EditorGUILayout.TextField("App Id", config.appId);
						if (string.IsNullOrEmpty(config.appId) || config.appId.Contains(" "))
							EditorGUILayout.LabelField("<color=\"yellow\">Missing Value</color>", style);
						if (config.appId != "" && !vdIdent.IsMatch(config.appId))
							EditorGUILayout.LabelField("<color=\"red\">Invalid Value</color>", style);

						// App Secret
						config.appSecret = EditorGUILayout.TextField("App Secret", config.appSecret);
						if (string.IsNullOrEmpty(config.appSecret) || config.appSecret.Contains(" "))
							EditorGUILayout.LabelField("<color=\"yellow\">Missing Value</color>", style);
						if (config.appSecret != "" && !vdIdent.IsMatch(config.appSecret))
							EditorGUILayout.LabelField("<color=\"red\">Invalid Value</color>", style);
					}

					// App Version
					if (!KGPackage.isGameShopPaymentPackage)
					{
						if (KGPackage.isPublishingGame)
						{
							PlayerSettings.bundleVersion = EditorGUILayout.TextField("App Version", PlayerSettings.bundleVersion);
							if (string.IsNullOrEmpty(PlayerSettings.bundleVersion) || PlayerSettings.bundleVersion.Contains(" "))
								EditorGUILayout.LabelField("<color=\"red\">Missing Value</color>", style);
							config.appVersion = KGBuildAPI.RefineVersion(PlayerSettings.bundleVersion);
							GUILayout.BeginHorizontal();
							GUILayout.FlexibleSpace();
							EditorGUILayout.LabelField(" * PlayerSettings.bundleVersion");
							GUILayout.EndHorizontal();
						}
						else
						{
							PlayerSettings.bundleVersion = "0.0.0";
							config.appVersion = "0.0.0";
						}
					}
					else
					{
						PlayerSettings.bundleVersion = "0.6.0";
						config.appVersion = "0.6.0";
					}

					EditorGUILayout.Space();

					// Market
					if (!KGPackage.isGameShopPaymentPackage)
					{
						if (KGPackage.isPublishingGame)
						{
							EditorGUILayout.BeginHorizontal();
							EditorGUILayout.LabelField("Market (Android)");
							config.isCustomMarket = EditorGUILayout.Toggle("Use custom market", config.isCustomMarket);
							EditorGUILayout.EndHorizontal();

							if (config.isCustomMarket == true)
							{
								config.market = EditorGUILayout.TextField("", config.market);
							}
							else
							{
								var googlePlayContent = new GUIContent("googlePlay", KGResources.googlePlay);
								var oneStoreContent = new GUIContent("oneStore", KGResources.oneStore);
								var kakaogameShopContent = new GUIContent("kakaogameShop", KGResources.kakaogameShop);

								if (config.market == "googlePlay")
								{
									googlePlayContent.text = "<b>" + googlePlayContent.text + " [√]</b>";
									googlePlayContent.image = KGResources.googlePlayActive;
								}
								else if (config.market == "oneStore")
								{
									oneStoreContent.text = "<b>" + oneStoreContent.text + " [√]</b>";
									oneStoreContent.image = KGResources.oneStoreActive;
								}
								else if (config.market == "kakaogameShop")
								{
									kakaogameShopContent.text = "<b>" + kakaogameShopContent.text + " [√]</b>";
									kakaogameShopContent.image = KGResources.kakaogameShopActive;
								}

								GUILayout.BeginHorizontal();
								GUILayout.FlexibleSpace();
								if (GUILayout.Button(googlePlayContent, btnSkin, GUILayout.Width(140), GUILayout.Height(44)))
									config.market = "googlePlay";
								if (GUILayout.Button(oneStoreContent, btnSkin, GUILayout.Width(140), GUILayout.Height(44)))
									config.market = "oneStore";
								if (GUILayout.Button(kakaogameShopContent, btnSkin, GUILayout.Width(140), GUILayout.Height(44)))
									config.market = "kakaogameShop";
								GUILayout.EndHorizontal();

								GUILayout.BeginHorizontal();
								GUILayout.FlexibleSpace();
								EditorGUILayout.LabelField("* iOS market is only appStore.");
								GUILayout.EndHorizontal();
							}
						}
						else
						{
							config.market = "googlePlay";
						}
					}
					else
					{
						config.market = "googlePlay";
					}

					// Server Type
					if (!KGPackage.isGameShopPaymentPackage)
					{
						if (KGPackage.isPublishingGame)
						{
							var qaContent = new GUIContent("QA", KGResources.qa);
							var realContent = new GUIContent("Real", KGResources.real);

							if (config.serverType == "qa")
							{
								qaContent.text = "<b>" + qaContent.text + " [√]</b>";
								qaContent.image = KGResources.qaActive;
							}
							else
							{
								realContent.text = "<b>" + realContent.text + " [√]</b>";
								realContent.image = KGResources.realActive;
							}

							EditorGUILayout.LabelField("Server Type");
							GUILayout.BeginHorizontal();
							GUILayout.FlexibleSpace();
							if (GUILayout.Button(qaContent, btnSkin, GUILayout.Width(140), GUILayout.Height(44)))
								config.serverType = "qa";
							if (GUILayout.Button(realContent, btnSkin, GUILayout.Width(140), GUILayout.Height(44)))
								config.serverType = "real";
							GUILayout.EndHorizontal();
							EditorGUILayout.Space();
						}
						else
						{
							config.serverType = "real";
						}
					}
					else
					{
						config.serverType = "alpha";
						serverType = (ServerType)EditorGUILayout.EnumPopup("Server Type", serverType);
						if (serverType == ServerType.ALPHA)
							config.serverType = "alpha";
						else if (serverType == ServerType.REAL)
							config.serverType = "real";
					}

					// Debug Level
					config.debugLevel = (KGConfiguration.KGDebugLevel)EditorGUILayout.EnumPopup("Debug Level", config.debugLevel);

					EditorGUILayout.Space();

					// Check
					DrawConfigurationUI();

					EditorGUILayout.Space();
				}
				catch (Exception e)
				{
					UnityEngine.Debug.LogException(e);
				}

				EditorGUI.indentLevel--;
			}
		}

		private void DrawConfigurationUI()
		{
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();

			if (GUILayout.Button("Check configuration setting"))
			{
				string infodeskUrl = "";

				if (string.Equals(KGSharedData.Configuration.serverType, "real", StringComparison.OrdinalIgnoreCase))
					infodeskUrl = "https://infodesk-zinny3.game.kakao.com/v2/app";
				else if (string.Equals(KGSharedData.Configuration.serverType, "qa", StringComparison.OrdinalIgnoreCase))
					infodeskUrl = "https://qa-infodesk-zinny3.game.kakao.com/v2/app";
				else if (string.Equals(KGSharedData.Configuration.serverType, "sandbox", StringComparison.OrdinalIgnoreCase))
					infodeskUrl = "https://sb-infodesk-zinny3.game.kakao.com/v2/app";
				else if (string.Equals(KGSharedData.Configuration.serverType, "alpha", StringComparison.OrdinalIgnoreCase))
					infodeskUrl = "https://alpha-infodesk-zinny3.game.kakao.com/v2/app";

				var www = new WWW(infodeskUrl + "?appId=" + KGSharedData.Configuration.appId +
					"&appVer=" + KGSharedData.Configuration.appVersion +
					"&market=" + KGSharedData.Configuration.market.ToLowerFirstChar() +
					"&sdkVer=" + KGPackage.version +
					"&os=android&lang=kr&deviceId=tempdeviceid");

				UnityEngine.Debug.Log(www.url);

				while (www.isDone == false)
					;

				var obj = JsonReader.Deserialize(www.text);
				var sb = new StringBuilder();
				var writer = new JsonWriter(sb);

				writer.Settings.PrettyPrint = true;
				writer.Write(obj);

				UnityEngine.Debug.Log(sb.ToString());

				if (www.error != null)
				{
					checkConfigurationFailed = true;
					configurationFailReason = www.error;
					if (www.text != null)
						configurationFailReason = www.text;

					UnityEngine.Debug.LogError(configurationFailReason);
				}
				else
				{
					checkConfigurationFailed = false;
					var verStatus = ((Dictionary<string, object>)((Dictionary<string, object>)obj)["content"])["appVerStatus"];

					EditorUtility.DisplayDialog("KakaoGame Unity Configuration", "Test OK.\r\n\r\nVersionStatus : " + verStatus, "OK");
				}
			}
			GUILayout.EndHorizontal();

			if (checkConfigurationFailed)
				EditorGUILayout.HelpBox("Test Failed\r\n" + "Reason : " + configurationFailReason, MessageType.Error);
		}

		private void EditorSettingGUI()
		{
			showEditorSetting = EditorGUILayout.Foldout(showEditorSetting, "Editor Setting");

			if (showEditorSetting)
			{
				var config = KGSharedData.instance.editorSDKConfiguration;

				EditorGUI.indentLevel++;

				config.useCustomDeviceKey = EditorGUILayout.Toggle("Use Custom Device Id", config.useCustomDeviceKey);
				if (config.useCustomDeviceKey)
					config.deviceKey = EditorGUILayout.TextField(config.deviceKey);
				config.useKakaoEditorLogin = EditorGUILayout.Toggle("Use Kakao Editor Login", config.useKakaoEditorLogin);
				if (config.useKakaoEditorLogin)
					config.javascriptKey = EditorGUILayout.TextField(config.javascriptKey);

				EditorGUI.indentLevel--;
			}

			EditorGUILayout.Space();
		}

		private void EtcSettingGUI()
		{
			showEtcSetting = EditorGUILayout.Foldout(showEtcSetting, "Etc Setting (Use as needed)");

			if (showEtcSetting)
			{
				EditorGUI.indentLevel++;

				bool fold = true;
				// com.android.vending.INSTALL_REFERRER
				ShowReceiverList("INSTALL_REFERRER Receivers (Android)", KGSharedData.instance.etcSDKConfiguration.receiverNames, KGSharedData.instance.etcSDKConfiguration.receiverValues, ref showReceivers);

				ShowFacebookPackageGUI("Facebook Package", ref showFacebookPcakageUI);

				ShowGooglePackageGUI("Google Package", ref showGooglePackageUI);
				EditorGUI.indentLevel--;
			}

			EditorGUILayout.Space();
		}

		private void URLPromotionSettingGUI()
		{
			showUrlPromotionSetting = EditorGUILayout.Foldout(showUrlPromotionSetting, "URL Promotion Setting(Android)");

			if (showUrlPromotionSetting)
			{
				EditorGUI.indentLevel++;

				var config = KGSharedData.instance.urlPromotionConfiguration;

				config.useURLPromotion = EditorGUILayout.Toggle("Use URL Promotion", config.useURLPromotion);

				EditorGUI.indentLevel--;
			}

			EditorGUILayout.Space();
		}

		private void ShowReceiverList(string name, List<string> namelist, List<string> valuelist, ref bool fold)
		{
			fold = EditorGUILayout.Foldout(fold, name);
			if (fold)
			{
				var names = namelist;
				var values = valuelist;

				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.Space();
				if (GUILayout.Button("Add"))
				{
					names.Add("RECEIVER_NAME");
					values.Add("RECEIVER_VALUE");
				}
				EditorGUILayout.EndHorizontal();

				EditorGUI.indentLevel++;
				for (var i = 0; i < names.Count; i++)
				{
					EditorGUILayout.BeginHorizontal();

					names[i] = EditorGUILayout.TextField(names[i]);
					values[i] = EditorGUILayout.TextField(values[i]);
					if (GUILayout.Button("X"))
					{
						names.RemoveRange(i, 1);
						values.RemoveRange(i, 1);
					}

					EditorGUILayout.EndHorizontal();
				}
				EditorGUI.indentLevel--;
			}
		}

		private void SetScriptingDefineSymbols(BuildTargetGroup target, string symbol)
		{
			string defineSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(target);
			UnityEngine.Debug.Log("current define Symbol : " + defineSymbols);
			if (!defineSymbols.Contains(symbol))
				defineSymbols += ";" + symbol;
			PlayerSettings.SetScriptingDefineSymbolsForGroup(target, defineSymbols);
		}

		private void ReplaceScriptingDefineSymbols(BuildTargetGroup target, string original, string replace)
		{
			string defineSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(target);
			UnityEngine.Debug.Log("current define Symbol : " + defineSymbols);
			if (defineSymbols.Contains(original))
			{
				defineSymbols = defineSymbols.Replace(original, replace);
			}
			UnityEngine.Debug.Log("modified define Symbol : " + defineSymbols);
			PlayerSettings.SetScriptingDefineSymbolsForGroup(target, defineSymbols);
		}

		private void ShowFacebookPackageGUI(string name, ref bool fold)
		{
			fold = EditorGUILayout.Foldout(fold, name);
			if (fold)
			{
				EditorGUILayout.BeginVertical();

				EditorGUI.BeginDisabledGroup(!KGSharedData.instance.useFacebookPackage);
				if (GUILayout.Button("Remove Facebook Package"))
				{ 
					if (!Directory.Exists(facebookPackagePath))
						Directory.CreateDirectory(facebookPackagePath);
					Directory.Move(facebookSDKSrcPath, facebookSDKDstPath);

					if (!Directory.Exists(kakaoGameFacebookSDK_ios))
						Directory.CreateDirectory(kakaoGameFacebookSDK_ios);
					Directory.Move(kakaoGameFacebookSDKSrcPath_ios, kakaoGameFacebookSDKDstPath_ios);

					if (KGSharedData.instance.useFacebook == true)
						facebook.OnDisable();

					SetScriptingDefineSymbols(BuildTargetGroup.Android, "NO_USE_FACEBOOK");
					SetScriptingDefineSymbols(BuildTargetGroup.iOS, "NO_USE_FACEBOOK");

					KGSharedData.instance.useFacebookPackage = false;
				}
				EditorGUI.EndDisabledGroup();

				EditorGUI.BeginDisabledGroup(KGSharedData.instance.useFacebookPackage);
				if (GUILayout.Button("Revert Facebook Package"))
				{
					Directory.Move(facebookSDKDstPath, facebookSDKSrcPath);

					Directory.Move(kakaoGameFacebookSDKDstPath_ios, kakaoGameFacebookSDKSrcPath_ios);

					if (KGSharedData.instance.useFacebook == false)
						facebook.OnEnable();

					ReplaceScriptingDefineSymbols(BuildTargetGroup.Android, "NO_USE_FACEBOOK", "");
					ReplaceScriptingDefineSymbols(BuildTargetGroup.iOS, "NO_USE_FACEBOOK", "");

					KGSharedData.instance.useFacebookPackage = true;
				}
				EditorGUI.EndDisabledGroup();

				EditorGUILayout.EndVertical();
			}
		}

		private void ShowGooglePackageGUI(string name, ref bool fold)
		{
			fold = EditorGUILayout.Foldout(fold, name);
			if (fold)
			{
				EditorGUILayout.BeginVertical();
				EditorGUI.BeginDisabledGroup(!KGSharedData.instance.useGooglePackage);
				if (GUILayout.Button("Remove Google Package"))
				{
					if (!Directory.Exists(kakaoGameGoogleSDK_ios))
						Directory.CreateDirectory(kakaoGameGoogleSDK_ios);
					Directory.Move(kakaoGameGoogleSDKSrcPath_ios, kakaoGameGoogleSDKDstPath_ios);

					if (KGSharedData.instance.useGoogle == true)
						google.OnDisable();

					KGSharedData.instance.useGooglePackage = false;
				}
				EditorGUI.EndDisabledGroup();

				EditorGUI.BeginDisabledGroup(KGSharedData.instance.useGooglePackage);
				if (GUILayout.Button("Revert Google Package"))
				{
					Directory.Move(kakaoGameGoogleSDKDstPath_ios, kakaoGameGoogleSDKSrcPath_ios);

					if (KGSharedData.instance.useGoogle == false)
						google.OnEnable();

					KGSharedData.instance.useGooglePackage = true;
				}
				EditorGUI.EndDisabledGroup();

				EditorGUILayout.EndVertical();
			}
		}

		private void SdkDevelopSettingGUI()
		{
			if (KGPackage.isSdkDevelop == false)
				return;

			showSdkDevelopSetting = EditorGUILayout.Foldout(showSdkDevelopSetting, "SDK Develop Setting (for SDK Developer)");

			if (showSdkDevelopSetting)
			{
				var config = KGSharedData.instance.sdkDevelopConfiguration;

				EditorGUI.indentLevel++;

				// iOS TestApp Auto Set 
				config.useIOSTestAppAutoSetting = EditorGUILayout.Toggle("iOS TestApp Auto Set", config.useIOSTestAppAutoSetting);

				// Bundle ID
				#if UNITY_5_6_OR_NEWER
				EditorGUILayout.TextField("Bundle ID", PlayerSettings.applicationIdentifier);
				#else
				EditorGUILayout.TextField("Bundle ID", PlayerSettings.bundleIdentifier);
				#endif
				GUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				EditorGUILayout.LabelField(" * PlayerSettings.bundleIdentifier");
				GUILayout.EndHorizontal();

				EditorGUI.indentLevel--;
			}
		}

		private void PushSettingGUI()
		{
			showPushSetting = EditorGUILayout.Foldout(showPushSetting, "Push Setting(Android)");
			if (showPushSetting)
			{
				EditorGUI.indentLevel++;

				push.DrawInspector();

				EditorGUI.indentLevel--;
			}
		}

		private void IdpSettingGUI()
		{
			showIdpSetting = EditorGUILayout.Foldout(showIdpSetting, "IDP Setting");
			if (showIdpSetting)
			{
				EditorGUI.indentLevel++;

				EditorGUI.BeginDisabledGroup(!KGSharedData.instance.useGooglePackage);
				google.DrawInspector();
				EditorGUI.EndDisabledGroup();

				EditorGUI.BeginDisabledGroup(!KGSharedData.instance.useFacebookPackage);
				facebook.DrawInspector();
				EditorGUI.EndDisabledGroup();

				EditorGUI.indentLevel--;
			}
		}

		private DateTime lastChanged = DateTime.Now;
		private bool changeFlushed = true;

		public override void OnInspectorGUI()
		{
			if (KGSharedData.instance == null)
			{
				EditorGUILayout.LabelField("WARNING");

				EditorGUILayout.LabelField("Failed to load KGSharedData from current project.");
				EditorGUILayout.LabelField("Please make sure your project has 'KGSharedData.json' file on 'Assets/Resources/Kakaogame/' path.");
				EditorGUILayout.LabelField("");
				EditorGUILayout.LabelField("Possible solution : You can paste 'Assets/Kakaogame/EmptyKGSharedData.json' to 'Assets/Resources/Kakaogame/KGSharedData.json' to restore KGSharedData defaults.");
				EditorGUILayout.LabelField("    This action will reset all settings to default values.");

				return;
			}
			if (Application.isEditor && Application.isPlaying)
			{
				EditorGUILayout.HelpBox("You cannot modify SDK settings while game is playing.", MessageType.Warning);

				GUI.enabled = false;
			}

			bool lastGUIEnabled = GUI.enabled;
			GUI.enabled = !Application.isPlaying;
			if (!KGPackage.isGameShopPaymentPackage)
				EditorGUILayout.LabelField("KakaoGame Unity " + KGPackage.version.ToString());
			else
				EditorGUILayout.LabelField("KGGameShop Payment Unity 0.6.0");
			ConfigurationGUI();
			if (!KGPackage.isGameShopPaymentPackage)
			{
				PushSettingGUI();
				EditorGUILayout.Space();
				EditorGUILayout.Space();
				IdpSettingGUI();
				EditorSettingGUI();
				EtcSettingGUI();
				URLPromotionSettingGUI();
				SdkDevelopSettingGUI();

				EditorGUILayout.Space();
				EditorGUILayout.Space();
				GUI.enabled = lastGUIEnabled;

				if (PlayerSettings.iOS.targetOSVersion < iOSTargetOSVersion.iOS_8_0)
					EditorGUILayout.HelpBox("KakaoGameSDK requires `iOS.targetOSVersion >= 8.0`", MessageType.Error);

				if (PlayerSettings.Android.targetSdkVersion >= AndroidSdkVersions.AndroidApiLevel16 &&
					PlayerSettings.Android.targetSdkVersion <= AndroidSdkVersions.AndroidApiLevel25)
					EditorGUILayout.HelpBox("KakaoGameSDK requires `Android.targetSDKVersion >= 26 or Android.targetSDKVersion == AndroidApiLevelAuto`", MessageType.Error);
			}

			if (GUI.changed)
			{
				lastChanged = DateTime.Now;
				changeFlushed = false;
			}

			GUI.enabled = true;
		}

		public void Awake()
		{
			KGResources.Initialize();
		}

		public void Update()
		{
			if (changeFlushed == false &&
				DateTime.Now - lastChanged >= TimeSpan.FromSeconds(1))
			{
				try
				{
					KGBuildAPI.Apply();
				}
				catch(Exception e)
				{
					UnityEngine.Debug.LogError("KakaoGameSDK::ApplyChange Error\r\n\r\n" + e.ToString());
				}

				changeFlushed = true;
			}
		}
	}
}
#endif
