using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

public class AssetBundleChecker : Editor {


    public static string[] BundleNames = new string[] { "world03part2blocksnew"
    ,"world03part1blocksnew"
    ,"world02blocksnew"
    ,"world01blocksnew"
    ,"sound"
    ,"shader"
    ,"sfx"
    ,"scenes"
    ,"commonres"
    ,"outward"
    ,"others"
    ,"monsters"
    ,"interfaces"
    ,"commonatlas"
    ,"city01blocksnew"
    ,"characters"
    ,"cganimator"
    ,"cg"
    ,"animations"
    };

   

    public static string ResPath = Application.dataPath + "/../../GameRes/AssetBundles/Windows/";
    [MenuItem("Assets/Excute Bundele Checker(巨特么卡)")]
    public static void Excute()
    {
        List<AssetBundle> bundles = new List<AssetBundle>();
        //Debug.Log(ResPath);
 
       
        for (int i = 0; i < BundleNames.Length; i++)
        {
            AssetBundle bundle = AssetBundle.LoadFromFile(ResPath + BundleNames[i]);
            //bundle.Unload(true);
            if (null != bundle) bundles.Add(bundle);
        }

        Dictionary<string,List<string>> infoDic = new Dictionary<string,List<string>>();
        foreach (var bundle in bundles)
        {
            Object[] paths = bundle.LoadAllAssets();

            foreach (var item in paths)
            {
                Debug.Log(item.name);
                //Debug.Log(item.);
                //Debug.Log(item);
                //Debug.Log(item.GetHashCode());
                //Debug.Log(item.GetInstanceID());
               
                //break;
            }
            //for (int i = 0; i < paths.Length; i++)
            //{
            //    var key = paths[i];
            //    if (infoDic.ContainsKey(key))
            //    {
            //        infoDic[key].Add(bundle.name);
            //    }
            //    else
            //    {
            //        List<string> temp = new List<string>();
            //        temp.Add(bundle.name);
            //        infoDic.Add(key, temp);
            //    }
            //}
            break;
        }



        foreach (var item in bundles)
        {
            item.Unload(true);
        }

        string textPath = ResPath + "导出bundle所有资源列表" + ".txt";


        using (FileStream fs = new FileStream(textPath, FileMode.Create))
        {
            if (!fs.CanWrite)
            {
                Debug.LogError(string.Format("The {0} Is Locked", textPath));
            }

            StreamWriter sw = new StreamWriter(fs);
            foreach (var cacheData in infoDic)
            {
                var tmpName = "";
                foreach (var item in cacheData.Value)
                {
                    tmpName += item;
                }
                sw.WriteLine(string.Format("{0},{1},{2}", cacheData.Key, cacheData.Value.Count, tmpName));
            }
            sw.Dispose();
            sw.Close();
            fs.Dispose();
            fs.Close();
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("导出完成", "导出路径为" + textPath, "OK");
        EditorUtility.ClearProgressBar();


    }
}
