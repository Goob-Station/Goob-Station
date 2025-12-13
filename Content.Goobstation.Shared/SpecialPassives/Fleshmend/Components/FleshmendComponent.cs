// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Marcus F <199992874+thebiggestbruh@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Alert;
using Content.Shared.Mobs;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;
using Robust.Shared.Utility;

namespace Content.Goobstation.Shared.SpecialPassives.Fleshmend.Components;

/// <summary>
///     Entities with this will rapidly heal physical injuries. This component holds the relevant data.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(fieldDeltas: true), AutoGenerateComponentPause]
public sealed partial class FleshmendComponent : Component
{
    /// <summary>
    /// The alert id of the component (if one should exist)
    /// </summary>
    [DataField]
    public ProtoId<AlertPrototype>? AlertId;

    /// <summary>
    /// How long should the effect go on for?
    /// </summary>
    [DataField, AutoNetworkedField]
    public float? Duration;

    [DataField, AutoNetworkedField]
    public TimeSpan MaxDuration = TimeSpan.Zero;

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoNetworkedField, AutoPausedField]
    public TimeSpan UpdateTimer = default!;

    /// <summary>
    /// Delay between healing ticks.
    /// </summary>
    [DataField, AutoNetworkedField]
    public TimeSpan UpdateDelay = TimeSpan.FromSeconds(1);

    /// <summary>
    /// Stores the sound source to be used in stopping the passive sound.
    /// </summary>
    [DataField]
    public EntityUid? SoundSource;

    /// <summary>
    /// Current mobstate of the entity.
    /// </summary>
    [DataField, AutoNetworkedField]
    public MobState Mobstate;

    /// <summary>
    /// The passive sound to be played.
    /// </summary>
    [DataField, AutoNetworkedField]
    public SoundSpecifier? PassiveSound;

    /// <summary>
    /// The ResPath to be used for the effect (in FleshmendEffectComponent)
    /// </summary>
    [DataField, AutoNetworkedField]
    public ResPath ResPath;

    /// <summary>
    /// The state for the effect's ResPath
    /// </summary>
    [DataField, AutoNetworkedField]
    public string? EffectState;

    /// <summary>
    /// Should the ability continue while on fire?
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool IgnoreFire = false;

    /// <summary>
    /// Should the ability continue while dead?
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool WorkWhileDead = false;

    [DataField, AutoNetworkedField]
    public float BruteHeal = -10f;

    [DataField, AutoNetworkedField]
    public float BurnHeal = -5f;

    [DataField, AutoNetworkedField]
    public float AsphyxHeal = -2f;

    [DataField, AutoNetworkedField]
    public float BleedingAdjust = -2.5f;

    [DataField, AutoNetworkedField]
    public float BloodLevelAdjust = 10f;
}
