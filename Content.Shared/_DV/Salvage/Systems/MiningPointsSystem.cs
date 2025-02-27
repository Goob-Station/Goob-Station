using Content.Shared._DV.Salvage.Components;
using Content.Shared._Lavaland.UnclaimedOre;
using Content.Shared.Access.Systems;
using Content.Shared.Lathe;
using Content.Shared.Materials;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Timing;
using Content.Shared._Goobstation.Silo;

namespace Content.Shared._DV.Salvage.Systems;

public sealed class MiningPointsSystem : EntitySystem
{
    [Dependency] private readonly SharedIdCardSystem _idCard = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;

    private EntityQuery<MiningPointsComponent> _query;

    public override void Initialize()
    {
        base.Initialize();

        _query = GetEntityQuery<MiningPointsComponent>();

        SubscribeLocalEvent<MiningPointsLatheComponent, MaterialEntityInsertedEvent>(OnMaterialEntityInserted);
        Subs.BuiEvents<MiningPointsLatheComponent>(LatheUiKey.Key, subs =>
        {
            subs.Event<LatheClaimMiningPointsMessage>(OnClaimMiningPoints);
        });
    }

    #region Event Handlers

    private void OnMaterialEntityInserted(Entity<MiningPointsLatheComponent> ent, ref MaterialEntityInsertedEvent args)
    {
        if (!_timing.IsFirstTimePredicted
            || !TryComp<UnclaimedOreComponent>(args.Inserted, out var unclaimedOre)
            || !TryComp<SiloUtilizerComponent>(ent, out var utilizer)
            || !utilizer.Silo.HasValue
            || Transform(utilizer.Silo.Value).MapID != Transform(ent).MapID)
            return;

        var points = unclaimedOre.MiningPoints * args.Count;
        if (points > 0)
            AddPoints(ent.Owner, (uint) points);
    }

    private void OnClaimMiningPoints(Entity<MiningPointsLatheComponent> ent, ref LatheClaimMiningPointsMessage args)
    {
        var user = args.Actor;
        if (TryFindIdCard(user) is {} dest)
            TransferAll(ent.Owner, dest);
    }

    #endregion
    #region Public API

    /// <summary>
    /// Tries to find the user's id card and gets its <see cref="MiningPointsComponent"/>.
    /// </summary>
    /// <remarks>
    /// Component is nullable for easy usage with the API due to Entity&lt;T&gt; not being usable for Entity&lt;T?&gt; arguments.
    /// </remarks>
    public Entity<MiningPointsComponent?>? TryFindIdCard(EntityUid user)
    {
        if (!_idCard.TryFindIdCard(user, out var idCard))
            return null;

        if (!_query.TryComp(idCard, out var comp))
            return null;

        return (idCard, comp);
    }

    /// <summary>
    /// Returns true if the user has at least some number of points on their ID card.
    /// </summary>
    public bool UserHasPoints(EntityUid user, uint points)
    {
        if (TryFindIdCard(user)?.Comp is not {} comp)
            return false;

        return comp.Points >= points;
    }

    /// <summary>
    /// Removes points from a holder, returning true if it succeeded.
    /// </summary>
    public bool RemovePoints(Entity<MiningPointsComponent?> ent, uint amount)
    {
        if (!_query.Resolve(ent, ref ent.Comp) || amount > ent.Comp.Points)
            return false;

        ent.Comp.Points -= amount;
        Dirty(ent);
        return true;
    }

    /// <summary>
    /// Add points to a holder.
    /// </summary>
    public bool AddPoints(Entity<MiningPointsComponent?> ent, uint amount)
    {
        if (!_query.Resolve(ent, ref ent.Comp))
            return false;

        ent.Comp.Points += amount;
        Dirty(ent);
        return true;
    }

    /// <summary>
    /// Transfer a number of points from source to destination.
    /// Returns true if the transfer succeeded.
    /// </summary>
    public bool Transfer(Entity<MiningPointsComponent?> src, Entity<MiningPointsComponent?> dest, uint amount)
    {
        // don't make a sound or anything
        if (amount == 0)
            return true;

        if (!_query.Resolve(src, ref src.Comp) || !_query.Resolve(dest, ref dest.Comp))
            return false;

        if (!RemovePoints(src, amount))
            return false;

        AddPoints(dest, amount);
        _audio.PlayPvs(new SoundPathSpecifier("/Audio/Effects/Cargo/ping.ogg"), src.Owner);
        return true;
    }

    /// <summary>
    /// Transfers all points from source to destination.
    /// Returns true if the transfer succeeded.
    /// </summary>
    public bool TransferAll(Entity<MiningPointsComponent?> src, Entity<MiningPointsComponent?> dest)
    {
        return _query.Resolve(src, ref src.Comp) && Transfer(src, dest, src.Comp.Points);
    }

    #endregion
}
