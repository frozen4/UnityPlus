using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System;

[AddComponentMenu("UI/Effects/Gradient")]
public class GGradient : BaseMeshEffect
{
    [SerializeField]
    private Color32 topColor = Color.white;
    [SerializeField]
    private Color32 bottomColor = Color.black;

#if UNITY_5_2_0
    public override void ModifyMesh(Mesh mesh)
    {
        if (!IsActive())
        {
            return;
        }

        Vector3[] vertexList = mesh.vertices;
        int count = mesh.vertexCount;
        if (count > 0)
        {
            float bottomY = vertexList[0].y;
            float topY = vertexList[0].y;

            for (int i = 1; i < count; i++)
            {
                float y = vertexList[i].y;
                if (y > topY)
                {
                    topY = y;
                }
                else if (y < bottomY)
                {
                    bottomY = y;
                }
            }
            List<Color32> colors = new List<Color32>();
            float uiElementHeight = topY - bottomY;
            for (int i = 0; i < count; i++)
            {
                colors.Add(Color32.Lerp(bottomColor, topColor, (vertexList[i].y - bottomY) / uiElementHeight));
            }
            mesh.SetColors(colors);
        }
    }
    
#else
    public override void ModifyMesh(VertexHelper vh)
    {
        if (!this.IsActive())
            return;

        List<UIVertex> vertexList = new List<UIVertex>();
        vh.GetUIVertexStream(vertexList);

        ModifyVertices(vertexList);

        vh.Clear();
        vh.AddUIVertexTriangleStream(vertexList);
    }

    public void ModifyVertices(List<UIVertex> vertexList)
    {
        if (!IsActive())
            return;

        int count = vertexList.Count;
        if (count > 0)
        {
            float bottomY = vertexList[0].position.y;
            float topY = vertexList[0].position.y;
            for (int i = 1; i < count; i++)
            {
                float y = vertexList[i].position.y;
                if (y > topY)
                {
                    topY = y;
                }
                else if (y < bottomY)
                {
                    bottomY = y;
                }
            }
            float uiElementHeight = topY - bottomY;
            for (int i = 0; i < count; i++)
            {
                UIVertex uiVertex = vertexList[i];
                uiVertex.color = Color32.Lerp(bottomColor, topColor, (uiVertex.position.y - bottomY) / uiElementHeight);
                vertexList[i] = uiVertex;
            }
        }
    }
#endif

    public void ChangeBottomColor(Color eColor)
    {
        bottomColor = eColor;
    }
}
