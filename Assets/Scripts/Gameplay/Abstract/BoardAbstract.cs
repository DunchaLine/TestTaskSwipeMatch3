using SwipeMatch3.Gameplay.Settings;
using System.Collections.Generic;
using UnityEngine;

namespace SwipeMatch3.Gameplay
{
    /// <summary>
    /// Абстрактный класс играбельной доски
    /// </summary>
    public abstract class BoardAbstract : MonoBehaviour
    {
        [SerializeField]
        private BoardSettings _boardSettings;

        public List<RowAbstract> Rows { get; private set; } = new List<RowAbstract>();

        [Zenject.Inject]
        private void Init(RowAbstract row, Zenject.DiContainer container)
        {
            for (int i = 0; i < _boardSettings.Rows.Length; i++)
            {
                RowAbstract newRow = container.InstantiatePrefabForComponent<RowAbstract>(row, transform);
                if (newRow == null)
                    continue;
                newRow.Init(i, _boardSettings.Rows[i].TilesInRow);
                Rows.Add(newRow);
            }
        }

        public abstract RowAbstract GetUpRow(int index);

        public abstract RowAbstract GetDownRow(int index);
    }
}
