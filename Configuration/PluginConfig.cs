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

        // 実行時キャッシュ（非virtual、UI/Patch から参照）
        public bool RuntimeEnabled;
        public float RuntimeBrightness;
    }
}