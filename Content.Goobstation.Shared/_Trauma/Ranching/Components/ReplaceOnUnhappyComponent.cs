// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
namespace Content.Goobstation.Shared._Trauma.Ranching.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class ReplaceOnUnhappyComponent : Component
{
    [DataField]
    public float HappinessRequired = -777f;

    [DataField(required:true)]
    public EntProtoId Ent;
}
