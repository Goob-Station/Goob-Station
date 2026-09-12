// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Actions;
using Content.Shared.EntityEffects;
using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared._Trauma.Actions;

/// <summary>
/// Applies entity effects to the target entity when using <see cref="EffectInstantActionEvent"/> or <see cref="EffectTargetActionEvent"/>.
/// Applies it to the performer as well if <see cref="OnPerformed"/> is true, regardless of event used.
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(EffectActionSystem))]
public sealed partial class EffectActionComponent : Component
{
    /// <summary>
    /// The effects to apply.
    /// </summary>
    [DataField(required: true)]
    public EntityEffect[] Effects = default!;

    /// <summary>
    /// Applies effects to the performer if true, regardless of event used.
    /// </summary>
    [DataField]
    public bool OnPerformed;
}

public sealed partial class EffectInstantActionEvent : InstantActionEvent;

public sealed partial class EffectTargetActionEvent : EntityTargetActionEvent;
