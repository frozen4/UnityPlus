using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
public class GTextGraphicBoard : MaskableGraphic
{
    [NonSerialized]
    private readonly VertexHelper s_VertexHelper = new VertexHelper();
    private IList<GTextEmotionModel> _Models;
    public override Texture mainTexture
    {
        get
        {
            if (CEmojiManager.Instance.EmotionAssets == null)
                return s_WhiteTexture;

            if (CEmojiManager.Instance.EmotionAssets.MainTexture == null)
                return s_WhiteTexture;
            else
                return CEmojiManager.Instance.EmotionAssets.MainTexture;
        }
    }
    public void SetVertexData(List<GTextEmotionModel> models)
    {
        _Models = models;
        DoMeshGeneration();
    }
    private void DoMeshGeneration()
    {
        if (rectTransform != null && rectTransform.rect.width >= 0 && rectTransform.rect.height >= 0)
            OnPopulateMesh(s_VertexHelper);
        else
            s_VertexHelper.Clear();
        s_VertexHelper.FillMesh(workerMesh);
        canvasRenderer.SetMesh(workerMesh);
    }
    static Vector2 uvMin = Vector2.zero;
    static Vector2 uvMax = Vector2.zero;
    protected override void OnPopulateMesh(VertexHelper toFill)
    {
        toFill.Clear();
        if (_Models == null || _Models.Count <= 0) return;
        Color32 color32 = color;

        //TODO lizhixiong
        //在计算定点的时候需要知道Unity的一些默认规则，否则容易陷入泥潭..
        //1.在生成三角片的时候坐标原点是左下角，不是常用的屏幕坐标系（左上角）,UV也一样。
        //2.Unity ugui 默认的三角形的定点绘制顺序如下
        //              0   1 
        //              3   2
        //  索引 ->   012 230
        for (int i = 0; i < _Models.Count; i++)
        {
            GTextEmotionModel model = _Models[i];
            GEmotionAsset asset = model.GetEmotionAsset();
            if (asset != null)
            {
                Vector4 outerUV = asset.GetOuterUV();
                uvMin.Set(outerUV.x, outerUV.w);
                uvMax.Set(outerUV.z, outerUV.y);
                AddQuad(toFill, model.PosMin, model.PosMax, color32, uvMin, uvMax);
            }
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
}
