using Cysharp.Threading.Tasks;
using SwipeMatch3.ObjectPoolingBackground;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Zenject;

namespace SwipeMatch3.Gameplay
{
    public class BackgroundObjectPoolingHandler : MonoBehaviour
    {
        [SerializeField]
        private RectTransform _backgroundObjectCanvasRect;

        private BackgroundObjectPool.Pool Pool { get; set; }

        private List<BackgroundObject> BackgroundObjects { get; set; } = new List<BackgroundObject>();

        [Inject]
        public async UniTask Init(BackgroundObjectPool.Pool pool)
        {
            Pool = pool;
            var token = this.GetCancellationTokenOnDestroy();
            var isSpawned = false;
            while (Pool.NumInactive > 0 || isSpawned == false)
                isSpawned = await SpawnObjectInPoolTask(token);

            CheckObjectsInPoolToDeactivateTask(token).Forget();
        }

        /// <summary>
        /// Таска, отвечающая за проверку на необходимость отключать объекты и возвращать их в пул
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        private async UniTask CheckObjectsInPoolToDeactivateTask(CancellationToken token)
        {
            while (true)
            {
                token.ThrowIfCancellationRequested();

                if (Pool.NumActive == 0)
                    continue;

                foreach (var backgroundObject in BackgroundObjects)
                {
                    if (backgroundObject._backgroundObjRenderer.isVisible == false)
                    {
                        Pool.Despawn(backgroundObject);
                        await SpawnObjectInPoolTask(token);
                    }
                }

                await UniTask.NextFrame();
            }
        }

        /// <summary>
        /// Таска, отвечающая за спавн объекта из пула
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        private async UniTask<bool> SpawnObjectInPoolTask(CancellationToken token)
        {
            while (Pool.NumInactive == 0)
            {
                await UniTask.NextFrame();
                if (token.IsCancellationRequested)
                    return false;
            }

            var spawnedObj = Pool.Spawn(_backgroundObjectCanvasRect);
            if (BackgroundObjects.Contains(spawnedObj) == false)
                BackgroundObjects.Add(spawnedObj);

            spawnedObj.SetParent(_backgroundObjectCanvasRect);
            return true;
        }
    }
}
