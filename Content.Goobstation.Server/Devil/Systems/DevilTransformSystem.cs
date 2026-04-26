using Content.Goobstation.Shared.Devil;
using Content.Goobstation.Shared.Devil.Actions;
using Content.Goobstation.Shared.Devil.Components;
using Content.Server.Actions;
using Content.Shared.Actions;
using Content.Shared.Actions.Components;
using Content.Shared.CombatMode;
using Content.Shared.Mind;
using System.Linq;

namespace Content.Goobstation.Server.Devil.Systems;

public sealed class DevilTransformSystem : EntitySystem
{
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly ActionsSystem _actions = default!;
    [Dependency] private readonly ActionContainerSystem _actionContainer = default!;

    private EntityQuery<ActionsComponent> _actionsQuery;

    public override void Initialize()
    {
        base.Initialize();

        _actionsQuery = GetEntityQuery<ActionsComponent>();

        SubscribeLocalEvent<DevilComponent, BecomeLesserDevilEvent>(OnBecomeLesser);
        SubscribeLocalEvent<DevilComponent, BecomeArchdevilEvent>(OnBecomeArch);
    }

    private void OnBecomeLesser(EntityUid uid, DevilComponent comp, BecomeLesserDevilEvent args)
    {
        Transform(uid, args.Prototype, lesser: true);
    }

    private void OnBecomeArch(EntityUid uid, DevilComponent comp, BecomeArchdevilEvent args)
    {
        Transform(uid, args.Prototype, lesser: false);
    }

    private void Transform(EntityUid uid, string prototype, bool lesser)
    {
        if (!TryComp<DevilTransformComponent>(uid, out var oldComp))
            return;

        var coords = Transform(uid).Coordinates;
        var newEntity = Spawn(prototype, coords);

        if (!EntityManager.TryCopyComponent(uid, newEntity, ref oldComp, out var newComp))
        {
            Del(uid);
            return;
        }

        // Transfer soul/power state explicitly because DevilComponent values
        if (TryComp<DevilComponent>(uid, out var oldDevil) &&
            TryComp<DevilComponent>(newEntity, out var newDevil))
        {
            newDevil.Souls = oldDevil.Souls;
            newDevil.PowerLevel = oldDevil.PowerLevel;
        }

        // Clear the old entity's action bar entries before transferring,
        if (_actionsQuery.TryComp(uid, out var oldActionsComp))
        {
            foreach (var actionId in oldActionsComp.Actions.ToArray())
            {
                _actions.RemoveAction((uid, oldActionsComp), actionId);
            }
        }

        if (TryComp<ActionsContainerComponent>(uid, out _) &&
            TryComp<ActionsContainerComponent>(newEntity, out _))
        {
            _actionContainer.TransferAllActionsWithNewAttached(uid, newEntity, newEntity);
        }

        // Transfer mind last, after the new entity is fully set up.
        if (_mind.TryGetMind(uid, out var mindId, out var mind))
            _mind.TransferTo(mindId, newEntity, mind: mind);

        EnsureComp<CombatModeComponent>(newEntity);

        if (lesser)
        {
            EnsureComp<DevilLesserFormComponent>(newEntity);

            // Lesser devils don't get jaunt or archfire.
            _actions.RemoveAction(newEntity, newComp.JauntActionEntity);
            _actions.RemoveAction(newEntity, newComp.ArchFireActionEntity);
        }
        else
        {
            RemComp<DevilLesserFormComponent>(newEntity);
            EnsureComp<ArchdevilComponent>(newEntity);

            // Archdevils lose hellfire, gain archfire.
            _actions.RemoveAction(newEntity, newComp.HellfireActionEntity);

            if (newComp.ArchFireActionEntity == null)
                _actions.AddAction(newEntity, ref newComp.ArchFireActionEntity, newComp.ArchFireAction);
        }

        Del(uid);
    }
}
