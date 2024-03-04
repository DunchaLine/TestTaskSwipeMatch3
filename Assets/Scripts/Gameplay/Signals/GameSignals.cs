using SwipeMatch3.Gameplay.Interfaces;

using System.Collections.Generic;

namespace SwipeMatch3.Gameplay.Signals
{
    public class GameSignals
    {
        /// <summary>
        /// Сигнал на свап
        /// </summary>
        public class SwapSignal
        {
            public readonly ITileMovable FirstSwapElement;
            public readonly ITileMovable SecondSwapElement;

            public SwapSignal(ITileMovable first, ITileMovable second)
            {
                FirstSwapElement = first;
                SecondSwapElement = second;
            }
        }

        /// <summary>
        /// Сигнал на смену доски
        /// </summary>
        public class ChangeBoardSignal
        {

        }

        /// <summary>
        /// Сигнал на нормализацию поля
        /// </summary>
        public class NormalizeTilesOnBoardSignal
        {
            public readonly List<int> ColumnsIndexes;

            public NormalizeTilesOnBoardSignal(List<int> columnsIndexes)
            {
                ColumnsIndexes = columnsIndexes;
            }
        }

        /// <summary>
        /// Сигнал на окончание свапа верхнего/нижнего тайла
        /// </summary>
        public class OnSwappingSpritesUpDownSignal
        {

        }

        public class SwapSpritesUpDownSignal
        {
            public readonly ITileMovable UpElement;
            public readonly ITileMovable DownElement;

            public SwapSpritesUpDownSignal(ITileMovable up, ITileMovable down)
            {
                UpElement = up;
                DownElement = down;
            }
        }
    }
}
