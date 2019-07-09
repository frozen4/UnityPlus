using Common;
using System.IO;
using UnityEngine;

public enum VoiceMode
{
    None = 0,
    OffLine,
    RealTimeVoiceTeam,
    RealTimeVoiceNational,
    Translation,
}

public interface IVoiceModule
{
    bool IsStarted { get; }

    VoiceMode VoiceMode { get; }

    bool Start();

    void Update();

    void Stop();
}

public class VoiceManager : Singleton<VoiceManager>, GameLogic.ITickLogic
{
    private IVoiceModule m_ActiveModule = null;

    public bool IsEnabled
    {
        get;
        private set;
    }

    public string VoiceDir      //存放声音文件的目录
    {
        get;
        private set;
    }

    public string TempVoiceFile       //临时声音文件
    {
        get;
        private set;
    }

    public float TempVoiceLength
    {
        get;
        set;
    }

    public VoiceMode VoiceMode
    {
        get
        {
            if (m_ActiveModule != null)
                return m_ActiveModule.VoiceMode;
            else
                return VoiceMode.None;
        }
    }

#if USING_GCLOUDVOICE && UNITY_EDITOR

    private Gcloud_VoiceMan m_gcloudVoiceMan = null;
    private Gcloud_OffLineVoice m_OffLineVoice = null;
    private Gcloud_RealTimeVoiceNational m_RealTimeVoiceNational = null;
    private Gcloud_RealTimeVoiceTeam m_RealTimeVoiceTeam = null;
    private Gcloud_TranslationVoice m_TranslationVoice = null;

    public Gcloud_VoiceMan Gcloud_VoiceMan
    {
        get { return m_gcloudVoiceMan; }
    }

    public Gcloud_OffLineVoice OffLineVoice
    {
        get { return m_OffLineVoice; }
    }

    public Gcloud_RealTimeVoiceNational RealTimeVoiceNational
    {
        get { return m_RealTimeVoiceNational; }
    }

    public Gcloud_RealTimeVoiceTeam RealTimeVoiceTeam
    {
        get { return m_RealTimeVoiceTeam; }
    }

    public Gcloud_TranslationVoice TranslationVoice
    {
        get { return m_TranslationVoice; }
    }

#elif USING_LGVC
    private LGVC.LGVC_VoiceMan m_lgvcVoiceMan = null;
    private LGVC.LGVC_OffLineVoice m_OffLineVoice = null;

    public LGVC.LGVC_VoiceMan LGVC_VoiceMan
    {
        get { return m_lgvcVoiceMan; }
    }

    public LGVC.LGVC_OffLineVoice OffLineVoice
    {
        get { return m_OffLineVoice; }
    }

#endif

    public void Init()
    {
        if (EntryPoint.Instance.GameCustomConfigParams.UseVoice)
            IsEnabled = InitVoice();
        else
            IsEnabled = false;

        VoiceDir = EntryPoint.Instance.VoiceDir;

        TempVoiceFile = Path.Combine(VoiceDir, "tmp.wav");
        //TempVoiceFile = Path.Combine(VoiceDir, "tmp.mp3");
        TempVoiceLength = 0;

        if (!Directory.Exists(VoiceDir))
            Directory.CreateDirectory(VoiceDir);

        HobaDebuger.Log(HobaText.Format("VoiceManager Init IsEnabled: {0}, VoiceDir: {1}", IsEnabled, VoiceDir));
    }


    private bool InitVoice()
    {
#if USING_GCLOUDVOICE && UNITY_EDITOR
        m_gcloudVoiceMan = new Gcloud_VoiceMan();
        bool ret = m_gcloudVoiceMan.Init();

        if (ret)
        {
            m_OffLineVoice = new Gcloud_OffLineVoice(this, VoiceMode.OffLine);

            m_RealTimeVoiceNational = new Gcloud_RealTimeVoiceNational(this, VoiceMode.RealTimeVoiceNational);

            m_RealTimeVoiceTeam = new Gcloud_RealTimeVoiceTeam(this, VoiceMode.RealTimeVoiceTeam);

            m_TranslationVoice = new Gcloud_TranslationVoice(this, VoiceMode.Translation);
        }
        return ret;
#elif USING_LGVC
        m_lgvcVoiceMan = new GameObject("LGVC_VoiceMan").AddComponent<LGVC.LGVC_VoiceMan>();
        bool ret = m_lgvcVoiceMan.Init();

        if (ret)
        {
            m_OffLineVoice = new LGVC.LGVC_OffLineVoice(this, VoiceMode.OffLine);
        }

        return ret;
#else
        return false;
#endif 
    }

    public void Tick(float dt)
    {
        if (!IsEnabled)
            return;

        if (m_ActiveModule != null)
            m_ActiveModule.Update();
    }

    public bool SwitchToVoiceMode(VoiceMode mode)
    {
        if (!IsEnabled)
            return false;

        bool bStart = false;

#if USING_GCLOUDVOICE && UNITY_EDITOR
        switch (mode)
        {
            case VoiceMode.OffLine:
                {
                    m_gcloudVoiceMan.SetVoiceMode(mode);
                    {
                        if (m_ActiveModule != null)
                            m_ActiveModule.Stop();
                        m_ActiveModule = m_OffLineVoice;

                        if (!m_ActiveModule.IsStarted)
                            m_ActiveModule.Start();

                        bStart = m_ActiveModule.IsStarted;
                    }
                }
                break;

            case VoiceMode.RealTimeVoiceTeam:
                {
                    m_gcloudVoiceMan.SetVoiceMode(mode);
                    {
                        if (m_ActiveModule != null)
                            m_ActiveModule.Stop();
                        m_ActiveModule = m_RealTimeVoiceTeam;

                        if (!m_ActiveModule.IsStarted)
                            m_ActiveModule.Start();

                        bStart = m_ActiveModule.IsStarted;
                    }
                }
                break;

            case VoiceMode.RealTimeVoiceNational:
                {
                    m_gcloudVoiceMan.SetVoiceMode(mode);
                    {
                        if (m_ActiveModule != null)
                            m_ActiveModule.Stop();
                        m_ActiveModule = m_RealTimeVoiceNational;

                        if (!m_ActiveModule.IsStarted)
                            m_ActiveModule.Start();

                        bStart = m_ActiveModule.IsStarted;
                    }
                }
                break;

            case VoiceMode.Translation:
                {
                    m_gcloudVoiceMan.SetVoiceMode(mode);
                    {
                        if (m_ActiveModule != null)
                            m_ActiveModule.Stop();
                        m_ActiveModule = m_TranslationVoice;

                        if (!m_ActiveModule.IsStarted)
                            m_ActiveModule.Start();

                        bStart = m_ActiveModule.IsStarted;
                    }
                }
                break;


            default:
                if (m_ActiveModule != null)
                    m_ActiveModule.Stop();
                m_ActiveModule = null;
                break;
        }

#elif USING_LGVC
        switch (mode)
        {
            case VoiceMode.OffLine:
                {
                    if (m_ActiveModule != null)
                        m_ActiveModule.Stop();
                    m_ActiveModule = m_OffLineVoice;

                    if (!m_ActiveModule.IsStarted)
                        m_ActiveModule.Start();

                    bStart = m_ActiveModule.IsStarted;
                }
                break;
            default:
                if (m_ActiveModule != null)
                    m_ActiveModule.Stop();
                m_ActiveModule = null;
                break;
        }
#endif
        return bStart;
    }

    public void TestPlayVoice()
    {
#if USING_GCLOUDVOICE && UNITY_EDITOR
        string path = Path.Combine(VoiceManager.Instance.VoiceDir, "1234-2def7ded-a621-47d4-8cbf-597c2587def0.wav");
        Gcloud_VoiceMan.PlayRecordedFile(path);
#elif USING_LGVC
        string path = Path.Combine(VoiceManager.Instance.VoiceDir, "1234-2def7ded-a621-47d4-8cbf-597c2587def0.wav");
        m_lgvcVoiceMan.PlayRecordedFile(path);
#endif
    }
}