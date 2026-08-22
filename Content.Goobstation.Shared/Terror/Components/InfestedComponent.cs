using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Terror.Components;

[RegisterComponent]
public sealed partial class InfestedComponent : Component
{
    /// <summary>
    /// How often spiderlings burst out.
    /// </summary>
    [DataField]
    public TimeSpan SpawnInterval = TimeSpan.FromSeconds(89);

    public int SpawnNumber;

    [DataField]
    public SoundSpecifier SpawnSound = new SoundPathSpecifier("/Audio/_Goobstation/Heretic/hiss/lowHiss3.ogg");

    [DataField]
    public List<EntProtoId> EggsTier1 = new()
    {
        "SpiderlingRed",
        "SpiderlingGray",
        "SpiderlingGreen"
    };

    public TimeSpan Accumulator = TimeSpan.Zero;
}
