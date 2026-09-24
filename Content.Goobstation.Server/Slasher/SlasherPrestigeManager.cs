using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Content.Server.Database;
using Robust.Shared.Asynchronous;
using Robust.Shared.Network;
using Robust.Shared.Player;

namespace Content.Goobstation.Server.Slasher;

public sealed class SlasherPrestigeManager : IPostInjectInit
{
    [Dependency] private readonly IServerDbManager _db = default!;
    [Dependency] private readonly ILogManager _logManager = default!;
    [Dependency] private readonly ITaskManager _task = default!;
    [Dependency] private readonly UserDbDataManager _userDb = default!;

    private readonly Dictionary<NetUserId, HashSet<string>> _ascensions = new();
    private readonly List<Task> _pendingSaveTasks = new();
    private ISawmill _sawmill = default!;

    void IPostInjectInit.PostInject()
    {
        _userDb.AddOnLoadPlayer(LoadData);
        _userDb.AddOnPlayerDisconnect(ClientDisconnected);
        _sawmill = _logManager.GetSawmill("slasher_prestige");
    }

    public void Shutdown()
    {
        _task.BlockWaitOnTask(Task.WhenAll(_pendingSaveTasks));
    }

    private async Task LoadData(ICommonSession session, CancellationToken cancel)
    {
        var stored = await _db.GetSlasherAscensions(session.UserId, cancel);
        cancel.ThrowIfCancellationRequested();
        _ascensions[session.UserId] = stored.ToHashSet();
    }

    private void ClientDisconnected(ICommonSession session)
    {
        _ascensions.Remove(session.UserId);
    }

    public bool HasAscension(NetUserId user, string ascensionId)
    {
        if (_ascensions.TryGetValue(user, out var ascensions))
            return ascensions.Contains(ascensionId);

        _sawmill.Error($"Tried to check the ascensions of {user} before they were loaded:\n{Environment.StackTrace}");
        return false;
    }

    public void GrantAscension(NetUserId user, string ascensionId)
    {
        if (!_ascensions.TryGetValue(user, out var ascensions))
        {
            _sawmill.Error($"Tried to grant {ascensionId} to {user} before load:\n{Environment.StackTrace}");
            return;
        }

        if (!ascensions.Add(ascensionId))
            return;

        _sawmill.Info($"{user} ascended as {ascensionId}");
        TrackPending(_db.AddSlasherAscension(user, ascensionId));
    }

    private async void TrackPending(Task task)
    {
        _pendingSaveTasks.Add(task);

        try
        {
            await task;
        }
        finally
        {
            _pendingSaveTasks.Remove(task);
        }
    }
}
