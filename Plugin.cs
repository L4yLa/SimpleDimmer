using IPA;
using IPA.Config;
using IPA.Config.Stores;
using SiraUtil.Zenject;
using SimpleDimmer.Configuration;
using SimpleDimmer.Installers;
using SimpleDimmer.Patches;
using HarmonyLib;
using IPALogger = IPA.Logging.Logger;

namespace SimpleDimmer
{
    [Plugin(RuntimeOptions.DynamicInit)]
    public class Plugin
    {
        internal static IPALogger Log { get; private set; } = null!;
        private Harmony? _harmony;

        [Init]
        public Plugin(IPALogger logger, Config conf, Zenjector zenjector)
        {
            Log = logger;
            PluginConfig.Instance = conf.Generated<PluginConfig>();
            zenjector.Install<MenuInstaller>(Location.Menu);
        }

        [OnEnable]
        public void OnEnable()
        {
            _harmony = new Harmony("com.l4yla.beatsaber.simpledimmer");
            _harmony.PatchAll(typeof(LightDimmerPatch).Assembly);
        }

        [OnDisable]
        public void OnDisable()
        {
            _harmony?.UnpatchSelf();
            _harmony = null;
        }
    }
}
