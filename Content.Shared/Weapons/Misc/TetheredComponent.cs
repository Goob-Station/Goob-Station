// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.GameStates;

namespace Content.Shared.Weapons.Misc;

/// <summary>
/// Added to entities tethered by a tethergun.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class TetheredComponent : Component
{
    [DataField("tetherer"), AutoNetworkedField]
    public EntityUid Tetherer;

    [ViewVariables(VVAccess.ReadWrite), DataField("originalAngularDamping"), AutoNetworkedField]
    public float OriginalAngularDamping;
}