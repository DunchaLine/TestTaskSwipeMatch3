using SwipeMatch3.Gameplay.Settings;
using SwipeMatch3.Gameplay.Signals;
using SwipeMatch3.Gameplay.Interfaces;
using static SwipeMatch3.Gameplay.BoardAbstract;

using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using Zenject;

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

        public readonly List<TileInBoard> tiles;

        public MatchInfo(int startX, int startY, int count, bool isHorizontal, string tileName, List<TileInBoard> tiles)
        {
            this.startX = startX;
            this.startY = startY;
            this.count = count;
            this.isHorizontal = isHorizontal;
            this.tileName = tileName;
            this.tiles = tiles;

            if (isHorizontal)
                end = startX + count;
            else
                end = startY + count;
        }
    }

    public class MatchesCalculator : IMatchesCalcularable
    {
        private readonly TileInBoard[] _tilesInBoard;
        private readonly int _boardHeight;
        private readonly int _boardWidth;
        private readonly BoardAbstract _board;

        private List<SpecialCase> _specialCases = new List<SpecialCase>();
        private SignalBus _signalBus;

        public MatchesCalculator(BoardAbstract board)
        {
            _board = board;
            _tilesInBoard = board.TilesInBoard;
            _boardHeight = board.Rows.Count;
            _boardWidth = board.Rows[0].Tiles.Count;
        }

        [Inject]
        private void Init(List<SpecialCase> cases, SignalBus signalBus)
        {
            if (_specialCases.Count == 0)
                _specialCases = cases;
            _signalBus = signalBus;
        }

        public void FindMatches()
        {
            List<MatchInfo> matchesInfo = new List<MatchInfo>();

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

                    // проверяем горизонтальный матч
                    MatchInfo newMatchInfo = null;
                    if (matchesInfo.FirstOrDefault(g => y == g.startY && x >= g.startX && x <= g.end) == null)
                        newMatchInfo = GetMatch(x, y, tileName, true);

                    if (newMatchInfo != null)
                        matchesInfo.Add(newMatchInfo);

                    // проверяем вертикальный матч
                    newMatchInfo = null;
                    if (matchesInfo.FirstOrDefault(g => x == g.startX && y >= g.startY && y <= g.end) == null)
                        newMatchInfo = GetMatch(x, y, tileName, false);

                    if (newMatchInfo != null)
                        matchesInfo.Add(newMatchInfo);
                }
            }

            // ищем совпадения частных случаев в board
            var specialCasesWithTiles = new Dictionary<SpecialCase, List<TileInBoard>>();
            foreach (var matchInfo in matchesInfo)
            {
                // проверяем среди уже найденных частных случаев
                bool isFind = false;
                HashSet<TileInBoard> matchesTiles = new HashSet<TileInBoard>(matchInfo.tiles);
                foreach (var specialCaseWithTiles in specialCasesWithTiles)
                {
                    // если тайлы в матче содержатся в тайлах в уже найденном частном случае => пропуск
                    HashSet<TileInBoard> specialCaseTiles = new HashSet<TileInBoard>(specialCaseWithTiles.Value);
                    if (matchesTiles.IsProperSubsetOf(specialCaseTiles))
                    {
                        isFind = true;
                        break;
                    }
                }

                if (isFind)
                    continue;

                var newCases = GetSpecialCases(matchInfo);
                foreach (var newCase in newCases)
                    specialCasesWithTiles.TryAdd(newCase.Key, newCase.Value);
            }

            List<TileInBoard> tilesToClear = new List<TileInBoard>();
            foreach (var specialCaseWithTiles in specialCasesWithTiles)
                tilesToClear.AddRange(specialCaseWithTiles.Value);

            foreach (var matchInfo in matchesInfo)
                tilesToClear.AddRange(matchInfo.tiles);

            _signalBus.Fire(new GameSignals.ClearTiles(tilesToClear));
        }

        /// <summary>
        /// Получение матча по координатам
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="tileName"></param>
        /// <param name="isHorizontal"></param>
        /// <returns></returns>
        private MatchInfo GetMatch(int x, int y, string tileName, bool isHorizontal)
        {
            int matchesCount;
            List<TileInBoard> tiles = new List<TileInBoard>();
            if (isHorizontal)
                matchesCount = GetHorizontalMatchCount(x, y, tileName, out tiles);
            else
                matchesCount = GetVerticalMatchCount(x, y, tileName, out tiles);

            // если в матче 3 или больше тайлов с одним именем
            if (matchesCount >= 3)
                return new MatchInfo(x, y, matchesCount, isHorizontal, tileName, tiles);

            return null;
        }

        /// <summary>
        /// Получение длины матча по горизонтали
        /// </summary>
        /// <param name="startX"></param>
        /// <param name="startY"></param>
        /// <param name="tileName"></param>
        /// <param name="matchedTiles"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Получение длины матча по вертикали
        /// </summary>
        /// <param name="startX"></param>
        /// <param name="startY"></param>
        /// <param name="tileName"></param>
        /// <param name="matchedTiles"></param>
        /// <returns></returns>
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

        private Dictionary<SpecialCase, List<TileInBoard>> GetSpecialCases(MatchInfo matchInfo)
        {
            Dictionary<SpecialCase, List<TileInBoard>> specialCaseWithTilesToClear =
                new Dictionary<SpecialCase, List<TileInBoard>>();
            foreach (var specialCase in _specialCases)
            {
                if (specialCase.IsCaseOnBoard(matchInfo, _tilesInBoard, out var tilesToClear))
                {
                    tilesToClear.AddRange(matchInfo.tiles);
                    specialCaseWithTilesToClear.TryAdd(specialCase, tilesToClear);

                }
            }

            return specialCaseWithTilesToClear;
        }
    }
}
