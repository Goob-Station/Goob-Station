using Content.Goobstation.Shared.Wraith.Banishment;
using Content.Goobstation.Shared.Wraith.Collisions;
using Content.Goobstation.Shared.Wraith.Components;
using Content.Goobstation.Shared.Wraith.WraithPoints;
using Content.Shared.StatusEffectNew;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Wraith.Systems;

public sealed class WraithSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _proto  = default!;
    [Dependency] private readonly WraithPointsSystem _wraithPoints = default!;
    [Dependency] private readonly StatusEffectsSystem _statusEffects = default!;

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
    }

    private void OnMapInit(Entity<WraithComponent> ent, ref MapInitEvent args) =>
        EntityManager.AddComponents(ent.Owner, _proto.Index(ent.Comp.Abilities));

    private void OnBanishment(Entity<WraithComponent> ent, ref BanishmentEvent args)
    {
        if (!_wraithPointsQuery.TryComp(ent, out var wp)
            || !_passiveWraithPointsQuery.TryComp(ent, out var passiveWp))
            return;

        _wraithPoints.ResetEverything((ent.Owner, wp), passiveWp);

        //TO DO: reset absorb corpse cd
    }

    private void OnCollide(Entity<WraithComponent> ent, ref StatusEffectOnCollideEvent args) =>
        _statusEffects.TryAddStatusEffectDuration(ent.Owner, ent.Comp.WraithWeakenedEffect, args.EffectTimespan);
}
