using System;
using GameSystemsScripts.Core.Runtime;
using UnityEngine;

namespace GameSystemsScripts.Core.FusePanel
{
    public sealed class FusePanelMechanic : IGameMechanic
    {
        private const float DragDepth = 1.0f;
        private static int _runtimeFuseSequence;

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

            Debug.Log("FusePanel: remove burned fuse click detected.");

            if (!TryRaycast(out var hit))
            {
                Debug.LogWarning("FusePanel: remove failed - raycast did not hit any collider.");
                return;
            }

            Debug.Log($"FusePanel: raycast hit '{hit.collider.name}'.");

            var fuse = hit.collider.GetComponentInParent<FuseItemContainer>();
            if (fuse == null || !fuse.IsBurned)
            {
                if (fuse == null)
                {
                    Debug.LogWarning("FusePanel: remove failed - hit object has no FuseItemContainer.");
                }
                else
                {
                    Debug.LogWarning($"FusePanel: remove failed - fuse '{fuse.name}' is not burned.");
                }
                return;
            }

            var slot = GetSlotByFuse(fuse);
            if (slot == null)
            {
                Debug.LogWarning($"FusePanel: remove failed - burned fuse '{fuse.name}' is not assigned to any slot runtime state.");
                return;
            }

            slot.CurrentFuseView = null;
            fuse.gameObject.SetActive(false);
            Debug.Log($"FusePanel: burned fuse '{fuse.name}' removed from slot '{slot.SlotId}'.");
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

            var sourceContainer = hit.collider.GetComponentInParent<FuseSourceContainer>();
            if (sourceContainer != null)
            {
                StartDragFromSource(sourceContainer);
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

        private void StartDragFromSource(FuseSourceContainer sourceContainer)
        {
            if (sourceContainer == null)
            {
                return;
            }

            if (_container.FuseItemPrefab == null)
            {
                Debug.LogError("FusePanel: cannot spawn dragged fuse - FuseItemPrefab is not assigned.");
                return;
            }

            var spawnPoint = sourceContainer.SpawnPoint;
            if (spawnPoint == null)
            {
                Debug.LogError($"FusePanel: source '{sourceContainer.name}' has no spawn point.");
                return;
            }

            var fuse = UnityEngine.Object.Instantiate(_container.FuseItemPrefab, spawnPoint);
            if (fuse == null)
            {
                Debug.LogError("FusePanel: failed to instantiate fuse from source.");
                return;
            }

            _runtimeFuseSequence++;
            var fuseId = $"runtime_drag_{_runtimeFuseSequence}";
            fuse.SetFuseId(fuseId);
            fuse.SetSourceContainerId(sourceContainer.ContainerId);
            fuse.SetVoltage(sourceContainer.SupplyVoltage);
            fuse.SetBurned(false);
            fuse.transform.SetParent(spawnPoint, false);
            fuse.transform.localPosition = Vector3.zero;
            fuse.transform.localRotation = Quaternion.identity;
            fuse.gameObject.SetActive(true);

            _component.FusesById[fuseId] = new FuseItemSpec
            {
                FuseId = fuseId,
                Voltage = sourceContainer.SupplyVoltage,
                Color = fuse.Color,
                Size = fuse.Size,
                SourceContainerId = sourceContainer.ContainerId,
                View = fuse
            };

            _component.DraggedFuse = fuse;
            _component.DragOriginalParent = spawnPoint;
            _component.DragOriginalPosition = spawnPoint.position;
            _component.Phase = FusePanelPhase.Dragging;
        }

        private void UpdateDraggedPosition()
        {
            var camera = _container.InputCamera;
            if (camera == null)
            {
                return;
            }

            var ray = camera.ScreenPointToRay(Input.mousePosition);
            _component.DraggedFuse.transform.position = ray.GetPoint(DragDepth);
        }

        private void TryDropDraggedFuse()
        {
            var draggedFuse = _component.DraggedFuse;
            var targetSlot = FindDropSlotIgnoringDraggedFuse(draggedFuse);
            if (targetSlot == null)
            {
                targetSlot = FindDropSlotByScreenDistance();
            }

            var targetSpec = targetSlot != null ? GetSlotByContainer(targetSlot) : null;
            if (targetSpec != null && targetSpec.CurrentFuseView == null)
            {
                targetSpec.CurrentFuseView = draggedFuse;
                draggedFuse.transform.SetParent(targetSpec.SlotTransform, false);
                draggedFuse.transform.localPosition = Vector3.zero;
                draggedFuse.transform.localRotation = Quaternion.identity;
                draggedFuse.SetBurned(false);
                Debug.Log($"FusePanel: fuse dropped into slot '{targetSlot.SlotId}'.");
                OnFuseDropped?.Invoke(draggedFuse, targetSlot);
            }
            else
            {
                Debug.LogWarning("FusePanel: drop failed - no valid empty slot found, returning fuse to source.");
                ReturnDraggedFuseToSource(draggedFuse);
            }

            _component.ResetDragState();
        }

        private FuseSlotContainer FindDropSlotIgnoringDraggedFuse(FuseItemContainer draggedFuse)
        {
            var camera = _container.InputCamera;
            if (camera == null)
            {
                return null;
            }

            var ray = camera.ScreenPointToRay(Input.mousePosition);
            var hits = Physics.RaycastAll(ray, 100f, _container.InteractionMask.value);
            if (hits == null || hits.Length == 0)
            {
                return null;
            }

            Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
            for (var i = 0; i < hits.Length; i++)
            {
                var hit = hits[i];
                var hitFuse = hit.collider.GetComponentInParent<FuseItemContainer>();
                if (hitFuse != null && hitFuse == draggedFuse)
                {
                    continue;
                }

                var slot = hit.collider.GetComponentInParent<FuseSlotContainer>();
                if (slot != null)
                {
                    return slot;
                }
            }

            return null;
        }

        private FuseSlotContainer FindDropSlotByScreenDistance()
        {
            var camera = _container.InputCamera;
            if (camera == null)
            {
                return null;
            }

            const float maxScreenDistance = 120f;
            var mouse = (Vector2)Input.mousePosition;
            FuseSlotContainer bestSlot = null;
            var bestDistance = float.MaxValue;

            for (var i = 0; i < _component.Slots.Count; i++)
            {
                var slot = _component.Slots[i];
                if (slot == null || slot.SlotTransform == null || slot.CurrentFuseView != null)
                {
                    continue;
                }

                var slotContainer = slot.SlotTransform.GetComponent<FuseSlotContainer>();
                if (slotContainer == null)
                {
                    continue;
                }

                var screenPoint = camera.WorldToScreenPoint(slot.SlotTransform.position);
                if (screenPoint.z <= 0f)
                {
                    continue;
                }

                var distance = Vector2.Distance(mouse, new Vector2(screenPoint.x, screenPoint.y));
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    bestSlot = slotContainer;
                }
            }

            if (bestSlot != null && bestDistance <= maxScreenDistance)
            {
                Debug.Log($"FusePanel: fallback slot match by screen distance ({bestDistance:F1}px).");
                return bestSlot;
            }

            return null;
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

            if (HasInstalledBurnedFuses())
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

            if (_container.BoxRoot != null)
            {
                _container.BoxRoot.SetActive(false);
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

        private bool HasInstalledBurnedFuses()
        {
            for (var i = 0; i < _component.Slots.Count; i++)
            {
                var fuse = _component.Slots[i].CurrentFuseView;
                if (fuse != null && fuse.IsBurned)
                {
                    return true;
                }
            }

            return false;
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

        private FuseSlotSpec GetSlotByContainer(FuseSlotContainer slotContainer)
        {
            if (slotContainer == null)
            {
                return null;
            }

            for (var i = 0; i < _component.Slots.Count; i++)
            {
                var slot = _component.Slots[i];
                if (slot.SlotTransform == slotContainer.transform)
                {
                    return slot;
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
