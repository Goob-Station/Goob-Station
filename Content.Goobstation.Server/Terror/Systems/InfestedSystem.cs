using Content.Goobstation.Shared.Terror.Components;
using Content.Shared.Popups;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Goobstation.Server.Terror.Systems;

public sealed class InfestedSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<InfestedComponent, ComponentShutdown>(OnShutdown);
    }

    private void OnShutdown(EntityUid uid, InfestedComponent comp, ComponentShutdown args)
    {
        _popup.PopupEntity(
            Loc.GetString("spiderling-infested-cured"),
            uid, uid,
            PopupType.SmallCaution);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<InfestedComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (comp.SpawnNumber <= 0)
                comp.SpawnNumber = _random.Next(1, 4);

            comp.Accumulator += TimeSpan.FromSeconds(frameTime);

            if (comp.Accumulator < comp.SpawnInterval)
                continue;

            comp.Accumulator -= comp.SpawnInterval;
            SpawnSpiderlings(uid, comp);
            _audio.PlayPvs(comp.SpawnSound, uid);
            _popup.PopupEntity(
                Loc.GetString("spiderling-infested-spawn"),
                uid, uid,
                PopupType.MediumCaution);
        }
    }

    private void SpawnSpiderlings(EntityUid uid, InfestedComponent comp)
    {
        var coords = Transform(uid).Coordinates;

        for (var i = 0; i < comp.SpawnNumber; i++)
        {
            var proto = _random.Pick(comp.EggsTier1);

            if (_proto.HasIndex<EntityPrototype>(proto))
                Spawn(proto, coords);
        }
    }
}
