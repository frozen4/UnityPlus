using UnityEngine;
using System.Collections;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public class BuildScriptTools
{

    [MenuItem("Build Tools /CollectCommonTextureInfo")]
    static void CollectInfo()
    {
        string rootPath = Application.dataPath + "/Outputs";
        string[] directoryEntries = Directory.GetFileSystemEntries(rootPath);
        List<string> assetbundleNameArray = new List<string>();//即文件夹名称
        foreach (string item in directoryEntries)
        {
            if (!item.Contains("."))
            {
#if UNITY_EDITOR_WIN
                string[] folderName = item.Split('\\');
                assetbundleNameArray.Add(folderName[1]);
#else
				//Debug.LogError(item);
				string[] folderName = item.Split('/');
				int length = folderName.Length;
				assetbundleNameArray.Add(folderName[length - 1 ]);
#endif
            }
        }


        List<string> collectFolderList = new List<string>();
        for (int index = 0; index < assetbundleNameArray.Count; index++)
        {

            if (assetbundleNameArray[index].Contains("Scenes") || assetbundleNameArray[index].Contains("Blocks"))

            //if (assetbundleNameArray[index].Contains("TestFFFF"))
            {
                collectFolderList.Add(assetbundleNameArray[index]);
            }
        }



        List<UnityEngine.Object> ObjectList = new List<UnityEngine.Object>();


        for (int i = 0; i < collectFolderList.Count; i++)
        {
            string folderName = "";
            folderName = collectFolderList[i];

            string path = Path.Combine(Application.dataPath, "Outputs/" + folderName + "/");
#if UNITY_EDITOR_WIN
            string path2 = path.Replace("/", @"\");
#else
		string path2 = path;
#endif
            string[] fileList = Directory.GetFiles(path2, "*.*", SearchOption.AllDirectories);
            string finnallyPath = "";
            for (int innerIndex = 0; innerIndex < fileList.Length; innerIndex++)
            {
                if (fileList[innerIndex].EndsWith("prefab"))
                {
                    finnallyPath = "Assets/Outputs/" + folderName + "/" + fileList[innerIndex].Replace(path2, "");
                    ObjectList.Add(AssetDatabase.LoadAssetAtPath<GameObject>(finnallyPath));
                }
            }
        }


        Selection.objects = ObjectList.ToArray();

        EditorUtility.DisplayCancelableProgressBar("资源检索", "Searching Assets", 0.3f);
        UnityEngine.Object[] outPutsAssets = Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.DeepAssets);
        
        
        UnityEngine.Object[] collectObjs = EditorUtility.CollectDependencies(outPutsAssets);

        Dictionary<UnityEngine.Object, int> collectInfoDic = new Dictionary<UnityEngine.Object, int>();

        int tempCount = 0;
        int texCount = 0;
        foreach (UnityEngine.Object item in outPutsAssets)
        {
            UnityEngine.Object[] objRoot = new UnityEngine.Object[] { item };
            UnityEngine.Object[] tempCollectRoot = EditorUtility.CollectDependencies(objRoot);

            foreach ( UnityEngine.Object innerItem in tempCollectRoot)
            {

                if (null == innerItem)
                {
                    continue;
                }
                if (innerItem is Texture)
                {
                    
                    texCount++;
                    if (collectInfoDic.ContainsKey(innerItem))
                    {

                        tempCount = collectInfoDic[innerItem];
                        collectInfoDic[innerItem] = tempCount + 1;
                    }
                    else
                    {
                        collectInfoDic.Add(innerItem, 1);
                    }
                }

            }
        }



        int collectCount = collectInfoDic.Count;


        if (0 == collectCount)
        {
            EditorUtility.DisplayDialog("Error", "没有收集到任何信息", "OK");
            EditorUtility.ClearProgressBar();
            return;
        }

        string outPath = Path.Combine(Application.dataPath, "The Fucking Texture Collect Info.txt");

        using (FileStream fs = new FileStream(outPath, FileMode.Create))
        {
            if (!fs.CanWrite)
            {
                EditorUtility.DisplayDialog("Error", "文件被锁定", "OK");
                EditorUtility.ClearProgressBar();
                return;
            }
            StreamWriter sw = new StreamWriter(fs);
            sw.WriteLine(string.Format("{0},{1},{2}", "图片名称", "引用次数", "资源路径"));
            foreach (KeyValuePair<UnityEngine.Object, int> kv in collectInfoDic)
            {
                string assetPath = AssetDatabase.GetAssetPath(kv.Key);
                sw.WriteLine(string.Format("{0},{1},{2}", kv.Key.name, kv.Value, assetPath));
            }
            sw.Dispose();
            sw.Close();
            fs.Dispose();
            fs.Close();
        }

        EditorUtility.DisplayDialog("输出文件成功", "文件已经生成在Asset下共有图片" + texCount, "OK");
        EditorUtility.ClearProgressBar();

    }

    [MenuItem("Build Tools /CheckRes")]
    static void CheckRes()
    {
        string rootPath = Application.dataPath + "/Outputs";
        string[] directoryEntries = Directory.GetFileSystemEntries(rootPath);
        List<string> assetbundleNameArray = new List<string>();//即文件夹名称
        foreach (string item in directoryEntries)
        {
            if (!item.Contains("."))
            {
#if UNITY_EDITOR_WIN
                string[] folderName = item.Split('\\');
                assetbundleNameArray.Add(folderName[1]);
#else
				//Debug.LogError(item);
				string[] folderName = item.Split('/');
				int length = folderName.Length;
				assetbundleNameArray.Add(folderName[length - 1 ]);
#endif
            }
        }


        List<string> collectFolderList = new List<string>();
        for (int index = 0; index < assetbundleNameArray.Count; index++)
        {

            if (assetbundleNameArray[index].Contains("Scenes") || assetbundleNameArray[index].Contains("Blocks"))

            //if (assetbundleNameArray[index].Contains("TestFFFF"))
            {
                collectFolderList.Add(assetbundleNameArray[index]);
            }
        }



        List<UnityEngine.Object> ObjectList = new List<UnityEngine.Object>();


        for (int i = 0; i < collectFolderList.Count; i++)
        {
            string folderName = "";
            folderName = collectFolderList[i];

            string path = Path.Combine(Application.dataPath, "Outputs/" + folderName + "/");
#if UNITY_EDITOR_WIN
            string path2 = path.Replace("/", @"\");
#else
		string path2 = path;
#endif
            string[] fileList = Directory.GetFiles(path2, "*.*", SearchOption.AllDirectories);
            string finnallyPath = "";
            for (int innerIndex = 0; innerIndex < fileList.Length; innerIndex++)
            {
                if (fileList[innerIndex].EndsWith("prefab"))
                {
                    finnallyPath = "Assets/Outputs/" + folderName + "/" + fileList[innerIndex].Replace(path2, "");
                    ObjectList.Add(AssetDatabase.LoadAssetAtPath<GameObject>(finnallyPath));
                }
            }
        }


        Selection.objects = ObjectList.ToArray();

        EditorUtility.DisplayCancelableProgressBar("资源检索", "Searching Assets", 0.3f);
        UnityEngine.Object[] outPutsAssets = Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.DeepAssets);
        UnityEngine.Object[] collectObjs = EditorUtility.CollectDependencies(outPutsAssets);


        Dictionary<UnityEngine.Object , int> collectInfoDic = new Dictionary<UnityEngine.Object , int>();

        Dictionary<UnityEngine.Object, int> shaderInfoDic = new Dictionary<UnityEngine.Object, int>();

        int collectCount = collectObjs.Length;
        if (0 == collectCount)
        {
            EditorUtility.DisplayDialog("Error", "没有收集到任何信息", "OK");
            EditorUtility.ClearProgressBar();
            return;
        }
        int collectProgress = 0 ;
        int tempCount = 0;
        int texCount  =0;
        foreach (UnityEngine.Object o in collectObjs)
        {
            if (null == o)
            {
                continue;
            }
            EditorUtility.DisplayCancelableProgressBar("资源整理", o.name, (float)collectProgress / (float)collectCount);
            if (o is Texture)
            {
                texCount++;
                if (collectInfoDic.ContainsKey(o))
                {
                    tempCount = collectInfoDic[o];
                    collectInfoDic[o] = tempCount + 1;
                }
                else
                {
                    collectInfoDic.Add(o, 1);
                }

            }
            else if (o is Shader)
            {
                if (!shaderInfoDic.ContainsKey(o))
                {
                    shaderInfoDic.Add(o, 1);
                }
                if (null!= o)
                {
                    Debug.LogError(o.name);
                }
            }
            collectProgress++;
        }

        string outPath = Path.Combine(Application.dataPath, "Shader Collect Info.txt");



        using (FileStream fs = new FileStream(outPath, FileMode.Create))
        {
            if (!fs.CanWrite)
            {
                EditorUtility.DisplayDialog("Error", "文件被锁定", "OK");
                EditorUtility.ClearProgressBar();
                return;
            }
            StreamWriter sw = new StreamWriter(fs);
            //sw.WriteLine(string.Format("{0},{1},{2}", "图片名称", "引用次数", "资源路径"));
            foreach (KeyValuePair<UnityEngine.Object, int> kv in shaderInfoDic)
            {
                string assetPath = AssetDatabase.GetAssetPath(kv.Key);
                sw.WriteLine(string.Format("{0},{1}", kv.Key.name, assetPath));
            }
            sw.Dispose();
            sw.Close();
            fs.Dispose();
            fs.Close();
        }


        //AssetBundleBuild[] buildMaps = new AssetBundleBuild[collectInfoDic.Count];
        int mapIndex = 0;
        List<AssetBundleBuild> buildList = new List<AssetBundleBuild>();
        foreach (KeyValuePair<UnityEngine.Object, int> kv in collectInfoDic)
        {
            string assetPath = AssetDatabase.GetAssetPath(kv.Key);
            AssetBundleBuild bundle = new AssetBundleBuild();
            bundle.assetBundleName = "TextureBundleTest";
            bundle.assetNames = new string[] { assetPath };
            buildList.Add(bundle);
        }

        foreach (KeyValuePair<UnityEngine.Object, int> kv in shaderInfoDic)
        {
            string assetPath = AssetDatabase.GetAssetPath(kv.Key);
            AssetBundleBuild bundle = new AssetBundleBuild();
            bundle.assetBundleName = "ShaderBundleTest";
            bundle.assetNames = new string[] { assetPath };
            buildList.Add(bundle);
        }


        //AssetBundleBuild[] buildMaps = buildList.ToArray();
        //BuildAssetBundleOptions options = BuildAssetBundleOptions.ForceRebuildAssetBundle | BuildAssetBundleOptions.ChunkBasedCompression;

        //BuildPipeline.BuildAssetBundles(Application.dataPath, buildMaps, options, BuildTarget.StandaloneWindows);
        EditorUtility.DisplayDialog("输出文件成功", "文件已经生成在Asset下共有图片" + texCount, "OK");
        EditorUtility.ClearProgressBar();
    }




    /// <summary>
    /// 设置指定的版本目录下所有文件的AssetName
    /// </summary>
    /// <param name="versionDir"></param>
    public static void SetVersionDirAssetName(string assetName)
    {
        var fullPath = Application.dataPath + "/" + assetName + "/";
        var relativeLen = assetName.Length + 8; // Assets 长度
        if (Directory.Exists(fullPath))
        {
            EditorUtility.DisplayProgressBar("设置AssetName名称", "正在设置AssetName名称中...", 0f);
            var dir = new DirectoryInfo(fullPath);
            var files = dir.GetFiles("*", SearchOption.AllDirectories);
            for (var i = 0; i < files.Length; ++i)
            {
                var fileInfo = files[i];
                EditorUtility.DisplayProgressBar("设置AssetName名称", "正在设置AssetName名称中...", 1f * i / files.Length);
                if (!fileInfo.Name.EndsWith(".meta"))
                {
                    var basePath = fileInfo.FullName.Substring(fullPath.Length - relativeLen).Replace('\\', '/');
                    var importer = AssetImporter.GetAtPath(basePath);
                    if (importer && importer.assetBundleName != assetName)
                    {
                        importer.assetBundleName = assetName;
                    }
                }
            }
            EditorUtility.ClearProgressBar();
        }
    }



    [MenuItem("Build Tools /创建ShaderVariants")]
    public static void CreateShaderVariants()
    {
        string rootPath = Application.dataPath + "/Outputs";
        string[] directoryEntries = Directory.GetFileSystemEntries(rootPath);
        List<string> assetbundleNameArray = new List<string>();//即文件夹名称
        foreach (string item in directoryEntries)
        {
            if (!item.Contains("."))
            {
#if UNITY_EDITOR_WIN
                string[] folderName = item.Split('\\');
                assetbundleNameArray.Add(folderName[1]);
#else
				//Debug.LogError(item);
				string[] folderName = item.Split('/');
				int length = folderName.Length;
				assetbundleNameArray.Add(folderName[length - 1 ]);
#endif
            }
        }


        List<string> collectFolderList = new List<string>();
        for (int index = 0; index < assetbundleNameArray.Count; index++)
        {
            collectFolderList.Add(assetbundleNameArray[index]);
        }



        List<UnityEngine.Object> ObjectList = new List<UnityEngine.Object>();


        for (int i = 0; i < collectFolderList.Count; i++)
        {
            string folderName = "";
            folderName = collectFolderList[i];

            string path = Path.Combine(Application.dataPath, "Outputs/" + folderName + "/");
#if UNITY_EDITOR_WIN
            string path2 = path.Replace("/", @"\");
#else
		string path2 = path;
#endif
            string[] fileList = Directory.GetFiles(path2, "*.*", SearchOption.AllDirectories);
            string finnallyPath = "";
            for (int innerIndex = 0; innerIndex < fileList.Length; innerIndex++)
            {
                if (fileList[innerIndex].EndsWith("prefab"))
                {
                    finnallyPath = "Assets/Outputs/" + folderName + "/" + fileList[innerIndex].Replace(path2, "");
                    ObjectList.Add(AssetDatabase.LoadAssetAtPath<GameObject>(finnallyPath));
                }
            }
        }


        Selection.objects = ObjectList.ToArray();

        EditorUtility.DisplayCancelableProgressBar("资源检索", "Searching Assets", 0.3f);
        UnityEngine.Object[] outPutsAssets = Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.DeepAssets);
        UnityEngine.Object[] collectObjs = EditorUtility.CollectDependencies(outPutsAssets);


        Dictionary<UnityEngine.Object, int> collectInfoDic = new Dictionary<UnityEngine.Object, int>();

        Dictionary<UnityEngine.Object, int> shaderInfoDic = new Dictionary<UnityEngine.Object, int>();

        int collectCount = collectObjs.Length;
        if (0 == collectCount)
        {
            EditorUtility.DisplayDialog("Error", "没有收集到任何信息", "OK");
            EditorUtility.ClearProgressBar();
            return;
        }
        int collectProgress = 0;
        int texCount = 0;
        foreach (UnityEngine.Object o in collectObjs)
        {
            if (null == o)
            {
                continue;
            }
            EditorUtility.DisplayCancelableProgressBar("资源整理", o.name, (float)collectProgress / (float)collectCount);
            if (o is Shader)
            {
                if (!shaderInfoDic.ContainsKey(o))
                {
                    shaderInfoDic.Add(o, 1);
                }
            }
            collectProgress++;
        }

        string outPath = Path.Combine(Application.dataPath, "Shader Collect Info.txt");



        ShaderVariantCollection shaderVariantCollection = new ShaderVariantCollection();
  
        foreach (KeyValuePair<UnityEngine.Object,int> kv in shaderInfoDic)
        {

            string assetPath = AssetDatabase.GetAssetPath(kv.Key);
            if (!assetPath.StartsWith("Asset"))
            {
                ShaderVariantCollection.ShaderVariant shaderVariant = new ShaderVariantCollection.ShaderVariant();
                shaderVariant.shader = (Shader)kv.Key;
                shaderVariantCollection.Add(shaderVariant);
            }

        }
        AssetDatabase.CreateAsset(shaderVariantCollection, "Assets/Outputs/Shader/ShaderPrefab.shadervariants");
        AssetDatabase.Refresh();
        AssetDatabase.SaveAssets();

        EditorUtility.DisplayDialog("生成成功", "shaderVariantCollection生成成功", "OK");
        EditorUtility.ClearProgressBar();
    }




    [MenuItem("Build Tools /检测内置shader引用")]
    public static void  CheckBuildinShader ()
    {
         string rootPath = Application.dataPath + "/Outputs";
        string[] directoryEntries = Directory.GetFileSystemEntries(rootPath);
        List<string> assetbundleNameArray = new List<string>();//即文件夹名称
        foreach (string item in directoryEntries)
        {
            if (!item.Contains("."))
            {
#if UNITY_EDITOR_WIN
                string[] folderName = item.Split('\\');
                assetbundleNameArray.Add(folderName[1]);
#else
				//Debug.LogError(item);
				string[] folderName = item.Split('/');
				int length = folderName.Length;
				assetbundleNameArray.Add(folderName[length - 1 ]);
#endif
            }
        }


        List<string> collectFolderList = new List<string>();
        for (int index = 0; index < assetbundleNameArray.Count; index++)
        {

            //if (assetbundleNameArray[index].Contains("Scenes") || assetbundleNameArray[index].Contains("Blocks"))

            //if (assetbundleNameArray[index].Contains("TestFFFF"))
            {
                collectFolderList.Add(assetbundleNameArray[index]);
            }
        }



        List<UnityEngine.Object> ObjectList = new List<UnityEngine.Object>();


        for (int i = 0; i < collectFolderList.Count; i++)
        {
            string folderName = "";
            folderName = collectFolderList[i];

            string path = Path.Combine(Application.dataPath, "Outputs/" + folderName + "/");
#if UNITY_EDITOR_WIN
            string path2 = path.Replace("/", @"\");
#else
		string path2 = path;
#endif
            string[] fileList = Directory.GetFiles(path2, "*.*", SearchOption.AllDirectories);
            string finnallyPath = "";
            for (int innerIndex = 0; innerIndex < fileList.Length; innerIndex++)
            {
                //if (fileList[innerIndex].EndsWith("prefab"))
                {
                    finnallyPath = "Assets/Outputs/" + folderName + "/" + fileList[innerIndex].Replace(path2, "");
                    ObjectList.Add(AssetDatabase.LoadAssetAtPath<GameObject>(finnallyPath));
                }
            }
        }


        Selection.objects = ObjectList.ToArray();

        EditorUtility.DisplayCancelableProgressBar("资源检索", "Searching Assets", 0.3f);
        UnityEngine.Object[] outPutsAssets = Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.DeepAssets);
        
#if !UseSingleCollect
        UnityEngine.Object[] collectObjs = EditorUtility.CollectDependencies(outPutsAssets);

        Dictionary<string, UnityEngine.Object> collectInfoDic = new Dictionary<string, UnityEngine.Object>();

        List<string> pathList = new List<string>();
        foreach (UnityEngine.Object item in collectObjs)
        {



            if (null == item)
                {
                    continue;
                }
            if (item is Shader)
                {
                    if (!collectInfoDic.ContainsKey(item.name))
                     {
                         collectInfoDic.Add(item.name, item);
                     }
                }
        }



        int collectCount = collectInfoDic.Count;


        if (0 == collectCount)
        {
            EditorUtility.DisplayDialog("Error", "没有收集到任何信息", "OK");
            EditorUtility.ClearProgressBar();
            return;
        }

        string outPath = Path.Combine(Application.dataPath, "BuildinShaderRefrenceInfo.txt");

        using (FileStream fs = new FileStream(outPath, FileMode.Create))
        {
            if (!fs.CanWrite)
            {
                EditorUtility.DisplayDialog("Error", "文件被锁定", "OK");
                EditorUtility.ClearProgressBar();
                return;
            }
            StreamWriter sw = new StreamWriter(fs);
            sw.WriteLine(string.Format("{0},{1}","资源路径","引用Shader"));
            foreach (KeyValuePair< string,UnityEngine.Object> kv in collectInfoDic)
            {
                string assetPath = AssetDatabase.GetAssetPath(kv.Value);
                sw.WriteLine(string.Format("{0},{1}", assetPath ,kv.Key));
            }
            sw.Dispose();
            sw.Close();
            fs.Dispose();
            fs.Close();
        }

        EditorUtility.DisplayDialog("输出文件成功", "文件已经生成在Asset下", "OK");
        EditorUtility.ClearProgressBar();
#else
        //UnityEngine.Object[] collectObjs = EditorUtility.CollectDependencies(outPutsAssets);

        Dictionary<string, UnityEngine.Object> collectInfoDic = new Dictionary<string, UnityEngine.Object>();

        int texCount = 0;
        List<string> pathList = new List<string>();
        foreach (UnityEngine.Object item in outPutsAssets)
        {
            UnityEngine.Object[] objRoot = new UnityEngine.Object[] { item };
            UnityEngine.Object[] tempCollectRoot = EditorUtility.CollectDependencies(objRoot);

            foreach (UnityEngine.Object innerItem in tempCollectRoot)
            {

                if (null == innerItem)
                {
                    continue;
                }
                if (innerItem is Shader)
                {
                    if (!collectInfoDic.ContainsKey(innerItem.name))
                    {
                        collectInfoDic.Add(innerItem.name, item);
                    }
                }

            }
        }



        int collectCount = collectInfoDic.Count;


        if (0 == collectCount)
        {
            EditorUtility.DisplayDialog("Error", "没有收集到任何信息", "OK");
            EditorUtility.ClearProgressBar();
            return;
        }

        string outPath = Path.Combine(Application.dataPath, "BuildinShaderRefrenceInfo.txt");

        using (FileStream fs = new FileStream(outPath, FileMode.Create))
        {
            if (!fs.CanWrite)
            {
                EditorUtility.DisplayDialog("Error", "文件被锁定", "OK");
                EditorUtility.ClearProgressBar();
                return;
            }
            StreamWriter sw = new StreamWriter(fs);
            sw.WriteLine(string.Format("{0},{1}", "资源路径", "引用Shader"));
            foreach (KeyValuePair<string, UnityEngine.Object> kv in collectInfoDic)
            {
                string assetPath = AssetDatabase.GetAssetPath(kv.Value);
                sw.WriteLine(string.Format("{0},{1}", assetPath, kv.Key));
            }
            sw.Dispose();
            sw.Close();
            fs.Dispose();
            fs.Close();
        }

        EditorUtility.DisplayDialog("输出文件成功", "文件已经生成在Asset下", "OK");
        EditorUtility.ClearProgressBar();
#endif
        
        
    }
}
