using UnityEngine;
using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Security;
using System.Runtime.InteropServices;

public class StringTable
{
    private Dictionary<string, string> _StringMap;

    public bool LoadText(string content)
    {
        _StringMap = new Dictionary<string, string>();

        try
        {
            SecurityElement doc = SecurityElement.FromString(content);
            SecurityElement strings = doc;
            if (strings == null)
                return false;

            if (strings.Children != null)
            {
                for(int i  = 0; i < strings.Children.Count; ++i)
                {
                    SecurityElement node = (SecurityElement)strings.Children[i];
                    var origin = node.Attributes["origin"];
                    var translation = node.Attributes["translation"];
                    if (origin != null && translation != null)
                        _StringMap[origin.ToString()] = translation.ToString();
                }
            }
        }
        catch (XmlSyntaxException)
        {
            return false;
        }
        catch (NullReferenceException)
        {
            return false;
        }
        return true;
    }

    /// <summary>
    /// get traslated string. return original string if not found
    /// </summary>
    /// <param name="origin"></param>
    /// <returns></returns>
    public string this[string origin]
    {
        get
        {
            string translation;
            if (_StringMap != null && _StringMap.TryGetValue(origin, out translation))
                return translation;
            else
                return origin;
        }
    }
}

public partial class Util
{
    public static float FloatZero = 1e-5f;
    /*
    public static void Forecah<TKey, TValue>(this Dictionary<TKey, TValue> dict, Action<TKey, TValue> EnumeratorFunc)
    { 
        if (dict == null || EnumeratorFunc == null) 
            throw new ArgumentNullException(); 
 
        var i = dict.GetEnumerator(); 
        while (i.MoveNext()) 
        { 
            EnumeratorFunc(i.Current.Key, i.Current.Value); 
        } 
    } 
    */

    public static bool IsZero(float f)
    {
        return (Mathf.Abs(f) < FloatZero);
    }

    public static int Random(int min, int max)
	{
		return UnityEngine.Random.Range(min, max);
	}

	public static float Random(float min, float max)
	{
		return UnityEngine.Random.Range(min, max);
	}

	public static string Uid(string uid)
	{
		int position = uid.LastIndexOf('_');
		return uid.Remove(0, position + 1);
	}

	public static long GetTime()
	{
		TimeSpan ts = new TimeSpan(DateTime.UtcNow.Ticks - new DateTime(1970, 1, 1, 0, 0, 0).Ticks);
		return (long)ts.TotalMilliseconds;
	}

	/// 手机震动
	public static void Vibrate()
	{
		//iPhoneUtils.Vibrate();
	}

	/// Base64编码
	public static string Encode(string message)
	{
		byte[] bytes = Encoding.UTF8.GetBytes(message);
		return Convert.ToBase64String(bytes);
	}

	/// Base64解码
	public static string Decode(string message)
	{
		byte[] bytes = Convert.FromBase64String(message);
		return Encoding.UTF8.GetString(bytes);
	}

	/// 判断数字
	public static bool IsNumeric(string str)
	{
		if (string.IsNullOrEmpty(str)) return false;
		for (int i = 0; i < str.Length; i++)
		{
			if (!Char.IsNumber(str[i])) { return false; }
		}
		return true;
	}

    /*
	/// HashToMD5Hex
	public static string HashToMD5Hex(string sourceStr)
	{
		byte[] Bytes = Encoding.UTF8.GetBytes(sourceStr);
		using (MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider())
		{
			byte[] result = md5.ComputeHash(Bytes);
			StringBuilder builder = HobaString.GetStaticStringBuilder();
			for (int i = 0; i < result.Length; i++)
				builder.Append(result[i].ToString("x2"));
			return builder.ToString();
		}
	}

	/// 计算字符串的MD5值
	public static string md5(string source)
	{
		MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
		byte[] data = System.Text.Encoding.UTF8.GetBytes(source);
		byte[] md5Data = md5.ComputeHash(data, 0, data.Length);
		md5.Clear();

		string destString = "";
		for (int i = 0; i < md5Data.Length; i++)
		{
			destString += System.Convert.ToString(md5Data[i], 16).PadLeft(2, '0');
		}
		destString = destString.PadLeft(32, '0');
		return destString;
	}
    */

    /// 计算文件的MD5值
    public static string md5file(string file)
	{
		try
		{
            if (!File.Exists(file))
            {
                return "0";
            }

            FileStream fs = new FileStream(file, FileMode.Open);

			System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
			byte[] retVal = md5.ComputeHash(fs);
			fs.Close();

			StringBuilder sb = HobaText.GetStringBuilder();
            for (int i = 0; i < retVal.Length; i++)
			{
				sb.Append(retVal[i].ToString("x2"));
			}
			return sb.ToString();
		}
		catch (Exception ex)
		{
			throw new Exception("md5file() fail, error:" + ex.Message);
		}
	}

	/// 是否为数字
	public static bool IsNumber(string strNumber)
	{
		Regex regex = new Regex("[^0-9]");
		return !regex.IsMatch(strNumber);
	}

	/// 取得数据存放目录
	/*
    public static string DataPath {
        get {
            string game = Const.AppName.ToLower();
            string dataPath = Application.dataPath;

            if (Application.platform == RuntimePlatform.IPhonePlayer) {
                string path = Path.GetDirectoryName(Path.GetDirectoryName(dataPath));
                return Path.Combine(path, "Documents/" + game + "/");
            } else if (Application.platform == RuntimePlatform.Android) {
                return "/sdcard/" + game + "/";
            }
            if (Const.DebugMode) {
                string target = string.Empty;
                if (Application.platform == RuntimePlatform.OSXEditor ||
                    Application.platform == RuntimePlatform.IPhonePlayer ||
                    Application.platform == RuntimePlatform.OSXEditor) {
                    target = "iphone";
                } else {
                    target = "android";
                }
                return dataPath + "/StreamingAssets/" + target + "/";
            }
            return "c:/" + game + "/";
        }
    }
    */

	public static byte[] ReadFile(string path)
	{
		byte[] str = null;
#if UNITY_IPHONE
        // 文档中说：WWW.isDone不能用循环的，需要用coroutines，此处的用法不合适，待修改  (lijian 20151021)
        path = path.Replace("file://", "");
#endif

#if  UNITY_EDITOR || UNITY_STANDALONE_WIN               //只在windows下起作用
        bool bUseFile = (EntryPoint.Instance.DebugSettingParams.LocalData && path.Contains("GameRes/Data"))
                || (EntryPoint.Instance.DebugSettingParams.LocalLua && path.Contains("GameRes/Lua"));

        if (bUseFile)
        {
            if (File.Exists(path))
            {
                try
                {
                    str = File.ReadAllBytes(path);
                }
                catch (Exception e)
                {
                    str = null;
                    Common.HobaDebuger.LogError("failed to read File with path: " + path + ": " + e);
                }
            }
            else
            {
                Common.HobaDebuger.Log("file: " + path + " File does not exist");
            }
        }
        else            //FileImage
#endif
        {
            if (FileImage.IsExist(path))
            {
                try
                {
                    str = FileImage.ReadAllBytes(path);
                }
                catch (Exception e)
                {
                    str = null;
                    Common.HobaDebuger.LogWarning("failed to read FileImage with path: " + path + ": " + e);
                }
            }
            else
            {
                Common.HobaDebuger.Log("file: " + path + " FileImage does not exist");
            }
        }

        return str;
	}

    public static void WriteFile(string path, byte[] content, int length)
    {
        FileInfo fi = new FileInfo(path);
        if (fi.Exists) return;

        Stream sw = fi.Create();
        sw.Write(content, 0, length);
        sw.Close();
    }

#if UNITY_EDITOR && UNITY_STANDALONE_WIN
    public static void OpenDir(string outputPath)
    {
        string str = "";
        for (int i = 0; i < outputPath.Length; ++i)
        {
            char c = outputPath[i];
            if (c == '/')
                c = '\\';
            str += c;
        }
        ShellExecute(
            IntPtr.Zero,
            "open",
            "Explorer.exe",
            str, // "/select, " + str,
            "",
            ShowCommands.SW_NORMAL);
    }

    public enum ShowCommands
    {
        SW_HIDE = 0,
        SW_SHOWNORMAL = 1,
        SW_NORMAL = 1,
        SW_SHOWMINIMIZED = 2,
        SW_SHOWMAXIMIZED = 3,
        SW_MAXIMIZE = 3,
        SW_SHOWNOACTIVATE = 4,
        SW_SHOW = 5,
        SW_MINIMIZE = 6,
        SW_SHOWMINNOACTIVE = 7,
        SW_SHOWNA = 8,
        SW_RESTORE = 9,
        SW_SHOWDEFAULT = 10,
        SW_FORCEMINIMIZE = 11,
        SW_MAX = 11
    }

    [DllImport("shell32.dll")]
    public static extern IntPtr ShellExecute(
        IntPtr hwnd,
        string lpOperation,
        string lpFile,
        string lpParameters,
        string lpDirectory,
        ShowCommands nShowCmd);

#endif

	/// 应用程序内容路径
    /*
	public static string AppContentPath()
	{
		string path = string.Empty;
		switch (Application.platform)
		{
			case RuntimePlatform.Android:
				path = "jar:file://" + Application.dataPath + "!/assets/android";
				break;
			case RuntimePlatform.IPhonePlayer:
				path = Application.dataPath + "/Raw/iphone";
				break;
			default:
				path = Application.dataPath + "/StreamingAssets/android";
				break;
		}
		return path;
	}
    */

	public static GameObject FindChildDirect(GameObject obj, string childname, bool recursive)
	{
		if (childname == ".")
		{
			return obj;
		}
		else if (childname == "..")
		{
			Transform parentTras = obj.transform.parent;
			return parentTras == null ? null : parentTras.gameObject;
		}

		for (int i = 0; i < obj.transform.childCount; ++i)
		{
			Transform trans = obj.transform.GetChild(i);
			GameObject child = trans.gameObject;
			if (child.name == childname)
				return child;
			else if (recursive)
			{
				GameObject ret = FindChildDirect(child, childname, recursive);
				if (ret != null)
					return ret;
			}
		}
		return null;
	}

	public static void SetLayerRecursively(GameObject obj, int layer)
	{
        if (obj == null)
            return;

		obj.layer = layer;
        Transform obj_trans = obj.transform;
        int nChildCount = obj_trans.childCount;
        for (int i = 0; i < nChildCount; ++i)
		{
            Transform trans = obj_trans.GetChild(i);
            if (trans == null)
                continue;

			GameObject child = trans.gameObject;
			SetLayerRecursively(child, layer);
		}
	}

    
    public static float MagnitudeH(Vector3 a)
    {
        return Mathf.Sqrt(a.x * a.x + a.z * a.z);
    }

    public static float DistanceH(Vector3 a, Vector3 b)
    {
        return Mathf.Sqrt((a.x - b.x)*(a.x - b.x) + (a.z - b.z)*(a.z - b.z));
    }

    public static float SquareDistanceH(Vector3 a, Vector3 b)
    {
        return (a.x - b.x) * (a.x - b.x) + (a.z - b.z) * (a.z - b.z);
    }

    public static float SquareDistance(Vector3 a, Vector3 b)
    {
        return (a.x - b.x) * (a.x - b.x) + (a.y - b.y) * (a.y - b.y) + (a.z - b.z) * (a.z - b.z);
    }

    public static bool IsValidDir(ref Vector3 vDir)
    {
        float mag = vDir.magnitude;
        if (mag < 0.02f)
        {
            return false;
        }
        vDir = vDir / mag;
        return true;
    }

    public static bool IsValidDir(ref Vector3 vDir, ref float magnitude)
    {
        magnitude = vDir.magnitude;
        if (magnitude < 0.02f)
        {
            return false;
        }
        vDir = vDir / magnitude;
        return true;
    }

    public static bool IsNaN(Vector3 vec)
    {
        return float.IsNaN(vec.x) || float.IsNaN(vec.y) || float.IsNaN(vec.z);
    }

    /// <summary>
    /// 销毁所有child
    /// </summary>
    public static void DestroyChildren(Transform transform, bool activeIncluded = true, GameObject exceptObj = null)
    {
        if (transform != null)
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                Transform t = transform.GetChild(i);
                if (t != null)
                {
                    if (t.gameObject != null)
                    {
                        if (exceptObj != null && t.gameObject == exceptObj)
                            continue;
                        if (activeIncluded)
                            GameObject.Destroy(t.gameObject);
                        else
                        {
                            if (t.gameObject.activeSelf)
                            {
                                GameObject.Destroy(t.gameObject);
                            }
                        }
                    }
                }
            }
        }
    }


    public static bool IsEqual(float a, float b)
    {
        return Math.Abs(a - b) <= 1e-6;
    }

    public static bool IsEqual(double a, double b)
    {
        return Math.Abs(a - b) <= 1e-6;
    }

    public static float GetMaxValidDistance(Vector3 srcPos, Vector3 dir, float maxDis)
    {
        float distance = maxDis;
        float t = 0;
        srcPos.y += 0.5f;  // 做适度拔高

        //collider检查
        if (maxDis < 0)
        {
            dir = -dir;
            maxDis = -maxDis;
        }

        //navmesh检查
        Vector3 vDelta = dir.normalized * maxDis;
        if (!NavMeshManager.Instance.IsConnected(srcPos, srcPos + vDelta, ref t) && t < 1)
        {
            var vHitPos = srcPos + vDelta * t;
            distance = (vHitPos - srcPos).magnitude;
        }

        RaycastHit hitInfo;
        if (Physics.Raycast(srcPos, dir, out hitInfo, maxDis, CUnityUtil.LayerMaskMovementObstacle) && hitInfo.distance < distance)
            distance = hitInfo.distance;

        return distance;
    }

    static char[] specials = new char[] { (char)8203 };
    //某些路径字符在
    public static string FixPathString(string path)
    {
        if (path.Length > 0 && path[0] == (char)8203)
            return path.TrimStart(specials);
        else
            return path;
    }
}