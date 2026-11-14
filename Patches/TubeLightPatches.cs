using System.Collections.Generic;
using HarmonyLib;
using SimpleDimmer.Configuration;
using UnityEngine;

namespace SimpleDimmer.Patches
{
    // チューブ型ライトの減光（B 方式）
    // ・本体の色はイベント入口で RGB を k 倍する。グロー（BloomPrePass）は本体の色を毎フレーム読むので、これで k 倍になる。
    // ・線（ParametricBoxController）とブルーム OFF 時の発光（Parametric3SliceSpriteController）は、
    //   白ブーストがアルファの2乗で付くため、Refresh の直前にアルファを上限付きで下げ、RGB を補正する。
    // ・Refresh は色が変わったときだけ呼ばれる。毎フレーム動くパッチは置かない。
    // ・チューブ型ライトはセイバーやノーツの演出にも使われるため、イベントから色を受けたものだけを対象にする。

    internal sealed class TubeDimEntry
    {
        public Color Original;
        public Color Dimmed;
        public bool LineMultipliesColorByAlpha;
        public float OriginalBoostToWhite;
        public TubeBloomPrePassLight Tube;

        // 部品に渡された色が、このモッドが入口で作った色かどうか（他の経路で書かれた色は触らない）
        public bool Matches(Color color) => color.r == Dimmed.r && color.g == Dimmed.g && color.b == Dimmed.b;
    }

    internal static class TubeRegistry
    {
        private static readonly AccessTools.FieldRef<TubeBloomPrePassLight, ParametricBoxController> LineRef =
            AccessTools.FieldRefAccess<TubeBloomPrePassLight, ParametricBoxController>("_parametricBoxController");
        private static readonly AccessTools.FieldRef<TubeBloomPrePassLight, Parametric3SliceSpriteController> GlowRef =
            AccessTools.FieldRefAccess<TubeBloomPrePassLight, Parametric3SliceSpriteController>("_dynamic3SliceSprite");
        private static readonly AccessTools.FieldRef<TubeBloomPrePassLight, float> BoostRef =
            AccessTools.FieldRefAccess<TubeBloomPrePassLight, float>("_boostToWhite");
        private static readonly AccessTools.FieldRef<ParametricBoxController, MeshRenderer> LineRendererRef =
            AccessTools.FieldRefAccess<ParametricBoxController, MeshRenderer>("_meshRenderer");

        private static readonly Dictionary<TubeBloomPrePassLight, TubeDimEntry> ByTube = new Dictionary<TubeBloomPrePassLight, TubeDimEntry>();
        private static readonly Dictionary<ParametricBoxController, TubeDimEntry> ByLine = new Dictionary<ParametricBoxController, TubeDimEntry>();
        private static readonly Dictionary<Parametric3SliceSpriteController, TubeDimEntry> ByGlow = new Dictionary<Parametric3SliceSpriteController, TubeDimEntry>();

        public static void Record(TubeBloomPrePassLight tube, Color original, Color dimmed)
        {
            if (!ByTube.TryGetValue(tube, out TubeDimEntry entry))
            {
                entry = Create(tube);
            }
            entry.Original = original;
            entry.Dimmed = dimmed;
        }

        public static bool TryGetLine(ParametricBoxController line, out TubeDimEntry entry) => ByLine.TryGetValue(line, out entry);

        public static bool TryGetGlow(Parametric3SliceSpriteController glow, out TubeDimEntry entry) => ByGlow.TryGetValue(glow, out entry);

        private static TubeDimEntry Create(TubeBloomPrePassLight tube)
        {
            var entry = new TubeDimEntry { Tube = tube };
            ByTube[tube] = entry;

            ParametricBoxController line = LineRef(tube);
            if (line != null)
            {
                entry.LineMultipliesColorByAlpha = LineMultipliesColorByAlpha(LineRendererRef(line));
                ByLine[line] = entry;
            }

            Parametric3SliceSpriteController glow = GlowRef(tube);
            if (glow != null)
            {
                ByGlow[glow] = entry;
            }

            // グローの白寄せ量は毎フレーム読まれる値なので、登録時に1度だけ k 倍する（シーン終了時に戻す）
            ref float boost = ref BoostRef(tube);
            entry.OriginalBoostToWhite = boost;
            if (boost > 0f)
            {
                boost *= DimmerState.Multiplier;
            }
            return entry;
        }

        // 線のシェーダが色にアルファを掛けるかどうか。
        // OpaqueNeonLight と、_MultiplyColorWithAlpha が有効な TransparentNeonLight は掛ける。
        // それ以外（計算式を確認していないシェーダを含む）は掛けないものとして扱い、RGB も k 倍のまま残す。
        private static bool LineMultipliesColorByAlpha(MeshRenderer renderer)
        {
            if (renderer == null) return false;
            Material material = renderer.sharedMaterial;
            if (material == null || material.shader == null) return false;
            switch (material.shader.name)
            {
                case "Custom/OpaqueNeonLight":
                    return true;
                case "Custom/TransparentNeonLight":
                    return material.HasProperty("_MultiplyColorWithAlpha") && material.GetFloat("_MultiplyColorWithAlpha") != 0f;
                default:
                    return false;
            }
        }

        public static void RestoreAndClear()
        {
            foreach (TubeDimEntry entry in ByTube.Values)
            {
                if (entry.OriginalBoostToWhite > 0f && entry.Tube != null)
                {
                    BoostRef(entry.Tube) = entry.OriginalBoostToWhite;
                }
            }
            ByTube.Clear();
            ByLine.Clear();
            ByGlow.Clear();
        }
    }

    // 入口：RGB を k 倍し、元の色を記録する
    [HarmonyPatch(typeof(TubeBloomPrePassLightWithId), "ColorWasSet")]
    internal static class TubeLightPatch
    {
        [HarmonyPriority(Priority.First)]
        private static void Prefix(ref Color __0, TubeBloomPrePassLight ____tubeBloomPrePassLight)
        {
            if (!DimmerState.LightsActive || ____tubeBloomPrePassLight == null) return;
            Color original = __0;
            ColorDimming.ScaleRgb(ref __0, DimmerState.Multiplier);
            TubeRegistry.Record(____tubeBloomPrePassLight, original, __0);
        }
    }

    // 線：画面アルファ（= アルファ × alphaMultiplier）を上限 1 で切ってから √k 倍する。
    // 線の明るさは (アルファ × alphaMultiplier)² に比例するので、色は k 倍、白は k² 倍になる。
    [HarmonyPatch(typeof(ParametricBoxController), "Refresh")]
    internal static class TubeLinePatch
    {
        private static void Prefix(ParametricBoxController __instance, ref Color ___color, float ___alphaMultiplier, out Color? __state)
        {
            __state = null;
            if (!DimmerState.LightsActive) return;
            if (!TubeRegistry.TryGetLine(__instance, out TubeDimEntry entry) || !entry.Matches(___color)) return;

            __state = ___color;
            Color color = ___color;
            if (entry.LineMultipliesColorByAlpha)
            {
                // アルファ側で k 倍になるので、入口で掛けた RGB の k 倍は外す
                color.r = entry.Original.r;
                color.g = entry.Original.g;
                color.b = entry.Original.b;
            }
            if (___alphaMultiplier > 0f)
            {
                color.a = DimmerState.SqrtMultiplier * Mathf.Min(color.a * ___alphaMultiplier, 1f) / ___alphaMultiplier;
            }
            ___color = color;
        }

        private static void Postfix(ref Color ___color, Color? __state)
        {
            if (__state.HasValue) ___color = __state.Value;
        }
    }

    // ブルーム OFF 時の発光：明るさはアルファに比例するので、アルファと RGB をそれぞれ √k 倍して k 倍にする
    [HarmonyPatch(typeof(Parametric3SliceSpriteController), "Refresh")]
    internal static class TubeBakedGlowPatch
    {
        private static void Prefix(Parametric3SliceSpriteController __instance, ref Color ___color, float ___alphaMultiplier, out Color? __state)
        {
            __state = null;
            if (!DimmerState.LightsActive) return;
            if (!TubeRegistry.TryGetGlow(__instance, out TubeDimEntry entry) || !entry.Matches(___color)) return;

            __state = ___color;
            float s = DimmerState.SqrtMultiplier;
            Color color = ___color;
            color.r = entry.Original.r * s;
            color.g = entry.Original.g * s;
            color.b = entry.Original.b * s;
            if (___alphaMultiplier > 0f)
            {
                color.a = s * Mathf.Min(color.a * ___alphaMultiplier, 1f) / ___alphaMultiplier;
            }
            ___color = color;
        }

        private static void Postfix(ref Color ___color, Color? __state)
        {
            if (__state.HasValue) ___color = __state.Value;
        }
    }
}
