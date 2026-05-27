using GameSystemsScripts.Core.Runtime;
using UnityEngine;

namespace GameSystemsScripts.Core.FusePanel
{
    public sealed class FusePanelSystem : IGameSystem
    {
        private readonly FusePanelContainer _container;
        private readonly GameState _activeState;

        public FusePanelComponent Component { get; }
        public FusePanelMechanic Mechanic { get; }

        public FusePanelSystem(
            FusePanelContainer container,
            FusePanelComponent sharedComponent = null,
            GameState activeState = GameState.Gameplay)
        {
            _container = container;
            _activeState = activeState;
            Component = sharedComponent ?? new FusePanelComponent();
            Mechanic = new FusePanelMechanic(Component, container);
            BuildRuntimeState();
        }

        public void Initialize(GameSystemsHandler gameSystemsHandler)
        {
            if (_container != null && _container.PanelRoot != null)
            {
                _container.PanelRoot.SetActive(false);
            }
        }

        public void UpdateSystem(GameSystemsHandler gameSystemsHandler, float deltaTime)
        {
            if (_container == null)
            {
                return;
            }

            if (gameSystemsHandler.CurrentState != _activeState)
            {
                return;
            }

            Mechanic.UpdateMechanic(gameSystemsHandler, deltaTime);
        }

        public void DisposeSystem()
        {
            Mechanic.DisposeMechanic();
        }

        public void BuildRuntimeState()
        {
            if (_container == null)
            {
                return;
            }

            Component.Slots.Clear();
            Component.FusesById.Clear();
            Component.ContainersById.Clear();
            Component.IsGenerationReady = false;
            Component.IsGenerationFailed = false;
            Component.CurrentGeneratedState = null;

            for (var i = 0; i < _container.SlotViews.Count; i++)
            {
                var slotView = _container.SlotViews[i];
                if (slotView == null)
                {
                    continue;
                }

                var slot = new FuseSlotSpec
                {
                    SlotId = slotView.SlotId,
                    RequiredVoltage = slotView.RequiredVoltage,
                    SlotTransform = slotView.transform
                };
                Component.Slots.Add(slot);
            }

            for (var i = 0; i < _container.FuseViews.Count; i++)
            {
                var fuseView = _container.FuseViews[i];
                if (fuseView == null || string.IsNullOrWhiteSpace(fuseView.FuseId))
                {
                    continue;
                }

                Component.FusesById[fuseView.FuseId] = new FuseItemSpec
                {
                    FuseId = fuseView.FuseId,
                    Voltage = fuseView.Voltage,
                    Color = fuseView.Color,
                    Size = fuseView.Size,
                    SourceContainerId = fuseView.SourceContainerId,
                    View = fuseView
                };
            }

            for (var i = 0; i < _container.SourceContainers.Count; i++)
            {
                var source = _container.SourceContainers[i];
                if (source == null || string.IsNullOrWhiteSpace(source.ContainerId))
                {
                    continue;
                }

                Component.ContainersById[source.ContainerId] = source;
            }

            // Capture startup placements: fuses already under slot transforms are treated as installed.
            for (var s = 0; s < Component.Slots.Count; s++)
            {
                var slot = Component.Slots[s];
                if (slot.SlotTransform == null)
                {
                    continue;
                }

                var installedFuse = slot.SlotTransform.GetComponentInChildren<FuseItemContainer>();
                slot.CurrentFuseView = installedFuse;
            }
        }
    }
}
