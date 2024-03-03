using UnityEngine;
using UnityEngine.UI;

namespace SwipeMatch3.Gameplay
{
    [RequireComponent(typeof(SpriteRenderer), typeof(Image))]
    public abstract class TileAbstract : MonoBehaviour
    {
        public bool IsInteractable => SpriteRenderer.color.a > 0 && Image.color.a > 0;

        public SpriteRenderer SpriteRenderer { get; private set; }

        public Image Image { get; private set; }

        public int IndexInRow { get; private set; }

        public void Init(int index)
        {
            Image = GetComponent<Image>();
            SpriteRenderer = GetComponent<SpriteRenderer>();
            IndexInRow = index;
        }

        public abstract void SwitchSprite(Sprite sprite);

        public abstract void Interact();
    }
}
