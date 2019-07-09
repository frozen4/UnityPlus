using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Text.RegularExpressions;
public class GText : Text, IPointerClickHandler
{
    //TODO 考虑数据缓存
    private const string ID_EMOTION = "e";
    private const string ID_LINK = "l";
    private const string TEXT_QUAD = "<quad name={0} size={1} width={2} />";
    //private const string TEXT_LINK = "<color={0}>{1}</color>";
    private static readonly Regex _TextRegex = new Regex(@"\[([el])\](.+?)\[\-\]", RegexOptions.Singleline);    //[l/e]xxx[-]
    private static readonly Regex _LinkRegex = new Regex(@"^(#[abcdefABCDEF0-9]{6}) (.+?)$", RegexOptions.Singleline);    //#ffffff xxx
    private List<GTextEmotionModel> _GraphicItemModels = new List<GTextEmotionModel>();
    private List<GTextLinkModel> _TextLinkModels = new List<GTextLinkModel>();
    private GTextGraphicBoard _GraphicBoard;
    private string _OriginText = string.Empty;
    private string _BuildText = string.Empty;
    private int _TextID = 0;

    [NoToLuaAttribute]
    public string OriginText { get { return _OriginText; } }

    [NoToLuaAttribute]
    public delegate void OnGTextClick(int textId, int linkId);

    [NoToLuaAttribute]
    public OnGTextClick OnClick = null;
    public int TextID { set { _TextID = value; } }

    //修正图片的位置 
    [NoToLua]
    public Vector3 GraphicOffset = new Vector3(0, 0, 0);
    [NoToLua]
    public float GraphicScale = 1.25f;

    protected override void Awake()
    {
        this.alignByGeometry = false;
        if (rectTransform.childCount <= 0)
        {
            _GraphicBoard = new GameObject("_GraphicBoard").AddComponent<GTextGraphicBoard>();
            _GraphicBoard.rectTransform.SetParent(transform, false);
        }
        else
        {
            _GraphicBoard = GetComponentInChildren<GTextGraphicBoard>();
            if (_GraphicBoard == null)
            {
                _GraphicBoard = rectTransform.GetChild(0).gameObject.AddComponent<GTextGraphicBoard>();
            }
        }
        FormatBoard();
    }

    [NoToLua]
    public void FormatBoard()
    {
        if (_GraphicBoard == null) return;
        //_GraphicBoard.rectTransform.anchorMin = rectTransform.anchorMin;
        //_GraphicBoard.rectTransform.anchorMax = rectTransform.anchorMax;
        _GraphicBoard.rectTransform.anchorMin = Vector2.zero;
        _GraphicBoard.rectTransform.anchorMax = Vector2.one;
        _GraphicBoard.rectTransform.offsetMin = Vector2.zero;
        _GraphicBoard.rectTransform.offsetMax = Vector2.zero;
        _GraphicBoard.rectTransform.pivot = rectTransform.pivot;
    }

    [NoToLua]
    public override void SetVerticesDirty()
    {
        //收集富文本信息
        _OriginText = m_Text;
        MatchCollection collections = _TextRegex.Matches(_OriginText);
        _GraphicItemModels.Clear();
        _TextLinkModels.Clear();
        int last_index = 0;
        int start_index = 0;
        var sb = HobaText.GetStringBuilder();

        for (var i = 0; i < collections.Count; i++)
        {
            Match match = collections[i];
            int match_index = match.Index;
            string type = match.Groups[1].Value;
            if (type == ID_EMOTION)
            {
                //TODO: 改成参数 
                int my_size = (int)(this.fontSize * GraphicScale);

                sb.Append(_OriginText.Substring(last_index, match_index - last_index));
                start_index = sb.Length;
                string quad = HobaText.Format(TEXT_QUAD, match.Groups[2].Value, (int)(fontSize), (GraphicScale));
                sb.Append(quad);
                GTextEmotionModel model = new GTextEmotionModel();
                model.TextIndex = start_index;
                model.Size = my_size;

                model.SpriteName = TranslateEmoji(match.Groups[2].Value);

                _GraphicItemModels.Add(model);
            }
            else if (type == ID_LINK)
            {
                string seg = _OriginText.Substring(last_index, match_index - last_index);
                sb.Append(seg);
                start_index = sb.Length;

                GTextLinkModel linkModel = new GTextLinkModel();

                //string link_txt;
                string inner_txt;
                string color_text;

                //colorful text
                MatchCollection collections_con = _LinkRegex.Matches(match.Groups[2].Value);
                if (collections_con.Count == 1 && collections_con[0].Groups.Count == 3)
                {
                    //link_txt = string.Format(TEXT_LINK, collections_con[0].Groups[1].Value, collections_con[0].Groups[2].Value);
                    color_text = collections_con[0].Groups[1].Value;
                    inner_txt = collections_con[0].Groups[2].Value;
                }
                else
                {
                    //link_txt = string.Format(TEXT_LINK, "yellow", match.Groups[2].Value);
                    color_text = "yellow";
                    inner_txt = match.Groups[2].Value;
                }

                sb.AppendFormat("<color={0}>", color_text);
                linkModel.StartIndex = sb.Length - 1;
                sb.AppendFormat(inner_txt);
                linkModel.EndIndex = sb.Length - 1;
                sb.Append("</color>");

                //linkModel.Text = inner_txt;

                _TextLinkModels.Add(linkModel);
            }
            last_index = match_index + match.Value.Length;
        }
        sb.Append(_OriginText.Substring(last_index, _OriginText.Length - last_index));
        _BuildText = sb.ToString();

        //重绘请求
        base.SetVerticesDirty();
    }
    //这里是富文本的主要逻辑
    //主要分为两部分：1.提取咱们自定义的富文本信息。2.根据提取的信息在做后续处理，包括图文混排和文字链接
    //图文混排： 会根据富文本中提取的信息生成定点位置和uv坐标，最终生成网格，其实相当于一个大Image。
    //文字链接： 根据富文本信息生成文字链接的虚拟点击矩形区域，用来鼠标点击的时候判断是否点到对应的文字。
    //咱们自定义的富文本标记格式为：[标记类型]表情名称or链接文字[-]
    //标记类型：目前只支持两种 一种 e 另一种 l ，e代表表情，l代表文字链接举例：表情 [e]Dog_laugh[-]，链接 [l]www.baidu.com[-]
    static UIVertex vert = new UIVertex();
    protected override void OnPopulateMesh(VertexHelper toFill)
    {
        m_Text = _BuildText;
        base.OnPopulateMesh(toFill);//生成文本网格
        //↓ 根据富文本信息做后续处理
        int i, j, startIndex, indics;
        for (i = 0; i < _GraphicItemModels.Count; i++)
        {
            GTextEmotionModel model = _GraphicItemModels[i];
            startIndex = model.TextIndex * 4;

            for (j = 0; j < 4; j++)
            {
                //清楚乱码
                indics = startIndex + j;
                if (indics >= toFill.currentVertCount) break;

                toFill.PopulateUIVertex(ref vert, indics);
                vert.uv0 = vert.uv1 = Vector2.zero;
                vert.position += new Vector3(GraphicOffset.x, GraphicOffset.y * 0.5f, GraphicOffset.z);

                toFill.SetUIVertex(vert, indics);

                //把quad的定点位置存下来在表情绘制的时候使用
                //只需要知道对角线的两个定点就能确定这个矩形
                if (j == 0)
                    model.PosMin = vert.position + new Vector3(0, (GraphicScale - 1) / 2 * fontSize, 0);
                if (j == 2)
                    model.PosMax = vert.position + new Vector3(0, -(GraphicScale - 1) / 2 * fontSize, 0);
            }
        }
        if (_GraphicBoard != null)
        {
            _GraphicBoard.SetVertexData(_GraphicItemModels);
        }

        for (i = 0; i < _TextLinkModels.Count; i++)
        {
            GTextLinkModel linkModel = _TextLinkModels[i];
            startIndex = linkModel.StartIndex * 4;
            int endIndex = linkModel.EndIndex * 4 + 3;
            if (startIndex >= toFill.currentVertCount)
            {
                continue;
            }
            toFill.PopulateUIVertex(ref vert, startIndex);
            Vector3 pos = vert.position;
            Vector3 last_char = pos;
            Bounds bounds = new Bounds(pos, Vector3.zero);
            for (j = startIndex + 2; j < endIndex; j += 2)
            {
                if (j >= toFill.currentVertCount)
                {
                    break;
                }

                toFill.PopulateUIVertex(ref vert, j);
                pos = vert.position;

                if (j % 4 == 0)
                {
                    if (pos.x < last_char.x)
                    {
                        if (bounds.size.x > 0)
                        {
                            linkModel.AddRect(new Rect(bounds.min, bounds.size));
                        }
                        bounds = new Bounds(pos, Vector3.zero);
                    }
                    else
                    {
                        bounds.Encapsulate(pos); // 扩展包围框
                    }

                    last_char = pos;
                }
                else
                {
                    bounds.Encapsulate(pos); // 扩展包围框
                }
            }

            if (bounds.size.x > 0)
            {
                linkModel.AddRect(new Rect(bounds.min, bounds.size));
            }
        }
        m_Text = _OriginText;
    }

    [NoToLuaAttribute]
    public void OnPointerClick(PointerEventData eventData)
    {
        if (OnClick == null) return;

        Vector2 lp;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, eventData.position, eventData.pressEventCamera, out lp);
        var clickLinkId = -1;
        for (int i = 0; i < _TextLinkModels.Count; i++)
        {
            GTextLinkModel linkModel = _TextLinkModels[i];
            for (int j = 0; j < linkModel.Rects.Count; ++j)
            {
                if (linkModel.Rects[j].Contains(lp))
                {
                    clickLinkId = i;
                    break;
                }
            }
        }
        //if (clickLinkId != -1)
        OnClick(_TextID, clickLinkId + 1);
    }

    [NoToLua]
    public override float preferredWidth
    {
        get
        {
            var settings = GetGenerationSettings(Vector2.zero);
            return cachedTextGeneratorForLayout.GetPreferredWidth(_BuildText, settings) / pixelsPerUnit;
        }
    }

    [NoToLua]
    public override float preferredHeight
    {
        get
        {
            var settings = GetGenerationSettings(new Vector2(GetPixelAdjustedRect().size.x, 0.0f));
            return cachedTextGeneratorForLayout.GetPreferredHeight(_BuildText, settings) / pixelsPerUnit;
        }
    }

    //E_xxx->xxx->sprite.Name
    string TranslateEmoji(string s)
    {
        return s.Replace("E_", "");

    }
}
