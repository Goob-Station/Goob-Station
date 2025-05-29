using Content.Shared.Clothing.EntitySystems;
using Content.Shared.Item;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.ChangePrefixAfterDelay;

public sealed class ChangePrefixAfterDelaySystem : EntitySystem
{
    [Dependency] private readonly ClothingSystem _clothing = default!;
    [Dependency] private readonly SharedItemSystem _item = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ChangePrefixAfterDelayComponent, ComponentInit>(OnInit);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<ChangePrefixAfterDelayComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (comp.ChangeAt == null || comp.ChangeAt > _timing.CurTime)
                continue;

            _clothing.SetEquippedPrefix(uid, comp.NewEquippedPrefix);
            _item.SetHeldPrefix(uid, comp.NewHeldPrefix);
            RemComp(uid, comp);
        }
    }

    private void OnInit(Entity<ChangePrefixAfterDelayComponent> ent, ref ComponentInit args)
    {
        ent.Comp.ChangeAt = _timing.CurTime + ent.Comp.Delay;
    }
}
