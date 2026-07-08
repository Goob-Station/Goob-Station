// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.StepTrigger.Systems;
using Content.Shared.EntityEffects;

namespace Content.Server.Tiles;

public sealed class TileEntityEffectSystem : EntitySystem
{
    [Dependency] private readonly SharedEntityEffectSystem _effect = default!; // goob edit - use system instead

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<TileEntityEffectComponent, StepTriggeredOffEvent>(OnTileStepTriggered);
        SubscribeLocalEvent<TileEntityEffectComponent, StepTriggerAttemptEvent>(OnTileStepTriggerAttempt);
    }

    private void OnTileStepTriggerAttempt(Entity<TileEntityEffectComponent> ent, ref StepTriggerAttemptEvent args)
    {
        args.Continue = true;
    }

    private void OnTileStepTriggered(Entity<TileEntityEffectComponent> ent, ref StepTriggeredOffEvent args)
    {
        var otherUid = args.Tripper;
        var effectArgs = new EntityEffectBaseArgs(otherUid, EntityManager);

        foreach (var effect in ent.Comp.Effects)
        {
            _effect.Effect(effect, effectArgs); // goob edit - use system instead
        }
    }
}
