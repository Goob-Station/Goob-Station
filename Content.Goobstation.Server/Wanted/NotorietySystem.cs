// SPDX-FileCopyrightText: 2026 Goob-Station Contributors <https://github.com/Goob-Station/Goob-Station>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Common.Wanted;
using Content.Shared.CriminalRecords.Systems;
using Content.Shared.Security;
using Robust.Shared.Timing;

namespace Content.Goobstation.Server.Wanted;

/// <summary>
/// Manages the notoriety system — tracks escalating criminal history on entities,
/// decays notoriety over time when criminals lay low, and provides bounty information
/// to other systems (e.g. the manhunt station event).
/// </summary>
public sealed class NotorietySystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;

    /// <summary>
    /// How long a criminal must stay out of trouble before losing one notoriety level.
    /// A level-5 criminal takes 5 × this interval (75 min) to fully clear their record.
    /// </summary>
    private static readonly TimeSpan DecayInterval = TimeSpan.FromMinutes(15);

    /// <summary>
    /// How often the decay pass runs. Keeps the Update loop cheap.
    /// </summary>
    private static readonly TimeSpan DecayCheckInterval = TimeSpan.FromMinutes(1);

    private TimeSpan _nextDecayCheck = TimeSpan.Zero;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<CriminalStatusUpdatedEvent>(OnStatusUpdated);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (_timing.CurTime < _nextDecayCheck)
            return;

        _nextDecayCheck = _timing.CurTime + DecayCheckInterval;
        DecayAll();
    }

    // ── Event handlers ────────────────────────────────────────────────────────

    private void OnStatusUpdated(CriminalStatusUpdatedEvent args)
    {
        // Only escalate for serious criminal statuses; clearing resets nothing.
        if (args.Status is not (SecurityStatus.Wanted or SecurityStatus.Dangerous or SecurityStatus.Perma))
            return;

        if (!IsValid(args.Entity))
            return;

        var comp = EnsureComp<NotorietyComponent>(args.Entity);

        if (comp.Level >= NotorietyComponent.MaxLevel)
            return;

        comp.Level++;
        comp.LastEscalationTime = _timing.CurTime;
        comp.TotalEscalations++;
        Dirty(args.Entity, comp);
    }

    // ── Decay ────────────────────────────────────────────────────────────────

    private void DecayAll()
    {
        var query = EntityQueryEnumerator<NotorietyComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (comp.Level <= 0)
                continue;

            var timeSinceLastEscalation = _timing.CurTime - comp.LastEscalationTime;
            if (timeSinceLastEscalation < DecayInterval)
                continue;

            comp.Level--;
            // Reset the timer so the next level also takes a full interval to decay.
            comp.LastEscalationTime = _timing.CurTime;
            Dirty(uid, comp);
        }
    }

    // ── Public API ───────────────────────────────────────────────────────────

    /// <summary>
    /// Returns the current notoriety level of an entity, or 0 if none exists.
    /// </summary>
    public int GetLevel(EntityUid uid)
    {
        return TryComp<NotorietyComponent>(uid, out var comp) ? comp.Level : 0;
    }

    /// <summary>
    /// Returns the credit bounty for the entity's current notoriety level.
    /// </summary>
    public int GetBounty(EntityUid uid)
    {
        return TryComp<NotorietyComponent>(uid, out var comp) ? comp.BountyAmount : 0;
    }
}
