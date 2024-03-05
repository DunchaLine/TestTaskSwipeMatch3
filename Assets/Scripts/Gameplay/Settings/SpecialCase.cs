using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SwipeMatch3.Gameplay.Settings
{
    [CreateAssetMenu(fileName = "SpecialCase", menuName = "SpecialCase/SpecialCases")]
    public class SpecialCase : ScriptableObject
    {
        [field: SerializeField]
        public Row[] Rows { get; private set; }

        private int _mostLengthRowIndex = -1;

        [Serializable]
        public class Row
        {
            public bool[] IsVisibleTile;

            public Row(bool[] isVisible)
            {
                IsVisibleTile = isVisible;
            }
        }

        private bool IsInitialCaseHorizontal
        {
            get
            {
                return Rows.Length < Rows[0].IsVisibleTile.Length;
            }
        }

        private void SetMostLengthRowIndex()
        {
            if (_mostLengthRowIndex != -1)
                return;

            if (Rows == null || Rows.Length == 0)
            {
                _mostLengthRowIndex = -1;
                return;
            }

            if (Rows.Length == 1)
            {
                _mostLengthRowIndex = Rows[0].IsVisibleTile.Length;
                return;
            }

            for (int i = 1; i < Rows.Length; i++)
            {
                if (Rows[_mostLengthRowIndex].IsVisibleTile.Where(g => g == true).Count() < Rows[i].IsVisibleTile.Where(g => g == true).Count())
                    _mostLengthRowIndex = i;
            }

            /*if (MostLengthRowIndex != - 1)
                return MostLengthRowIndex;


            Row mostLengthRow = Rows[0];
            if (Rows.Length == 1)
                return MostLengthRowIndex;

            for (int i = 1; i < Rows.Length; i++)
            {
                if (mostLengthRow.IsVisibleTile.Where(g => g == true).Count() < Rows[i].IsVisibleTile.Where(g => g == true).Count())
                    mostLengthRow = Rows[i];
            }

            return MostLengthRowIndex;*/
        }

        /// <summary>
        /// Возвращает количество тайлов в фигуре
        /// </summary>
        /// <returns></returns>
        public int GetVisibleTiles()
        {
            int tilesCount = 0;
            foreach (var row in Rows)
                tilesCount += row.IsVisibleTile.Where(g => g == true).Count();

            return tilesCount;
        }

        public bool IsEqual(MatchInfo matchInfo, BoardAbstract.TileInBoard[] tilesInBoard)
        {
            RotateFigure(Rows);
            /*SetMostLengthRowIndex();
            if (_mostLengthRowIndex == -1)
                return false;*/


            // если квадрат, то mirror нужен RotateFigure


            return false;
        }

        private Row[] MirrorFigure(Row[] rows)
        {
            Row[] newRows = new Row[_mostLengthRowIndex];
            for (int i = 0; i < newRows.Length; i++)
                newRows[i].IsVisibleTile = new bool[rows[i].IsVisibleTile.Length];

            for (int i = 0; i < newRows.Length; i++)
            {
                for (int j = 0; j < newRows[i].IsVisibleTile.Length; j++)
                    newRows[i].IsVisibleTile[j] = rows[rows.Length - 1 - i].IsVisibleTile[j];
            }

            return newRows;

            /*for (int i = 0; i < rows.Length; i++)
            {
                for (int j = 0; j < rows[i].IsVisibleTile.Length; j++)
                {

                }
            }*/
        }

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

            /*for (int i = 0; i < rows.Length; i++)
            {
                for (int j = 0; j < rows[i].IsVisibleTile.Length; j++)
                    newRows[j].IsVisibleTile[i] = rows[i].IsVisibleTile[j];
            }*/

            return newRows;
        }
    }
}
