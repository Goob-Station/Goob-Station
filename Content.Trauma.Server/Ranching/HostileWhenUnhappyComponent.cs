// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.NPC.HTN;

namespace Content.Trauma.Server.Ranching;

/// <summary>
/// Switches the HTN of an entity when their happiness reaches a certain threshold then switches it back when it reaches a values higher than <see cref="HappinessRequired"/>
/// </summary>
[RegisterComponent]
public sealed partial class HostileWhenUnhappyComponent : Component
{
    [DataField]
    public float HappinessRequired = -10f;

    [DataField(required: true)]
    public HTNCompoundTask UnhappyTask;

    [DataField(required: true)]
    public HTNCompoundTask HappyTask;
}
