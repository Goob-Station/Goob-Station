using System.Numerics;
using Content.Shared.Damage;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared._Shitcode.Heretic.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class StarGazeComponent : Component
{
    [DataField]
    public Vector2 Slowdown = new(0.1f, 0.1f);

    [DataField]
    public int LastStage = -1;

    [DataField]
    public float ScreamProb = 0.05f;

    [DataField]
    public float MaxThrowLength = 0.05f;

    [DataField]
    public float ThrowSpeed = 5f;

    [DataField]
    public SoundSpecifier ObliterateSound = new SoundPathSpecifier("/Audio/_Goobstation/Wizard/supermatter.ogg");

    [DataField]
    public EntProtoId AshProto = "Ash";

    [DataField]
    public DamageSpecifier Damage = new()
    {
        DamageDict =
        {
            { "Heat", 5 },
        },
    };

    [DataField]
    public float BeamScale = 2f;

    [DataField]
    public float LaserThickness = 0.9f;

    [DataField]
    public float GravityPullSizeModifier = 2f;

    [DataField]
    public Vector2 MinMaxLaserRange = new(4f, 14f);

    [DataField]
    public float LaserSpeed = 0.05f;

    [ViewVariables, AutoNetworkedField]
    public MapCoordinates? CursorPosition;

    [DataField, AutoNetworkedField]
    public EntityUid? Endpoint;

    [DataField]
    public SoundSpecifier BeamSound = new SoundPathSpecifier("/Audio/_Goobstation/Heretic/stargazer/beam_loop_one.ogg");

    [DataField, NonSerialized]
    public EntityUid? BeamSoundEnt;

    [DataField]
    public float LaserDuration = 10f;

    [ViewVariables(VVAccess.ReadWrite)]
    public float Accumulator;

    [DataField]
    public float DamageInterval = 0.1f;

    [ViewVariables(VVAccess.ReadWrite)]
    public float DamageAccumulator;

    [DataField]
    public float UpdateInterval = 0.01f;

    [DataField]
    public float TimeSinceBeamCreation;

    [DataField]
    public float Duration = 10f;

    [DataField]
    public bool StartedBlasting;

    [DataField]
    public SpriteSpecifier Beam1 =
        new SpriteSpecifier.Rsi(new ResPath("/Textures/_Goobstation/Heretic/Effects/effects.rsi"), "beam1");

    [DataField]
    public SpriteSpecifier Beam2 =
        new SpriteSpecifier.Rsi(new ResPath("/Textures/_Goobstation/Heretic/Effects/effects.rsi"), "beam2");

    [DataField]
    public SpriteSpecifier Beam3 =
        new SpriteSpecifier.Rsi(new ResPath("/Textures/_Goobstation/Heretic/Effects/effects.rsi"), "beam3");

    [DataField]
    public SpriteSpecifier Start1 =
        new SpriteSpecifier.Rsi(new ResPath("/Textures/_Goobstation/Heretic/Effects/effects96x96.rsi"), "start1");

    [DataField]
    public SpriteSpecifier End1 =
        new SpriteSpecifier.Rsi(new ResPath("/Textures/_Goobstation/Heretic/Effects/effects.rsi"), "end1");

    [DataField]
    public SpriteSpecifier Start2 =
        new SpriteSpecifier.Rsi(new ResPath("/Textures/_Goobstation/Heretic/Effects/effects96x96.rsi"), "start2");

    [DataField]
    public SpriteSpecifier End2 =
        new SpriteSpecifier.Rsi(new ResPath("/Textures/_Goobstation/Heretic/Effects/effects.rsi"), "end2");

    [DataField]
    public SpriteSpecifier Start3 =
        new SpriteSpecifier.Rsi(new ResPath("/Textures/_Goobstation/Heretic/Effects/effects96x96.rsi"), "start3");

    [DataField]
    public SpriteSpecifier End3 =
        new SpriteSpecifier.Rsi(new ResPath("/Textures/_Goobstation/Heretic/Effects/effects.rsi"), "end3");
}
