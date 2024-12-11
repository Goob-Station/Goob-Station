namespace Content.Server.WhiteDream.BloodCult.Objectives;

[RegisterComponent, Access(typeof(KillTargetCultSystem))]
public sealed partial class KillTargetCultComponent : Component
{
    [DataField(required: true), ViewVariables(VVAccess.ReadWrite)]
    public string Title = string.Empty;

    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public EntityUid? Target;
}
