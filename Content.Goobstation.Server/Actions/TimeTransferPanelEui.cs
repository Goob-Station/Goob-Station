using Content.Goobstation.Shared.Administration;
using Content.Server.Administration;
using Content.Server.Administration.Commands;
using Content.Server.Administration.Managers;
using Content.Server.Database;
using Content.Server.EUI;
using Content.Shared.Administration;
using Content.Shared.Eui;
using Robust.Shared.Network;

namespace Content.Goobstation.Server.Actions;

public sealed class TimeTransferPanelEui : BaseEui
{
    [Dependency] private readonly IAdminManager _adminMan = default!;
    [Dependency] private readonly ILogManager _log = default!;
    [Dependency] private readonly IPlayerLocator _playerLocator = default!;
    [Dependency] private readonly IServerDbManager _databaseMan = default!;

    private readonly ISawmill _sawmill;

    public TimeTransferPanelEui()
    {
        IoCManager.InjectDependencies(this);

        _sawmill = _log.GetSawmill("admin.time_eui");
    }

    public override TimeTransferPanelEuiState GetNewState()
    {
        var hasFlag = _adminMan.HasAdminFlag(Player, AdminFlags.Moderator);

        return new TimeTransferPanelEuiState(hasFlag);
    }

    public override void HandleMessage(EuiMessageBase msg)
    {
        base.HandleMessage(msg);

        if (msg is not TimeTransferEuiMessage message)
            return;

        TransferTime(message.PlayerId, message.TimeData, message.Overwrite);
    }

    public async void TransferTime(string playerId, List<TimeTransferData> timeData, bool overwrite)
    {
        if (!_adminMan.HasAdminFlag(Player, AdminFlags.Moderator))
        {
            _sawmill.Warning($"{Player.Name} ({Player.UserId} tried to add roles time without moderator flag)");
            return;
        }

        var playerData = await _playerLocator.LookupIdByNameAsync(playerId);
        if (playerData == null)
        {
            _sawmill.Warning($"{Player.Name} ({Player.UserId} tried to add roles time to not existing player {playerId})");
            SendMessage(new TimeTransferWarningEuiMessage(Loc.GetString("time-transfer-panel-no-player-database-message"), Color.Red));
            return;
        }

        if (overwrite)
            SetTime(playerData.UserId, timeData);
        else
            AddTime(playerData.UserId, timeData);
    }

    public async void SetTime(NetUserId userId, List<TimeTransferData> timeData)
    {
        var updateList = new List<PlayTimeUpdate>();

        foreach (var data in timeData)
        {
            var time = TimeSpan.FromMinutes(PlayTimeCommandUtilities.CountMinutes(data.TimeString));
            updateList.Add(new PlayTimeUpdate(userId, data.PlaytimeTracker, time));
        }

        await _databaseMan.UpdatePlayTimes(updateList);

        _sawmill.Info($"{Player.Name} ({Player.UserId} saved {updateList.Count} trackers for {userId})");

        SendMessage(new TimeTransferWarningEuiMessage(Loc.GetString("time-transfer-panel-warning-set-success"), Color.LightGreen));
    }

    public async void AddTime(NetUserId userId, List<TimeTransferData> timeData)
    {
        var playTimeList = await _databaseMan.GetPlayTimes(userId);

        Dictionary<string, TimeSpan> playTimeDict = new();

        foreach (var playTime in playTimeList)
        {
            playTimeDict.Add(playTime.Tracker, playTime.TimeSpent);
        }

        var updateList = new List<PlayTimeUpdate>();

        foreach (var data in timeData)
        {
            var time = TimeSpan.FromMinutes(PlayTimeCommandUtilities.CountMinutes(data.TimeString));
            if (playTimeDict.TryGetValue(data.PlaytimeTracker, out var addTime))
                time += addTime;

            updateList.Add(new PlayTimeUpdate(userId, data.PlaytimeTracker, time));
        }

        await _databaseMan.UpdatePlayTimes(updateList);

        _sawmill.Info($"{Player.Name} ({Player.UserId} saved {updateList.Count} trackers for {userId})");

        SendMessage(new TimeTransferWarningEuiMessage(Loc.GetString("time-transfer-panel-warning-add-success"), Color.LightGreen));
    }

    public override async void Opened()
    {
        base.Opened();
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
