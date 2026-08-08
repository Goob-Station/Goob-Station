using Content.Server.Database;
using Robust.Server.Player;
using Robust.Shared.Enums;
using Robust.Shared.Network;
using Robust.Shared.Player;

namespace Content.Goobstation.Server.Slasher.Systems;

/// <summary>
/// Tracks which slasher prestige ascensions each player has earned.
/// </summary>
public sealed class SlasherPrestigeSystem : EntitySystem
{
    [Dependency] private readonly IServerDbManager _db = default!;
    [Dependency] private readonly IPlayerManager _player = default!;

    private readonly Dictionary<NetUserId, HashSet<string>> _ascensions = new();

    public override void Initialize()
    {
        base.Initialize();

        _player.PlayerStatusChanged += OnPlayerStatusChanged;
    }

    public override void Shutdown()
    {
        base.Shutdown();

        _player.PlayerStatusChanged -= OnPlayerStatusChanged;
    }

    private async void OnPlayerStatusChanged(object? sender, SessionStatusEventArgs e)
    {
        if (e.NewStatus != SessionStatus.Connected)
            return;

        var userId = e.Session.UserId;
        var stored = await _db.GetSlasherAscensionsAsync(userId);

        var set = GetOrCreate(userId);
        set.UnionWith(stored);
    }

    public bool HasAscension(NetUserId user, string ascensionId)
    {
        return _ascensions.TryGetValue(user, out var set) && set.Contains(ascensionId);
    }

    public void GrantAscension(NetUserId user, string ascensionId)
    {
        var set = GetOrCreate(user);

        if (!set.Add(ascensionId))
            return;

        _db.AddSlasherAscensionAsync(user, ascensionId);
    }

    private HashSet<string> GetOrCreate(NetUserId user)
    {
        if (!_ascensions.TryGetValue(user, out var set))
        {
            set = new HashSet<string>();
            _ascensions[user] = set;
        }

        return set;
    }
}
