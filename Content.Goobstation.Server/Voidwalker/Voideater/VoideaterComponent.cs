using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.Voidwalker.Voideater;

[RegisterComponent]
public sealed partial class VoideaterComponent : Component
{

    [DataField]
    public TimeSpan SleepDuration = TimeSpan.FromSeconds(15);

    [DataField]
    public EntProtoId SleepingEffectProto = "StatusEffectForcedSleeping";
}
