using System.Numerics;
using Content.Shared.Chat.Prototypes;
using Content.Shared.FixedPoint;
using Content.Shared.Speech;
using Content.Shared.StatusEffect;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Shitcode.Heretic.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class ShadowCloakedComponent : Component
{
    [ViewVariables]
    public bool WasVisible = true;

    [DataField]
    public ProtoId<StatusEffectPrototype> Status = "ShadowCloak";

    [DataField]
    public ProtoId<EmoteSoundsPrototype> EmoteSounds = "ShadowCloak";

    [DataField]
    public ProtoId<SpeechSoundsPrototype> SpeechSounds = "ShadowCloak";

    [DataField]
    public ProtoId<SpeechVerbPrototype> SpeechVerb = "Hiss";

    [DataField]
    public EntProtoId ShadowCloakEntity = "ShadowCloakEntity";

    [DataField]
    public SoundSpecifier Sound = new SoundCollectionSpecifier("Curse");

    [DataField]
    public bool DebuffOnEarlyReveal;

    [DataField]
    public Vector2 MoveSpeedModifiers = new(1.25f, 1.25f);

    [DataField]
    public Vector2 EarlyRemoveMoveSpeedModifiers = new(0.75f, 0.75f);

    [DataField]
    public TimeSpan KnockdownTime = TimeSpan.FromSeconds(0.5f);

    [DataField]
    public TimeSpan SlowdownTime = TimeSpan.FromSeconds(10f);

    [DataField]
    public float DoAfterSlowdown = 3f;

    [DataField]
    public FixedPoint2 DamageBeforeReveal = 25;

    [DataField]
    public FixedPoint2 SustainedDamage = 0f;

    [DataField]
    public TimeSpan RevealCooldown = TimeSpan.FromMinutes(1f);

    [DataField]
    public TimeSpan ForceRevealCooldown = TimeSpan.FromMinutes(2f);

    [DataField]
    public FixedPoint2 SustainedDamageReductionRate = 1;
}
