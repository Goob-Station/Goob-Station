using Content.Goobstation.Client.News.Conditions;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Goobstation.Client.News;

[Prototype("newsEntry")]
public sealed partial class NewsEntryPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField(required: true)]
    public LocId Title;

    [DataField]
    public LocId? Summary;

    [DataField]
    public DateTime? Date;

    [DataField(required: true)]
    public ResPath Document;

    [DataField]
    public ResPath? Card;

    [DataField]
    public List<NewsCondition> Conditions = new();

    [DataField]
    public NewsPopup? Popup;
}

[DataDefinition]
public sealed partial class NewsPopup
{
    [DataField]
    public NewsPopupFrequency Frequency = NewsPopupFrequency.Once;

    [DataField]
    public int Days = 7;

    [DataField]
    public bool AllowNeverShow;

    [DataField]
    public List<NewsCondition> Conditions = new();
}

public enum NewsPopupFrequency : byte
{
    Once,
    EveryConnect,
    Days,
}
