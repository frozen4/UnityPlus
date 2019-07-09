// Colorful FX - Unity Asset
// Copyright (c) 2015 - Thomas Hourdel
// http://www.thomashourdel.com

namespace Colorful
{
	using UnityEngine;

	[ExecuteInEditMode]
	[AddComponentMenu("Colorful FX/Color Correction/Brightness, Contrast, Gamma")]
	public class BrightnessContrastGamma : BaseEffect
	{
		[Range(-100f, 100f), Tooltip("Moving the slider to the right increases tonal values and expands highlights, to the left decreases values and expands shadows.")]
		public float Brightness = 0f;

		[Range(-100f, 100f), Tooltip("Expands or shrinks the overall range of tonal values.")]
		public float Contrast = 0f;

		public Vector3 ContrastCoeff = new Vector3(0.5f, 0.5f, 0.5f);

		[Range(0.1f, 9.9f), Tooltip("Simple power function.")]
		public float Gamma = 1f;

        private int _BCGId = 0;
        private int _CoeffsId = 0;

        protected void Awake()
        {
            _BCGId = Shader.PropertyToID("_BCG");
            _CoeffsId = Shader.PropertyToID("_Coeffs");
        }

        public bool CanTurnOff()
        {
            return Brightness == 0f && Contrast == 0f && Gamma == 1f;
        }

        [ImageEffectOpaque]
		protected override void OnRenderImage(RenderTexture source, RenderTexture destination)
		{
			if (Brightness == 0f && Contrast == 0f && Gamma == 1f)
			{
				Graphics.Blit(source, destination);
				return;
			}

            Material.SetVector(_BCGId, new Vector4((Brightness + 100f) * 0.01f, (Contrast + 100f) * 0.01f, 1.0f / Gamma));
            Material.SetVector(_CoeffsId, ContrastCoeff);
			Graphics.Blit(source, destination, Material);
		}
	}
}
