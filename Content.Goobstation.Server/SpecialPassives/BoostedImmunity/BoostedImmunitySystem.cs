using Content.Goobstation.Shared.SpecialPassives.BoostedImmunity.Components;
using Content.Server.GameTicking;
using Content.Shared.Traits;
using Robust.Server.Player;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.SpecialPassives.BoostedImmunity;

public sealed class BoostedImmunitySystem : SharedBoostedImmunitySystem
{
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly IPrototypeManager _protoManager = default!;
    [Dependency] private readonly GameTicker _ticker = default!;

    public override void Initialize()
    {
        base.Initialize();

    }

    public readonly ProtoId<TraitCategoryPrototype> DisabilityProto = "Disabilities";
    protected override void RemoveDisabilities(Entity<BoostedImmunityComponent> ent)
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
