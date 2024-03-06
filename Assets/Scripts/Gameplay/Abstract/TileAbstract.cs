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
        public SpriteRenderer SpriteRenderer { get; private set; }

        public Image Image { get; private set; }

        public int IndexInRow { get; private set; }

        public Color Color => color;

        public TileSetting TileSetting { get; protected set; }

        protected Color color;

        private bool isInited = false;

        public virtual void Init(int index, TileSetting tileSetting)
        {
            if (isInited == false)
                SettingTile();
            
            if (tileSetting == null)
                tileSetting = (TileSetting) ScriptableObject.CreateInstance(typeof(TileSetting));
            TileSetting = tileSetting;

            if (tileSetting.Sprite != null && tileSetting.Visible)
            {
                Image.sprite = tileSetting.Sprite;
                SpriteRenderer.sprite = tileSetting.Sprite;
                color.a = 255f;
            }
            else
            {
                color.a = 0f;
                TileSetting = (TileSetting)ScriptableObject.CreateInstance(typeof(TileSetting));
            }

            Image.color = color;
            SpriteRenderer.color = color;

            IndexInRow = index;
        }

        /// <summary>
        /// Установка настроек для тайла
        /// </summary>
        private void SettingTile()
        {
            Image = GetComponent<Image>();
            SpriteRenderer = GetComponent<SpriteRenderer>();
            color = Image.color;
            isInited = true;
        }
    }
}
