using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class HUDTextMan : MonoBehaviourSingleton<HUDTextMan>
{
    //public CanvasScaler CanvasScaler;

    private string[] FontConfigFiles = null;
    private string[] KRFontConfigFiles = null;
    private float _CanvasScaleFactor = float.NaN;
    private Dictionary<int, GBMFontAndMotionTextModel> _ModelCache = new Dictionary<int, GBMFontAndMotionTextModel>();
    private UnityObjectPool _HudTextPool = null;

    private RectTransform _rectTransform;

    public void Init()
    {
        // 避免了字符串的拼接和Lua->C#的string传递
        FontConfigFiles = new string[] {
            "MotionConfig/attack_normal",
            "MotionConfig/attack_crit",
            "MotionConfig/under_attack_crit",
            "MotionConfig/under_attack_normal",
            "MotionConfig/heal",
            "MotionConfig/hitrecoverey",
            "MotionConfig/get_coin",
            "MotionConfig/get_exp",
            "MotionConfig/get_exp_profession",
            "MotionConfig/attack_xishou",
            "MotionConfig/attack_gedang",
            "MotionConfig/under_attack_xishou",
            "MotionConfig/under_attack_gedang",
            "MotionConfig/attack_daduan",
            "MotionConfig/under_attack_daduan",
            "MotionConfig/attack_elem_light",
            "MotionConfig/attack_elem_dark",
            "MotionConfig/attack_elem_ice",
            "MotionConfig/attack_elem_fire",
            "MotionConfig/attack_elem_wind",
            "MotionConfig/attack_elem_thunder",
            "MotionConfig/attack_elem_light_c",
            "MotionConfig/attack_elem_dark_c",
            "MotionConfig/attack_elem_ice_c",
            "MotionConfig/attack_elem_fire_c",
            "MotionConfig/attack_elem_wind_c",
            "MotionConfig/attack_elem_thunder_c",
        };

        KRFontConfigFiles = new string[] {
            "MotionConfig/attack_normal",
            "MotionConfig/attack_crit",
            "MotionConfig/under_attack_crit",
            "MotionConfig/under_attack_normal",
            "MotionConfig/heal",
            "MotionConfig/hitrecoverey",
            "MotionConfig/get_coin",
            "MotionConfig/get_exp",
            "MotionConfig/get_exp_profession",
            "MotionConfig/attack_xishou_kr",
            "MotionConfig/attack_gedang_kr",
            "MotionConfig/under_attack_xishou_kr",
            "MotionConfig/under_attack_gedang_kr",
            "MotionConfig/attack_daduan_kr",
            "MotionConfig/under_attack_daduan_kr",
            "MotionConfig/attack_elem_light",
            "MotionConfig/attack_elem_dark",
            "MotionConfig/attack_elem_ice",
            "MotionConfig/attack_elem_fire",
            "MotionConfig/attack_elem_wind",
            "MotionConfig/attack_elem_thunder",
            "MotionConfig/attack_elem_light_c",
            "MotionConfig/attack_elem_dark_c",
            "MotionConfig/attack_elem_ice_c",
            "MotionConfig/attack_elem_fire_c",
            "MotionConfig/attack_elem_wind_c",
            "MotionConfig/attack_elem_thunder_c",
        };

        _rectTransform = GetComponent<RectTransform>();
        if (_rectTransform == null)
        {
            _rectTransform = gameObject.AddComponent<RectTransform>();
        }

        var hudtext = Resources.Load<GameObject>("UI/HUDText");
        _HudTextPool = new UnityObjectPool(hudtext, 10);

        HUDText.OnComplete = RecycleText;
    }

    public void ShowText(string text, Vector3 pos, int configId)
    {
        if (string.IsNullOrEmpty(text) || Main.Main3DCamera == null || !Main.Main3DCamera.enabled)
            return;

        var configs = FontConfigFiles;
        if (EntryPoint.Instance.GetUserLanguageCode() == "KR")
            configs = KRFontConfigFiles;

        if (configs == null || configId >= configs.Length || configId < 0)
        {
            Common.HobaDebuger.LogWarningFormat("HUD Text Config id is wrong");
            return;
        }

        GBMFontAndMotionTextModel model = null;
        if (!_ModelCache.TryGetValue(configId, out model))
        {
            model = Resources.Load<GBMFontAndMotionTextModel>(configs[configId]);
            _ModelCache.Add(configId, model);
        }
        
        if (model != null)
        {
            var hudtext = _HudTextPool.Get() as GameObject;
            //hudtext.SetActive(true);
            var hudTextComp = hudtext.GetComponent<HUDText>();
            if(hudTextComp != null)
            {
                if (model._IsOnTopLayer)
                    hudTextComp.RectTrans.SetParent(transform.GetChild(1), false);
                else
                    hudTextComp.RectTrans.SetParent(transform.GetChild(0), false);

                pos.x += UnityEngine.Random.Range(-model._XRandomNum, model._XRandomNum);
                pos.y += UnityEngine.Random.Range(-model._XRandomNum, model._XRandomNum);
                pos.z += UnityEngine.Random.Range(-model._XRandomNum, model._XRandomNum);

                Vector3 uipos = TranslatePosToUIPos(pos);

                //Debug.LogWarning("uipos " + uipos);
                hudTextComp.Show(text, uipos, model);
            }
        }
    }

    private void RecycleText(HUDText hudtext)
    {
        if (hudtext == null) return;

        hudtext.RectTrans.anchoredPosition3D = new Vector3(10000, 0, 0);
        var go = hudtext.gameObject;
        if (_HudTextPool != null)
            _HudTextPool.Release(go);
    }

    private Vector3 TranslatePosToUIPos(Vector3 pos)
    {
        Rect rect=_rectTransform.rect;
        //Rect rect_c=Main.Main3DCamera.pixelRect;

        Vector2 a = RectTransformUtility.WorldToScreenPoint(Main.Main3DCamera, pos);
        var uipos = new Vector3(a.x * rect.width / Screen.width, a.y * rect.height / Screen.height, 0);

        //Debug.LogWarning(pos + ", " + uipos);

        return uipos;
    }

    public void Clear()
    {
        _ModelCache.Clear();
        _HudTextPool.ReleaseAll();
    }
}
