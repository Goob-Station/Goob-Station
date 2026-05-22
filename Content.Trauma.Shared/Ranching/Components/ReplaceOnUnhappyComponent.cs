// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Trauma.Shared.Ranching.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class ReplaceOnUnhappyComponent : Component
{
    [DataField]
    public float HappinessRequired = -777f;

    [DataField(required:true)]
    public EntProtoId Ent;
}
