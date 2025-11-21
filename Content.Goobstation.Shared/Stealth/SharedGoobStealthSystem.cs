// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <aviu00@protonmail.com>
// SPDX-FileCopyrightText: 2025 Fishbait <Fishbait@git.ml>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 fishbait <gnesse@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Stealth.Components;
using Content.Shared.Stealth;
using Content.Shared.Damage;
using Content.Shared.Ninja.Components;
using Content.Shared.Ninja.Systems;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared.Weapons.Ranged.Events;
using Content.Shared.Throwing;
using Content.Goobstation.Shared.Slasher.Components; // For SlasherIncorporealComponent

namespace Content.Goobstation.Shared.Stealth;

/// <summary>
/// This handles goobstations additions to stealth system
/// </summary>
public sealed class SharedGoobStealthSystem : EntitySystem
{
    [Dependency] private readonly SharedStealthSystem _stealth = default!;
    [Dependency] private readonly SharedNinjaSuitSystem _suit = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<StealthComponent, MeleeAttackEvent> (OnMeleeAttack);
        SubscribeLocalEvent<StealthComponent, SelfBeforeGunShotEvent> (OnGunShootAttack);
        SubscribeLocalEvent<StealthComponent, BeforeDamageChangedEvent>(OnTakeDamage);
        SubscribeLocalEvent<StealthComponent, BeforeThrowEvent>(OnThrow);
    }

    private void OnTakeDamage(Entity<StealthComponent> ent, ref BeforeDamageChangedEvent args)
    {
        if (!ent.Comp.RevealOnDamage)
            return;

        if (!args.Damage.AnyPositive()) // being healed does not reveal
            return;

        if (args.Damage.GetTotal() <= ent.Comp.Threshold) //damage needs to be above threshold
            return;

        _stealth.ModifyVisibility(ent.Owner, ent.Comp.MaxVisibility, ent.Comp);
        TryRevealNinja(ent.Owner);
    }

    private void OnMeleeAttack(Entity<StealthComponent> ent, ref MeleeAttackEvent args)
    {
        if (!ent.Comp.RevealOnAttack)
            return;

        _stealth.ModifyVisibility(ent.Owner, ent.Comp.MaxVisibility, ent.Comp);
        TryRevealNinja(ent.Owner);
    }

    private void OnGunShootAttack(Entity<StealthComponent> ent, ref SelfBeforeGunShotEvent args)
    {
        if (!ent.Comp.RevealOnAttack)
            return;

        _stealth.ModifyVisibility(ent.Owner, ent.Comp.MaxVisibility, ent.Comp);
        TryRevealNinja(ent.Owner);
    }

    private void OnThrow(Entity<StealthComponent> ent, ref BeforeThrowEvent args)
    {
        if (!ent.Comp.RevealOnAttack)
            return;

        // Some goida stuff. If a slasher attempts to throw an item it stops them from throwing it BUTTTTT THEY STILL GET REVEALED, so here we are.
        // Slasher
        if (TryComp<SlasherIncorporealComponent>(ent.Owner, out var slasher) && slasher.IsIncorporeal)
            return;

        _stealth.ModifyVisibility(ent.Owner, ent.Comp.MaxVisibility, ent.Comp);
        TryRevealNinja(ent.Owner);
    }

    public void TryRevealNinja(EntityUid uid)
    {
        if (!TryComp(uid, out SpaceNinjaComponent? ninja))
            return;

        if (ninja.Suit is { } suit
            && TryComp<NinjaSuitComponent>(suit, out var suitComp))
            _suit.RevealNinja((suit, suitComp), uid, true);
    }
}
