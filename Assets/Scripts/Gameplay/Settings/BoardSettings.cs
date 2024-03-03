using System;
using UnityEngine;

namespace SwipeMatch3.Gameplay.Settings
{
    /// <summary>
    /// Настройка варианта играбельной доски
    /// </summary>
    [CreateAssetMenu(fileName = "BoardSettings", menuName = "Board/BoardScriptableSettings")]
    public class BoardSettings : ScriptableObject
    {
        [field: SerializeField]
        public Row[] Rows { get; private set; }

        [Serializable]
        public class Row
        {
            public TileSetting[] TilesInRow;
        }
    }
}
