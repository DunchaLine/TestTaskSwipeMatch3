using SwipeMatch3.Gameplay.Interfaces;
using UnityEngine;

namespace SwipeMatch3.Gameplay
{
    public class GameplayHandler : MonoBehaviour
    {
        // добавить проверочные методы на возможность смены спрайтов только у соседних объектов

        public void SwapSprites(ITileMovable first, ITileMovable second)
        {
            if (first.TileSetting.Visible == false)
                return;

            var firstSetting = first.TileSetting;
            var secondSetting = second.TileSetting;

            first.SetNewSetting(secondSetting);
            second.SetNewSetting(firstSetting);
        }
    }
}
