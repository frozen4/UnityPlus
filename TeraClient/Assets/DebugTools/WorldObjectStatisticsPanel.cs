using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Text;

public class WorldObjectStatisticsPanel : GBase
{
    private static int InfoTypeCount = 2;
    private static int MaxMemLog = 150;
	
	public GameObject _Container;
    public Text _Info;
    public GButtonColorElastic _MauBtn;

    private int _Index = -1;
	private StringBuilder _StrBuilder;
	private string _WorldRootName = "World01(Clone)";
    private Queue<string> _LogItems = new Queue<string>();

    protected override void Awake()
	{
        //Application.logMessageReceived += (condition, stackTrace, type) =>
        //{
        //    if (_LogItems.Count >= MaxMemLog) 
        //    {
        //        _LogItems.Dequeue();
        //    }
        //    string col = string.Empty;
        //    if (type == LogType.Error)
        //        col = "#ff0000";
        //    else if (type == LogType.Warning)
        //        col = "#ffff00";
        //    else if (type == LogType.Exception)
        //        col = "#ff8247";
        //    else if (type == LogType.Log)
        //        col = "#ffffff";
        //    else if (type == LogType.Assert)
        //        col = "#ffb90f";
        //    else
        //        col = "#8b8b00";
        //    condition = GetHtmlColorString(condition, col);
        //    _LogItems.Enqueue(condition);
        //    if (_Index == 2 && _Container.activeSelf)
        //        Refresh();
        //};

        _MauBtn.OnClick = (go) =>
        {
            _Info.raycastTarget = !_Info.raycastTarget;
            _MauBtn.text = _Info.raycastTarget ? "自动滚屏" : "手动拖拽";
        };

    }
    private string GetHtmlColorString(string src, Color color)
    {
        return "<color=\"#"+ColorUtility.ToHtmlStringRGB(color).ToLower() +"\">" + src + "</color>";
    }
    private string GetHtmlColorString(string src, string color)
    {
        return "<color=" + color + ">" + src + "</color>";
    }
    private void OpenOrColosePanel()
    {
        _Container.SetActive(!_Container.activeSelf);
        if (_Index == -1)
            ChangeView();
    }

    private void ChangeView()
    {
        if (!IsActive()) return;
        int next_index = _Index + 1;
        if (next_index > InfoTypeCount) 
		{
			next_index = 0;
		}
		
        _Index = next_index;
		Refresh();
    }

    private void Refresh()
    {
        if (_Index == 0)
        {
            StatisticsAnim();
        }
        else if (_Index == 1)
        {
            StatisticsPartic();
        }
        else if (_Index == 2)
        {
             StatisticsLogs();
        }
    }
	
    private void StatisticsAnim()
    {
		_StrBuilder = new StringBuilder();
		_StrBuilder.AppendLine("==========动画相关信息==========");
		_StrBuilder.AppendLine();
		GameObject world_root = GameObject.Find(_WorldRootName);
        if (world_root == null) 
        {
            _StrBuilder.AppendLine("找不到根节点！");
            _Info.text = _StrBuilder.ToString();
            return;
        }
        
		Animator[] animators = world_root.GetComponentsInChildren<Animator>();
		Animation[] animations = world_root.GetComponentsInChildren<Animation>();
		_StrBuilder.AppendLine("Animator组件 : "+animators.Length);
		_StrBuilder.AppendLine("Animation组件 : "+animations.Length);
		_StrBuilder.AppendLine("==========animators==========");
		int i;
		for(i=0;i<animators.Length;i++)
		{
			Transform anim_trans = animators[i].transform;
			string path = anim_trans.name;
			while (anim_trans.parent!=null)
			{
				path = "\\"+anim_trans.parent.name+"\\"+path;
				anim_trans = anim_trans.parent;
			}
			_StrBuilder.AppendLine(path);
		}
		
		_StrBuilder.AppendLine();
		_StrBuilder.AppendLine("==========animations==========");
		for(i=0;i<animations.Length;i++)
		{
			Transform anim_trans = animations[i].transform;
			string path = anim_trans.name;
			while (anim_trans.parent!=null)
			{
				path = "\\"+anim_trans.parent.name+"\\"+path;
				anim_trans = anim_trans.parent;
			}
			_StrBuilder.AppendLine(path);
		}
		_StrBuilder.AppendLine();
		_StrBuilder.AppendLine();
		_StrBuilder.AppendLine("==========over==========");
		_Info.text = _StrBuilder.ToString();
    }
    private void StatisticsPartic()
    {
		_StrBuilder = new StringBuilder();
		_StrBuilder.AppendLine("==========粒子相关信息==========");
		_StrBuilder.AppendLine();
		GameObject world_root = GameObject.Find(_WorldRootName);
        if (world_root == null)
        {
            _StrBuilder.AppendLine("找不到根节点！");
            _Info.text = _StrBuilder.ToString();
            return;
        }

        ParticleSystem[] partic = world_root.GetComponentsInChildren<ParticleSystem>();
		
		_StrBuilder.AppendLine("粒子组件数 : "+partic.Length);
		
		_StrBuilder.AppendLine();
		
		int i;
		for(i=0;i<partic.Length;i++)
		{
			Transform anim_trans = partic[i].transform;
			string path = anim_trans.name;
			while (anim_trans.parent!=null)
			{
				path = "\\"+anim_trans.parent.name+"\\"+path;
				anim_trans = anim_trans.parent;
			}
			_StrBuilder.AppendLine(path);
		}

		_StrBuilder.AppendLine();
		_StrBuilder.AppendLine();
		_StrBuilder.AppendLine("统计完成");
		_Info.text = _StrBuilder.ToString();
    }
	
	List<string> tex_info = new List<string>();
    private void StatisticsTex()
    {
		tex_info = new List<string>();
		tex_info.Add("==========贴图信息==========");
		
		GameObject world_root = GameObject.Find(_WorldRootName);
        if (world_root == null)
        {
            _StrBuilder.AppendLine("找不到根节点！");
            _Info.text = _StrBuilder.ToString();
            return;
        }

        Renderer[] renderers = world_root.GetComponentsInChildren<Renderer>();
		tex_info.Add("Renderer组件 : "+renderers.Length);
		tex_info.Add("");
		
		//List<string> texture_paths = new List<string>();

#if UNITY_EDITOR
        int i,j;
		for(i=0;i<renderers.Length;i++)
		{
            // Transform anim_trans = renderers[i].transform;
            // string path = anim_trans.name;
            // while (anim_trans.parent!=null)
            // {
            // 	path = "\\"+anim_trans.parent.name+"\\"+path;
            // 	anim_trans = anim_trans.parent;
            // }
            // tex_info.Add(path);

            Material[] mats = renderers[i].materials;
            for (j = 0; j < mats.Length; j++)
            {
                Texture tex = mats[j].mainTexture;
                if (tex == null) continue;
                //string path = UnityEditor.AssetDatabase.GetAssetPath(tex);
                tex_info.Add(tex.name + " : "+tex.width+" x "+tex.height);
            }
        }
#endif
        tex_info.Add("");
		tex_info.Add("统计完成");
    }
    private void StatisticsLogs()
    {
        _Info.text = string.Join("\n", _LogItems.ToArray());
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
        {
            if (Input.GetKeyDown(KeyCode.F1))
            {
                OpenOrColosePanel();
            }
        }
        else if (Input.GetKeyDown(KeyCode.F1))
        {
            ChangeView();
        }
    }
}
