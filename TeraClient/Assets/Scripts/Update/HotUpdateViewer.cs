using System.Collections;
using UnityEngine.UI;
using Common;
using UnityEngine;
using System.IO;


public class HotUpdateViewer
{
    private bool pIsJustShowNew = true; //是否只展示新UI
    private Text pCurrenVersion = null;
    private Text pServerVersion = null;
    private Text pTextDescription = null;
    private Slider pSliderPart = null;
    private Slider pSliderAll = null;

    private Text pTextTips = null;
    private Text pTextSize = null;
    private Text pTextProgress = null;
    private Text pTextPackage = null;
    private Slider pSlderProgress = null;

    private GameObject pFrameVideoD = null;
    private GameObject pFrameVideoU = null;
    private GameObject pFramePictureD = null;
    private GameObject pFramePictureU = null;
    private Toggle pToggleVideo = null;
    private GNewList pListProgress = null;
    private RawImage pRawImageVideo = null;
    private Image pImagePicture = null;

    private Text pTextCircle = null;
    
    public GameObject Frame_Message = null;
    public GameObject Frame_Progress = null;
    public GameObject Frame_Promotion = null;

    private bool _IsShowingVideo = false;
    private int _CurShowIndex = -1;
    private bool _IsAutoEnd = false;

    private Coroutine _RefreshCorutine = null;

    public HotUpdateViewer()
    {
        var panelHotUpdate = EntryPoint.Instance.PanelHotUpdate;

        pCurrenVersion = panelHotUpdate.FindChild("Lab_Tips1").GetComponent<Text>();

        pServerVersion = panelHotUpdate.FindChild("Lab_Tips2").GetComponent<Text>();

        pTextDescription = panelHotUpdate.FindChild("Lab_Tips3").GetComponent<Text>();
        if (pIsJustShowNew)
            pTextDescription.gameObject.SetActive(false);

        pSliderPart = panelHotUpdate.FindChild("Sld_Part").GetComponent<Slider>();
        if (pIsJustShowNew)
            pSliderPart.gameObject.SetActive(false);

        pSliderAll = panelHotUpdate.FindChild("Sld_All").GetComponent<Slider>();
        if (pIsJustShowNew)
            pSliderAll.gameObject.SetActive(false);

        Frame_Message = EntryPoint.Instance.PanelMessageBox.FindChild("Frame_Message");
        Frame_Message.SetActive(false);

        pTextTips = panelHotUpdate.FindChild("Frame_Info/Lab_Tips4").GetComponent<Text>();
        pTextSize = panelHotUpdate.FindChild("Frame_Info/Lab_Size").GetComponent<Text>();
        pTextSize.gameObject.SetActive(false);
        Frame_Progress = panelHotUpdate.FindChild("Frame_Info/Frame_Progress");
        Frame_Progress.SetActive(false);
        pTextProgress = Frame_Progress.FindChild("Lab_Package/Lab_Progress").GetComponent<Text>();
        pTextPackage = Frame_Progress.FindChild("Lab_Package").GetComponent<Text>();
        pSlderProgress = Frame_Progress.FindChild("Mask_Sld/Sld_New").GetComponent<Slider>();

        Frame_Promotion = panelHotUpdate.FindChild("Frame_Info/Frame_Promotion");
        pFrameVideoD = Frame_Promotion.FindChild("RdoGroup_TopTabs/Rdo_Video/Img_D");
        pFrameVideoU = Frame_Promotion.FindChild("RdoGroup_TopTabs/Rdo_Video/Img_U");
        pFramePictureD = Frame_Promotion.FindChild("RdoGroup_TopTabs/Rdo_Picture/Img_D");
        pFramePictureU = Frame_Promotion.FindChild("RdoGroup_TopTabs/Rdo_Picture/Img_U");
        pToggleVideo = Frame_Promotion.FindChild("RdoGroup_TopTabs/Rdo_Video").GetComponent<Toggle>();
        pListProgress = Frame_Promotion.FindChild("List_Progress").GetComponent<GNewList>();
        pRawImageVideo = Frame_Promotion.FindChild("Img_Video").GetComponent<RawImage>();
        pImagePicture = Frame_Promotion.FindChild("Img_Picture").GetComponent<Image>();
        Frame_Promotion.SetActive(false);
        pFrameVideoD.SetActive(false);
        pFramePictureD.SetActive(false);

        Frame_Promotion.FindChild("RdoGroup_TopTabs/Rdo_Video").GetComponent<Toggle>().onValueChanged.AddListener((isCheck) => { OnToggleTopTabs(true, isCheck); });
        Frame_Promotion.FindChild("RdoGroup_TopTabs/Rdo_Picture").GetComponent<Toggle>().onValueChanged.AddListener((isCheck) => { OnToggleTopTabs(false, isCheck); });
        Frame_Promotion.FindChild("Btn_SwitchLeft").GetComponent<Button>().onClick.AddListener(() => { OnClickSwitch(true); });
        Frame_Promotion.FindChild("Btn_SwitchRight").GetComponent<Button>().onClick.AddListener(() => { OnClickSwitch(false); });

        pTextCircle = EntryPoint.Instance.PanelCirle.FindChild("Lab_Circle").GetComponent<Text>();
    }

    private void OnToggleTopTabs(bool isVideo, bool isCheck)
    {
        if (isVideo)
        {
            if (pFrameVideoU.activeSelf == isCheck)
                pFrameVideoU.SetActive(!isCheck);
            if (pFrameVideoD.activeSelf != isCheck)
                pFrameVideoD.SetActive(isCheck);
            if (_IsShowingVideo) return;
            SwitchPromotion(true);
        }
        else
        {
            if (pFramePictureU.activeSelf == isCheck)
                pFramePictureU.SetActive(!isCheck);
            if (pFramePictureD.activeSelf != isCheck)
                pFramePictureD.SetActive(isCheck);
            if (!_IsShowingVideo) return;
            SwitchPromotion(false);
        }
    }

    private void OnClickSwitch(bool isLeft)
    {
        int index = GetPromotionIndex(isLeft);
        if (index == _CurShowIndex) return;

        if (_IsShowingVideo)
        {
            _IsAutoEnd = false;
            SelectVideo(index);
        }
        else
            SelectPicture(index);
    }

    private int GetPromotionIndex(bool isPrevious)
    {
        int ret = isPrevious ? _CurShowIndex-1 : _CurShowIndex+1;
        var paths = _IsShowingVideo ? EntryPoint.Instance.UpdatePromotionParams.VideoPaths : EntryPoint.Instance.UpdatePromotionParams.PicturePaths;
        if (isPrevious)
        {
            if (ret < 0)
                ret = paths.Count - 1;
        }
        else
        {
            if (ret >= paths.Count)
                ret = 0;
        }
        return ret;
    }

    public void SetAllProgress(float progress)
    {
        if (progress < 0.0f) return;

        float value = 0.0f;
        if (progress > 1.0f)
            value = 1.0f;
        else
            value = progress;

        if (pIsJustShowNew)
            pSliderAll.value = value;
    }

    public void SetPartProgress(float progress)
    {
        if (pIsJustShowNew || progress < 0.0f) return;

        float value = 0.0f;
        if (progress > 1.0f)
            value = 1.0f;
        else
            value = progress;

        pSliderPart.value = value;
    }

    public void SetDesc(string desc)
    {
        if (pIsJustShowNew) return;

        pTextDescription.text = desc;
    }

    public void PrepareEnterGame()
    {
        pTextPackage.text = string.Empty;
        pTextProgress.gameObject.SetActive(false);
        pTextSize.gameObject.SetActive(false);
    }

    public void SetEnterGameProgress(float progress)
    {
        if (progress < 0.0f) return;

        float value = 0.0f;
        if (progress > 1.0f)
            value = 1.0f;
        else
            value = progress;
        
        pSlderProgress.value = value;
        pTextProgress.text = HobaText.Format("{0:0} %", value * 100);
        if (!Frame_Progress.activeSelf)
            Frame_Progress.SetActive(true);
    }

    public void SetEnterGameTips(string tips)
    {
        pTextTips.text = tips;
    }

    public void SetPackageNum(int current, int total)
    {
        pTextPackage.text = HobaText.Format("({0}/{1})", current, total);
    }

    private string _InstallInfo = "";
    public void SetInstallInfo(string str)
    {
        _InstallInfo = str;
        SetInstallText(_InstallInfo, _InstallPercent);
    }
    
    private float _InstallPercent = -1.0f;
    public void SetInstallPercent(float fPercent)
    {
        float _InstallPercent = fPercent;
        SetInstallText(_InstallInfo, _InstallPercent);
    }

    public void SetInstallText(string installInfo, float percent)
    {
        pTextTips.text = installInfo;
        //bool bShow = percent >= 0f;
        //if (pTextSize.gameObject.activeSelf != bShow)
        //    pTextSize.gameObject.SetActive(bShow);
        //if (bShow)
        //    pTextSize.text = HobaText.Format("({0:0} %)", percent * 100.0f);
    }

    public void SetInstallProgress(int current, int total)
    {
        _InstallPercent = (float) current / total;
        SetInstallText(_InstallInfo, current, total);
    }

    public void SetInstallText(string installInfo, int current, int total)
    {
        pTextTips.text = installInfo;
        bool bShow = ((float)current / total) >= 0f;
        if (pTextSize.gameObject.activeSelf != bShow)
            pTextSize.gameObject.SetActive(bShow);
        if (bShow)
            pTextSize.text = HobaText.Format("({0} / {1})", current, total);
    }

    //多语言需要设置
    public void SetDownloadInfo(string str1, string str2, string str3)
    {
    }

    public void SetTopTabsTitle(string videoTitle, string pictureTitle)
    {
        pFrameVideoU.FindChild("Lab_Name").GetComponent<Text>().text = videoTitle;
        pFrameVideoD.FindChild("Lab_Name").GetComponent<Text>().text = videoTitle;
        pFramePictureU.FindChild("Lab_Name").GetComponent<Text>().text = pictureTitle;
        pFramePictureD.FindChild("Lab_Name").GetComponent<Text>().text = pictureTitle;
    }

    public void SetDownloadInfo_TextUpate(string str1)
    {
        pTextTips.text = str1;
    }
    
    private long finishedSize;
    private long totalSize;
    private float fKBSpeed;
    public void SetFileDownloadInfo(long finishedSize, long totalSize, float fKBS)
    {
        this.finishedSize = finishedSize;
        this.totalSize = totalSize;
        this.fKBSpeed = fKBS;

        SetTextSize();

        float value = totalSize > 0 ? (float)finishedSize / (float)totalSize : 0;
        pSlderProgress.value = value;
        pTextProgress.text = HobaText.Format("{0:0} %", value * 98);
        if (!Frame_Progress.activeSelf)
            Frame_Progress.SetActive(true);
    }

    private void SetTextSize()
    {
        string sizeStr = string.Empty;
        float fMB = totalSize / (1024.0f * 1024.0f);
        if (fMB >= 1f)
        {
            float finished = finishedSize / (1024.0f * 1024.0f);
            finished = ((finished * 10) - (finished * 10) % 1) / 10; //保留一位小数
            string finishedStr = string.Empty;
            if (finished % 1 > 0)
                finishedStr = HobaText.Format("{0:0.0} MB", finished);
            else
                finishedStr = HobaText.Format("{0:0} MB", finished);

            string totalStr = string.Empty;
            fMB = ((fMB * 10) - (fMB * 10) % 1) / 10; //保留一位小数
            if (fMB % 1 > 0)
                totalStr = HobaText.Format("{0:0.0} MB", fMB);
            else
                totalStr = HobaText.Format("{0:0} MB", fMB);

            sizeStr = HobaText.Format("{0} / {1}", finishedStr, totalStr);
        }
        else
        {
            float fKB = totalSize / 1024.0f;
            float finished = finishedSize / 1024.0f;
            sizeStr = HobaText.Format("{0:0} KB / {1:0} KB", finished, fKB);
        }

        float fMBSpeed = fKBSpeed / 1024.0f;
        string speedStr = string.Empty;
        if (fMBSpeed == 0f)
            speedStr = "0 KB/s";
        else if (fMBSpeed >= 1f)
        {
            fMBSpeed = ((fMBSpeed * 10) - (fMBSpeed * 10) % 1) / 10; //保留一位小数
            if (fMBSpeed % 1 > 0)
                //有小数点
                speedStr = HobaText.Format("{0:0.0} MB/s", fMBSpeed);
            else
                speedStr = HobaText.Format("{0:0} MB/s", fMBSpeed);
        }
        else
        {
            speedStr = HobaText.Format("{0:0} KB/s", fKBSpeed);
        }

        if (!pTextSize.gameObject.activeSelf)
            pTextSize.gameObject.SetActive(true);
        pTextSize.text = HobaText.Format("({0})({1})", sizeStr, speedStr);
    }

    public void SetCurrentVersion(string strVersion)
    {
        pCurrenVersion.text = HobaText.Format("{0}{1}", UpdateInfoUtil.GetStateString(UPDATE_STATE.UpdateString_CurrentVersion), strVersion);
    }

    public void SetServerVersion(string strVersion)
    {
        pServerVersion.text = HobaText.Format("{0}{1}", UpdateInfoUtil.GetStateString(UPDATE_STATE.UpdateString_ServerVersion), strVersion);
    }

    public void SetCircle(bool bShow, string strContent = null)
    {
        var panelCircle = EntryPoint.Instance.PanelCirle;
        if (panelCircle.activeSelf != bShow)
            panelCircle.SetActive(bShow);
        if (bShow && strContent != null)
            pTextCircle.text = strContent;
    }

    public void StartPromotion()
    {
        _IsShowingVideo = true;
        pToggleVideo.isOn = true;
        Frame_Promotion.SetActive(true);
        SwitchPromotion(true);
    }

    public void SwitchPromotion(bool isVideo)
    {
        _IsShowingVideo = isVideo;
        var paths = isVideo ? EntryPoint.Instance.UpdatePromotionParams.VideoPaths : EntryPoint.Instance.UpdatePromotionParams.PicturePaths;
        pListProgress.SetItemCount(paths.Count);

        if (pRawImageVideo.gameObject.activeSelf != isVideo)
            pRawImageVideo.gameObject.SetActive(isVideo);
        if (pImagePicture.gameObject.activeSelf == isVideo)
            pImagePicture.gameObject.SetActive(!isVideo);

        _CurShowIndex = 0;
        _IsAutoEnd = false;
        EntryPoint.Instance.VideoManager.StopVideo();
        if (_RefreshCorutine != null)
        {
            EntryPoint.Instance.StopCoroutine(_RefreshCorutine);
            _RefreshCorutine = null;
        }

        if (isVideo)
        {
            SelectVideo(0);
        }
        else
            SelectPicture(0);
    }
    
    public void SelectVideo(int index)
    {
        var paths = EntryPoint.Instance.UpdatePromotionParams.VideoPaths;
        if (paths.Count < index + 1) return;
        _CurShowIndex = index;

        pListProgress.SetSelection(index);
        string videoPath = Path.Combine(Application.streamingAssetsPath, paths[index]);
        System.Action onEnd = () =>
        {
            if (_IsAutoEnd)
            {
                EntryPoint.Instance.VideoManager.StopVideo();
                SelectVideo(GetPromotionIndex(false));
            }
        };
        EntryPoint.Instance.VideoManager.PlayVideo(videoPath, pRawImageVideo, null, onEnd, false);
        _IsAutoEnd = true;
    }

    public void SelectPicture(int index)
    {
        var paths = EntryPoint.Instance.UpdatePromotionParams.PicturePaths;
        if (paths.Count < index + 1) return;
        _CurShowIndex = index;

        pListProgress.SetSelection(index);

        var path = paths[index];
        int lastIndex = path.LastIndexOf('.');
        if (lastIndex >= 0)
            path = path.Substring(0, lastIndex);
        var sprite = Resources.Load<Sprite>(path);
        pImagePicture.sprite = sprite;

        if(_RefreshCorutine != null)
            EntryPoint.Instance.StopCoroutine(_RefreshCorutine);
        _RefreshCorutine = EntryPoint.Instance.StartCoroutine(RefreshPicture().GetEnumerator());
    }

    private IEnumerable RefreshPicture()
    {
        yield return new WaitForSeconds(15);
        SelectPicture(GetPromotionIndex(false));
    }

    public void Destroy()
    {
        _IsAutoEnd = false;
        EntryPoint.Instance.VideoManager.StopVideo();
        if (_RefreshCorutine != null)
        {
            EntryPoint.Instance.StopCoroutine(_RefreshCorutine);
            _RefreshCorutine = null;
        }
    }
}