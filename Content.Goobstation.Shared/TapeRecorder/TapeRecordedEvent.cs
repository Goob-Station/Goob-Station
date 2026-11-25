namespace Content.Goobstation.Shared.TapeRecorder;

[ByRefEvent]
public readonly record struct TapeRecordedEvent(EntityUid Recorder);
