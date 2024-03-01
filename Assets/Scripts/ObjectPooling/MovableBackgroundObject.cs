using UnityEngine;

namespace SwipeMatch3.ObjectPoolingBackground
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class MovableBackgroundObject : BackgroundObject
    {
        private Rigidbody2D _rigidBody2D;

        private void Awake()
        {
            _rigidBody2D = GetComponent<Rigidbody2D>();
        }

        // прописать абстрактный метод, который будет отвечать за инициализацию при повторном спавне
        // в данном случае, он будет "толкать объект при помощи Rigidbody2D"

        protected override void OnEnable()
        {

        }

        protected override void OnDisable()
        {

        }
    }
}
