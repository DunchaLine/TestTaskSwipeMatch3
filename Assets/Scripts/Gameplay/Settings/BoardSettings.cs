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

        public bool IsCorrect()
        {
            if (Rows == null || Rows.Length == 0)
                return false;

            if (Rows.Length > 1)
            {
                int firstRowTileCount = Rows[0].TilesInRow.Length;
                for (int i = 1; i < Rows.Length; i++)
                {
                    if (firstRowTileCount != Rows[i].TilesInRow.Length)
                        return false;
                }
            }

            return true;
        }
    }
}
