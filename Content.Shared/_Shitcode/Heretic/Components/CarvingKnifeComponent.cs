using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Content.Shared.Heretic.Prototypes;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._Goobstation.Heretic.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class CarvingKnifeComponent : Component
{
    [DataField]
    public HashSet<ProtoId<RuneCarvingPrototype>> Carvings = new();

    [DataField(serverOnly: true)]
    public List<EntityUid> DrawnRunes = new();

    [DataField]
    public int MaxRuneAmount = 3;

    [DataField]
    public TimeSpan RuneDrawTime = TimeSpan.FromSeconds(5f);

    [DataField]
    public SoundSpecifier Sound = new SoundPathSpecifier("/Audio/_Goobstation/Heretic/sheath.ogg");

    [DataField]
    public EntProtoId RunebreakAction = "ActionRunebreak";

    [DataField]
    public EntityUid? RunebreakActionEntity;
}

[Serializable, NetSerializable]
public sealed class RuneCarvingSelectedMessage(ProtoId<RuneCarvingPrototype> protoId) : BoundUserInterfaceMessage
{
    public ProtoId<RuneCarvingPrototype> ProtoId { get; } = protoId;
}

[Serializable, NetSerializable]
public enum CarvingKnifeUiKey : byte
{
    Key
}

[Serializable, NetSerializable]
public sealed partial class CarveRuneDoAfterEvent(ProtoId<RuneCarvingPrototype> carving) : DoAfterEvent
{
    public ProtoId<RuneCarvingPrototype> Carving = carving;

    public CarveRuneDoAfterEvent() : this(default) { }

    public override DoAfterEvent Clone() => this;
}

public sealed partial class DeleteAllCarvingsEvent : InstantActionEvent;
