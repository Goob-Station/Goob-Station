using Content.Shared._CorvaxGoob.AppearanceConverter;
using Content.Shared.Chat.Prototypes;
using Content.Shared.Chat.TypingIndicator;
using Content.Shared.DisplacementMap;
using Content.Shared.Inventory;
using Content.Shared.Speech;
using Robust.Shared.Audio;
using Robust.Shared.Enums;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using System.Numerics;

namespace Content.Server._CorvaxGoob.AppearanceConverter;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class AppearanceConverterComponent : SharedAppearanceConverterComponent
{
    [DataField]
    public SlotFlags Slots = SlotFlags.GLOVES;

    [DataField]
    public SoundSpecifier? TransformSound = new SoundCollectionSpecifier("RadiationPulse");

    [DataField]
    public TransformProfile? OriginalForm;

    [DataField]
    public EntityUid? Equipee;

    [DataField]
    public TimeSpan ScanningDoAfterTime = TimeSpan.FromSeconds(5);

    [DataField]
    public TimeSpan TransformDelay = TimeSpan.FromSeconds(30);

    [DataField]
    public Dictionary<string, AppearanceConverterDetailTransformProfile> ProfilesDetailData = new();

    // AutoNetworkable

    [DataField, AutoNetworkedField]
    public Dictionary<string, AppearanceConverterVisualTransformProfile> ProfilesVisualData = new();

    [DataField, AutoNetworkedField]
    public string? SelectedProfile = null;

    [DataField, AutoNetworkedField]
    public bool Transformed = false;

    [DataField, AutoNetworkedField]
    public TimeSpan NextTransformTime = TimeSpan.Zero;
}

/// <summary>
/// Кусок от <see cref="TransformProfile"/>, содержащий в себе только серверную часть.
/// </summary>
public struct AppearanceConverterDetailTransformProfile
{
    public string? InventorySpecies;

    public Dictionary<string, DisplacementData>? Displacements;

    public Dictionary<string, DisplacementData>? MaleDisplacements;

    public Dictionary<string, DisplacementData>? FemaleDisplacements;

    public string? Voice;

    public Gender? Gender;

    public Vector2 Scale;

    public ProtoId<SpeechVerbPrototype>? SpeechVerbPrototype;

    public ProtoId<TypingIndicatorPrototype>? TypingIndicator;

    public ProtoId<SpeechSoundsPrototype>? SpeechSounds;

    public ProtoId<EmoteSoundsPrototype>? EmoteSounds;
}
