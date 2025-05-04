namespace Content.Goobstation.Server.RelayedDeathrattle;

[RegisterComponent]
public sealed partial class RelayedDeathrattleComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite)]
    public EntityUid? Target;

    [DataField]
    public LocId CritMessage = "deathrattle-implant-critical-message";

    [DataField]
    public LocId DeathMessage = "deathrattle-implant-dead-message";

    [DataField]
    public bool RequireCrewMonitor = true;
}
