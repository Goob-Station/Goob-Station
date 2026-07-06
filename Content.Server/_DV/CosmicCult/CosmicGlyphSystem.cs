// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Solstice <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 deltanedas <@deltanedas:kde.org>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

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
using Robust.Shared.Timing;

namespace Content.Server._DV.CosmicCult;

public sealed class CosmicGlyphSystem : SharedCosmicGlyphSystem
{
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly AudioSystem _audio = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly SharedCosmicCultSystem _cosmicCult = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    private readonly HashSet<Entity<CosmicCultComponent>> _cultists = [];
    private readonly HashSet<Entity<HumanoidAppearanceComponent>> _humanoids = [];

    public override void Initialize()
    {
        SubscribeLocalEvent<CosmicGlyphComponent, ActivateInWorldEvent>(OnUseGlyph);
        SubscribeLocalEvent<CosmicGlyphComponent, ComponentStartup>(OnGlyphCreated);
    }

    #region Base trigger

    private void OnGlyphCreated(Entity<CosmicGlyphComponent> ent, ref ComponentStartup args)
    {
        ent.Comp.Timer = _timing.CurTime + ent.Comp.SpawnTime;
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var glyphQuery = EntityQueryEnumerator<CosmicGlyphComponent>();
        while (glyphQuery.MoveNext(out var uid, out var comp))
        {
            if (_timing.CurTime < comp.Timer) continue;
            if (comp.State == GlyphStatus.Spawning || comp.State == GlyphStatus.Cooldown)
            {
                _appearance.SetData(uid, GlyphVisuals.Status, GlyphStatus.Ready);
                comp.State = GlyphStatus.Ready;
                return;
            }
            if (comp.State == GlyphStatus.Active)
            {
                ActivateGlyph(new Entity<CosmicGlyphComponent>(uid, comp));
            }
            if (comp.State == GlyphStatus.Despawning)
            {
                QueueDel(uid);
            }
        }
    }

    private void OnUseGlyph(Entity<CosmicGlyphComponent> uid, ref ActivateInWorldEvent args)
    {
        var tgtpos = Transform(uid).Coordinates;
        var userCoords = Transform(args.User).Coordinates;
        if (args.Handled || !userCoords.TryDistance(EntityManager, tgtpos, out var distance) || distance > uid.Comp.ActivationRange || !_cosmicCult.EntityIsCultist(args.User) || uid.Comp.State != GlyphStatus.Ready)
            return;

        var cultists = GatherCultists(uid, uid.Comp.ActivationRange);
        var requiredCultists = uid.Comp.RequiredCultists;

        if (TryComp(args.User, out LoneCosmicCultLeadComponent? leadComp))
            requiredCultists = Math.Max(requiredCultists - leadComp.CultAbilityDeduction, 1);

        if (cultists.Count < requiredCultists)
        {
            _popup.PopupEntity(Loc.GetString("cult-glyph-not-enough-cultists"), uid, args.User);
            return;
        }

        var ev = new CheckGlyphConditionsEvent(args.User, cultists);
        RaiseLocalEvent(uid, ref ev);
        if (ev.Cancelled) return;

        args.Handled = true;
        uid.Comp.User = args.User;
        if (uid.Comp.ActivationTime > TimeSpan.FromSeconds(0))
        {
            _appearance.SetData(uid, GlyphVisuals.Status, GlyphStatus.Active);
            uid.Comp.State = GlyphStatus.Active;
            uid.Comp.Timer = _timing.CurTime + uid.Comp.ActivationTime;
            _audio.PlayPvs(uid.Comp.ChargeSFX, Transform(uid).Coordinates);
        }
        else ActivateGlyph(uid);
    }

    private void ActivateGlyph(Entity<CosmicGlyphComponent> ent)
    {
        if (ent.Comp.EraseOnUse)
        {
            EraseGlyph(ent);
        }
        else if (ent.Comp.CooldownTime > TimeSpan.FromSeconds(0))
        {
            _appearance.SetData(ent, GlyphVisuals.Status, GlyphStatus.Cooldown);
            ent.Comp.State = GlyphStatus.Cooldown;
            ent.Comp.Timer = _timing.CurTime + ent.Comp.CooldownTime;
        }
        else
        {
            _appearance.SetData(ent, GlyphVisuals.Status, GlyphStatus.Ready);
            ent.Comp.State = GlyphStatus.Ready;
        }

        if (ent.Comp.User is not { } user) return;
        var cultists = GatherCultists(ent, ent.Comp.ActivationRange);
        var tryInvokeEv = new TryActivateGlyphEvent(user, cultists);
        RaiseLocalEvent(ent, ref tryInvokeEv);
        var tgtpos = Transform(ent).Coordinates;
        if (tryInvokeEv.Cancelled || cultists.Count < ent.Comp.RequiredCultists)
        {
            _audio.PlayPvs(ent.Comp.FailSFX, tgtpos);
            return;
        }

        var damage = ent.Comp.ActivationDamage / cultists.Count;
        foreach (var cultist in cultists)
            _damageable.TryChangeDamage(cultist, damage, true);

        _audio.PlayPvs(ent.Comp.TriggerSFX, tgtpos, AudioParams.Default.WithVolume(+1f));
        Spawn(ent.Comp.GlyphVFX, tgtpos);
        ent.Comp.User = null;
    }
    #endregion

    #region Housekeeping
    /// <summary>
    ///     Gets all cultists/constructs near a glyph.
    /// </summary>
    public HashSet<Entity<CosmicCultComponent>> GatherCultists(EntityUid uid, float range)
    {
        _cultists.Clear();
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
        _humanoids.Clear();
        _lookup.GetEntitiesInRange<HumanoidAppearanceComponent>(Transform(uid).Coordinates, range, _humanoids);

        if (exclude != null)
            _humanoids.RemoveWhere(exclude);

        _humanoids.RemoveWhere(target => HasComp<CosmicBlankComponent>(target) || HasComp<CosmicLapseComponent>(target)); // We never want these.

        return _humanoids;
    }
    #endregion
}
