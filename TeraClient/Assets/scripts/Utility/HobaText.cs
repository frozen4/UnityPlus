using System.Text;
using System.Collections.Generic;
using System;

/* =========================================================================
 * 【备注】
    - String是一个UTF-16编码的文本
    - String是一个引用类型
    - String是不可变的

    【措施】
    - StringBuilder代替String.Format (后者在性能上和GC上都有极大的提升)
    - 复用StringBuilder

    【注意】
    - 非线程安全
    - 循环调用也会出现逻辑错误 
    var sb = HobaText.GetStringBuilder();
    sb.Append(HobaText.Formate("{0}", 10000));
  =======================================================================*/
public static class HobaText
{
    private static StringBuilder _StringBuilder = new StringBuilder(1024);
    private static StringBuilder _FormatStringBuilder = new StringBuilder(1024);
    private static List<string> _StringList = new List<string>();

    public static StringBuilder GetStringBuilder()
    {
#if true
        _StringBuilder.Length = 0;
        return _StringBuilder;
#else
        return new StringBuilder();
#endif
    }

    public static StringBuilder GetStringBuilder(string str)
    {
#if true
        _StringBuilder.Length = 0;
        _StringBuilder.Append(str);
        return _StringBuilder;
#else
        return new StringBuilder(str);
#endif
    }

    public static List<string> GetStringList()
    {
        _StringList.Clear();
        return _StringList;
    }

    public static string Format(string format, params object[] args)
    {
#if true
        _FormatStringBuilder.Length = 0;
        _FormatStringBuilder.AppendFormat(format, args);
        return _FormatStringBuilder.ToString();
#else
        return string.Format(format, args);
#endif
    }

#if false
    public static void Test()
    {
        var COUNT = 1024;
        var SCALE = 1;

        var str1 = "Hello";
        var str2 = " ";
        var str3 = "TERA";

        //long startMem, endMem, costMem;
        long startTime, endTime, costTime;
        // test1
        //startMem = Profiler.GetMonoHeapSize();
        startTime = DateTime.Now.Ticks;
        for (var i = 0; i < COUNT; i++)
        {
            var res = str1 + str2 + i;
        }
        endTime = DateTime.Now.Ticks;
        //endMem = Profiler.GetMonoHeapSize();
        costTime = (endTime - startTime) * SCALE;
        //costMem = endMem - startMem;
        UnityEngine.Debug.Log("Method 1 costs " + costTime);   // 10001

        //startMem = Profiler.GetMonoHeapSize();
        startTime = DateTime.Now.Ticks;
        for (var i = 0; i < COUNT; i++)
        {
            var res = string.Format("{0}{1}{2}", str1, str2, i);
        }
        endTime = DateTime.Now.Ticks;
        //endMem = Profiler.GetMonoHeapSize();
        costTime = (endTime - startTime) * SCALE;
        //costMem = endMem - startMem;
        UnityEngine.Debug.Log("Method 2 costs " + costTime);   // 20002

        //startMem = GC.GetTotalMemory(true);
        startTime = DateTime.Now.Ticks;
        for (var i = 0; i < COUNT; i++)
        {
            var res = HobaText.Format("{0}{1}{2}", str1, str2, i);
        }
        endTime = DateTime.Now.Ticks;
        //endMem = GC.GetTotalMemory(true);
        costTime = (endTime - startTime) * SCALE;
        //costMem = endMem - startMem;
        UnityEngine.Debug.Log("Method 3 costs " + costTime);  // 10000
    }
#endif
}
