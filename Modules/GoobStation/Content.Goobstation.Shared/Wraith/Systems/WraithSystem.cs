using Content.Goobstation.Shared.Wraith.Actions;
using Content.Goobstation.Shared.Wraith.Banishment;
using Content.Goobstation.Shared.Wraith.Collisions;
using Content.Goobstation.Shared.Wraith.Components;
using Content.Goobstation.Shared.Wraith.Other;
using Content.Goobstation.Shared.Wraith.WraithPoints;
using Content.Shared.Actions;
using Content.Shared.Actions.Components;
using Content.Shared.StatusEffectNew;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.Wraith.Systems;

public sealed class WraithSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _proto  = default!;
    [Dependency] private readonly WraithPointsSystem _wraithPoints = default!;
    [Dependency] private readonly StatusEffectsSystem _statusEffects = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly AbsorbCorpseSystem _corpse = default!;
    [Dependency] private readonly INetManager _netManager = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    private EntityQuery<WraithPointsComponent> _wraithPointsQuery;
    private EntityQuery<PassiveWraithPointsComponent> _passiveWraithPointsQuery;
    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        _wraithPointsQuery = GetEntityQuery<WraithPointsComponent>();
        _passiveWraithPointsQuery = GetEntityQuery<PassiveWraithPointsComponent>();

        SubscribeLocalEvent<WraithComponent, MapInitEvent>(OnMapInit);

        SubscribeLocalEvent<WraithComponent, BanishmentEvent>(OnBanishment);
        SubscribeLocalEvent<WraithComponent, StatusEffectOnCollideEvent>(OnCollide);

        SubscribeLocalEvent<WraithComponent, WraithWeakenedAddedEvent>(OnWraithWeakenedAdded);
        SubscribeLocalEvent<WraithComponent, WraithWeakenedRemovedEvent>(OnWraithWeakenedRemoved);

        SubscribeLocalEvent<WraithComponent, BanishmentDoneEvent>(OnBanishmentDone);
    }

    private void OnMapInit(Entity<WraithComponent> ent, ref MapInitEvent args) =>
        EntityManager.AddComponents(ent.Owner, _proto.Index(ent.Comp.Abilities));

    private void OnBanishment(Entity<WraithComponent> ent, ref BanishmentEvent args)
    {
        if (!_wraithPointsQuery.TryComp(ent, out var wp)
            || !_passiveWraithPointsQuery.TryComp(ent, out var passiveWp))
            return;

        _wraithPoints.ResetEverything((ent.Owner, wp), passiveWp);

        // reset absorb corpse to original delay
        if (!TryComp<ActionsComponent>(ent.Owner, out var actions))
            return;

        foreach (var action in actions.Actions)
        {
            if (!TryComp<ActionUseDelayOnUseComponent>(action, out var delay))
                return;

            _actions.SetUseDelay(action, delay.OriginalUseDelay);
        }

        // reset absorb corpse
        _corpse.Reset(ent.Owner);
    }

    private void OnCollide(Entity<WraithComponent> ent, ref StatusEffectOnCollideEvent args) =>
        _statusEffects.TryAddStatusEffectDuration(ent.Owner, ent.Comp.WraithWeakenedEffect, args.EffectTimespan);

    private void OnWraithWeakenedAdded(Entity<WraithComponent> ent, ref WraithWeakenedAddedEvent args)
    {
        if (!_timing.IsFirstTimePredicted)
            return;

        EnsureComp<WraithInsanityComponent>(ent.Owner);
    }

    private void OnWraithWeakenedRemoved(Entity<WraithComponent> ent, ref WraithWeakenedRemovedEvent args)
    {
        if (!_timing.IsFirstTimePredicted)
            return;

        RemCompDeferred<WraithInsanityComponent>(ent.Owner);
    }

    private void OnBanishmentDone(Entity<WraithComponent> ent, ref BanishmentDoneEvent args)
    {
        if (_netManager.IsClient)
            return;

        QueueDel(ent.Owner);
        SpawnAtPosition(ent.Comp.WraithDeathEffect, Transform(ent.Owner).Coordinates);
    }
}
