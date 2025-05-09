// SPDX-FileCopyrightText: 2025 GoidaBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Scruq445 <storchdamien@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goidastation.Common.Changeling;
using Content.Goidastation.Server.Implants.Components;
using Content.Shared.Implants;

namespace Content.Goidastation.Server.Implants.Systems;

public sealed class ChangelingImplantSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ChangelingImplantComponent, ImplantImplantedEvent>(OnImplanted);
    }

    public void OnImplanted(EntityUid uid, ChangelingImplantComponent comp, ref ImplantImplantedEvent ev)
    {
        if (ev.Implanted.HasValue)
            EnsureComp<ChangelingComponent>(ev.Implanted.Value);
    }
}
