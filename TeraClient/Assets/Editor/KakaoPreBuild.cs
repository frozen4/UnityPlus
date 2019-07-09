using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.IO;
using System.Xml;

#pragma warning disable 0414

public class KakaoPreBuild
{
    #region Start
    private static readonly string _Permission = "permission.C2D_MESSAGE";
    private static readonly string _ReceiverName = "com.kakaogame.push.KGPushBroadcastReceiver";
    internal static void Start()
    {
        SetManifestBID();
    }

    //Set Bundle Identify to Kakao stuff in AndroidManifest
    private static void SetManifestBID()
    {
        PlayerSettings.applicationIdentifier = "com.kakaogames.tera";
#if UNITY_ANDROID
        string manifestPath = BuildTools.TopLevelManifestFilePath;

        XmlDocument doc = new XmlDocument();
        doc.Load(manifestPath);
        if (doc == null)
        {
            Debug.LogErrorFormat("[Kakao]Set Manifest failed, Could not open {0}", manifestPath);
            return;
        }
        // Set to permission
        {
            XmlNodeList permissionNodes = doc.GetElementsByTagName("uses-permission");
            for (int i = 0; i < permissionNodes.Count; i++)
            {
                XmlNode node = permissionNodes[i];
                string nodeName = node.Attributes["android:name"].Value;
                if (nodeName.Contains(_Permission))
                {
                    node.Attributes["android:name"].Value = string.Format("{0}.{1}", PlayerSettings.applicationIdentifier, _Permission);

                    Debug.Log("[Kakao]Set Bundle Identify to permission in AndroidManifest");
                    break;
                }
            }
        }
        /*
        // Set to category in receiver
        {
            XmlNodeList receiverNodes = doc.GetElementsByTagName("receiver");
            for (int i = 0; i < receiverNodes.Count; i++)
            {
                XmlNode receiverNode = receiverNodes[i];
                string nodeName = receiverNode.Attributes["android:name"].Value;
                if (nodeName.Equals(_ReceiverName))
                {
                    XmlNode filterNode = receiverNode.SelectSingleNode("intent-filter");
                    XmlNode categoryNode = filterNode.SelectSingleNode("category");
                    categoryNode.Attributes["android:name"].Value = PlayerSettings.applicationIdentifier;

                    Debug.Log("[Kakao]Set Bundle Identify to category in AndroidManifest");
                    break;
                }
            }
        }
        */
        doc.Save(manifestPath);
#endif
    }
    #endregion

    #region Disable
    private static readonly string[] _AllFolders =
    {
        "FacebookSDK",
        "Kakaogame",
        "Clean Setting UI",
        "Resources/Kakaogame",
        "Resources/KakaoSplash",
    };
    private static readonly string[] _FilesInPlugins =
    {
        "Plugins/Android/assets/kakao_game_sdk_configuration.xml",
        "Plugins/Android/google-services.json",
        "Plugins/Android/KGUnityPlayerActivity.jar",
        "Plugins/Android/libs/common-3.8.1.aar",
        "Plugins/Android/libs/gamesdk-3.8.1.aar",
        "Plugins/Android/libs/idp_device-3.8.1.aar",
        "Plugins/Android/libs/idp_facebook-3.8.1.aar",
        "Plugins/Android/libs/idp_googlegame-3.8.1.aar",
        "Plugins/Android/libs/idp_kakao-3.8.1.aar",
        "Plugins/Android/libs/kakaolib-3.8.1.aar",
        "Plugins/Editor/generate_xml_from_google_services_json.py",
    };
    private static readonly string _OriginAndroidManifestPath = Path.Combine(Application.dataPath, "../../SDK/Kakao/AndroidManifest_NoKakao/AndroidManifest.xml");
    private static readonly string _OriginGradlePath = Path.Combine(Application.dataPath, "../../SDK/Kakao/AndroidManifest_NoKakao/mainTemplate.gradle");
    private static readonly string _DestGradlePath = Path.Combine(Application.dataPath, "Plugins/Android/mainTemplate.gradle");

    public static void Disable()
    {
        DeleteSDKFolder();
        DeletePluginFiles();
        ReplaceAndroidManifest();
        ReplaceGradle();
    }

    private static void DeleteSDKFolder()
    {
        foreach (var folder in _AllFolders)
        {
            string folderPath = Path.Combine(Application.dataPath, folder);
            FolderUtil.Delete(folderPath);
            Debug.LogFormat("[Kakao]DeleteSDKFolder => Asset/{0} Succeed", folder);
        }
    }

    private static void DeletePluginFiles()
    {
        foreach (var file in _FilesInPlugins)
        {
            string filePath = Path.Combine(Application.dataPath, file);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                Debug.LogFormat("[Kakao]DeletePluginFiles => Asset/{0} Succeed", file);
            }
        }
    }

    private static void ReplaceAndroidManifest()
    {
#if UNITY_ANDROID
        if (!File.Exists(_OriginAndroidManifestPath))
        {
            Debug.LogFormat("[Kakao]ReplaceAndroidManifest failed, AndroidManifest.xml not exists, please check the path:\n{0}", _OriginAndroidManifestPath);
            return;
        }
        string destPath = BuildTools.TopLevelManifestFilePath;
        if (File.Exists(destPath))
            File.Delete(destPath);

        File.Copy(_OriginAndroidManifestPath, destPath);
        Debug.Log("[Kakao]ReplaceAndroidManifest Succeed");
#endif
    }

    private static void ReplaceGradle()
    {
#if UNITY_ANDROID
        if (!File.Exists(_OriginGradlePath))
        {
            Debug.LogFormat("[Kakao]ReplaceGradle failed, mainTemplate.gradle not exists, please check the path:\n{0}", _OriginGradlePath);
            return;
        }
        if (File.Exists(_DestGradlePath))
            File.Delete(_DestGradlePath);

        File.Copy(_OriginGradlePath, _DestGradlePath);
        Debug.Log("[Kakao]ReplaceGradle Succeed");
#endif
    }
    #endregion
}
