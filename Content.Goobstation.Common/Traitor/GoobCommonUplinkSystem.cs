using Robust.Shared.Prototypes;

namespace Content.Goobstation.Common.Traitor;

public abstract class GoobCommonUplinkSystem : EntitySystem
{
    public abstract ProtoId<UplinkPreferencePrototype> GetUplinkPreference(EntityUid mindEnt);
    public abstract EntityUid? FindUplinkTarget(EntityUid user, string[] searchComponents);
}
