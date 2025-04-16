// SPDX-FileCopyrightText: 2023 Just-a-Unity-Dev <just-a-unity-dev@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Kara <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2023 LankLTE <135308300+LankLTE@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 LankLTE <twlowe06@gmail.com>
// SPDX-FileCopyrightText: 2023 eclips_e <67359748+Just-a-Unity-Dev@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Defusable.Components;
using Content.Server.Defusable.Systems;
using Content.Server.Wires;
using Content.Shared.Defusable;
using Content.Shared.Wires;

namespace Content.Server.Defusable.WireActions;

public sealed partial class DelayWireAction : ComponentWireAction<DefusableComponent>
{
    public override Color Color { get; set; } = Color.Yellow;
    public override string Name { get; set; } = "wire-name-bomb-delay";
    public override bool LightRequiresPower { get; set; } = false;

    public override StatusLightState? GetLightState(Wire wire, DefusableComponent comp)
    {
        return comp.DelayWireUsed ? StatusLightState.On : StatusLightState.Off;
    }

    public override object StatusKey { get; } = DefusableWireStatus.DelayIndicator;

    public override bool Cut(EntityUid user, Wire wire, DefusableComponent comp)
    {
        return true;
    }

    public override bool Mend(EntityUid user, Wire wire, DefusableComponent comp)
    {
        return true;
    }

    public override void Pulse(EntityUid user, Wire wire, DefusableComponent comp)
    {
        EntityManager.System<DefusableSystem>().DelayWirePulse(user, wire, comp);
    }
}