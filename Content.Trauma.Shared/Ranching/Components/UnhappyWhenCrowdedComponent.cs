// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Tag;

namespace Content.Trauma.Shared.Ranching.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class UnhappyWhenCrowdedComponent : Component
{
    [DataField]
    public int MinEntities = 6;

    [DataField]
    public int Range = 5;

    [DataField]
    public float HappinessToDecrease = -5;

    [DataField]
    public ProtoId<TagPrototype> Tag;

    [DataField]
    public TimeSpan UpdateFrequency = TimeSpan.FromSeconds(10);

    [DataField]
    public TimeSpan NextUpdate = TimeSpan.Zero;
}
