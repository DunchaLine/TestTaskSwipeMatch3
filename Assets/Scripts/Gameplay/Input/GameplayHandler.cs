using SwipeMatch3.Gameplay.Interfaces;
using SwipeMatch3.Gameplay.Signals;

using Zenject;

namespace SwipeMatch3.Gameplay
{
    public class GameplayHandler
    {
        private BoardsHandler _boardsHandler;

        private SignalBus _signalBus;

        [Inject]
        public void Init(SignalBus signalBus, BoardsHandler boardsHandler)
        {
            _signalBus = signalBus;
            _boardsHandler = boardsHandler;
        }

        /// <summary>
        /// Свайп тайлов сверху вниз
        /// </summary>
        /// <param name="swapUpDownSignal"></param>
        public void SwapSpritesUpToDown(GameSignals.SwapSpritesUpDownSignal swapUpDownSignal)
        {
            ITileMovable downElement = swapUpDownSignal.DownElement;
            ITileMovable upElement = swapUpDownSignal.UpElement;

            if (downElement == null || upElement == null)
                return;

            if (downElement.TileSetting.Visible && upElement.TileSetting.Visible)
                return;

            Swap(downElement, upElement);
        }

        /// <summary>
        /// Свап спрайтов
        /// </summary>
        /// <param name="swapSignal"></param>
        public void SwapSprites(GameSignals.SwapSignal swapSignal)
        {
            ITileMovable first = swapSignal.FirstSwapElement;
            ITileMovable second = swapSignal.SecondSwapElement;

            if (IsCorrectSwap(first, second) == false)
                return;

            Swap(first, second);

            int firstTileIndex = _boardsHandler.GetTileColumnIndex(first);
            int secondTileIndex = _boardsHandler.GetTileColumnIndex(second);

            _signalBus.Fire(new GameSignals.NormalizeTilesOnBoardSignal(
                new System.Collections.Generic.List<int> { firstTileIndex, secondTileIndex }));
        }

        /// <summary>
        /// Свап тайлов
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        private void Swap(ITileMovable first, ITileMovable second)
        {
            var firstSetting = first.TileSetting;
            var secondSetting = second.TileSetting;

            first.SetNewSetting(secondSetting);
            second.SetNewSetting(firstSetting);
        }

        /// <summary>
        /// Корректный ли свап (свап может быть только с видимым и невидимым; только для соседних тайлов)
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        private bool IsCorrectSwap(ITileMovable first, ITileMovable second)
        {
            // нельзя сдвинуть объекты, если первый из них - невидимый
            if (first.TileSetting.Visible == false || second == null)
                return false;

            // нельзя взаимодействовать с блоками, которые на данный момент не являются интерактивными
            if (first.IsInteractable == false || second.IsInteractable == false)
                return false;

            return _boardsHandler.IsTilesNeighbors(first, second);
        }
    }
}
