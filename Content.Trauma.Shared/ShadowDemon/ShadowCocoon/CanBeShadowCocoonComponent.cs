using Content.Shared.DoAfter;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Trauma.Shared.ShadowDemon.ShadowCocoon;

/// <summary>
/// Marks an entity as able to be made into a shadow cocoon
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class CanBeShadowCocoonComponent : Component;

[Serializable, NetSerializable]
public sealed partial class ShadowCocoonDoAfterEvent : SimpleDoAfterEvent;
