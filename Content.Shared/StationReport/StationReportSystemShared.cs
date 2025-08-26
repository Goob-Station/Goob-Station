using Robust.Shared.Serialization;

namespace Content.Shared.StationReport;

[Serializable, NetSerializable]
public sealed class StationReportEvent : EntityEventArgs
{
    //Makes it nullable
    public string? StationReportText { get; }
    public StationReportEvent(string? text)
    {
        StationReportText = text;
    }
}
