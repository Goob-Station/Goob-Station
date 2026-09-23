using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Content.Server.Database;
using Robust.Shared.Network;
using Robust.Shared.Player;

namespace Content.Goobstation.Server.Slasher;

public sealed class SlasherPrestigeManager
{
    [Dependency] private readonly IServerDbManager _db = default!;
    [Dependency] private readonly UserDbDataManager _userDb = default!;

    private readonly Dictionary<NetUserId, HashSet<string>> _ascensions = new();

    public void Initialize()
    {
        _userDb.AddOnLoadPlayer(LoadData);
        _userDb.AddOnPlayerDisconnect(session => _ascensions.Remove(session.UserId));
    }

    private async Task LoadData(ICommonSession session, CancellationToken cancel)
    {
        var stored = await _db.GetSlasherAscensionsAsync(session.UserId);
        cancel.ThrowIfCancellationRequested();
        _ascensions[session.UserId] = stored.ToHashSet();
    }

    public bool HasAscension(NetUserId user, string ascensionId)
        => _ascensions.TryGetValue(user, out var set) && set.Contains(ascensionId);

    public async void GrantAscension(NetUserId user, string ascensionId)
    {
        if (_ascensions.TryGetValue(user, out var set) && !set.Add(ascensionId))
            return;

        await _db.AddSlasherAscensionAsync(user, ascensionId);
    }
}
