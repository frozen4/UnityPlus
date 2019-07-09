using UnityEngine;
using UnityEditor;
using System.Text;

[CustomEditor(typeof(GUILinkHolder))]
public class GUILinkHolderInspector : Editor
{
    MessageType msg_type;
    string check_msg = "Every thing is fine";
    public override void OnInspectorGUI()
    {
        SerializedObject so = new SerializedObject(target);

        SerializedProperty sp = so.FindProperty("Links");

        //bool t = EditorGUILayout.PropertyField(sp);
        //
        //if (t)
        EditorGUILayout.HelpBox(check_msg, msg_type);
        {
            ++EditorGUI.indentLevel;

            for (int i = 0; i < sp.arraySize; ++i)
            {
                SerializedProperty spi = sp.GetArrayElementAtIndex(i);

                if (DrawItem(i, spi))
                {
                    sp.DeleteArrayElementAtIndex(i);
                    break;
                }
            }

            --EditorGUI.indentLevel;
        }

        if (GUILayout.Button("Add One"))
        {
            GUILinkHolder link = target as GUILinkHolder;
            link.Links.Add(new GUILinkHolder.LinkItem());
        }
        GUILayout.Space(5);
        if (GUILayout.Button("Check"))
        {
            Check();
        }

        so.ApplyModifiedProperties();
    }

    bool DrawItem(int idx, SerializedProperty item)
    {
        ++EditorGUI.indentLevel;

        bool to_delete = false;

        EditorGUILayout.BeginVertical("AS TextArea", GUILayout.MinHeight(10f));

        EditorGUILayout.BeginHorizontal(GUI.skin.textArea);

        if (GUILayout.Button("X", GUILayout.Width(20)))
        {
            to_delete = true;
        }

        GUILayout.Label(new GUIContent(string.Format("Item {0}", idx)));

        GUILayout.EndHorizontal();



        GUILayout.BeginVertical();

        SerializedProperty sp_id = item.FindPropertyRelative("Name");
        SerializedProperty sp_ctrl = item.FindPropertyRelative("UIObject");

        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(sp_id);
        if (EditorGUI.EndChangeCheck())
        {
            var link = target as GUILinkHolder;

            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < link.Links.Count; ++i)
            {
                if (i == idx)
                    continue;

                if (sp_id.stringValue == link.Links[i].Name)
                {
                    sb.AppendLine(string.Format("Name {0} is exist, in item({1})", sp_id.stringValue, i));
                }
            }

            SendMsg(sb.ToString());
        }

        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(sp_ctrl);
        if (EditorGUI.EndChangeCheck())
        {
            if (sp_ctrl.objectReferenceValue != null)
            {
                var link = target as GUILinkHolder;

                StringBuilder sb = new StringBuilder();

                for (int i = 0; i < link.Links.Count; ++i)
                {
                    if (i == idx)
                        continue;

                    if (sp_ctrl.objectReferenceValue == link.Links[i].UIObject)
                    {
                        sb.AppendLine(string.Format("UIObject {0} is exist, in item({1})", sp_ctrl.objectReferenceValue.name, i));
                    }
                }

                SendMsg(sb.ToString());
            }

            if (sp_ctrl.objectReferenceValue != null && sp_id.stringValue == string.Empty)
            {
                RectTransform rt = sp_ctrl.objectReferenceValue as RectTransform;
                sp_id.stringValue = rt.name;
            }
        }

        GUILayout.EndVertical();

        EditorGUILayout.EndVertical();

        --EditorGUI.indentLevel;

        return to_delete;
    }

    void Check()
    {
        var link = target as GUILinkHolder;

        StringBuilder sb = new StringBuilder();

        for (int i = 0; i < link.Links.Count; ++i)
        {
            GUILinkHolder.LinkItem item = link.Links[i];
            if (item.UIObject == null)
            {
                sb.AppendLine(string.Format("Item {0} ui ctrl is missing", i));
            }

            if (item.Name == string.Empty)
            {
                sb.AppendLine(string.Format("Item {0} ui ctrl name is empty", i));
            }
        }

        SendMsg(sb.ToString());
    }

    void SendMsg(string msg)
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
