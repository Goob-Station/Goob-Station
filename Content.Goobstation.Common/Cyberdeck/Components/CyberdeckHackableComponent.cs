// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Common.Cyberdeck.Components;

/// <summary>
/// When Cyberdeck hacks this device, it will take
/// specified amount of charges.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class CyberdeckHackableComponent : Component
{
    [DataField, AutoNetworkedField]
    public int Cost = 1;

    [DataField("delay"), AutoNetworkedField]
    public TimeSpan HackingTime = TimeSpan.FromSeconds(2);

    [DataField]
    public EntProtoId? AfterHackingEffect = "CyberdeckAfterHackingEffect";
}
