using SwipeMatch3.Gameplay.Signals;

using UnityEngine;

using Zenject;

namespace SwipeMatch3.Gameplay.Installers
{
    /// <summary>
    /// Installer игрового пространства
    /// </summary>
    public class GameplayInstaller : MonoInstaller
    {
        [SerializeField]
        private TileAbstract _tilePrefab;

        [SerializeField]
        private RowAbstract _rowPrefab;

        public override void InstallBindings()
        {
            SignalBusInstaller.Install(Container);

            // сигналы
            Container.DeclareSignal<GameSignals.SwapSignal>();
            Container.DeclareSignal<GameSignals.ChangeBoardSignal>();
            Container.DeclareSignal<GameSignals.NormalizeTilesOnBoardSignal>();
            Container.DeclareSignal<GameSignals.SwapSpritesUpDownSignal>();
            Container.DeclareSignal<GameSignals.OnSwappingSpritesUpDownSignal>();

            // установка сигналов в определенные методы
            Container.BindInterfacesAndSelfTo<GameplayHandler>().AsSingle();
            Container.BindSignal<GameSignals.SwapSignal>().ToMethod<GameplayHandler>(g => g.SwapSprites).FromResolve();
            Container.BindSignal<GameSignals.SwapSpritesUpDownSignal>().ToMethod<GameplayHandler>(g => g.SwapSpritesUpToDown).FromResolve();

            Container.BindInterfacesAndSelfTo<BoardsHandler>().AsSingle();

            Container.BindInterfacesAndSelfTo<TileAbstract>().FromInstance(_tilePrefab).AsSingle();
            Container.BindInterfacesAndSelfTo<RowAbstract>().FromInstance(_rowPrefab).AsSingle();
            Container.BindInterfacesAndSelfTo<BoardAbstract>().FromComponentsInHierarchy().AsSingle().Lazy();
        }
    }
}