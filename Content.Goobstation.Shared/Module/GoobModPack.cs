// SPDX-FileCopyrightText: 2025 August Eymann <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Ted Lukin <66275205+pheenty@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Module;

namespace Content.Goobstation.Shared.Module;

public sealed class GoobModPack : ModulePack
{
    public override string PackName => "Goobstation"; // would call it goobmod but will make it clearer what your supposed to be rebuilding

    public override IReadOnlySet<RequiredAssembly> RequiredAssemblies { get; } = new HashSet<RequiredAssembly>
    {
        RequiredAssembly.ForClient("Content.Goobstation.Client"),
        RequiredAssembly.ForClient("Content.Goobstation.UIKit.Client"),
        RequiredAssembly.ForServer("Content.Goobstation.Server"),
    };
}
