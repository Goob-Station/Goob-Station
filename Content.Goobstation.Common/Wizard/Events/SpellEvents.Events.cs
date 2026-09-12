using Robust.Shared.Audio;

namespace Content.Goobstation.Common.Wizard.Events;

[DataDefinition]
public sealed partial class GlobalTileToggleEvent : EntityEventArgs
{
    [DataField]
    public SoundSpecifier? Sound = new SoundPathSpecifier("/Audio/_Goobstation/Wizard/ghost.ogg");
}

[DataDefinition]
public sealed partial class SummonGhostsEvent : EntityEventArgs
{
    [DataField]
    public SoundSpecifier? Sound = new SoundPathSpecifier("/Audio/_Goobstation/Wizard/ghost2.ogg");
}

[DataDefinition]
public sealed partial class DimensionShiftEvent : EntityEventArgs
{
    [DataField]
    public SoundSpecifier? Sound = new SoundPathSpecifier("/Audio/_Goobstation/Wizard/ghost.ogg");

    [DataField]
    public float OxygenMoles = 10f;

    [DataField]
    public float NitrogenMoles = 10f;

    [DataField]
    public float CarbonDioxideMoles = 10f;

    [DataField]
    public float Temperature = 273.15f - 5f; // issue of cant access Atmospherics.T0C. whateverrr its a universal constant

    [DataField]
    public string? Parallax = "Wizard";
}