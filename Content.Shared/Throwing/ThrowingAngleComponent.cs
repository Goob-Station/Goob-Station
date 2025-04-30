// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.GameStates;

namespace Content.Shared.Throwing;

/// <summary>
/// When thrown applies a specific angle to the thrown entity.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ThrowingAngleComponent : Component
{
    /// <summary>
    /// Do we apply throwing spin to the entity.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField("angularVelocity"), AutoNetworkedField]
    public bool AngularVelocity;

    [ViewVariables(VVAccess.ReadWrite), DataField("angle"), AutoNetworkedField]
    public Angle Angle;
}