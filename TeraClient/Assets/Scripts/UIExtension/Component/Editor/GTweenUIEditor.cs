using UnityEditor;
using UnityEditor.UI;
using UnityEngine;
using System.Collections.Generic;

[CustomEditor(typeof(GTweenUIBase), true)]
public class GTweenUIEditor : Editor
{
    private SerializedProperty _OnTweenStart;
    private SerializedProperty _OnTweenFinish;
    private SerializedProperty _AutoPlay;
    private SerializedProperty _Delay;
    private SerializedProperty _Duration;
    private SerializedProperty _InitState;
    private SerializedProperty _StopWhenDisabled;
    private SerializedProperty _TweenList;

    private GTweenUIBase _TweenUIBase;

    protected virtual void OnEnable()
    {
        _TweenUIBase = target as GTweenUIBase;

        _Delay = serializedObject.FindProperty("Delay");
        _Duration = serializedObject.FindProperty("Duration");
        _AutoPlay = serializedObject.FindProperty("AutoPlay");
        _InitState = serializedObject.FindProperty("InitState");
        _StopWhenDisabled = serializedObject.FindProperty("StopWhenDisabled");

        _TweenList = serializedObject.FindProperty("TweenList");

        _OnTweenStart = serializedObject.FindProperty("OnTweenStart");
        _OnTweenFinish = serializedObject.FindProperty("OnTweenFinish");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(_Duration);
        EditorGUILayout.PropertyField(_Delay);
        EditorGUILayout.PropertyField(_AutoPlay);
        EditorGUILayout.PropertyField(_InitState);
        EditorGUILayout.PropertyField(_StopWhenDisabled);

        GUILayout.Space(10);
        DrawList(_TweenList);
        GUILayout.Space(10);

        EditorGUILayout.PropertyField(_OnTweenStart);
        EditorGUILayout.PropertyField(_OnTweenFinish);

        serializedObject.ApplyModifiedProperties();
    }

    protected virtual void DrawList(SerializedProperty list)
    {
        ++EditorGUI.indentLevel;

        for (int i = 0; i < _TweenList.arraySize; ++i)
        {
            SerializedProperty spi = _TweenList.GetArrayElementAtIndex(i);
            if (_TweenUIBase.TweenList[i] != null)
            {
                DrawItem(i, spi);
                GUILayout.Space(2);
            }
        }
        EditorGUI.indentLevel -= 1;
    }

    protected virtual void DrawItem(int idx, SerializedProperty item)
    {
        ++EditorGUI.indentLevel;

        EditorGUILayout.BeginVertical("AS TextArea", GUILayout.MinHeight(10f));
        GUILayout.Space(5);

        SerializedProperty sp_enabled = item.FindPropertyRelative("IsEnabled");
        if (sp_enabled.boolValue)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(5);
            if (GUILayout.Button("-", GUILayout.Width(20)))
            {
                sp_enabled.boolValue = false;
                return;
            }

            GUILayout.Label(new GUIContent(_TweenUIBase.ItemName4Editor(idx)));
            GUILayout.EndHorizontal();

            GUILayout.BeginVertical();

            SerializedProperty sp_tweenType = item.FindPropertyRelative("TweenType");
            //SerializedProperty sp_tweenTarget = item.FindPropertyRelative("tweenTarget");
            SerializedProperty sp_startValue = item.FindPropertyRelative("StartValue");
            SerializedProperty sp_endValue = item.FindPropertyRelative("EndValue");

            EditorGUILayout.PropertyField(sp_tweenType);
            //EditorGUILayout.PropertyField(sp_tweenTarget);
            EditorGUILayout.PropertyField(sp_startValue);
            EditorGUILayout.PropertyField(sp_endValue);

            if ((GTweenUIBase.TweenType)sp_tweenType.enumValueIndex == GTweenUIBase.TweenType.Curv)
            {
                SerializedProperty sp_animCurv = item.FindPropertyRelative("AnimCurv");
                EditorGUILayout.PropertyField(sp_animCurv);
            }
            GUILayout.EndVertical();
        }
        else
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(5);
            if (GUILayout.Button("+", GUILayout.Width(20)))
            {
                sp_enabled.boolValue = true;
                return;
            }

            GUILayout.Label(new GUIContent(_TweenUIBase.ItemName4Editor(idx)));
            GUILayout.EndHorizontal();
        }

        GUILayout.Space(5);
        EditorGUILayout.EndVertical();

        EditorGUI.indentLevel -= 1;
    }

}