using Content.Server.Atmos.Components;
using Content.Server.Atmos.EntitySystems;
using Content.Server.Popups;
using Content.Shared.IdentityManagement;
using Content.Shared.Popups;
using Robust.Shared.Player;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Goobstation.Server.SpecialPassives.SpontaneousCombustion;

public sealed class SpontaneousCombustionSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly FlammableSystem _flammable = default!;
    [Dependency] private readonly PopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SpontaneousCombustionComponent, MapInitEvent>(OnMapInit);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var curTime = _timing.CurTime;

        var query = EntityQueryEnumerator<SpontaneousCombustionComponent, FlammableComponent>();
        while (query.MoveNext(out var uid, out var comp, out var flam))
        {
            if (curTime < comp.NextCombustion)
                continue;

            UpdateTime((uid, comp));
            Combust((uid, comp, flam));
        }
    }

    private void OnMapInit(Entity<SpontaneousCombustionComponent> ent, ref MapInitEvent args)
    {
        UpdateTime(ent);
    }

    private void UpdateTime(Entity<SpontaneousCombustionComponent> ent)
    {
        var range = ent.Comp.CombustionRangeSeconds;
        var time = _random.NextFloat(range.X, range.Y);
        ent.Comp.NextCombustion = _timing.CurTime + TimeSpan.FromSeconds(time);
    }

    private void Combust(Entity<SpontaneousCombustionComponent, FlammableComponent> ent)
    {
        var range = ent.Comp1.FireStackRange;
        var stacks = _random.NextFloat(range.X, range.Y);
        _flammable.AdjustFireStacks(ent, stacks, ent.Comp2, true);
        var identity = Identity.Entity(ent, EntityManager);
        _popup.PopupEntity(Loc.GetString(ent.Comp1.MessageSelf), ent, ent, PopupType.LargeCaution);
        _popup.PopupEntity(Loc.GetString(ent.Comp1.MessageOthers, ("ent", identity)),
            ent,
            Filter.PvsExcept(ent),
            false,
            PopupType.LargeCaution);
    }
}
