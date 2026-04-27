// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.EntityEffects;

namespace Content.Trauma.Shared.StatusEffects;

/// <summary>
/// Status effect that applies entity effects on removal and on apply.
/// The name sucks, but I can't think of a better one.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class StatusEffectEffectsApplyComponent : Component
{
    /// <summary>
    /// The effects to apply when the status effect is applied.
    /// </summary>
    [DataField]
    public EntityEffect[]? EffectsOnApply;

    /// <summary>
    /// The effects to apply when the status effect is removed.
    /// </summary>
    [DataField]
    public EntityEffect[]? EffectsOnRemoval;
}
