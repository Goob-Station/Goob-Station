// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Common.Standing;

namespace Content.Server._White.Traits.Assorted;

public sealed class LayingDownModifierSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<LayingDownModifierComponent, ComponentStartup>(OnStartup);
    }

    private void OnStartup(EntityUid uid, LayingDownModifierComponent component, ComponentStartup args)
    {
        if (!TryComp<LayingDownComponent>(uid, out var layingDown))
            return;

        layingDown.StandingUpTime *= component.LayingDownCooldownMultiplier;
        layingDown.SpeedModify *= component.DownedSpeedMultiplierMultiplier;
    }
}