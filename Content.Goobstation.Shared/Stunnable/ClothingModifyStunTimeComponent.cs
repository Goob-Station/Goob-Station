// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Stunnable;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ClothingModifyStunTimeComponent : Component
{
    [DataField, AutoNetworkedField, ViewVariables(VVAccess.ReadWrite)]
    public float Modifier = 1f;
}