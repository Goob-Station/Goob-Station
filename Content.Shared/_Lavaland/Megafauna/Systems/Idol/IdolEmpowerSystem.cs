using Content.Shared._Lavaland.Megafauna.Components.Idol;
using Content.Shared.Whitelist;
using Content.Shared.StatusEffectNew;
using Robust.Shared.Prototypes;

namespace Content.Shared._Lavaland.Megafauna.Systems.Idol;

public sealed class IdolEmpowerSystem : EntitySystem
{
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly EntityWhitelistSystem _whitelist = default!;
    [Dependency] private readonly StatusEffectsSystem _status = default!;

    private readonly HashSet<EntityUid> _entities = new();

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<IdolEmpowerComponent, ComponentStartup>(OnStartup);
    }

    private void OnStartup(Entity<IdolEmpowerComponent> ent, ref ComponentStartup args)
    {
        DoEmpower(
            ent.Owner,
            ent.Comp.EmpowerRange,
            ent.Comp.Whitelist,
            ent.Comp.StatusEffectRally,
            ent.Comp.Duration
        );
    }

    private void DoEmpower(EntityUid source, float range, EntityWhitelist? whitelist, EntProtoId effect, TimeSpan duration)
    {
        _entities.Clear();

        // Get all entities in range
        _lookup.GetEntitiesInRange(Transform(source).Coordinates, range, _entities);

        foreach (var affected in _entities)
        {
            if (!_whitelist.IsWhitelistPass(whitelist, affected))
                continue;

            _status.TryAddStatusEffect(affected, effect, out _, duration);
        }
    }
}
