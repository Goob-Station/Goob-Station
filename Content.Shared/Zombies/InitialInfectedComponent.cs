// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.StatusIcon;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.Zombies;

[RegisterComponent, NetworkedComponent]
public sealed partial class InitialInfectedComponent : Component
{
    [DataField]
    public ProtoId<FactionIconPrototype> StatusIcon = "InitialInfectedFaction";
}