using System.Linq;
using Content.Server.Hands.Systems;
using Content.Server.Stack;
using Content.Server.Store.Components;
using Content.Shared.Emag.Components;
using Content.Shared.Emag.Systems;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Content.Shared.Stacks;
using Content.Shared._Pirate.Banking;
using Robust.Server.Containers;
using Robust.Server.GameObjects;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
using Robust.Shared.Prototypes;

namespace Content.Server._Pirate.Banking;

public sealed class ATMSystem : SharedATMSystem
{
    [Dependency] private readonly UserInterfaceSystem _ui = default!;
    [Dependency] private readonly BankCardSystem _bankCardSystem = default!;
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
    [Dependency] private readonly StackSystem _stackSystem = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly SharedAudioSystem _audioSystem = default!;
    [Dependency] private readonly ContainerSystem _container = default!;
    [Dependency] private readonly HandsSystem _handsSystem = default!;


    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ATMComponent, EntInsertedIntoContainerMessage>(OnCardInserted);
        SubscribeLocalEvent<ATMComponent, EntRemovedFromContainerMessage>(OnCardRemoved);
        SubscribeLocalEvent<ATMComponent, ATMRequestWithdrawMessage>(OnWithdrawRequest);
        SubscribeLocalEvent<ATMComponent, InteractUsingEvent>(OnInteractUsing);
        SubscribeLocalEvent<ATMComponent, ComponentStartup>(OnComponentStartup);
        SubscribeLocalEvent<ATMComponent, GotEmaggedEvent>(OnEmag);
    }

    private void OnEmag(EntityUid uid, ATMComponent component, ref GotEmaggedEvent args)
    {
        args.Handled = true;
    }

    private void OnComponentStartup(EntityUid uid, ATMComponent component, ComponentStartup args)
    {
        UpdateUiState(uid, -1, false, Loc.GetString("atm-ui-insert-card"));
    }

    private void OnInteractUsing(EntityUid uid, ATMComponent component, InteractUsingEvent args)
    {
        if (!TryComp<CurrencyComponent>(args.Used, out var currency) || !currency.Price.Keys.Contains(component.CurrencyType))
        {
            return;
        }

        if (!component.CardSlot.Item.HasValue)
        {
            _popupSystem.PopupEntity(Loc.GetString("atm-trying-insert-cash-error"), args.Target, args.User, PopupType.Medium);
            _audioSystem.PlayPvs(component.SoundDeny, uid);
            return;
        }

        var stack = Comp<StackComponent>(args.Used);
        var bankCard = Comp<BankCardComponent>(component.CardSlot.Item.Value);
        var amount = stack.Count;

        if (!_bankCardSystem.TryChangeBalance(bankCard.AccountId!.Value, amount))
        {
            _popupSystem.PopupEntity(Loc.GetString("atm-deposit-failed"), uid, args.User, PopupType.Medium);
            _audioSystem.PlayPvs(component.SoundDeny, uid);
            return;
        }

        Del(args.Used);

        _audioSystem.PlayPvs(component.SoundInsertCurrency, uid);
        UpdateUiState(uid, _bankCardSystem.GetBalance(bankCard.AccountId.Value), true, Loc.GetString("atm-ui-select-withdraw-amount"));
    }

    private void OnCardInserted(EntityUid uid, ATMComponent component, EntInsertedIntoContainerMessage args)
    {
        if (!TryComp<BankCardComponent>(args.Entity, out var bankCard) || !bankCard.AccountId.HasValue)
        {
            _container.EmptyContainer(args.Container);
            return;
        }

        UpdateUiState(uid, _bankCardSystem.GetBalance(bankCard.AccountId.Value), true, Loc.GetString("atm-ui-select-withdraw-amount"));
    }

    private void OnCardRemoved(EntityUid uid, ATMComponent component, EntRemovedFromContainerMessage args)
    {
        UpdateUiState(uid, -1, false, Loc.GetString("atm-ui-insert-card"));
    }

    private void OnWithdrawRequest(EntityUid uid, ATMComponent component, ATMRequestWithdrawMessage args)
    {
        if (!TryComp<BankCardComponent>(component.CardSlot.Item, out var bankCard) || !bankCard.AccountId.HasValue)
        {
            if (component.CardSlot.ContainerSlot != null)
                _container.EmptyContainer(component.CardSlot.ContainerSlot);
            return;
        }

        if (!_bankCardSystem.TryGetAccount(bankCard.AccountId.Value, out var account) ||
            account.AccountPin != args.Pin && !HasComp<EmaggedComponent>(uid))
        {
            _popupSystem.PopupEntity(Loc.GetString("atm-wrong-pin"), uid);
            _audioSystem.PlayPvs(component.SoundDeny, uid);
            return;
        }

        if (!_bankCardSystem.TryChangeBalance(account.AccountId, -args.Amount))
        {
            _popupSystem.PopupEntity(Loc.GetString("atm-not-enough-cash"), uid);
            _audioSystem.PlayPvs(component.SoundDeny, uid);
            return;
        }

        var player = args.Actor;
        var spawnedMoney = _stackSystem.SpawnAtPosition(args.Amount, _prototypeManager.Index<StackPrototype>(component.CreditStackPrototype), Transform(player).Coordinates);

        if (!_handsSystem.TryPickupAnyHand(player, spawnedMoney))
        {
        }
        
        _audioSystem.PlayPvs(component.SoundWithdrawCurrency, uid);

        UpdateUiState(uid, account.Balance, true, Loc.GetString("atm-ui-select-withdraw-amount"));
    }

    private void UpdateUiState(EntityUid uid, int balance, bool hasCard, string infoMessage)
    {
        var state = new ATMBuiState
        {
            AccountBalance = balance,
            HasCard = hasCard,
            InfoMessage = infoMessage
        };

        _ui.SetUiState(uid, ATMUiKey.Key, state);
    }
}
