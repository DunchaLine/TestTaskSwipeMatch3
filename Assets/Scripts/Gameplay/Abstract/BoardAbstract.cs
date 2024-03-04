using SwipeMatch3.Gameplay.Interfaces;
using SwipeMatch3.Gameplay.Settings;

using System.Collections.Generic;
using System.Linq;

using Unity.Mathematics;
using UnityEngine;

namespace SwipeMatch3.Gameplay
{
    /// <summary>
    /// Абстрактный класс играбельной доски
    /// </summary>
    public abstract class BoardAbstract : MonoBehaviour
    {
        public struct TileInBoard
        {
            public int2 Coordinates { get; private set; }

            public TileAbstract Tile { get; private set; }

            public TileInBoard(int2 coordinates, TileAbstract tile)
            {
                Coordinates = coordinates;
                Tile = tile;
            }
        }

        [SerializeField]
        private BoardSettings _boardSettings;

        public List<RowAbstract> Rows { get; private set; } = new List<RowAbstract>();

        public TileInBoard[] TilesInBoard { get; private set; }

        public bool IsActive { get; private set; } = false;

        public int BoardWidth { get; private set; }

        public int BoardHeight { get; private set; }

        [Zenject.Inject]
        private void Init(RowAbstract row, Zenject.DiContainer container)
        {
            if (_boardSettings == null || _boardSettings.IsCorrect() == false)
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

            BoardWidth = Rows[0].Tiles.Count;
            BoardHeight = Rows.Count;

            SetTilesCoordinates();
        }

        /// <summary>
        /// Установка координат для каждого тайла
        /// </summary>
        private void SetTilesCoordinates()
        {
            TilesInBoard = new TileInBoard[BoardHeight * BoardWidth];
            int i = 0;
            for (int rowIndex = 0; rowIndex < Rows.Count; rowIndex++)
            {
                var row = Rows[rowIndex];
                for (int tileIndex = 0; tileIndex < row.Tiles.Count; tileIndex++)
                {
                    TilesInBoard[i] = new TileInBoard(new int2(rowIndex, tileIndex), row.Tiles[tileIndex]);
                    i++;
                }
            }
        }

        /// <summary>
        /// Получение индекса столбца, в котором находится тайл
        /// </summary>
        /// <param name="tileMovable"></param>
        /// <returns></returns>
        public int GetTileColumnIndex(ITileMovable tileMovable)
        {
            for (int i = 0; i < Rows.Count; i++)
            {
                var tile = Rows[i].GetTile(tileMovable);
                if (tile == null)
                    continue;

                return tile.IndexInRow;
            }

            return -1;
        }

        /// <summary>
        /// Получение тайла по координатам
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public TileInBoard GetTileByCoordinates(int x, int y)
        {
            return TilesInBoard.FirstOrDefault(g => g.Coordinates.Equals(new int2(x, y)));
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
    }
}
