// SPDX-FileCopyrightText: 2025 August Eymann <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Ted Lukin <66275205+pheenty@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Module;

namespace Content.Pirate.Shared.Module;

public sealed class PirateModPack : ModulePack
{
    public override string PackName => "Pirate";

    public override IReadOnlySet<RequiredAssembly> RequiredAssemblies { get; } = new HashSet<RequiredAssembly>
    {
        RequiredAssembly.Client("Content.Pirate.Client"),
        RequiredAssembly.Client("Content.Pirate.UIKit"),
        RequiredAssembly.Server("Content.Pirate.Server"),
        RequiredAssembly.Shared("Content.Pirate.Common"),
    };
}
