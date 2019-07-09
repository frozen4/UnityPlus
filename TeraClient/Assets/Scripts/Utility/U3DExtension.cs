using System.Text;
using UnityEngine;

//扩展方法
public static class U3DExtension
{
    public static bool EqualTo(this Vector3 vec, Vector3 other)
    {
        return Mathf.Abs(vec.x - other.x) < Util.FloatZero && Mathf.Abs(vec.y - other.y) < Util.FloatZero && Mathf.Abs(vec.z - other.z) < Util.FloatZero;
    }

    public static bool IsZero(this Vector3 vec)
    {
        return vec == Vector3.zero;
        //return Mathf.Abs(vec.x) < Util.FloatZero && Mathf.Abs(vec.y) < Util.FloatZero && Mathf.Abs(vec.z) < Util.FloatZero;
    }

    public static bool IsOne(this Vector3 vec)
    {
        return vec == Vector3.one;
        //return Mathf.Abs(vec.x-1) < Util.FloatZero && Mathf.Abs(vec.y-1) < Util.FloatZero && Mathf.Abs(vec.z-1) < Util.FloatZero;
    }

    public static string Normalize(this string str)
    {
        StringBuilder builder = HobaText.GetStringBuilder(str);
        int len = str.Length;
        for (int i = 0; i < len; ++i)
        {
            if (str[i] == '\\')
                builder[i] = '/';
        }
        return builder.ToString();
    }

    public static string NormalizeDir(this string str)
    {
        StringBuilder builder = HobaText.GetStringBuilder(str);
        int len = str.Length;
        for (int i = 0; i < len; ++i)
        {
            if (str[i] == '\\')
                builder[i] = '/';
        }

        if (len > 0)
        {
            char last = str[len - 1];
            if (last != '/')
                builder.Append('/');
        }
        return builder.ToString();
    }

    public static bool IsNormalized(this string str)
    {
        int len = str.Length;
        for (int i = 0; i < len; ++i)
        {
            if (str[i] == '\\')
                return false;
        }
        return true;
    }

    public static void SetDirAndUp(this Transform transform, Vector3 vDir, Vector3 vUp)
    {
        vDir.Normalize();
        vUp.Normalize();
        transform.LookAt(transform.position + vDir, vUp);
    }
}

