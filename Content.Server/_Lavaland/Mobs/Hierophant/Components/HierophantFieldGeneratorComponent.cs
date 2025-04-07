// SPDX-FileCopyrightText: 2025 Rouden <149893554+Roudenn@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later
using Robust.Shared.Prototypes;

namespace Content.Server._Lavaland.Mobs.Hierophant.Components;

[RegisterComponent]
public sealed partial class HierophantFieldGeneratorComponent : Component
{
    [ViewVariables]
    public bool Enabled;

    [ViewVariables]
    public List<EntityUid> Walls = new();

    [DataField]
    public int Radius;

    [DataField]
    public EntProtoId HierophantPrototype = "LavalandBossHierophant";

    [DataField]
    public EntProtoId WallPrototype = "WallHierophantArenaTemporary";

    [DataField]
    public EntityUid? ConnectedHierophant;
}