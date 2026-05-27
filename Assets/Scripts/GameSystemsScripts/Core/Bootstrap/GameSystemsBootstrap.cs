using GameSystemsScripts.Core.FusePanel;
using GameSystemsScripts.Core.Runtime;
using UnityEngine;

namespace GameSystemsScripts.Core.Bootstrap
{
    public sealed class GameSystemsBootstrap : MonoBehaviour
    {
        [SerializeField] private SceneSystemsContainer sceneSystemsContainer;
        [SerializeField] private GameState initialState = GameState.Gameplay;

        private GameSystemsHandler _gameSystemsHandler;

        private void Awake()
        {
            _gameSystemsHandler = new GameSystemsHandler();
            _gameSystemsHandler.SetState(initialState);
            LoadGameComponents();
            _gameSystemsHandler.Initialize();
        }

        private void Update()
        {
            _gameSystemsHandler?.Update(Time.deltaTime);
        }

        private void OnDestroy()
        {
            _gameSystemsHandler?.Dispose();
            _gameSystemsHandler = null;
        }

        private void LoadGameComponents()
        {
            if (sceneSystemsContainer == null)
            {
                return;
            }

            if (sceneSystemsContainer.FusePanelContainer != null)
            {
                var sharedFusePanelComponent = new FusePanelComponent();
                _gameSystemsHandler.RegisterSystem(new FusePanelFillSystem(
                    sceneSystemsContainer.FusePanelContainer,
                    sharedFusePanelComponent));
                _gameSystemsHandler.RegisterSystem(new FusePanelSystem(
                    sceneSystemsContainer.FusePanelContainer,
                    sharedFusePanelComponent));
            }
        }
    }
}
