using Robust.Shared.Prototypes;

namespace Content.Shared.Heretic;

public abstract partial class SharedHereticRitualSystem : EntitySystem
{
    [Dependency] protected readonly IPrototypeManager _proto = default!;

    public HereticRitualPrototype GetRitual(ProtoId<HereticRitualPrototype> id)
        => _proto.Index(id);

    public bool TryDoRitual(EntityUid performer, EntityUid platform, ProtoId<HereticRitualPrototype> ritualId)
    {
        var rit = GetRitual(ritualId);

        if (rit.Requirements != null && rit.Requirements.Count > 0)
        {

        }
        if (rit.Output != null && rit.Output.Count > 0)
        {

        }
        if (rit.OutputEvent != null)
            RaiseLocalEvent(rit.OutputEvent);

        return true;
    }
}
