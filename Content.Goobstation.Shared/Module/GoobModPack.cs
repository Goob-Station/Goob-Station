// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Module;

namespace Content.Goobstation.Shared.Module;

public sealed class GoobModPack : ModulePack
{
    public override string PackName => "Goobstation";

    public override IReadOnlySet<RequiredAssembly> RequiredAssemblies { get; } = new HashSet<RequiredAssembly>
    {
        RequiredAssembly.Client("Content.Goobstation.Client"),
        RequiredAssembly.Client("Content.Goobstation.UIKit"),
        RequiredAssembly.Server("Content.Goobstation.Server"),
        RequiredAssembly.Shared("Content.Goobstation.Maths"),
        RequiredAssembly.Shared("Content.Goobstation.Common"),
    };
}
