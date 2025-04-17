// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 username <113782077+whateverusername0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 whateverusername0 <whateveremail>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <aviu00@protonmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Heretic;

namespace Content.Server.Heretic.Abilities;

public sealed partial class HereticAbilitySystem
{
    private void SubscribeLock()
    {
        SubscribeLocalEvent<HereticComponent, EventHereticBulglarFinesse>(OnBulglarFinesse);
        SubscribeLocalEvent<HereticComponent, EventHereticLastRefugee>(OnLastRefugee);
        // add eldritch id here

        SubscribeLocalEvent<HereticComponent, HereticAscensionLockEvent>(OnAscensionLock);
    }

    private void OnBulglarFinesse(Entity<HereticComponent> ent, ref EventHereticBulglarFinesse args)
    {

    }
    private void OnLastRefugee(Entity<HereticComponent> ent, ref EventHereticLastRefugee args)
    {

    }

    private void OnAscensionLock(Entity<HereticComponent> ent, ref HereticAscensionLockEvent args)
    {

    }
}
