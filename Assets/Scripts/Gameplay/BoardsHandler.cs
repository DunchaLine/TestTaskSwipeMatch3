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

        private IMatchesCalcularable _matchesCalculator;

        private DiContainer _container;

        private List<int> ColumnsChecking { get; set; } = new List<int>();

        //private List<(UniTask<List<ITileMovable>>, int)> tasksToDropdownWithColumnIndex = new List<(UniTask<List<ITileMovable>>, int)>();
        private Dictionary<UniTask, (CancellationTokenSource, int)> tasksToDropdown = new Dictionary<UniTask, (CancellationTokenSource, int)>();

        private bool _isTilesChanged = false;

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

            SetMatchCalculator().Forget();
            CheckBoardOnInit();
        }

        private void CheckBoardOnInit()
        {
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
            _signalBus.Subscribe<GameSignals.FindMatches>(FindMatches);
            _signalBus.Subscribe<GameSignals.ChangeBoardSignal>(SetNextBoardActive);
            _signalBus.Subscribe<GameSignals.ReInitBoardSignal>(ReInitActiveBoard);
        }

        private async UniTask SetMatchCalculator()
        {
            while (ActiveBoard.Rows.Count == 0)
                await UniTask.NextFrame();

            // TODO: добавить возможность выбора типа калькулятора
            _matchesCalculator = new MatchesCalculator(ActiveBoard);

            var matchesCalculator = _container.TryResolve<IMatchesCalcularable>();
            if (matchesCalculator != null)
                _container.Rebind<IMatchesCalcularable>().FromInstance(_matchesCalculator).AsCached();
            else
                _container.Bind<IMatchesCalcularable>().FromInstance(_matchesCalculator).AsCached();

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
        /// Попытка удалить таску, обрабатывающую столбец с индексом columnIndex
        /// </summary>
        /// <param name="columnIndex"></param>
        /// <param name="findedUniTaskToken"></param>
        /// <returns></returns>
        private bool TryRemoveUniTaskOnColumn(int columnIndex, out CancellationTokenSource findedUniTaskToken)
        {
            foreach (var dropdownTask in tasksToDropdown)
            {
                // если таска сейчас не запущена - пропуск
                if (dropdownTask.Key.Status != UniTaskStatus.Pending)
                    continue;

                // если индекс столбца в таске не соответствует искомому - пропуск
                if (columnIndex != dropdownTask.Value.Item2)
                    continue;

                // возвращаем токен и true
                findedUniTaskToken = dropdownTask.Value.Item1;
                tasksToDropdown.Remove(dropdownTask.Key);
                return true;
            }

            findedUniTaskToken = new CancellationTokenSource();
            return false;
        }

        private UniTask CreateUniTaskDefer(List<TileInBoard> tiles, int columnIndex, CancellationToken token)
        {
            return UniTask.Defer(() => MoveTilesDownTask(tiles, token));
        }

        /// <summary>
        /// Нормализация тайлов на board
        /// </summary>
        private async void NormalizeTilesOnBoard(GameSignals.NormalizeTilesOnBoardSignal signal)
        {
            List<int> columnsIndexes = signal.ColumnsIndexes.Distinct().ToList();
            var tmpTasks = new List<UniTask>();
            UniTaskCompletionSource tmps = new UniTaskCompletionSource();
            foreach (var columnIndex in columnsIndexes)
            {
                if (columnIndex < 0)
                    continue;

                // получаем список всех тайлов в столбце под индексом columnIndex
                List<TileInBoard> tilesInColumn = GetTilesInColumn(columnIndex);
                CancellationTokenSource token = new CancellationTokenSource();

                CancellationTokenSource findedToken;

                // если столбец не надо нормализовывать
                if (IsTileColumnNeedToNormalize(tilesInColumn, out int _) == false)
                {
                    // ищем таску с нормализацией столбца среди уже запущенных и останавливаем если она есть
                    if (TryRemoveUniTaskOnColumn(columnIndex, out findedToken))
                        findedToken.Cancel();

                    continue;
                }

                // если столбец уже проверяется, нужно перезапустить проверку
                if (TryRemoveUniTaskOnColumn(columnIndex, out findedToken))
                {
                    findedToken.Cancel();
                    UniTask taskDefer = CreateUniTaskDefer(tilesInColumn, columnIndex, token.Token);
                    tasksToDropdown.Add(taskDefer, new (token, columnIndex));
                    tmpTasks.Add(taskDefer);
                    continue;
                }

                UniTask task = CreateUniTaskDefer(tilesInColumn, columnIndex, token.Token);
                tasksToDropdown.Add(task, new (token, columnIndex));
                tmpTasks.Add(task);
            }

            await UniTask.WhenAll(tasksToDropdown.Keys);
            // удаляем из списка те таски, которые были пройдены на во время этого запуска нормализации
            foreach (var tmpTask in tmpTasks)
            {
                if (tasksToDropdown.ContainsKey(tmpTask))
                    tasksToDropdown.Remove(tmpTask);
            }

            ColumnsChecking = new List<int>();
            while (tasksToDropdown.Count > 0)
            {
                Debug.Log($"waiting while tasksToDropdown count == 0");
                await UniTask.NextFrame();
            }

            Debug.Log($"firing to find matches");
            _signalBus.Fire<GameSignals.FindMatches>();
        }

        /// <summary>
        /// Таска на нормализацию столбца
        /// </summary>
        /// <param name="columnIndex"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        private async UniTask MoveTilesDownTask(List<TileInBoard> tilesInColumn, CancellationToken token)
        {
            List<ITileMovable> movableTiles = new List<ITileMovable>();
            try
            {
                // пока надо опускать тайлы в столбце
                while (IsTileColumnNeedToNormalize(tilesInColumn, out int startIndex))
                {
                    if (startIndex == 0)
                        break;

                    if (token.IsCancellationRequested)
                        return;

                    //int index = 0;
                    TileInBoard currentTile = tilesInColumn[startIndex - 1];
                    TileInBoard upperTile = tilesInColumn[startIndex];
                    // пока можно, пытаемся опустить тайл
                    while (CanMoveDown(currentTile, upperTile))
                    {
                        if (token.IsCancellationRequested)
                            return;

                        // получаем IMovable для каждого тайла
                        ITileMovable currentMovable = currentTile.Tile.GetComponent<ITileMovable>();
                        ITileMovable upperMovable = upperTile.Tile.GetComponent<ITileMovable>();

                        if (currentMovable == null || upperMovable == null)
                            break;

                        // если и текущий и тайл выше невидимые
                        /*if (currentMovable.TileSetting.Visible == false && upperMovable.TileSetting.Visible == false)
                        {
                            // поднимаемся на один тайл выше
                            index++;
                            if (index + 1 > tilesInColumn.Count)
                                break;

                            currentTile = tilesInColumn[index];
                            upperTile = tilesInColumn[index + 1];

                            continue;
                        }*/

                        movableTiles.Add(currentMovable);
                        movableTiles.Add(upperMovable);

                        currentMovable.OnStartSwapUpDown();
                        upperMovable.OnStartSwapUpDown();

                        await UniTask.WaitForSeconds(.2f);
                        if (token.IsCancellationRequested)
                            return;
                        // запускаем сигналы на сам свап, и на действия, необходимые во время свапа вверх/вниз
                        _signalBus.Fire(
                            new GameSignals.SwapSpritesUpDownSignal(currentMovable, upperMovable));
                        _signalBus.Fire(new GameSignals.OnSwappingSpritesUpDownSignal());

                        await UniTask.WaitForSeconds(.2f);
                        // так как 
                        /*if (IsEndSwapUpDown(upperTile))
                            upperMovable.OnEndSwapUpDown();*/
                        if (IsEndSwapUpDown(currentTile))
                            currentMovable.OnEndSwapUpDown();
                        if (IsEndSwapUpDown(upperTile))
                            upperMovable.OnEndSwapUpDown();

                        // тк произошёл свап, 
                        currentTile = upperTile;
                        upperTile = ActiveBoard.GetTileByCoordinates(upperTile.Coordinates.x, upperTile.Coordinates.y + 1);
                        /*currentTile = upperTile;
                        upperTile = ActiveBoard.GetTileByCoordinates(upperTile.Coordinates.x, );*/
                    }
                }
            }
            finally
            {
                foreach (var movableTile in movableTiles)
                    movableTile.OnEndSwapUpDown();
            }

        }



        private bool IsEndSwapUpDown(TileInBoard tileInBoard)
        {
            if (tileInBoard.Tile.TileSetting.Visible == false)
                return true;

            // если тайл находится в первой строке - true
            if (tileInBoard.Coordinates.y == 0)
                return true;

            // проходим по каждом тайлу под тайлом в параметрах
            for (int i = tileInBoard.Coordinates.y; i >= 0; i--)
            {
                // если хоть один тайл невидимый - false
                var lowerTile = ActiveBoard.GetTileByCoordinates(tileInBoard.Coordinates.x, i);
                if (lowerTile.Tile != null && lowerTile.Tile.TileSetting.Visible == false)
                    return false;
            }

            return true;
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
            if (currentTile.Tile == null || upperTile.Tile == null)
                return false;

            return currentTile.Tile.TileSetting.Visible == false && upperTile.Tile.TileSetting.Visible;
        }

        /// <summary>
        /// Нужно ли нормализовать столбец
        /// </summary>
        /// <param name="tiles"></param>
        /// <returns></returns>
        private bool IsTileColumnNeedToNormalize(List<TileInBoard> tiles, out int startIndex)
        {
            startIndex = -1;
            if (tiles == null || tiles.Count < 2)
                return false;

            if (tiles.Count == 2)
            {
                startIndex = 1;
                return IsUpperTileHanging(tiles[0], tiles[1]);
            }

            for (int i = 0; i <= tiles.Count - 2; i++)
            {
                // проверяем снизу вверх. Если над текущим невидимым тайлом висит видимый тайл =>
                // нужно нормализовать
                var currentTile = tiles[i];
                var tileUpper = tiles[i + 1];
                if (IsUpperTileHanging(currentTile, tileUpper))
                {
                    startIndex = i + 1;
                    return true;
                }
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

        private void FindMatches()
        {
            _matchesCalculator.FindMatches();
        }

        public void SetNextBoardActive()
        {
            if (Boards.Count == 1)
                goto SettingActiveBoard;

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

            ActiveBoard.SetBoardInactive();
            ActiveBoard = Boards[activeBoardIndex];

            SettingActiveBoard:
            SettingActiveBoard();
        }

        public void ReInitActiveBoard()
        {
            SettingActiveBoard();
        }

        private void SettingActiveBoard()
        {
            foreach (var task in tasksToDropdown)
                task.Value.Item1.Cancel();

            tasksToDropdown = new Dictionary<UniTask, (CancellationTokenSource, int)>();

            ActiveBoard.InitBoard();
            SetMatchCalculator().Forget();
            CheckBoardOnInit();
        }

        public void Dispose()
        {
            _signalBus.Unsubscribe<GameSignals.NormalizeTilesOnBoardSignal>(NormalizeTilesOnBoard);
            _signalBus.Unsubscribe<GameSignals.FindMatches>(FindMatches);
            _signalBus.Unsubscribe<GameSignals.ChangeBoardSignal>(SetNextBoardActive);
            _signalBus.Unsubscribe<GameSignals.ReInitBoardSignal>(ReInitActiveBoard);
        }
    }
}
