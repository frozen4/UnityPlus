using UnityEngine;
using System.IO;
using Common;

public class CScreenShotMan :Singleton<CScreenShotMan>
{
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
    private readonly string _ScreenShotFolderPath = Path.Combine(System.Environment.CurrentDirectory, "ScreenCapture");
#else
    private readonly string _ScreenShotFolderPath = Path.Combine(Application.persistentDataPath, "ScreenCapture");
#endif
    public Texture2D ScreenShot { get; private set; }

    public void SetScreenShot(Texture2D tex2d)
    {
        ScreenShot = tex2d;
    }

    public void SaveScreenShot()
    {
        if (ScreenShot == null)
        {
            HobaDebuger.LogWarning("SaveScreenShot failed, ScreenShot texture got null");
            return;
        }

        if (!Directory.Exists(_ScreenShotFolderPath))
        {
            Directory.CreateDirectory(_ScreenShotFolderPath);
        }


        string png_name = System.DateTime.Now.ToFileTime() + ".png";
        string file_name = Path.Combine(_ScreenShotFolderPath, png_name);
        FileStream file = File.Open(file_name, FileMode.Create, FileAccess.Write);
        using (BinaryWriter bw = new BinaryWriter(file))
        {
            byte[] bytes = ScreenShot.EncodeToPNG();
            bw.Write(bytes);
        }
        file.Close();

#if !UNITY_EDITOR
#if UNITY_IOS
        //_SavePhoto(file_name);
        IOSUtil.iosSaveImageToPhotosAlbum(file_name);
#elif UNITY_ANDROID
        //AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        //AndroidJavaObject jo = jc.GetStatic<AndroidJavaObject>("currentActivity");
        //jo.CallStatic("savePhoto", file_name);
        AndroidUtil.SavePhoto(file_name);
#endif
#endif
    }

    public void AbandonScreenShot()
    {
        if (ScreenShot != null)
        {
            Texture2D.Destroy(ScreenShot);
            ScreenShot = null;
        }
    }
}