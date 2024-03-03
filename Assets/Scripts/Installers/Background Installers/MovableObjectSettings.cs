using System;
using UnityEngine;

[Serializable]
public class MovableObjectSettings
{
    [field: SerializeField, Range(1, 3)]
    public float Magnitude { get; private set; }

    [field: SerializeField, Range(2, 6)]
    public float Frequency { get; private set; }

    [field: SerializeField, Range(2f, 5f)]
    public float MaxSpeed { get; private set; }

    [field: SerializeField, Range(1f, 2f)]
    public float MinSpeed { get; private set; }
}
