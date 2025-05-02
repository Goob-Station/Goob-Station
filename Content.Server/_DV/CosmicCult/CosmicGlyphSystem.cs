using Content.Server.Popups;
using Content.Shared._DV.CosmicCult.Components.Examine;
using Content.Shared._DV.CosmicCult.Components;
using Content.Shared._DV.CosmicCult;
using Content.Shared.Damage;
using Content.Shared.Examine;
using Content.Shared.Humanoid;
using Content.Shared.Interaction;
using Content.Shared.Mobs.Systems;
using Robust.Server.Audio;
using Robust.Shared.Audio;
using Robust.Shared.Containers;

namespace Content.Server._DV.CosmicCult;

public sealed class CosmicGlyphSystem : EntitySystem
{
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly AudioSystem _audio = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly SharedCosmicCultSystem _cosmicCult = default!;

    private readonly HashSet<Entity<CosmicCultComponent>> _cultists = [];
    private readonly HashSet<Entity<HumanoidAppearanceComponent>> _humanoids = [];

    public override void Initialize()
    {
        SubscribeLocalEvent<CosmicGlyphComponent, ExaminedEvent>(OnExamine);
        SubscribeLocalEvent<CosmicGlyphComponent, ActivateInWorldEvent>(OnUseGlyph);
    }

    #region Base trigger

    private void OnExamine(Entity<CosmicGlyphComponent> uid, ref ExaminedEvent args)
    {
        if (_cosmicCult.EntityIsCultist(args.Examiner))
        {
            args.PushMarkup(Loc.GetString("cosmic-examine-glyph-cultcount", ("COUNT", uid.Comp.RequiredCultists)));
        }
        else
        {
            args.PushMarkup(Loc.GetString("cosmic-examine-text-glyphs"));
        }
    }

    private void OnUseGlyph(Entity<CosmicGlyphComponent> uid, ref ActivateInWorldEvent args)
    {
        var tgtpos = Transform(uid).Coordinates;
        var userCoords = Transform(args.User).Coordinates;
        if (args.Handled || !userCoords.TryDistance(EntityManager, tgtpos, out var distance) || distance > uid.Comp.ActivationRange || !_cosmicCult.EntityIsCultist(args.User))
            return;
        var cultists = GatherCultists(uid, uid.Comp.ActivationRange);
        if (cultists.Count < uid.Comp.RequiredCultists)
        {
            _popup.PopupEntity(Loc.GetString("cult-glyph-not-enough-cultists"), uid, args.User);
            return;
        }

        args.Handled = true;
        var tryInvokeEv = new TryActivateGlyphEvent(args.User, cultists);
        RaiseLocalEvent(uid, tryInvokeEv);
        if (tryInvokeEv.Cancelled)
            return;

        var damage = uid.Comp.ActivationDamage / cultists.Count;
        foreach (var cultist in cultists)
        {
            _damageable.TryChangeDamage(cultist, damage, true);
        }

        _audio.PlayPvs(uid.Comp.GylphSFX, tgtpos, AudioParams.Default.WithVolume(+1f));
        Spawn(uid.Comp.GylphVFX, tgtpos);
        QueueDel(uid);
    }
    #endregion

    #region Housekeeping
    /// <summary>
    ///     Gets all cultists/constructs near a glyph.
    /// </summary>
    public HashSet<Entity<CosmicCultComponent>> GatherCultists(EntityUid uid, float range)
    {
        _lookup.GetEntitiesInRange<CosmicCultComponent>(Transform(uid).Coordinates, range, _cultists);
        _cultists.RemoveWhere(entity => !_mobState.IsAlive(entity) || _container.IsEntityInContainer(entity));
        return _cultists;
    }

    /// <summary>
    ///     Gets all the humanoids near a glyph.
    /// </summary>
    /// <param name="uid">The glyph.</param>
    /// <param name="range">Radius for a lookup.</param>
    /// <param name="exclude">Filter to exclude from return.</param>
    public HashSet<Entity<HumanoidAppearanceComponent>> GetTargetsNearGlyph(EntityUid uid, float range, Predicate<Entity<HumanoidAppearanceComponent>>? exclude = null)
    {
        _lookup.GetEntitiesInRange<HumanoidAppearanceComponent>(Transform(uid).Coordinates, range, _humanoids);
        if (exclude != null)
            _humanoids.RemoveWhere(exclude);
        _humanoids.RemoveWhere(target => HasComp<CosmicBlankComponent>(target) || HasComp<CosmicLapseComponent>(target)); // We never want these.

        return _humanoids;
    }
    #endregion
}
