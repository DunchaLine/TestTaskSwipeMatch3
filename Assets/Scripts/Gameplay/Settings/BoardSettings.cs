using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BoardSettings", menuName = "Board/BoardScriptableSettings")]
public class BoardSettings : ScriptableObject
{
    [field: SerializeField]
    public Row[] Rows { get; private set; }

    [Serializable]
    public class Row
    {
        public Sprite[] SpritesInRow;
    }
    /*[field: SerializeField]
    public int Rows { get; private set; } = 6;

    [field: SerializeField]
    public int Columns { get; private set; } = 4;*/

    //public Sprite[,] Board = new Sprite[Columns, Rows];
}
