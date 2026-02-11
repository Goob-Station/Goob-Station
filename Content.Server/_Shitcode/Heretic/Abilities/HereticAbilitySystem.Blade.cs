// SPDX-FileCopyrightText: 2024 username <113782077+whateverusername0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 whateverusername0 <whateveremail>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <aviu00@protonmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gluesniffler <linebarrelerenthusiast@gmail.com>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2025 yglop <95057024+yglop@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Heretic.Components.PathSpecific;
using Content.Shared._Goobstation.Heretic.Components;
using Content.Shared.Heretic;
using Robust.Shared.Timing;
using Content.Shared._Shitmed.Medical.Surgery.Wounds.Components; // Shitmed Change

namespace Content.Server.Heretic.Abilities;

public sealed partial class HereticAbilitySystem
{
    protected override void SubscribeBlade()
    {
        base.SubscribeBlade();

        SubscribeLocalEvent<HereticComponent, HereticDanceOfTheBrandEvent>(OnDanceOfTheBrand);
        SubscribeLocalEvent<HereticComponent, HereticChampionStanceEvent>(OnChampionStance);
        SubscribeLocalEvent<HereticComponent, EventHereticFuriousSteel>(OnFuriousSteel);

        SubscribeLocalEvent<HereticComponent, HereticAscensionBladeEvent>(OnAscensionBlade);
    }

    private void OnDanceOfTheBrand(Entity<HereticComponent> ent, ref HereticDanceOfTheBrandEvent args)
    {
        var riposte = EnsureComp<RiposteeComponent>(ent);
        riposte.Data.TryAdd("HereticBlade", new());
    }

    private void OnChampionStance(Entity<HereticComponent> ent, ref HereticChampionStanceEvent args)
    {
        foreach (var part in _body.GetBodyChildren(ent))
        {
            if (!TryComp(part.Id, out WoundableComponent? woundable))
                continue;

            woundable.CanRemove = false;
            Dirty(part.Id, woundable);
        }

        EnsureComp<ChampionStanceComponent>(ent);
    }
    private void OnFuriousSteel(Entity<HereticComponent> ent, ref EventHereticFuriousSteel args)
    {
        if (!TryUseAbility(ent, args))
            return;

        _pblade.AddProtectiveBlade(ent);
        for (var i = 1; i < 3; i++)
        {
            Timer.Spawn(TimeSpan.FromSeconds(0.5f * i),
                () =>
                {
                    if (TerminatingOrDeleted(ent))
                        return;

                    _pblade.AddProtectiveBlade(ent);
                });
        }

        args.Handled = true;
    }

    private void OnAscensionBlade(Entity<HereticComponent> ent, ref HereticAscensionBladeEvent args)
    {
        EnsureComp<SilverMaelstromComponent>(ent);
    }
}
