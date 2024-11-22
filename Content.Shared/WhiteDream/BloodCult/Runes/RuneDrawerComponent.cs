using Content.Shared.DoAfter;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared.WhiteDream.BloodCult.Runes;

[RegisterComponent, NetworkedComponent]
public sealed partial class RuneDrawerComponent : Component
{
    [DataField]
    public float EraseTime = 4f;

    [DataField]
    public SoundSpecifier StartDrawingSound = new SoundPathSpecifier("/Audio/WhiteDream/BloodCult/butcher.ogg");

    public SoundSpecifier EndDrawingSound = new SoundPathSpecifier("/Audio/WhiteDream/BloodCult/blood.ogg");
}

[Serializable, NetSerializable]
public enum RuneDrawerBuiKey
{
    Key
}

[Serializable, NetSerializable]
public sealed class RuneDrawerSelectedMessage(RuneSelectorPrototype selectedRune) : BoundUserInterfaceMessage
{
    public ProtoId<RuneSelectorPrototype> SelectedRune { get; private set; } = selectedRune.ID;
}

[Serializable, NetSerializable]
public sealed partial class RuneEraseDoAfterEvent : SimpleDoAfterEvent;

[Serializable, NetSerializable]
public sealed partial class DrawRuneDoAfter : SimpleDoAfterEvent
{
    public ProtoId<RuneSelectorPrototype> Rune;
    public SoundSpecifier EndDrawingSound;
}
