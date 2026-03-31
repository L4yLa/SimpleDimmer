using SimpleDimmer.UI;
using Zenject;

namespace SimpleDimmer.Installers
{
    public class MenuInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<DimmerViewController>().AsSingle().NonLazy();
        }
    }
}
