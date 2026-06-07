using System.Linq;
using System.Numerics;
using Content.Goobstation.Shared.Wraith.Events;
using Content.Shared._White.Grab;
using Content.Shared.Administration.Logs;
using Content.Shared.Database;
using Content.Shared.Popups;
using Content.Shared.StatusEffectNew;
using Content.Shared.Weapons.Melee;
using Content.Shared.Weapons.Melee.Events;

namespace Content.Goobstation.Shared.Wraith.Revenant;

public sealed class TouchOfEvilSystem : EntitySystem
{
    [Dependency] private readonly GrabThrownSystem _throw = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly StatusEffectsSystem _statusEffects = default!;
    [Dependency] private readonly SharedPopupSystem _popups = default!;
    [Dependency] private readonly ISharedAdminLogManager _admin = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<TouchOfEvilComponent, TouchOfEvilEvent>(OnTouchOfEvil);
        SubscribeLocalEvent<TouchOfEvilComponent, MeleeHitEvent>(OnMeleeHit);

        SubscribeLocalEvent<ActiveTouchOfEvilComponent, StatusEffectRemovedEvent>(StatusRemoved);
        SubscribeLocalEvent<ActiveTouchOfEvilComponent, StatusEffectAppliedEvent>(StatusApplied);
    }

    private void OnTouchOfEvil(Entity<TouchOfEvilComponent> ent, ref TouchOfEvilEvent args)
    {
        _statusEffects.TryAddStatusEffect(ent.Owner, ent.Comp.TouchOfEvilEffect, out _, ent.Comp.BuffDuration);

        args.Handled = true;
    }

    private void OnMeleeHit(Entity<TouchOfEvilComponent> ent, ref MeleeHitEvent args)
    {
        if (!args.HitEntities.Any()
            || !ent.Comp.Active)
            return;

        foreach (var entity in args.HitEntities)
        {
            _throw.Throw(entity,
                ent.Owner,
                GetThrowDirection(ent.Owner, entity),
                ent.Comp.ThrowSpeed);
        }
    }

    private void StatusApplied(Entity<ActiveTouchOfEvilComponent> ent, ref StatusEffectAppliedEvent args)
    {
        if (!TryComp<TouchOfEvilComponent>(args.Target, out var touch)
            || !TryComp<MeleeWeaponComponent>(args.Target, out var melee))
            return;

        _popups.PopupClient(Loc.GetString("revenant-touch-of-evil-start"), args.Target, args.Target, PopupType.LargeCaution);
        _admin.Add(LogType.Action, LogImpact.Low, $"{args.Target}'s Touch of Evil duration has started");

        touch.OriginalDamage = melee.Damage;
        touch.Active = true;
        ent.Comp.ThrowSpeed = touch.ThrowSpeed;
        Dirty(args.Target, touch);

        melee.Damage *= touch.DamageBuff;
        Dirty(args.Target, melee);
    }

    private void StatusRemoved(Entity<ActiveTouchOfEvilComponent> ent, ref StatusEffectRemovedEvent args)
    {
        if (!TryComp<TouchOfEvilComponent>(args.Target, out var touch)
            || !TryComp<MeleeWeaponComponent>(args.Target, out var melee)
            || touch.OriginalDamage == null)
            return;

        _popups.PopupClient(Loc.GetString("revenant-touch-of-evil-end"), args.Target, args.Target, PopupType.Medium);
        _admin.Add(LogType.Action, LogImpact.Low, $"{args.Target}'s Touch of Evil duration has ended");

        melee.Damage = touch.OriginalDamage;
        Dirty(args.Target, melee);

        touch.Active = false;
        touch.OriginalDamage = null;

        Dirty(args.Target, touch);
    }

    private Vector2 GetThrowDirection(EntityUid user, EntityUid target)
    {
        var entPos = _transform.GetMapCoordinates(user).Position;
        var targetPos = _transform.GetMapCoordinates(target).Position;
        return targetPos - entPos;
    }
}
