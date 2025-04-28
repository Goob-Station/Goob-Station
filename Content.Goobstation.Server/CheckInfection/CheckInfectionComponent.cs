using Robust.Shared.Audio;

namespace Content.Goobstation.Server.CheckInfection;

[RegisterComponent]
public sealed partial class CheckInfectionComponent : Component
{
    [DataField]
    public TimeSpan DoAfterDuration = TimeSpan.FromSeconds(5);

    [DataField]
    public SoundSpecifier ScanningEndSound = new SoundPathSpecifier("/Audio/Items/Medical/healthscanner.ogg");

    /// <summary>
    /// Who was the target of the last scan?
    /// </summary>
    [ViewVariables]
    public EntityUid? LastTarget;

    /// <summary>
    /// Was the last scanned target infected?
    /// </summary>
    [ViewVariables]
    public bool WasInfected;

}
