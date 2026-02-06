using Content.Shared._Shitcode.Heretic.Systems;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Utility;

namespace Content.Shared._Shitcode.Heretic.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class SacramentsOfPowerComponent : Component
{
    [DataField, AutoNetworkedField]
    public SacramentsState State = SacramentsState.Opening;

    [DataField]
    public TimeSpan StateUpdateAt;

    [DataField]
    public float DamageReturnRatio = 0.65f;

    [DataField]
    public float StaminaDamageReturnRatio = 1f;

    [DataField]
    public SoundSpecifier Sound = new SoundPathSpecifier("/Audio/_Goobstation/Heretic/ark_deathrattle.ogg");

    [DataField]
    public SoundSpecifier ActivationSound = new SoundPathSpecifier("/Audio/_Goobstation/Heretic/piano_hit.ogg");

    [DataField]
    public TimeSpan ActivationTime = TimeSpan.FromSeconds(0.8);

    [DataField]
    public TimeSpan DeactivationTime = TimeSpan.FromSeconds(0.9);

    [DataField]
    public TimeSpan EffectTime = TimeSpan.FromSeconds(5);

    [DataField]
    public ResPath SpritePath = new("_Goobstation/Heretic/Effects/effects.rsi");

    [DataField]
    public Dictionary<SacramentsState, string> SpriteStates = new()
    {
        { SacramentsState.Opening, "eye_open" },
        { SacramentsState.Open, "eye_pulse" },
        { SacramentsState.Closing, "eye_close" },
    };
}
