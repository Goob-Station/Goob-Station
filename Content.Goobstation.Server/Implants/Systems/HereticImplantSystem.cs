// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Scruq445 <storchdamien@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Server.Implants.Components;
using Content.Shared.Heretic;
using Content.Shared.Implants;

namespace Content.Goobstation.Server.Implants.Systems;

public sealed class HereticImplantSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<HereticImplantComponent, ImplantImplantedEvent>(OnImplanted);
    }

    public void OnImplanted(EntityUid uid, HereticImplantComponent comp, ref ImplantImplantedEvent ev)
    {
        if (ev.Implanted.HasValue)
            EnsureComp<HereticComponent>(ev.Implanted.Value);
    }
}
