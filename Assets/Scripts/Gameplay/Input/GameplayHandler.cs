using SwipeMatch3.Gameplay.Interfaces;
using SwipeMatch3.Gameplay.Signals;
using System.Collections.Generic;
using UnityEngine;

namespace SwipeMatch3.Gameplay
{
    public class GameplayHandler
    {
        private BoardsHandler _boardsHandler;

        [Zenject.Inject]
        public void Init(BoardsHandler boardsHandler)
        {
            _boardsHandler = boardsHandler;
        }

        // добавить проверочные методы на возможность смены спрайтов только у соседних объектов

        public void SwapSprites(GameSignals.SwapSignal swapSignal)
        {
            var first = swapSignal.FirstSwapElement;
            var second = swapSignal.SecondSwapElement;

            if (IsCorrectSwap(first, second) == false)
                return;

            var firstSetting = first.TileSetting;
            var secondSetting = second.TileSetting;

            first.SetNewSetting(secondSetting);
            second.SetNewSetting(firstSetting);

            // отправить событие на нормализацию board
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
