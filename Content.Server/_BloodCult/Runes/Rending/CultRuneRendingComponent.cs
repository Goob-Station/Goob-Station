using Content.Shared.DoAfter;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Components;
using Robust.Shared.Prototypes;

namespace Content.Server.WhiteDream.BloodCult.Runes.Rending;

[RegisterComponent]
public sealed partial class CultRuneRendingComponent : Component
{
    [DataField]
    public float SummonTime = 40;

    [DataField]
    public SoundSpecifier FinishedDrawingAudio =
        new SoundPathSpecifier("/Audio/WhiteDream/BloodCult/rending_draw_finished.ogg");

    [DataField]
    public SoundSpecifier SummonAudio = new SoundPathSpecifier("/Audio/WhiteDream/BloodCult/rending_ritual.ogg");

    [DataField]
    public EntProtoId NarsiePrototype = "MobNarsieSpawn";

    /// <summary>
    ///     Used to track if the rune is being used right now.
    /// </summary>
    public DoAfterId? CurrentDoAfter;

    /// <summary>
    ///     Used to track the summon audio entity.
    /// </summary>
    public Entity<AudioComponent>? AudioEntity;
}
