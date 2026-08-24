using Content.Shared._DV.CosmicCult.Components;
using Content.Shared.Popups;
using Robust.Shared.Random;

namespace Content.Server._DV.CosmicCult.Abilities;

public sealed class CosmicTransmuteSystem : EntitySystem
{
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    private readonly HashSet<EntityUid> _entities = [];

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CosmicGlyphTransmuteComponent, TryActivateGlyphEvent>(OnTransmuteGlyph);
        SubscribeLocalEvent<CosmicGlyphTransmuteComponent, CheckGlyphConditionsEvent>(OnCheckGlyphConditions);
    }

    private void OnCheckGlyphConditions(Entity<CosmicGlyphTransmuteComponent> uid, ref CheckGlyphConditionsEvent args)
    {
        var possibleTargets = GatherEntities(uid);
        if (possibleTargets.Count == 0)
        {
            _popup.PopupEntity(Loc.GetString("cult-glyph-conditions-not-met"), uid, args.User);
            args.Cancel();
            return;
        }
    }

    private void OnTransmuteGlyph(Entity<CosmicGlyphTransmuteComponent> uid, ref TryActivateGlyphEvent args)
    {
        var ev = new CheckGlyphConditionsEvent(args.User, args.Cultists);
        RaiseLocalEvent(uid, ref ev);
        if (ev.Cancelled)
        {
            args.Cancel();
            return;
        }

        var tgtpos = Transform(uid).Coordinates;
        var possibleTargets = GatherEntities(uid);
        var target = _random.Pick(possibleTargets);
        if (!TryComp<CosmicTransmutableComponent>(target, out var comp)) return;
        Spawn(comp.TransmutesTo, tgtpos);
        QueueDel(target);
    }


    /// <summary>
    ///     Gets all whitelisted entities near a glyph.
    /// </summary>
    private HashSet<EntityUid> GatherEntities(Entity<CosmicGlyphTransmuteComponent> ent)
    {
        _entities.Clear();
        _lookup.GetEntitiesInRange(Transform(ent).Coordinates, ent.Comp.TransmuteRange, _entities);
        _entities.RemoveWhere(item => !TryComp<CosmicTransmutableComponent>(item, out var comp) || comp.RequiredGlyphType != MetaData(ent).EntityPrototype!.ID);
        return _entities;
    }
}
