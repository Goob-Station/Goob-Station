using Content.Shared._Shitmed.Autodoc.Components;
using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;

namespace Content.Shared._Shitmed.Autodoc.Systems;

public sealed class HandsFillSystem : EntitySystem
{
    [Dependency] private readonly SharedHandsSystem _hands = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HandsFillComponent, MapInitEvent>(OnMapInit);
    }

    private void OnMapInit(Entity<HandsFillComponent> ent, ref MapInitEvent args)
    {
        if (!TryComp<HandsComponent>(ent, out var hands))
            return;

        var coords = Transform(ent).Coordinates;
        foreach (var (name, fill) in ent.Comp.Hands)
        {
            _hands.AddHand(ent, name, HandLocation.Middle, hands);

            if (fill is not {} id)
                continue;

            var uid = Spawn(id, coords);
            if (!_hands.TryPickup(ent, uid, name, animate: false, handsComp: hands))
            {
                Log.Error($"Entity {ToPrettyString(ent)} couldn't pick up item {id} into its '{name}' hand!");
                Del(uid);
            }
        }
    }
}
