using System.Linq;
using Content.Goobstation.Shared.Gangwars.Components;
using Content.Goobstation.Shared.ManifestListings;
using Content.Shared.Examine;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Content.Shared.Store;
using Content.Shared.Store.Components;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.Gangwars.Systems;

/// <summary>
/// The store balance is loaded from and saved to the members GangMemberComponent.
/// </summary>
public sealed class GangLockerStoreSystem : EntitySystem
{
    [Dependency] private readonly SharedUserInterfaceSystem _ui = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly INetManager _netManager = default!;
    [Dependency] private readonly GangHiddenStructureSystem _hidden = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public static readonly ProtoId<CurrencyPrototype> GangPointCurrency = "GangPoint";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<GangLockerComponent, InteractHandEvent>(OnInteractHand);
        SubscribeLocalEvent<GangLockerComponent, BoundUIOpenedEvent>(OnStoreOpened);
        SubscribeLocalEvent<GangLockerComponent, BoundUIClosedEvent>(OnStoreClosed);
        SubscribeLocalEvent<GangLockerComponent, ExaminedEvent>(OnExamined);
        SubscribeLocalEvent<GangLockerComponent, ListingPurchasedEvent>(OnPurchase);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (!_netManager.IsServer)
            return;

        var query = EntityQueryEnumerator<GangLockerComponent, GangHiddenStructureComponent>();
        while (query.MoveNext(out var uid, out var comp, out var hidden))
        {
            if (comp.CurrentStoreUser != null)
                _hidden.KeepRevealed((uid, hidden));
        }
    }

    private void OnExamined(Entity<GangLockerComponent> locker, ref ExaminedEvent args)
    {
        if (TryComp<GangHiddenStructureComponent>(locker, out var hidden) && hidden.IsHidden
            || !TryComp<GangMemberComponent>(args.Examiner, out _))
            return;

        var query = EntityQueryEnumerator<GangwarRuleComponent>();
        if (!query.MoveNext(out _, out var rule) || rule.GangNames.Count == 0)
            return;

        var scores = rule.GangScores;
        var rankedColors = rule.GangNames.Keys.OrderByDescending(c => scores.GetValueOrDefault(c)).ToList();
        var updateIn = (int) (rule.NextScoreUpdate - _timing.CurTime).TotalSeconds;


        args.PushMarkup(Loc.GetString("gang-locker-examine-header"), 1);

        for (var i = 0; i < rankedColors.Count; i++)
        {
            var color = rankedColors[i];
            var hex = color.ToHexNoAlpha().TrimStart('#');
            var name = rule.GangNames.GetValueOrDefault(color, hex);
            var score = scores.GetValueOrDefault(color);
            args.PushMarkup(Loc.GetString("gang-locker-examine-gang-entry", ("color", hex), ("name", name), ("rank", i + 1), ("score", score)));
        }

        args.PushMarkup(Loc.GetString("gang-locker-examine-update-timer", ("seconds", updateIn)));
    }

    private void OnStoreOpened(Entity<GangLockerComponent> locker, ref BoundUIOpenedEvent args)
    {
        var msg = new StoreRequestUpdateInterfaceMessage
        {
            Actor = args.Actor,
            Entity = GetNetEntity(locker.Owner),
        };
        RaiseLocalEvent(locker.Owner, msg);
    }

    private void OnInteractHand(Entity<GangLockerComponent> locker, ref InteractHandEvent args)
    {
        if (args.Handled)
            return;

        if (!TryComp<StoreComponent>(locker, out var storeComp)
            || !TryComp<GangMemberComponent>(args.User, out var memberComp)
            || memberComp.Gang != locker.Comp.GangColor
            || TryComp<GangHiddenStructureComponent>(locker, out var hidden) && hidden.IsHidden)
            return;

        args.Handled = true;

        var alreadyOpenForUser = _ui.IsUiOpen(locker.Owner, StoreUiKey.Key, args.User);

        if (!alreadyOpenForUser
            && locker.Comp.CurrentStoreUser is { } currentUser
            && currentUser != args.User)
        {
            _popup.PopupClient(Loc.GetString("gang-store-in-use"), locker.Owner, args.User);
            return;
        }

        if (!alreadyOpenForUser)
        {
            storeComp.Balance[GangPointCurrency] = memberComp.GangPoints;
            locker.Comp.CurrentStoreUser = args.User;
            Dirty(locker);
        }

        _hidden.KeepRevealed(locker.Owner);

        _ui.TryToggleUi(locker.Owner, StoreUiKey.Key, args.User);
    }

    private void OnStoreClosed(Entity<GangLockerComponent> locker, ref BoundUIClosedEvent args)
    {
        if (locker.Comp.CurrentStoreUser == null)
            return;

        locker.Comp.CurrentStoreUser = null;
        Dirty(locker);
        _hidden.KeepRevealed(locker.Owner);
    }

    private void OnPurchase(Entity<GangLockerComponent> locker, ref ListingPurchasedEvent args)
    {
        if (!TryComp<GangMemberComponent>(args.User, out var member)
            || !args.Data.Cost.TryGetValue(GangPointCurrency, out var cost))
            return;

        member.GangPoints -= (int) cost;
        Dirty(args.User, member);
    }
}
