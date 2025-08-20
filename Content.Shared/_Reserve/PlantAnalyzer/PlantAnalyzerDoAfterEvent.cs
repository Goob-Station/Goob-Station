using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared._Reserve.PlantAnalyzer;

[Serializable, NetSerializable]
public sealed partial class PlantAnalyzerDoAfterEvent : SimpleDoAfterEvent
{
}