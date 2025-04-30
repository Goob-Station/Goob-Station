// SPDX-FileCopyrightText: 2025 FaDeOkno <logkedr18@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Server.Interaction;
using Content.Server.Jittering;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Goobstation.Server.Obsessed;

public sealed class ObsessedSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly InteractionSystem _interaction = default!;
    [Dependency] private readonly JitteringSystem _jittering = default!;

    public override void Initialize()
    {
        base.Initialize();

    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        var query = EntityQueryEnumerator<ObsessedComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            UpdateSanity(uid, comp);
            UpdateEffects(uid, comp);
        }
    }

    private void UpdateSanity(EntityUid uid, ObsessedComponent comp)
    {
        if (comp.SanityNextUpdate > _timing.CurTime)
            return;

        comp.SanityNextUpdate = _timing.CurTime + TimeSpan.FromSeconds(comp.SanityUpdateInterval);

        comp.Sanity += comp.SanityLoss;

        if (_interaction.InRangeUnobstructed(comp.Target, Transform(uid).Coordinates, 7f))
        {
            comp.Sanity += comp.SanityRecovery[comp.FollowUpdates >= comp.FollowUpdatesToRecovery ? ObsessionInteraction.Follow : ObsessionInteraction.See];
            comp.FollowUpdates = Math.Clamp(comp.FollowUpdates + 1, 0, 12);
        }
        else
            comp.FollowUpdates = Math.Clamp(comp.FollowUpdates - 1, 0, 12);
    }

    private void UpdateEffects(EntityUid uid, ObsessedComponent comp)
    {
        if (comp.EffectNextUpdate > _timing.CurTime)
            return;

        comp.EffectNextUpdate = _timing.CurTime + TimeSpan.FromSeconds(_random.NextFloat(comp.EffectUpdateInterval.Item1, comp.EffectUpdateInterval.Item2));

        List<ObsessionEffect> effects = comp.SanityThresholds
                                          .Where(x => x.Value >= comp.Sanity)
                                          .Select(x => x.Key)
                                          .ToList();

        if (effects.Any())
            DoEffect(uid, comp, _random.Pick(effects));
    }

    private void DoEffect(EntityUid uid, ObsessedComponent comp, ObsessionEffect effect)
    {
        // todo
        switch (effect)
        {
            case ObsessionEffect.Popup:
                break;
            case ObsessionEffect.Shake:
                _jittering.DoJitter(uid, TimeSpan.FromSeconds(1f), true, 6f, 6f);
                break;
            case ObsessionEffect.Sound:
                break;
            case ObsessionEffect.Speech:
                break;
            case ObsessionEffect.Damage:
                break;
        }
    }
}
