namespace Content.Shared._Goobstation.VentCrawling.Components;

[RegisterComponent, Virtual]
public partial class VentCrawlerJunctionComponent : Component
{
    /// <summary>
    ///     The angles to connect to.
    /// </summary>
    [DataField("degrees")] public List<Angle> Degrees = new();
}
