// SPDX-FileCopyrightText: 2022 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2023 Scribbles0 <91828755+Scribbles0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Errant <35878406+Errant-4@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Server.Atmos.EntitySystems;
using Content.Server.Body.Systems;
using Content.Server.Disposal.Unit.Components;

namespace Content.Server.Disposal.Unit.EntitySystems;

public sealed class BeingDisposedSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BeingDisposedComponent, InhaleLocationEvent>(OnInhaleLocation);
        SubscribeLocalEvent<BeingDisposedComponent, ExhaleLocationEvent>(OnExhaleLocation);
        SubscribeLocalEvent<BeingDisposedComponent, AtmosExposedGetAirEvent>(OnGetAir);
    }

    private void OnGetAir(EntityUid uid, BeingDisposedComponent component, ref AtmosExposedGetAirEvent args)
    {
        if (TryComp<DisposalHolderComponent>(component.Holder, out var holder))
        {
            args.Gas = holder.Air;
            args.Handled = true;
        }
    }

    private void OnInhaleLocation(EntityUid uid, BeingDisposedComponent component, InhaleLocationEvent args)
    {
        if (TryComp<DisposalHolderComponent>(component.Holder, out var holder))
        {
            args.Gas = holder.Air;
        }
    }

    private void OnExhaleLocation(EntityUid uid, BeingDisposedComponent component, ExhaleLocationEvent args)
    {
        if (TryComp<DisposalHolderComponent>(component.Holder, out var holder))
        {
            args.Gas = holder.Air;
        }
    }
}