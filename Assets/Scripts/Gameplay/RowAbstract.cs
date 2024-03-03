using System.Collections.Generic;
using UnityEngine;

using Zenject;

namespace SwipeMatch3.Gameplay
{
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
        
        public void Init(int indexInBoard, int tilesCount)
        {
            if (Tile == null)
                return;

            IndexInBoard = indexInBoard;
            for (int i = 0; i < tilesCount; i++)
            {
                TileAbstract newTile = _container.InstantiatePrefabForComponent<TileAbstract>(Tile, transform);
                newTile.Init(i);
                Tiles.Add(newTile);
            }
        }

        public abstract Tile GetLeftTile();

        public abstract Tile GetRightTile();

    }
}
