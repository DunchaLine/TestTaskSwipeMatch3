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
            Container.Bind<GameplayHandler>().FromComponentInHierarchy().AsSingle();

            Container.BindInterfacesAndSelfTo<TileAbstract>().FromInstance(_tilePrefab).AsSingle();
            Container.BindInterfacesAndSelfTo<RowAbstract>().FromInstance(_rowPrefab).AsSingle();
            Container.BindInterfacesAndSelfTo<BoardAbstract>().FromComponentsInHierarchy().AsSingle().Lazy();
        }
    }
}