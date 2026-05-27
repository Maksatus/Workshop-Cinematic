namespace GameSystemsScripts.Core.Runtime
{
    public interface IGameSystem
    {
        void Initialize(GameSystemsHandler gameSystemsHandler);
        void UpdateSystem(GameSystemsHandler gameSystemsHandler, float deltaTime);
        void DisposeSystem();
    }
}
