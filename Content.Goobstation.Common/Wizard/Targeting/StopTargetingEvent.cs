using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Common.Wizard.Targeting;

[Serializable, NetSerializable]
public sealed class StopTargetingEvent() : EntityEventArgs;
