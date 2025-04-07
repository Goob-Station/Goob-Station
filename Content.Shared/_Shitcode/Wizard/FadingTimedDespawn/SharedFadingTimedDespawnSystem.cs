// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Spawners;
using Robust.Shared.Timing;

namespace Content.Shared._Goobstation.Wizard.FadingTimedDespawn;

/// <summary>
/// This is a copy of SharedTimedDespawnSystem with some modifications
/// </summary>
public abstract class SharedFadingTimedDespawnSystem : EntitySystem
{
    [Dependency] protected readonly IGameTiming Timing = default!;

    private readonly HashSet<EntityUid> _queuedDespawnEntities = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<FadingTimedDespawnComponent, AfterAutoHandleStateEvent>(OnAfterAutoHandleState);

        UpdatesOutsidePrediction = true;
    }

    private void OnAfterAutoHandleState(Entity<FadingTimedDespawnComponent> ent, ref AfterAutoHandleStateEvent args)
    {
        if (ent.Comp.FadeOutStarted)
            FadeOut(ent);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (!Timing.IsFirstTimePredicted)
            return;

        _queuedDespawnEntities.Clear();

        var query = EntityQueryEnumerator<FadingTimedDespawnComponent>();

        while (query.MoveNext(out var uid, out var comp))
        {
            if (!CanDelete(uid))
                continue;

            comp.Lifetime -= frameTime;

            if (comp.Lifetime > 0f)
                continue;

            if (comp.FadeOutTime <= 0f)
            {
                _queuedDespawnEntities.Add(uid);
                continue;
            }

            if (!comp.FadeOutStarted)
            {
                comp.FadeOutStarted = true;
                comp.Lifetime += comp.FadeOutTime;
                FadeOut((uid, comp));
                Dirty(uid, comp);
                continue;
            }

            _queuedDespawnEntities.Add(uid);
        }

        foreach (var queued in _queuedDespawnEntities)
        {
            var ev = new TimedDespawnEvent();
            RaiseLocalEvent(queued, ref ev);
            QueueDel(queued);
        }
    }

    protected virtual void FadeOut(Entity<FadingTimedDespawnComponent> ent)
    {
    }

    protected abstract bool CanDelete(EntityUid uid);
}