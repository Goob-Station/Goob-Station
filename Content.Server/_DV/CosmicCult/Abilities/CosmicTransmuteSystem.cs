using System.Linq;
using Content.Shared._DV.CosmicCult.Components;
using Content.Shared.Popups;
using Content.Shared.Whitelist;
using Robust.Shared.Random;

namespace Content.Server._DV.CosmicCult.Abilities;

public sealed class CosmicTransmuteSystem : EntitySystem
{
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly EntityWhitelistSystem _entityWhitelist = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    private readonly HashSet<EntityUid> _entities = [];

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CosmicGlyphTransmuteComponent, TryActivateGlyphEvent>(OnTransmuteGlyph);
    }

    private void OnTransmuteGlyph(Entity<CosmicGlyphTransmuteComponent> uid, ref TryActivateGlyphEvent args)
    {
        var tgtpos = Transform(uid).Coordinates;
        var possibleTargets = GatherEntities(uid);
        if (possibleTargets.Count == 0)
        {
            _popup.PopupEntity(Loc.GetString("cult-glyph-conditions-not-met"), uid, args.User);
            args.Cancel();
            return;
        }
        if (possibleTargets.Count > 1)
        {
            _popup.PopupEntity(Loc.GetString("cult-glyph-too-many-targets"), uid, args.User);
            args.Cancel();
            return;
        }

        Spawn(_random.Pick(uid.Comp.Transmutations), tgtpos);
        QueueDel(possibleTargets.First());
    }


    /// <summary>
    ///     Gets all whitelisted entities near a glyph.
    /// </summary>
    private HashSet<EntityUid> GatherEntities(Entity<CosmicGlyphTransmuteComponent> ent)
    {
        _entities.Clear();
        _lookup.GetEntitiesInRange(Transform(ent).Coordinates, ent.Comp.TransmuteRange, _entities);
        _entities.RemoveWhere(item => !_entityWhitelist.IsValid(ent.Comp.Whitelist, item));
        return _entities;
    }
}
