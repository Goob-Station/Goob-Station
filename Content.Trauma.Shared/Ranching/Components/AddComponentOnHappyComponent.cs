// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Trauma.Shared.Ranching.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class AddComponentOnHappyComponent : Component
{
    [DataField]
    public float HappinessRequired = 777f;

    [DataField]
    public ComponentRegistry Components;
}
