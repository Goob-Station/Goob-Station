// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared._Goobstation.Weapons.SmartGun;

/// <summary>
/// Component attached to an entity in nullspace,
/// manages laser pointer lines for trails instead of using pvs overrides for each laser pointer.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class LaserPointerManagerComponent : Component
{
    [DataField, AutoNetworkedField]
    public Dictionary<NetEntity, LaserPointerData> Data = new();
}

[DataDefinition, Serializable, NetSerializable]
public sealed partial class LaserPointerData(Color color, Vector2 start, Vector2 end)
{
    [ViewVariables]
    public Color Color = color;

    [ViewVariables]
    public Vector2 Start = start;

    [ViewVariables]
    public Vector2 End = end;

    public LaserPointerData() : this(Color.Red, Vector2.Zero, Vector2.Zero)
    {
    }
}