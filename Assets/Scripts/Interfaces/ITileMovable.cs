using SwipeMatch3.Gameplay.Settings;

using UnityEngine;

namespace SwipeMatch3.Gameplay.Interfaces
{
    public interface ITileMovable
    {
        public bool IsInteractable { get; }

        public Color Color { get; }

        public SpriteRenderer SpriteRenderer { get; }

        public TileSetting TileSetting { get; }

        public void SetNewSetting(TileSetting newSetting);

        public void OnStartSwapUpDown();

        public void OnEndSwapUpDown();
    }
}
