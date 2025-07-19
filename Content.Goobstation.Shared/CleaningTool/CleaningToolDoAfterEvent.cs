using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.CleaningTool;

[Serializable, NetSerializable]
public sealed partial class CleaningToolDoAfterEvent : DoAfterEvent
{
    [DataField(required: true)]
    public List<NetEntity> Entities = default!;

    private CleaningToolDoAfterEvent()
    {
    }

    public CleaningToolDoAfterEvent(List<NetEntity> entities)
    {
        Entities = entities;
    }

    public override DoAfterEvent Clone() => this;
}
