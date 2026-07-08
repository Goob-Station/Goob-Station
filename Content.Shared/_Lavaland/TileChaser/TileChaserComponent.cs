// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Lavaland.TileChaser;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class TileChaserComponent : Component
{
    [ViewVariables, AutoNetworkedField]
    public EntityUid? Target;

    [DataField]
    public float Speed = 4.5f;

    [DataField]
    public int MaxSteps = 20;

    [ViewVariables, AutoNetworkedField]
    public int Steps;

    [DataField]
    public float BaseCooldown = 1f;

    [ViewVariables]
    public float CooldownTimer;

    [DataField(required: true)]
    public EntProtoId Spawn;
}
