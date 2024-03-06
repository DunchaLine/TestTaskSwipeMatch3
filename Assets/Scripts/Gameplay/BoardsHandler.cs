using static SwipeMatch3.Gameplay.BoardAbstract;
using SwipeMatch3.Gameplay.Interfaces;
using SwipeMatch3.Gameplay.Signals;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using UnityEngine;

using Zenject;
using Cysharp.Threading.Tasks;

namespace SwipeMatch3.Gameplay
{
    public class BoardsHandler : IDisposable
    {
        public List<BoardAbstract> Boards { get; private set; }

        public BoardAbstract ActiveBoard { get; private set; }

        private SignalBus _signalBus;

        private MatchesCalculator _matchesCalculator;

        private DiContainer _container;

        private List<int> ColumnsChecking { get; set; } = new List<int>();

        private List<UniTask> tasksToDropdown = new List<UniTask>();

        private CancellationTokenSource _cancellationToken;

        [Inject]
        private void Init(SignalBus signalBus, List<BoardAbstract> boards, DiContainer container)
        {
            if (boards == null || boards.Count == 0)
            {
                Debug.LogError($"Boards on scene is null or empty");
                return;
            }

            _signalBus = signalBus;
            SubscribeSignals();

            _container = container;
            ActiveBoard = boards[0];
            Boards = boards;
            foreach (var board in boards)
            {
                if (board == ActiveBoard)
                {
                    _container.Inject(board);
                    board.InitBoard();
                    continue;
                }

                board.SetBoardInactive();
            }

            _cancellationToken = new CancellationTokenSource();
            SetMatchCalculator().Forget();
            // TODO: временно, перенести в UI, который будет запускать нормализацию по кнопке на старте игры
            var columnsIndexes = new List<int>();
            for (int i = 0; i < ActiveBoard.BoardWidth; i++)
                columnsIndexes.Add(i);
            _signalBus.Fire(new GameSignals.NormalizeTilesOnBoardSignal(columnsIndexes));
        }

        /// <summary>
        /// Подписки на сигналы
        /// </summary>
        private void SubscribeSignals()
        {
            _signalBus.Subscribe<GameSignals.NormalizeTilesOnBoardSignal>(NormalizeTilesOnBoard);
            _signalBus.Subscribe<GameSignals.FindMatches>(GetTilesToSetInvisible);
            _signalBus.Subscribe<GameSignals.ChangeBoardSignal>(SetNextBoardActive);
            _signalBus.Subscribe<GameSignals.ReInitBoardSignal>(ReInitActiveBoard);
        }

        private async UniTask SetMatchCalculator()
        {
            while (ActiveBoard.Rows.Count == 0)
                await UniTask.NextFrame();

            _matchesCalculator = new MatchesCalculator(ActiveBoard);

            var matchesCalculator = _container.TryResolve<MatchesCalculator>();
            if (matchesCalculator != null)
                _container.Rebind<MatchesCalculator>().FromInstance(_matchesCalculator).AsCached();
            else
                _container.Bind<MatchesCalculator>().FromInstance(_matchesCalculator).AsCached();

            _container.Inject(_matchesCalculator);
        }

        /// <summary>
        /// Получение списка тайлов в столбце
        /// </summary>
        /// <param name="columnIndex"></param>
        /// <returns></returns>
        public List<TileInBoard> GetTilesInColumn(int columnIndex)
        {
            List<TileInBoard> tilesInBoard = new List<TileInBoard>();
            for (int x = 0; x < ActiveBoard.BoardHeight; x++)
                tilesInBoard.Add(ActiveBoard.GetTileByCoordinates(columnIndex, x));

            return tilesInBoard;
        }

        public int GetTileColumnIndex(ITileMovable tileMovable)
        {
            return ActiveBoard.GetTileColumnIndex(tileMovable);
        }

        /// <summary>
        /// Нормализация тайлов на board
        /// </summary>
        private async void NormalizeTilesOnBoard(GameSignals.NormalizeTilesOnBoardSignal signal)
        {
            await UniTask.WhenAll(tasksToDropdown);
            tasksToDropdown = new List<UniTask>();

            var columns = signal.ColumnsIndexes.Distinct().ToList();
            foreach (var columnIndex in columns)
            {
                if (columnIndex < 0)
                    continue;

                // сохраняем список всех тайлов в одном столбце
                List<TileInBoard> tilesInColumn = GetTilesInColumn(columnIndex);

                // если в текущем столбце нет "висящих" тайлов => пропуск
                if (IsTileColumnNeedToNormalize(tilesInColumn) == false)
                    continue;

                // если этот столбец уже проверяется => пропуск
                if (ColumnsChecking.Contains(columnIndex))
                    continue;

                tasksToDropdown.Add(MoveTilesDownTask(columnIndex));
            }

            await UniTask.WhenAll(tasksToDropdown);
            tasksToDropdown = new List<UniTask>();
            ColumnsChecking = new List<int>();
            _signalBus.Fire<GameSignals.FindMatches>();
        }

        /// <summary>
        /// Таска на нормализацию столбца
        /// </summary>
        /// <param name="columnIndex"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        private async UniTask MoveTilesDownTask(int columnIndex, List<ITileMovable> tilesMovedDown = default)
        {
            ColumnsChecking.Add(columnIndex);

            List<TileInBoard> tilesInColumn = GetTilesInColumn(columnIndex);
            if (tilesMovedDown == null)
                tilesMovedDown = new List<ITileMovable>();

            // обновлять тайлы при каждом входе в UniTask
            int index = 0;
            foreach (var tileInColumn in tilesInColumn)
            {
                if (index + 1 >= tilesInColumn.Count)
                    break;

                // получаем текущий тайл и тайл над ним
                var currentTile = tileInColumn;
                var upperTile = tilesInColumn[index + 1];
                // пока можно, опускаем тайл
                while (CanMoveDown(currentTile, upperTile))
                {
                    Debug.Log($"up down column");

                    var currentImovable = currentTile.Tile.GetComponent<ITileMovable>();
                    var upperImovable = upperTile.Tile.GetComponent<ITileMovable>();

                    if (currentImovable == null || upperImovable == null)
                        break;
                    else if (currentImovable.TileSetting.Visible == false && upperImovable.TileSetting.Visible == false)
                        break;
                    // если тайлы ещё не подписаны на события во время их движения вниз
                    if (tilesMovedDown.Contains(currentImovable) == false)
                    {
                        tilesMovedDown.Add(currentImovable);
                        currentImovable.OnStartSwapUpDown();
                    }

                    if (tilesMovedDown.Contains(upperImovable) == false)
                    {
                        tilesMovedDown.Add(upperImovable);
                        upperImovable.OnStartSwapUpDown();
                    }

                    await UniTask.WaitForSeconds(.2f);

                    if (_cancellationToken.IsCancellationRequested)
                    {
                        foreach (var tile in tilesMovedDown)
                            tile.OnEndSwapUpDown();

                        Debug.Log($"cancelling task");
                        return;
                    }

                    // запускаем сигналы на сам свап, и на действия, необходимые во время свапа вверх/вниз
                    _signalBus.Fire(
                        new GameSignals.SwapSpritesUpDownSignal(currentImovable, upperImovable));
                    _signalBus.Fire(new GameSignals.OnSwappingSpritesUpDownSignal());

                    await UniTask.WaitForSeconds(.2f);
                    upperImovable.OnEndSwapUpDown();
                    tilesMovedDown.Remove(upperImovable);
                }
                index++;
            }
            
            // если в столбце все ещё нужно опускать тайлы, запускаем заново
            if (IsNeedToMoveDown(tilesInColumn))
                await MoveTilesDownTask(columnIndex, tilesMovedDown);

            foreach (var tile in tilesMovedDown)
                tile.OnEndSwapUpDown();

            ColumnsChecking.Remove(columnIndex);
        }

        /// <summary>
        /// Нужно ли опускать тайлы
        /// </summary>
        /// <param name="tilesInColumn"></param>
        /// <returns></returns>
        private bool IsNeedToMoveDown(List<TileInBoard> tilesInColumn)
        {
            // если нет ни одного видимого объекта в столбце, его двигать не нужно
            var firstVisible = tilesInColumn.FirstOrDefault(g => g.Tile.TileSetting.Visible);
            if (firstVisible.Tile == null)
                return false;

            // сортируем столбцы по true
            var sortedTiles = tilesInColumn.OrderByDescending(g => g.Tile.TileSetting.Visible).ToList();
            for (int i = 0; i < sortedTiles.Count; i++)
            {
                // если индекс не совпадает с позицией по оси, нужно ещё опускать тайлы
                if (sortedTiles[i].Coordinates.y != i)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Можно ли опустить upperTile в currentTile
        /// Можно опустить только если текущий тайл невидимый, а тайл над ним = видимый
        /// </summary>
        /// <param name="currentTile"></param>
        /// <param name="upperTile"></param>
        /// <returns></returns>
        private bool CanMoveDown(TileInBoard currentTile, TileInBoard upperTile)
        {
            return currentTile.Tile.TileSetting.Visible == false && upperTile.Tile.TileSetting.Visible;
        }

        /// <summary>
        /// Нужно ли нормализовать столбец
        /// </summary>
        /// <param name="tiles"></param>
        /// <returns></returns>
        private bool IsTileColumnNeedToNormalize(List<TileInBoard> tiles)
        {
            if (tiles == null || tiles.Count < 2)
                return false;

            if (tiles.Count == 2)
                return IsUpperTileHanging(tiles[0], tiles[1]);

            for (int i = 0; i <= tiles.Count - 2; i++)
            {
                // проверяем снизу вверх. Если над текущим невидимым тайлом висит видимый тайл =>
                // нужно нормализовать
                var currentTile = tiles[i];
                var tileUpper = tiles[i + 1];
                if (IsUpperTileHanging(currentTile, tileUpper))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Висит ли upperTile над tile
        /// </summary>
        /// <param name="tile"></param>
        /// <param name="upperTile"></param>
        /// <returns></returns>
        private bool IsUpperTileHanging(TileInBoard tile, TileInBoard upperTile)
        {
            if (tile.Tile.TileSetting.Visible == false && upperTile.Tile.TileSetting.Visible)
                return true;

            return false;
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

        private void GetTilesToSetInvisible()
        {
            _matchesCalculator.FindMatches();
        }

        // добавить метод для замены поля
        // будет вызываться извне, по нажатии на кнопку
        // нужно активировать следующее, после текущего, поле
        // если поле последнее => активировать первое
        public void SetNextBoardActive()
        {
            _cancellationToken.Cancel();
            if (Boards.Count == 1)
            {
                ActiveBoard.InitBoard();
                return;
            }

            // получаем индекс активной board на сцене
            int activeBoardIndex = 0;
            for (int i = 0; i < Boards.Count; i++)
            {
                if (Boards[i] != ActiveBoard)
                    continue;

                activeBoardIndex = i;
            }

            // если это последняя board, то индекс следующий = 0
            if (activeBoardIndex == Boards.Count - 1)
                activeBoardIndex = 0;
            // в других случаях, индекс прибавляем на 1
            else
                activeBoardIndex++;

            _cancellationToken = new CancellationTokenSource();
            ActiveBoard.SetBoardInactive();
            ActiveBoard = Boards[activeBoardIndex];
            ActiveBoard.InitBoard();
            SetMatchCalculator().Forget();
        }

        public void ReInitActiveBoard()
        {
            _cancellationToken.Cancel();
            ActiveBoard.InitBoard();
        }

        public void Dispose()
        {
            _signalBus.Unsubscribe<GameSignals.NormalizeTilesOnBoardSignal>(NormalizeTilesOnBoard);
            _signalBus.Unsubscribe<GameSignals.FindMatches>(GetTilesToSetInvisible);
            _signalBus.Unsubscribe<GameSignals.ChangeBoardSignal>(SetNextBoardActive);
            _signalBus.Unsubscribe<GameSignals.ReInitBoardSignal>(ReInitActiveBoard);
        }
    }
}
