// SPDX-FileCopyrightText: 2025 Armok <155400926+ARMOKS@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Solstice <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._DV.CosmicCult.Prototypes;
using Content.Shared.Damage;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._DV.CosmicCult.Components;

[RegisterComponent, NetworkedComponent, Access(typeof(SharedMonumentSystem))]
[AutoGenerateComponentState, AutoGenerateComponentPause]
public sealed partial class MonumentComponent : Component
{
    /// <summary>
    /// The sound effect played when entropy is infused into The Monument.
    /// </summary>
    [DataField]
    public SoundSpecifier InfusionSFX = new SoundPathSpecifier("/Audio/_DV/CosmicCult/insert_entropy.ogg");

    /// <summary>
    /// the list of glyphs that this monument is allowed to scribe
    /// </summary>
    [DataField, AutoNetworkedField]
    public HashSet<ProtoId<GlyphPrototype>> UnlockedGlyphs = [];

    /// <summary>
    /// the glyph that will be scribed when the button is pressed
    /// </summary>
    [DataField, AutoNetworkedField]
    public ProtoId<GlyphPrototype> SelectedGlyph;

    /// <summary>
    /// the total amount of entropy that has been inserted into the monument
    /// </summary>
    [DataField, AutoNetworkedField]
    public int TotalEntropy;

    /// <summary>
    /// how much progress (entropy and converted crew) the cult has made
    /// </summary>
    [DataField, AutoNetworkedField]
    public int CurrentProgress;

    /// <summary>
    /// how much progress the cult need to make to tier up
    /// </summary>
    [DataField, AutoNetworkedField]
    public int TargetProgress;

    /// <summary>
    /// offset used to make the progress bar reset to 0 every time
    /// </summary>
    [DataField, AutoNetworkedField]
    public int ProgressOffset;

    /// <summary>
    /// A bool we use to set whether The Monument's UI is available or not.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool Enabled = true;

    /// <summary>
    /// how long the monument takes to transform on a tier up
    /// </summary>
    [DataField]
    public TimeSpan TransformTime = TimeSpan.FromSeconds(2.8);

    /// <summary>
    /// the entity for the currently scribed glyph
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntityUid? CurrentGlyph;

    /// <summary>
    /// the timer used for ticking healing from vacuous vitality
    /// </summary>
    [AutoPausedField, DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    public TimeSpan CheckTimer = default!;

    /// <summary>
    /// the amount of time between the above timer's ticks
    /// </summary>
    [DataField]
    public TimeSpan CheckWait = TimeSpan.FromSeconds(1);

    /// <summary>
    /// Passive healing factor for cultists w/ the ability near the monument
    /// </summary>
    [DataField]
    public DamageSpecifier MonumentHealing = new()
    {
        DamageDict = new()
        {
            { "Blunt", 1},
            { "Slash", 1 },
            { "Piercing", 1 },
            { "Heat", 1},
            { "Shock", 1},
            { "Cold", 1},
            { "Poison", 1},
            { "Radiation", 1},
            { "Asphyxiation", 1 }
        }
    };

    /// <summary>
    /// wether or not there's a stage change queued
    /// </summary>
    [DataField]
    public bool CanTierUp = true;

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField]
    public TimeSpan? PhaseOutTimer;
}

[Serializable, NetSerializable]
public sealed class InfluenceSelectedMessage(ProtoId<InfluencePrototype> influenceProtoId) : BoundUserInterfaceMessage
{
    public ProtoId<InfluencePrototype> InfluenceProtoId = influenceProtoId;
}

[Serializable, NetSerializable]
public sealed class GlyphSelectedMessage(ProtoId<GlyphPrototype> glyphProtoId) : BoundUserInterfaceMessage
{
    public ProtoId<GlyphPrototype> GlyphProtoId = glyphProtoId;
}

[Serializable, NetSerializable]
public sealed class GlyphRemovedMessage : BoundUserInterfaceMessage;

[Serializable, NetSerializable]
public enum MonumentVisuals : byte
{
    Monument,
    Transforming,
    FinaleReached,
    Tier3,
}

[Serializable, NetSerializable]
public enum MonumentVisualLayers : byte
{
    MonumentLayer,
    TransformLayer,
    FinaleLayer,
}
