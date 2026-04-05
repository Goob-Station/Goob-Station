// SPDX-FileCopyrightText: 2026 Goob-Station Contributors <https://github.com/Goob-Station/Goob-Station>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Common.Wanted;
using Content.Shared.CriminalRecords.Systems;
using Content.Shared.Security;
using Robust.Shared.Timing;

namespace Content.Goobstation.Server.Wanted;

/// <summary>
/// Manages the notoriety system — tracks escalating criminal history on entities and provides
/// bounty information to other systems.
/// </summary>
public sealed class NotorietySystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<CriminalStatusUpdatedEvent>(OnStatusUpdated);
    }

    private void OnStatusUpdated(CriminalStatusUpdatedEvent args)
    {
        // Only escalate when assigned a serious criminal status, not when cleared.
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