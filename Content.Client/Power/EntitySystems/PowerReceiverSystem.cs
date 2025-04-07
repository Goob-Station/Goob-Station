// SPDX-FileCopyrightText: 2024 Aidenkrz <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Mervill <mervills.email@gmail.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 ShadowCommander <10494922+ShadowCommander@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later
using System.Diagnostics.CodeAnalysis;
using Content.Client.Power.Components;
using Content.Shared.Power.Components;
using Content.Shared.Power.EntitySystems;
using Content.Shared.Examine;
using Robust.Shared.GameStates;

namespace Content.Client.Power.EntitySystems;

public sealed class PowerReceiverSystem : SharedPowerReceiverSystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ApcPowerReceiverComponent, ExaminedEvent>(OnExamined);
        SubscribeLocalEvent<ApcPowerReceiverComponent, ComponentHandleState>(OnHandleState);
    }

    private void OnExamined(Entity<ApcPowerReceiverComponent> ent, ref ExaminedEvent args)
    {
        args.PushMarkup(GetExamineText(ent.Comp.Powered));
    }

    private void OnHandleState(EntityUid uid, ApcPowerReceiverComponent component, ref ComponentHandleState args)
    {
        if (args.Current is not ApcPowerReceiverComponentState state)
            return;

        component.Powered = state.Powered;
    }

    public override bool ResolveApc(EntityUid entity, [NotNullWhen(true)] ref SharedApcPowerReceiverComponent? component)
    {
        if (component != null)
            return true;

        if (!TryComp(entity, out ApcPowerReceiverComponent? receiver))
            return false;

        component = receiver;
        return true;
    }
}
