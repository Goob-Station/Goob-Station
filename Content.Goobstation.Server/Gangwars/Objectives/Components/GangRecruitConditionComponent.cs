namespace Content.Goobstation.Server.Gangwars.Objectives.Components;

/// <summary>
/// Recruit 3 members to your gang.
/// </summary>
[RegisterComponent]
public sealed partial class GangLeaderRecruitConditionComponent : Component
{
    [DataField]
    public int Recruited;
}
