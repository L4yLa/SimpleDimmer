using System.Collections.Generic;
using HarmonyLib;
using SimpleDimmer.Configuration;
using UnityEngine;

namespace SimpleDimmer.Patches
{
    // ウォールの枠と光りは Refresh の間だけ色を暗くし、終わったら元に戻す。
    // 元の色を残すことで、Chroma の「同じ色なら再設定しない」判定も正しく働く。
    // 枠のシェーダは画面アルファに「色のアルファ × 倍率」を書き、後処理でその2乗の白が足される。
    // RGB だけを下げると白が残るため、画面アルファを上限 1 で切ってから √k 倍する（白も k 倍になる）。
    [HarmonyPatch(typeof(ParametricBoxFrameController), nameof(ParametricBoxFrameController.Refresh))]
    internal static class WallFramePatch
    {
        private static readonly AccessTools.FieldRef<ParametricBoxFrameController, MeshRenderer> RendererRef =
            AccessTools.FieldRefAccess<ParametricBoxFrameController, MeshRenderer>("_meshRenderer");

        // 枠ごとの「画面アルファの倍率」。0 は画面アルファを書かない枠（アルファは触らない）
        private static readonly Dictionary<ParametricBoxFrameController, float> AlphaFactors = new Dictionary<ParametricBoxFrameController, float>();

        [HarmonyPriority(Priority.First)]
        private static void Prefix(ParametricBoxFrameController __instance, out Color __state)
        {
            __state = __instance.color;
            if (!DimmerState.WallsActive) return;

            ColorDimming.ScaleRgb(ref __instance.color, DimmerState.Multiplier);

            if (!AlphaFactors.TryGetValue(__instance, out float factor))
            {
                factor = GetAlphaFactor(RendererRef(__instance));
                AlphaFactors[__instance] = factor;
            }
            if (factor > 0f)
            {
                __instance.color.a = DimmerState.SqrtMultiplier * Mathf.Min(__instance.color.a * factor, 1f) / factor;
            }
        }

        private static void Postfix(ParametricBoxFrameController __instance, Color __state)
        {
            __instance.color = __state;
        }

        // ParametricBoxFrameHD は 2 × アルファ、LW は アルファ をそのまま出力する（逆コンパイルで確認）。
        // 鏡像の枠など、アルファを画面に書かない設定（_BlendSrcFactorA = 0）は 0 を返す
        private static float GetAlphaFactor(MeshRenderer renderer)
        {
            if (renderer == null) return 0f;
            Material material = renderer.sharedMaterial;
            if (material == null || material.shader == null) return 0f;
            if (material.HasProperty("_BlendSrcFactorA") && material.GetFloat("_BlendSrcFactorA") == 0f) return 0f;
            switch (material.shader.name)
            {
                case "Custom/ParametricBoxFrameHD":
                    return 2f;
                case "Custom/ParametricBoxFrameLW":
                    return 1f;
                default:
                    return 0f;
            }
        }

        public static void Clear() => AlphaFactors.Clear();
    }

    [HarmonyPatch(typeof(ParametricBoxFakeGlowController), nameof(ParametricBoxFakeGlowController.Refresh))]
    internal static class WallFakeGlowPatch
    {
        [HarmonyPriority(Priority.First)]
        private static void Prefix(ParametricBoxFakeGlowController __instance, out Color __state)
        {
            __state = __instance.color;
            if (!DimmerState.WallsActive) return;
            ColorDimming.ScaleRgb(ref __instance.color, DimmerState.Multiplier);
        }

        private static void Postfix(ParametricBoxFakeGlowController __instance, Color __state)
        {
            __instance.color = __state;
        }
    }

    // ウォールの中身はシェーダの色を直接書くため、書き込みの後で暗くした色に書き直す
    [HarmonyPatch(typeof(StretchableObstacle), nameof(StretchableObstacle.SetAllProperties))]
    internal static class WallCorePatch
    {
        // SetAllProperties(float width, float height, float length, Color color, float manualUvOffset)
        private static void Postfix(
            Color __3,
            MaterialPropertyBlockController[] ____materialPropertyBlockControllers,
            float ____addColorMultiplier,
            float ____obstacleCoreLerpToWhiteFactor)
        {
            if (!DimmerState.WallsActive) return;
            WallCoreColor.Apply(
                ____materialPropertyBlockControllers,
                __3,
                ____addColorMultiplier,
                ____obstacleCoreLerpToWhiteFactor,
                setMainColor: true);
        }
    }

    internal static class WallCoreColor
    {
        private static readonly int MainColorId = Shader.PropertyToID("_Color");
        private static readonly int AddColorId = Shader.PropertyToID("_AddColor");
        private static readonly int TintColorId = Shader.PropertyToID("_TintColor");

        public static void Apply(
            MaterialPropertyBlockController[]? controllers,
            Color color,
            float addColorMultiplier,
            float lerpToWhiteFactor,
            bool setMainColor)
        {
            if (controllers == null) return;
            float multiplier = DimmerState.Multiplier;

            Color addColor = ColorDimming.ScaledRgb(color * addColorMultiplier, multiplier);
            addColor.a = 0f;
            Color tintColor = ColorDimming.ScaledRgb(Color.Lerp(color, Color.white, lerpToWhiteFactor), multiplier);
            Color mainColor = ColorDimming.ScaledRgb(color, multiplier);

            foreach (MaterialPropertyBlockController controller in controllers)
            {
                if (controller == null) continue;
                MaterialPropertyBlock block = controller.materialPropertyBlock;
                if (setMainColor)
                {
                    block.SetColor(MainColorId, mainColor);
                }
                block.SetColor(AddColorId, addColor);
                block.SetColor(TintColorId, tintColor);
                controller.ApplyChanges();
            }
        }
    }
}
