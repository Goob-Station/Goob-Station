using Content.Casino.Client.Games;
using Content.Casino.Shared;
using Content.Casino.Shared.Games;
using Robust.Shared.Reflection;

namespace Content.Casino.Client;

/// <summary>
/// Manages client-side game implementations by automatically instantiating
/// and initializing all types that implement IGameClient.
/// </summary>
public sealed class ClientGameManager : IPostInjectInit
{
    [Dependency] private readonly IDynamicTypeFactory _typeFactory = default!;
    [Dependency] private readonly IReflectionManager _reflectionManager = default!;

    private readonly Dictionary<Type, IGameClient> _gameClients = new();

    public void PostInject()
    {
        IoCManager.InjectDependencies(this);

        var clientTypes = _reflectionManager.GetAllChildren<IGameClient>();

        foreach (var type in clientTypes)
        {
            if (type.IsAbstract || type.IsInterface)
                continue;

            var client = _typeFactory.CreateInstance<IGameClient>(type);
            client.Initialize();

            _gameClients.Add(type, client);
        }
    }

    public T GetClient<T>() where T : IGameClient
    {
        if (_gameClients.TryGetValue(typeof(T), out var client))
            return (T)client;

        throw new InvalidOperationException($"Game client of type {typeof(T).Name} not found.");
    }

    public bool HasClient<T>() where T : IGameClient
    {
        return _gameClients.ContainsKey(typeof(T));
    }
}

