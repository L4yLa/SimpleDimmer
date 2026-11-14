using System;
using SimpleDimmer.Configuration;
using SimpleDimmer.Patches;
using Zenject;

namespace SimpleDimmer.Installers
{
    public class GameCoreInstaller : Installer
    {
        public override void InstallBindings()
        {
            // ライトの初期色が設定される前に有効にするため、バインド時点で切り替える
            DimmerState.IsGameScene = true;
            Container.BindInterfacesTo<GameSceneDeactivator>().AsSingle().NonLazy();
        }
    }

    internal class GameSceneDeactivator : IDisposable
    {
        public void Dispose()
        {
            DimmerState.IsGameScene = false;
            TubeRegistry.RestoreAndClear();
            WallFramePatch.Clear();
        }
    }
}
