using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using LuaInterface;

namespace LT.Lua
{
    public class LuaMemoryProfiler : EditorWindow
    {
        private static bool canUse
        {
            get
            {
                return (EditorApplication.isPlaying && EntryPoint.Instance.IsInited);
            }
        }

        private LuaScriptMgr _luaScriptMgr;
        public LuaScriptMgr LuaScriptMgr
        {
            get
            {
                if (_luaScriptMgr == null)
                {
                    _luaScriptMgr = LuaScriptMgr.Instance;
                    _luaScriptMgr.DoString("require [[Tools.LuaMemoryProfiler]]");
                }
                return _luaScriptMgr;
            }
        }

        [MenuItem("Hoba Tools/Lua/Lua Memory Profiler", false, 2)]
        static void OpenWindow()
        {
            EditorWindow window = GetWindow<LuaMemoryProfiler>(false, "Lua Memory Profiler");
            if (window.position.position == Vector2.zero)
            {
                Resolution res = Screen.currentResolution;
                window.position = new Rect(res.width / 2 - 300, res.height / 2 - 300, 600, 600);
            }
            window.Show();
        }

        private static bool inRecord = false;

        class LuaMemoryTreeViewItem : TreeViewItem
        {
            public float memory, avgMemory;
            public int count;
            public int lineCount = 1;

            public LuaMemoryTreeViewItem(int id, LuaTable tab)
            {
                this.id = id;
                this.depth = 0;
                this.displayName = tab[1] as string;
                this.lineCount = 1;
                foreach (var c in this.displayName)
                {
                    if (c == '\n')
                        this.lineCount++;
                }

                this.count = System.Convert.ToInt32(tab[2]);
                this.memory = 1000 * System.Convert.ToSingle(tab[3]);
                this.avgMemory = this.memory / this.count;
            }
        }

        float startRecordCount = 0;
        float endRecordCount = 0;
        float totalIncreaseMem = 0;
        float anaMemRecord = 0;
        float toolMemRecord = 0;

        private void OnEnable()
        {
            startRecordCount = 0;
            endRecordCount = 0;
            totalIncreaseMem = 0;
            anaMemRecord = 0;
            toolMemRecord = 0;
            inRecord = false;
        }

        TreeView m_TreeView;
        void OnGUI()
        {
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Scripts Diff", EditorUtility.FormatBytes(Mathf.RoundToInt(anaMemRecord)), EditorStyles.boldLabel, GUILayout.Width(250));
            EditorGUILayout.LabelField("Total Memory Diff", EditorUtility.FormatBytes(Mathf.RoundToInt(totalIncreaseMem)), GUILayout.Width(250));
            EditorGUILayout.LabelField("Profiler Tool Diff", EditorUtility.FormatBytes(Mathf.RoundToInt(toolMemRecord)), GUILayout.Width(250));
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUI.enabled = canUse && !inRecord;
            // lua returned KB 
            if (GUILayout.Button("Start Record"))
            {
                inRecord = true; 
                startRecordCount = 1000 * System.Convert.ToSingle(LuaScriptMgr.CallLuaFunction("SG_StartRecordAlloc")[0]);
            }

            GUI.enabled = canUse && inRecord;
            if (GUILayout.Button("Stop Record"))
            {
                inRecord = false;
                var m_Root = new TreeViewItem { id = 0, depth = -1, displayName = "Root" };
                int m_Id = 1;

                var profilerData = LuaScriptMgr.CallLuaFunction("SG_StopRecordAllocAndDumpStat");
                endRecordCount = 1000 * System.Convert.ToSingle(profilerData[0]);
                totalIncreaseMem = 1000 * System.Convert.ToSingle(profilerData[1]);
                anaMemRecord = 1000 * System.Convert.ToSingle(profilerData[2]);
                toolMemRecord = 1000 * System.Convert.ToSingle(profilerData[3]);
                
                LuaTable memStat = profilerData[4] as LuaTable;
                var enumerator = memStat.GetEnumerator();
                
                while (enumerator.MoveNext())
                {
                    var child = new LuaMemoryTreeViewItem(m_Id++, enumerator.Value as LuaTable);
                    m_Root.AddChild(child);
                }
                if (!m_Root.hasChildren)
                {
                    EditorUtility.DisplayDialog("Error", "未统计到Lua运行数据，请游戏操作Lua脚本之后再点击结束按钮", "确定");
                    return;
                }
                //LuaDLL.lua_pop(LuaState.L, 1);

                Debug.Log("Total Memory Diff " + EditorUtility.FormatBytes(Mathf.RoundToInt(endRecordCount - startRecordCount)));

                m_TreeView = LuaMemoryTreeView.Create(m_Root);
            }

            GUI.enabled = true;
            GUILayout.EndHorizontal();

            if (m_TreeView != null)
                m_TreeView.OnGUI(new Rect(0, 60, position.width, position.height - 30));
        }

        class LuaMemoryTreeView : TreeView
        {
            public static LuaMemoryTreeView Create(TreeViewItem root)
            {
                var columns = new[]
                {
                    new MultiColumnHeaderState.Column
                    {
                        headerContent = new GUIContent("File"),
                        headerTextAlignment = TextAlignment.Left,
                        width = 350,
                        minWidth = 200,
                        autoResize = false,
                    },
                    new MultiColumnHeaderState.Column
                    {
                        headerContent = new GUIContent("Count"),
                        headerTextAlignment = TextAlignment.Right,
                        width = 60,
                        minWidth = 40,
                        autoResize = false,
                    },
                    new MultiColumnHeaderState.Column
                    {
                        headerContent = new GUIContent("TotalMem"),
                        headerTextAlignment = TextAlignment.Right,
                        width = 150,
                        minWidth = 120,
                        autoResize = false,
                    },
                    new MultiColumnHeaderState.Column
                    {
                        headerContent = new GUIContent("AvgMem(/Frame)"),
                        headerTextAlignment = TextAlignment.Right,
                        width = 150,
                        minWidth = 120,
                        autoResize = true
                    },
                };

                var state = new MultiColumnHeaderState(columns);
                return new LuaMemoryTreeView(new TreeViewState(), new MultiColumnHeader(state), root);
            }

            TreeViewItem m_Root = null;
            IEnumerable<LuaMemoryTreeViewItem> m_Data = null;
            public LuaMemoryTreeView(TreeViewState state, MultiColumnHeader multiColumnHeader, TreeViewItem root)
            : base(state, multiColumnHeader)
            {
                showAlternatingRowBackgrounds = true;
                showBorder = true;
                multiColumnHeader.sortingChanged += OnSortingChanged;

                m_Root = root;
                m_Data = root.children.Cast<LuaMemoryTreeViewItem>();

                Reload();
            }

            protected override TreeViewItem BuildRoot()
            {
                return m_Root;
            }

            protected override float GetCustomRowHeight(int row, TreeViewItem item)
            {
                var item2 = item as LuaMemoryTreeViewItem;
                if (item2 == null)
                    return base.GetCustomRowHeight(row, item);
                return item2.lineCount * 16;
            }

            protected override void RowGUI(RowGUIArgs args)
            {
                var item = (LuaMemoryTreeViewItem)args.item;

                for (int i = 0; i < args.GetNumVisibleColumns(); ++i)
                {
                    var cellRect = args.GetCellRect(i);

                    string value = "";
                    switch (args.GetColumn(i))
                    {
                        case 0:
                            value = item.displayName;
                            break;
                        case 1:
                            value = item.count.ToString();
                            break;
                        case 2:
                            value = EditorUtility.FormatBytes(Mathf.RoundToInt(item.memory));
                            break;
                        case 3:
                            value = EditorUtility.FormatBytes(Mathf.RoundToInt(item.avgMemory));
                            break;
                    }

                    if (i == 0)
                        DefaultGUI.Label(cellRect, value, args.selected, args.focused);
                    else
                        DefaultGUI.LabelRightAligned(cellRect, value, args.selected, args.focused);
                }
            }

            void OnSortingChanged(MultiColumnHeader multiColumnHeader)
            {
                if (multiColumnHeader.sortedColumnIndex == -1)
                {
                    return; // No column to sort for (just use the order the data are in)
                }

                // Sort the roots of the existing tree items
                var sortedColumns = multiColumnHeader.state.sortedColumns;
                if (sortedColumns.Length == 0)
                    return;

                var sortOption = sortedColumns[0];

                IOrderedEnumerable<LuaMemoryTreeViewItem> sortedData = null;
                switch (sortOption)
                {
                    case 0:
                        sortedData = m_Data.OrderBy(l => l.displayName);
                        break;
                    case 1:
                        sortedData = m_Data.OrderBy(l => l.count);
                        break;
                    case 2:
                        sortedData = m_Data.OrderBy(l => l.memory);
                        break;
                    case 3:
                        sortedData = m_Data.OrderBy(l => l.avgMemory);
                        break;
                }

                m_Root.children = sortedData.Cast<TreeViewItem>().ToList();
                Reload();
            }
        }
    }
}