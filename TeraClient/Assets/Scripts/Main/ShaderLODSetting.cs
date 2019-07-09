using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public enum EShaderLevel
{
	Low = 0,
	Normal,
	High
}

public class ShaderLODSetting : MonoBehaviour
{
	private EShaderLevel _shader_level;
	private int[] _shader_level_numberic = new int[]{ 200, 400, 600 };

	public EShaderLevel ShaderLevel {
		get {
			return _shader_level;
		}
		set {
			if (_shader_level != value) {
				_shader_level = value;

				if (_shader_level == EShaderLevel.Low) {
					Shader.globalMaximumLOD = 200;
				} else if (_shader_level == EShaderLevel.Normal) {
					Shader.globalMaximumLOD = 400;
				} else if (_shader_level == EShaderLevel.High) {
					Shader.globalMaximumLOD = 600;
				}
			}
		}
	}

	void Start ()
	{
		ShaderLevel = EShaderLevel.High;
	}

#if UNITY_EDITOR
	[ContextMenu ("SetLOD_Low")]
	void SetLOD_Low ()
	{
		ShaderLevel = EShaderLevel.Low;
	}

	[ContextMenu ("SetLOD_Normal")]
	void SetLOD_Normal ()
	{
		ShaderLevel = EShaderLevel.Normal;
	}

	[ContextMenu ("SetLOD_High")]
	void SetLOD_High ()
	{
		ShaderLevel = EShaderLevel.High;
	}
#endif
}

#if UNITY_EDITOR
[CustomEditor (typeof(ShaderLODSetting))]
public class ShaderLODSettingEd : Editor
{
	public override void OnInspectorGUI ()
	{
		ShaderLODSetting t = target as ShaderLODSetting;
		EditorGUILayout.EnumPopup ("LOD", t.ShaderLevel);
	}
}
#endif