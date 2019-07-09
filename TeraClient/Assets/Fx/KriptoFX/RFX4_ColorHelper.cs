using System;
using UnityEngine;

public static class RFX4_ColorHelper
{
    const float TOLERANCE = 0.0001f;

    public struct HSBColor
    {
        public float H;
        public float S;
        public float B;
        public float A;

        public HSBColor(float h, float s, float b, float a)
        {
            this.H = h;
            this.S = s;
            this.B = b;
            this.A = a;
        }
    }

    public static HSBColor ColorToHSV(Color color)
    {
        HSBColor ret = new HSBColor(0f, 0f, 0f, color.a);

        float r = color.r;
        float g = color.g;
        float b = color.b;

        float max = Mathf.Max(r, Mathf.Max(g, b));

        if (max <= 0)
            return ret;

        float min = Mathf.Min(r, Mathf.Min(g, b));
        float dif = max - min;

        if (max > min)
        {
            if (Math.Abs(g - max) < TOLERANCE)
                ret.H = (b - r)/dif*60f + 120f;
            else if (Math.Abs(b - max) < TOLERANCE)
                ret.H = (r - g)/dif*60f + 240f;
            else if (b > g)
                ret.H = (g - b)/dif*60f + 360f;
            else
                ret.H = (g - b)/dif*60f;
            if (ret.H < 0)
                ret.H = ret.H + 360f;
        }
        else
            ret.H = 0;

        ret.H *= 1f/360f;
        ret.S = (dif/max)*1f;
        ret.B = max;

        return ret;
    }

    public static Color HSVToColor(HSBColor hsbColor)
    {
        float r = hsbColor.B;
        float g = hsbColor.B;
        float b = hsbColor.B;
        if (Math.Abs(hsbColor.S) > TOLERANCE)
        {
            float max = hsbColor.B;
            float dif = hsbColor.B*hsbColor.S;
            float min = hsbColor.B - dif;

            float h = hsbColor.H*360f;

            if (h < 60f)
            {
                r = max;
                g = h*dif/60f + min;
                b = min;
            }
            else if (h < 120f)
            {
                r = -(h - 120f)*dif/60f + min;
                g = max;
                b = min;
            }
            else if (h < 180f)
            {
                r = min;
                g = max;
                b = (h - 120f)*dif/60f + min;
            }
            else if (h < 240f)
            {
                r = min;
                g = -(h - 240f)*dif/60f + min;
                b = max;
            }
            else if (h < 300f)
            {
                r = (h - 240f)*dif/60f + min;
                g = min;
                b = max;
            }
            else if (h <= 360f)
            {
                r = max;
                g = min;
                b = -(h - 360f)*dif/60 + min;
            }
            else
            {
                r = 0;
                g = 0;
                b = 0;
            }
        }

        return new Color(Mathf.Clamp01(r), Mathf.Clamp01(g), Mathf.Clamp01(b), hsbColor.A);
    }

    public static Color ConvertRGBColorByHUE(Color rgbColor, float hue)
    {
        var brightness = ColorToHSV(rgbColor).B;
        if (brightness < TOLERANCE)
            brightness = TOLERANCE;
        var hsv = ColorToHSV(rgbColor / brightness);
        hsv.H = hue;
        var color = HSVToColor(hsv) * brightness;
        color.a = rgbColor.a;
        return color;
    }
}
