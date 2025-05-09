// SPDX-FileCopyrightText: 2023 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Shared.NameIdentifier;

/// <summary>
/// Generates a unique numeric identifier for entities, with specifics controlled by a <see cref="NameIdentifierGroupPrototype"/>.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class NameIdentifierComponent : Component
{
    [DataField("group", required: true, customTypeSerializer:typeof(PrototypeIdSerializer<NameIdentifierGroupPrototype>))]
    public string Group = string.Empty;

    /// <summary>
    /// The randomly generated ID for this entity.
    /// </summary>
    [DataField("identifier"), ViewVariables(VVAccess.ReadWrite), AutoNetworkedField]
    public int Identifier = -1;

    /// <summary>
    /// The full name identifier for this entity.
    /// </summary>
    [DataField("fullIdentifier"), ViewVariables(VVAccess.ReadWrite), AutoNetworkedField]
    public string FullIdentifier = string.Empty;
}