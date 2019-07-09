using UnityEngine;

namespace EntityVisualEffect
{
    public enum RenderingMode
    {
        Opaque,
        CutOut,
        Fade,
        Transparent,
        CutOutTransparent,
    }

    public static class MaterialUtility
    {
        public const int RenderQueue_Background = 1000;
        public const int RenderQueue_Geometry = 2000;
        public const int RenderQueue_AlphaTest = 2450;
        public const int RenderQueue_Transparent = 3000;
        public const int RenderQueue_Overlay = 4000;

        public static void SetMaterialRenderingMode(Material material, RenderingMode renderingMode)
        {
             switch (renderingMode) 
             {
                case RenderingMode.Opaque:
                    material.SetInt (ShaderIDs.SrcBlend, (int)UnityEngine.Rendering.BlendMode.One);
                    material.SetInt (ShaderIDs.DstBlend, (int)UnityEngine.Rendering.BlendMode.Zero);
                    material.SetInt (ShaderIDs.ZWrite, 1);
                    material.DisableKeyword ("_ALPHATEST_ON");
                    material.DisableKeyword ("_ALPHABLEND_ON");
                    material.DisableKeyword ("_ALPHAPREMULTIPLY_ON");
                    material.renderQueue = RenderQueue_Geometry;
                    break;
                case RenderingMode.CutOut:
                    material.SetInt(ShaderIDs.SrcBlend, (int)UnityEngine.Rendering.BlendMode.One);
                    material.SetInt(ShaderIDs.DstBlend, (int)UnityEngine.Rendering.BlendMode.Zero);
                    material.SetInt(ShaderIDs.ZWrite, 1);
                    material.EnableKeyword("_ALPHATEST_ON");
                    material.DisableKeyword("_ALPHABLEND_ON");
                    material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    material.renderQueue = RenderQueue_AlphaTest;
                    break;
                 case RenderingMode.Fade:
                    material.SetInt(ShaderIDs.SrcBlend, (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    material.SetInt (ShaderIDs.DstBlend, (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    material.SetInt(ShaderIDs.ZWrite, 0);
                    material.DisableKeyword ("_ALPHATEST_ON");
                    material.EnableKeyword ("_ALPHABLEND_ON");
                    material.DisableKeyword ("_ALPHAPREMULTIPLY_ON");
                    material.renderQueue = RenderQueue_Transparent;
                    break;
                 case RenderingMode.Transparent:
                    material.SetInt(ShaderIDs.SrcBlend, (int)UnityEngine.Rendering.BlendMode.One);
                    material.SetInt(ShaderIDs.DstBlend, (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    material.SetInt(ShaderIDs.ZWrite, 0);
                    material.DisableKeyword("_ALPHATEST_ON");
                    material.DisableKeyword("_ALPHABLEND_ON");
                    material.EnableKeyword("_ALPHAPREMULTIPLY_ON");
                    material.renderQueue = RenderQueue_Transparent;
                    break;
                 case RenderingMode.CutOutTransparent:
                    material.SetInt(ShaderIDs.SrcBlend, (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    material.SetInt (ShaderIDs.DstBlend, (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    material.SetInt (ShaderIDs.ZWrite, 0);
                    material.EnableKeyword("_ALPHATEST_ON");
                    material.EnableKeyword("_ALPHABLEND_ON");
                    material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    material.renderQueue = RenderQueue_Transparent;
                    break;
                 default:
                     break;
             }
        }
    }
}
