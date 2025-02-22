using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared._Lavaland.Shelter;

[Serializable, NetSerializable]
public sealed partial class ShelterCapsuleDeployDoAfterEvent : SimpleDoAfterEvent;
