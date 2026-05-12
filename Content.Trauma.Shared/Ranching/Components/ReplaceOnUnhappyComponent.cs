// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Trauma.Shared.Ranching.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class ReplaceOnUnhappyComponent : Component
{
    [DataField]
    public float HappinessRequired = -777f;

    [DataField]
    public EntProtoId Ent;
}
