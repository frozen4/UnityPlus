using UnityEngine;
using System.Collections.Generic;

[AddComponentMenu("AQUAS/Reflection")]
[ExecuteInEditMode] // Make mirror live-update even when not in play mode
public class AQUAS_Reflection : MonoBehaviour
{
    #region Variables
    public int m_TextureSize = 256;
    public float m_ClipPlaneOffset = 0.07f;

    public LayerMask m_ReflectLayers;

    private Dictionary<Camera, Camera> m_ReflectionCameras = new Dictionary<Camera, Camera>(); // Camera -> Camera table
    private Dictionary<Camera, Skybox> m_CameraSkyBoxs = new Dictionary<Camera, Skybox>();

    private RenderTexture m_ReflectionTexture = null;
    private int m_OldReflectionTextureSize = 0;

    private static bool s_InsideRendering = false;
    //private static Matrix4x4 s_scaleOffset = Matrix4x4.TRS(new Vector3(0.5f, 0.5f, 0.5f), Quaternion.identity, new Vector3(0.5f, 0.5f, 0.5f));

    private Renderer m_RendererComponent = null;

    private Camera m_CurrentCamera;
    private Camera m_reflectionCamera;
    //private Transform m_reflectionCameraTrans;

    void Start()
    {
        //m_camCullDistances = new float[32];
        //m_camCullDistances[CUnityUtil.Layer_Terrain] = 100;
        //m_camCullDistances[CUnityUtil.Layer_Building] = 50;

        m_ReflectLayers = CUnityUtil.LayerMaskBackground;
    }

    public Renderer RendererComponent
    {
        get
        {
            if (m_RendererComponent == null)
                m_RendererComponent = GetComponent<Renderer>();
            return m_RendererComponent;
        }
    }

    public void OnWillRenderObject()
    {
        if (!enabled)
            return;

        Material[] materials = null;
        if (m_RendererComponent == null)
            m_RendererComponent = GetComponent<Renderer>();
        if (m_RendererComponent != null)
            materials = m_RendererComponent.sharedMaterials;

        if (m_RendererComponent == null || materials == null || !m_RendererComponent.enabled)
            return;

        Camera cam = Camera.current;
        if (!cam)
            return;

        if (cam == Main.Main3DCamera)
        {
            // Safeguard from recursive reflections.        
            if (s_InsideRendering)
                return;
            s_InsideRendering = true;

            if (cam != m_CurrentCamera)
            {
                m_CurrentCamera = cam;
            }

            Camera reflectionCamera;
            CreateMirrorObjects(cam, out reflectionCamera);

            if (reflectionCamera != m_reflectionCamera)
            {
                m_reflectionCamera = reflectionCamera;
                //m_reflectionCameraTrans = reflectionCamera.transform;
            }

            // find out the reflection plane: position and normal in world space
            Vector3 pos = transform.position;
            Vector3 normal = transform.up;

            UpdateCameraModes(cam, reflectionCamera);

            // Render reflection
            // Reflect camera around reflection plane
            float d = -Vector3.Dot(normal, pos) - m_ClipPlaneOffset;
            Vector4 reflectionPlane = new Vector4(normal.x, normal.y, normal.z, d);

            Matrix4x4 reflection = Matrix4x4.zero;
            CalculateReflectionMatrix(ref reflection, reflectionPlane);
            //Vector3 oldpos = m_CurrentCameraTrans.position;
            //Vector3 newpos = reflection.MultiplyPoint( oldpos );
            reflectionCamera.worldToCameraMatrix = cam.worldToCameraMatrix * reflection;

            // Setup oblique projection matrix so that near plane is our reflection
            // plane. This way we clip everything below/above it for free.
            Vector4 clipPlane = CameraSpacePlane(reflectionCamera, pos, normal, 1.0f);
            Matrix4x4 projection = cam.projectionMatrix;
            CalculateObliqueMatrix(ref projection, clipPlane);
            reflectionCamera.projectionMatrix = projection;

            //reflectionCamera.layerCullDistances = m_camCullDistances;
            reflectionCamera.cullingMask = ~(1 << 4) & m_ReflectLayers.value; // never render water layer
            reflectionCamera.targetTexture = m_ReflectionTexture;

            GL.invertCulling = true;        //should be used but inverts the faces of the terrain

            reflectionCamera.Render();

            GL.invertCulling = false;        //should be used but inverts the faces of the terrain
            
            
            // Set matrix on the shader that transforms UVs from object space into screen
            // space. We want to just project reflection texture on screen.
            //Matrix4x4 scaleOffset = Matrix4x4.TRS(
            //	new Vector3(0.5f,0.5f,0.5f), Quaternion.identity, new Vector3(0.5f,0.5f,0.5f) );

            //Vector3 scale = transform.lossyScale;
            //Matrix4x4 mtx = transform.localToWorldMatrix * Matrix4x4.Scale(new Vector3(1.0f / scale.x, 1.0f / scale.y, 1.0f / scale.z));
            //mtx = s_scaleOffset * cam.projectionMatrix * cam.worldToCameraMatrix * mtx;

            for (var i = 0; i < materials.Length; i++)
            {
                var mat = materials[i];
                if (mat.HasProperty(ShaderIDs.ReflectionTex))
                {
                    mat.SetTexture(ShaderIDs.ReflectionTex, m_ReflectionTexture);
                }
                //mat.SetMatrix("_ProjMatrix", mtx);
            }
            
            s_InsideRendering = false;
        }
    }
    #endregion

    public void SetFullReflection(bool bReflection)
    {
        Material[] materials = null;
        if (m_RendererComponent == null)
            m_RendererComponent = GetComponent<Renderer>();
        if (m_RendererComponent != null)
            materials = m_RendererComponent.sharedMaterials;
        
        if (m_RendererComponent == null || materials == null)
            return;

        for (int i = 0; i < materials.Length; i++)
        {
            materials[i].SetInt(ShaderIDs.AquasUseReflections, bReflection ? 1 : 0);
        }

        this.enabled = bReflection;                 //保证效果
    }

    private void Clear()
    {
        var it = m_ReflectionCameras.GetEnumerator();
        while (it.MoveNext())
        {
            var reflectionCamera = it.Current.Value;
            if (reflectionCamera != null)
            {
                reflectionCamera.targetTexture = null;
                Destroy(reflectionCamera.gameObject);
            }
        }
        it.Dispose();
        m_ReflectionCameras.Clear();
        UpdateCameraSkyboxs();

        if (m_ReflectionTexture)
        {
            Destroy(m_ReflectionTexture);
            m_ReflectionTexture = null;
        }
        m_reflectionCamera = null;
        m_CurrentCamera = null;

        m_CameraSkyBoxs.Clear();
        m_RendererComponent = null;
    }
    //<summary>
    // Cleans up all the objects that were possibly created
    //</summary>
    void OnDisable()
    {
        Clear();
    }

    private void UpdateCameraModes(Camera src, Camera dest)
    {
        if (dest == null)
            return;

        //sets camera to clear the same way as current camera
        dest.clearFlags = src.clearFlags;
        dest.backgroundColor = src.backgroundColor;

        if (src.clearFlags == CameraClearFlags.Skybox)
        {
            Skybox sky = null;
            Skybox mysky = null;

            if (!m_CameraSkyBoxs.TryGetValue(src, out sky))
                sky = src.GetComponent<Skybox>();

            if (!m_CameraSkyBoxs.TryGetValue(dest, out mysky))
                mysky = dest.GetComponent<Skybox>();

            if (!sky || !sky.material)
            {
                mysky.enabled = false;
            }
            else
            {
                mysky.enabled = true;
                mysky.material = sky.material;
            }
        }

        ///<summary>
        ///Updates other values to match current camera.
        ///Even if camera&projection matrices are supplied, some of values are used elsewhere (e.g. skybox uses far plane)
        /// </summary>
        dest.farClipPlane = src.farClipPlane;
        dest.nearClipPlane = src.nearClipPlane;
        dest.orthographic = src.orthographic;
        dest.fieldOfView = src.fieldOfView;
        dest.aspect = src.aspect;
        dest.orthographicSize = src.orthographicSize;
    }

    //<summary>
    //Creates any objects needed on demand
    //</summary>
    private void CreateMirrorObjects(Camera currentCamera, out Camera reflectionCamera)
    {
        reflectionCamera = null;

        //Reflection render texture
        if (!m_ReflectionTexture || m_OldReflectionTextureSize != m_TextureSize)
        {
            if (m_ReflectionTexture)
                Destroy(m_ReflectionTexture);
            m_ReflectionTexture = new RenderTexture(m_TextureSize, m_TextureSize, 16);
            m_ReflectionTexture.name = HobaText.Format("__MirrorReflection{0}", GetInstanceID());
            m_ReflectionTexture.isPowerOfTwo = true;
            m_ReflectionTexture.hideFlags = HideFlags.DontSave;
            m_OldReflectionTextureSize = m_TextureSize;
        }

        //Camera for reflection
        if (!m_ReflectionCameras.TryGetValue(currentCamera, out reflectionCamera)) // catch both not-in-dictionary and in-dictionary-but-deleted-GO
        {
            GameObject go = new GameObject(
                HobaText.Format("Mirror Refl Camera id{0} for {1}", GetInstanceID(), currentCamera.GetInstanceID()),
                typeof(Camera), typeof(Skybox));
            reflectionCamera = go.GetComponent<Camera>();
            reflectionCamera.enabled = false;
            reflectionCamera.transform.position = transform.position;
            reflectionCamera.transform.rotation = transform.rotation;
            reflectionCamera.gameObject.AddComponent<FlareLayer>();
            go.hideFlags = HideFlags.HideAndDontSave;
            m_ReflectionCameras.Add(currentCamera, reflectionCamera);

            UpdateCameraSkyboxs();
        }
    }

    //<summary>
    //Extended sign: returns -1, 0 or 1 based on sign of a
    //</summary>
    private static float sgn(float a)
    {
        if (a > 0.0f) return 1.0f;
        if (a < 0.0f) return -1.0f;
        return 0.0f;
    }

    //<summary>
    //Given position/normal of the plane, calculates plane in camera space.
    //</summary>
    private Vector4 CameraSpacePlane(Camera cam, Vector3 pos, Vector3 normal, float sideSign)
    {
        Vector3 offsetPos = pos + normal * m_ClipPlaneOffset;
        Matrix4x4 m = cam.worldToCameraMatrix;
        Vector3 cpos = m.MultiplyPoint(offsetPos);
        Vector3 cnormal = m.MultiplyVector(normal).normalized * sideSign;
        return new Vector4(cnormal.x, cnormal.y, cnormal.z, -Vector3.Dot(cpos, cnormal));
    }

    //<summary>
    //Adjusts the given projection matrix so that near plane is the given clipPlane
    //clipPlane is given in camera space
    //</summary>
    private static void CalculateObliqueMatrix(ref Matrix4x4 projection, Vector4 clipPlane)
    {
        Vector4 q = projection.inverse * new Vector4(
            sgn(clipPlane.x),
            sgn(clipPlane.y),
            1.0f,
            1.0f
            );
        Vector4 c = clipPlane * (2.0F / (Vector4.Dot(clipPlane, q)));
        // third row = clip plane - fourth row
        projection[2] = c.x - projection[3];
        projection[6] = c.y - projection[7];
        projection[10] = c.z - projection[11];
        projection[14] = c.w - projection[15];
    }

    //<summary>
    //Calculates reflection matrix around the given plane
    //</summary>
    private static void CalculateReflectionMatrix(ref Matrix4x4 reflectionMat, Vector4 plane)
    {
        reflectionMat.m00 = (1F - 2F * plane[0] * plane[0]);
        reflectionMat.m01 = (-2F * plane[0] * plane[1]);
        reflectionMat.m02 = (-2F * plane[0] * plane[2]);
        reflectionMat.m03 = (-2F * plane[3] * plane[0]);

        reflectionMat.m10 = (-2F * plane[1] * plane[0]);
        reflectionMat.m11 = (1F - 2F * plane[1] * plane[1]);
        reflectionMat.m12 = (-2F * plane[1] * plane[2]);
        reflectionMat.m13 = (-2F * plane[3] * plane[1]);

        reflectionMat.m20 = (-2F * plane[2] * plane[0]);
        reflectionMat.m21 = (-2F * plane[2] * plane[1]);
        reflectionMat.m22 = (1F - 2F * plane[2] * plane[2]);
        reflectionMat.m23 = (-2F * plane[3] * plane[2]);

        reflectionMat.m30 = 0F;
        reflectionMat.m31 = 0F;
        reflectionMat.m32 = 0F;
        reflectionMat.m33 = 1F;
    }

    private void UpdateCameraSkyboxs()
    {
        m_CameraSkyBoxs.Clear();
        var e = m_ReflectionCameras.GetEnumerator();
        while (e.MoveNext())
        {
            Camera cam1 = e.Current.Key;
            Camera cam2 = e.Current.Value;

            if (cam1 != null && !m_CameraSkyBoxs.ContainsKey(cam1))
                m_CameraSkyBoxs.Add(cam1, cam1.GetComponent<Skybox>());
            if (cam2 != null && !m_CameraSkyBoxs.ContainsKey(cam2))
                m_CameraSkyBoxs.Add(cam2, cam2.GetComponent<Skybox>());
        }
    }
}
