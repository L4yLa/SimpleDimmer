using System.Runtime.CompilerServices;
using IPA.Config.Stores;

[assembly: InternalsVisibleTo(GeneratedStore.AssemblyVisibilityTarget)]
namespace SimpleDimmer.Configuration
{
    public class PluginConfig
    {
        public static PluginConfig Instance { get; set; } = null!;

        // IPA による自動ロード・保存用（virtual）
        public virtual bool Enabled { get; set; } = false;
        public virtual float Brightness { get; set; } = 1.0f;
        // Step index: 0=0.1, 1=0.05, 2=0.01
        public virtual int StepIndex { get; set; } = 0;

        // 実行時キャッシュ（非virtual、UI/Patch から参照）
        public bool RuntimeEnabled;
        public float RuntimeBrightness;
        public int RuntimeStepIndex;
    }
}
