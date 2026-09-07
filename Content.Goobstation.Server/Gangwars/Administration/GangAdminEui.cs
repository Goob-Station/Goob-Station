using Content.Goobstation.Shared.Gangwars.Administration;
using Content.Goobstation.Shared.Gangwars.Systems;
using Content.Server.Administration;
using Content.Server.Administration.Managers;
using Content.Server.EUI;
using Content.Shared.Administration;
using Content.Shared.Administration.Logs;
using Content.Shared.Database;
using Content.Shared.Eui;

namespace Content.Goobstation.Server.Gangwars.Administration;

/// <summary>
/// Server side of the admin gang panel.
/// </summary>
public sealed class GangAdminEui : BaseEui
{
    [Dependency] private readonly IEntityManager _entMan = default!;
    [Dependency] private readonly IAdminManager _adminMan = default!;
    [Dependency] private readonly ISharedAdminLogManager _adminLog = default!;

    private GangwarRuleSystem _gangs = default!;

    public GangAdminEui()
    {
        IoCManager.InjectDependencies(this);
        _gangs = _entMan.System<GangwarRuleSystem>();
    }

    public override GangAdminEuiState GetNewState()
    {
        return new GangAdminEuiState(new List<GangAdminInfo>(_gangs.BuildGangList()), _gangs.BuildPlayerList());
    }

    public override void HandleMessage(EuiMessageBase msg)
    {
        base.HandleMessage(msg);

        if (!_adminMan.HasAdminFlag(Player, AdminFlags.Admin))
            return;

        switch (msg)
        {
            case GangAdminManualRenameMessage rename:
                _gangs.MigrateGang(rename.OldColor, rename.NewColor, rename.NewName.Trim());
                _adminLog.Add(LogType.Action, LogImpact.High,
                    $"{Player.Name} manually renamed gang {rename.OldColor.ToHex()} to \"{rename.NewName.Trim()}\" with color {rename.NewColor.ToHex()}");
                break;

            case GangAdminForceRenameMessage force:
                if (_gangs.ForceRemake(force.Color))
                    _adminLog.Add(LogType.Action, LogImpact.High,
                        $"{Player.Name} forced gang {force.Color.ToHex()} to remake itself");
                break;

            case GangAdminKickMessage kick:
                var kickUid = _entMan.GetEntity(kick.Member);
                if (_gangs.KickMember(kickUid))
                    _adminLog.Add(LogType.Action, LogImpact.High,
                        $"{Player.Name} kicked {_entMan.ToPrettyString(kickUid)} from their gang");
                break;

            case GangAdminSetLeaderMessage setLeader:
                var leaderUid = _entMan.GetEntity(setLeader.Member);
                if (_gangs.SetLeader(leaderUid))
                    _adminLog.Add(LogType.Action, LogImpact.High,
                        $"{Player.Name} set {_entMan.ToPrettyString(leaderUid)} as their gang's leader");
                break;

            case GangAdminSetPointsMessage setPoints:
                var pointsUid = _entMan.GetEntity(setPoints.Member);
                if (_gangs.SetMemberPoints(pointsUid, setPoints.Points))
                    _adminLog.Add(LogType.Action, LogImpact.High,
                        $"{Player.Name} set {_entMan.ToPrettyString(pointsUid)}'s gang points to {setPoints.Points}");
                break;

            case GangAdminAddMemberMessage addMember:
                var addUid = _entMan.GetEntity(addMember.Player);
                if (_gangs.AddMember(addMember.GangColor, addUid))
                    _adminLog.Add(LogType.Action, LogImpact.High,
                        $"{Player.Name} added {_entMan.ToPrettyString(addUid)} to gang {addMember.GangColor.ToHex()}");
                break;

            default:
                return;
        }

        StateDirty();
    }

    public override void Opened()
    {
        base.Opened();
        _adminMan.OnPermsChanged += OnPermsChanged;
        _entMan.System<GangAdminRefreshSystem>().Register(this);
        StateDirty();
    }

    public override void Closed()
    {
        base.Closed();
        _adminMan.OnPermsChanged -= OnPermsChanged;
        _entMan.System<GangAdminRefreshSystem>().Unregister(this);
    }

    private void OnPermsChanged(AdminPermsChangedEventArgs args)
    {
        if (args.Player == Player)
            StateDirty();
    }
}
