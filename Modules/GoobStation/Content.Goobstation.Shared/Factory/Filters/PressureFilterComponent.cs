// SPDX-FileCopyrightText: 2025 deltanedas <@deltanedas:kde.org>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Atmos;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Factory.Filters;

/// <summary>
/// Requires that the pressure of an entity's gas mixture is within some range.
/// Since atmos is server only, client will predict it blocking everything.
/// </summary>
[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class PressureFilterComponent : Component
{
    /// <summary>
    /// Minimum pressure to require.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float Min;

    /// <summary>
    /// Maximum pressure to require.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float Max = Atmospherics.OneAtmosphere * 10f;
}

[Serializable, NetSerializable]
public enum PressureFilterUiKey : byte
{
    Key
}

[Serializable, NetSerializable]
public sealed partial class PressureFilterSetMinMessage(float min) : BoundUserInterfaceMessage
{
    public readonly float Min = min;
}

[Serializable, NetSerializable]
public sealed partial class PressureFilterSetMaxMessage(float max) : BoundUserInterfaceMessage
{
    public readonly float Max = max;
}
