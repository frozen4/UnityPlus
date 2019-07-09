using UnityEngine;
using Common;
using System;
using System.Collections;

#if USING_GCLOUDVOICE

public class Gcloud_VoiceMan
{
    private gcloud_voice.IGCloudVoice m_voiceengine = null;

    public gcloud_voice.IGCloudVoice VoiceEngine
    {
        get { return m_voiceengine; }
    }

    public bool Init()
    {
        int ret = 0;

        string AppID = EntryPoint.Instance.GameCustomConfigParams.GCloud_AppID;
        string AppKey = EntryPoint.Instance.GameCustomConfigParams.GCloud_AppKey;

        if (m_voiceengine == null)
        {
            m_voiceengine = gcloud_voice.GCloudVoice.GetEngine();
            System.TimeSpan ts = System.DateTime.UtcNow - new System.DateTime(1970, 1, 1, 0, 0, 0, 0);
            string strTime = System.Convert.ToInt64(ts.TotalSeconds).ToString();

            ret = m_voiceengine.SetAppInfo(AppID, AppKey, strTime);
            if (ret != 0)
            {
                m_voiceengine = null;
                HobaDebuger.LogError(HobaText.Format("IGCloudVoice SetAppInfo Failed! ret: {0}", ret));
            }
            else
            {
                ret = m_voiceengine.Init();
                if (ret != 0)
                {
                    m_voiceengine = null;
                    HobaDebuger.LogError(HobaText.Format("IGCloudVoice Init Failed! ret: {0}", ret));
                }
            }
        }

        return ret == 0;
    }

    public bool SetVoiceMode(VoiceMode mode)
    {
        if (m_voiceengine == null)
            return false;
        switch (mode)
        {
            case VoiceMode.OffLine:
                m_voiceengine.SetMode(gcloud_voice.GCloudVoiceMode.Messages);
                break;
            case VoiceMode.RealTimeVoiceTeam:
                m_voiceengine.SetMode(gcloud_voice.GCloudVoiceMode.RealTime);
                break;
            case VoiceMode.RealTimeVoiceNational:
                m_voiceengine.SetMode(gcloud_voice.GCloudVoiceMode.RealTime);
                break;
            case VoiceMode.Translation:
                m_voiceengine.SetMode(gcloud_voice.GCloudVoiceMode.Translation);
                break;
            default:
                break;
        }
        return true;
    }

    public int DownloadRecordedFile(string fileId, string downloadPath, int timeout)
    {
        return m_voiceengine.DownloadRecordedFile(fileId, downloadPath, timeout);
    }

    public int UploadRecordedFile(string recordPath, int timeout)
    {
        return m_voiceengine.UploadRecordedFile(recordPath, timeout);
    }

    public int PlayRecordedFile(string downloadFilePath)
    {
        return m_voiceengine.PlayRecordedFile(downloadFilePath);
    }

    public int StopPlayFile()
    {
        return m_voiceengine.StopPlayFile();
    }

    public int StartRecording(string recordPath)
    {
        return m_voiceengine.StartRecording(recordPath);
    }

    public int StopRecording(Action<int> callback)
    {
        int ret = m_voiceengine.StopRecording();

        if (ret != 0)
        {
            VoiceManager.Instance.TempVoiceLength = 0;
        }
        else
        {
            VoiceManager.Instance.TempVoiceLength = GetRecFileLength(VoiceManager.Instance.TempVoiceFile);
        }

        callback(ret);
        return ret;
    }

    private int[] _bytes = new int[1];
    private float[] _seconds = new float[1];
    public float GetRecFileLength(string filePath)
    {
        float seconds = 0;
        _seconds[0] = 0;
        int ret = m_voiceengine.GetFileParam(filePath, _bytes, _seconds);
        if (ret != 0)
        {
            seconds = 0;
        }
        else
        {
            seconds = _seconds[0];
        }

        return seconds;
    }

    public int SpeechToText(string fileID, int language, int msTimeout)
    {
        return m_voiceengine.SpeechToText(fileID, language, msTimeout);
    }
}

#endif