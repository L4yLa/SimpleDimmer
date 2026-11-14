namespace SimpleDimmer.Configuration
{
    // パッチから参照する実行時の状態。設定ファイルには保存しない
    internal static class DimmerState
    {
        public static bool Enabled;
        public static bool DimmingLights = true;
        public static bool DimmingWalls = true;
        public static bool IsGameScene;
        public static float Multiplier = 1f;
        public static float SqrtMultiplier = 1f;

        public static void SetPerceivedBrightness(int percent)
        {
            Multiplier = BrightnessCurve.ToMultiplier(percent);
            SqrtMultiplier = UnityEngine.Mathf.Sqrt(Multiplier);
        }

        public static bool LightsActive => Enabled && IsGameScene && DimmingLights && Multiplier < 1f;

        public static bool WallsActive => Enabled && IsGameScene && DimmingWalls && Multiplier < 1f;
    }
}
