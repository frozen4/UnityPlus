using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public class ProportionHelperWindow : EditorWindow
{
    
    private static EditorWindow _EditorWindow;

    [MenuItem("Hoba Tools/性能分析/帧率热力图", false, 2)]
    static void OpenWindow()
    {
        _EditorWindow = GetWindow<ProportionHelperWindow>(false, "Proportion");
        if (_EditorWindow.position.position == Vector2.zero)
        {
            Resolution res = Screen.currentResolution;
            _EditorWindow.position = new Rect(res.width / 2 - 300, res.height / 2 - 300, 678, 870);
        }
        _EditorWindow.Show();
    }

    struct FpsInfo
    {
        public int x, y;
        public float fps;
        public FpsInfo(string info)
        {
            var infos =  info.Split(',');
            fps = float.Parse(infos[0]);
            x = int.Parse(infos[1]);
            y = int.Parse(infos[2]);
        }
    }

    List<FpsInfo> _FpsInfo = new List<FpsInfo>();
    Texture2D _TextureFps;
    int _BlowUp = 3;//放大倍数
    int _Row = 0;
    int _Col = 0;

    float _MinFps = 0;
    float _MaxFps = 0;
    int _Weight = 3;
    float RedToYellow = 0.66f;
    float YellowToGreen = 0.33f;

    void MakeTexture()
    {
        _FpsInfo.Clear();

        var GameResPath = Application.dataPath + "/../../GameRes/";
        if (!File.Exists(GameResPath + "fps.csv"))
            return;

        StreamReader streamReader = new StreamReader(GameResPath + "fps.csv");
        string strLine = streamReader.ReadLine();
        _Row = int.Parse(strLine);
        strLine = streamReader.ReadLine();
        _Col = int.Parse(strLine);
        strLine = streamReader.ReadLine();
        while (strLine != null)
        {
            var info = new FpsInfo(strLine);
            var fps = info.fps;
            if (fps > _MaxFps)
                _MaxFps = fps;
            if (fps < _MinFps)
                _MinFps = fps;
            _FpsInfo.Add(info);
            strLine = streamReader.ReadLine();
        }
        _MinFps = 0;// _MinFps / _Weight;
        streamReader.Close();

        var col = _Col * _BlowUp - _BlowUp + 1;
        var row = _Row * _BlowUp - _BlowUp + 1;
        _TextureFps = new Texture2D(col, row);

        float[,] WeightColor = new float[col, row];
        //Debug.Log("Count : " + row * col);
        //Debug.Log("row : " + row + " col : " + col);
        //Debug.Log("_MaxFps : " + _MaxFps + " _MinFps : " + _MinFps);
        //Debug.Log("_FpsInfo Count : " + _FpsInfo.Count);
        //Debug.Log("_Weight : " + _Weight);
        for (int m = 0; m < _FpsInfo.Count; m++)
        {
            var info = _FpsInfo[m];
            var y = info.y * _BlowUp;
            var x = info.x * _BlowUp;
            for (int i = x - _Weight; i <= x + _Weight; i++)
            {
                if (i < 0 || i >= col) continue;
                for (int j = y - _Weight; j <= y + _Weight; j++)
                {
                    if (j < 0 || j >= row) continue;
                    var xDis = Mathf.Abs(x - i);
                    var yDis = Mathf.Abs(y - j);
                    if (xDis * xDis + yDis * yDis > _Weight * _Weight)
                        continue;
                    float dis = Vector2.Distance(new Vector2(xDis, yDis), new Vector2(0, 0));
                    WeightColor[i, j] += ((float)(_Weight - dis) / _Weight) * info.fps;
                }
            }
        }
        for (int i = 0; i < row; i ++)
        {
            for(int j = 0; j < col; j++)
            {
                _TextureFps.SetPixel(j, i, GetFpsColor(WeightColor[j, i]));
            }
        }
        
        _TextureFps.Apply();
    }

    Color GetFpsColor(float fps)
    {
        var mul = (fps - _MinFps) / (_MaxFps - _MinFps);
        Color color = Color.white;

        if(mul > RedToYellow)
        {
            color = new Color(1, 1, 0);
            color.g = 1 - ((float)(mul - RedToYellow) * (1 / (1 - RedToYellow)));
        }
        else if (mul > YellowToGreen)
        {
            color = new Color(0, 1, 0);
            color.r = ((float)(mul - YellowToGreen) * (1 / YellowToGreen));
        }
        else
        {
            color = new Color(0, 0, 1);
            color.g = mul * (1 / YellowToGreen);
            color.b = 1 - mul * (1 / YellowToGreen);
        }
        return color;
    }

    private void OnGUI()
    {
        EditorGUILayout.Space();
        GUILayout.BeginHorizontal();
        _Weight = EditorGUILayout.IntField("权重范围", _Weight);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        _BlowUp = EditorGUILayout.IntField("放大倍数", _BlowUp);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        RedToYellow = EditorGUILayout.FloatField("红色到黄色界限", RedToYellow);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        YellowToGreen = EditorGUILayout.FloatField("黄色到绿色界限", YellowToGreen);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        if(GUILayout.Button("生成热力图"))
        {
            MakeTexture();
        }
        GUILayout.EndHorizontal();
        EditorGUILayout.Space();
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("导出热力图"))
        {
            var filePath = EditorUtility.SaveFilePanel("保存热力图", "", "HeatMap","png");
            var bytes = _TextureFps.EncodeToPNG();
            var file = File.Open(filePath, FileMode.Create);
            var binary = new BinaryWriter(file);
            binary.Write(bytes);
            file.Close();
        }
        GUILayout.EndHorizontal();

        GUILayout.Box(_TextureFps);
    }
}