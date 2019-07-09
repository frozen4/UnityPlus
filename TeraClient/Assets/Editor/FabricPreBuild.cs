#if USING_FABRIC
using UnityEngine;
using System;
using System.Collections;
using System.IO;
using System.Xml;

public class FabricPreBuild
{
    private readonly static string _PluginsSrcPath = Path.Combine(Application.dataPath, "../../SDK/Fabric/Plugins");
    private readonly static string _PluginsDestPath = Path.Combine(Application.dataPath, "Plugins");

    private readonly static string _FabricApplicationName = "io.fabric.unity.android.FabricApplication";
    private readonly static string _TopLevelMainfestFilePath = Path.Combine(Application.dataPath, "Plugins/Android/AndroidManifest.xml");

    internal static void Start()
    {
        CopyPlugins();
        SetManifestApplicationName();
    }

    //拷贝插件
    private static void CopyPlugins()
    {
        #if UNITY_IOS
        string srcPath = Path.Combine(_PluginsSrcPath, "iOS");
        string destPath = Path.Combine(_PluginsDestPath, "iOS");
        FolderUtil.CopyAndReplaceSub(srcPath, destPath);
        #elif UNITY_ANDROID
        string srcPath = Path.Combine(_PluginsSrcPath, "Android");
        string destPath = Path.Combine(_PluginsDestPath, "Android");
        FolderUtil.CopyAndReplaceSub(srcPath, destPath); 
        #endif
    }

    //修改AndroidManifest的application的name
    private static void SetManifestApplicationName()
    {
        #if UNITY_ANDROID
        XmlDocument doc = new XmlDocument();
        doc.Load(_TopLevelMainfestFilePath);
        if (doc == null)
        {
            Debug.LogErrorFormat("[Fabric]Set Manifest failed, Could not open {0}", _TopLevelMainfestFilePath);
            return;
        }

        // Get android namespace
        XmlNodeList applicationNodes = doc.GetElementsByTagName("application");
        if (applicationNodes.Count < 1)
        {
            Debug.LogErrorFormat("[Fabric]Could not find <application> tag in {0}", _TopLevelMainfestFilePath);
            return;
        }

        Debug.LogFormat("[Fabric]Setting Fabric Application Name in: {0}", _TopLevelMainfestFilePath);
        XmlNode applicationNode = applicationNodes[0];
        string androidNs = applicationNode.GetNamespaceOfPrefix("android");
        var applicationNameAttr = doc.CreateNode(XmlNodeType.Attribute, "name", androidNs);
        applicationNameAttr.Value = _FabricApplicationName;
        applicationNode.Attributes.SetNamedItem(applicationNameAttr);

        doc.Save(_TopLevelMainfestFilePath);
        #endif
    }
}
#endif