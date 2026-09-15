using Robust.Shared.Serialization;

namespace Content.Goobstation.Common.Wizard.Events;

[Serializable, NetSerializable]
public sealed class StopTargetingEvent : EntityEventArgs;