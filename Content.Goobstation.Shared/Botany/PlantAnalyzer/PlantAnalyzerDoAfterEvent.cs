using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Botany.PlantAnalyzer;

[Serializable, NetSerializable]
public sealed partial class PlantAnalyzerDoAfterEvent : SimpleDoAfterEvent;
