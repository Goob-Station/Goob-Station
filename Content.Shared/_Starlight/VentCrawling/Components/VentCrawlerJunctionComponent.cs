namespace Content.Shared._Starlight.VentCrawling.Components;

[RegisterComponent, Virtual]
public partial class VentCrawlerJunctionComponent : Component
{
    /// <summary>
    ///     The angles to connect to.
    /// </summary>
    [DataField("degrees")] public List<Angle> Degrees = new();
}
