using UnityEngine;
using System;
using System.Collections.Generic;

public static class CUnityHelper 
{
    public static bool Color32Equal(Color32 c, byte r, byte g, byte b, byte a)
    {
        return c.r == r && c.g == g && c.b == b && c.a == a;
    }

    public static byte GetAlpha(uint c)
    {
        return (byte)((c & 0xff000000) >> 24);
    }

    public static byte GetRed(uint c)
    {
        return (byte)((c & 0x00ff0000) >> 16);
    }

    public static byte GetGreen(uint c)
    {
        return (byte)((c & 0x0000ff00) >> 8);
    }

    public static byte GetBlue(uint c)
    {
        return (byte)(c & 0x000000ff);
    }

    public static Color32 UInt32ToColor32(uint c)
    {
        return new Color32(GetRed(c), GetGreen(c), GetBlue(c), GetAlpha(c));
    }

	public static String RGBHexString(this Color32 color)
	{
		return String.Format("{0:x02}{1:x02}{2:x02}", color.r, color.g, color.b);
	}

	public static GameObject FindChild(this GameObject obj, String name)
	{
		Transform child = obj.transform.Find(name);
		return child ? child.gameObject : null;
	}


	public static GameObject FindChild(this GameObject obj, params String[] childNames)
	{
		GameObject child = obj;
		for (Int32 i=0; i<childNames.Length; ++i)
		{
			child = FindChild(child, childNames[i]);
			if (child == null)
				return null;
		}
		return child;
	}

	public static TComponent FindChild<TComponent>(this GameObject obj, String childName) where TComponent : Component
	{
		GameObject child = FindChild(obj, childName);
		if (child != null)
			return child.GetComponent<TComponent>();
		else
			return null;
	}

	public static TComponent FindChildComponent<TComponent>(this GameObject obj, params String[] childNames) where TComponent : Component
	{
		GameObject child = FindChild(obj, childNames);
		if (child != null)
			return child.GetComponent<TComponent>();
		else
			return null;
	}

	public static void RequireChild(this GameObject obj, out GameObject child, String name)
	{
		child = FindChild(obj, name);
		if (child == null)
			throw new Exception(String.Format("Child with name {0} not found in {1}", name, obj));
	}

	public static void RequireChild(this GameObject obj, out GameObject child, params String[] childNames)
	{
		child = FindChild(obj, childNames);
		if (child == null)
			throw new Exception(String.Format("Child not found in {0}", obj));
	}

	public static void RequireChild<TComponent>(this GameObject obj, out TComponent component, String name) where TComponent : Component
	{
		TComponent child = FindChildComponent<TComponent>(obj, name);
		if (child == null)
			throw new Exception(String.Format("Component with child name {0} and type {1} not found in {2}", name, typeof(TComponent).Name, obj));
		component = child;
	}

	public static void RequireChild<TComponent>(this GameObject obj, out TComponent component, params String[] childNames) where TComponent : Component
	{
		TComponent child = FindChildComponent<TComponent>(obj, childNames);
		if (child == null)
		{
			String lastChildName = "";
			if (childNames != null && childNames.Length > 0)
				lastChildName = childNames[childNames.Length-1];
			throw new Exception(String.Format("Component with type {0} (last child name: {1}) not found in {2}", typeof(TComponent).Name, lastChildName, obj));
		}
		component = child;
	}

	public static GameObject FindChildRecursively(this GameObject obj, String name)
	{
		if (obj.name == name)
			return obj;

		for (Int32 i=0; i<obj.transform.childCount; ++i)
		{
			var child = FindChildRecursively(obj.transform.GetChild(i).gameObject, name);
			if (child != null)
				return child;
		}
		
		return null;
	}

	public static TComponent FindChildComponentRecursively<TComponent>(this GameObject obj, String childName) where TComponent : Component
	{
		GameObject child = FindChildRecursively(obj, childName);
		if (child != null)
			return child.GetComponent<TComponent>();
		else
			return null;
	}

	public static void RegisterLogCallback(Application.LogCallback handler)
	{
        //Application.logMessageReceivedThreaded += handler;
        Application.logMessageReceived += handler;
	}

	public static void UnregisterLogCallback(Application.LogCallback handler)
	{
		//Application.logMessageReceivedThreaded -= handler;
        Application.logMessageReceived -= handler;
	}

    struct BatchItem
    {
        public List<MeshRenderer> MeshList;
    }

    private static readonly Dictionary<string, BatchItem> _BatchInfos = new Dictionary<string, BatchItem>();
    public static void StaticBatching(UnityEngine.GameObject go)
    {
        MeshRenderer[] mrs = go.GetComponentsInChildren<MeshRenderer>();
        _BatchInfos.Clear();

        foreach (MeshRenderer mr in mrs)
        {
            var mat = mr.material;
            if (mat != null)
            {
                BatchItem o;
                if (!_BatchInfos.TryGetValue(mat.name, out o))
                {
                    o = new BatchItem() { MeshList = new List<MeshRenderer>() };
                    _BatchInfos.Add(mat.name, o);
                }

                o.MeshList.Add(mr);
            }
        }

        int index = 0;
        var e = _BatchInfos.GetEnumerator();
        while (e.MoveNext())
        {
            BatchItem pb = e.Current.Value;

            if (pb.MeshList.Count <= 1)
                continue;

            var root = new GameObject(HobaText.Format("Batch{0}", index++));
            var mt = pb.MeshList[0].sharedMaterial;

            var goArray = new GameObject[pb.MeshList.Count];
            var rootTrans = root.transform;
            for (int i = 0; i < pb.MeshList.Count; i++)
            {
                var cc = pb.MeshList[i];
                var g = cc.gameObject;
                g.transform.parent = rootTrans;

                if (i != 0)
                    cc.sharedMaterial = mt;
                goArray[i] = g;
            }

            StaticBatchingUtility.Combine(goArray, root);
            rootTrans.parent = go.transform;
        }
        e.Dispose();
    }

    public static void StopStaticBatching()
    {
        _BatchInfos.Clear();
    }

    public static Texture2D GenAlphaTexFromJPG(byte[] jpgdata, byte[] alpha_data)
    {
        if (jpgdata == null || alpha_data == null)
            return null;

        Texture2D tex = new Texture2D(0, 0, TextureFormat.RGB24, false);
        if (!tex.LoadImage(jpgdata))
        {
            return null;
        }

        Color32[] data = tex.GetPixels32();
        int size = data.Length;

        if (alpha_data.Length != size)
        {
            UnityEngine.Object.Destroy(tex);
            return null;
        }

        for (int i = 0; i < size; i++)
        {
            data[i].a = alpha_data[i];
        }

        Texture2D texnew = new Texture2D(tex.width, tex.height, TextureFormat.ARGB32, false);
        texnew.SetPixels32(data);
        texnew.Apply();
        UnityEngine.Object.Destroy(tex);
        return texnew;
    }
}
