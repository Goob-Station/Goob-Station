namespace Content.Goobstation.Server.Obsession;

[RegisterComponent, Access(typeof(ObsessionObjectivesSystem))]
public sealed partial class ObsessionKillConditionComponent : Component
{
    [DataField]
    public string Name = "";

    [DataField]
    public string Desc = "";

    public bool Success = false;
}
