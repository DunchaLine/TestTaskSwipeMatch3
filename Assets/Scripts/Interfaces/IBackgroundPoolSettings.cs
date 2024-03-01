using SwipeMatch3.ObjectPoolingBackground;
using System.Collections.Generic;
using UnityEngine;

public interface IBackgroundPoolSettings
{
    public GameObject Prefab { get; }

    public List<Sprite> Sprites { get; }
    
    public int ObjectCount { get; }
}
