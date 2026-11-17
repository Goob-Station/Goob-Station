using Content.Goobstation.Common.MartialArts;
using Content.Goobstation.Shared.MartialArts.Components;
using Content.Goobstation.Shared.MartialArts.Events;
using Content.Goobstation.Shared.MartialArts.Specific.Components;
using Content.Goobstation.Shared.Stealth;
using Content.Shared._Shitmed.Targeting;
using Content.Shared.Alert;
using Content.Shared.Damage;
using Content.Shared.Damage.Systems;
using Content.Shared.IdentityManagement;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Popups;
using Content.Shared.Standing;
using Content.Shared.StatusEffectNew;
using Content.Shared.Weapons.Melee;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.MartialArts.Specific;

/// <summary>
/// TODO: this one still a little more goida and hardcoded than i would prefer, for now
/// </summary>
public sealed partial class NinjutsuSystem : EntitySystem
{
    [Dependency] private AlertsSystem _alerts = default!;
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private DamageableSystem _damageable = default!;
    [Dependency] private SharedMeleeWeaponSystem _meleeWeapon = default!;
    [Dependency] private SharedStaminaSystem _stamina = default!;
    [Dependency] private StandingStateSystem _standingState = default!;
    [Dependency] private StatusEffectsSystem _status = default!;
    [Dependency] private SharedGoobStealthSystem _goobStealth = default!;
    [Dependency] private MartialArtsSystem _martialArts = default!;
    [Dependency] private SharedPopupSystem _popup = default!;
    [Dependency] private IPrototypeManager _proto = default!;

    private static readonly ProtoId<MartialArtPrototype> Ninjutsu = "Ninjutsu";
    private static readonly ProtoId<AlertCategoryPrototype> NinjutsuAlertCategory = "Ninjutsu";
    private static readonly EntProtoId NinjutsuLossOfSurprise = "StatusEffectNinjutsuLossOfSurprise";

    [SubscribeLocalEvent(after: [typeof(MartialArtsComboSystem)])]
    private void AfterComboAttackPerformed(Entity<NinjutsuSneakAttackComponent> ent, ref AfterComboAttackPerformedEvent args)
    {
        if (!HasLossOfSurprise(ent.Owner))
        {
            ResetDebuff(ent);
            return;
        }

        ResetDebuff(ent);

        if (args.Type != ComboAttackType.Harm
            || !IsWeaponValid(args.Performer, args.Weapon, out var meleeComp)
            || !_standingState.IsDown(args.Target))
            return;

        DoSwiftStrike(ent, args.Target, (args.Weapon, meleeComp));
    }

    private void DoSwiftStrike(EntityUid performer, EntityUid target, Entity<MeleeWeaponComponent> weapon)
    {
        if (performer == weapon.Owner)
            _stamina.TakeStaminaDamage(target, 30f);

        var fireRate = TimeSpan.FromSeconds(1f / _meleeWeapon.GetAttackRate(weapon, performer, weapon));
        var minFireRate = TimeSpan.FromSeconds(1f / 8f); // This is basically the attack speed of a HF Blade.

        if (fireRate.TotalSeconds - fireRate.TotalSeconds / 2f <= minFireRate.TotalSeconds)
            return;

        weapon.Comp.NextAttack -= fireRate / 2f;
        Dirty(weapon);
    }

    /// todo wish this was prototyped
    [SubscribeLocalEvent]
    private void OnNinjutsuMeleeHit(Entity<NinjutsuSneakAttackComponent> ent, ref MeleeHitEvent ev)
    {
        if (!ev.IsHit)
            return;

        if (HasLossOfSurprise(ent.Owner))
        {
            ResetDebuff(ent);
            return;
        }

        ResetDebuff(ent);

        if (ev.HitEntities.Count == 0)
            return;

        var target = ev.HitEntities[0];
        if (target == ent.Owner
            || !HasComp<MobStateComponent>(target)
            || !IsWeaponValid(ent, ev.Weapon, out _))
            return;

        if (ent.Comp.Multiplier > 1f)
            ev.BonusDamage = ev.BaseDamage * (ent.Comp.Multiplier - 1f);

        // Heavy attack
        if (ev.Direction != null)
            return;

        var total = ev.BaseDamage.GetTotal();
        if (total <= 0f)
            return;

        // Assassinate

        var isUnarmed = ent.Owner == ev.Weapon;
        var damageType = isUnarmed ? "Blunt" : "Slash";
        var modifier = isUnarmed ? ent.Comp.AssassinateUnarmedModifier : ent.Comp.AssassinateModifier;
        var bonusDamage = new DamageSpecifier
        {
            DamageDict =
            {
                { damageType, modifier },
            },
            ArmorPenetration = ent.Comp.AssassinateArmorPierce,
        };
        _damageable.TryChangeDamage(target, bonusDamage, origin: ent.Owner, canMiss: false, targetPart: TargetBodyPart.Chest);

        var sound = isUnarmed ? ent.Comp.AssassinateSoundUnarmed : ent.Comp.AssassinateSoundArmed;
        _audio.PlayPredicted(sound, target, ent);

        // TODO while this is not an actual prototyped move (copied from MartialArtsComboSystem)
        var identityPerformer = Identity.Entity(ent, EntityManager);
        var identityReceiver = Identity.Entity(target, EntityManager);

        var sPerformer = Loc.GetString(
            "martial-arts-action-sender",
            ("performer", identityPerformer),
            ("receiver", identityReceiver),
            ("move", "Assassinate")
        );
        var sReceiver = Loc.GetString(
            "martial-arts-action-receiver",
            ("performer", identityPerformer),
            ("receiver", identityReceiver),
            ("move", "Assassinate")
        );

        _popup.PopupEntity(sPerformer, ent, ent);
        _popup.PopupEntity(sReceiver, target, target);
    }

    /// Movement speed buff after kill
    private void OnMobStateChanged(ref MobStateChangedEvent ev)
    {
        if (ev.Origin is not { } origin)
            return;

        if (!_martialArts.HasMartialArtActive(origin, _proto.Index(Ninjutsu)))
            return;

        if (ev.NewMobState != MobState.Dead)
            return;

        _martialArts.ApplyMultiplier(origin, 1.2f, 0f, TimeSpan.FromSeconds(3), MartialArtModifierType.MoveSpeed);
    }

    public bool HasLossOfSurprise(Entity<MobStateComponent?> ent)
    {
        return _status.HasStatusEffect(ent, NinjutsuLossOfSurprise);
    }
}
