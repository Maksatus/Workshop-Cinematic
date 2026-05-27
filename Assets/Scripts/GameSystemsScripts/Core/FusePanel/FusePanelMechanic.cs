using System;
using GameSystemsScripts.Core.Runtime;
using UnityEngine;

namespace GameSystemsScripts.Core.FusePanel
{
    public sealed class FusePanelMechanic : IGameMechanic
    {
        private const float DragDepth = 2.25f;

        private readonly FusePanelComponent _component;
        private readonly FusePanelContainer _container;

        public event Action<FuseItemContainer> OnFuseRemoved;
        public event Action<FuseItemContainer, FuseSlotContainer> OnFuseDropped;
        public event Action OnValidationFailed;
        public event Action OnCompleted;

        public FusePanelMechanic(FusePanelComponent component, FusePanelContainer container)
        {
            _component = component;
            _container = container;
        }

        public void UpdateMechanic(GameSystemsHandler gameSystemsHandler, float deltaTime)
        {
            if (_component.IsCompleted || _container == null)
            {
                return;
            }

            if (_component.IsGenerationFailed)
            {
                return;
            }

            if (!_component.WasActivated || !_component.IsGenerationReady)
            {
                return;
            }

            if (_component.Phase == FusePanelPhase.Presenting || _component.Phase == FusePanelPhase.Inactive)
            {
                _component.Phase = FusePanelPhase.RemovingBurned;
            }

            if (_component.Phase == FusePanelPhase.RemovingBurned || _component.Phase == FusePanelPhase.Dragging)
            {
                HandleRemoveBurnedInput();
                HandleDragInput();
                TryValidate();
            }
        }

        public void DisposeMechanic()
        {
            OnFuseRemoved = null;
            OnFuseDropped = null;
            OnValidationFailed = null;
            OnCompleted = null;
        }

        private void HandleRemoveBurnedInput()
        {
            if (!Input.GetMouseButtonDown(0))
            {
                return;
            }

            if (!TryRaycast(out var hit))
            {
                return;
            }

            var fuse = hit.collider.GetComponentInParent<FuseItemContainer>();
            if (fuse == null || !fuse.IsBurned)
            {
                return;
            }

            var slot = GetSlotByFuse(fuse);
            if (slot == null)
            {
                return;
            }

            slot.CurrentFuseView = null;
            fuse.gameObject.SetActive(false);
            OnFuseRemoved?.Invoke(fuse);
        }

        private void HandleDragInput()
        {
            if (Input.GetMouseButtonDown(0))
            {
                TryStartDrag();
            }

            if (Input.GetMouseButton(0) && _component.DraggedFuse != null)
            {
                UpdateDraggedPosition();
            }

            if (Input.GetMouseButtonUp(0) && _component.DraggedFuse != null)
            {
                TryDropDraggedFuse();
            }
        }

        private void TryStartDrag()
        {
            if (!TryRaycast(out var hit))
            {
                return;
            }

            var fuse = hit.collider.GetComponentInParent<FuseItemContainer>();
            if (fuse == null || !fuse.gameObject.activeInHierarchy)
            {
                return;
            }

            // Drag starts only from source containers or from currently empty workflow after burn removal.
            if (GetSlotByFuse(fuse) != null)
            {
                return;
            }

            _component.DraggedFuse = fuse;
            _component.DragOriginalParent = fuse.transform.parent;
            _component.DragOriginalPosition = fuse.transform.position;
            _component.Phase = FusePanelPhase.Dragging;
        }

        private void UpdateDraggedPosition()
        {
            var camera = _container.InputCamera;
            if (camera == null)
            {
                return;
            }

            var mousePosition = Input.mousePosition;
            mousePosition.z = DragDepth;
            _component.DraggedFuse.transform.position = camera.ScreenToWorldPoint(mousePosition);
        }

        private void TryDropDraggedFuse()
        {
            var draggedFuse = _component.DraggedFuse;
            FuseSlotContainer targetSlot = null;
            if (TryRaycast(out var hit))
            {
                targetSlot = hit.collider.GetComponentInParent<FuseSlotContainer>();
            }

            var targetSpec = targetSlot != null ? GetSlotById(targetSlot.SlotId) : null;
            if (targetSpec != null && targetSpec.CurrentFuseView == null)
            {
                targetSpec.CurrentFuseView = draggedFuse;
                draggedFuse.transform.SetParent(targetSpec.SlotTransform, true);
                draggedFuse.transform.position = targetSpec.SlotTransform.position;
                draggedFuse.SetBurned(false);
                OnFuseDropped?.Invoke(draggedFuse, targetSlot);
            }
            else
            {
                ReturnDraggedFuseToSource(draggedFuse);
            }

            _component.ResetDragState();
        }

        private void ReturnDraggedFuseToSource(FuseItemContainer fuse)
        {
            if (fuse == null)
            {
                return;
            }

            if (_component.DragOriginalParent != null)
            {
                fuse.transform.SetParent(_component.DragOriginalParent, true);
            }

            fuse.transform.position = _component.DragOriginalPosition;
        }

        private void TryValidate()
        {
            if (!AreAllSlotsFilled())
            {
                return;
            }

            _component.Phase = FusePanelPhase.Validation;
            var hasMismatches = false;

            for (var i = 0; i < _component.Slots.Count; i++)
            {
                var slot = _component.Slots[i];
                if (slot.CurrentFuseView == null)
                {
                    continue;
                }

                if (slot.CurrentFuseView.Voltage >= slot.RequiredVoltage)
                {
                    continue;
                }

                slot.CurrentFuseView.SetBurned(true);
                slot.CurrentFuseView.gameObject.SetActive(false);
                slot.CurrentFuseView = null;
                hasMismatches = true;
            }

            if (hasMismatches)
            {
                _component.LastResult = FusePanelResult.RetryRequired;
                _component.Phase = FusePanelPhase.RemovingBurned;
                OnValidationFailed?.Invoke();
                return;
            }

            _component.LastResult = FusePanelResult.Success;
            _component.IsCompleted = true;
            _component.Phase = FusePanelPhase.Completed;
            if (_container.PanelRoot != null)
            {
                _container.PanelRoot.SetActive(false);
            }

            OnCompleted?.Invoke();
        }

        private bool AreAllSlotsFilled()
        {
            for (var i = 0; i < _component.Slots.Count; i++)
            {
                if (_component.Slots[i].CurrentFuseView == null)
                {
                    return false;
                }
            }

            return _component.Slots.Count > 0;
        }

        private FuseSlotSpec GetSlotByFuse(FuseItemContainer fuse)
        {
            for (var i = 0; i < _component.Slots.Count; i++)
            {
                if (_component.Slots[i].CurrentFuseView == fuse)
                {
                    return _component.Slots[i];
                }
            }

            return null;
        }

        private FuseSlotSpec GetSlotById(string slotId)
        {
            for (var i = 0; i < _component.Slots.Count; i++)
            {
                if (_component.Slots[i].SlotId == slotId)
                {
                    return _component.Slots[i];
                }
            }

            return null;
        }

        private bool TryRaycast(out RaycastHit hit)
        {
            hit = default;
            var camera = _container.InputCamera;
            if (camera == null)
            {
                return false;
            }

            var ray = camera.ScreenPointToRay(Input.mousePosition);
            return Physics.Raycast(ray, out hit, 100f, _container.InteractionMask.value);
        }
    }
}
