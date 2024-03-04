using SwipeMatch3.Gameplay.Interfaces;
using SwipeMatch3.Gameplay.Settings;

using System.Collections.Generic;

using UnityEngine;

using Zenject;

namespace SwipeMatch3.Gameplay
{
    /// <summary>
    /// Абстрактный класс строки со списком тайлов
    /// </summary>
    public abstract class RowAbstract : MonoBehaviour
    {
        public List<TileAbstract> Tiles { get; private set; } = new List<TileAbstract>();

        public int IndexInBoard { get; private set; }

        private TileAbstract Tile;

        private DiContainer _container;

        [Inject]
        private void Init(TileAbstract tile, DiContainer container)
        {
            Tile = tile;
            _container = container;
        }
        
        public void Init(int indexInBoard, TileSetting[] tilesInRow)
        {
            if (Tile == null)
                return;

            IndexInBoard = indexInBoard;
            for (int i = 0; i < tilesInRow.Length; i++)
            {
                TileAbstract newTile = _container.InstantiatePrefabForComponent<TileAbstract>(Tile, transform);
                if (newTile == null)
                    continue;

                newTile.Init(i, tilesInRow[i]);
                Tiles.Add(newTile);
            }
        }

        /// <summary>
        /// Равен ли tile передаваемому tileMovable
        /// </summary>
        /// <param name="tile"></param>
        /// <param name="tileMovable"></param>
        /// <returns></returns>
        private bool IsITileMovableEquals(TileAbstract tile, ITileMovable tileMovable)
        {
            return tile != null && tileMovable == tile as ITileMovable;
        }

        /// <summary>
        /// Получение тайла по ITileMovable
        /// </summary>
        /// <param name="movableTile"></param>
        /// <returns></returns>
        public TileAbstract GetTile(ITileMovable movableTile)
        {
            foreach (var tile in Tiles)
            {
                if (IsITileMovableEquals(tile, movableTile))
                    return tile;
            }

            return null;
        }

        public TileAbstract GetTile(int index)
        {
            if (index >= Tiles.Count)
                return null;
            return Tiles[index];
        }
    }
}
