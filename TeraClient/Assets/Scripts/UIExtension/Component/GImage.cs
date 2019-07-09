using UnityEngine;
using UnityEngine.UI;
public class GImage : Image
{
    [SerializeField]
    private UVFillMethod m_UvFillMethod = UVFillMethod.SymmetryLR;
    public UVFillMethod uvFillMethod { get { return m_UvFillMethod; } set { m_UvFillMethod = value; } }
    protected override void OnPopulateMesh(VertexHelper toFill)
    {
        if (overrideSprite == null || m_UvFillMethod == UVFillMethod.None)
        {
            base.OnPopulateMesh(toFill);
            return;
        }

        if (type == Type.Simple)
        {
            GenerateSimple(toFill);
        }
        else if (type == Type.Sliced)
        {
            if (m_UvFillMethod != UVFillMethod.None)
            {
                Debug.Log("GImage Type not supported for this method.");
            }
            //GenerateSliced(toFill);
            base.OnPopulateMesh(toFill);
        }
        else
        {
            if (m_UvFillMethod != UVFillMethod.None)
            {
                Debug.Log("GImage Type not supported for this method.");
            }
            else
            {
                base.OnPopulateMesh(toFill);
            }
        }
    }
    public override void SetNativeSize()
    {
        if (overrideSprite != null)
        {
            float w = overrideSprite.rect.width / pixelsPerUnit;
            float h = overrideSprite.rect.height / pixelsPerUnit;

            if (m_UvFillMethod == UVFillMethod.SymmetryLR)
            {
                rectTransform.anchorMax = rectTransform.anchorMin;
                rectTransform.sizeDelta = new Vector2(w * 2, h);
                SetAllDirty();
            }
            else if (m_UvFillMethod == UVFillMethod.SymmetryUD)
            {
                rectTransform.anchorMax = rectTransform.anchorMin;
                rectTransform.sizeDelta = new Vector2(w, h * 2);
                SetAllDirty();
            }
            else if (m_UvFillMethod == UVFillMethod.SymmetryLRUD)
            {
                rectTransform.anchorMax = rectTransform.anchorMin;
                rectTransform.sizeDelta = new Vector2(w * 2, h * 2);
                SetAllDirty();
            }
            else
            {
                rectTransform.anchorMax = rectTransform.anchorMin;
                rectTransform.sizeDelta = new Vector2(w, h);
                SetAllDirty();
            }
        }
    }

    static readonly Vector3[] simplePoints = new Vector3[25];
    static readonly Vector2[] uvPoints = new Vector2[25];


    private void GenerateSimple(VertexHelper vh)
    {
        var rect = GetDrawingDimensions(m_UvFillMethod == UVFillMethod.Contain ? false : preserveAspect);
        var uv = (overrideSprite != null) ? UnityEngine.Sprites.DataUtility.GetOuterUV(overrideSprite) : Vector4.zero;
        var color32 = color;
        vh.Clear();

        if (m_UvFillMethod == UVFillMethod.SymmetryLR)
        {
            vh.AddVert(new Vector3(rect.x, rect.y), color32, new Vector2(uv.x, uv.y));
            vh.AddVert(new Vector3(rect.x, rect.w), color32, new Vector2(uv.x, uv.w));
            vh.AddVert(new Vector3(rect.x + (rect.z - rect.x) * .5f, rect.w), color32, new Vector2(uv.z, uv.w));
            vh.AddVert(new Vector3(rect.x + (rect.z - rect.x) * .5f, rect.y), color32, new Vector2(uv.z, uv.y));
            vh.AddVert(new Vector3(rect.z, rect.w), color32, new Vector2(uv.x, uv.w));
            vh.AddVert(new Vector3(rect.z, rect.y), color32, new Vector2(uv.x, uv.y));

            vh.AddTriangle(0, 1, 2);
            vh.AddTriangle(2, 3, 0);
            vh.AddTriangle(3, 2, 4);
            vh.AddTriangle(4, 5, 3);
        }
        else if (m_UvFillMethod == UVFillMethod.SymmetryUD)
        {
            vh.AddVert(new Vector3(rect.x, rect.y), color32, new Vector2(uv.x, uv.w));
            vh.AddVert(new Vector3(rect.z, rect.y), color32, new Vector2(uv.z, uv.w));
            vh.AddVert(new Vector3(rect.x, rect.y + (rect.w - rect.y) * 0.5f), color32, new Vector2(uv.x, uv.y));
            vh.AddVert(new Vector3(rect.z, rect.y + (rect.w - rect.y) * 0.5f), color32, new Vector2(uv.z, uv.y));
            vh.AddVert(new Vector3(rect.x, rect.w), color32, new Vector2(uv.x, uv.w));
            vh.AddVert(new Vector3(rect.z, rect.w), color32, new Vector2(uv.z, uv.w));

            vh.AddTriangle(0, 2, 3);
            vh.AddTriangle(3, 1, 0);
            vh.AddTriangle(2, 4, 5);
            vh.AddTriangle(5, 3, 2);
        }
        else if (m_UvFillMethod == UVFillMethod.SymmetryLRUD)
        {
            //Vector3[] simplePoints = new Vector3[9];
            //Vector2[] uvPoints = new Vector2[4];

            simplePoints[0] = new Vector3(rect.x, rect.y);
            simplePoints[1] = new Vector3(rect.x, rect.y + (rect.w - rect.y) * 0.5f);
            simplePoints[2] = new Vector3(rect.x, rect.w);
            simplePoints[3] = new Vector3(rect.x + (rect.z - rect.x) * .5f, rect.y);
            simplePoints[4] = new Vector3(rect.x + (rect.z - rect.x) * .5f, rect.y + (rect.w - rect.y) * 0.5f);
            simplePoints[5] = new Vector3(rect.x + (rect.z - rect.x) * .5f, rect.w);
            simplePoints[6] = new Vector3(rect.z, rect.y);
            simplePoints[7] = new Vector3(rect.z, rect.y + (rect.w - rect.y) * 0.5f);
            simplePoints[8] = new Vector3(rect.z, rect.w);

            uvPoints[0] = new Vector2(uv.x, uv.y);
            uvPoints[1] = new Vector2(uv.x, uv.w);
            uvPoints[2] = new Vector2(uv.z, uv.y);
            uvPoints[3] = new Vector2(uv.z, uv.w);

            AddQuad(vh, simplePoints[0], simplePoints[4], color32, uvPoints[1], uvPoints[2]);
            AddQuad(vh, simplePoints[1], simplePoints[5], color32, uvPoints[0], uvPoints[3]);
            AddQuad(vh, simplePoints[3], simplePoints[7], color32, uvPoints[3], uvPoints[0]);
            AddQuad(vh, simplePoints[4], simplePoints[8], color32, uvPoints[2], uvPoints[1]);
        }
        else if (m_UvFillMethod == UVFillMethod.Contain)
        {
            if (overrideSprite != null)
            {
                Vector2 p = rectTransform.pivot;

                float w = rect.z - rect.x;
                float h = rect.w - rect.y;
                float a = w * overrideSprite.rect.height / overrideSprite.rect.width - h;

                if (a < 0)
				{
                    a = h * overrideSprite.rect.width / overrideSprite.rect.height - w;
                    rect.x = rect.x - a * p.x;
                    rect.z = rect.z + a * (1 - p.x);
                }
				else
                {
                    rect.y = rect.y - a * p.y;
                    rect.w = rect.w + a * (1 - p.y);
                }
            }

            vh.AddVert(new Vector3(rect.x, rect.y), color32, new Vector2(uv.x, uv.y));
            vh.AddVert(new Vector3(rect.x, rect.w), color32, new Vector2(uv.x, uv.w));
            vh.AddVert(new Vector3(rect.z, rect.w), color32, new Vector2(uv.z, uv.w));
            vh.AddVert(new Vector3(rect.z, rect.y), color32, new Vector2(uv.z, uv.y));

            vh.AddTriangle(0, 1, 2);
            vh.AddTriangle(2, 3, 0);
        }
    }
    static void AddQuad(VertexHelper vertexHelper, Vector2 posMin, Vector2 posMax, Color32 color, Vector2 uvMin, Vector2 uvMax)
    {
        int startIndex = vertexHelper.currentVertCount;

        vertexHelper.AddVert(new Vector3(posMin.x, posMin.y, 0), color, new Vector2(uvMin.x, uvMin.y));
        vertexHelper.AddVert(new Vector3(posMin.x, posMax.y, 0), color, new Vector2(uvMin.x, uvMax.y));
        vertexHelper.AddVert(new Vector3(posMax.x, posMax.y, 0), color, new Vector2(uvMax.x, uvMax.y));
        vertexHelper.AddVert(new Vector3(posMax.x, posMin.y, 0), color, new Vector2(uvMax.x, uvMin.y));

        vertexHelper.AddTriangle(startIndex, startIndex + 1, startIndex + 2);
        vertexHelper.AddTriangle(startIndex + 2, startIndex + 3, startIndex);
    }

    //This thing has Problem!
//#if UNITY_EDITOR
//    private void GenerateSliced(VertexHelper vh)
//    {
//        if (!hasBorder)
//        {
//            GenerateSimple(vh);
//            return;
//        }

//        Vector4 outer, inner, padding, border;

//        if (overrideSprite != null)
//        {
//            outer = UnityEngine.Sprites.DataUtility.GetOuterUV(overrideSprite);
//            inner = UnityEngine.Sprites.DataUtility.GetInnerUV(overrideSprite);
//            padding = UnityEngine.Sprites.DataUtility.GetPadding(overrideSprite);
//            border = overrideSprite.border;
//        }
//        else
//        {
//            outer = Vector4.zero;
//            inner = Vector4.zero;
//            padding = Vector4.zero;
//            border = Vector4.zero;
//        }

//        Rect rect = GetPixelAdjustedRect();
//        border = GetAdjustedBorders(border / pixelsPerUnit, rect);
//        padding = padding / pixelsPerUnit;


//        if (m_UvFillMethod == UVFillMethod.SymmetryLR)
//        {
//            //Vector3[] simplePoints = new Vector3[16];
//            //Vector2[] uvPoints = new Vector2[12];

//            simplePoints[0] = new Vector3(padding.x, padding.y);
//            simplePoints[1] = new Vector3(padding.x, border.y);
//            simplePoints[2] = new Vector3(padding.x, rect.height - border.w);
//            simplePoints[3] = new Vector3(padding.x, rect.height - padding.w);

//            simplePoints[4] = new Vector3(border.x, padding.y);
//            simplePoints[5] = new Vector3(border.x, border.y);
//            simplePoints[6] = new Vector3(border.x, rect.height - border.w);
//            simplePoints[7] = new Vector3(border.x, rect.height - padding.w);

//            simplePoints[8] = new Vector3(rect.width - border.x, padding.y);
//            simplePoints[9] = new Vector3(rect.width - border.x, border.y);
//            simplePoints[10] = new Vector3(rect.width - border.x, rect.height - border.w);
//            simplePoints[11] = new Vector3(rect.width - border.x, rect.height - padding.w);

//            simplePoints[12] = new Vector3(rect.width - padding.x, padding.y);
//            simplePoints[13] = new Vector3(rect.width - padding.x, border.y);
//            simplePoints[14] = new Vector3(rect.width - padding.x, rect.height - border.w);
//            simplePoints[15] = new Vector3(rect.width - padding.x, rect.height - padding.w);

//            for (int i = 0; i < 16; ++i)
//            {
//                simplePoints[i].x += rect.x;
//                simplePoints[i].y += rect.y;
//            }

//            uvPoints[0] = new Vector2(outer.x, outer.y);
//            uvPoints[1] = new Vector2(outer.x, inner.y);
//            uvPoints[2] = new Vector2(outer.x, inner.w);
//            uvPoints[3] = new Vector2(outer.x, outer.w);

//            uvPoints[4] = new Vector2(inner.x, outer.y);
//            uvPoints[5] = new Vector2(inner.x, inner.y);
//            uvPoints[6] = new Vector2(inner.x, inner.w);
//            uvPoints[7] = new Vector2(inner.x, outer.w);

//            uvPoints[8] = new Vector2(outer.z, outer.y);
//            uvPoints[9] = new Vector2(outer.z, inner.y);
//            uvPoints[10] = new Vector2(outer.z, inner.w);
//            uvPoints[11] = new Vector2(outer.z, outer.w);

//            vh.Clear();
//            var color32 = color;

//            for (int j = 0; j < 12; j++)
//            {
//                vh.AddVert(simplePoints[j], color32, uvPoints[j]);
//            }
//            vh.AddTriangle(0, 1, 5);
//            vh.AddTriangle(5, 4, 0);
//            vh.AddTriangle(1, 2, 6);
//            vh.AddTriangle(6, 5, 1);
//            vh.AddTriangle(2, 3, 7);
//            vh.AddTriangle(7, 6, 2);
//            vh.AddTriangle(4, 5, 9);
//            vh.AddTriangle(9, 8, 4);
//            vh.AddTriangle(5, 6, 10);
//            vh.AddTriangle(10, 9, 5);
//            vh.AddTriangle(6, 7, 11);
//            vh.AddTriangle(11, 10, 6);

//            vh.AddVert(simplePoints[8], color32, uvPoints[4]);
//            vh.AddVert(simplePoints[9], color32, uvPoints[5]);
//            vh.AddVert(simplePoints[10], color32, uvPoints[6]);
//            vh.AddVert(simplePoints[11], color32, uvPoints[7]);
//            vh.AddVert(simplePoints[12], color32, uvPoints[0]);
//            vh.AddVert(simplePoints[13], color32, uvPoints[1]);
//            vh.AddVert(simplePoints[14], color32, uvPoints[2]);
//            vh.AddVert(simplePoints[15], color32, uvPoints[3]);

//            vh.AddTriangle(12, 13, 17);
//            vh.AddTriangle(17, 16, 12);
//            vh.AddTriangle(13, 14, 18);
//            vh.AddTriangle(18, 17, 13);
//            vh.AddTriangle(14, 15, 19);
//            vh.AddTriangle(19, 18, 14);
//        }
//        else if (m_UvFillMethod == UVFillMethod.SymmetryUD)
//        {
//            //Vector3[] simplePoints = new Vector3[16];
//            //Vector2[] uvPoints = new Vector2[12];

//            padding = Vector4.zero;

//            simplePoints[0] = new Vector3(padding.x, padding.w);
//            simplePoints[1] = new Vector3(border.x, padding.w);
//            simplePoints[2] = new Vector3(rect.width - border.z, padding.w);
//            simplePoints[3] = new Vector3(rect.width - padding.z, padding.w);

//            simplePoints[4] = new Vector3(padding.x, border.w);
//            simplePoints[5] = new Vector3(border.x, border.w);
//            simplePoints[6] = new Vector3(rect.width - border.z, border.w);
//            simplePoints[7] = new Vector3(rect.width - padding.z, border.w);

//            simplePoints[8] = new Vector3(padding.x, rect.height - border.w);
//            simplePoints[9] = new Vector3(border.x, rect.height - border.w);
//            simplePoints[10] = new Vector3(rect.width - border.z, rect.height - border.w);
//            simplePoints[11] = new Vector3(rect.width - padding.z, rect.height - border.w);

//            simplePoints[12] = new Vector3(padding.x, rect.height - padding.y);
//            simplePoints[13] = new Vector3(border.x, rect.height - padding.y);
//            simplePoints[14] = new Vector3(rect.width - border.z, rect.height - padding.y);
//            simplePoints[15] = new Vector3(rect.width - padding.z, rect.height - padding.y);

//            for (int i = 0; i < 16; ++i)
//            {
//                simplePoints[i].x += rect.x;
//                simplePoints[i].y += rect.y;
//            }

//            uvPoints[0] = new Vector2(outer.x, outer.w);
//            uvPoints[1] = new Vector2(inner.x, outer.w);
//            uvPoints[2] = new Vector2(inner.z, outer.w);
//            uvPoints[3] = new Vector2(outer.z, outer.w);

//            uvPoints[4] = new Vector2(outer.x, inner.w);
//            uvPoints[5] = new Vector2(inner.x, inner.w);
//            uvPoints[6] = new Vector2(inner.z, inner.w);
//            uvPoints[7] = new Vector2(outer.z, inner.w);

//            uvPoints[8] = new Vector2(outer.x, outer.y);
//            uvPoints[9] = new Vector2(inner.x, outer.y);
//            uvPoints[10] = new Vector2(inner.z, outer.y);
//            uvPoints[11] = new Vector2(outer.z, outer.y);

//            vh.Clear();
//            var color32 = color;

//            for (int j = 0; j < 12; j++)
//            {
//                vh.AddVert(simplePoints[j], color32, uvPoints[j]);
//            }
//            vh.AddTriangle(0, 4, 5);
//            vh.AddTriangle(5, 1, 0);
//            vh.AddTriangle(4, 8, 9);
//            vh.AddTriangle(9, 5, 4);
//            vh.AddTriangle(1, 5, 6);
//            vh.AddTriangle(6, 2, 1);
//            vh.AddTriangle(5, 9, 10);
//            vh.AddTriangle(10, 6, 5);
//            vh.AddTriangle(2, 6, 7);
//            vh.AddTriangle(7, 3, 2);
//            vh.AddTriangle(6, 10, 11);
//            vh.AddTriangle(11, 7, 6);

//            vh.AddVert(simplePoints[8], color32, uvPoints[4]);
//            vh.AddVert(simplePoints[9], color32, uvPoints[5]);
//            vh.AddVert(simplePoints[10], color32, uvPoints[6]);
//            vh.AddVert(simplePoints[11], color32, uvPoints[7]);

//            vh.AddVert(simplePoints[12], color32, uvPoints[0]);
//            vh.AddVert(simplePoints[13], color32, uvPoints[1]);
//            vh.AddVert(simplePoints[14], color32, uvPoints[2]);
//            vh.AddVert(simplePoints[15], color32, uvPoints[3]);

//            vh.AddTriangle(12, 16, 17);
//            vh.AddTriangle(17, 13, 12);
//            vh.AddTriangle(13, 17, 18);
//            vh.AddTriangle(18, 14, 13);
//            vh.AddTriangle(14, 18, 19);
//            vh.AddTriangle(19, 15, 14);
//        }
//        else if (m_UvFillMethod == UVFillMethod.SymmetryLRUD)
//        {
//            //Vector3[] simplePoints = new Vector3[25];
//            //Vector2[] uvPoints = new Vector2[25];

//            simplePoints[0] = new Vector3(padding.x, padding.w);
//            simplePoints[1] = new Vector3(padding.x, border.w);
//            simplePoints[2] = new Vector3(padding.x, rect.height - border.w);
//            simplePoints[3] = new Vector3(border.x, padding.w);
//            simplePoints[4] = new Vector3(border.x, border.w);
//            simplePoints[5] = new Vector3(border.x, rect.height - border.w);
//            simplePoints[6] = new Vector3(rect.width - border.x, padding.w);
//            simplePoints[7] = new Vector3(rect.width - border.x, border.w);
//            simplePoints[8] = new Vector3(rect.width - border.x, rect.height - border.w);

//            simplePoints[9] = simplePoints[6];
//            simplePoints[10] = new Vector3(rect.width, padding.w);
//            simplePoints[11] = simplePoints[7];
//            simplePoints[12] = new Vector3(rect.width, border.w);
//            simplePoints[13] = simplePoints[8];
//            simplePoints[14] = new Vector3(rect.width, rect.height - border.w);

//            simplePoints[15] = simplePoints[2];
//            simplePoints[16] = new Vector3(padding.x, rect.height - padding.y);
//            simplePoints[17] = simplePoints[5];
//            simplePoints[18] = new Vector2(border.x, rect.height - padding.y);
//            simplePoints[19] = simplePoints[8];
//            simplePoints[20] = new Vector3(rect.width - border.x, rect.height - padding.y);

//            simplePoints[21] = simplePoints[19];
//            simplePoints[22] = simplePoints[20];
//            simplePoints[23] = simplePoints[14];
//            simplePoints[24] = new Vector3(rect.width - padding.x, rect.height - padding.y);

//            for (int i = 0; i < 25; ++i)
//            {
//                simplePoints[i].x += rect.x;
//                simplePoints[i].y += rect.y;
//            }

//            uvPoints[0] = new Vector2(outer.x, outer.w);
//            uvPoints[1] = new Vector2(outer.x, inner.w);
//            uvPoints[2] = new Vector2(outer.x, outer.y);
//            uvPoints[3] = new Vector2(inner.x, outer.w);
//            uvPoints[4] = new Vector2(inner.x, inner.w);
//            uvPoints[5] = new Vector2(inner.x, outer.y);
//            uvPoints[6] = new Vector2(outer.z, outer.w);
//            uvPoints[7] = new Vector2(outer.z, inner.w);
//            uvPoints[8] = new Vector2(outer.z, outer.y);

//            uvPoints[9] = uvPoints[3];
//            uvPoints[10] = uvPoints[0];
//            uvPoints[11] = uvPoints[4];
//            uvPoints[12] = uvPoints[1];
//            uvPoints[13] = uvPoints[5];
//            uvPoints[14] = uvPoints[2];

//            uvPoints[15] = uvPoints[1];
//            uvPoints[16] = uvPoints[0];
//            uvPoints[17] = uvPoints[4];
//            uvPoints[18] = uvPoints[3];
//            uvPoints[19] = uvPoints[7];
//            uvPoints[20] = uvPoints[6];

//            uvPoints[21] = uvPoints[4];
//            uvPoints[22] = uvPoints[3];
//            uvPoints[23] = uvPoints[1];
//            uvPoints[24] = uvPoints[0];

//            vh.Clear();
//            var color32 = color;

//            vh.AddVert(simplePoints[0], color32, uvPoints[0]);
//            vh.AddVert(simplePoints[1], color32, uvPoints[1]);
//            vh.AddVert(simplePoints[2], color32, uvPoints[2]);
//            vh.AddVert(simplePoints[3], color32, uvPoints[3]);
//            vh.AddVert(simplePoints[4], color32, uvPoints[4]);
//            vh.AddVert(simplePoints[5], color32, uvPoints[5]);
//            vh.AddVert(simplePoints[6], color32, uvPoints[6]);
//            vh.AddVert(simplePoints[7], color32, uvPoints[7]);
//            vh.AddVert(simplePoints[8], color32, uvPoints[8]);
//            vh.AddTriangle(0, 1, 4);
//            vh.AddTriangle(4, 3, 0);
//            vh.AddTriangle(1, 2, 5);
//            vh.AddTriangle(5, 4, 1);
//            vh.AddTriangle(3, 4, 7);
//            vh.AddTriangle(7, 6, 3);
//            vh.AddTriangle(4, 5, 8);
//            vh.AddTriangle(8, 7, 4);

//            vh.AddVert(simplePoints[9], color32, uvPoints[9]);
//            vh.AddVert(simplePoints[10], color32, uvPoints[10]);
//            vh.AddVert(simplePoints[11], color32, uvPoints[11]);
//            vh.AddVert(simplePoints[12], color32, uvPoints[12]);
//            vh.AddVert(simplePoints[13], color32, uvPoints[13]);
//            vh.AddVert(simplePoints[14], color32, uvPoints[14]);
//            vh.AddTriangle(9, 11, 12);
//            vh.AddTriangle(12, 10, 9);
//            vh.AddTriangle(11, 13, 14);
//            vh.AddTriangle(14, 12, 11);

//            vh.AddVert(simplePoints[15], color32, uvPoints[15]);
//            vh.AddVert(simplePoints[16], color32, uvPoints[16]);
//            vh.AddVert(simplePoints[17], color32, uvPoints[17]);
//            vh.AddVert(simplePoints[18], color32, uvPoints[18]);
//            vh.AddVert(simplePoints[19], color32, uvPoints[19]);
//            vh.AddVert(simplePoints[20], color32, uvPoints[20]);
//            vh.AddTriangle(15, 16, 18);
//            vh.AddTriangle(18, 17, 15);
//            vh.AddTriangle(17, 18, 20);
//            vh.AddTriangle(20, 19, 17);

//            vh.AddVert(simplePoints[21], color32, uvPoints[21]);
//            vh.AddVert(simplePoints[22], color32, uvPoints[22]);
//            vh.AddVert(simplePoints[23], color32, uvPoints[23]);
//            vh.AddVert(simplePoints[24], color32, uvPoints[24]);
//            vh.AddTriangle(21, 22, 24);
//            vh.AddTriangle(24, 23, 21);
//        }
//        else
//        {
//            //Vector3[] simplePoints = new Vector3[4];
//            //Vector2[] uvPoints = new Vector2[4];

//            simplePoints[0] = new Vector2(padding.x, padding.y);
//            simplePoints[3] = new Vector2(rect.width - padding.z, rect.height - padding.w);

//            simplePoints[1].x = border.x;
//            simplePoints[1].y = border.y;
//            simplePoints[2].x = rect.width - border.z;
//            simplePoints[2].y = rect.height - border.w;

//            for (int i = 0; i < 4; ++i)
//            {
//                simplePoints[i].x += rect.x;
//                simplePoints[i].y += rect.y;
//            }

//            uvPoints[0] = new Vector2(outer.x, outer.y);
//            uvPoints[1] = new Vector2(inner.x, inner.y);
//            uvPoints[2] = new Vector2(inner.z, inner.w);
//            uvPoints[3] = new Vector2(outer.z, outer.w);

//            vh.Clear();

//            for (int x = 0; x < 3; ++x)
//            {
//                int x2 = x + 1;

//                for (int y = 0; y < 3; ++y)
//                {
//                    if (!fillCenter && x == 1 && y == 1)
//                        continue;

//                    int y2 = y + 1;

//                    AddQuad(vh,
//                        new Vector2(simplePoints[x].x, simplePoints[y].y),
//                        new Vector2(simplePoints[x2].x, simplePoints[y2].y),
//                        color,
//                        new Vector2(uvPoints[x].x, uvPoints[y].y),
//                        new Vector2(uvPoints[x2].x, uvPoints[y2].y));
//                }
//            }
//        }
//    }
//#endif

    private Vector4 GetDrawingDimensions(bool shouldPreserveAspect)
    {
        var padding = Vector4.zero;
        var size = Vector2.zero;
        if (overrideSprite != null)
        {
            padding = UnityEngine.Sprites.DataUtility.GetPadding(overrideSprite);
            size = new Vector2(overrideSprite.rect.width, overrideSprite.rect.height);
        }

        padding.z = padding.x;
        padding.w = padding.y;
        Rect r = GetPixelAdjustedRect();

        int spriteW = Mathf.RoundToInt(size.x);
        int spriteH = Mathf.RoundToInt(size.y);

        var v = new Vector4(
                padding.x / spriteW,
                padding.y / spriteH,
                (spriteW - padding.z) / spriteW,
                (spriteH - padding.w) / spriteH);

        if (shouldPreserveAspect && size.sqrMagnitude > 0.0f)
        {
            var spriteRatio = size.x / size.y;
            var rectRatio = r.width / r.height;

            if (spriteRatio > rectRatio)
            {
                var oldHeight = r.height;
                r.height = r.width * (1.0f / spriteRatio);
                r.y += (oldHeight - r.height) * rectTransform.pivot.y;
            }
            else
            {
                var oldWidth = r.width;
                r.width = r.height * spriteRatio;
                r.x += (oldWidth - r.width) * rectTransform.pivot.x;
            }
        }

        v = new Vector4(
                r.x + r.width * v.x,
                r.y + r.height * v.y,
                r.x + r.width * v.z,
                r.y + r.height * v.w
                );

        return v;
    }

    Vector4 GetAdjustedBorders(Vector4 border, Rect rect)
    {
        for (int axis = 0; axis <= 1; axis++)
        {
            float combinedBorders = border[axis] + border[axis + 2];
            if (rect.size[axis] < combinedBorders && combinedBorders != 0)
            {
                float borderScaleRatio = rect.size[axis] / combinedBorders;
                border[axis] *= borderScaleRatio;
                border[axis + 2] *= borderScaleRatio;
            }
        }
        return border;
    }
}
