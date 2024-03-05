using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using static SwipeMatch3.Gameplay.BoardAbstract;

namespace SwipeMatch3.Gameplay.Settings
{
    [CreateAssetMenu(fileName = "SpecialCase", menuName = "SpecialCase/SpecialCases")]
    public class SpecialCase : ScriptableObject
    {
        [field: SerializeField]
        public Row[] Rows { get; private set; }

        [Serializable]
        public class Row
        {
            public bool[] IsVisibleTile;

            public Row(bool[] isVisible)
            {
                IsVisibleTile = isVisible;
            }
        }

        /// <summary>
        /// ѕолучение максимального количество видимых тайлов в одной строке в фигуре
        /// </summary>
        /// <param name="rows"></param>
        /// <returns></returns>
        private int GetMaxLengthVisibleTilesInRow(Row[] rows)
        {
            if (rows == null || rows.Length == 0)
                return -1;
            else if (rows.Length == 1)
                return rows[0].IsVisibleTile.Length;

            var maxTilesInRow = rows[0].IsVisibleTile.Where(g => g == true).Count();
            for (int i = 1; i < rows.Length; i++)
            {
                var count = rows[i].IsVisibleTile.Where(g => g == true).Count();
                if (count > maxTilesInRow)
                    maxTilesInRow = count;
            }

            return maxTilesInRow;
        }

        /// <summary>
        /// ¬озвращает количество тайлов в фигуре
        /// </summary>
        /// <returns></returns>
        public int GetVisibleTiles()
        {
            int tilesCount = 0;
            foreach (var row in Rows)
                tilesCount += row.IsVisibleTile.Where(g => g == true).Count();

            return tilesCount;
        }

        public bool IsCaseOnBoard(MatchInfo matchInfo, TileInBoard[] tilesInBoard, out List<TileInBoard> tilesToClear)
        {
            var rows = Rows;
            tilesToClear = new List<TileInBoard>();
            // у фигуры максимум может быть 4 состо€ни€
            for (int i = 0; i < 4; i++)
            {
                rows = RotateFigure(rows);
                if (IsCaseOnBoard(rows, matchInfo, tilesInBoard, out tilesToClear))
                    return true;
            }

            return false;
        }

        private bool IsCaseOnBoard(Row[] rows, MatchInfo matchInfo, TileInBoard[] tilesInBoard, out List<TileInBoard> tilesToClear)
        {
            tilesToClear = new List<TileInBoard>();
            // если у фигуры максимальное число видимых тайлов > максимального числа в матче => return false
            // TODO: добавить возможность комбинаций если в матче больше тайлов, чем в фигуре
            if (GetMaxLengthVisibleTilesInRow(rows) != matchInfo.count)
                return false;

            if (TryGetRowIndexWithMatch(rows, matchInfo, out int rowMatchIndex) == false)
                return false;

            // временно частные случаи только дл€ горизонтальных матчей
            if (matchInfo.isHorizontal == false)
            {
                Debug.LogError("vertical match");
                return false;
            }

            // проходим по всем строкам над стартовой
            for (int rowIndex = rowMatchIndex + 1; rowIndex < rows.Length; rowIndex++)
            {
                int offsetY = rowIndex - rowMatchIndex;
                var tiles = GetTilesToClearFromBoard(rows[rowIndex], tilesInBoard, matchInfo, offsetY);
                if (tiles.Count == 0)
                {
                    tilesToClear = new List<TileInBoard>();
                    return false;
                }
                tilesToClear.AddRange(tiles);
            }

            // проходим по всем строка под стартовой
            for (int rowIndex = rowMatchIndex - 1; rowIndex >= 0; rowIndex--)
            {
                int offset = rowIndex - rowMatchIndex;
                var tiles = GetTilesToClearFromBoard(rows[rowIndex], tilesInBoard, matchInfo, offset);
                if (tiles.Count == 0)
                {
                    tilesToClear = new List<TileInBoard>();
                    return false;
                }

                tilesToClear.AddRange(tiles);
            }

            if (tilesToClear.Count > 0)
            {
                Debug.Log($"find special case: {this.name} in match: {matchInfo.tileName}");
                return true;
            }
            return false;
        }

        /// <summary>
        /// ѕолучение тайлов на очистку по строке из фигуры
        /// </summary>
        /// <param name="row"></param>
        /// <param name="tilesInBoard"></param>
        /// <param name="matchInfo"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        private List<TileInBoard> GetTilesToClearFromBoard(Row row, TileInBoard[] tilesInBoard, MatchInfo matchInfo, int y)
        {
            List<TileInBoard> tilesToClear = new List<TileInBoard>();
            //List<int> visibleTilesX = new List<int>();

            //проходим по всем тайлам в строке над стартовой
            for (int tileIndex = 0; tileIndex < row.IsVisibleTile.Length; tileIndex++)
            {
                if (row.IsVisibleTile[tileIndex] == false)
                    continue;
                //visibleTilesX.Add(tileIndex);
                var tile = tilesInBoard.FirstOrDefault(
                    g => g.Coordinates.Equals(new int2(matchInfo.startX + tileIndex, matchInfo.startY + y)));
                if (tile.Tile == null || matchInfo.tileName != tile.Tile.TileSetting.TileName)
                    return new List<TileInBoard>();

                tilesToClear.Add(tile);
            }

            /*foreach (var visibleTileX in visibleTilesX)
            {
                var tile = tilesInBoard.FirstOrDefault(
                    g => g.Coordinates.Equals(new int2(matchInfo.startX + visibleTileX, matchInfo.startY + y)));
                if (tile.Tile == null || matchInfo.tileName != tile.Tile.TileSetting.TileName)
                    return new List<TileInBoard>();

                tilesToClear.Add(tile);
            }*/

            return tilesToClear;
        }

        /// <summary>
        /// ѕолучение индекса строки из фигуры по искомому матчу
        /// </summary>
        /// <param name="rows"></param>
        /// <param name="matchInfo"></param>
        /// <param name="rowWithMatchIndex"></param>
        /// <returns></returns>
        private bool TryGetRowIndexWithMatch(Row[] rows, MatchInfo matchInfo, out int rowWithMatchIndex)
        {
            rowWithMatchIndex = -1;
            for (int i = 0; i < rows.Length; i++)
            {
                int visibleInRow = 0;

                foreach (var isVisible in rows[i].IsVisibleTile)
                {
                    if (isVisible == false)
                    {
                        visibleInRow = 0;
                        continue;
                    }
                    visibleInRow++;

                    if (visibleInRow >= matchInfo.count)
                    {
                        rowWithMatchIndex = i;
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// ѕоворот фигуры
        /// </summary>
        /// <param name="rows"></param>
        /// <returns></returns>
        private Row[] RotateFigure(Row[] rows)
        {
            Row[] newRows = new Row[rows[0].IsVisibleTile.Length];
            for (int i = 0; i < newRows.Length; i++)
                newRows[i] = new Row(new bool[rows.Length]);

            int tmpMax = rows[0].IsVisibleTile.Length - 1;

            for (int j = 0; j < rows.Length; j++)
            {
                for (int i = 0; i < rows[0].IsVisibleTile.Length; i++)
                    newRows[i].IsVisibleTile[j] = rows[j].IsVisibleTile[tmpMax - i];
            }

            return newRows;
        }
    }
}
