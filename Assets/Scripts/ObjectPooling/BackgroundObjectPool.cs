using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace SwipeMatch3.ObjectPoolingBackground
{
    /// <summary>
    /// Пул объектов бэкграунда
    /// </summary>
    public class BackgroundObjectPool
    {
        public class Pool : MonoMemoryPool<RectTransform, BackgroundObject>
        {
            private List<Sprite> _sprites;

            [Inject]
            public void Init(IBackgroundPoolSettings settings)
            {
                _sprites = settings.Sprites;
            }

            /// <summary>
            /// Вызывается один раз, при создании
            /// </summary>
            /// <param name="item"></param>
            protected override void OnCreated(BackgroundObject item)
            {
                base.OnCreated(item);
                item.ResetTransform();
            }

            /// <summary>
            /// Вызывается каждый раз при выдаче объекта из пула
            /// </summary>
            /// <param name="item"></param>
            protected override void OnSpawned(BackgroundObject item)
            {
                item.gameObject.SetActive(true);
                item.SetSprite(GetRangomSprite());
            }

            /// <summary>
            /// Вызывается сразу же после возврата объекта в пул
            /// </summary>
            /// <param name="item"></param>
            protected override void OnDespawned(BackgroundObject item)
            {
                item.gameObject.SetActive(false);
            }

            /// <summary>
            /// Вызывается каждый раз при выдаче объекта из пула
            /// В отличие от OnSpawned, содержит параметры, которые передаются при инициализации пула
            /// </summary>
            /// <param name="p1"></param>
            /// <param name="item"></param>
            protected override void Reinitialize(RectTransform rect, BackgroundObject item)
            {
                item.SetParent(rect);
                item.transform.localPosition = GetRandomPosition(rect, out bool isLeft);
                item.SetBehaviour(GetDestinationPoint(rect, isLeft));
            }

            /// <summary>
            /// Получение случайной позиции в пределах верхней половины Rect для y
            /// и левой, либо правой позиции для x
            /// </summary>
            /// <param name="rect"></param>
            /// <returns></returns>
            private Vector3 GetRandomPosition(RectTransform rect, out bool isLeft)
            {
                int random = Random.Range(0, 2);
                float y = Random.Range(0f, rect.rect.height / 2);
                int xNat = random == 0 ? -1 : 1;

                isLeft = xNat == -1;

                return new Vector3(rect.rect.width / 2 * xNat, y, -1f);
            }

            /// <summary>
            /// Получение конечной точки по rect в зависимости от isLeft
            /// </summary>
            /// <param name="rect"></param>
            /// <param name="isLeft"></param>
            /// <returns></returns>
            private Vector2 GetDestinationPoint(RectTransform rect, bool isLeft)
            {
                float y = Random.Range(0f, rect.rect.height / 2);
                float x = rect.rect.width / 2;
                if (isLeft == false)
                    x *= -1;

                return new Vector2(x, y);
            }

            /// <summary>
            /// Получение случайного спрайта из списка
            /// </summary>
            /// <returns></returns>
            private Sprite GetRangomSprite()
            {
                return _sprites[Random.Range(0, _sprites.Count)];
            }
        }
    }
}
