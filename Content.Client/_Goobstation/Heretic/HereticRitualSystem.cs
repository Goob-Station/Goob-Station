using Content.Shared._Goobstation.Heretic.Components;
using Content.Shared.Heretic;

namespace Content.Client._Goobstation.Heretic;

public sealed class HereticRitualSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeNetworkEvent<HereticRitualsUpdateMessage>(OnRitualsUpdateMessage);
    }

    private void OnRitualsUpdateMessage(HereticRitualsUpdateMessage ev)
    {
        var entity = GetEntity(ev.Entity);

        if (!TryComp<HereticComponent>(entity, out var heretic))
            return;

        heretic.KnownRituals = ev.KnownRituals;
    }
}
