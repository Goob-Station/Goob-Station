using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Keyring;

[Serializable, NetSerializable]
public sealed partial class KeyringDoAfterEvent : SimpleDoAfterEvent;
