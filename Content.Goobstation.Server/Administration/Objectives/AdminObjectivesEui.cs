// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Administration.Objectives;
using Content.Server.Administration.Managers;
using Content.Server.EUI;
using Content.Shared.Administration;
using Content.Shared.Eui;

namespace Content.Goobstation.Server.Administration.Objectives;

/// <summary>
/// Admin window to view, add and remove the objectives of one mind.
/// </summary>
public sealed class AdminObjectivesEui : BaseEui
{
    [Dependency] private readonly IAdminManager _adminMan = default!;
    [Dependency] private readonly IEntityManager _entMan = default!;
    [Dependency] private readonly ILogManager _log = default!;

    private readonly AdminObjectivesSystem _system;
    private readonly ISawmill _sawmill;

    private readonly EntityUid _mind;
    private readonly string _playerName;

    public AdminObjectivesEui(EntityUid mind, string playerName)
    {
        IoCManager.InjectDependencies(this);

        _system = _entMan.System<AdminObjectivesSystem>();
        _sawmill = _log.GetSawmill("admin.objectives_eui");
        _mind = mind;
        _playerName = playerName;
    }

    public override EuiStateBase GetNewState()
    {
        return _system.BuildState(_mind, _playerName);
    }

    public override void HandleMessage(EuiMessageBase msg)
    {
        base.HandleMessage(msg);

        if (msg is AdminObjectivesRefreshMessage)
        {
            StateDirty();
            return;
        }

        if (!_adminMan.HasAdminFlag(Player, AdminFlags.Admin))
        {
            _sawmill.Warning($"{Player.Name} ({Player.UserId}) tried to edit objectives without the admin flag");
            return;
        }

        bool success;
        string message;
        switch (msg)
        {
            case AdminObjectivesAddPrototypeMessage add:
                EntityUid? target = add.TargetMind is { } netTarget ? _entMan.GetEntity(netTarget) : null;
                success = _system.TryAddPrototype(Player, _mind, add.ProtoId, target, add.Bypass, out message);
                break;
            case AdminObjectivesAddCustomMessage custom:
                success = _system.TryAddCustom(Player, _mind, custom.Title, custom.Description, custom.Issuer, out message);
                break;
            case AdminObjectivesAddStealMessage steal:
                success = _system.TryAddSteal(Player, _mind, _entMan.GetEntity(steal.Item), steal.Issuer, out message);
                break;
            case AdminObjectivesRemoveMessage remove:
                success = _system.TryRemove(Player, _mind, _entMan.GetEntity(remove.Objective), out message);
                break;
            default:
                return;
        }

        SendMessage(new AdminObjectivesResultMessage(message, success));
        StateDirty();
    }
}
