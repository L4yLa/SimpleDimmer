using SimpleDimmer.Configuration;
using Zenject;

namespace SimpleDimmer.Installers
{
    public class GameCoreInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<GameSceneActivator>().AsSingle().NonLazy();
        }
    }

    internal class GameSceneActivator : IInitializable, System.IDisposable
    {
        public void Initialize()
        {
            PluginConfig.Instance.RuntimeIsGameScene = true;
        }

        public void Dispose()
        {
            PluginConfig.Instance.RuntimeIsGameScene = false;
        }
    }
}
