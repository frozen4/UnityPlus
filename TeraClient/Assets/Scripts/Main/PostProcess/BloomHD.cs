using UnityEngine;

namespace PostProcess
{
    [RequireComponent(typeof(Camera))]
    public class BloomHD : MonoBehaviour
    {
        [Range(0, 4)]
        public float Threshold = 1.2f;
        [Range(0, 2)]
        public float Intensity = 1;
        [Range(0, 2)]
        public float Radius = 0.5f;
        [Range(0, 16)]
        public int Iteration = 8;
        public float SoftKneeA = 0;
        public bool WithFlicker = false;

        Material _material;

      
        RenderTexture[] rtCache = new RenderTexture[16];
        RenderTexture[] rtCache1 = new RenderTexture[16];

        private int _Level = 1;
        private int _Iteration = 8;

        //简化模式下的参数
        public static int _LEVEL = 2;
        public static int _ITERATION = 4;

        private int _ThresholdId = 0;
        private int _CurveId = 0;
        private int _UpSampleScaleId = 0;
        private int _IntensityId = 0;
        private int _BloomTexId = 0;

        void Awake()
        {
            //_ThresholdId = Shader.PropertyToID("_Threshold");
            //_CurveId = Shader.PropertyToID("_Curve");
            //_UpSampleScaleId = Shader.PropertyToID("_UpSampleScale");
            //_IntensityId = Shader.PropertyToID("_Intensity");
            //_BloomTexId = Shader.PropertyToID("_BloomTex");

            _ThresholdId = ShaderIDs.ThresholdId;
            _CurveId = ShaderIDs.CurveId;
            _UpSampleScaleId = ShaderIDs.UpSampleScaleId;
            _IntensityId = ShaderIDs.IntensityId;
            _BloomTexId = ShaderIDs.BloomTexId;
        }

        void OnEnable()
        {
            GetComponent<Camera>().allowHDR = true;
        }

        void OnDisable()
        {
            GetComponent<Camera>().allowHDR = false;

            if (_material != null)
            {
                Object.Destroy(_material);
                _material = null;
            }
        }

        private void UpdateParams()
        {
            if (Main.IsInGame())
            {
                if (GFXConfig.Instance.IsUseSimpleBloomHD)
                {
                    _Level = Application.isMobilePlatform ? _LEVEL : 1;
                    _Iteration = Mathf.Clamp(Iteration, 2, _ITERATION);
                }
                else
                {
                    _Level = 1;
                    _Iteration = Mathf.Clamp(Iteration, 2, 8);
                }
            }
            else
            {
                _Level = 1;
                _Iteration = Application.isMobilePlatform ? _ITERATION : 8;
            }
        }

        public void RenderImage(ref RenderTexture src, ref RenderTexture dest)
        {
            UpdateParams();

            Render(src, dest);

            RenderTexture tmp = src;
            src = dest;
            dest = tmp;
        }

        private enum BloomHDPass
        {
            Bright = 0,
            BrightWithFlicker,
            Downsample,
            DownsampleWithFlicker,
            Upsample,
            Blend,
            Final,
        }

        void Render(RenderTexture src, RenderTexture dest)
        {
            if (_material == null)
            {
                Shader shader = PostProcessChain.GetShader("Hidden/PostProcessing/BloomHD");
                if (shader == null)
                {
                    Graphics.Blit(src, dest);
                    return;
                }
                _material = new Material(shader);
            }

            var threshold = Mathf.GammaToLinearSpace(Threshold);
            _material.SetFloat(_ThresholdId, threshold);

            float SoftKnee = SoftKneeA;
            var knee = threshold * SoftKnee + 1e-5f;
            var curve = new Vector3(threshold - knee, knee * 2, 0.25f / knee);
            _material.SetVector(_CurveId, curve);

            float sampleRadius = Radius;
            _material.SetFloat(_UpSampleScaleId, sampleRadius);

            _material.SetFloat(_IntensityId, Intensity);

            RenderTextureFormat rtFormat = RenderTextureFormat.DefaultHDR;

            int filterTexWidth = Screen.width >> _Level;
            int filterTexHeight = Screen.height >> _Level;

            RenderTexture filter_texture = RenderTexture.GetTemporary(filterTexWidth, filterTexHeight, 0, rtFormat); 
            filter_texture.name = "filter_texture";
            RenderTexture blend_texture = RenderTexture.GetTemporary(filterTexWidth, filterTexHeight, 0, rtFormat);
            blend_texture.name = "blend_texture";

            // 注意：WithFlicker 字面意思与实际作用正好相反
            Graphics.Blit(src, filter_texture, _material, WithFlicker ? (int)BloomHDPass.Bright : (int)BloomHDPass.BrightWithFlicker);

            int levelWidth = filterTexWidth >> 1;
            int levelHeight = filterTexHeight >> 1;

            for (int i = 0; i < 16; ++i)
            {
                rtCache[i] = null;
                rtCache1[i] = null;
            }

            RenderTexture last = filter_texture;

            // down sample
            int it = 0;
            for (; it < _Iteration && levelWidth >= 4 && levelHeight >= 4; ++it)
            {
                rtCache[it] = RenderTexture.GetTemporary(levelWidth, levelHeight, 0, rtFormat); 
                rtCache[it].name = "rtCache";

                // 注意：WithFlicker 字面意思与实际作用正好相反
                Graphics.Blit(last, rtCache[it], _material, WithFlicker ? (int)BloomHDPass.Downsample : (int)BloomHDPass.DownsampleWithFlicker);

                levelWidth = levelWidth >> 1;
                levelHeight = levelHeight >> 1;

                last = rtCache[it];
            }

            // up sampler
            for (int i = it - 2; i >= 0; i--)
            {
                rtCache1[i] = RenderTexture.GetTemporary(rtCache[i].width, rtCache[i].height, 0, rtFormat);
                rtCache1[i].name = "rtCache1";

                _material.SetTexture(_BloomTexId, rtCache[i]);

                Graphics.Blit(last, rtCache1[i], _material, (int)BloomHDPass.Upsample);

                last = rtCache1[i];
            }
            _material.SetTexture(_BloomTexId, filter_texture);

            // blend
            RenderTexture rt = rtCache1[0] == null ? last : rtCache1[0];

            Graphics.Blit(rt, blend_texture, _material, (int)BloomHDPass.Blend);

            //_material.
            _material.SetTexture(_BloomTexId, blend_texture);

            Graphics.Blit(src, dest, _material, (int)BloomHDPass.Final);

            RenderTexture.ReleaseTemporary(blend_texture);
            RenderTexture.ReleaseTemporary(filter_texture);

            for (int i = 0; i < 16; ++i)
            {
                if (rtCache[i] != null)
                    RenderTexture.ReleaseTemporary(rtCache[i]);
                if (rtCache1[i] != null)
                    RenderTexture.ReleaseTemporary(rtCache1[i]);
            }
        }
    }
}