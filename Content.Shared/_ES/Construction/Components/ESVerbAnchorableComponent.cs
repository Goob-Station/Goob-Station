using Content.Shared.DoAfter;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared._ES.Construction.Components;

/// <summary>
/// Used to allow an object to be anchored/unanchored via a verb.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class ESVerbAnchorableComponent : Component
{
    [DataField]
    public SoundSpecifier? AnchorSound = new SoundPathSpecifier("/Audio/Items/ratchet.ogg");
}

[Serializable, NetSerializable]
public sealed partial class ESToggleAnchorDoAfterEvent : SimpleDoAfterEvent;
