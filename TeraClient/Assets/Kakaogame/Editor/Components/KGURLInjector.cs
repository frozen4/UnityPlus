#if PLATFORM_KAKAO
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using System.Xml.Linq;

namespace Kakaogame.SDK.Editor.Component
{
	class KGURLInjector
	{
		private static readonly string schemePrefix = "kakaogame";
		// for testing

		private static void InjectIntentFilter(XmlDocument xml, XmlNode activity)
		{
			List<XmlNode> deleteIntentFilters = new List<XmlNode>();

			XmlNodeList intentFilters = activity.SelectNodes("intent-filter");
			foreach (XmlNode intentFilter in intentFilters)
			{
				var data = intentFilter.SelectSingleNode("data");

				try {
					if (data != null)
					{
						int number = 0;
						if (data.Attributes["android:scheme"].Value.StartsWith(schemePrefix) 
							&& int.TryParse(data.Attributes["android:scheme"].Value.Substring(schemePrefix.Length, 1), out number))
						{
							//activity.RemoveChild(intentFilter);
							deleteIntentFilters.Add(intentFilter);
						}
						else if (data.Attributes["android:scheme"].Value.StartsWith("@string/kakao_scheme"))
						{
							//activity.RemoveChild(intentFilter);
							deleteIntentFilters.Add(intentFilter);
						}
					}
				}
				catch(Exception e)
				{ 
				}
			}

			foreach (XmlNode deleteIntentFilter in deleteIntentFilters)
			{
				activity.RemoveChild(deleteIntentFilter);
			}

			/*
            <intent-filter>
               <action android:name="android.intent.action.VIEW" />
               <category android:name="android.intent.category.DEFAULT" />
               <category android:name="android.intent.category.BROWSABLE" />
               <data android:scheme="kakaogame[AppId]" />
            </intent-filter>
            */
			var newIntentFilter = xml.CreateElement("intent-filter");
			var actionNode = xml.CreateElement("action");
			var categoryDefault = xml.CreateElement("category");
			var categoryBrowsable = xml.CreateElement("category");
			var dataNode = xml.CreateElement("data");

			var application = xml.GetElementsByTagName("application")[0];
			var ns = application.GetNamespaceOfPrefix("android");

			application.Attributes["android:theme"].Value = "@style/UnityThemeSelector";
			//Create a new attribute. for android:hardwareAccelerated
			XmlNode hwaccAttr = xml.CreateNode(XmlNodeType.Attribute, "hardwareAccelerated", ns);
			hwaccAttr.Value = "true";
			application.Attributes.SetNamedItem (hwaccAttr);

			// Create a new attribute. for android:name

			//XmlNode nameAttr = xml.CreateNode(XmlNodeType.Attribute, "name", ns);
			//nameAttr.Value = "com.kakaogame.KGApplication";
			//application.Attributes.SetNamedItem (nameAttr);

			// set main activity for url promotion
			//var mainActivity = xml.GetElementsByTagName("activity")[0];
			//if (KGSharedData.instance.urlPromotionConfiguration.useURLPromotion)
			//	mainActivity.Attributes["android:name"].Value = "com.kakaogame.KGUnityPlayerActivity";
			//else
			//	mainActivity.Attributes["android:name"].Value = "com.unity3d.player.UnityPlayerActivity";

			actionNode.SetAttribute("name", ns, "android.intent.action.VIEW");
			categoryDefault.SetAttribute("name", ns, "android.intent.category.DEFAULT");
			categoryBrowsable.SetAttribute("name", ns, "android.intent.category.BROWSABLE");
			dataNode.SetAttribute("scheme", ns, schemePrefix + KGSharedData.Configuration.appId);

			newIntentFilter.AppendChild(actionNode);
			newIntentFilter.AppendChild(categoryDefault);
			newIntentFilter.AppendChild(categoryBrowsable);
			newIntentFilter.AppendChild(dataNode);

			activity.AppendChild(newIntentFilter);

			/*
			<intent-filter>
   				<action android:name="android.intent.action.VIEW" />
   				<category android:name="android.intent.category.DEFAULT" />
				<category android:name="android.intent.category.BROWSABLE" />
				<data android:scheme="@string/kakao_scheme" />
			</intent-filter>
			*/
			newIntentFilter = xml.CreateElement("intent-filter");
			actionNode = xml.CreateElement("action");
			categoryDefault = xml.CreateElement("category");
			categoryBrowsable = xml.CreateElement("category");
			dataNode = xml.CreateElement("data");

			actionNode.SetAttribute("name", ns, "android.intent.action.VIEW");
			categoryDefault.SetAttribute("name", ns, "android.intent.category.DEFAULT");
			categoryBrowsable.SetAttribute("name", ns, "android.intent.category.BROWSABLE");
			dataNode.SetAttribute("scheme", ns, "@string/kakao_scheme");

			newIntentFilter.AppendChild(actionNode);
			newIntentFilter.AppendChild(categoryDefault);
			newIntentFilter.AppendChild(categoryBrowsable);
			newIntentFilter.AppendChild(dataNode);

			activity.AppendChild(newIntentFilter);

			/*
			<intent-filter>
   				<action android:name="android.intent.action.VIEW" />
   				<category android:name="android.intent.category.DEFAULT" />
				<category android:name="android.intent.category.BROWSABLE" />
				<data android:scheme="@string/kakao_scheme" android:host="@string/kakaolink_host" />
			</intent-filter>
			*/
			newIntentFilter = xml.CreateElement("intent-filter");
			actionNode = xml.CreateElement("action");
			categoryDefault = xml.CreateElement("category");
			categoryBrowsable = xml.CreateElement("category");
			dataNode = xml.CreateElement("data");

			actionNode.SetAttribute("name", ns, "android.intent.action.VIEW");
			categoryDefault.SetAttribute("name", ns, "android.intent.category.DEFAULT");
			categoryBrowsable.SetAttribute("name", ns, "android.intent.category.BROWSABLE");
			dataNode.SetAttribute("scheme", ns, "@string/kakao_scheme");
			dataNode.SetAttribute("host", ns, "@string/kakaolink_host");

			newIntentFilter.AppendChild(actionNode);
			newIntentFilter.AppendChild(categoryDefault);
			newIntentFilter.AppendChild(categoryBrowsable);
			newIntentFilter.AppendChild(dataNode);

			activity.AppendChild(newIntentFilter);

			/*
			<intent-filter>
   				<action android:name="android.intent.action.VIEW" />
   				<category android:name="android.intent.category.DEFAULT" />
				<category android:name="android.intent.category.BROWSABLE" />
				<data android:scheme="@string/kakao_scheme" android:host="@string/kakaostory_host" />
			</intent-filter>
			*/
			newIntentFilter = xml.CreateElement("intent-filter");
			actionNode = xml.CreateElement("action");
			categoryDefault = xml.CreateElement("category");
			categoryBrowsable = xml.CreateElement("category");
			dataNode = xml.CreateElement("data");

			actionNode.SetAttribute("name", ns, "android.intent.action.VIEW");
			categoryDefault.SetAttribute("name", ns, "android.intent.category.DEFAULT");
			categoryBrowsable.SetAttribute("name", ns, "android.intent.category.BROWSABLE");
			dataNode.SetAttribute("scheme", ns, "@string/kakao_scheme");
			dataNode.SetAttribute("host", ns, "@string/kakaostory_host");

			newIntentFilter.AppendChild(actionNode);
			newIntentFilter.AppendChild(categoryDefault);
			newIntentFilter.AppendChild(categoryBrowsable);
			newIntentFilter.AppendChild(dataNode);

			activity.AppendChild(newIntentFilter);

			if (KGPackage.isPublishingGame)
			{
				XmlNodeList metaData = application.SelectNodes("meta-data");
				bool findSkipPermission = false;
				foreach (XmlNode child in metaData)
				{
					if (child.Attributes["android:name"].Value.Equals("unityplayer.SkipPermissionsDialog") == true)
					{
						findSkipPermission = true;
						break;
						//application.RemoveChild(child);
					}
				}

				if (findSkipPermission == false)
				{
					var skipPermissionsDialog = xml.CreateElement("meta-data");
					skipPermissionsDialog.SetAttribute("name", ns, "unityplayer.SkipPermissionsDialog");
					skipPermissionsDialog.SetAttribute("value", ns, "true");
					application.AppendChild(skipPermissionsDialog);
				}
			}
		}

		public static void InjectKakaoManifest()
		{
			var androidPluginPath = "Assets/Plugins/Android";
			if (Directory.Exists (androidPluginPath) == false) {
				Directory.CreateDirectory (androidPluginPath);
				UnityEditor.AssetDatabase.ImportAsset (androidPluginPath);
			}
			var manifestPath = androidPluginPath + "/AndroidManifest.xml";
			if (File.Exists(manifestPath) == false)
				File.Copy(KGPackage.defaultManifestPath, manifestPath);

			var xml = new XmlDocument();
			xml.Load(manifestPath);

			var activities = xml.GetElementsByTagName("activity");

			foreach (XmlNode activity in activities)
			{
				var activityName = activity.Attributes["android:name"].Value;
				var intentFilters = activity.SelectNodes("intent-filter");

				foreach (XmlNode intentFilter in intentFilters)
				{
					foreach (XmlNode action in intentFilter.SelectNodes("action"))
					{
						// find main activity
						if (action.Attributes["android:name"] != null &&
							action.Attributes["android:name"].Value == "android.intent.action.MAIN")
						{
							InjectIntentFilter(xml, activity);
						}
					}
				}
			}

//			foreach (string permission in permissions)
//			{
//				var usePermissionNode = xml.CreateElement("uses-permission");
//				usePermissionNode.SetAttribute("name", "android", permission);
//				manifest.AppendChild(usePermissionNode);
//			}

			if (KGSharedData.instance != null && KGSharedData.UseFacebook == false) {
				DeleteFacebookNodes (xml);
			}

			xml.Save(manifestPath);
			UnityEditor.AssetDatabase.ImportAsset (manifestPath);
		}

		private static void DeleteNodeWithName(XmlDocument xml, string type, string nameContains)
		{
			List<XmlNode> deleteNodeList = new List<XmlNode> ();

			XmlNodeList nodes = xml.GetElementsByTagName(type);
			foreach (XmlNode node in nodes) {
				string nodeName = node.Attributes["android:name"].Value;
				if (nodeName.ToLower ().Contains(nameContains)) {
					deleteNodeList.Add (node);
				}
			}

			foreach (XmlNode node in deleteNodeList) {
				if (node != null) {
					node.ParentNode.RemoveChild (node);
				}
			}
		}

		private static void DeleteFacebookNodes(XmlDocument xml)
		{
			DeleteNodeWithName(xml, "activity", "com.facebook");
			DeleteNodeWithName(xml, "meta-data", "com.facebook");
			DeleteNodeWithName(xml, "provider", "com.facebook");
		}
	}
}
#endif
