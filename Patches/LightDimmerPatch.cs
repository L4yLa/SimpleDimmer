using HarmonyLib;
using SimpleDimmer.Configuration;
using UnityEngine;

namespace SimpleDimmer.Patches
{
    [HarmonyPatch]
    internal static class LightDimmerPatch
    {
        private static void ApplyDimming(ref Color color)
        {
            if (!PluginConfig.Instance.RuntimeEnabled) return;
            if (!PluginConfig.Instance.RuntimeIsGameScene) return;
            if (!PluginConfig.Instance.RuntimeDimmingLights) return;

            float intensity = PluginConfig.Instance.RuntimeBrightness;
            if (intensity >= 1f) return;

            float alpha = color.a;
            Color.RGBToHSV(color, out float h, out float s, out float v);
            v *= intensity;
            color = Color.HSVToRGB(h, s, v, hdr: true);
            color.a = alpha;
        }

        private static void ApplyDimmingWall(ref Color color)
        {
            if (!PluginConfig.Instance.RuntimeEnabled) return;
            if (!PluginConfig.Instance.RuntimeIsGameScene) return;
            if (!PluginConfig.Instance.RuntimeDimmingWalls) return;

            float intensity = PluginConfig.Instance.RuntimeBrightness;
            if (intensity >= 1f) return;

            float alpha = color.a;
            Color.RGBToHSV(color, out float h, out float s, out float v);
            v *= intensity;
            color = Color.HSVToRGB(h, s, v, hdr: true);
            color.a = alpha;
        }

        [HarmonyPrefix]
        [HarmonyPriority(Priority.First)]
        [HarmonyPatch(typeof(BloomPrePassBackgroundColorsGradientTintColorWithLightIds), nameof(BloomPrePassBackgroundColorsGradientTintColorWithLightIds.ColorWasSet))]
        private static void BloomPrePassBackgroundColorsGradientTintColorWithLightIds_ColorWasSet(ref Color __0) =>
            ApplyDimming(ref __0);

        [HarmonyPrefix]
        [HarmonyPriority(Priority.First)]
        [HarmonyPatch(typeof(BloomPrePassBackgroundColorsGradientElementWithLightId), nameof(BloomPrePassBackgroundColorsGradientElementWithLightId.ColorWasSet))]
        private static void BloomPrePassBackgroundColorsGradientElementWithLightId_ColorWasSet(ref Color __0) =>
            ApplyDimming(ref __0);

        [HarmonyPrefix]
        [HarmonyPriority(Priority.First)]
        [HarmonyPatch(typeof(BloomPrePassBackgroundLightWithId), nameof(BloomPrePassBackgroundLightWithId.ColorWasSet))]
        private static void BloomPrePassBackgroundLightWithId_ColorWasSet(ref Color __0) =>
            ApplyDimming(ref __0);

        [HarmonyPrefix]
        [HarmonyPriority(Priority.First)]
        [HarmonyPatch(typeof(DirectionalLightWithId), nameof(DirectionalLightWithId.ColorWasSet))]
        private static void DirectionalLightWithId_ColorWasSet(ref Color __0) =>
            ApplyDimming(ref __0);

        [HarmonyPrefix]
        [HarmonyPriority(Priority.First)]
        [HarmonyPatch(typeof(InstancedMaterialLightWithId), nameof(InstancedMaterialLightWithId.ColorWasSet))]
        private static void InstancedMaterialLightWithId_ColorWasSet(ref Color __0) =>
            ApplyDimming(ref __0);

        [HarmonyPrefix]
        [HarmonyPriority(Priority.First)]
        [HarmonyPatch(typeof(MaterialLightWithId), nameof(MaterialLightWithId.ColorWasSet))]
        private static void MaterialLightWithId_ColorWasSet(ref Color __0) =>
            ApplyDimming(ref __0);

        [HarmonyPrefix]
        [HarmonyPriority(Priority.First)]
        [HarmonyPatch(typeof(ParticleSystemLightWithId), nameof(ParticleSystemLightWithId.ColorWasSet))]
        private static void ParticleSystemLightWithId_ColorWasSet(ref Color __0) =>
            ApplyDimming(ref __0);

        [HarmonyPrefix]
        [HarmonyPriority(Priority.First)]
        [HarmonyPatch(typeof(SpriteLightWithId), nameof(SpriteLightWithId.ColorWasSet))]
        private static void SpriteLightWithId_ColorWasSet(ref Color __0) =>
            ApplyDimming(ref __0);

        [HarmonyPrefix]
        [HarmonyPriority(Priority.First)]
        [HarmonyPatch(typeof(TubeBloomPrePassLightWithId), nameof(TubeBloomPrePassLightWithId.ColorWasSet))]
        private static void TubeBloomPrePassLightWithId_ColorWasSet(ref Color __0) =>
            ApplyDimming(ref __0);

        [HarmonyPrefix]
        [HarmonyPriority(Priority.First)]
        [HarmonyPatch(typeof(UnityLightWithId), nameof(UnityLightWithId.ColorWasSet))]
        private static void UnityLightWithId_ColorWasSet(ref Color __0) =>
            ApplyDimming(ref __0);

        [HarmonyPrefix]
        [HarmonyPriority(Priority.First)]
        [HarmonyPatch(typeof(LightWithIds.LightWithId), nameof(LightWithIds.LightWithId.ColorWasSet))]
        private static void LightWithIds_LightWithId_ColorWasSet(ref Color __0) =>
            ApplyDimming(ref __0);

        // StretchableObstacle: SetAllProperties(float width, float height, float length, Color color, float manualUvOffset)
        [HarmonyPrefix]
        [HarmonyPriority(Priority.First)]
        [HarmonyPatch(typeof(StretchableObstacle), nameof(StretchableObstacle.SetAllProperties))]
        private static void StretchableObstacle_SetAllProperties(
            float width, float height, float length, ref Color color, float manualUvOffset) =>
            ApplyDimmingWall(ref color);

        // ParametricBoxFakeGlowController: Refresh() reads public Color color field
        [HarmonyPrefix]
        [HarmonyPriority(Priority.First)]
        [HarmonyPatch(typeof(ParametricBoxFakeGlowController), nameof(ParametricBoxFakeGlowController.Refresh))]
        private static void ParametricBoxFakeGlowController_Refresh_Pre(
            ParametricBoxFakeGlowController __instance, out Color __state)
        {
            __state = __instance.color;
            ApplyDimmingWall(ref __instance.color);
        }

        [HarmonyPostfix]
        [HarmonyPriority(Priority.First)]
        [HarmonyPatch(typeof(ParametricBoxFakeGlowController), nameof(ParametricBoxFakeGlowController.Refresh))]
        private static void ParametricBoxFakeGlowController_Refresh_Post(
            ParametricBoxFakeGlowController __instance, Color __state)
            => __instance.color = __state;

        // ParametricBoxController: Refresh() reads public Color color field
        [HarmonyPrefix]
        [HarmonyPriority(Priority.First)]
        [HarmonyPatch(typeof(ParametricBoxController), nameof(ParametricBoxController.Refresh))]
        private static void ParametricBoxController_Refresh_Pre(
            ParametricBoxController __instance, out Color __state)
        {
            __state = __instance.color;
            ApplyDimmingWall(ref __instance.color);
        }

        [HarmonyPostfix]
        [HarmonyPriority(Priority.First)]
        [HarmonyPatch(typeof(ParametricBoxController), nameof(ParametricBoxController.Refresh))]
        private static void ParametricBoxController_Refresh_Post(
            ParametricBoxController __instance, Color __state)
            => __instance.color = __state;
    }
}