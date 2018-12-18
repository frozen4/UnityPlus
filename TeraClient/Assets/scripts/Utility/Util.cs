using UnityEngine;
using System;
using System.IO;

public partial class Util
{
	public static byte[] ReadFile(string path)
	{
        return File.ReadAllBytes(path);
	}
}