// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <aviu00@protonmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Shared._White.Blink;
using Content.Shared.Damage;
using Content.Shared.Damage.Systems;
using Content.Shared.Electrocution;
using Content.Shared.Examine;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.StatusEffect;
using Content.Shared.Timing;
using Content.Shared.UserInterface;
using Content.Shared.Weapons.Melee;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Prototypes;

namespace Content.Shared._Goobstation.Wizard.Spellblade;

public abstract class SharedSpellbladeSystem : EntitySystem
{
    [Dependency] protected readonly UseDelaySystem UseDelay = default!;
    [Dependency] protected readonly SharedAudioSystem Audio = default!;
    [Dependency] private   readonly IPrototypeManager _protoManager = default!;
    [Dependency] private   readonly SharedHandsSystem _hands = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SpellbladeComponent, SpellbladeEnchantMessage>(OnMessage);
        SubscribeLocalEvent<SpellbladeComponent, ExaminedEvent>(OnExamine);
        SubscribeLocalEvent<SpellbladeComponent, ActivatableUIOpenAttemptEvent>(OnOpenAttempt);

        SubscribeLocalEvent<SpellbladeComponent, LightningSpellbladeEnchantmentEvent>(OnLightning);
        SubscribeLocalEvent<SpellbladeComponent, BluespaceSpellbladeEnchantmentEvent>(OnBluespace);
        SubscribeLocalEvent<SpellbladeComponent, FireSpellbladeEnchantmentEvent>(OnFire);
        SubscribeLocalEvent<SpellbladeComponent, SpacetimeSpellbladeEnchantmentEvent>(OnSpacetime);
        SubscribeLocalEvent<SpellbladeComponent, ForceshieldSpellbladeEnchantmentEvent>(OnForceshield);

        SubscribeLocalEvent<ElectrocutionAttemptEvent>(OnElectrocutionAttempt);

        SubscribeLocalEvent<ShieldedComponent, BeforeStaminaDamageEvent>(OnBeforeStaminaDamage);
        SubscribeLocalEvent<ShieldedComponent, BeforeStatusEffectAddedEvent>(OnBeforeStatusEffect);
        SubscribeLocalEvent<ShieldedComponent, DamageModifyEvent>(OnDamageModify);
    }

    private void OnDamageModify(Entity<ShieldedComponent> ent, ref DamageModifyEvent args)
    {
        args.Damage = DamageSpecifier.ApplyModifierSet(args.Damage,
            DamageSpecifier.PenetrateArmor(ent.Comp.Resistances, args.ArmorPenetration));
    }

    private void OnBeforeStatusEffect(Entity<ShieldedComponent> ent, ref BeforeStatusEffectAddedEvent args)
    {
        if (!ent.Comp.AntiStun || args.Key is not ("Stun"))
            return;

        args.Cancelled = true;
    }

    private void OnBeforeStaminaDamage(Entity<ShieldedComponent> ent, ref BeforeStaminaDamageEvent args)
    {
        if (ent.Comp.AntiStun)
            args.Cancelled = true;
    }

    private void OnForceshield(Entity<SpellbladeComponent> ent, ref ForceshieldSpellbladeEnchantmentEvent args)
    {
        var enchant = EnsureComp<ForceshieldSpellbladeEnchantmentComponent>(ent);
        enchant.ShieldLifetime = args.ShieldLifetime;
    }

    private void OnSpacetime(Entity<SpellbladeComponent> ent, ref SpacetimeSpellbladeEnchantmentEvent args)
    {
        EnsureComp<SpacetimeSpellbladeEnchantmentComponent>(ent);

        if (!TryComp(ent, out MeleeWeaponComponent? weapon) || args.MeleeMultiplier <= 0f)
            return;

        weapon.AttackRate *= args.MeleeMultiplier;
        weapon.HeavyStaminaCost /= args.MeleeMultiplier;
        weapon.Damage /= args.MeleeMultiplier;
        Dirty(ent.Owner, weapon);
    }

    private void OnFire(Entity<SpellbladeComponent> ent, ref FireSpellbladeEnchantmentEvent args)
    {
        var enchant = EnsureComp<FireSpellbladeEnchantmentComponent>(ent);
        enchant.FireStacks = args.FireStacks;
        enchant.Range = args.Range;

        UseDelay.SetLength(ent.Owner, args.Delay);

        AddIgniteOnMeleeHitComponent(ent, args.FireStacksOnHit);
    }

    protected virtual void AddIgniteOnMeleeHitComponent(EntityUid uid, float fireStacks) { }

    private void OnBluespace(Entity<SpellbladeComponent> ent, ref BluespaceSpellbladeEnchantmentEvent args)
    {
        var blink = EnsureComp<BlinkComponent>(ent);

        blink.Distance = args.Distance;
        blink.KnockdownTime = args.KnockdownTime;
        blink.KnockdownRadius = args.KnockdownRadius;

        Dirty(ent.Owner, blink);

        UseDelay.SetLength(ent.Owner, args.ToggleDelay);
        UseDelay.SetLength(ent.Owner, args.BlinkDelay, blink.BlinkDelay);
    }

    private void OnLightning(Entity<SpellbladeComponent> ent, ref LightningSpellbladeEnchantmentEvent args)
    {
        var enchant = EnsureComp<LightningSpellbladeEnchantmentComponent>(ent);

        enchant.ShockDamage = args.ShockDamage;
        enchant.ShockTime = args.ShockTime;
        enchant.Range = args.Range;
        enchant.Siemens = args.Siemens;
        enchant.ArcDepth = args.ArcDepth;
        enchant.BoltCount = args.BoltCount;
        enchant.LightningPrototype = args.LightningPrototype;

        UseDelay.SetLength(ent.Owner, args.Delay);
    }

    private void OnElectrocutionAttempt(ElectrocutionAttemptEvent ev)
    {
        if (IsHoldingItemWithComponent<LightningSpellbladeEnchantmentComponent>(ev.TargetUid))
            ev.Cancel();
    }

    private void OnOpenAttempt(Entity<SpellbladeComponent> ent, ref ActivatableUIOpenAttemptEvent args)
    {
        if (ent.Comp.EnchantmentName == null || args.Cancelled)
            return;

        args.Cancel();
    }

    private void OnExamine(Entity<SpellbladeComponent> ent, ref ExaminedEvent args)
    {
        var comp = ent.Comp;

        if (comp.EnchantmentName == null)
            return;

        var name = Loc.GetString(comp.EnchantmentName);
        args.PushMarkup(Loc.GetString("spellblade-examine-enchantment", ("name", name)));
    }

    private void OnMessage(Entity<SpellbladeComponent> ent, ref SpellbladeEnchantMessage args)
    {
        var (uid, comp) = ent;

        if (comp.EnchantmentName != null)
            return;

        if (!_protoManager.TryIndex(args.ProtoId, out var proto))
            return;

        Audio.PlayPredicted(comp.EnchantSound, uid, args.Actor);

        comp.EnchantmentName = proto.Name;
        Dirty(ent);

        if (proto.Event != null)
            RaiseLocalEvent(uid, proto.Event);
    }

    public bool IsHoldingItemWithComponent<T>(EntityUid user) where T : Component
    {
        return _hands.EnumerateHeld(user).Any(HasComp<T>);
    }
}