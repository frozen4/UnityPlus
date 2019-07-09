
using UnityEngine;
using System.Collections;

public class FrameRenderTools : MonoBehaviour
{

    public int RenderTime = 30;
    public float TimeDetela = 0.08f;
    public string OutputPath;
    public int targetFrame = 30;
    private int frameCount = 1;
    private bool bStop = false;
    private string fileName;

    void Start()
    {
        Invoke("StopCap", RenderTime);
        Time.timeScale = 0;
        if (frameCount < 10) { fileName = "0000" + frameCount.ToString() + ".jpg"; }
        else if (frameCount < 100) { fileName = "000" + frameCount.ToString() + ".jpg"; }
        else if (frameCount < 1000) { fileName = "00" + frameCount.ToString() + ".jpg"; }
        else if (frameCount < 10000) { fileName = "0" + frameCount.ToString() + ".jpg"; }
        else { fileName = frameCount.ToString() + ".jpg"; }
        Application.CaptureScreenshot(OutputPath + fileName);
        frameCount++;
        Time.timeScale = TimeDetela;
        InvokeRepeating("TakeCap", 1f / (float)targetFrame, 1f / (float)targetFrame);
    }

    void Awake()
    {
        Application.targetFrameRate = targetFrame;
    }


    void TakeCap()
    {
        if (!bStop)
        {
            Time.timeScale = 0;
            if (frameCount < 10) { fileName = "0000" + frameCount.ToString() + ".png"; }
            else if (frameCount < 100) { fileName = "000" + frameCount.ToString() + ".png"; }
            else if (frameCount < 1000) { fileName = "00" + frameCount.ToString() + ".png"; }
            else if (frameCount < 10000) { fileName = "0" + frameCount.ToString() + ".png"; }
            else { fileName = frameCount.ToString() + ".png"; }
            Application.CaptureScreenshot(OutputPath + fileName);
            frameCount++;
            Time.timeScale = TimeDetela;
        }
    }

    void StopCap()
    {
        bStop = true;
        Time.timeScale = 0;
        CancelInvoke("TakeCap");
        Debug.Log("渲染完毕");
    }


    void Update()
    {
        //  按ESC退出全屏  
        if (Input.GetKey(KeyCode.Escape))
        {
            Screen.fullScreen = false;  //退出全屏           
        }
        //设置7680*1080的全屏  
        if (Input.GetKey(KeyCode.J))
        {
            Screen.SetResolution(3840, 2160, true);
        }
        if (Input.GetKey(KeyCode.K))
        {
            Screen.SetResolution(Screen.width, Screen.height, true);
        }
        //按A全屏  
        if (Input.GetKey(KeyCode.L))
        {
            //获取设置当前屏幕分辩率  
            Resolution[] resolutions = Screen.resolutions;
            //设置当前分辨率  
            Screen.SetResolution(resolutions[resolutions.Length - 1].width, resolutions[resolutions.Length - 1].height, true);
            Screen.fullScreen = true;  //设置成全屏,  
        }
    }
}
 