// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.EntityEffects;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Goobstation.Shared._Trauma.StatusEffects;

/// <summary>
/// Regularly applies entity effects to the target entity.
/// This is predicted for the target entity, other mobs effects aren't predicted.
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(StatusEffectEffectsSystem))]
[AutoGenerateComponentState, AutoGenerateComponentPause]
public sealed partial class StatusEffectEffectsComponent : Component
{
    /// <summary>
    /// The effects to apply.
    /// </summary>
    [DataField(required: true)]
    public EntityEffect[] Effects = default!;

    /// <summary>
    /// How long to wait between updates.
    /// </summary>
    [DataField]
    public TimeSpan UpdateDelay = TimeSpan.FromSeconds(1);

    /// <summary>
    /// When to next apply effects to the target.
    /// </summary>
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    [AutoPausedField, AutoNetworkedField]
    public TimeSpan NextUpdate;
}
