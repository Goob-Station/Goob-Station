using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.SlotMachine.CoinFlipper;

[Serializable, NetSerializable]
public sealed partial class CoinFlipperDoAfterEvent : SimpleDoAfterEvent;
