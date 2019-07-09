using UnityEngine;
using UnityEngine.UI;
using GNewUI;

public class InteractableUIHolder : MonoBehaviour
{
    public GButton[] GBtns;
    public GImageModel[] ImageModels;
    public Button[] Buttons;
    public Toggle[] Toggles;
    public Scrollbar[] Scrollbars;
    public InputField[] InputFields;
    public Dropdown[] Dropdowns;

    //GNewUI
    public GNewListBase[] NewLists;
    public GNewTabList[] NewTabLists;
    public GNewLayoutTable[] NewLayoutTables;
    //public GUIAnim[] newUIAnims;

    public DG.Tweening.DOTweenAnimation[] newDOTAnims;
    //public GEventTrigger[] GEventTriggers;
    public GScrollWatcher[] ScrollDrags;
    public GSlideButton[] SlideButtons;

    public GWebView[] newWebViews;
    public GDragablePageView[] dragablePageViews;

    //Panels
    [System.Serializable]
    public struct PanelInfo
    {
        public GameObject Panel;
        public bool DontOverrideSorting;
        public int OrderOffset;
        public bool IsIgnoreResizng;
        public bool IsKeptFullScreen;
        public bool IsNeedBlockRay;

        public Vector2 OffsetMax;
        public Vector2 OffsetMin;


        [System.NonSerialized]
        public Vector4 LastArea;
    }

    public PanelInfo[] PanelInfoList;

    public void SetupUISorting(int layer, int base_order)
    {
        GameObject g = gameObject;
        Canvas root_canv = g.GetComponent<Canvas>();
        if (root_canv == null)
        {
            root_canv = g.AddComponent<Canvas>();
        }
        GNewUITools.SetupUILayerOrder(root_canv, base_order, layer, true);

        if (PanelInfoList != null && PanelInfoList.Length > 0)
        {
            for (int i = 0; i < PanelInfoList.Length; i++)
            {
                GameObject g_panel = PanelInfoList[i].Panel;
                if (!PanelInfoList[i].DontOverrideSorting)
                {
                    Canvas canv = g_panel.GetComponent<Canvas>();
                    if (canv == null)
                    {
                        canv = g_panel.AddComponent<Canvas>();
                    }
                    GNewUITools.SetupUILayerOrder(canv, PanelInfoList[i].OrderOffset + base_order, layer, PanelInfoList[i].IsNeedBlockRay);
                }
            }
        }

        //UI TODO: Cache usa?
        UISizeAdapter usa = g.GetComponentInParent<UISizeAdapter>();
        if (usa != null)
        {
            usa.Register(g);
        }
    }
}
