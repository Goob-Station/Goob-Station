using Content.Goobstation.Shared.CrematorImmune;
using Content.Goobstation.Shared.Devil;
using Content.Goobstation.Shared.Devil.Actions;
using Content.Goobstation.Shared.Devil.Components;
using Content.Goobstation.Shared.Religion;
using Content.Goobstation.Shared.Supermatter.Components;
using Content.Server.Actions;
using Content.Server.Antag.Components;
using Content.Server.Atmos.Components;
using Content.Server.Speech.Components;
using Content.Server.Zombies;
using Content.Shared._Shitmed.Body.Components;
using Content.Shared.Actions;
using Content.Shared.Actions.Components;
using Content.Shared.CombatMode;
using Content.Shared.Mind;
using Content.Shared.Shuttles.Components;

namespace Content.Goobstation.Server.Devil.Systems;

public sealed class DevilTransformSystem : EntitySystem
{
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly ActionContainerSystem _actionContainer = default!;
    [Dependency] private readonly ActionsSystem _actions = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DevilComponent, BecomeLesserDevilEvent>(OnBecomeLesser);
        SubscribeLocalEvent<DevilComponent, BecomeArchdevilEvent>(OnBecomeArch);
        SubscribeLocalEvent<DevilTransformComponent, MapInitEvent>(OnMapInit);
    }

    private void OnMapInit(EntityUid uid, DevilTransformComponent comp, MapInitEvent args)
    {
        if (HasComp<ArchdevilComponent>(uid))
        {
            // Archdevil gets archfire
            if (comp.ArchFireActionEntity == null)
                _actions.AddAction(uid, ref comp.ArchFireActionEntity, comp.ArchFireAction);
        }
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
        if (!TryComp<DevilTransformComponent>(uid, out var comp))
            return;

        var xform = Transform(uid);
        var newEntity = Spawn(prototype, xform.Coordinates);

        // Transfer mind
        if (_mind.TryGetMind(uid, out var mindId, out var mind))
        {
            _mind.TransferTo(mindId, newEntity, mind: mind);
        }

        // Transfer actions
        if (TryComp<ActionsContainerComponent>(uid, out var oldActions) &&
            TryComp<ActionsContainerComponent>(newEntity, out var newActions))
        {
            _actionContainer.TransferAllActionsWithNewAttached(uid, newEntity, newEntity);

            // Prevent shutdown systems from trying to remove already-transferred actions
            RemCompDeferred<ActionsContainerComponent>(uid);
        }

        if (TryComp<DevilComponent>(uid, out var oldDevil))
        {
            var newDevil = EnsureComp<DevilComponent>(newEntity);
            newDevil.Souls = oldDevil.Souls;
            newDevil.PowerLevel = oldDevil.PowerLevel;
        }

        CopyDevilComponents(uid, newEntity);
        EnsureComp<CombatModeComponent>(newEntity);

        // Get the transform component from the new entity
        if (!TryComp<DevilTransformComponent>(newEntity, out var newComp))
        {
            Del(uid);
            return;
        }

        if (lesser)
        {
            EnsureComp<DevilLesserFormComponent>(newEntity);
            _actions.RemoveAction(newEntity, newComp.JauntActionEntity);
        }
        else
        {
            RemComp<DevilLesserFormComponent>(newEntity);
            EnsureComp<ArchdevilComponent>(newEntity);

            if (newComp.HellfireActionEntity != null)
            {
                _actions.RemoveAction(newEntity, newComp.HellfireActionEntity);
            }

            if (newComp.ArchFireActionEntity == null)
            {
                _actions.AddAction(newEntity, ref newComp.ArchFireActionEntity, newComp.ArchFireAction);
            }
        }

        // Delete old entity
        Del(uid);
    }


    private void CopyIfExists<T>(EntityUid oldUid, EntityUid newUid)
        where T : Component, new()
    {
        if (HasComp<T>(oldUid))
            EnsureComp<T>(newUid);
    }

    private void CopyDevilComponents(EntityUid oldUid, EntityUid newUid)
    {
        // Abilities
        CopyIfExists<DevilAuthorityComponent>(oldUid, newUid);
        CopyIfExists<DevilGripComponent>(oldUid, newUid);
        CopyIfExists<DevilHeresyComponent>(oldUid, newUid);
        CopyIfExists<DevilSummonPitchforkComponent>(oldUid, newUid);
        CopyIfExists<HellstepActionComponent>(oldUid, newUid);

        // Grip effects
        CopyIfExists<GripSidegradeRotComponent>(oldUid, newUid);
        CopyIfExists<GripSidegradeStunComponent>(oldUid, newUid);

        // Traits
        CopyIfExists<NoLimbForYouComponent>(oldUid, newUid);
        CopyIfExists<PreventBucklingComponent>(oldUid, newUid);
        CopyIfExists<UncontainableComponent>(oldUid, newUid);

        // Original traits
        CopyIfExists<ZombieImmuneComponent>(oldUid, newUid);
        CopyIfExists<BreathingImmunityComponent>(oldUid, newUid);
        CopyIfExists<PressureImmunityComponent>(oldUid, newUid);
        CopyIfExists<ActiveListenerComponent>(oldUid, newUid);
        CopyIfExists<WeakToHolyComponent>(oldUid, newUid);
        CopyIfExists<CrematoriumImmuneComponent>(oldUid, newUid);
        CopyIfExists<AntagImmuneComponent>(oldUid, newUid);
        CopyIfExists<SupermatterImmuneComponent>(oldUid, newUid);
        CopyIfExists<FTLSmashImmuneComponent>(oldUid, newUid);
    }
}
