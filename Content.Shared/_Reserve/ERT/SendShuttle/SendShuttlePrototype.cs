using Robust.Shared.Prototypes;

namespace Content.Shared._Reserve.ERT.SendShuttlePrototype;

[Prototype("shuttleType")]
public sealed class SendShuttlePrototype : IPrototype
{
    [ViewVariables]
    [IdDataField]
    public string ID { get; } = default!;


    [DataField]
    public bool DefaultIsAnnounce = true;

    [DataField]
    public bool ForsedAnnounce = false;

    [DataField]
    public Color AnnounceColor = Color.Gold;

    [DataField]
    public bool IsPlayAuidoFromAnnouncement = false;

    [DataField]
    public string AnnouncementText = string.Empty;

    [DataField]
    public string AnnouncerText = "shuttle-send-default-announcer";


    [DataField]
    public bool IsPlayAudio = true;

    [DataField]
    public string AudioPath = "/Audio/_Reserve/announcement/ertyes.ogg";

    [DataField]
    public int Volume = 0;


    [DataField]
    public bool IsLoadGrid = true;

    [DataField]
    public string GridPath = string.Empty;


    [DataField]
    public bool IsSetAlertLevel = false;

    [DataField]
    public string AlertLevelCode = string.Empty;


    [DataField]
    public string HintText = string.Empty;
}
