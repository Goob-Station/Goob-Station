using Content.Shared.Chat.Prototypes;
using Content.Shared.Chat.TypingIndicator;
using Content.Shared.DisplacementMap;
using Content.Shared.DoAfter;
using Content.Shared.Humanoid;
using Content.Shared.Humanoid.Markings;
using Content.Shared.Humanoid.Prototypes;
using Content.Shared.Speech;
using Robust.Shared.Enums;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using System.Numerics;

namespace Content.Shared._CorvaxGoob.AppearanceConverter;

[NetworkedComponent]
public abstract partial class SharedAppearanceConverterComponent : Component;

[Serializable, NetSerializable]
public enum AppearanceConverterUiKey : byte
{
    Key
}

[Serializable, NetSerializable]
public sealed class AppearanceConverterDNAScanDataMessage : BoundUserInterfaceMessage
{
    public readonly string? DNA;

    public AppearanceConverterDNAScanDataMessage(string? dna)
    {
        DNA = dna;
    }
}

[Serializable, NetSerializable]
public sealed class AppearanceConverterSelectProfileMessage : BoundUserInterfaceMessage
{
    public readonly string? DNA;

    public AppearanceConverterSelectProfileMessage(string? dna)
    {
        DNA = dna;
    }
}

[Serializable, NetSerializable]
public sealed class AppearanceConverterTransformMessage : BoundUserInterfaceMessage;

[Serializable, NetSerializable]
public sealed class AppearanceConverterDeTransformMessage : BoundUserInterfaceMessage;

[Serializable, NetSerializable]
public sealed class AppearanceConverterBoundUserInterfaceState : BoundUserInterfaceState
{
    public Dictionary<string, AppearanceConverterVisualTransformProfile>? Profiles;

    public string? SelectedProfile;

    public bool Transformed;

    public TimeSpan NextTransformTime;
}

[Serializable, NetSerializable]
public sealed partial class AppearanceConverterDoAfterEvent : SimpleDoAfterEvent;

/// <summary>
/// Набор данных, который позволяет полностью передать внешние черты и некоторые черты любого гуманоида.
/// </summary>
[Serializable, NetSerializable]
public struct TransformProfile
{
    public string Name;

    public string? Fingerprint;

    public string? InventorySpecies;

    public Dictionary<string, DisplacementData>? Displacements;

    public Dictionary<string, DisplacementData>? MaleDisplacements;

    public Dictionary<string, DisplacementData>? FemaleDisplacements;

    public string DNA;

    public string? Voice;

    public Gender? Gender;

    public int? Age;

    public Vector2 Scale;

    public ProtoId<SpeciesPrototype>? SpeciesPrototype;

    public ProtoId<SpeechVerbPrototype>? SpeechVerbPrototype;

    public ProtoId<TypingIndicatorPrototype>? TypingIndicator;

    public ProtoId<SpeechSoundsPrototype>? SpeechSounds;

    public ProtoId<EmoteSoundsPrototype>? EmoteSounds;

    public Color? SkinColor;

    public Color? EyesColor;

    public Sex? Sex;

    public MarkingSet? Markings;
}

/// <summary>
/// Кусок от <see cref="TransformProfile"/>, содержащий в себе только ту часть, которая передаётся в интерфейс игрока.
/// </summary>
[Serializable, NetSerializable]
public struct AppearanceConverterVisualTransformProfile
{
    public string Name;

    public string? Fingerprint;

    public string DNA;

    public int? Age;

    public ProtoId<SpeciesPrototype>? SpeciesPrototype;

    public Color? SkinColor;

    public Color? EyesColor;

    public Sex? Sex;

    public MarkingSet? Markings;
}
