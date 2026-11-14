using System.Runtime.CompilerServices;
using IPA.Config.Stores;

[assembly: InternalsVisibleTo(GeneratedStore.AssemblyVisibilityTarget)]
namespace SimpleDimmer.Configuration
{
    public class PluginConfig
    {
        public static PluginConfig Instance { get; set; } = null!;

        public virtual bool Enabled { get; set; } = false;

        // 体感の明るさ（%）。実際の倍率は BrightnessCurve で求める
        public virtual int PerceivedBrightness { get; set; } = 100;

        public virtual bool DimmingLights { get; set; } = true;
        public virtual bool DimmingWalls { get; set; } = true;
    }
}
