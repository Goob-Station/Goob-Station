using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Common.Zombies;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentPause]
public sealed partial class SiliconZombieComponent : Component
{
    [DataField]
    public TimeSpan PendingSparkInterval = TimeSpan.FromSeconds(15);

    [DataField, AutoPausedField]
    public TimeSpan NextSpark;

    [DataField]
    public EntProtoId SparkEffect = "EffectSparks";

    [DataField]
    public SoundSpecifier SparkSound = new SoundCollectionSpecifier("sparks");
}
