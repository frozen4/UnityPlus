using UnityEngine;
using UnityEditor;
using LuaInterface;
using UnityEditor.IMGUI.Controls;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace LT.Lua
{
    public class LuaTracer : EditorWindow
    {
        private static EditorWindow _EditorWindow;

        private static string CodeSoftPath;
        private const string CodeSoftPathSaveKey = "LuaTracerCodeSoftPathSaveKey";

        public static string GameResPath;

        public static bool AutoGoToCodeSoft = false;

        public bool IsWhiteList = false;
        public string WhiteList = "";
        private const string WhiteListSaveKey = "LuaTracerWhiteListSaveKey";
        public bool IsBlackList = true;
        public string BlackList = "Lplus.lua;protobuf;Debug.lua;Quaternion.lua;Vector3.lua";
        private const string BlackListSaveKey = "LuaTracerBlackListSaveKey";

        [MenuItem("Hoba Tools/Lua/Lua Tracer", false, 2)]
        static void OpenWindow()
        {
            _EditorWindow = GetWindow<LuaTracer>(false, "Lua Tracer");
            if (_EditorWindow.position.position == Vector2.zero)
            {
                Resolution res = Screen.currentResolution;
                _EditorWindow.position = new Rect(res.width / 2 - 300, res.height / 2 - 300, 678, 870);
            }
            _EditorWindow.Show();
        }

        private LuaScriptMgr _luaScriptMgr;
        public LuaScriptMgr LuaScriptMgr
        {
            get
            {
                if (_luaScriptMgr == null)
                {
                    _luaScriptMgr = LuaScriptMgr.Instance;
                    _luaScriptMgr.DoString("require [[Tools.LuaTracer]]");
                }
                return _luaScriptMgr;
            }
        }

        public bool WorkEnable
        {
            get
            {
                return EntryPoint.Instance.IsInited;
            }
        }

        List<string> LuaRes = new List<string>();
        private void OnEnable()
        {
            GameResPath = Application.dataPath + "/../../GameRes/";
            CodeSoftPath = PlayerPrefs.GetString(CodeSoftPathSaveKey, "");

            BlackList = PlayerPrefs.GetString(BlackListSaveKey, BlackList);
            WhiteList = PlayerPrefs.GetString(WhiteListSaveKey, WhiteList);

            ReadData();
        }

        void ReadData()
        {
            LuaRes.Clear();
            if (!File.Exists(GameResPath + "LuaTracer.csv"))
                return;
            StreamReader streamReader = new StreamReader(GameResPath + "LuaTracer.csv");
            string strLine = streamReader.ReadLine();
            while (strLine != null)
            {
                LuaRes.Add(strLine);
                strLine = streamReader.ReadLine();
            }
            streamReader.Close();
            MakeTree();
        }

        int CodeInfoCount = 0;
        int Page = 0;
        const int PageItemCount = 50;
        const int PageShowCount = 10000;
        TreeView m_TreeView;
        static float TreeViewWidth = 0;
        float TreeViewHeight = 16 * (PageItemCount + 2);
        bool isInTrace = false;
        private void OnGUI()
        {
            GUILayout.BeginHorizontal();
            GUI.enabled = WorkEnable;
            if (GUILayout.Button("StartTrace", GUILayout.Height(30)))
            {
                isInTrace = true;
                LuaScriptMgr.CallLuaFunction("LT_StartLuaTrace", IsWhiteList, IsBlackList, WhiteList, BlackList);
            }
            GUI.enabled = WorkEnable && isInTrace;
            if (GUILayout.Button("StopTrace", GUILayout.Height(30)))
            {
                isInTrace = false;
                LuaRes.Clear();
                Page = 0;
                
                var data = LuaScriptMgr.CallLuaFunction("LT_StopLuaTrace");
                ReadData();
            }
            GUI.enabled = true;
            GUILayout.EndHorizontal();

            TreeViewWidth = position.width / 2 - 20;

            GUILayout.BeginVertical();
            if (m_TreeView != null)
                m_TreeView.OnGUI(new Rect(10, 35, TreeViewWidth, TreeViewHeight));
            GUILayout.EndVertical();
            
            GUILayout.BeginArea(new Rect(TreeViewWidth + 30, 50, position.width - TreeViewWidth - 40, TreeViewHeight));
            EditorGUILayout.Space();

            GUILayout.BeginHorizontal();
            GUILayout.Label("选择编辑Lua文件的软件，目前支持sublime、vscode");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label(CodeSoftPath, GUILayout.Width(TreeViewWidth - 80), GUILayout.Height(30));
            if(GUILayout.Button("选择", GUILayout.Width(60)))
            {
                var exePath = EditorUtility.OpenFilePanel("选择Lua脚本编辑软件", "", "exe");
                if(!string.IsNullOrEmpty(exePath))
                {
                    CodeSoftPath = exePath;
                    PlayerPrefs.SetString(CodeSoftPathSaveKey, CodeSoftPath);
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(50);
            //*******************************************黑名单白名单*******************************************//
            var isWhiteList = IsWhiteList;
            isWhiteList = GUILayout.Toggle(isWhiteList, "使用白名单(编辑后保存在本地,';'分隔)");
            if(isWhiteList != IsWhiteList)
            {
                IsWhiteList = isWhiteList;
                IsBlackList = !isWhiteList;
            }
            var whiteList = WhiteList;
            whiteList = GUILayout.TextArea(whiteList, GUILayout.Height(80));
            if(whiteList != WhiteList)
            {
                WhiteList = whiteList;
                PlayerPrefs.SetString(WhiteListSaveKey, WhiteList);
            }
            
            EditorGUILayout.Space();
            var isBlackList = IsBlackList;
            isBlackList = GUILayout.Toggle(isBlackList, "使用黑名单(编辑后保存在本地,';'分隔))");
            if(isBlackList != IsBlackList)
            {
                IsBlackList = isBlackList;
                IsWhiteList = !isBlackList;
            }
            var blackList = BlackList;
            blackList = GUILayout.TextArea(blackList, GUILayout.Height(80));
            if(blackList != BlackList)
            {
                BlackList = blackList;
                PlayerPrefs.SetString(BlackListSaveKey, BlackList);
            }

            GUILayout.Space(50);
            //*******************************************翻页*******************************************//
            GUILayout.BeginHorizontal();
            GUI.enabled = Page > 0;
            var showBtn = string.Format("上一页({0})", Page);
            if (GUILayout.Button(showBtn, GUILayout.Height(30)))
            {
                if(Page > 0)
                {
                    Page--;
                    MakeTree();
                }
            }

            var totalPage = LuaRes.Count / PageShowCount - 1;
            GUI.enabled = Page <= totalPage;
            showBtn = string.Format("下一页({0})", totalPage - Page + 1);
            if (GUILayout.Button(showBtn, GUILayout.Height(30)))
            {
                Page++;
                MakeTree();
            }
            GUI.enabled = true;
            GUILayout.EndHorizontal();
            
            GUILayout.Space(50);
            //*******************************************调试*******************************************//
            AutoGoToCodeSoft = GUILayout.Toggle(AutoGoToCodeSoft, "自动跳转到Lua代码(需要设置上面编辑软件路径)");
            if (AutoGoToCodeSoft)
            {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("上一步", GUILayout.Height(30)))
                {
                    var sel = m_TreeView.GetSelection();
                    if (sel.Count > 0 && sel[0] > 0)
                    {
                        m_TreeView.SetSelection(new int[] { sel[0] - 1 }, TreeViewSelectionOptions.FireSelectionChanged);
                    }
                }
                if (GUILayout.Button("下一步", GUILayout.Height(30)))
                {
                    var sel = m_TreeView.GetSelection();
                    if (sel.Count > 0 && sel[0] < m_TreeView.GetRows().Count)
                    {
                        m_TreeView.SetSelection(new int[] { sel[0] + 1 }, TreeViewSelectionOptions.FireSelectionChanged);
                    }
                }
                GUILayout.EndHorizontal();
            }

            GUILayout.EndArea();
        }

        void MakeTree()
        {
            var m_Root = new TreeViewItem { id = 0, depth = -1, displayName = "Root" };
            int iStart = Page * PageShowCount;
            for (int i = 0; i < PageShowCount; i ++)
            {
                var ind = iStart + i;
                if (ind >= LuaRes.Count)
                    break;
                var res = LuaRes[ind];
                var child = new CustomTreeViewItem(i, ind, res);
                m_Root.AddChild(child);
            }
           m_TreeView = CustomTreeView.Create(m_Root);
        }

        class CustomTreeViewItem : TreeViewItem
        {
            public int lineCount = 1;
            public int index = 0;

            public CustomTreeViewItem(int id, int index, string res)
            {
                this.id = id;
                this.depth = 0;
                this.index = index;
                string[] splits = res.Split(',');
                this.displayName = res;
                foreach (var c in this.displayName)
                {
                    if (c == '\n')
                        this.lineCount++;
                }
            }
        }

        class CustomTreeView : TreeView
        {
            public static CustomTreeView Create(TreeViewItem root)
            {
                var columns = new[]
                {
                    new MultiColumnHeaderState.Column
                    {
                        headerContent = new GUIContent("ID"),
                        headerTextAlignment = TextAlignment.Center,
                        width = 50,
                        maxWidth = 60,
                        autoResize = false,
                    },
                    new MultiColumnHeaderState.Column
                    {
                        headerContent = new GUIContent("文件名称:行数(双击跳转)"),
                        headerTextAlignment = TextAlignment.Center,
                        width = 280,
                        maxWidth = 350,
                        autoResize = false,
                    }
                };

                var state = new MultiColumnHeaderState(columns);
                return new CustomTreeView(new TreeViewState(), new MultiColumnHeader(state), root);
            }

            TreeViewItem m_Root = null;
            IEnumerable<CustomTreeViewItem> m_Data = null;
            public CustomTreeView(TreeViewState state, MultiColumnHeader multiColumnHeader, TreeViewItem root)
            : base(state, multiColumnHeader)
            {
                showAlternatingRowBackgrounds = true;
                showBorder = true;

                m_Root = root;
                m_Data = root.children.Cast<CustomTreeViewItem>();

                Reload();
            }

            protected override void DoubleClickedItem(int id)
            {
                OpenFile(id);
            }

            private void OpenFile(int id)
            {
                if (string.IsNullOrEmpty(CodeSoftPath))
                    return;
                var children = m_Root.children;
                if (id >= children.Count)
                    return;
                var displayName = children[id].displayName;
                int index = displayName.LastIndexOf(".lua");
                string fileName = displayName.Substring(0, index + 4);
                string line = displayName.Substring(index + 5);
                string args = GameResPath + "Lua/" + fileName;
                args = args.Replace("/", "\\");
                if (CodeSoftPath.Contains("sublime"))
                {
                    args = string.Format(args + ":{0}:0", line);
                }
                else if (CodeSoftPath.Contains("Code"))
                {
                    args = string.Format("-g " + args + ":{0}:0", line);
                }
                System.Diagnostics.Process.Start(CodeSoftPath, args);
            }

            //protected override void ContextClickedItem(int id)
            protected override void SelectionChanged(IList<int> selectedIds)
            {
                if(AutoGoToCodeSoft)
                    OpenFile(selectedIds[0]);
            }

            protected override TreeViewItem BuildRoot()
            {
                return m_Root;
            }

            protected override float GetCustomRowHeight(int row, TreeViewItem item)
            {
                var item2 = item as CustomTreeViewItem;
                if (item2 == null)
                    return base.GetCustomRowHeight(row, item);
                return item2.lineCount * 16;
            }

            protected override void RowGUI(RowGUIArgs args)
            {
                var item = (CustomTreeViewItem)args.item;

                for (int i = 0; i < args.GetNumVisibleColumns(); ++i)
                {
                    var cellRect = args.GetCellRect(i);

                    string value = "";
                    switch (args.GetColumn(i))
                    {
                        case 0:
                            value = item.index.ToString();
                            break;
                        case 1:
                            value = item.displayName;
                            break;
                    }

                    DefaultGUI.Label(cellRect, value, args.selected, args.focused);
                    //if (i == 0)
                    //    DefaultGUI.Label(cellRect, value, args.selected, args.focused);
                    //else
                    //    DefaultGUI.LabelRightAligned(cellRect, value, args.selected, args.focused);
                }
            }
        }
    }
}