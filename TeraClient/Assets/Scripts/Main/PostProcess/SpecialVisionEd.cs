#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PostProcessChain))]
public class SpecialVisionEd : Editor
{
    delegate bool DrawHead();
    delegate void DrawBody();
    public override void OnInspectorGUI()
    {

        //base.OnInspectorGUI();
        PostProcessChain sv = target as PostProcessChain;
        // brightness & contrast
        //if (sv.Brightness_Contrast != null)
        {
            DrawItem(
                () =>
                {
                    EditorGUILayout.LabelField("# Motion Blur");
                    sv.EnableMotionBlur = EditorGUILayout.Toggle(" ", sv.EnableMotionBlur);
                    return sv.EnableMotionBlur;
                },
                () =>
                {
                    EditorGUI.BeginDisabledGroup(true);
                    sv.MotionBlurParamter = EditorGUILayout.Vector4Field("Mix", sv.MotionBlurParamter);
                    EditorGUI.EndDisabledGroup();
                });
        }
        {
            DrawItem(
                () =>
                {
                    EditorGUILayout.LabelField("# Fog");
                    sv.EnableFog = EditorGUILayout.Toggle(" ", sv.EnableFog);
                    return sv.EnableFog;
                },
                () =>
                {
                    sv.fog_color = EditorGUILayout.ColorField("Fog Color", sv.fog_color);
                    sv.fog_paramter.x = EditorGUILayout.FloatField("Distance Fog Begin", sv.fog_paramter.x);
                    sv.fog_paramter.y = EditorGUILayout.FloatField("Distance Fog Length", sv.fog_paramter.y);
                    sv.fog_paramter.z = EditorGUILayout.FloatField("Height Fog Begin", sv.fog_paramter.z);
                    sv.fog_paramter.w = EditorGUILayout.FloatField("Height Fog Length", sv.fog_paramter.w);
                });
        }
        {
            DrawItem(
                () =>
                {
                    EditorGUILayout.LabelField("# Depth Of Field");
                    sv.EnableDepthOfField = EditorGUILayout.Toggle(" ", sv.EnableDepthOfField);
                    return sv.EnableDepthOfField;
                },
                () =>
                {
                    sv.depthoffield_paramter.x = EditorGUILayout.FloatField("Distance", sv.depthoffield_paramter.x);
                    sv.depthoffield_paramter.y = EditorGUILayout.FloatField("Range", sv.depthoffield_paramter.y);
					sv.depthoffield_paramter.z = EditorGUILayout.FloatField("Bokeh", sv.depthoffield_paramter.z);
                });
        }
        {
            DrawItem(
                () =>
                {
                    EditorGUILayout.LabelField("# HSV");
                    sv.EnableHsvAdjust = EditorGUILayout.Toggle(" ", sv.EnableHsvAdjust);
                    return sv.EnableHsvAdjust;
                },
                () =>
                {
                    sv._hsv_adjust_paramters.x = EditorGUILayout.FloatField("Hue [0, 360]", sv._hsv_adjust_paramters.x);
                    sv._hsv_adjust_paramters.y = EditorGUILayout.FloatField("Saturation [0, 1]", sv._hsv_adjust_paramters.y);
                    sv._hsv_adjust_paramters.z = EditorGUILayout.FloatField("Value [0, 1]", sv._hsv_adjust_paramters.z);
                });
        }
        {
            DrawItem(
                () =>
                {
                    EditorGUI.BeginDisabledGroup(true);
                    EditorGUILayout.LabelField("# Brightness & Contrast");
                    EditorGUI.EndDisabledGroup();

                    return sv.EnableHsvAdjust;
                },
                () =>
                {
                    //EditorGUI.BeginDisabledGroup(true);

                    sv.brightness_contrast_paramters.x = EditorGUILayout.FloatField("Brightness Factor [0, 2]", sv.brightness_contrast_paramters.x);

                    //EditorGUI.EndDisabledGroup();

                    sv.brightness_contrast_paramters.y = EditorGUILayout.FloatField("Contrast Factor [0, 2]", sv.brightness_contrast_paramters.y);
                    sv.brightness_contrast_paramters.z = EditorGUILayout.FloatField("Average Brightness [0, 1]", sv.brightness_contrast_paramters.z);
                });
        }
        // bloom
        //if (sv.Bloom != null)
        {
            DrawItem(
                () =>
                {
                    EditorGUILayout.LabelField("# BloomHD");
					EditorGUI.BeginDisabledGroup(true);
                    sv.EnableBloomHD = EditorGUILayout.Toggle(" ", sv.EnableBloomHD);
					EditorGUI.EndDisabledGroup();
                    return sv.EnableBloomHD;
                },
                () =>
                {
                    //sv.bloom_paramters.x = EditorGUILayout.FloatField("Brightness Threshold [0, 1]", sv.bloom_paramters.x);
                    //sv.bloom_paramters.y = EditorGUILayout.FloatField("Bloom Blend Factor [0, 1]", sv.bloom_paramters.y);
                });
        }
        {
            DrawItem(
                () =>
                {
                    EditorGUILayout.LabelField("# Simple Radial Blur");
                    sv.EnableRadialBlur = EditorGUILayout.Toggle(" ", sv.EnableRadialBlur);
                    return sv.EnableRadialBlur;
                },
                () =>
                {
                    sv.radial_blur_pos = EditorGUILayout.Vector2Field("Pos", sv.radial_blur_pos);
                    sv.radial_blur_paramters.x = EditorGUILayout.FloatField("P1", sv.radial_blur_paramters.x);
                    sv.radial_blur_paramters.y = EditorGUILayout.FloatField("P2", sv.radial_blur_paramters.y);
                });
        }
        // special vision
        //if (sv.SpecialVision != null)
        {
            DrawItem(
                () =>
                {
                    EditorGUILayout.LabelField("# Special Vision");
                    sv.EnableSpecialVision = EditorGUILayout.Toggle(" ", sv.EnableSpecialVision);
                    return sv.EnableSpecialVision;
                },
                () =>
                {
                    sv.vision_color = EditorGUILayout.ColorField("Special Vision Color (RGB)", sv.vision_color);
                    sv.special_vision_paramters.x = EditorGUILayout.FloatField("Fuzzy Factor", sv.special_vision_paramters.x);
                    sv.special_vision_paramters.y = EditorGUILayout.FloatField("Brightness Factor [0, 2]", sv.special_vision_paramters.y);
                });
        }
    }

    void DrawItem(DrawHead draw_head, DrawBody draw_body)
    {
        EditorGUILayout.BeginVertical(GUI.skin.textField);
        EditorGUILayout.BeginHorizontal(GUI.skin.button);

        bool enable_body_edit = draw_head();

        EditorGUILayout.EndHorizontal();
        EnterSection(enable_body_edit);

        draw_body();

        ExitSection();
        EditorGUILayout.EndVertical();
    }

    void EnterSection(bool enable)
    {
        EditorGUI.BeginDisabledGroup(!enable);
        EditorGUILayout.BeginVertical(GUI.skin.box);
        ++EditorGUI.indentLevel;
    }

    void ExitSection()
    {
        --EditorGUI.indentLevel;
        EditorGUILayout.EndVertical();
        EditorGUI.EndDisabledGroup();
    }
}

#endif