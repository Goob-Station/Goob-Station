using Content.Goobstation.Shared.Terror.Components;
using Content.Shared.Popups;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Goobstation.Server.Terror.Systems;

public sealed class InfestedSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;

    // This is literally TimedSpawner but for Terror spider
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<InfestedComponent, ComponentStartup>(OnStartup);
    }

    private void OnStartup(EntityUid uid, InfestedComponent comp, ComponentStartup args)
    {
        // Pick random spawn count between 1 and 3 if not set
        if (comp.SpawnNumber <= 0)
            comp.SpawnNumber = _random.Next(1, 4);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<InfestedComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            var delta = TimeSpan.FromSeconds(frameTime);

            comp.Accumulator += delta;
            comp.CureAccumulator += delta;

            if (comp.CureAccumulator >= comp.TimeToCure)
            {
                _popup.PopupPredicted("Seems like the spiderlings are all gone.", uid, uid, PopupType.SmallCaution);
                RemComp<InfestedComponent>(uid);
                continue;
            }

            if (comp.Accumulator >= comp.Timer)
            {
                comp.Accumulator -= comp.Timer;
                SpawnSpiderlings(uid, comp);
                _audio.PlayPredicted(comp.SpawnSound, uid, uid);
                _popup.PopupPredicted("Suddenly, spiderlings spawn all over you!", uid, uid, PopupType.MediumCaution);
            }
        }
    }

    private void SpawnSpiderlings(EntityUid uid, InfestedComponent comp)
    {
        if (!TryComp(uid, out TransformComponent? xform))
            return;

        var coords = xform.Coordinates;

        for (var i = 0; i < comp.SpawnNumber; i++)
        {
            var proto = _random.Pick(comp.EggsTier1);

            if (!_proto.HasIndex<EntityPrototype>(proto))
                continue;

            Spawn(proto, coords);
        }
    }
}
