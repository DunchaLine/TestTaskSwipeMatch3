using SwipeMatch3.Gameplay.Interfaces;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SwipeMatch3.Gameplay
{
    public class InputHandler : MonoBehaviour
    {
        [SerializeField]
        private GraphicRaycaster _raycaster;

        private PointerEventData _pointerEvent;

        private ITileMovable _currentTileUnderInput = null;

        private GameplayHandler GameplayHandler { get; set; }

        [Zenject.Inject]
        private void Init(GameplayHandler handler)
        {
            GameplayHandler = handler;
        }

        private void Start()
        {
            _pointerEvent = new PointerEventData(null);
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                _currentTileUnderInput = null;

                var tileUnder = GetTileUnderTouch();
                if (tileUnder != null)
                    _currentTileUnderInput = tileUnder;
            }

            if (Input.GetMouseButtonUp(0))
            {
                if (_currentTileUnderInput == null)
                    return;

                var tileOnTouchUp = GetTileUnderTouch();
                if (tileOnTouchUp == _currentTileUnderInput)
                    return;

                GameplayHandler.SwapSprites(_currentTileUnderInput, tileOnTouchUp);

                // проверить на свайп

                _currentTileUnderInput = null;
            }

        }

        private ITileMovable GetTileUnderTouch()
        {
            _pointerEvent.position = Input.mousePosition;
            var results = new List<RaycastResult>();
            _raycaster.Raycast(_pointerEvent, results);

            if (results == null || results.Count == 0)
                return null;

            foreach (var result in results)
            {
                if (result.gameObject == null || result.gameObject.TryGetComponent(out ITileMovable tileMovable) == false)
                    continue;

                Debug.Log($"hit tile: {result.gameObject.name}");
                return tileMovable;
            }

            return null;
        }
    }
}
