using Robust.Shared.Serialization;

namespace Content.Shared.StationReport;

[Serializable, NetSerializable]
public sealed class StationReportEvent : EntityEventArgs
{
    // Make it nullable
    public string? StationReportText { get; }

    // Parameter must be nullable
    public StationReportEvent(string? text)
    {
        StationReportText = text;
    }
}
