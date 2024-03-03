using SwipeMatch3.Gameplay.Interfaces;
using SwipeMatch3.Gameplay.Settings;
using UnityEngine;

namespace SwipeMatch3.Gameplay
{
    /// <summary>
    /// Класс тайла, который можно перемещать
    /// </summary>
    public class MovableTile : TileAbstract, ITileMovable
    {
        private void SetNewSprite(Sprite sprite)
        {
            if (sprite == null)
                return;

            SpriteRenderer.sprite = sprite;
            Image.sprite = sprite;
        }

        private void UpdateAlfaInColor(float alfa)
        {
            color = new Color(color.r, color.g, color.b, alfa);
            SpriteRenderer.color = color;
            Image.color = color;
        }

        public void SetNewSetting(TileSetting newSetting)
        {
            TileSetting = newSetting;
            SetNewSprite(newSetting.Sprite);
            UpdateAlfaInColor(newSetting.Visible ? 255 : 0);
        }

        public override void Interact()
        {
            throw new System.NotImplementedException();
        }

    }
}
