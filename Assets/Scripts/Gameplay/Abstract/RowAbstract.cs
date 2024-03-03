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
        
        public void Init(int indexInBoard, Sprite[] spritesInRow)
        {
            if (Tile == null)
                return;

            IndexInBoard = indexInBoard;
            for (int i = 0; i < spritesInRow.Length; i++)
            {
                TileAbstract newTile = _container.InstantiatePrefabForComponent<TileAbstract>(Tile, transform);
                if (newTile == null)
                    continue;

                newTile.Init(i, spritesInRow[i]);
                Tiles.Add(newTile);
            }
        }

        public abstract Tile GetLeftTile();

        public abstract Tile GetRightTile();

    }
}
