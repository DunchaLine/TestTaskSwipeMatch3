using SwipeMatch3.Gameplay.Signals;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Zenject;

namespace SwipeMatch3.UI
{
    public class MainUIHandler : MonoBehaviour
    {
        private SignalBus _signalBus;

        [Inject]
        private void Init(SignalBus signalBus)
        {
            _signalBus = signalBus;
        }

        public void GoToNextBoard()
        {
            _signalBus.Fire<GameSignals.ChangeBoardSignal>();
        }

        public void RestartBoard()
        {
            _signalBus.Fire<GameSignals.ReInitBoardSignal>();
        }
    }
}
