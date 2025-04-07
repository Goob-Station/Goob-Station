// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 SlamBamActionman <83650252+SlamBamActionman@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared.Roles.RoleCodeword;

/// <summary>
/// Used to display and highlight codewords in chat messages on the client.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, Access(typeof(SharedRoleCodewordSystem), Other = AccessPermissions.Read)]
public sealed partial class RoleCodewordComponent : Component
{
    /// <summary>
    /// Contains the codewords tied to a role.
    /// Key string should be unique for the role.
    /// </summary>
    [DataField, AutoNetworkedField]
    public Dictionary<string, CodewordsData> RoleCodewords = new();

    public override bool SessionSpecific => true;
}

[DataDefinition, Serializable, NetSerializable]
public partial struct CodewordsData
{
    [DataField]
    public Color Color;

    [DataField]
    public List<string> Codewords;

    public CodewordsData(Color color, List<string> codewords)
    {
        Color = color;
        Codewords = codewords;
    }
}