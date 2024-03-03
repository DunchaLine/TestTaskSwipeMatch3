using UnityEngine;

namespace SwipeMatch3.ObjectPoolingBackground
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class MovableBackgroundObject : BackgroundObject
    {
        private Rigidbody2D _rigidBody2D;

        private Vector2 _direction;

        private Vector2 _orthogonal;

        private MovableObjectSettings _movableSettings;

        private float _speed;

        private float _startTime;

        [Zenject.Inject]
        private void Init(MovableObjectSettings movableSettings)
        {
            _movableSettings = movableSettings;
        }

        private void Awake()
        {
            _rigidBody2D = GetComponent<Rigidbody2D>();
        }

        public override void SetBehaviour(Vector2 destinationPoint)
        {
            // TODO: вызывается 3 раза для каждого из объекта, пофиксить
            _direction = (destinationPoint - new Vector2(transform.position.x, transform.position.y)).normalized;
            _orthogonal = new Vector2(_direction.y, _direction.x);
            _speed = Random.Range(_movableSettings.MinSpeed, _movableSettings.MaxSpeed);
            _startTime = Time.time;
            //Debug.Log($"direction: {_direction} with speed: {_speed}");
        }

        private void FixedUpdate()
        {
            float t = Time.time - _startTime;
            _rigidBody2D.velocity = _direction * _speed + _orthogonal * _movableSettings.Magnitude * Mathf.Sin(_movableSettings.Frequency * t);
        }
    }
}
