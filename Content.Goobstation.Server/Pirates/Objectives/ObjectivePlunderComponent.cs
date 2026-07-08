// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Goobstation.Server.Pirates.Objectives;

[RegisterComponent]
public sealed partial class ObjectivePlunderComponent : Component
{
    //[ViewVariables(VVAccess.ReadOnly)][NonSerialized] public Entity<ResourceSiphonComponent>? BoundSiphon;
    [DataField] public float Plundered = 0f;
}