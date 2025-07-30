// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Lumminal <81829924+Lumminal@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Werewolf.Prototypes;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Werewolf.Components;

[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class WerewolfTransformComponent : Component
{
    /// <summary>
    /// The current werewolf form we have selected to transform into
    /// </summary>
    [AutoNetworkedField]
    [DataField]
    public ProtoId<WerewolfFormPrototype> CurrentWerewolfForm;

    /// <summary>
    /// The list of all available forms to the werewolf
    /// </summary>
    [AutoNetworkedField]
    [DataField]
    public HashSet<ProtoId<WerewolfFormPrototype>> WerewolfForms = new();

    /// <summary>
    /// The list of unlocked forms to the werewolf
    /// </summary>
    [AutoNetworkedField]
    [DataField]
    public HashSet<ProtoId<WerewolfFormPrototype>> UnlockedWerewolfForms = new();
}
