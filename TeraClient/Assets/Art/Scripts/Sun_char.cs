using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class Sun_char : MonoBehaviour
{
    /***
     * 用途：
     * 创建/选择角色阶段使用的效果脚本，根据人物朝向调整全局光照方向
     */

    public Color SunColorchar = Color.white;
    [Range(0, 8)]
    public float SunColorcharIntensity = 1;

    // public static implicit operator Color(Vector4 v);
    private Vector4 _Color = Vector4.zero;

    public void Update()
    {
        Shader.SetGlobalVector(ShaderIDs.SunDirchar, -transform.forward);
        _Color.x = SunColorchar.r;
        _Color.y = SunColorchar.g;
        _Color.z = SunColorchar.b;
        _Color.w = SunColorcharIntensity;
        Shader.SetGlobalColor(ShaderIDs.SunColorchar, _Color);
    }
}