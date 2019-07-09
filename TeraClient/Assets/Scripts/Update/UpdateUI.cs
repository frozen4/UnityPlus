using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Common;

public enum MessageBoxStyle
{
    MB_OK,
    MB_YesNo,
    MB_OkCancel,
}

public class WaitForUserClick : IEnumerator
{
    private GameObject _FrameMessage;
    private GameObject _FrameInfo;

    private GButton _BtnYes;
    private GButton _BtnNo;
    private RectTransform _RectBtnYes;
    private RectTransform _RectBtnNo;

    public static int RetCode = -1;

    public WaitForUserClick(MessageBoxStyle style,
        string text, string title, string tips = "", string num = "")
    {
        _FrameMessage = EntryPoint.Instance.PanelMessageBox.FindChild("Frame_Message");
        if (EntryPoint.Instance.PanelHotUpdate != null)
            _FrameInfo = EntryPoint.Instance.PanelHotUpdate.FindChild("Frame_Info");

        _BtnYes = _FrameMessage.FindChild("Btn_Yes").GetComponent<GButton>();
        _BtnNo = _FrameMessage.FindChild("Btn_No").GetComponent<GButton>();
        _BtnYes.OnClick = OnClickYes;
        _BtnNo.OnClick = OnClickNo;
        _RectBtnYes = _BtnYes.RectTrans;
        _RectBtnNo = _BtnNo.RectTrans;
        
        bool bShowTips = !string.IsNullOrEmpty(tips);
        var msgTips = _FrameMessage.FindChild("Lay_Content/Frame_Middle/Lab_DownloadTips").GetComponent<Text>();
        if (msgTips.gameObject.activeSelf != bShowTips)
            msgTips.gameObject.SetActive(bShowTips);
        if (bShowTips)
            msgTips.text = tips;

        var msgTitle = _FrameMessage.FindChild("Lab_MsgTitle").GetComponent<Text>();
        var msgText = _FrameMessage.FindChild("Lay_Content/Lab_Message").GetComponent<Text>();
        msgText.text = text;
        msgTitle.text = title;

        bool bShowNum = !string.IsNullOrEmpty(num);
        var msgNum = _FrameMessage.FindChild("Lay_Content/Frame_Middle/Lab_Number").GetComponent<Text>();
        if (msgNum.gameObject.activeSelf != bShowNum)
            msgNum.gameObject.SetActive(bShowNum);
        if (bShowNum)
            msgNum.text = num;

        var frameMiddle = _FrameMessage.FindChild("Lay_Content/Frame_Middle");
        if (frameMiddle.activeSelf != (bShowTips || bShowNum))
            frameMiddle.SetActive(bShowTips || bShowNum);

        RetCode = -1;

        OnShow(style);
    }

    private void OnShow(MessageBoxStyle style)
    {
        switch (style)
        {
            case MessageBoxStyle.MB_OK:
                ShowMessageBox(true);
                _RectBtnYes.anchoredPosition = new Vector2(0, _RectBtnYes.anchoredPosition.y);
                _BtnYes.gameObject.SetActive(true);
                _BtnNo.gameObject.SetActive(false);
                {
                    string strOk = UpdateInfoUtil.GetStateString(UPDATE_STATE.UpdateString_Ok);
                    if (!string.IsNullOrEmpty(strOk))
                        _BtnYes.text = strOk;
                }
                break;
            case MessageBoxStyle.MB_OkCancel:
                ShowMessageBox(true);
                _RectBtnYes.anchoredPosition = new Vector2(-_RectBtnNo.anchoredPosition.x, _RectBtnYes.anchoredPosition.y);
                _BtnYes.gameObject.SetActive(true);
                _BtnNo.gameObject.SetActive(true);
                {
                    string strOk = UpdateInfoUtil.GetStateString(UPDATE_STATE.UpdateString_Ok);
                    if (!string.IsNullOrEmpty(strOk))
                        _BtnYes.text = strOk;
                    string strCancel = UpdateInfoUtil.GetStateString(UPDATE_STATE.UpdateString_Cancel);
                    if (!string.IsNullOrEmpty(strCancel))
                        _BtnNo.text = strCancel;
                }
                break;
            case MessageBoxStyle.MB_YesNo:
                ShowMessageBox(true);
                _RectBtnYes.anchoredPosition = new Vector2(-_RectBtnNo.anchoredPosition.x, _RectBtnYes.anchoredPosition.y);
                _BtnYes.gameObject.SetActive(true);
                _BtnNo.gameObject.SetActive(true);
                {
                    string strYes = UpdateInfoUtil.GetStateString(UPDATE_STATE.UpdateString_Yes);
                    if (!string.IsNullOrEmpty(strYes))
                        _BtnYes.text = strYes;
                    string strNo = UpdateInfoUtil.GetStateString(UPDATE_STATE.UpdateString_No);
                    if (!string.IsNullOrEmpty(strNo))
                        _BtnNo.text = strNo;
                }
                break;
            default:
                ShowMessageBox(true);
                _RectBtnYes.anchoredPosition = new Vector2(0, _RectBtnYes.anchoredPosition.y);
                _BtnYes.gameObject.SetActive(true);
                _BtnNo.gameObject.SetActive(false);
                {
                    string strOk = UpdateInfoUtil.GetStateString(UPDATE_STATE.UpdateString_Ok);
                    if (!string.IsNullOrEmpty(strOk))
                        _BtnYes.text = strOk;
                }
                break;
        }
    }

    private void OnHide()
    {
        ShowMessageBox(false);
        _BtnYes.gameObject.SetActive(false);
        _BtnNo.gameObject.SetActive(false);
    }

    private void OnClickYes(GameObject go)
    {
        OnHide();
        _BtnYes.OnClick = null;
        _BtnYes.OnClick = null;
        RetCode = 1;
    }
    private void OnClickNo(GameObject go)
    {
        OnHide();
        _BtnYes.OnClick = null;
        _BtnYes.OnClick = null;
        RetCode = 0;
    }

    private void ShowMessageBox(bool isShowBox)
    {
        if (_FrameMessage.activeSelf != isShowBox)
            _FrameMessage.SetActive(isShowBox);
        if (_FrameInfo != null && _FrameInfo.activeSelf == isShowBox)
            _FrameInfo.SetActive(!isShowBox);
    }

    public object Current { get { return RetCode == -1 ? null : (object)RetCode; } }

    public bool MoveNext()
    {
        return RetCode == -1;
    }

    public void Reset()
    {
        RetCode = -1;
    }
}

/*
public class UpdateCircle : Singleton<UpdateCircle>
{
    public GameObject _FrameCircle;
    public GameObject _ImageCircle;
    bool _IsShow = false;

    public void Show(bool bShow)
    {
        if (_FrameCircle == null)
            _FrameCircle = GameUpdateMan.Instance.Frame_Circle;
        
        if (_ImageCircle == null)
            _ImageCircle = GameUpdateMan.Instance.Image_Circle;

        _IsShow = bShow;

        if (_FrameCircle != null)
            _FrameCircle.SetActive(bShow);
    }

    public void Tick()
    {
        if (_IsShow && _ImageCircle != null)
        {
            _ImageCircle.transform.Rotate(Vector3.forward, 2);
        }
    }
}
*/