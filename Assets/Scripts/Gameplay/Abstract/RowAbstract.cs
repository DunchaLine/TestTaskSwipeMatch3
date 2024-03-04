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

        public TileAbstract GetTile(ITileMovable movableTile)
        {
            foreach (var tile in Tiles)
            {
                var tileMovable = tile as ITileMovable;
                if (tileMovable != null && movableTile == tileMovable)
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

        public abstract MovableTile GetLeftTile();

        public abstract MovableTile GetRightTile();

    }
}
