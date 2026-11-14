using UnityEngine;

namespace SimpleDimmer.Configuration
{
    // 体感の明るさ（CIE L*）から輝度の倍率へ変換する（L* の逆変換）
    internal static class BrightnessCurve
    {
        public static float ToMultiplier(int percent)
        {
            float lightness = Mathf.Clamp(percent, 0, 100);
            if (lightness > 8f)
            {
                float f = (lightness + 16f) / 116f;
                return f * f * f;
            }
            return lightness / 903.3f;
        }
    }
}
