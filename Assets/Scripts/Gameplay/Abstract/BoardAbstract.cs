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

        public bool IsActive { get; private set; } = false;

        [Zenject.Inject]
        private void Init(RowAbstract row, Zenject.DiContainer container)
        {
            if (IsCorrectSettings() == false)
            {
                Debug.LogError($"Board: {name} has incorrect settings: {_boardSettings.name}");
                return;
            }

            for (int i = 0; i < _boardSettings.Rows.Length; i++)
            {
                RowAbstract newRow = container.InstantiatePrefabForComponent<RowAbstract>(row, transform);
                if (newRow == null)
                    continue;
                newRow.Init(i, _boardSettings.Rows[i].TilesInRow);
                Rows.Add(newRow);
            }
        }

        private bool IsCorrectSettings()
        {
            if (_boardSettings == null || _boardSettings.IsCorrect() == false)
                return false;

            var rowsInSettings = _boardSettings.Rows;
            if (rowsInSettings.Length == 1)
                return true;

            int firstRowTileCount = rowsInSettings[0].TilesInRow.Length;
            for (int i = 1; i < rowsInSettings.Length; i++)
            {
                if (firstRowTileCount != rowsInSettings[i].TilesInRow.Length)
                    return false;
            }

            return true;
        }

        public void SetBoardActive()
        {
            IsActive = true;
            transform.gameObject.SetActive(true);
        }

        public void SetBoardInactive()
        {
            IsActive = false;
            transform.gameObject.SetActive(false);
        }

        public abstract RowAbstract GetUpRow(int index);

        public abstract RowAbstract GetDownRow(int index);
    }
}
