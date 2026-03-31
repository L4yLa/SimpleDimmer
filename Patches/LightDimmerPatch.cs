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
            if (!PluginConfig.Instance.Enabled) return;
            float intensity = PluginConfig.Instance.Brightness;
            if (intensity >= 1f) return;
            float alpha = color.a;
            Color.RGBToHSV(color, out float h, out float s, out float v);
            v *= intensity;
            color = Color.HSVToRGB(h, s, v);
            color.a = alpha;
        }

        [HarmonyPrefix]
        [HarmonyPriority(Priority.First)]
        [HarmonyPatch(typeof(BloomPrePassBackgroundColorsGradientTintColorWithLightIds), nameof(BloomPrePassBackgroundColorsGradientTintColorWithLightIds.ColorWasSet))]
        private static void BloomPrePassBackgroundColorsGradientTintColorWithLightIds_ColorWasSet(ref Color newColor) =>
            ApplyDimming(ref newColor);

        [HarmonyPrefix]
        [HarmonyPriority(Priority.First)]
        [HarmonyPatch(typeof(BloomPrePassBackgroundColorsGradientElementWithLightId), nameof(BloomPrePassBackgroundColorsGradientElementWithLightId.ColorWasSet))]
        private static void BloomPrePassBackgroundColorsGradientElementWithLightId_ColorWasSet(ref Color newColor) =>
            ApplyDimming(ref newColor);

        [HarmonyPrefix]
        [HarmonyPriority(Priority.First)]
        [HarmonyPatch(typeof(BloomPrePassBackgroundLightWithId), nameof(BloomPrePassBackgroundLightWithId.ColorWasSet))]
        private static void BloomPrePassBackgroundLightWithId_ColorWasSet(ref Color newColor) =>
            ApplyDimming(ref newColor);

        [HarmonyPrefix]
        [HarmonyPriority(Priority.First)]
        [HarmonyPatch(typeof(DirectionalLightWithId), nameof(DirectionalLightWithId.ColorWasSet))]
        private static void DirectionalLightWithId_ColorWasSet(ref Color newColor) =>
            ApplyDimming(ref newColor);

        [HarmonyPrefix]
        [HarmonyPriority(Priority.First)]
        [HarmonyPatch(typeof(InstancedMaterialLightWithId), nameof(InstancedMaterialLightWithId.ColorWasSet))]
        private static void InstancedMaterialLightWithId_ColorWasSet(ref Color newColor) =>
            ApplyDimming(ref newColor);

        [HarmonyPrefix]
        [HarmonyPriority(Priority.First)]
        [HarmonyPatch(typeof(MaterialLightWithId), nameof(MaterialLightWithId.ColorWasSet))]
        private static void MaterialLightWithId_ColorWasSet(ref Color newColor) =>
            ApplyDimming(ref newColor);

        [HarmonyPrefix]
        [HarmonyPriority(Priority.First)]
        [HarmonyPatch(typeof(ParticleSystemLightWithId), nameof(ParticleSystemLightWithId.ColorWasSet))]
        private static void ParticleSystemLightWithId_ColorWasSet(ref Color newColor) =>
            ApplyDimming(ref newColor);

        [HarmonyPrefix]
        [HarmonyPriority(Priority.First)]
        [HarmonyPatch(typeof(SpriteLightWithId), nameof(SpriteLightWithId.ColorWasSet))]
        private static void SpriteLightWithId_ColorWasSet(ref Color newColor) =>
            ApplyDimming(ref newColor);

        [HarmonyPrefix]
        [HarmonyPriority(Priority.First)]
        [HarmonyPatch(typeof(TubeBloomPrePassLightWithId), nameof(TubeBloomPrePassLightWithId.ColorWasSet))]
        private static void TubeBloomPrePassLightWithId_ColorWasSet(ref Color newColor) =>
            ApplyDimming(ref newColor);

        [HarmonyPrefix]
        [HarmonyPriority(Priority.First)]
        [HarmonyPatch(typeof(UnityLightWithId), nameof(UnityLightWithId.ColorWasSet))]
        private static void UnityLightWithId_ColorWasSet(ref Color newColor) =>
            ApplyDimming(ref newColor);

        [HarmonyPrefix]
        [HarmonyPriority(Priority.First)]
        [HarmonyPatch(typeof(LightWithIds.LightWithId), nameof(LightWithIds.LightWithId.ColorWasSet))]
        private static void LightWithIds_LightWithId_ColorWasSet(ref Color newColor) =>
            ApplyDimming(ref newColor);
    }
}
