using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Настройки пула объектов бэкграунда
/// </summary>
[Serializable]
public class BackgroundPoolSettings : IBackgroundPoolSettings
{
    [field: SerializeField]
    public GameObject Prefab { get; private set; }

    [field: SerializeField]
    public List<Sprite> Sprites { get; private set; }

    [field: SerializeField, Range(1, 10)]
    public int ObjectCount { get; private set; }
}
