using System.Collections.Generic;
using UnityEngine;

namespace SwipeMatch3.Gameplay
{
    public abstract class BoardAbstract : MonoBehaviour
    {
        [field: SerializeField, Range(6, 10)]
        public int RowsLimit { get; private set; } = 6;

        [field: SerializeField, Range(3, 6)]
        public int TilesLimit { get; private set; } = 4;

        public List<RowAbstract> Rows { get; private set; } = new List<RowAbstract>();

        [Zenject.Inject]
        private void Init(RowAbstract row, Zenject.DiContainer container)
        {
            for (int i = 0; i < RowsLimit; i++)
            {
                RowAbstract newRow = container.InstantiatePrefabForComponent<RowAbstract>(row, transform);
                newRow.Init(i, TilesLimit);
                Rows.Add(newRow);
            }
        }

        public abstract RowAbstract GetUpRow(int index);

        public abstract RowAbstract GetDownRow(int index);
    }
}
