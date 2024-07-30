using Robust.Shared.Prototypes;

namespace Content.Shared.Heretic;

public abstract partial class SharedHereticRitualSystem : EntitySystem
{

    public bool TryDoRitual(EntityUid performer, EntityUid platform, ProtoId<HereticRitualPrototype> ritualId)
    {

        return true;
    }
}
