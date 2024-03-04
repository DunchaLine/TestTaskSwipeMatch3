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

        public void SwapSpritesUpToDown(GameSignals.SwapSpritesUpDownSignal swapUpDownSignal)
        {
            var downElement = swapUpDownSignal.DownElement;
            var upElement = swapUpDownSignal.UpElement;

            if (downElement == null || upElement == null)
                return;

            if (downElement.TileSetting.Visible && upElement.TileSetting.Visible)
                return;

            Swap(downElement, upElement);
        }

        public void SwapSprites(GameSignals.SwapSignal swapSignal)
        {
            var first = swapSignal.FirstSwapElement;
            var second = swapSignal.SecondSwapElement;

            if (IsCorrectSwap(first, second) == false)
                return;

            Swap(first, second);

            // отправлять сигнал только для тех столбцов, в которых произошли изменения
            // как-то получить индексы стобцов, в которых произошли изменения и отправить их, 
            // изменив NormalizeTilesOnBoardSignal, добавив в него List<int>. 
            // в BoardsHandler.NormalizeTilesOnBoard тоже добавить List<int> и проходить не по всем
            // а только по измененным
            _signalBus.Fire<GameSignals.NormalizeTilesOnBoardSignal>();
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

        private bool IsCorrectSwap(ITileMovable first, ITileMovable second)
        {
            // нельзя сдвинуть объекты, если первый из них - невидимый
            if (first.TileSetting.Visible == false)
                return false;

            // нельзя взаимодействовать с блоками, которые на данный момент не являются интерактивными
            if (first.IsInteractable == false || second.IsInteractable == false)
                return false;

            return _boardsHandler.IsTilesNeighbors(first, second);
        }
    }
}
