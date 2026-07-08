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
