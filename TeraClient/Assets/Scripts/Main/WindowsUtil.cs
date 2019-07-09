using UnityEngine;  
using System.Collections;  
using System;  
using System.Runtime.InteropServices;
using System.IO;

#if UNITY_STANDALONE_WIN
using System.Drawing;
using System.Drawing.Imaging;
#endif

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]  
public class OpenFileName  
{  
    public int structSize = 0;  
    public IntPtr dlgOwner = IntPtr.Zero;  
    public IntPtr instance = IntPtr.Zero;  
    public String filter = null;  
    public String customFilter = null;  
    public int maxCustFilter = 0;  
    public int filterIndex = 0;  
    public String file = null;  
    public int maxFile = 0;  
    public String fileTitle = null;  
    public int maxFileTitle = 0;  
    public String initialDir = null;  
    public String title = null;  
    public int flags = 0;  
    public short fileOffset = 0;  
    public short fileExtension = 0;  
    public String defExt = null;  
    public IntPtr custData = IntPtr.Zero;  
    public IntPtr hook = IntPtr.Zero;  
    public String templateName = null;  
    public IntPtr reservedPtr = IntPtr.Zero;  
    public int reservedInt = 0;  
    public int flagsEx = 0;
    public static void OpenPhoto()
    {
        OpenFileName ofn = new OpenFileName();

        ofn.structSize = Marshal.SizeOf(ofn);

        ofn.filter = "(*.jpg*.png)\0*.jpg;*.png";

        ofn.file = new string(new char[256]);

        ofn.maxFile = ofn.file.Length;

        ofn.fileTitle = new string(new char[64]);

        ofn.maxFileTitle = ofn.fileTitle.Length;

        ofn.initialDir = UnityEngine.Application.dataPath;// 默认路径  

        ofn.title = "Open Project";

        // ofn.defExt = "jpg";// 显示文件的类型  
        
        ofn.flags = 0x00080000 | 0x00001000 | 0x00000800 | 0x00000200 | 0x00000008;//OFN_EXPLORER|OFN_FILEMUSTEXIST|OFN_PATHMUSTEXIST| OFN_ALLOWMULTISELECT|OFN_NOCHANGEDIR  

        if (WindowDll.GetOpenFileName(ofn))
        {
            //Debug.Log("Selected file with full path: {0}" + ofn.file);

            ////需要增加压缩处理
            //OnPhotoCameraFileResult(ofn.file);
            OpenFile(ofn.file);
        }
    }

    public static void OpenFile(string fileName)
    {
#if UNITY_STANDALONE_WIN
        FileStream fs = new FileStream(fileName, FileMode.Open);
        Bitmap img1 = new Bitmap(fs);
        var tempPath = Path.Combine(EntryPoint.Instance.CustomPicDir, "HeadIcon.png");
        Bitmap img2 = new Bitmap(128, 128, PixelFormat.Format32bppArgb);
        img2.SetResolution(128f, 128f);
        using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(img2))
        {
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            g.DrawImage(img1, new Rectangle(0, 0, img2.Width, img2.Height), 0, 0, img1.Width, img1.Height, GraphicsUnit.Pixel);
            g.Dispose();
            img2.Save(tempPath, ImageFormat.Png);
        }

        //byte[] bytes = Util.ReadFile(tempPath);
        //FileStream newFs = new FileStream(tempPath, FileMode.Create, FileAccess.Write);
        //newFs.Write(bytes, 0, bytes.Length);
        //newFs.Close();
        //newFs.Dispose();
        OnPhotoCameraFileResult(tempPath);
#endif
    }
    public static void OnPhotoCameraFileResult(string filePath)
    {
        EntryPoint.Instance.OnPhotoCameraFileResult(filePath);
    }

    IEnumerator CallTheCamera()
    {
#if UNITY_STANDALONE_WIN
        yield return Application.RequestUserAuthorization(UserAuthorization.WebCam);
        if (Application.HasUserAuthorization(UserAuthorization.WebCam))
        {
            WebCamDevice[] devices = WebCamTexture.devices;
            if (0 == devices.Length) yield return null;
            var deviceName = devices[0].name;
            // 摄像机摄像的区域
            var webTex = new WebCamTexture(deviceName, 400, 300, -112);
            var cameraTexture = GameObject.Find("CameraTexture");
            if (null == cameraTexture)
            {
                cameraTexture = new GameObject("CameraTexture");
                cameraTexture.transform.localPosition = Vector3.zero;
                cameraTexture.transform.localRotation = Quaternion.identity;
                cameraTexture.transform.localScale = Vector3.one;
            }
           //.GetComponent().mainTexture = webTex;
            var image = cameraTexture.GetComponent<WebCamTexture>();
            if (null == image) yield return null;
            image = webTex;
            webTex.Play();
        }
#endif
        yield return null;
    }

    IEnumerator GetTexture2D()
    {
        yield return new WaitForEndOfFrame();
        Texture2D t = new Texture2D(960,640);
        t.ReadPixels(new Rect(15, 177, 406.5f, 281.6f), 0, 0, false);
        t.Apply();
        // 把图片数据转换为 byte 数组
        byte[] byt = t.EncodeToPNG();
        // 然后保存为图片
      
        File.WriteAllBytes(Application.dataPath + "/Resources/temp.jpg", byt);
      
    }

}
public class WindowDll
{
    [DllImport("Comdlg32.dll", SetLastError = true, ThrowOnUnmappableChar = true, CharSet = CharSet.Auto)]
    public static extern bool GetOpenFileName([In, Out] OpenFileName ofn);
    public static bool GetOpenFileName1([In, Out] OpenFileName ofn)
    {
        return GetOpenFileName(ofn);
    }
}  

