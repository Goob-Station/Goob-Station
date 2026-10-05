// SPDX-License-Identifier: AGPL-3.0-or-later

using System;
using Content.Goobstation.Shared.IoC;
using Robust.Shared.ContentPack;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Entry;

public sealed class EntryPoint : GameShared
{
    [Dependency] private IPrototypeManager _proto = default!;

    public override void PreInit()
    {
        IoCManager.InjectDependencies(this);
        SharedGoobContentIoC.Register();
    }

    public override void Init()
    {
        base.Init();

        _proto.PartialDirectory(new("/Prototypes/_Goobstation/Partials"), 0);
        _proto.PartialDirectory(new("/Prototypes/_Trauma/Partials"), 1); // Since trauma is doing this too
    }
}