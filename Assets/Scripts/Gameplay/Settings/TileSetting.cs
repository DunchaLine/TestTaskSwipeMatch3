using UnityEngine;
using static SwipeMatch3.Gameplay.BoardAbstract;

namespace SwipeMatch3.Gameplay.Settings
{
    /// <summary>
    /// Настройка варианта тайла
    /// </summary>
    [CreateAssetMenu(fileName = "TileSetting", menuName = "Board/TileScriptableSetting")]
    public class TileSetting : ScriptableObject
    {
        [field: SerializeField]
        public Sprite Sprite { get; private set; }

        [field: SerializeField]
        public bool Visible { get; private set; } = false;

        [field: SerializeField]
        public bool IsInteractable { get; private set; } = true;
    }
}
