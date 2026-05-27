using System;
using System.Collections.Generic;

namespace GameSystemsScripts.Core.Runtime
{
    public sealed class GameSystemsHandler : IDisposable
    {
        private readonly List<IGameSystem> _systems = new List<IGameSystem>();

        public GameState CurrentState { get; private set; } = GameState.Gameplay;
        public bool IsDisposed { get; private set; }

        public void RegisterSystem(IGameSystem system)
        {
            if (system == null)
            {
                throw new ArgumentNullException(nameof(system));
            }

            _systems.Add(system);
        }

        public void Initialize()
        {
            for (var i = 0; i < _systems.Count; i++)
            {
                _systems[i].Initialize(this);
            }
        }

        public void Update(float deltaTime)
        {
            if (IsDisposed)
            {
                return;
            }

            for (var i = 0; i < _systems.Count; i++)
            {
                _systems[i].UpdateSystem(this, deltaTime);
            }
        }

        public void SetState(GameState state)
        {
            CurrentState = state;
        }

        public void Dispose()
        {
            if (IsDisposed)
            {
                return;
            }

            for (var i = _systems.Count - 1; i >= 0; i--)
            {
                _systems[i].DisposeSystem();
            }

            _systems.Clear();
            IsDisposed = true;
        }
    }
}
