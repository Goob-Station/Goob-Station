using Content.Server.EUI;
using Content.Shared.Actions.Components;
using Content.Shared.Actions.Events;
using Content.Shared.Eui;

namespace Content.Server.Actions;

public sealed partial class ConfirmableActionEui : BaseEui
{
    private readonly Entity<ConfirmableActionComponent> _ent;
    private readonly EntityUid _user;
    private readonly IEntityManager _entityManager;

    public ConfirmableActionEui(Entity<ConfirmableActionComponent> ent, EntityUid user, IEntityManager entMan)
    {
        _ent = ent;
        _user = user;
        _entityManager = entMan;
    }

    public override void HandleMessage(EuiMessageBase msg)
    {
        base.HandleMessage(msg);

        if (msg is not ConfirmableActionEuiMessage { } amsg
        || !amsg.Accepted)
            return;

        var actionsSystem = _entityManager.System<ActionsSystem>();
        if (_entityManager.TryGetComponent<ActionComponent>(_ent, out var action))
            actionsSystem.PerformAction(_user, (_ent, action));
    }
}
