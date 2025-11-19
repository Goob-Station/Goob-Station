using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Goobstation.Shared.Wraith.Minions.Harbinger;

[RegisterComponent, NetworkedComponent]
public sealed partial class TentacleHookComponent : Component
{
    [DataField]
    public EntProtoId TentacleProto = "TentacleHook";

    [ViewVariables]
    public EntityUid? Projectile;

    [DataField]
    public SpriteSpecifier HookSprite =
        new SpriteSpecifier.Rsi(new ResPath("_Goobstation/Wraith/Objects/Line/tentacle.rsi"), "end_tentacle");

    [DataField]
    public SpriteSpecifier RopeSprite =
    new SpriteSpecifier.Rsi(new ResPath("_Goobstation/Wraith/Objects/Line/tentacle.rsi"), "mid_tentacle");

    /// <summary>
    /// Sounds to be played whwn hooking.
    /// </summary>
    [DataField]
    public SoundSpecifier? HookSound = new SoundPathSpecifier("/Audio/_Goobstation/Misc/Hastur/tentacle_hit.ogg");
}
