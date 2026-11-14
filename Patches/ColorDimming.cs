using UnityEngine;

namespace SimpleDimmer.Patches
{
    internal static class ColorDimming
    {
        public static void ScaleRgb(ref Color color, float multiplier)
        {
            color.r *= multiplier;
            color.g *= multiplier;
            color.b *= multiplier;
        }

        // RGB を s 倍、アルファを上限 1 で切ってから s 倍する。
        // 色（RGB × アルファ）は s² = k 倍、白ブースト（アルファ²）も k 倍になる
        public static void ScaleRgbAndAlpha(ref Color color, float s)
        {
            color.r *= s;
            color.g *= s;
            color.b *= s;
            color.a = s * Mathf.Min(color.a, 1f);
        }

        public static Color ScaledRgb(Color color, float multiplier)
        {
            ScaleRgb(ref color, multiplier);
            return color;
        }
    }
}
