using Content.Shared.Item;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.MantisBlades;

public sealed class SharedMantisBladeSystem : EntitySystem
{
    [Dependency] private readonly SharedItemSystem _item = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MantisBladeComponent, ComponentInit>(OnInit);
    }

    private void OnInit(Entity<MantisBladeComponent> ent, ref ComponentInit args)
    {
        _item.SetHeldPrefix(ent, "popout");

        Timer.Spawn(ent.Comp.VisualDuration,
            () =>
        {
            if (!Deleted(ent))
                _item.SetHeldPrefix(ent, null);
        });
    }
}
