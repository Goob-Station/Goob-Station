using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Antag.Intro;

/// <summary>
/// A full-screen animation played when you get an antagonist role.
/// </summary>
[Prototype]
public sealed partial class AntagIntroPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    /// <summary>
    /// What scene to play (This is the actual animation).
    /// </summary>
    [DataField(required: true)]
    public string Scene = default!;

    /// <summary>
    /// What track to play. It's recommended you remove the briefing sound if you have this set.
    /// </summary>
    [DataField]
    public SoundSpecifier? Track;

    /// <summary>
    /// How long the server will hold the role back if the animation fails to end before giving it back to the client.
    /// You only need to change this if your animation plays longer than 20 seconds for whatever reason.
    /// </summary>
    [DataField]
    public TimeSpan Timeout = TimeSpan.FromSeconds(20);
}
