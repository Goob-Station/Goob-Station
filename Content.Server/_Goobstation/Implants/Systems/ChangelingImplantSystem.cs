using Content.Server._Goobstation.Implants.Components;
using Content.Shared.Changeling;
using Content.Shared.Implants;

namespace Content.Server._Goobstation.Implants.Systems;

public sealed class ChangelingImplantSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ChangelingImplantComponent, ImplantImplantedEvent>(OnImplanted);
    }

    public void OnImplanted(EntityUid uid, ChangelingImplantComponent comp, ref ImplantImplantedEvent ev)
    {
        if (ev.Implanted.HasValue)
            EnsureComp<ChangelingComponent>(ev.Implanted.Value);
    }
}
