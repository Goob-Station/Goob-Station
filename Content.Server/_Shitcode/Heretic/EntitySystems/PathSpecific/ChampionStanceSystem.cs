// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 username <113782077+whateverusername0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 whateverusername0 <whateveremail>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Clothing;
using Content.Server.Heretic.Components.PathSpecific;
using Content.Shared._Shitmed.Body.Events;
using Content.Shared.Body.Part;
using Content.Shared.Damage;
using Content.Shared.Damage.Events;

namespace Content.Server.Heretic.EntitySystems.PathSpecific;

public sealed partial class ChampionStanceSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ChampionStanceComponent, DamageModifyEvent>(OnDamageModify);
        SubscribeLocalEvent<ChampionStanceComponent, TakeStaminaDamageEvent>(OnTakeStaminaDamage);
        SubscribeLocalEvent<ChampionStanceComponent, DelayedKnockdownAttemptEvent>(OnDelayedKnockdownAttempt);

        // if anyone is reading through and does not have EE newmed you can remove these handlers
        SubscribeLocalEvent<ChampionStanceComponent, BodyPartAttachedEvent>(OnBodyPartAttached);
        SubscribeLocalEvent<ChampionStanceComponent, BodyPartRemovedEvent>(OnBodyPartRemoved);
    }

    private void OnDelayedKnockdownAttempt(Entity<ChampionStanceComponent> ent, ref DelayedKnockdownAttemptEvent args)
    {
        if (!Condition(ent))
            return;

        args.Cancel();
    }

    private bool Condition(Entity<ChampionStanceComponent> ent)
    {
        if (!TryComp<DamageableComponent>(ent, out var dmg)
        || dmg.TotalDamage < 50f) // taken that humanoids have 100 damage before critting
            return false;
        return true;
    }

    private void OnDamageModify(Entity<ChampionStanceComponent> ent, ref DamageModifyEvent args)
    {
        if (!Condition(ent))
            return;

        args.Damage = args.OriginalDamage / 2f;
    }

    private void OnTakeStaminaDamage(Entity<ChampionStanceComponent> ent, ref TakeStaminaDamageEvent args)
    {
        if (!Condition(ent))
            return;

        args.Multiplier /= 2.5f;
    }

    private void OnBodyPartAttached(Entity<ChampionStanceComponent> ent, ref BodyPartAttachedEvent args)
    {
        // can't touch this
        args.Part.Comp.CanSever = false;
    }
    private void OnBodyPartRemoved(Entity<ChampionStanceComponent> ent, ref BodyPartRemovedEvent args)
    {
        // can touch this
        args.Part.Comp.CanSever = true;
    }
}