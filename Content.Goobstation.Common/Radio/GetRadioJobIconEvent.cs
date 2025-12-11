namespace Content.Goobstation.Common.Radio;

/// <summary>
/// Raised when a radio message is sent to try to get a job icon and job name for the sender, to display next to their name in the chatbox.
/// </summary>
/// <param name="JobIconID">ProtoID of the sender's job icon. (Can be null)</param>
/// <param name="JobName">String of the sender's job title. (Can be null)</param>
[ByRefEvent]
public record struct GetRadioJobIconEvent
{
    public string? JobIconID { get; set; }
    public string? JobName { get; set; }
}
