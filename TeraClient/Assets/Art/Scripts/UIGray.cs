using Common;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 图片置灰
/// </summary>
public static class UIGray
{
    private static Material GrayMat;

    /// <summary>
    /// 创建置灰材质球
    /// </summary>
    /// <returns></returns>
    private static Material GetGrayMat()
    {
        if (GrayMat != null) return GrayMat;
        Shader shader = Shader.Find("UI/Gray");
        if (shader == null)
        {
            HobaDebuger.Log("UIGray cannt  find ui.gray ");
            return null;
        }
        Material mat = new Material(shader);
        GrayMat = mat;

        return GrayMat;
    }

    /// <summary>
    /// 图片置灰
    /// </summary>
    /// <param name="img"></param>
    public static void SetUIGray(Image img)
    {
        img.material = GetGrayMat();
        img.SetMaterialDirty();
    }

    /// <summary>
    /// 图片回复
    /// </summary>
    /// <param name="img"></param>
    public static void Recovery(Image img)
    {
        img.material = null;
    }

}

