using System;
using System.Reflection;
using HarmonyLib;
using SimpleDimmer.Configuration;
using UnityEngine;

namespace SimpleDimmer.Patches
{
    // 導入されている場合だけ当てるパッチ（Chroma / VMCLight）。参照はせず、型を名前で探す
    internal static class OptionalPatches
    {
        public static void Apply(Harmony harmony)
        {
            ChromaObstaclePatch.Apply(harmony);
            VmcLightPatch.Apply(harmony);
        }
    }

    // Chroma で色を付けたウォールは SetAllProperties を通らず、中身の色を Chroma が直接書く
    internal static class ChromaObstaclePatch
    {
        private static AccessTools.FieldRef<object, ParametricBoxFrameController>? _frame;
        private static AccessTools.FieldRef<object, MaterialPropertyBlockController[]>? _controllers;
        private static AccessTools.FieldRef<object, float>? _addColorMultiplier;
        private static AccessTools.FieldRef<object, float>? _lerpToWhiteFactor;

        public static void Apply(Harmony harmony)
        {
            Type? type = AccessTools.TypeByName("Chroma.Colorizer.ObstacleColorizer");
            if (type == null) return;

            try
            {
                MethodInfo? refresh = AccessTools.DeclaredMethod(type, "Refresh", Type.EmptyTypes);
                if (refresh == null) throw new MissingMethodException(type.FullName, "Refresh");

                _frame = AccessTools.FieldRefAccess<ParametricBoxFrameController>(type, "_obstacleFrame");
                _controllers = AccessTools.FieldRefAccess<MaterialPropertyBlockController[]>(type, "_materialPropertyBlockControllers");
                _addColorMultiplier = AccessTools.FieldRefAccess<float>(type, "_addColorMultiplier");
                _lerpToWhiteFactor = AccessTools.FieldRefAccess<float>(type, "_obstacleCoreLerpToWhiteFactor");

                harmony.Patch(
                    refresh,
                    prefix: new HarmonyMethod(typeof(ChromaObstaclePatch), nameof(Prefix)),
                    postfix: new HarmonyMethod(typeof(ChromaObstaclePatch), nameof(Postfix)));
                Plugin.Log.Info("Chroma obstacle patch applied.");
            }
            catch (Exception e)
            {
                _frame = null;
                Plugin.Log.Warn($"Chroma obstacle patch skipped: {e.Message}");
            }
        }

        private static void Prefix(object __instance, out Color __state)
        {
            ParametricBoxFrameController? frame = _frame?.Invoke(__instance);
            __state = frame != null ? frame.color : default;
        }

        // Chroma は色が変わったときだけ枠の色を更新して中身を書く。変わっていなければ何もしない
        private static void Postfix(object __instance, Color __state)
        {
            if (!DimmerState.WallsActive || _frame == null) return;
            ParametricBoxFrameController frame = _frame(__instance);
            if (frame == null || frame.color == __state) return;

            WallCoreColor.Apply(
                _controllers!(__instance),
                frame.color,
                _addColorMultiplier!(__instance),
                _lerpToWhiteFactor!(__instance),
                setMainColor: false);
        }
    }

    // VMCLight はライトの色を受け取る後処理から VMC へ送る。受け取る色を暗くした値に差し替える
    internal static class VmcLightPatch
    {
        private static readonly string[] TargetTypes =
        {
            "VMCLight.HarmonyPatches.InGameLightPatch",
            "VMCLight.HarmonyPatches.ChromaLightPatch",
        };

        public static void Apply(Harmony harmony)
        {
            foreach (string typeName in TargetTypes)
            {
                Type? type = AccessTools.TypeByName(typeName);
                if (type == null) continue;

                try
                {
                    MethodInfo? postfix = AccessTools.DeclaredMethod(type, "Postfix");
                    if (postfix == null) throw new MissingMethodException(type.FullName, "Postfix");
                    harmony.Patch(postfix, prefix: new HarmonyMethod(typeof(VmcLightPatch), nameof(Prefix)));
                    Plugin.Log.Info($"VMCLight patch applied: {type.Name}");
                }
                catch (Exception e)
                {
                    Plugin.Log.Warn($"VMCLight patch skipped ({type.Name}): {e.Message}");
                }
            }
        }

        private static void Prefix(ref Color color)
        {
            if (!DimmerState.LightsActive) return;
            ColorDimming.ScaleRgb(ref color, DimmerState.Multiplier);
        }
    }
}
