using Content.Goobstation.Shared.SpecialPassives.BoostedImmunity.Components;
using Content.Server.GameTicking;
using Content.Shared.Mobs.Components;
using Content.Shared.Traits;
using Robust.Server.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.SpecialPassives.BoostedImmunity;

public sealed class BoostedImmunitySystem : SharedBoostedImmunitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly IPrototypeManager _protoManager = default!;
    [Dependency] private readonly GameTicker _ticker = default!;

    private EntityQuery<MobStateComponent> _mobStateQuery;

    public override void Initialize()
    {
        base.Initialize();

        _mobStateQuery = GetEntityQuery<MobStateComponent>();

        SubscribeLocalEvent<BoostedImmunityComponent, MapInitEvent>(OnMapInit);
    }

    private void OnMapInit(Entity<BoostedImmunityComponent> ent, ref MapInitEvent args)
    {
        ent.Comp.UpdateTimer = _timing.CurTime + ent.Comp.UpdateDelay;

        if (ent.Comp.Duration.HasValue)
            ent.Comp.MaxDuration = _timing.CurTime + TimeSpan.FromSeconds((double) ent.Comp.Duration);

        if (_mobStateQuery.TryComp(ent, out var state))
            ent.Comp.Mobstate = state.CurrentState;

        RemoveDisabilities(ent);

        Cycle(ent);
    }

    public readonly ProtoId<TraitCategoryPrototype> DisabilityProto = "Disabilities";
    private void RemoveDisabilities(Entity<BoostedImmunityComponent> ent)
    {
        _playerManager.TryGetSessionByEntity(ent, out var session);

        if (session == null)
            return;

        var profile = _ticker.GetPlayerProfile(session);

        foreach (var trait in profile.TraitPreferences)
        {
            if (!_protoManager.TryIndex<TraitPrototype>(trait, out var traitProto))
                continue;

            if (traitProto.Category != DisabilityProto)
                continue;

            var comps = traitProto.Components;
            EntityManager.RemoveComponents(ent, comps);
        }
    }
}
