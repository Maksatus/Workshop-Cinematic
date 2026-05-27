using System;
using System.Collections.Generic;
using GameSystemsScripts.Core.Runtime;
using UnityEngine;
using Random = UnityEngine.Random;

namespace GameSystemsScripts.Core.FusePanel
{
    public sealed class FusePanelFillMechanic : IGameMechanic
    {
        private const int MinVoltage = 5;
        private const int MaxVoltageInclusive = 12;
        private const float BurnedRatio = 0.3f;

        private readonly FusePanelComponent _panelComponent;
        private readonly FusePanelFillComponent _fillComponent;
        private readonly FusePanelContainer _container;

        public FusePanelFillMechanic(
            FusePanelComponent panelComponent,
            FusePanelFillComponent fillComponent,
            FusePanelContainer container)
        {
            _panelComponent = panelComponent;
            _fillComponent = fillComponent;
            _container = container;
        }

        public void UpdateMechanic(GameSystemsHandler gameSystemsHandler, float deltaTime)
        {
            if (_container == null || _container.ActivationLink == null)
            {
                return;
            }

            var activationObject = _container.ActivationLink.ActivationObject;
            var isActiveNow = activationObject != null && activationObject.activeInHierarchy;
            var risingEdge = isActiveNow && !_fillComponent.WasActivationObjectActive;

            _fillComponent.WasActivationObjectActive = isActiveNow;
            if (!risingEdge)
            {
                return;
            }

            _panelComponent.ActivationAttemptId++;
            _panelComponent.IsCompleted = false;
            _panelComponent.WasActivated = true;
            _panelComponent.LastResult = FusePanelResult.RetryRequired;
            _panelComponent.Phase = FusePanelPhase.Presenting;
            _panelComponent.IsGenerationReady = false;
            _panelComponent.IsGenerationFailed = false;
            _panelComponent.ResetDragState();
            _panelComponent.CurrentGeneratedState = null;

            if (_container.PanelRoot != null)
            {
                _container.PanelRoot.SetActive(true);
            }

            if (_container.SplineAnimation != null)
            {
                _container.SplineAnimation.Pause();
            }

            if (!GenerateAndApply())
            {
                _panelComponent.IsGenerationFailed = true;
                if (_container.PanelRoot != null)
                {
                    _container.PanelRoot.SetActive(false);
                }

                Debug.LogError("FusePanelFillMechanic failed to generate/apply panel state. Fuse panel flow is blocked for this activation.");
                return;
            }

            _panelComponent.IsGenerationReady = true;
        }

        public void DisposeMechanic()
        {
        }

        private bool GenerateAndApply()
        {
            if (_panelComponent.Slots.Count == 0)
            {
                Debug.LogError("FusePanelFillMechanic: no slots configured.");
                return false;
            }

            if (_container.FuseItemPrefab == null)
            {
                Debug.LogError("FusePanelFillMechanic: FuseItemPrefab is not assigned.");
                return false;
            }

            var generated = new GeneratedPanelState
            {
                ActivationAttemptId = _panelComponent.ActivationAttemptId,
                GeneratedAtUtc = DateTime.UtcNow
            };

            _panelComponent.FusesById.Clear();
            for (var i = 0; i < _container.FuseViews.Count; i++)
            {
                var sourceFuse = _container.FuseViews[i];
                if (sourceFuse == null || string.IsNullOrWhiteSpace(sourceFuse.FuseId))
                {
                    continue;
                }

                _panelComponent.FusesById[sourceFuse.FuseId] = new FuseItemSpec
                {
                    FuseId = sourceFuse.FuseId,
                    Voltage = sourceFuse.Voltage,
                    Color = sourceFuse.Color,
                    Size = sourceFuse.Size,
                    SourceContainerId = sourceFuse.SourceContainerId,
                    View = sourceFuse
                };
            }

            for (var i = 0; i < _panelComponent.Slots.Count; i++)
            {
                var slot = _panelComponent.Slots[i];
                if (slot.SlotTransform == null)
                {
                    Debug.LogError("FusePanelFillMechanic: slot transform is null.");
                    return false;
                }

                ClearSlotFuseInstances(slot);

                var voltage = Random.Range(MinVoltage, MaxVoltageInclusive + 1);
                var fuse = UnityEngine.Object.Instantiate(_container.FuseItemPrefab, slot.SlotTransform);
                if (fuse == null)
                {
                    Debug.LogError("FusePanelFillMechanic: failed to instantiate fuse prefab.");
                    return false;
                }

                var generatedFuseId = $"generated_{_panelComponent.ActivationAttemptId}_{slot.SlotId}_{i}";
                fuse.SetFuseId(generatedFuseId);

                slot.RequiredVoltage = voltage;
                slot.CurrentFuseView = fuse;
                var slotView = slot.SlotTransform.GetComponent<FuseSlotContainer>();
                if (slotView != null)
                {
                    slotView.SetRequiredVoltage(voltage);
                }

                fuse.SetVoltage(voltage);
                fuse.SetBurned(false);
                fuse.transform.SetParent(slot.SlotTransform, false);
                fuse.transform.localPosition = Vector3.zero;
                fuse.transform.localRotation = Quaternion.identity;
                fuse.gameObject.SetActive(true);

                _panelComponent.FusesById[generatedFuseId] = new FuseItemSpec
                {
                    FuseId = generatedFuseId,
                    Voltage = voltage,
                    Color = fuse.Color,
                    Size = fuse.Size,
                    SourceContainerId = fuse.SourceContainerId,
                    View = fuse
                };

                generated.Slots.Add(new GeneratedSlotState
                {
                    SlotId = slot.SlotId,
                    RequiredVoltage = voltage,
                    AssignedFuseId = generatedFuseId,
                    IsBurned = false
                });
            }

            var burnedCount = Mathf.Clamp(Mathf.CeilToInt(_panelComponent.Slots.Count * BurnedRatio), 1, _panelComponent.Slots.Count);
            var burnedIndices = PickUniqueIndices(_panelComponent.Slots.Count, burnedCount);
            for (var i = 0; i < burnedIndices.Count; i++)
            {
                var slotIndex = burnedIndices[i];
                var slot = _panelComponent.Slots[slotIndex];
                if (slot.CurrentFuseView == null)
                {
                    Debug.LogError("FusePanelFillMechanic: generated slot has no fuse.");
                    return false;
                }

                slot.CurrentFuseView.SetBurned(true);
                generated.Slots[slotIndex].IsBurned = true;
            }

            _panelComponent.CurrentGeneratedState = generated;
            return true;
        }

        private static void ClearSlotFuseInstances(FuseSlotSpec slot)
        {
            slot.CurrentFuseView = null;
            var existingFuses = slot.SlotTransform.GetComponentsInChildren<FuseItemContainer>(true);
            for (var i = 0; i < existingFuses.Length; i++)
            {
                UnityEngine.Object.Destroy(existingFuses[i].gameObject);
            }
        }

        private static List<int> PickUniqueIndices(int maxExclusive, int count)
        {
            var indices = new List<int>(maxExclusive);
            for (var i = 0; i < maxExclusive; i++)
            {
                indices.Add(i);
            }

            Shuffle(indices);
            if (indices.Count > count)
            {
                indices.RemoveRange(count, indices.Count - count);
            }

            return indices;
        }

        private static void Shuffle<T>(IList<T> list)
        {
            for (var i = list.Count - 1; i > 0; i--)
            {
                var j = Random.Range(0, i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }
    }
}
