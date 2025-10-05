using System.Linq;
using Content.Goobstation.Common.Medical;
using Content.Goobstation.Server.Shizophrenia;
using Content.Server.Actions;
using Content.Server.Body.Systems;
using Content.Server.Humanoid;
using Content.Shared.Body.Events;
using Content.Shared.Body.Organ;
using Content.Shared.Body.Part;
using Content.Shared.Inventory;
using Robust.Server.GameStates;
using Robust.Server.Player;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Goobstation.Server.Cyberpsychosis;

public sealed partial class CyberSanitySystem : EntitySystem
{
    [Dependency] private readonly BodySystem _body = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly SchizophreniaSystem _shizophrenia = default!;
    [Dependency] private readonly HumanoidAppearanceSystem _humanoid = default!;
    [Dependency] private readonly InventorySystem _inventory = default!;
    [Dependency] private readonly PvsOverrideSystem _pvsOverride = default!;
    [Dependency] private readonly MetaDataSystem _meta = default!;

    public override void Initialize()
    {
        base.Initialize();
        UpdatesBefore.Add(typeof(ActionsSystem));

        InitializeGain();

        SubscribeLocalEvent<CyberSanityComponent, MapInitEvent>(OnMapInit);
    }

    private void OnMapInit(EntityUid uid, CyberSanityComponent comp, MapInitEvent args)
    {

    }

    private void UpdateSanity(EntityUid uid, CyberSanityComponent comp)
    {
        if (comp.NextGain > _timing.CurTime)
            return;

        comp.NextGain = _timing.CurTime + TimeSpan.FromSeconds(1);
        GainSanity(uid, comp);
    }

    private void UpdateEffects(EntityUid uid, CyberSanityComponent comp)
    {
        if (comp.NextEffect > _timing.CurTime)
            return;

        if (comp.Sanity > comp.EffectThresholds.Keys.Max())
            return;

        comp.NextEffect = _timing.CurTime + TimeSpan.FromSeconds(_random.NextFloat(7f, 40f));

        var effects = comp.EffectThresholds.Where(x => x.Key >= comp.Sanity).SelectMany(x => x.Value).ToList();

        if (effects.Count <= 0)
            return;

        _random.Pick(effects).Effect(new(uid, EntityManager));
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var sanityQuery = EntityQueryEnumerator<CyberSanityComponent>();
        while (sanityQuery.MoveNext(out var uid, out var comp))
        {
            UpdateSanity(uid, comp);
            UpdateEffects(uid, comp);
        }
    }
}

