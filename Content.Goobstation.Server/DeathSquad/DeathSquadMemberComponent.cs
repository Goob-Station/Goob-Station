using Content.Shared.FixedPoint;

namespace Content.Goobstation.Server.DeathSquad;

[RegisterComponent]
public sealed partial class DeathSquadMemberComponent : Component
{
    /// <summary>
    /// The amount added to this entities critical and dead states.
    /// </summary>
    [DataField]
    public FixedPoint2 NewHealth = 250;
}
