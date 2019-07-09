using System;
using UnityEngine;
using System.Text;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using Common;

public static class UnityMemoryDump
{
    private static StringBuilder _stringBuilder = new StringBuilder();

    public static void Dump(string fileName)
    {
        File.Delete(fileName);
        StreamWriter sw = File.CreateText(fileName);
        if (sw == null)
            return;

        GatherGameCounter(sw);

        GatherComponents(sw);

        sw.Close();
        sw.Dispose();
    }

    private static void GatherGameCounter(StreamWriter sw)
    {
        //EntryPoint
        WriteLine(sw, GetCountString("EntryPoint.Instance._TimerList", EntryPoint.Instance._TimerList.GetTimerCount()));
        WriteLine(sw, GetCountString("EntryPoint.Instance._LateTimerList", EntryPoint.Instance._LateTimerList.GetTimerCount()));

        WriteLine(sw, "\n");
    }

    private static void GatherComponents(StreamWriter sw)
    {
        List<GameObject> gameObjects = new List<GameObject>();
        SortedDictionary<string, int> dicComponents = new SortedDictionary<string, int>();

        foreach (var go in Resources.FindObjectsOfTypeAll<GameObject>())
        {
            foreach (var component in go.GetComponents<Component>())
            {
                if (component == null)
                    continue;

                string typeString = component.GetType().ToString();
                int count;
                if (!dicComponents.TryGetValue(typeString, out count))
                {
                    count = 0;
                    dicComponents.Add(typeString, 0);
                }
                dicComponents[typeString] = count + 1;
            }

            gameObjects.Add(go);
        }

        WriteLine(sw, "Num GameObject: {0}", gameObjects.Count);

        int nComponents = 0;
        foreach (var kv in dicComponents)
        {
            nComponents += kv.Value;
        }
        WriteLine(sw, "\nNum Component: {0}", nComponents);
        foreach(var kv in dicComponents)
        {
           WriteLine(sw, "\t{0}\t\t{1}", kv.Key, kv.Value);
        }
    }

    private static string GetCountString(string name, int count)
    {
        return string.Format("{0}:\t\t{1}", name, count);
    }

    public static void WriteLine(StreamWriter sw, string format, params object[] args)
    {
        _stringBuilder.Length = 0;
        _stringBuilder.AppendFormat(format, args);
        sw.WriteLine(_stringBuilder.ToString());
    }

}