// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 pheenty <fedorlukin2006@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Shitmed.Targeting;
using Content.Shared.Damage;
using Content.Shared.Interaction;
using Content.Shared.Projectiles;
using Content.Shared.Throwing;
using Content.Shared.Weapons.Ranged.Events;
using Content.Shared.Whitelist;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;

namespace Content.Goobstation.Shared.Nail;

public sealed class NailSystem : EntitySystem
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly EntityWhitelistSystem _whitelist = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HammerComponent, AfterInteractEvent>(OnHammering);
        SubscribeLocalEvent<NailgunComponent, AmmoShotEvent>(OnNailgunShot);
        SubscribeLocalEvent<NailComponent, LandEvent>(OnNailLand);
        SubscribeLocalEvent<NailComponent, EmbedEvent>(OnNailEmbed);
    }

    // Try to hammer a nail into the entity it is embedded into
    private void OnHammering(Entity<HammerComponent> ent, ref AfterInteractEvent args)
    {
        if (args.Handled
            || !args.CanReach
            || args.Target == null
            || !TryComp<NailComponent>(args.Target, out var nailComp)
            || !TryComp<EmbeddableProjectileComponent>(args.Target, out var embeddable)
            || embeddable.EmbeddedIntoUid == null)
            return;

        HammerNail(args.Target.Value, embeddable.EmbeddedIntoUid.Value, nailComp, args.User, nailComp.ForceBodyPart, ent.Comp.DamageMultiplier);
        _audio.PlayPredicted(ent.Comp.Sound, ent, args.User);
        args.Handled = true;
    }

    // Mark shot nails as shot from nailgun
    private void OnNailgunShot(EntityUid uid, NailgunComponent component, ref AmmoShotEvent args)
    {
        foreach (var nail in args.FiredProjectiles)
        {
            if (!TryComp<NailComponent>(nail, out var nailComp))
                continue;
            nailComp.ShotFromNailgun = true;
            Dirty(nail, nailComp);
        }
    }

    // Remove the shot from nailgun mark
    private void OnNailLand(Entity<NailComponent> ent, ref LandEvent args) =>
        ent.Comp.ShotFromNailgun = false;

    // Try to automatically hammer the nail shot from a nailgun if requirements are met
    private void OnNailEmbed(Entity<NailComponent> ent, ref EmbedEvent args)
    {
        if (!ent.Comp.AutoHammerIntoNonWhitelisted
            && !_whitelist.IsWhitelistPass(ent.Comp.Whitelist, args.Embedded)
            || !ent.Comp.ShotFromNailgun)
            return;

        HammerNail(ent, args.Embedded, ent.Comp, args.Shooter, ent.Comp.ForceBodyPart);
    }

    // Hammer the nail into the target
    private void HammerNail(EntityUid uid, EntityUid target, NailComponent? comp = null, EntityUid? source = null, TargetBodyPart? targetBodyPart = null, float damageMultiplier = 1)
    {
        if (!Resolve(uid, ref comp))
            return;

        _damageable.TryChangeDamage(target,
            _whitelist.IsWhitelistPass(comp.Whitelist, target)
                ? comp.DamageToWhitelisted * damageMultiplier
                : comp.Damage * damageMultiplier,
            origin: source,
            targetPart: targetBodyPart);

        if (_net.IsServer)
            Del(uid);
    }
}
