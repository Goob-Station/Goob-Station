// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Content.Goobstation.Shared.Xenobiology.XenobiologyBountyConsole;
using Content.Server.NameIdentifier;
using Content.Shared.Cargo;
using Content.Shared.IdentityManagement;
using Content.Shared.NameIdentifier;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Goobstation.Server.Xenobiology.XenobiologyBountyConsole;

public sealed class StationXenobiologyBountyDatabaseSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly NameIdentifierSystem _nameIdentifier = default!;
    [Dependency] private readonly XenobiologyBountyConsoleSystem _xenoConsole = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    [ValidatePrototypeId<NameIdentifierGroupPrototype>]
    private const string BountyNameIdentifierGroup = "Bounty";

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<StationXenobiologyBountyDatabaseComponent, MapInitEvent>(OnMapInit);
    }

    private void OnMapInit(Entity<StationXenobiologyBountyDatabaseComponent> database, ref MapInitEvent args) =>
        FillBountyDatabase(database!);

    public override void Update(float frametime)
    {
        base.Update(frametime);

        var query = EntityQueryEnumerator<StationXenobiologyBountyDatabaseComponent>();
        while (query.MoveNext(out var uid, out var db))
        {
            if (db.NextGlobalMarketRefresh > _timing.CurTime)
                continue;

            RerollBountyDatabase((uid, db));
            ShiftAllTowardsDefaultPointMultiplier(db);

            db.NextGlobalMarketRefresh = _timing.CurTime + db.GlobalMarketRefreshDelay;
        }
    }
    public void FillBountyDatabase(Entity<StationXenobiologyBountyDatabaseComponent?> database)
    {
        if (!Resolve(database, ref database.Comp))
            return;

        var bounties = _proto.EnumeratePrototypes<XenobiologyBountyPrototype>();
        foreach (var bounty in bounties)
            TryAddBounty(database, bounty);

        SortBounties(database.Comp);
        RandomizeAllBountyPointMultipliers(database.Comp);
        _xenoConsole.UpdateBountyConsoles();
    }

    public void RerollBountyDatabase(Entity<StationXenobiologyBountyDatabaseComponent?> entity)
    {
        if (!Resolve(entity, ref entity.Comp))
            return;

        entity.Comp.Bounties.Clear();
        FillBountyDatabase(entity);
    }

    [PublicAPI]
    public bool TryAddBounty(EntityUid uid, string bountyId, StationXenobiologyBountyDatabaseComponent? component = null)
    {
        return _proto.TryIndex<XenobiologyBountyPrototype>(bountyId, out var bounty) && TryAddBounty(uid, bounty, component);
    }

    public bool TryAddBounty(EntityUid uid, XenobiologyBountyPrototype bounty, StationXenobiologyBountyDatabaseComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return false;

        _nameIdentifier.GenerateUniqueName(uid, BountyNameIdentifierGroup, out var randomVal);
        var newBounty = new XenobiologyBountyData(bounty, randomVal);

        if (component.Bounties.Any(bountyData => bountyData.Id == newBounty.Id))
        {
            Log.Error("Failed to add bounty {ID} because another one with the same ID already existed!", newBounty.Id);
            return false;
        }

        newBounty.InitialMultiplier = newBounty.CurrentMultiplier;
        component.Bounties.Add(newBounty);
        return true;
    }

    [PublicAPI]
    public bool TryRemoveBounty(Entity<StationXenobiologyBountyDatabaseComponent?> ent,
        string dataId,
        bool skipped,
        EntityUid? actor = null)
    {
        return TryGetBountyFromId(ent.Owner, dataId, out var data, ent.Comp) && TryRemoveBounty(ent, data, skipped, actor);
    }

    [PublicAPI]
    public bool TryRemoveBounty(Entity<StationXenobiologyBountyDatabaseComponent?> ent,
        XenobiologyBountyData data,
        bool skipped,
        EntityUid? actor = null)
    {
        if (!Resolve(ent, ref ent.Comp))
            return false;

        for (var i = 0; i < ent.Comp.Bounties.Count; i++)
        {
            if (ent.Comp.Bounties[i].Id != data.Id)
                continue;

            string? actorName = null;
            if (actor != null)
            {
                var getIdentityEvent = new TryGetIdentityShortInfoEvent(ent.Owner, actor.Value);
                RaiseLocalEvent(getIdentityEvent);
                actorName = getIdentityEvent.Title;
            }

            ent.Comp.History.Add(new XenobiologyBountyHistoryData(data,
                skipped
                    ? CargoBountyHistoryData.BountyResult.Skipped
                    : CargoBountyHistoryData.BountyResult.Completed,
                _timing.CurTime,
                actorName));
            ent.Comp.Bounties.RemoveAt(i);
            return true;
        }

        return false;
    }

    [PublicAPI]
    public bool TryGetBountyFromId(
        EntityUid uid,
        string id,
        [NotNullWhen(true)] out XenobiologyBountyData? bounty,
        StationXenobiologyBountyDatabaseComponent? component = null)
    {
        bounty = null;
        if (!Resolve(uid, ref component))
            return false;

        foreach (var bountyData in component.Bounties
                     .Where(bountyData => bountyData.Id == id))
        {
            bounty = bountyData;
            break;
        }

        return bounty != null;
    }

    public void SortBounties(StationXenobiologyBountyDatabaseComponent db)
    {
        db.Bounties = db.Bounties
            .OrderBy(bounty => !_proto.TryIndex(bounty.Bounty, out var proto) ? 0 : GetActualValue(bounty))
            .ToList();
    }
    public void RandomizeAllBountyPointMultipliers(StationXenobiologyBountyDatabaseComponent db)
    {
        foreach (var bounty in db.Bounties)
            RandomizeBountyPointMultiplier(bounty);
    }

    public void ShiftAllTowardsDefaultPointMultiplier(StationXenobiologyBountyDatabaseComponent db)
    {
        foreach (var bounty in db.Bounties)
            ShiftTowardsDefaultPointMultiplier(bounty);
    }

    public void RandomizeBountyPointMultiplier(XenobiologyBountyData bounty) =>
        bounty.CurrentMultiplier = _random.NextFloat(bounty.MinMultiplier, bounty.MaxMultiplier);

    public void IncreaseBountyPointMultiplier(XenobiologyBountyData bounty) =>
        bounty.CurrentMultiplier = _random.NextFloat(bounty.CurrentMultiplier, bounty.MaxMultiplier);


    public void DecreaseBountyPointMultiplier(XenobiologyBountyData bounty) =>
        bounty.CurrentMultiplier = _random.NextFloat(bounty.MinMultiplier, bounty.CurrentMultiplier);

    public void ShiftTowardsDefaultPointMultiplier(XenobiologyBountyData bounty) =>
        bounty.CurrentMultiplier = MathHelper.Lerp(bounty.CurrentMultiplier, bounty.InitialMultiplier, 0.3f);

    public int GetActualValue(XenobiologyBountyData bounty)
    {
        if (!_proto.TryIndex(bounty.Bounty, out var proto))
            return 0;

        double absNumber = Math.Abs(proto.PointsAwarded * bounty.CurrentMultiplier);
        var exponent = Math.Floor(Math.Log10(absNumber));
        var baseValue = 0.25 * Math.Pow(10, exponent);
        var roundedAbs = Math.Round(absNumber / baseValue, MidpointRounding.AwayFromZero) * baseValue;
        return  (int)roundedAbs;
    }
}
