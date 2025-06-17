// SPDX-License-Identifier: AGPL-3.0-or-later

using System;
using Content.Pirate.Shared.IoC;
using Robust.Shared.ContentPack;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;
using Robust.Shared.Prototypes;

namespace Content.Pirate.Shared.Entry;

public sealed class EntryPoint : GameShared
{
    public override void PreInit()
    {
        IoCManager.InjectDependencies(this);
        SharedPirateContentIoC.Register();
    }
}
