using System.Linq;
using Content.Goobstation.Shared.Wraith.Components;
using Content.Goobstation.Shared.Wraith.Events;
using Content.Server.Ghost;
using Robust.Shared.Random;

namespace Content.Goobstation.Server.Wraith;

public sealed class SpookActionSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SpookComponent, FlipLightsEvent>(OnFlipLights);
    }

    private void OnFlipLights(Entity<SpookComponent> ent, ref FlipLightsEvent args)
    {
        // taken from ghost boo system

        if (args.Handled)
            return;

        var entities = _lookup.GetEntitiesInRange(args.Performer, ent.Comp.FlipLightRadius).ToList();
        _random.Shuffle(entities);

        var booCounter = 0;
        foreach (var entity in entities)
        {
            var ev = new GhostBooEvent();
            RaiseLocalEvent(entity, ev);

            if (ev.Handled)
                booCounter++;

            if (booCounter >= ent.Comp.FlipLightMaxTargets)
                break;
        }

        args.Handled = true;
    }
}
