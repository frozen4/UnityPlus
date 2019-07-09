using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.IO;
using System.Xml;

#pragma warning disable 0414

public class LongtuPreBuild
{
    private static readonly string _PluginsSrcPath = Path.Combine(Application.dataPath, "../../SDK/Longtu/libs");
    private static readonly string _PluginsDestPath = Path.Combine(Application.dataPath, "Plugins/Android/libs");
    private static readonly string _PluginsResDestPath = Path.Combine(Application.dataPath, "Plugins/Android/res");
    private static readonly string _NoticeSDKPluginsSrcPath = Path.Combine(Application.dataPath, "../../SDK/NoticeSDK/libs");
    private static readonly string _NoticeSDKPluginsResPath = Path.Combine(Application.dataPath, "../../SDK/NoticeSDK/res");

    private static readonly string _ApplicationName = "com.bh.sdk.BHApplication";
    private static readonly string _MetaDataName = "LTGameID";
    private static readonly string _MetaDataValue = "500016";
    private static readonly string _MinSDKVersionValue = "18";
    private static readonly string _TargetSDKVersionValue = "22";

    #region Start
    private static readonly string[] _ConflictFolders =
    {
        "LGVC",
        "Plugins/UnityChannel",
        "Plugins/UnityPurchasing",
        //"Plugins/Android/answers",
        //"Plugins/Android/beta",
        //"Plugins/Android/crashlytics",
        //"Plugins/Android/crashlytics-wrapper",
        //"Plugins/Android/fabric",
        //"Plugins/Android/fabric-init",
    };
    private static readonly string[] _ConflictFilesInPlugins =
    {
        "Plugins/Android/libs/applauncher.aar",
        "Plugins/Android/libs/singular_sdk-7.4.0.aar",
        "Plugins/Android/libs/SingularUnityBridge.jar",
    };
    private static readonly string[] _LibFiles =
    {
        "applauncher.jar",
        "bhsdk7.5.1.jar",
        "noticesdk.jar",
    };

    public static void Start()
    {
        PlayerSettings.applicationIdentifier = "com.longtugame.tera";
        DeleteConflictFolder();
        DeleteConflictPluginFiles();
        CopyLibFiles();
        SetManifest();
    }

    private static void DeleteConflictFolder()
    {
        foreach (var folder in _ConflictFolders)
        {
            string folderPath = Path.Combine(Application.dataPath, folder);
            FolderUtil.Delete(folderPath);
            Debug.LogFormat("[Longtu]DeleteSDKFolder => Asset/{0} Succeed", folder);
        }
    }

    private static void DeleteConflictPluginFiles()
    {
        foreach (var file in _ConflictFilesInPlugins)
        {
            string filePath = Path.Combine(Application.dataPath, file);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                Debug.LogFormat("[Longtu]DeletePluginFiles => Asset/{0} Succeed", file);
            }
        }
    }

    private static void CopyLibFiles()
    {
        foreach (var file in _LibFiles)
        {
            string filePath = Path.Combine(_PluginsSrcPath, file);
            if (File.Exists(filePath))
            {
                string destFilePath = Path.Combine(_PluginsDestPath, file);
                if (File.Exists(destFilePath))
                    File.Delete(destFilePath);
                File.Copy(filePath, destFilePath);
                Debug.LogFormat("[Longtu]CopyLibFiles => {0} Succeed", destFilePath);
            }
        }

        // Notice SDK Lib
        foreach (var file in _LibFiles)
        {
            string filePath = Path.Combine(_NoticeSDKPluginsSrcPath, file);
            if (File.Exists(filePath))
            {
                string destFilePath = Path.Combine(_PluginsDestPath, file);
                if (File.Exists(destFilePath))
                    File.Delete(destFilePath);
                File.Copy(filePath, destFilePath);
                Debug.LogFormat("[Longtu]CopyLibFiles => {0} Succeed", destFilePath);
            }
        }

        // Notice SDK res
        FolderUtil.CopyAndReplaceSub(_NoticeSDKPluginsResPath, _PluginsResDestPath);
    }

    //修改AndroidManifest的application的name
    private static void SetManifest()
    {
#if UNITY_ANDROID
        XmlDocument doc = new XmlDocument();
        string manifestPath = BuildTools.TopLevelManifestFilePath;
        doc.Load(manifestPath);
        if (doc == null)
        {
            Debug.LogErrorFormat("[Longtu]Set Manifest failed, Could not open {0}", manifestPath);
            return;
        }

        // Get android namespace
        XmlNodeList applicationNodes = doc.GetElementsByTagName("application");
        if (applicationNodes.Count < 1)
        {
            Debug.LogErrorFormat("[Longtu]Could not find <application> tag in {0}", manifestPath);
            return;
        }

        Debug.LogFormat("[Longtu]Setting Longtu Application Name in: {0}", manifestPath);
        XmlNode applicationNode = applicationNodes[0];
        string androidNs = applicationNode.GetNamespaceOfPrefix("android");
        var applicationNameAttr = doc.CreateNode(XmlNodeType.Attribute, "name", androidNs);
        applicationNameAttr.Value = _ApplicationName;
        applicationNode.Attributes.SetNamedItem(applicationNameAttr);

        // 写入meta-data
        {
            bool hasMetaData = false;
            for (int i = 0; i < applicationNode.ChildNodes.Count; i++)
            {
                XmlNode node = applicationNode.ChildNodes[i];
                if (node.Name.Equals("meta-data"))
                {
                    if (node.Attributes != null)
                    {
                        var nameAttribute = node.Attributes["android:name"];
                        if (nameAttribute != null && nameAttribute.Value.Equals(_MetaDataName))
                        {
                            var metaValueAttri = doc.CreateNode(XmlNodeType.Attribute, "value", androidNs);
                            metaValueAttri.Value = _MetaDataValue;
                            node.Attributes.SetNamedItem(metaValueAttri);
                            hasMetaData = true;
                            break;
                        }
                    }
                }
            }
            if (!hasMetaData)
            {
                var metaData = doc.CreateNode(XmlNodeType.Element, "meta-data", string.Empty);
                var metaNameAttri = doc.CreateNode(XmlNodeType.Attribute, "name", androidNs);
                metaNameAttri.Value = _MetaDataName;
                metaData.Attributes.SetNamedItem(metaNameAttri);
                var metaValueAttri = doc.CreateNode(XmlNodeType.Attribute, "value", androidNs);
                metaValueAttri.Value = _MetaDataValue;
                metaData.Attributes.SetNamedItem(metaValueAttri);
                applicationNode.AppendChild(metaData);
            }
        }

        /*
        //写入sdk版本
        {
            bool hasSdk = false;
            var sdkNodes = doc.GetElementsByTagName("uses-sdk");
            if (sdkNodes != null)
            {
                for (int i = 0;i < sdkNodes.Count;i++)
                {
                    var node = sdkNodes[i];
                    var targetAttri = node.Attributes["android:targetSdkVersion"];
                    if (targetAttri != null)
                    {
                        targetAttri.Value = _TargetSDKVersionValue;
                        hasSdk = true;
                    }
                }
            }
            if (!hasSdk)
            {
                var sdk = doc.CreateNode(XmlNodeType.Element, "uses-sdk", string.Empty);
                var targetAttri = doc.CreateNode(XmlNodeType.Attribute, "android:targetSdkVersion", string.Empty);
                targetAttri.Value = _TargetSDKVersionValue;
                sdk.Attributes.SetNamedItem(targetAttri);
                doc.DocumentElement.AppendChild(sdk);
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
        "Plugins/Android/res",
    };
    private static readonly string[] _FilesInPlugins =
    {
        "Plugins/Android/libs/applauncher.jar",
        "Plugins/Android/libs/bhsdk7.5.1.jar",
        "Plugins/Android/libs/noticesdk.jar",
    };

    public static void Disable()
    {
        DeleteSDKFolder();
        DeletePluginFiles();
    }

    private static void DeleteSDKFolder()
    {
        foreach (var folder in _AllFolders)
        {
            string folderPath = Path.Combine(Application.dataPath, folder);
            FolderUtil.Delete(folderPath);
            Debug.LogFormat("[Longtu]DeleteSDKFolder => Asset/{0} Succeed", folder);
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
                Debug.LogFormat("[Longtu]DeletePluginFiles => Asset/{0} Succeed", file);
            }
        }
    }
    #endregion
}
