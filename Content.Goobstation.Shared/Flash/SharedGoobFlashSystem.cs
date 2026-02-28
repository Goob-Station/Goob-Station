// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Common.Flash;

namespace Content.Goobstation.Shared.Flash;

public sealed class SharedGoobFlashSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<FlashVulnerableComponent, CheckFlashVulnerable>(OnFlashVulnerableCheck);
    }

    public void OnFlashVulnerableCheck(Entity<FlashVulnerableComponent> ent, ref CheckFlashVulnerable args)
    {
        args.Vulnerable = true;
    }
}
