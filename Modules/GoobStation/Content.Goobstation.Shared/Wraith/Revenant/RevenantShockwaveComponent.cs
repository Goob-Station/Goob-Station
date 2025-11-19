using Content.Shared.Damage;
using Content.Shared.Tag;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Wraith.Revenant;

[RegisterComponent, NetworkedComponent]
public sealed partial class RevenantShockwaveComponent : Component
{
    [DataField]
    public SoundSpecifier? ShockSound = new SoundPathSpecifier("/Audio/_Goobstation/Wraith/revshock.ogg");

    /// <summary>
    ///  Search range of shockwave
    /// </summary>
    [DataField]
    public float SearchRange = 8f;

    /// <summary>
    ///  How many tiles to pry
    /// </summary>
    [DataField]
    public float TilesToPry = 10;

    /// <summary>
    /// How long to knockdown people
    /// </summary>
    [DataField]
    public TimeSpan KnockdownDuration = TimeSpan.FromSeconds(10f);

    [ViewVariables]
    public ProtoId<TagPrototype> WindowTag = "Window";

    [ViewVariables]
    public ProtoId<TagPrototype> WallTag = "Wall";

    /// <summary>
    /// Damage dealt to windows and walls
    /// </summary>
    [DataField]
    public DamageSpecifier? StructureDamage = new();
}
