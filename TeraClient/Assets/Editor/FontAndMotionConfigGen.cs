using UnityEngine;
using System.Collections;
using UnityEditor;
using System.IO;

public class FontAndMotionConfigGen
{
	[MenuItem("Hoba Tools/生成动画字体配置")]
    static public void MotionTextGen()
    {
        string configPath = "Assets/Resources/MotionTextConfig/";
        if (!Directory.Exists(configPath)) Directory.CreateDirectory(configPath);
        
        GMotionModel config1 = ScriptableObject.CreateInstance<GMotionModel>();
        AssetDatabase.CreateAsset(config1, configPath + "MotionTemplate.asset");
        
        GBMFontAndMotionTextModel config2 = ScriptableObject.CreateInstance<GBMFontAndMotionTextModel>();
        AssetDatabase.CreateAsset(config2, configPath + "GBMFontAndMotionTextModel.asset");
    }
}
