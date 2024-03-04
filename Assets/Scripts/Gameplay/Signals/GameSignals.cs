using SwipeMatch3.Gameplay.Interfaces;

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
    }
}
