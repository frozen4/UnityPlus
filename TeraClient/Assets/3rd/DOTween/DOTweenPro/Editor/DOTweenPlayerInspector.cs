//using System;
//using System.Collections.Generic;
//using System.IO;
using DG.DemiEditor;
using DG.DOTweenEditor.Core;
using DG.Tweening;
using DG.Tweening.Core;
using UnityEditor;
using UnityEngine;
//using UnityEditorInternal;
//using UnityEngine.UI;

using System.Collections.Generic;

namespace DG.DOTweenEditor
{
    [CustomEditor(typeof(DOTweenPlayer))]
    public class DOTweenPlayerInspector : Editor
    {
        private DOTweenPlayer _src;
        private void OnEnable()
        {
            this._src = (base.target as DOTweenPlayer);
            if (Application.isPlaying)
            {
                return;
            }
            //MonoBehaviour[] components = this._src.GetComponents<MonoBehaviour>();
            //int num = ArrayUtility.IndexOf<MonoBehaviour>(components, this._src);
            //int i = 0;
            //for (int j = 0; j < num; j++)
            //{
            //    if (components[j] is ABSAnimationComponent)
            //    {
            //        i++;
            //    }
            //}
            //while (i > 0)
            //{
            //    i--;
            //    ComponentUtility.MoveComponentUp(this._src);
            //}
        }

        //Vector2 srcPos;

        public override void OnInspectorGUI()
        {
            EditorGUIUtils.SetGUIStyles(null);
            EditorGUIUtility.labelWidth = 80f;
            EditorGUIUtils.InspectorLogo();
            GUILayout.Label("Custom Player", EditorGUIUtils.sideLogoIconBoldLabelStyle);

            //VisualManagerPreset preset = this._src.preset;
            //this._src.preset = (VisualManagerPreset)EditorGUILayout.EnumPopup("Preset", this._src.preset, new GUILayoutOption[0]);
            //if (preset != this._src.preset)
            //{
            //    VisualManagerPreset preset2 = this._src.preset;
            //    if (preset2 == VisualManagerPreset.PoolingSystem)
            //    {
            //        this._src.onEnableBehaviour = OnEnableBehaviour.RestartFromSpawnPoint;
            //        this._src.onDisableBehaviour = OnDisableBehaviour.Rewind;
            //    }
            //}

            if (GUILayout.Button("Register"))
            {
                if (Application.isPlaying)
                {
                    return;
                }
                RegisterAll(_src, false);
            }

            if (GUILayout.Button("Register Children"))
            {
                if (Application.isPlaying)
                {
                    return;
                }
                RegisterAll(_src, true);
            }

            GUILayout.Space(6f);
            int to_delete = -1;
            if (_src.AllGroups != null && _src.AllGroups.Length > 0)
            {
                //srcPos = GUILayout.BeginScrollView(srcPos);
                GUILayout.BeginVertical();

                for (int i = 0; i < _src.AllGroups.Length; i++)
                {
                    //GUILayout.BeginArea
                    DOTweenPlayer.DOTGroup dg_item = _src.AllGroups[i];

                    GUILayout.Space(6f);

                    GUILayout.BeginVertical("Button");

                    EditorGUILayout.LabelField("Group ID : " + dg_item.ID);
                    EditorGUILayout.LabelField("Count : " + dg_item.AllComps.Length);

                    OnEnableBehaviour_My onEnableBehaviour = dg_item.onEnableBehaviour;
                    OnDisableBehaviour_My onDisableBehaviour = dg_item.onDisableBehaviour;

                    if (GUILayout.Button("X", GUILayout.Width(20)))
                    {
                        to_delete = i;
                    }
                    dg_item.onEnableBehaviour = (OnEnableBehaviour_My)EditorGUILayout.EnumPopup(new GUIContent("On Enable", "Eventual actions to perform when this gameObject is activated"),
                        dg_item.onEnableBehaviour, new GUILayoutOption[0]);
                    dg_item.onDisableBehaviour = (OnDisableBehaviour_My)EditorGUILayout.EnumPopup(new GUIContent("On Disable", "Eventual actions to perform when this gameObject is deactivated"),
                        dg_item.onDisableBehaviour, new GUILayoutOption[0]);

                    GUILayout.Space(6f);

                    if (!Application.isPlaying)
                    {
                        GUILayout.BeginHorizontal();

                        //bool b_autoPlay = false;
                        //bool b_autoKill = false;

                        //for (int k = 0; k < dg_item.AllComps.Length; k++)
                        //{
                        //    if (dg_item.AllComps[k].autoPlay)
                        //    {
                        //        b_autoPlay = true;
                        //        break;
                        //    }
                        //}

                        //for (int k = 0; k < dg_item.AllComps.Length; k++)
                        //{
                        //    if (dg_item.AllComps[k].autoKill)
                        //    {
                        //        b_autoKill = true;
                        //        break;
                        //    }
                        //}

                        //EditorGUI.BeginChangeCheck();
                        //b_autoPlay = DeGUILayout.ToggleButton(b_autoPlay, new GUIContent("AutoPlay", "If selected, the tween will play automatically"));
                        //if (EditorGUI.EndChangeCheck())
                        //{
                        //    ToggleAutoPlay(_src, i, b_autoPlay);
                        //}

                        //EditorGUI.BeginChangeCheck();
                        //b_autoKill = DeGUILayout.ToggleButton(b_autoKill, new GUIContent("AutoKill", "If selected, the tween will be killed when it completes, and won't be reusable"));
                        //if (EditorGUI.EndChangeCheck())
                        //{
                        //    ToggleAutoPlay(_src, i, b_autoPlay);
                        //}


                        if (GUILayout.Button("AutoPlay"))
                        {
                            ToggleAutoPlay(_src, i, true);
                        }
                        if (GUILayout.Button("X AutoPlay"))
                        {
                            ToggleAutoPlay(_src, i, false);
                        }
                        if (GUILayout.Button("AutoKill"))
                        {
                            ToggleAutoKill(_src, i, true);
                        }
                        if (GUILayout.Button("X AutoKill"))
                        {
                            ToggleAutoKill(_src, i, false);
                        }

                        GUILayout.EndHorizontal();
                    }
                    else
                    {
                        GUILayout.BeginHorizontal();
                        if (GUILayout.Button("Play"))
                        {
                            _src.Play(dg_item.ID);
                        }
                        if (GUILayout.Button("Pause"))
                        {
                            _src.Pause(dg_item.ID);
                        }
                        if (GUILayout.Button("Rewind"))
                        {
                            _src.Rewind(dg_item.ID);
                        }
                        if (GUILayout.Button("Restart"))
                        {
                            _src.Restart(dg_item.ID);
                        }
                        GUILayout.EndHorizontal();
                    }
                    GUILayout.EndVertical();

                    if (to_delete!=-1)
                    {
                        DeleteGroup(this._src, to_delete);
                        to_delete = -1;
                        break;
                    }
                }
                //GUILayout.EndScrollView();
                GUILayout.EndVertical();
            }

            if (GUI.changed)
            {
                EditorUtility.SetDirty(this._src);
            }
        }

//#if UNITY_EDITOR
        public void RegisterAll(DOTweenPlayer dtp_player, bool b_children)
        {
            //Undo.RegisterCompleteObjectUndo(dtp_player, "DTPlayer");
            Undo.RecordObject(dtp_player, "DTPlayer");

            //dtp_player.AllComps = null;

            DOTweenAnimation[] a_dtas = b_children ?
                dtp_player.GetComponentsInChildren<DOTweenAnimation>(true) : dtp_player.GetComponents<DOTweenAnimation>();

            Dictionary<string, List<DOTweenAnimation>> dic_dta = new Dictionary<string, List<DOTweenAnimation>>();
            List<string> l_keys = new List<string>();

            for (int i = 0; i < a_dtas.Length; i++)
            {
                string s_key = a_dtas[i].id;
                List<DOTweenAnimation> l_dta = null;

                if (!dic_dta.TryGetValue(s_key, out l_dta))
                {
                    l_keys.Add(s_key);
                    l_dta = new List<DOTweenAnimation>();
                    dic_dta[s_key] = l_dta;
                }

                l_dta.Add(a_dtas[i]);
            }

            DOTweenPlayer.DOTGroup[] old_grp = dtp_player.AllGroups;

            dtp_player.AllGroups = new DOTweenPlayer.DOTGroup[l_keys.Count];

            for (int i = 0; i < l_keys.Count; i++)
            {
                dtp_player.AllGroups[i] = new DOTweenPlayer.DOTGroup(l_keys[i], dic_dta[l_keys[i]]);

                //Copy old group settings
                for (int k = 0; k < old_grp.Length; k++)
                {
                    if (old_grp[k].ID == l_keys[i])
                    {
                        dtp_player.AllGroups[i].onEnableBehaviour = old_grp[k].onEnableBehaviour;
                        dtp_player.AllGroups[i].onDisableBehaviour = old_grp[k].onDisableBehaviour;
                        break;
                    }
                }
            }

            DOTweenVisualManager[] a_dtvs = b_children ?
                dtp_player.GetComponentsInChildren<DOTweenVisualManager>(true) : dtp_player.GetComponents<DOTweenVisualManager>();

            for (int i = 0; i < a_dtvs.Length; i++)
            {
                DestroyImmediate(a_dtvs[i]);
            }
        }

        public void DeleteGroup(DOTweenPlayer dtp_player, int i_pos)
        {
            if (_src.AllGroups != null && _src.AllGroups.Length > i_pos)
            {
                DOTweenPlayer.DOTGroup[] new_list = new DOTweenPlayer.DOTGroup[_src.AllGroups.Length - 1];
                for (int i = 0, k = 0; i < _src.AllGroups.Length; i++)
                {
                    if (i != i_pos)
                    {
                        new_list[k] = _src.AllGroups[i];
                        k += 1;
                    }
                }
                _src.AllGroups = new_list;
            }
        }

        public void ToggleAutoKill(DOTweenPlayer dtp_player, int i_pos, bool b_kill)
        {
            for (int k = 0; k < dtp_player.AllGroups[i_pos].AllComps.Length; k++)
            {
                //dtp_player.AllGroups[i_pos].AllComps[k].autoKill = b_kill;        //This shows a wrong way to go
                SerializedObject so = new SerializedObject(dtp_player.AllGroups[i_pos].AllComps[k]);
                SerializedProperty sp = so.FindProperty("autoKill");
                sp.boolValue = b_kill;
                so.ApplyModifiedProperties();
            }
        }

        public void ToggleAutoPlay(DOTweenPlayer dtp_player, int i_pos, bool b_autoPlay)
        {
            for (int k = 0; k < dtp_player.AllGroups[i_pos].AllComps.Length; k++)
            {
                //dtp_player.AllGroups[i_pos].AllComps[k].autoPlay = b_autoPlay;        //This shows a wrong way to go
                SerializedObject so = new SerializedObject(dtp_player.AllGroups[i_pos].AllComps[k]);
                SerializedProperty sp = so.FindProperty("autoPlay");
                sp.boolValue = b_autoPlay;
                so.ApplyModifiedProperties();
            }
        }

//#endif
    }
}