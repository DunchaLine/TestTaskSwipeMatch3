using SwipeMatch3.Gameplay.Interfaces;
using SwipeMatch3.Gameplay.Settings;
using SwipeMatch3.Gameplay.Signals;

using UnityEngine;

using Zenject;

namespace SwipeMatch3.Gameplay
{
    /// <summary>
    /// Класс тайла, который можно перемещать
    /// </summary>
    public class MovableTile : TileAbstract, ITileMovable, ITileDestroyable
    {
        public bool IsInteractable { get; private set; }

        private SignalBus _signalBus;

        [Inject]
        public void Init(SignalBus signalBus)
        {
            _signalBus = signalBus;
        }

        public override void Init(int index, TileSetting tileSetting)
        {
            base.Init(index, tileSetting);
            IsInteractable = TileSetting.IsInteractable;
        }

        /// <summary>
        /// Перед началом Swap вверх/вниз
        /// </summary>
        public void OnStartSwapUpDown()
        {
            Debug.Log($"subscribing up down tile: {TileSetting.TileName}");
            _signalBus.Subscribe<GameSignals.OnSwappingSpritesUpDownSignal>(ChangeInteractable);
        }

        /// <summary>
        /// По окончании Swap вверх/вниз
        /// </summary>
        public void OnEndSwapUpDown()
        {
            _signalBus.TryUnsubscribe<GameSignals.OnSwappingSpritesUpDownSignal>(ChangeInteractable);
            IsInteractable = true;
        }


        private void ChangeInteractable()
        {
            IsInteractable = false;
        }

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

        public void Destroy()
        {
            if (TileSetting.Visible == false)
                return;

            TileSetting tileSetting = (TileSetting) ScriptableObject.CreateInstance(typeof(TileSetting));
            SetNewSetting(tileSetting);
        }
    }
}
