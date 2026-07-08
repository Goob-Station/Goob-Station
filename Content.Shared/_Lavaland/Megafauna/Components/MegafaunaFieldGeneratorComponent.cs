// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Lavaland.EntityShapes.Shapes;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Lavaland.Megafauna.Components;

/// <summary>
/// Generates a square field  around the megafauna then it starts attacking.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MegafaunaFieldGeneratorComponent : Component
{
    [ViewVariables, AutoNetworkedField]
    public bool Enabled;

    [ViewVariables, AutoNetworkedField]
    public List<EntityUid> Walls = new();

    [DataField(required: true)]
    public EntityShape WallShape;

    [DataField, AutoNetworkedField]
    public EntProtoId WallId;
}
