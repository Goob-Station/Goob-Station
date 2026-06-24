// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Trauma.Shared.TimedReplace;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentPause]
public sealed partial class TimedReplaceComponent : Component
{
    /// <summary>
    /// What entity should spawn
    /// </summary>
    [DataField(required: true)]
    public EntProtoId Entity;

    /// <summary>
    /// How long to replace
    /// </summary>
    [DataField]
    public TimeSpan Time = TimeSpan.FromSeconds(60);

    [DataField]
    public bool Active;

    /// <summary>
    /// The time at which the entity this component is attached to will be replaced with <see cref="Entity"/>
    /// </summary>
    [DataField, AutoPausedField]
    public TimeSpan SpawnTime = TimeSpan.Zero;
}
