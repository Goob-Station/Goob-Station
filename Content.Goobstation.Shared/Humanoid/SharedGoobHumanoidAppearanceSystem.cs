// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Humanoid;

namespace Content.Goobstation.Shared.Humanoid;

public sealed class SharedGoobHumanoidAppearanceSystem : EntitySystem
{
    [Dependency] private readonly SharedHumanoidAppearanceSystem _humanoidAppearanceSystem = default!;

    public void SwapSex(EntityUid uid, HumanoidAppearanceComponent? humanoid = null)
    {
        if (!Resolve(uid, ref humanoid)
            || humanoid.Sex == Sex.Unsexed)
            return;

        // Not set up for future possible alien sexes
        if (humanoid.Sex == Sex.Male)
        {
            _humanoidAppearanceSystem.SetSex(uid, Sex.Female);
            return;
        }
        _humanoidAppearanceSystem.SetSex(uid,Sex.Male);

    }
}
