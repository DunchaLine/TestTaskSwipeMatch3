using SwipeMatch3.Gameplay.Settings;
using UnityEngine;
using UnityEngine.UI;

namespace SwipeMatch3.Gameplay
{
    /// <summary>
    /// Абстрактный класс тайла
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer), typeof(Image))]
    public abstract class TileAbstract : MonoBehaviour
    {
        public bool IsInteractable => SpriteRenderer.color.a > 0 && Image.color.a > 0;

        public SpriteRenderer SpriteRenderer { get; private set; }

        public Image Image { get; private set; }

        public int IndexInRow { get; private set; }

        public Color Color => color;

        public TileSetting TileSetting { get; protected set; }

        protected Color color;

        public void Init(int index, TileSetting tileSetting)
        {
            Image = GetComponent<Image>();
            SpriteRenderer = GetComponent<SpriteRenderer>();
            if (tileSetting == null)
                tileSetting = (TileSetting) ScriptableObject.CreateInstance(typeof(TileSetting));
            TileSetting = tileSetting;
            color = Image.color;

            if (tileSetting.Sprite != null && tileSetting.Visible)
            {
                Image.sprite = tileSetting.Sprite;
                SpriteRenderer.sprite = tileSetting.Sprite;
            }
            else
            {
                color.a = 0f;

                Image.color = color;
                SpriteRenderer.color = color;
            }

            IndexInRow = index;
        }

        public abstract void Interact();
    }
}
