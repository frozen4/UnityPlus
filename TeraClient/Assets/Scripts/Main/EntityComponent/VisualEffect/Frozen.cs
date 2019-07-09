using UnityEngine;
using Object = UnityEngine.Object;
using System;

namespace EntityVisualEffect
{
	public class Frozen : BaseEffect
    {
        private static Material FrozenMaterial;
        private static string FrozenEffectMatPath = @"Assets/Outputs/Others/FrozenEffect.mat";
        public static Material ForzenMaterial { get { return FrozenMaterial; } }

        private Action _LoadedCallback = null;
        public Frozen()
        {
            Type = EffectType.Frozen;
        }

        public void Start(Action action)
        {
            if (null == FrozenMaterial)
            {
                Action<UnityEngine.Object> callback = (asset) =>
                {
                    FrozenMaterial = asset as Material;
                    if (_LoadedCallback != null)
                    {
                        _LoadedCallback();
                        _LoadedCallback = null;
                    }
                };
                CAssetBundleManager.AsyncLoadResource(FrozenEffectMatPath, callback, false, "others");
                _LoadedCallback = action;
            }
            else
            {
                if (action != null) action();
            }
        }
	}
}
