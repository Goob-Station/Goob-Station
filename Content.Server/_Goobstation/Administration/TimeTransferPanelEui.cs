using Content.Server.Administration;
using Content.Server.Administration.Managers;
using Content.Server.EUI;
using Content.Server.Players.PlayTimeTracking;
using Content.Shared._Goobstation.Administration;
using Content.Shared.Administration;
using Content.Shared.Eui;
using Content.Shared.Players.PlayTimeTracking;
using Robust.Server.Player;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using System.Linq;

namespace Content.Server._Goobstation.Administration;

public sealed class TimeTransferPanelEui : BaseEui
{
    [Dependency] private readonly IAdminManager _adminMan = default!;
    [Dependency] private readonly ILogManager _log = default!;
    [Dependency] private readonly IPlayerManager _playerMan = default!;
    [Dependency] private readonly IPrototypeManager _protoMan = default!;
    [Dependency] private readonly PlayTimeTrackingManager _playTimeMan = default!;

    private readonly ISawmill _sawmill;

    public TimeTransferPanelEui()
    {
        IoCManager.InjectDependencies(this);

        _sawmill = _log.GetSawmill("admin.time_eui");
    }

    public override TimeTransferPanelEuiState GetNewState()
    {
        var jobs = _protoMan.EnumeratePrototypes<PlayTimeTrackerPrototype>()
            .Select(job => job.ID)
            .OrderBy(id => id)
            .ToList();

        var hasFlag = _adminMan.HasAdminFlag(Player, AdminFlags.Moderator);

        return new TimeTransferPanelEuiState(jobs, hasFlag);
    }

    public override void HandleMessage(EuiMessageBase msg)
    {
        base.HandleMessage(msg);

        if (msg is not TimeTransferEuiMessage message)
            return;

        AddTime(message.Player, message.Job, message.Time);
    }

    public async void AddTime(NetUserId playerId, string jobId, float time)
    {
        if (!_adminMan.HasAdminFlag(Player, AdminFlags.Moderator))
        {
            _sawmill.Warning($"{Player.Name} ({Player.UserId} tried to add {jobId} role time without moderator flag)");
            return;
        }

        var session = _playerMan.GetSessionById(playerId);
        if (session == null)
        {
            _sawmill.Warning($"Session for player {playerId} not found");
            return;
        }
            

        _playTimeMan.AddTimeToTracker(session, jobId, TimeSpan.FromMinutes(time));
        _playTimeMan.SaveSession(session);

        _sawmill.Info($"{Player.Name} ({Player.UserId} added {time} minutes to {playerId.UserId}'s {jobId} role)");
    }

    public override async void Opened()
    {
        base.Opened();
        StateDirty();
        _adminMan.OnPermsChanged += OnPermsChanged;
    }

    public override void Closed()
    {
        base.Closed();
        _adminMan.OnPermsChanged -= OnPermsChanged;
    }

    private void OnPermsChanged(AdminPermsChangedEventArgs args)
    {
        if (args.Player != Player)
        {
            return;
        }

        StateDirty();
    }
}
