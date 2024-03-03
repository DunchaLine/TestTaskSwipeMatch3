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

        public abstract MovableTile GetLeftTile();

        public abstract MovableTile GetRightTile();

    }
}
