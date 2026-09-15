// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Prototypes;

namespace Content.Shared._Oskarrr.WildLandsTribe;

/// <summary>
/// Spawns all listed aborigine ghost-role spawners once, then deletes itself.
/// </summary>
[RegisterComponent]
public sealed partial class WildLandsTribeCampSpawnerComponent : Component
{
    [DataField(required: true)]
    public List<EntProtoId> Prototypes = new();

    [DataField]
    public float Offset = 1.5f;
}
