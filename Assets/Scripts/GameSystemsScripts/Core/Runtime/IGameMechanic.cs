namespace GameSystemsScripts.Core.Runtime
{
    public interface IGameMechanic
    {
        void UpdateMechanic(GameSystemsHandler gameSystemsHandler, float deltaTime);
        void DisposeMechanic();
    }
}
