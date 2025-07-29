namespace Content.Corvax.Interfaces.Server;

public interface IServerJoinQueueManager
{
    public bool IsEnabled { get; }
    public int PlayerInQueueCount { get; }
    public int ActualPlayersCount { get; }
    public void Initialize();
    public void PostInitialize();
}
