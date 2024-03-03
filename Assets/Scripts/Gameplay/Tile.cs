using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SwipeMatch3.Gameplay
{
    public class Tile : TileAbstract
    {
        public override void Interact()
        {
            throw new System.NotImplementedException();
        }

        public override void SwitchSprite(Sprite sprite)
        {
            Image.sprite = sprite;
            SpriteRenderer.sprite = sprite;
        }
    }
}
