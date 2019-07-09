using UnityEditor;
using UnityEditor.UI;
using UnityEngine;
using System.Collections.Generic;
using GNewUI;

[CustomEditor(typeof(GUIAnim), true)]
public class GUIAnimEditor : Editor
{
    private GUIAnim _UIAnim;
    private SerializedProperty _AnimList;
    private SerializedProperty _SoundList;
    private SerializedProperty _FxList;
    private SerializedProperty _ATList;

    private MessageType msg_type;
    private string check_msg = "Every thing is fine";

    GUIContent _ct_fade;
    GUIContent _ct_from;
    GUIContent _ct_name;
    GUIContent _ct_to;
    GUIContent _ct_add;

    protected virtual void OnEnable()
    {
        _UIAnim = target as GUIAnim;

        _AnimList = serializedObject.FindProperty("AnimEvents");
        _SoundList = serializedObject.FindProperty("SoundEvents");
        _FxList = serializedObject.FindProperty("FxEvents");
        _ATList = serializedObject.FindProperty("AnimTrans");

        _ct_fade = new GUIContent("fade");
        _ct_from = new GUIContent("from");
        _ct_name = new GUIContent("Name");
        _ct_to = new GUIContent("Transist to");
        _ct_add = new GUIContent("Add new");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        //System.Text.StringBuilder sb = new System.Text.StringBuilder();

        GUILayout.Space(10);
        EditorGUILayout.HelpBox(check_msg, msg_type);

        GUILayout.Space(10);
        DrawList(_AnimList, GNewUI.AimEventType.Anim);
        GUILayout.Space(10);

        DrawList(_SoundList, GNewUI.AimEventType.Sound);
        GUILayout.Space(10);

        DrawList(_FxList, GNewUI.AimEventType.Fx);
        GUILayout.Space(10);

        DrawList(_ATList, GNewUI.AimEventType.AnimTransition);
        GUILayout.Space(10);

        //ShowMsg(sb.ToString());

        if (GUILayout.Button("Check", GUILayout.Width(100f), GUILayout.Height(40f)))
        {
            Check();
        }
        GUILayout.Space(5);

        serializedObject.ApplyModifiedProperties();
    }

    private bool[] showList = new bool[(int)GNewUI.AimEventType.Total];

    protected virtual void DrawList(SerializedProperty list, GNewUI.AimEventType evt_type)
    {
        ++EditorGUI.indentLevel;
        showList[(int)evt_type] = EditorGUILayout.Foldout(showList[(int)evt_type], evt_type.ToString() + (evt_type != AimEventType.AnimTransition ? " Events" : "s"));

        if (showList[(int)evt_type])
        {
            EditorGUILayout.BeginVertical("AS TextArea", GUILayout.MinHeight(10f));
            //GUILayout.Label(new GUIContent(evt_type.ToString() + " Events"));

            for (int i = 0; i < list.arraySize; ++i)
            {
                SerializedProperty spi = list.GetArrayElementAtIndex(i);
                if (spi != null)
                {
                    if (DrawItem(i, spi, evt_type))
                    {
                        list.DeleteArrayElementAtIndex(i);
                        break;
                    }
                    GUILayout.Space(2);
                }
            }

            //Add Button
            {
                GUILayout.Space(5);

                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(5);

                if (GUILayout.Button("+", GUILayout.Width(20)))
                {
                    _UIAnim.AddEventBlock(evt_type);
                    return;
                }
                GUILayout.Label(_ct_add);
                GUILayout.EndHorizontal();
                GUILayout.Space(5);
            }
            EditorGUILayout.EndVertical();
        }
        EditorGUI.indentLevel -= 1;
    }

    protected virtual bool DrawItem(int idx, SerializedProperty item, GNewUI.AimEventType evt_type)
    {
        ++EditorGUI.indentLevel;
        bool to_delete = false;

        GUILayout.Space(5);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("-", GUILayout.Width(20)))
        {
            to_delete = true;
        }

        EditorGUILayout.BeginVertical("Button", GUILayout.MinHeight(10f));
        GUILayout.Space(5);
        SerializedProperty sp_name = item.FindPropertyRelative("Name");
        //EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(sp_name, evt_type == AimEventType.AnimTransition ? _ct_to : _ct_name);
        //if (EditorGUI.EndChangeCheck())
        //{
        //    if (string.IsNullOrEmpty(sp_name.stringValue))
        //    {
        //        err_msg.AppendLine(string.Format("Name is empty, in {1} Events [{2}]", evt_type.ToString(), idx.ToString()));
        //    }
        //    else if (_UIAnim.IsEventBlockExisted(sp_name.stringValue, evt_type))
        //    {
        //        err_msg.AppendLine(string.Format("Name {0} is exist, in {1} Events [{2}]", sp_name.stringValue, evt_type.ToString(), idx.ToString()));
        //    }
        //}

        //GUILayout.BeginVertical();

        if (evt_type == GNewUI.AimEventType.Anim)
        {
            SerializedProperty sp_anim = item.FindPropertyRelative("Anim");
            SerializedProperty sp_animName = item.FindPropertyRelative("AnimName");
            SerializedProperty sp_crossFade = item.FindPropertyRelative("CrossFade");

            EditorGUILayout.PropertyField(sp_anim);
            EditorGUILayout.PropertyField(sp_animName);
            EditorGUILayout.PropertyField(sp_crossFade);
        }
        else if (evt_type == GNewUI.AimEventType.Sound)
        {
            SerializedProperty sp_sndPath = item.FindPropertyRelative("SndPath");
            SerializedProperty sp_node = item.FindPropertyRelative("Node");
            SerializedProperty sp_lifeTime = item.FindPropertyRelative("LifeTime");
            SerializedProperty sp_Is2D = item.FindPropertyRelative("Is2D");

            EditorGUILayout.PropertyField(sp_sndPath);
            EditorGUILayout.PropertyField(sp_node);
            EditorGUILayout.PropertyField(sp_lifeTime);
            EditorGUILayout.PropertyField(sp_Is2D);
        }
        else if (evt_type == GNewUI.AimEventType.Fx)
        {
            SerializedProperty sp_fxPath = item.FindPropertyRelative("FxPath");
            SerializedProperty sp_anchor = item.FindPropertyRelative("Anchor");
            SerializedProperty sp_hook = item.FindPropertyRelative("Hook");
            SerializedProperty sp_lifeTime = item.FindPropertyRelative("LifeTime");
            SerializedProperty sp_IsUI = item.FindPropertyRelative("IsUI");

            EditorGUILayout.PropertyField(sp_fxPath);
            EditorGUILayout.PropertyField(sp_anchor);
            EditorGUILayout.PropertyField(sp_hook);
            EditorGUILayout.PropertyField(sp_lifeTime);
            EditorGUILayout.PropertyField(sp_IsUI);
        }
        else if (evt_type == GNewUI.AimEventType.AnimTransition)
        {
            SerializedProperty sp_anim = item.FindPropertyRelative("Anim");
            EditorGUILayout.PropertyField(sp_anim);

            SerializedProperty sp_list_from = item.FindPropertyRelative("Froms");
            SerializedProperty sp_list_time = item.FindPropertyRelative("CrossFadeTimes");

            GUILayout.BeginVertical();

            for (int i = 0; i < sp_list_from.arraySize; ++i)
            {
                bool is_to_delete = false;

                ++EditorGUI.indentLevel;

                SerializedProperty spi = sp_list_from.GetArrayElementAtIndex(i);
                SerializedProperty spt = sp_list_time.GetArrayElementAtIndex(i);

                EditorGUILayout.BeginHorizontal(GUI.skin.textArea);

                if (GUILayout.Button("X", GUILayout.Width(20)))
                {
                    is_to_delete = true;
                }

                GUILayout.BeginVertical();
                //EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(spi, _ct_from);
                //if (EditorGUI.EndChangeCheck())
                //{
                //for (int k = 0; k < sp_list_from.Count; ++i)
                //{
                //    if (i == idx)
                //        continue;
                //    if (spi.stringValue == _linkBase.Links[i].Name)
                //    {
                //        err_msg.AppendLine(string.Format("AnimTransition {0}->{1} existed", item.name, spi.stringValue));
                //    }
                //}
                //}
                EditorGUILayout.PropertyField(spt, _ct_fade);
                GUILayout.EndVertical();

                GUILayout.EndHorizontal();

                EditorGUI.indentLevel -= 1;

                if (is_to_delete)
                {
                    sp_list_from.DeleteArrayElementAtIndex(i);
                    sp_list_time.DeleteArrayElementAtIndex(i);
                    break;
                }
            }
            GUILayout.EndVertical();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("+", GUILayout.Width(20)))
            {
                sp_list_from.InsertArrayElementAtIndex(sp_list_from.arraySize);
                sp_list_time.InsertArrayElementAtIndex(sp_list_time.arraySize);
            }
            GUILayout.Label(_ct_add);
            GUILayout.EndHorizontal();
        }

        //GUILayout.EndVertical();

        GUILayout.Space(5);
        EditorGUILayout.EndVertical();

        GUILayout.EndHorizontal();

        EditorGUI.indentLevel -= 1;

        return to_delete;
    }

    //bool DrawAnimTransistion(int idx, SerializedProperty item, System.Text.StringBuilder err_msg)
    //{
    //    ++EditorGUI.indentLevel;



    //    SerializedProperty sp_name = item.FindPropertyRelative("Name");
    //    EditorGUI.BeginChangeCheck();
    //    EditorGUILayout.PropertyField(sp_name, new GUIContent("Transist To"));
    //    if (EditorGUI.EndChangeCheck())
    //    {
    //        if (string.IsNullOrEmpty(sp_name.stringValue))
    //        {
    //            err_msg.AppendLine(string.Format("AnimTransition #{0} dont have a name. ", idx));
    //        }
    //    }



    //    EditorGUI.indentLevel -= 1;
    //}

    void Check()
    {
        System.Text.StringBuilder err_msg = new System.Text.StringBuilder();
        if (_UIAnim != null)
        {
            for (int i = 0; i < _UIAnim.AnimEvents.Count; i++)
            {
                CheckItem(i, _UIAnim.AnimEvents[i], GNewUI.AimEventType.Anim, ref err_msg);
            }
            for (int i = 0; i < _UIAnim.SoundEvents.Count; i++)
            {
                CheckItem(i, _UIAnim.SoundEvents[i], GNewUI.AimEventType.Sound, ref err_msg);
            }
            for (int i = 0; i < _UIAnim.FxEvents.Count; i++)
            {
                CheckItem(i, _UIAnim.FxEvents[i], GNewUI.AimEventType.Fx, ref err_msg);
            }
            for (int i = 0; i < _UIAnim.AnimTrans.Count; i++)
            {
                CheckItem(i, _UIAnim.AnimTrans[i], GNewUI.AimEventType.AnimTransition, ref err_msg);
            }
        }

        ShowMsg(err_msg.ToString());
    }

    void CheckItem(int idx, EventBlock item, GNewUI.AimEventType evt_type, ref System.Text.StringBuilder err_msg)
    {
        if (string.IsNullOrEmpty(item.Name))
        {
            err_msg.AppendFormat("A setting with no name appears in {0} [{1}] ", evt_type.ToString(), idx.ToString());
        }
        else if (_UIAnim.IsEventBlockExisted(item.Name, idx, evt_type))
        {
            err_msg.AppendFormat("Name {0} duplicated, in {1} [{2}] \n", item.Name, evt_type.ToString(), idx.ToString());
        }

        if (evt_type == AimEventType.Anim)
        {
            AnimEventBlock aeb = item as AnimEventBlock;
            if (aeb != null)
            {
                if (string.IsNullOrEmpty(aeb.AnimName))
                {
                    err_msg.AppendFormat("Anim Name not set in {0} : {1} \n", evt_type.ToString(), aeb.Name);
                }
                if (aeb.Anim == null)
                {
                    err_msg.AppendFormat("Animation not set in {0} : {1} \n", evt_type.ToString(), aeb.Name);
                }
            }
        }
        if (evt_type == AimEventType.Sound)
        {
            SoundEventBlock seb = item as SoundEventBlock;
            if (seb != null)
            {
                if (string.IsNullOrEmpty(seb.SndPath))
                {
                    err_msg.AppendFormat("Sound Path not set in {0} : {1} \n", evt_type.ToString(), seb.Name);
                }
                if (seb.Node == null)
                {
                    err_msg.AppendFormat("Sound Node not set in {0} : {1} \n", evt_type.ToString(), seb.Name);
                }
            }
        }
        else if (evt_type == AimEventType.Fx)
        {
            FxEventBlock fb = item as FxEventBlock;
            if (fb != null)
            {
                if (string.IsNullOrEmpty(fb.FxPath))
                {
                    err_msg.AppendFormat("Fx Path not set in {0} : {1} \n", evt_type.ToString(), fb.Name);
                }
                if (fb.Hook == null)
                {
                    err_msg.AppendFormat("Fx Hook not set in {0} : {1} \n", evt_type.ToString(), fb.Name);
                }
            }
        }
        else if (evt_type == AimEventType.AnimTransition)
        {
            AnimTransition at = item as AnimTransition;
            if (at.Froms.Length != at.CrossFadeTimes.Length)
            {
                err_msg.AppendFormat("AnimTransition data error ->{0} \n", item.Name);
            }
            if (at != null)
            {
                for (int i = 0; i < at.Froms.Length; i++)
                {
                    if (string.IsNullOrEmpty(at.Froms[i]))
                    {
                        err_msg.AppendFormat("Incomplete AnimTransition appears ? ->{0} \n", item.Name);
                        continue;
                    }

                    for (int k = i + 1; k < at.Froms.Length; k++)
                    {
                        if (at.Froms[k] == at.Froms[i])
                        {
                            err_msg.AppendFormat("AnimTransition {0}->{1} duplicated \n", at.Froms[k], item.Name);
                            break;
                        }
                    }
                }
            }
        }
    }

    private void ShowMsg(string msg)
    {
        if (msg == string.Empty)
        {
            check_msg = "Every thing is fine";
            msg_type = MessageType.Info;
        }
        else
        {
            check_msg = msg;
            msg_type = MessageType.Warning;
        }
    }
}