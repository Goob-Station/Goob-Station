namespace Content.Pirate.Server.Research.SophicScribe;

[RegisterComponent]
public sealed partial class SophicScribeComponent : Component
{
    [DataField("accumulator")]
    public float Accumulator;

    [DataField("announceInterval")]
    public TimeSpan AnnounceInterval = TimeSpan.FromMinutes(2);

    [DataField("nextAnnounce")]
    public TimeSpan NextAnnounceTime;

    /// <summary>
    ///     Antispam.
    /// </summary>
    public TimeSpan StateTime = default!;

    [DataField("stateCD")]
    public TimeSpan StateCD = TimeSpan.FromSeconds(5);
}
