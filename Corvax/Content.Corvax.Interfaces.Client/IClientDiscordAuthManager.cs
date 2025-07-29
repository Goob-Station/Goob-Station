using Content.Corvax.Interfaces.Shared;

namespace Content.Corvax.Interfaces.Client;

public interface IClientDiscordAuthManager : ISharedDiscordAuthManager
{
    public string AuthUrl { get; }
}
