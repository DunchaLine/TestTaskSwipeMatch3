using SwipeMatch3.Gameplay.Settings;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using static SwipeMatch3.Gameplay.BoardAbstract;

namespace SwipeMatch3.Gameplay
{
    public class MatchInfo
    {
        public readonly int startX;

        public readonly int startY;

        public readonly int count;

        public readonly int end;

        public readonly bool isHorizontal;

        public readonly string tileName;

        //public readonly List<TileInBoard> tiles;

        public MatchInfo(int startX, int startY, int count, bool isHorizontal, string tileName)//List<TileInBoard> tiles)
        {
            this.startX = startX;
            this.startY = startY;
            this.count = count;
            this.isHorizontal = isHorizontal;
            this.tileName = tileName;
            //this.tiles = tiles;

            if (isHorizontal)
                end = startX + count;
            else
                end = startY + count;
        }
    }

    public class MatchesCalculator
    {
        private readonly TileInBoard[] _tilesInBoard;
        private readonly int _boardHeight;
        private readonly int _boardWidth;
        private readonly BoardAbstract _board;

        private List<SpecialCase> _specialCases = new List<SpecialCase>();

        public MatchesCalculator(BoardAbstract board)
        {
            _board = board;
            _tilesInBoard = board.TilesInBoard;
            _boardHeight = board.Rows.Count;
            _boardWidth = board.Rows[0].Tiles.Count;
        }

        [Zenject.Inject]
        private void Init(List<SpecialCase> cases)
        {
            if (_specialCases.Count == 0)
                _specialCases = cases;
        }

        public List<TileInBoard> FindMatches()
        {
            List<MatchInfo> horizontalMatchesInfo = new List<MatchInfo>();
            List<MatchInfo> verticalMatchesInfo = new List<MatchInfo>();

            /*var matches = new int[_boardHeight][];
            for (int i = 0; i < matches.Length; i++)
            {
                matches[i] = new int[_boardWidth];
                for (int j = 0; j < _boardWidth; j++)
                    matches[i][j] = 0;
            }*/

            // проходим по каждой координате в board
            for (int y = 0; y < _boardHeight; y++)
            {
                for (int x = 0; x < _boardWidth; x++)
                {
                    // получаем тайл по координатам
                    var tile = _board.GetTileByCoordinates(x, y, out string tileName);
                    if (string.IsNullOrEmpty(tileName))
                    {
                        if (tile.Tile == null)
                            Debug.LogError($"cant find tile with coordinates {x},{y}");
                        continue;
                    }

                    if (horizontalMatchesInfo.FirstOrDefault(g => y == g.startY && x >= g.startX && x <= g.end) == null)//) == null)
                    {
                        int horizontalMatchesCount = GetHorizontalMatchCount(x, y, tileName, out var tiles);
                        if (horizontalMatchesCount >= 3)
                        {
                            //Debug.Log($"find horizontal match by startPos: {x},{y} with count: {horizontalMatchesCount}");
                            MatchInfo newMatchInfo = new MatchInfo(x, y, horizontalMatchesCount, true, tileName);//tiles);
                            horizontalMatchesInfo.Add(newMatchInfo);
                        }
                    }

                    if (verticalMatchesInfo.FirstOrDefault(g => x == g.startX && y >= g.startY && y <= g.end) == null)// && ) == null)
                    {
                        int verticalMatchesCount = GetVerticalMatchCount(x, y, tileName, out var tiles);
                        if (verticalMatchesCount >= 3)
                        {
                            //Debug.Log($"find vertical match by startPos: {x},{y} with count: {verticalMatchesCount}");
                            MatchInfo newMatchInfo = new MatchInfo(x, y, verticalMatchesCount, false, tileName);//tiles);
                            verticalMatchesInfo.Add(newMatchInfo);
                        }
                    }
                }
            }

            foreach (var matchInfo in horizontalMatchesInfo)
                CheckSpecialCases(matchInfo);

            foreach (var matchInfo in verticalMatchesInfo)
                CheckSpecialCases(matchInfo);

            return null;
        }

        private int GetHorizontalMatchCount(int startX, int startY, string tileName, out List<TileInBoard> matchedTiles)
        {
            matchedTiles = new List<TileInBoard>();
            int count = 0;

            for (int x = startX; x < _boardWidth; x++)
            {
                var tile = _board.GetTileByCoordinates(x, startY, out string nextTileName);
                if (tileName != nextTileName)
                    break;

                matchedTiles.Add(tile);
                count++;
            }

            return count;
        }

        private int GetVerticalMatchCount(int startX, int startY, string tileName, out List<TileInBoard> matchedTiles)
        {
            matchedTiles = new List<TileInBoard>();
            int count = 0;

            for (int y = startY; y < _boardHeight; y++)
            {
                var tile = _board.GetTileByCoordinates(startX, y, out string nextTileName);
                if (tileName != nextTileName)
                    break;

                matchedTiles.Add(tile);
                count++;
            }

            return count;
        }

        private void CheckSpecialCases(MatchInfo matchInfo)
        {
            var specialCases = new List<SpecialCase>();
            foreach (var specialCase in _specialCases)
            {
                if (specialCase.IsCaseOnBoard(matchInfo, _tilesInBoard, out var tilesToClear))
                    specialCases.Add(specialCase);
            }
            
            // очищать тайлы при помощи запуска сигнала на очистку, в который передавать список с тайлами, которые нужно сделать прозрачными
        }
    }
}
