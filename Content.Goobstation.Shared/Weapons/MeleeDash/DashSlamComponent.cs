using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Weapons.MeleeDash;

/// <summary>
/// When the holder lands from a melee dash with this item, everyone nearby is knocked down.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class DashSlamComponent : Component
{
    /// <summary>
    /// Radius of the slam on landing.
    /// </summary>
    [DataField]
    public float Range = 2.5f;

    /// <summary>
    /// How long to knock people down for.
    /// </summary>
    [DataField]
    public TimeSpan KnockdownTime = TimeSpan.FromSeconds(3);

    [DataField]
    public SoundSpecifier? SlamSound;
}

[ByRefEvent]
public readonly record struct DashLandedEvent(EntityUid User);
