using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using static SwipeMatch3.Gameplay.BoardAbstract;

namespace SwipeMatch3.Gameplay
{
    public class MatchesCalculator
    {
        public class MatchInfo
        {
            public readonly int startX;

            public readonly int startY;

            public readonly int count;

            public readonly bool isHorizontal;

            public MatchInfo(int startX, int startY, int count, bool isHorizontal)
            {
                this.startX = startX;
                this.startY = startY;
                this.count = count;
                this.isHorizontal = isHorizontal;
            }
        }

        private readonly TileInBoard[] _tilesInBoard;
        private readonly int _boardHeight;
        private readonly int _boardWidth;
        private readonly BoardAbstract _board;

        public MatchesCalculator(BoardAbstract board)
        {
            _board = board;
            _tilesInBoard = board.TilesInBoard;
            _boardHeight = board.Rows.Count;
            _boardWidth = board.Rows[0].Tiles.Count;
        }

        public List<TileInBoard> FindMatches()
        {
            List<MatchInfo> matchesInfo = new List<MatchInfo>();

            var matches = new int[_boardHeight][];
            for (int i = 0; i < matches.Length; i++)
            {
                matches[i] = new int[_boardWidth];
                for (int j = 0; j < _boardWidth; j++)
                    matches[i][j] = 0;
            }

            // проходим по каждой координате в board
            for (int x = 0; x < _boardWidth; x++)
            {
                for (int y = 0; y < _boardHeight; y++)
                {
                    // получаем тайл по координатам
                    var tile = _board.GetTileByCoordinates(x, y, out string tileName);
                    //var tile = _board.GetTileByCoordinates(x, y);
                    if (string.IsNullOrEmpty(tileName))
                    {
                        if (tile.Tile == null)
                            Debug.LogError($"cant find tile with coordinates {x},{y}");
                        continue;
                    }

                    /*// если матч ещё не был найден в этих индексах
                    if (matches[x][y] == 0)
                    {
                        int horizontalMatchesCount = GetHorizontalMatchCount(x, y, tileName);
                        if (horizontalMatchesCount >= 3)
                        {
                            Debug.Log($"find horizontal match by startPos: {x},{y} with count: {horizontalMatchesCount}");
                            MatchInfo newMatchInfo = new MatchInfo(x, y, horizontalMatchesCount, true);
                            matchesInfo.Add(newMatchInfo);
                        }

                        int verticalMatchesCount = GetVerticalMatchCount(x, y, tileName);
                        if (verticalMatchesCount >= 3)
                        {
                            Debug.Log($"find horizontal match by startPos: {x},{y} with count: {verticalMatchesCount}");
                            MatchInfo newMatchInfo = new MatchInfo(y, x, verticalMatchesCount, false);
                            matchesInfo.Add(newMatchInfo);
                        }
                    }*/
                }
            }

            return null;
        }

        private int GetHorizontalMatchCount(int startX, int startY, string tileName)
        {
            int count = 0;

            for (int x = startX; x < _boardWidth; x++)
            {
                var tile = _board.GetTileByCoordinates(x, startY, out string nextTileName);
                if (tileName == nextTileName)
                    count++;
                else
                    break;
            }

            return count;
        }

        private int GetVerticalMatchCount(int startX, int startY, string tileName)
        {
            int count = 0;

            for (int y = startY; y < _boardHeight; y++)
            {
                var tile = _board.GetTileByCoordinates(startX, y, out string nextTileName);
                if (tileName != nextTileName)
                    break;

                count++;
            }

            return count;
        }
    }
}
