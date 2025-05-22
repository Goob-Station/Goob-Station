// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gluesniffler <linebarrelerenthusiast@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Shitmed.Medical.Surgery.Pain.Components;
using Content.Goobstation.Maths.FixedPoint;
using Robust.Shared.GameStates;

namespace Content.Shared._Shitmed.Medical.Surgery.Consciousness.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class ConsciousnessComponent : Component
{
    /// <summary>
    /// Represents the limit at which point the entity falls unconscious.
    /// </summary>
    [DataField(required: true)]
    [ViewVariables(VVAccess.ReadOnly)]
    public FixedPoint2 Threshold = 95;

    /// <summary>
    /// Represents the base consciousness value before applying any modifiers.
    /// </summary>
    [DataField]
    [ViewVariables(VVAccess.ReadOnly)]
    public FixedPoint2 RawConsciousness = -1;

    /// <summary>
    /// Gets the consciousness value after applying the multiplier and clamping between 0 and Cap.
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly)]
    public FixedPoint2 Consciousness => FixedPoint2.Clamp(RawConsciousness * Multiplier, 0, Cap);

    /// <summary>
    /// Represents the multiplier to be applied on the RawConsciousness.
    /// </summary>
    [DataField]
    [ViewVariables(VVAccess.ReadOnly)]
    public FixedPoint2 Multiplier = 1.0;

    /// <summary>
    /// Represents the maximum possible consciousness value. Also used as the default RawConsciousness value if it is set to -1.
    /// </summary>
    [DataField]
    [ViewVariables(VVAccess.ReadOnly)]
    public FixedPoint2 Cap = 190;

    /// <summary>
    /// Represents the collection of additional effects that modify the base consciousness level.
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly)]
    public Dictionary<(EntityUid, string), ConsciousnessModifier> Modifiers = new();

    /// <summary>
    /// Represents the collection of coefficients that further modulate the consciousness level.
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly)]
    public Dictionary<(EntityUid, string), ConsciousnessMultiplier> Multipliers = new();

    /// <summary>
    /// Defines which parts of the consciousness state are necessary for the entity.
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly)]
    public Dictionary<string, (EntityUid?, bool, bool)> RequiredConsciousnessParts = new();

    [ViewVariables(VVAccess.ReadOnly)]
    public Entity<NerveSystemComponent> NerveSystem = default;

    [DataField] // whoops.
    public TimeSpan ConsciousnessUpdateTime = TimeSpan.FromSeconds(0.8f);

    [ViewVariables(VVAccess.ReadOnly)]
    public TimeSpan NextConsciousnessUpdate;

    // Forceful control attributes, it's recommended not to use them directly.
    [ViewVariables(VVAccess.ReadWrite)]
    public bool PassedOut = false;

    [ViewVariables(VVAccess.ReadOnly)]
    public TimeSpan PassedOutTime = TimeSpan.Zero;

    [ViewVariables(VVAccess.ReadOnly)]
    public TimeSpan ForceConsciousnessTime = TimeSpan.Zero;

    [ViewVariables(VVAccess.ReadOnly)]
    public bool ForceDead;

    [ViewVariables(VVAccess.ReadOnly)]
    public bool ForceUnconscious;

    // funny
    [ViewVariables(VVAccess.ReadOnly)]
    public bool ForceConscious;

    [ViewVariables(VVAccess.ReadOnly)]
    public bool IsConscious = true;
    // Forceful control attributes, it's recommended not to use them directly.

    [DataField]
    public bool HasPainScreams;
}
