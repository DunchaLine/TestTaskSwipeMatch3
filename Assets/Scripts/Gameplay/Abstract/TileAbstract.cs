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

        public void Init(int index, Sprite sprite)
        {
            Image = GetComponent<Image>();
            SpriteRenderer = GetComponent<SpriteRenderer>();
            if (sprite != null)
            {
                Image.sprite = sprite;
                SpriteRenderer.sprite = sprite;
            }
            else
            {
                Color color = Image.color;
                color.a = 0f;
                Image.color = color;
                SpriteRenderer.color = color;
            }

            IndexInRow = index;
        }

        public abstract void SwitchSprite(Sprite sprite);

        public abstract void Interact();
    }
}
