using SwipeMatch3.Gameplay.Interfaces;
using System.Collections.Generic;
using UnityEngine;

namespace SwipeMatch3.Gameplay
{
    public class BoardsHandler
    {
        public List<BoardAbstract> Boards { get; private set; }

        public BoardAbstract ActiveBoard { get; private set; }

        [Zenject.Inject]
        private void Init(List<BoardAbstract> boards)
        {
            if (boards == null || boards.Count == 0)
            {
                Debug.LogError($"Boards on scene is null or empty");
                return;
            }

            ActiveBoard = boards[0];
            Boards = boards;
            foreach (var board in boards)
            {
                if (board == ActiveBoard)
                    continue;

                board.SetBoardInactive();
            }
        }

        /// <summary>
        /// Получение моно компонентов тайлов по ITileMovable интерфейсу
        /// </summary>
        /// <param name="first">первый тайл с интерфейсом ITileMovable</param>
        /// <param name="second">второй тайл с интерфейсом ITileMovable</param>
        /// <param name="firstRowIndex">индекс строки, в которой находится первый тайл</param>
        /// <param name="secondRowIndex">индекс строки, в которой находится второй тайл</param>
        /// <returns>Item1 - компонент первого тайла, Item2 - компонент второго тайла</returns>
        private (TileAbstract, TileAbstract) GetTilesMonoComponents(ITileMovable first, ITileMovable second,
            out int firstRowIndex, out int secondRowIndex)
        {
            (TileAbstract, TileAbstract) tiles = (null, null);
            firstRowIndex = -1;
            secondRowIndex = -1;

            // проходим по каждой строке в активном Board
            var rows = ActiveBoard.Rows;
            for (int i = 0; i < rows.Count; i++)
            {
                // если не нашли до этого первый тайл, ищем в текущей строке
                if (tiles.Item1 == null)
                {
                    tiles.Item1 = rows[i].GetTile(first);
                    firstRowIndex = i;
                }
                // ищем второй тайл
                if (tiles.Item2 == null)
                {
                    tiles.Item2 = rows[i].GetTile(second);
                    secondRowIndex = i;
                }

                // если нашли => break;
                if (tiles.Item1 != null && tiles.Item2 != null)
                    break;
            }

            return tiles;
        }

        /// <summary>
        /// Являются ли тайлы соседями
        /// </summary>
        /// <param name="firstIndex">индекс первого тайла</param>
        /// <param name="secondId">индекс второго тайла</param>
        /// <returns>true - если второй индекс находится в пределах +-1 от первого
        /// false - если нет</returns>
        private bool IsTilesNeighbors(int firstIndex, int secondId)
        {
            if (firstIndex + 1 == secondId || firstIndex - 1 == secondId)
                return true;

            return false;
        }

        /// <summary>
        /// Находятся ли тайлы в одном столбце
        /// </summary>
        /// <param name="firstIndex">первый индекс</param>
        /// <param name="secondIndex">второй индекс</param>
        /// <returns></returns>
        private bool IsTilesInOneColumn(int firstIndex, int secondIndex)
        {
            return firstIndex == secondIndex;
        }

        /// <summary>
        /// Являются ли тайлы соседями
        /// </summary>
        /// <param name="first">первый тайл с интерфейсом ITileMovable</param>
        /// <param name="second">второй тайл с интерфейсом ITileMovable</param>
        /// <returns></returns>
        public bool IsTilesNeighbors(ITileMovable first, ITileMovable second)
        {
            if (first == null || second == null)
                return false;

            var tiles = GetTilesMonoComponents(first, second, out int firstRowIndex, out int secondRowIndex);

            TileAbstract firstTile = tiles.Item1;
            TileAbstract secondTile = tiles.Item2;

            if (firstTile == null || secondTile == null)
                return false;

            // если оба тайла находятся на одной строке
            if (firstRowIndex == secondRowIndex)
                return IsTilesNeighbors(firstTile.IndexInRow, secondTile.IndexInRow);

            // если тайлы находятся не в одном столбце
            if (IsTilesInOneColumn(firstTile.IndexInRow, secondTile.IndexInRow) == false)
                return false;

            // если оба тайла находятся в одном столбце и второй тайл не находится в индексе +-1 от первого
            if (IsTilesNeighbors(firstRowIndex, secondRowIndex) == false)
                return false;

            // если второй тайл невидимый и они находятся в одном столбце => false
            if (secondTile.TileSetting.Visible == false)
                return false;

            return true;
        }

        // добавить метод для замены поля
        // будет вызываться извне, по нажатию на кнопку
        // нужно активировать следующее, после текущего, поле
        // если поле последнее => активировать первое
        public void SetNextBoardActive()
        {

        }
    }
}
