using SwipeMatch3.ObjectPoolingBackground;
using UnityEngine;
using Zenject;

namespace SwipeMatch3.Gameplay
{
    public class GameInstaller : MonoInstaller
    {
        [SerializeField]
        private BackgroundObject poolObjectPrefab;

        [SerializeField] [Range(1, 10)]
        private int poolLimit = 3;

        public override void InstallBindings()
        {
            Container.BindMemoryPool<MovableBackgroundObject, BackgroundObjectPool.Pool>()
                .WithInitialSize(poolLimit)
                .FromComponentInNewPrefab(poolObjectPrefab).NonLazy();
        }
    }
}