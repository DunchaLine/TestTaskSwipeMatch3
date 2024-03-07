using UnityEngine;

namespace SwipeMatch3.ObjectPoolingBackground
{
    [RequireComponent(typeof(SpriteRenderer), typeof(Rigidbody2D))]
    public abstract class BackgroundObject : MonoBehaviour
    {
        public RectTransform RectTransform { get; private set; }

        public SpriteRenderer _backgroundObjRenderer { get; private set; }

        protected BackgroundObjectPool ObjectPool { get; private set; }

        private bool IsInit { get; set; } = false;

        public abstract void SetBehaviour(Vector2 destinationPoint);

        public void SetParent(RectTransform rect)
        {
            if (transform.parent != null && transform.parent.Equals(rect))
                return;

            transform.SetParent(rect);
        }

        public virtual void ResetTransform()
        {
            if (IsInit == false)
                Init();

            RectTransform.anchoredPosition = Vector3.zero;
            transform.position = Vector3.zero;
            transform.localScale = new Vector3(.2f, .2f, .2f);
        }

        [Zenject.Inject]
        private void Init()
        {
            _backgroundObjRenderer = GetComponent<SpriteRenderer>();
            RectTransform = GetComponent<RectTransform>();

            IsInit = true;
        }

        private void Awake()
        {
            if (IsInit == false)
                Init();
        }

        public void SetSprite(Sprite sprite)
        {
            if (_backgroundObjRenderer == null)
                _backgroundObjRenderer = GetComponent<SpriteRenderer>();

            _backgroundObjRenderer.sprite = sprite;
        }
    }
}
