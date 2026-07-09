// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using System.Numerics;
using Content.Goobstation.Common.CCVar;
using Content.Goobstation.Common.Projectiles; // Goobstation
using Content.Shared.Atmos.Components;
using Content.Server.Atmos.EntitySystems;
using Content.Server.Cargo.Systems;
using Content.Server.Weapons.Ranged.Components;
using Content.Shared.Cargo;
using Content.Shared.Damage;
using Content.Shared.Damage.Systems;
using Content.Shared.Projectiles;
using Content.Shared.Weapons.Melee;
using Content.Shared.Weapons.Ranged;
using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Weapons.Ranged.Events;
using Content.Shared.Weapons.Ranged.Systems;
using Content.Shared.Weapons.Hitscan.Components;
using Content.Shared.Weapons.Hitscan.Events;
using Robust.Shared.Audio;
using Robust.Shared.Configuration; // Goobstation
using Robust.Shared.Map;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Robust.Shared.Containers;
using Content.Shared._Lavaland.Weapons.Ranged.Events; // Lavaland Change
using Robust.Server.GameObjects; // Goobstation
using Content.Goobstation.Common.Weapons.Ranged;
using Content.Shared._Shitmed.Targeting;
using Content.Shared.Atmos.Components;
using Content.Shared.Body.Components;
using Content.Shared.Effects;
using Content.Shared.PowerCell;
using Robust.Shared.Random; // Lavaland Change

namespace Content.Server.Weapons.Ranged.Systems;

public sealed partial class GunSystem : SharedGunSystem
{
    [Dependency] private readonly PricingSystem _pricing = default!;
    [Dependency] private readonly SharedColorFlashEffectSystem _color = default!;
    [Dependency] private readonly SharedStaminaSystem _stamina = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly PowerCellSystem _powerCell = default!;
    [Dependency] private readonly SharedMapSystem _map = default!;

    // Goobstation
    [Dependency] private readonly FlammableSystem _flammable = default!;
    [Dependency] private readonly TransformSystem _transform = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;

    private const float DamagePitchVariation = 0.05f;
    private float _crawlHitzoneSize; // Goobstation

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<BallisticAmmoProviderComponent, PriceCalculationEvent>(OnBallisticPrice);
        _cfg.OnValueChanged(GoobCVars.CrawlHitzoneSize, value => _crawlHitzoneSize = value, true); // Goobstation
    }

    private void OnBallisticPrice(Entity<BallisticAmmoProviderComponent> ent, ref PriceCalculationEvent args)
    {
        if (string.IsNullOrEmpty(ent.Comp.Proto) || ent.Comp.UnspawnedCount == 0)
            return;

        if (!ProtoManager.TryIndex<EntityPrototype>(ent.Comp.Proto, out var proto))
        {
            Log.Error($"Unable to find fill prototype for price on {ent.Comp.Proto} on {ToPrettyString(ent)}");
            return;
        }

        // Probably good enough for most.
        var price = _pricing.GetEstimatedPrice(proto);
        args.Price += price * ent.Comp.UnspawnedCount;
    }

    public override void Shoot(Entity<GunComponent> gun, List<(EntityUid? Entity, IShootable Shootable)> ammo,
        EntityCoordinates fromCoordinates, EntityCoordinates toCoordinates, out bool userImpulse, EntityUid? user = null, bool throwItems = false)
    {
        SharedShoot(gun.Owner, gun.Comp, ammo, fromCoordinates, toCoordinates, out userImpulse, user, throwItems); // Pirate: gunplay
    }

    // Goobstation start
    public TargetBodyPart? GetTargetPart(Entity<TargetingComponent?>? ent,
        MapCoordinates shootCoords,
        MapCoordinates targetCoords)
    {
        if (shootCoords.MapId != targetCoords.MapId || ent == null)
            return null;

        var targeting = ent.Value;

        if (!Resolve(targeting, ref targeting.Comp, false))
            return null;

        var dist = (shootCoords.Position - targetCoords.Position).Length();
        var missChance = MathHelper.Lerp(0f, 1f, Math.Clamp(dist / 2f, 0f, 1f));
        return Random.Prob(missChance) ? TargetBodyPart.Chest : targeting.Comp.Target;
    }

    private void SetProjectilePerfectHitEntities(EntityUid projectile,
        Entity<TargetingComponent?>? shooter,
        MapCoordinates coords)
    {
        if (shooter == null)
            return;

        var ent = shooter.Value;

        if (!Resolve(ent, ref ent.Comp, false))
            return;

        var comp = EnsureComp<ProjectileMissTargetPartChanceComponent>(projectile);
        var look = _lookup.GetEntitiesInRange<BodyComponent>(coords, 2f, LookupFlags.Dynamic);
        foreach (var (uid, body) in look)
        {
            if (body.BodyType != Shared._Shitmed.Body.BodyType.Complex)
                continue;

            var part = GetTargetPart(shooter, coords, _transform.GetMapCoordinates(ent));

            if (part is null or TargetBodyPart.Chest)
                continue;

            comp.PerfectHitEntities.Add(uid);
        }
    }
    // Goobstation end

    private void ShootOrThrow(EntityUid uid, Vector2 mapDirection, Vector2 gunVelocity, Entity<GunComponent> gun, EntityUid? user,
        Vector2? targetCoordinates = null) // Goobstation
    {
        if (gun.Comp.Target is { } target && !TerminatingOrDeleted(target))
        {
            var targeted = EnsureComp<TargetedProjectileComponent>(uid);
            targeted.Target = target;
            Dirty(uid, targeted);
        }

        // Do a throw
        if (!HasComp<ProjectileComponent>(uid))
        {
            RemoveShootable(uid);
            // TODO: Someone can probably yeet this a billion miles so need to pre-validate input somewhere up the call stack.
            ThrowingSystem.TryThrow(uid, mapDirection, gun.Comp.ProjectileSpeedModified, user);
            return;
        }

        ShootProjectile(uid, mapDirection, gunVelocity, gun, user, gun.Comp.ProjectileSpeedModified,
        targetCoordinates); // Goobstation
    }

    /// <summary>
    /// Gets a linear spread of angles between start and end.
    /// </summary>
    /// <param name="start">Start angle in degrees</param>
    /// <param name="end">End angle in degrees</param>
    /// <param name="intervals">How many shots there are</param>
    public Angle[] LinearSpread(Angle start, Angle end, int intervals) // Goob edit
    {
        var angles = new Angle[intervals];
        DebugTools.Assert(intervals > 1);

        for (var i = 0; i <= intervals - 1; i++)
        {
            angles[i] = new Angle(start + (end - start) * i / (intervals - 1));
        }

        return angles;
    }

    private Angle GetRecoilAngle(TimeSpan curTime, GunComponent component, Angle direction, EntityUid? user = null) // Goobstation user
    {
        // Pirate: gunplay
        return GetPredictedRecoilAngle(curTime, (component.Owner, component), direction, user);
    }

    protected override void Popup(string message, EntityUid? uid, EntityUid? user) { }

    protected override void CreateEffect(EntityUid gunUid, MuzzleFlashEvent message, EntityUid? user = null)
    {
        var filter = Filter.Pvs(gunUid, entityManager: EntityManager);

        if (TryComp<ActorComponent>(user, out var actor))
            filter.RemovePlayer(actor.PlayerSession);

        RaiseNetworkEvent(message, filter);
    }

    public override void PlayImpactSound(EntityUid otherEntity, DamageSpecifier? modifiedDamage, SoundSpecifier? weaponSound, bool forceWeaponSound)
    {
        DebugTools.Assert(!Deleted(otherEntity), "Impact sound entity was deleted");

        // Like projectiles and melee,
        // 1. Entity specific sound
        // 2. Ammo's sound
        // 3. Nothing
        var playedSound = false;

        if (!forceWeaponSound && modifiedDamage != null && modifiedDamage.GetTotal() > 0 && TryComp<RangedDamageSoundComponent>(otherEntity, out var rangedSound))
        {
            var type = SharedMeleeWeaponSystem.GetHighestDamageSound(modifiedDamage, ProtoManager);

            if (type != null && rangedSound.SoundTypes?.TryGetValue(type, out var damageSoundType) == true)
            {
                Audio.PlayPvs(damageSoundType, otherEntity, AudioParams.Default.WithVariation(DamagePitchVariation));
                playedSound = true;
            }
            else if (type != null && rangedSound.SoundGroups?.TryGetValue(type, out var damageSoundGroup) == true)
            {
                Audio.PlayPvs(damageSoundGroup, otherEntity, AudioParams.Default.WithVariation(DamagePitchVariation));
                playedSound = true;
            }
        }

        if (!playedSound && weaponSound != null)
        {
            Audio.PlayPvs(weaponSound, otherEntity);
        }
    }
}
