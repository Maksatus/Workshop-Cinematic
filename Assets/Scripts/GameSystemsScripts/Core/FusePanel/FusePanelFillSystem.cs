using GameSystemsScripts.Core.Runtime;

namespace GameSystemsScripts.Core.FusePanel
{
    public sealed class FusePanelFillSystem : IGameSystem
    {
        private readonly GameState _activeState;
        private readonly FusePanelContainer _container;

        public FusePanelFillComponent Component { get; }
        public FusePanelFillMechanic Mechanic { get; }

        public FusePanelFillSystem(
            FusePanelContainer container,
            FusePanelComponent panelComponent,
            GameState activeState = GameState.Gameplay)
        {
            _container = container;
            _activeState = activeState;
            Component = new FusePanelFillComponent();
            Mechanic = new FusePanelFillMechanic(panelComponent, Component, container);
        }

        public void Initialize(GameSystemsHandler gameSystemsHandler)
        {
        }

        public void UpdateSystem(GameSystemsHandler gameSystemsHandler, float deltaTime)
        {
            if (_container == null || gameSystemsHandler.CurrentState != _activeState)
            {
                return;
            }

            Mechanic.UpdateMechanic(gameSystemsHandler, deltaTime);
        }

        public void DisposeSystem()
        {
            Mechanic.DisposeMechanic();
        }
    }
}
