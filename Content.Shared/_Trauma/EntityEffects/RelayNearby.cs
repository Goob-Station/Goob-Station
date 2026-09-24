using Content.Shared.EntityEffects;
using Content.Shared.Whitelist;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Shared._Trauma.EntityEffects;

/// <summary>
/// Relays an effect to every entity in some radius, matching some conditions.
/// Does not apply it to this effect's target entity.
/// </summary>
public sealed partial class RelayNearby : EntityEffectBase<RelayNearby>
{
    /// <summary>
    /// The effect to apply to found entities.
    /// </summary>
    [DataField]
    public EntityEffect Effect = default!;

    /// <summary>
    /// The component to use for lookups.
    /// If this is Transform it will find any entity in range.
    /// Use the rarest component you can for best performance.
    /// You don't need to include this in <see cref="Whitelist"/>.
    /// </summary>
    [DataField(required: true)]
    public string CompName = string.Empty;
    // TODO: use CompName if plant holder is moved to shared

    /// <summary>
    /// Cached type for the component.
    /// </summary>
    internal Type? Comp;

    /// <summary>
    /// Radius to search around the target entity.
    /// </summary>
    [DataField]
    public float Range = 5f;

    /// <summary>
    /// Flags to use for lookups.
    /// </summary>
    [DataField]
    public LookupFlags Flags = LookupFlags.All;

    /// <summary>
    /// If non-null, found entities must also match this whitelist.
    /// </summary>
    [DataField]
    public EntityWhitelist? Whitelist;

    /// <summary>
    /// If non-null, found entities cannot match this blacklist.
    /// </summary>
    [DataField]
    public EntityWhitelist? Blacklist;

    public override string? EntityEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => Effect.EntityEffectGuidebookText(prototype, entSys); // lazy
}

public sealed partial class RelayNearbyEffectSystem : EntityEffectSystem<TransformComponent, RelayNearby>
{
    [Dependency] private EffectDataSystem _data = default!;
    [Dependency] private EntityLookupSystem _lookup = default!;
    [Dependency] private EntityWhitelistSystem _whitelist = default!;
    [Dependency] private SharedEntityEffectsSystem _effects = default!;
    [Dependency] private SharedTransformSystem _transform = default!;

    private HashSet<Entity<IComponent>> _found = new();

    protected override void Effect(Entity<TransformComponent> ent, ref EntityEffectEvent<RelayNearby> args)
    {
        var effect = args.Effect;
        if (effect.Comp == null)
        {
            var reg = Factory.GetRegistration(effect.CompName);
            effect.Comp = reg.Type;
        }
        var type = effect.Comp;

        var relayed = effect.Effect;
        var range = effect.Range;
        var flags = effect.Flags;
        var whitelist = effect.Whitelist;
        var blacklist = effect.Blacklist;

        var coords = _transform.GetMapCoordinates(ent, ent.Comp);
        _found.Clear();
        _lookup.GetEntitiesInRange(type, coords, range, _found, flags);
        foreach (var found in _found)
        {
            var uid = found.Owner;
            if (uid == ent.Owner) // don't apply to itself
                continue;

            if (!_whitelist.CheckBoth(uid, blacklist: blacklist, whitelist: whitelist))
                continue;

            _data.CopyData(ent, uid);
            _effects.TryApplyEffect(uid, relayed, args.Scale, args.User);
            _data.ClearData(uid);
        }
    }
}
