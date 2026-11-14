using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using SimpleDimmer.Configuration;
using UnityEngine;

namespace SimpleDimmer.Patches
{
    // ライトは種類ごとに色の RGB とアルファの使い方が違うため、実際に明るさへ効く側に倍率を掛ける。
    // Chroma はライトごとに ColorWasSet を直接呼ぶので、標準・Chroma のどちらの経路もここを通る。
    // チューブ型ライトは TubeLightPatches.cs で扱う。

    // 画面のアルファを書かないライト（Unity のライト・背景色）。RGB だけを下げる
    [HarmonyPatch]
    internal static class RgbLightPatch
    {
        private static IEnumerable<MethodBase> TargetMethods()
        {
            Type[] types =
            {
                typeof(BloomPrePassBackgroundColorsGradientTintColorWithLightIds),
                typeof(BloomPrePassBackgroundColorsGradientElementWithLightId),
                typeof(BloomPrePassBackgroundLightWithId),
                typeof(BloomPrePassBackgroundColorsGradientTintColorWithLightId),
                typeof(DirectionalLightWithId),
                typeof(UnityLightWithId),
                typeof(PointLightWithIds),
                typeof(DirectionalLightWithLightGroupIds),
            };
            foreach (Type type in types)
            {
                yield return AccessTools.DeclaredMethod(type, "ColorWasSet", new[] { typeof(Color) });
            }
        }

        [HarmonyPriority(Priority.First)]
        private static void Prefix(ref Color __0)
        {
            if (!DimmerState.LightsActive) return;
            ColorDimming.ScaleRgb(ref __0, DimmerState.Multiplier);
        }
    }

    // 描画色のアルファが画面のアルファになり、後処理で白ブーストが付くライト（A 方式）。
    // RGB を下げるだけでは白が残るので、アルファも上限 1 で切ってから下げる
    [HarmonyPatch]
    internal static class AlphaLightPatch
    {
        private static IEnumerable<MethodBase> TargetMethods()
        {
            Type[] types =
            {
                typeof(InstancedMaterialLightWithId),
                typeof(ParticleSystemLightWithId),
                typeof(RectangleFakeGlowLightWithId),
                typeof(ColorArrayLightWithIds.ColorArrayLightWithId),
                typeof(MixedLightsColorSetterRuntimeLightWithIds),
                typeof(ParticleSystemLightWithIds),
            };
            foreach (Type type in types)
            {
                yield return AccessTools.DeclaredMethod(type, "ColorWasSet", new[] { typeof(Color) });
            }
        }

        [HarmonyPriority(Priority.First)]
        private static void Prefix(ref Color __0)
        {
            if (!DimmerState.LightsActive) return;
            ColorDimming.ScaleRgbAndAlpha(ref __0, DimmerState.SqrtMultiplier);
        }
    }

    // グループ型ライトの子。親が受け取った色をまとめて処理する。
    // 親が RuntimeLightWithIds 系なら親の ColorWasSet 側で、ColorArrayLightWithId は自身の上書きメソッド側で処理する。
    [HarmonyPatch(typeof(LightWithIds.LightWithId), "ColorWasSet")]
    internal static class GroupedLightChildPatch
    {
        [HarmonyPriority(Priority.First)]
        private static void Prefix(LightWithIds.LightWithId __instance, ref Color __0, LightWithIds ____parentLightWithIds)
        {
            if (!DimmerState.LightsActive) return;
            if (__instance is ColorArrayLightWithIds.ColorArrayLightWithId) return;
            if (____parentLightWithIds is RuntimeLightWithIds || ____parentLightWithIds is RuntimeLightWithLightGroupIds) return;
            ColorDimming.ScaleRgb(ref __0, DimmerState.Multiplier);
        }
    }

    // アルファだけを使う設定があるマテリアル系ライト
    [HarmonyPatch(typeof(MaterialLightWithId), "ColorWasSet")]
    internal static class MaterialLightPatch
    {
        [HarmonyPriority(Priority.First)]
        private static void Prefix(ref Color __0, bool ____setAlphaOnly, bool ____alphaIntoColor)
        {
            if (!DimmerState.LightsActive) return;
            if (____setAlphaOnly || ____alphaIntoColor)
            {
                __0.a *= DimmerState.Multiplier;
            }
            else
            {
                ColorDimming.ScaleRgbAndAlpha(ref __0, DimmerState.SqrtMultiplier);
            }
        }
    }

    [HarmonyPatch(typeof(MaterialLightWithIds), "ColorWasSet")]
    internal static class MaterialLightsPatch
    {
        [HarmonyPriority(Priority.First)]
        private static void Prefix(ref Color __0, bool ____setAlphaOnly, bool ____alphaIntoColor)
        {
            if (!DimmerState.LightsActive) return;
            if (____setAlphaOnly || ____alphaIntoColor)
            {
                __0.a *= DimmerState.Multiplier;
            }
            else
            {
                ColorDimming.ScaleRgbAndAlpha(ref __0, DimmerState.SqrtMultiplier);
            }
        }
    }

    // 強度だけを使う設定では、アルファが明度になる
    [HarmonyPatch(typeof(DirectionalLightWithIds), "ColorWasSet")]
    internal static class DirectionalLightsPatch
    {
        [HarmonyPriority(Priority.First)]
        private static void Prefix(ref Color __0, bool ____setIntensityOnly)
        {
            if (!DimmerState.LightsActive) return;
            if (____setIntensityOnly)
            {
                __0.a *= DimmerState.Multiplier;
            }
            else
            {
                ColorDimming.ScaleRgb(ref __0, DimmerState.Multiplier);
            }
        }
    }

    // アルファだけを使う設定ではアルファで表示・非表示も決まるため、判定の後で描画色のアルファを下げる
    [HarmonyPatch(typeof(SpriteLightWithId), "ColorWasSet")]
    internal static class SpriteLightPatch
    {
        [HarmonyPriority(Priority.First)]
        private static void Prefix(ref Color __0, bool ____setAlphaOnly)
        {
            if (!DimmerState.LightsActive || ____setAlphaOnly) return;
            ColorDimming.ScaleRgb(ref __0, DimmerState.Multiplier);
        }

        private static void Postfix(bool ____setAlphaOnly, SpriteRenderer ____spriteRenderer)
        {
            if (!DimmerState.LightsActive || !____setAlphaOnly || ____spriteRenderer == null) return;
            Color color = ____spriteRenderer.color;
            color.a *= DimmerState.Multiplier;
            ____spriteRenderer.color = color;
        }
    }

    [HarmonyPatch(typeof(SpriteArrayLightWithId), "ColorWasSet")]
    internal static class SpriteArrayLightPatch
    {
        [HarmonyPriority(Priority.First)]
        private static void Prefix(ref Color __0, bool ____setAlphaOnly)
        {
            if (!DimmerState.LightsActive || ____setAlphaOnly) return;
            ColorDimming.ScaleRgb(ref __0, DimmerState.Multiplier);
        }

        private static void Postfix(bool ____setAlphaOnly, SpriteRenderer[] ____spriteRenderers)
        {
            if (!DimmerState.LightsActive || !____setAlphaOnly || ____spriteRenderers == null) return;
            float multiplier = DimmerState.Multiplier;
            foreach (SpriteRenderer spriteRenderer in ____spriteRenderers)
            {
                if (spriteRenderer == null) continue;
                Color color = spriteRenderer.color;
                color.a *= multiplier;
                spriteRenderer.color = color;
            }
        }
    }
}
