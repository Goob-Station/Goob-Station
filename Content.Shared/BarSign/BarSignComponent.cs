// SPDX-FileCopyrightText: 2022 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared.BarSign;

/// <summary>
/// Makes it possible to switch this entity's sprite and name using a BUI.
/// <seealso cref="BarSignPrototype"/>
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true)]
public sealed partial class BarSignComponent : Component
{
    /// <summary>
    /// The current bar sign prototype being displayed.
    /// </summary>
    [DataField, AutoNetworkedField]
    public ProtoId<BarSignPrototype>? Current;
}

/// <summary>
/// The key for the BoundUserInterface.
/// </summary>
[Serializable, NetSerializable]
public enum BarSignUiKey : byte
{
    Key
}

/// <summary>
/// The enum to be used for appearance data of the bar sign.
/// </summary>
[Serializable, NetSerializable]
public enum BarSignVisuals : byte
{
    BarSignPrototype,
}

/// <summary>
/// Send from the client when setting the bar sign.
/// </summary>
[Serializable, NetSerializable]
public sealed class SetBarSignMessage(ProtoId<BarSignPrototype> sign) : BoundUserInterfaceMessage
{
    /// <summary>
    /// The new prototype to use.
    /// </summary>
    public ProtoId<BarSignPrototype> Sign = sign;
}