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
        /// Есть ли частный случай на board
        /// </summary>
        /// <param name="matchInfo"></param>
        /// <param name="tilesInBoard"></param>
        /// <param name="tilesToClear"></param>
        /// <returns></returns>
        public bool IsCaseOnBoard(MatchInfo matchInfo, TileInBoard[] tilesInBoard, out List<TileInBoard> tilesToClear)
        {
            tilesToClear = new List<TileInBoard>();
            if (IsCaseCorrect() == false)
            {
                Debug.LogError($"incorrect tiles count in case: {name}");
                return false;
            }

            var rows = Rows;
            // у фигуры максимум может быть 4 состояния
            for (int i = 0; i < 4; i++)
            {
                rows = RotateFigure(rows);
                if (IsCaseOnBoard(rows, matchInfo, tilesInBoard, out tilesToClear))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Проверка на корректность частного случая
        /// </summary>
        /// <returns></returns>
        private bool IsCaseCorrect()
        {
            // в частном случае должно быть одинаковое количество тайлов во всех строках

            if (Rows == null || Rows.Length == 0)
                return false;

            int tilesInRowCount = Rows[0].IsVisibleTile.Length;
            for (int i = 1; i < Rows.Length; i++)
            {
                if (tilesInRowCount != Rows[i].IsVisibleTile.Length)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Получение максимального количество видимых тайлов в одной строке в фигуре
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
        /// Получение максимального количества видимых тайлов в одном столбце в фигуре
        /// </summary>
        /// <param name="rows"></param>
        /// <param name="columnIndex"></param>
        /// <returns></returns>
        private int GetMaxLengthVisibleTilesInColumn(Row[] rows, out int columnIndex)
        {
            columnIndex = -1;
            if (rows == null || rows.Length == 0)
                return -1;


            int maxTilesInColumn = 0;
            columnIndex = 0;
            for (int j = 0; j < rows[0].IsVisibleTile.Length; j++)
            {
                int visibleTilesInColumn = 0;
                for (int i = 0; i < rows.Length; i++)
                {
                    if (rows[i].IsVisibleTile[j])
                        visibleTilesInColumn++;
                }
                if (visibleTilesInColumn > maxTilesInColumn)
                {
                    maxTilesInColumn = visibleTilesInColumn;
                    columnIndex = j;
                }
            }

            return maxTilesInColumn;
        }

        /// <summary>
        /// Получение тайлов на очистку по строке из фигуры
        /// </summary>
        /// <param name="row">строка фигуры (частного случая)</param>
        /// <param name="tilesInBoard">тайлы на доске</param>
        /// <param name="matchInfo">инфа о матче</param>
        /// <param name="y">позиция по y относительно частного случая</param>
        /// <returns></returns>
        private List<TileInBoard> GetTilesToClearFromBoard(Row row, TileInBoard[] tilesInBoard, MatchInfo matchInfo, int y)
        {
            List<TileInBoard> tilesToClear = new List<TileInBoard>();

            //проходим по всем тайлам в строке над стартовой
            for (int tileIndex = 0; tileIndex < row.IsVisibleTile.Length; tileIndex++)
            {
                if (row.IsVisibleTile[tileIndex] == false)
                    continue;

                var tile = tilesInBoard.FirstOrDefault(
                    g => g.Coordinates.Equals(new int2(matchInfo.startX + tileIndex, matchInfo.startY + y)));
                if (tile.Tile == null || matchInfo.tileName != tile.Tile.TileSetting.TileName)
                    return new List<TileInBoard>();

                tilesToClear.Add(tile);
            }

            return tilesToClear;
        }

        /// <summary>
        /// Получение списка тайлов (из частного случая) на очистку для вертикального матча
        /// </summary>
        /// <param name="rows">список строк от фигуры (частного случая)</param>
        /// <param name="tilesInBoard">тайлы на доске</param>
        /// <param name="matchInfo">инфа о матче</param>
        /// <param name="x">позиция по x относительно частного случая</param>
        /// <param name="xOffset">offset для x</param>
        /// <returns></returns>
        private List<TileInBoard> GetTilesToClearFromBoardByVertical(Row[] rows, TileInBoard[] tilesInBoard, MatchInfo matchInfo, int x, int xOffset)
        {
            var tilesToClear = new List<TileInBoard>();
            // проходим по каждому тайлу в столбце
            for (int i = 0; i < rows.Length; i++)
            {
                if (rows[i].IsVisibleTile[x] == false)
                    continue;

                var tile = tilesInBoard.FirstOrDefault(
                    g => g.Coordinates.Equals(new int2(matchInfo.startX - x - xOffset, matchInfo.startY + i)));

                if (tile.Tile == null || matchInfo.tileName != tile.Tile.TileSetting.TileName)
                    return new List<TileInBoard>();

                tilesToClear.Add(tile);
            }

            return tilesToClear;
        }

        /// <summary>
        /// Есть ли вертикальный матч на доске в частном случае
        /// </summary>
        /// <param name="rows">список строк от фигуры (частного случая)</param>
        /// <param name="matchInfo">инфа о матче</param>
        /// <param name="tilesInBoard">тайлы на доске</param>
        /// <param name="tilesToClear">список тайлов для удаления</param>
        /// <returns></returns>
        private bool IsVerticalCaseOnBoard(Row[] rows, MatchInfo matchInfo, TileInBoard[] tilesInBoard, out List<TileInBoard> tilesToClear)
        {
            tilesToClear = new List<TileInBoard>();
            // TODO: добавить возможность комбинаций если в матче больше тайлов, чем в фигуре
            if (GetMaxLengthVisibleTilesInColumn(rows, out int columnIndex) != matchInfo.count)
                return false;

            int columnsCount = rows[0].IsVisibleTile.Length;

            // если колонка с матчем - первая => переходим на проверки справа
            if (columnIndex == 0)
                goto RightColumnsCheck;

            // если есть в фигуре колонки левее, но на board нет колонки левее - пропуск
            if (matchInfo.startX == 0)
            {
                tilesToClear = new List<TileInBoard>();
                return false;
            }

            // проход по всем столбцам слева от столбца с матчем
            int offset = columnIndex - 1 == 0 ? 1 : 0;
            for (int j = columnIndex - 1; j >= 0; j--)
            {
                var tiles = GetTilesToClearFromBoardByVertical(rows, tilesInBoard, matchInfo, j, offset);
                if (tiles.Count == 0)
                {
                    tilesToClear = new List<TileInBoard>();
                    return false;
                }

                tilesToClear.AddRange(tiles);
            }

            // проход по всем столбцам справа от столбца с матчем
            RightColumnsCheck:

            for (int j = columnIndex + 1; j < columnsCount; j++)
            {
                offset = -j * 2;
                var tiles = GetTilesToClearFromBoardByVertical(rows, tilesInBoard, matchInfo, j, offset);
                if (tiles.Count == 0)
                {
                    tilesToClear = new List<TileInBoard>();
                    return false;
                }

                tilesToClear.AddRange(tiles);
            }

            if (tilesToClear.Count != 0)
                return true;

            return false;
        }

        /// <summary>
        /// Есть ли матч на доске в частном случае
        /// </summary>
        /// <param name="rows">список строк от фигуры (частного случая)</param>
        /// <param name="matchInfo">инфа о матче</param>
        /// <param name="tilesInBoard">тайлы на доске</param>
        /// <param name="tilesToClear">список тайлов для удаления</param>
        /// <returns></returns>
        private bool IsCaseOnBoard(Row[] rows, MatchInfo matchInfo, TileInBoard[] tilesInBoard, out List<TileInBoard> tilesToClear)
        {
            tilesToClear = new List<TileInBoard>();
            // если матч == вертикальным, для него свои проверки
            if (matchInfo.isHorizontal == false)
            {
                // если фигура горизонтальная, а матч вертикальный => возвращаем false
                if (IsVerticalCaseOnBoard(rows, matchInfo, tilesInBoard, out var verticalTilesToClear))
                {
                    tilesToClear.AddRange(verticalTilesToClear);
                    if (tilesToClear.Count > 0)
                    {
                        Debug.Log($"find vertical special case: {name} in match: {matchInfo.tileName} ");
                        return true;
                    }
                }

                tilesToClear = new List<TileInBoard>();
                return false;
            }

            // если фигура вертикальная, а матч горизонтальный
            // или количество тайлов не совпадает с количеством тайлов в матче => возвращаем false
            // TODO: добавить возможность комбинаций если в матче больше тайлов, чем в фигуре
            if (GetMaxLengthVisibleTilesInRow(rows) != matchInfo.count)
                return false;

            // TODO: мб нет необходимости в этой проверки, вынести получения индекса строки в GetMaxLengthVisibleTilesInRow
            if (TryGetRowIndexWithMatch(rows, matchInfo, out int rowMatchIndex) == false)
                return false;

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
                Debug.Log($"find horizontal special case: {this.name} in match: {matchInfo.tileName}");
                return true;
            }
            return false;
        }

        /// <summary>
        /// Получение индекса строки из фигуры по искомому матчу
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
        /// Поворот фигуры
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
