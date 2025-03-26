using System.Linq;
using Content.Server._Goobstation.ServerCurrency;
using Content.Server.Popups;
using Content.Shared._Goobstation.Chat;
using Content.Shared._Shitmed.Body.Events;
using Content.Shared.Actions;
using Content.Shared.Body.Components;
using Content.Shared.Body.Systems;
using Content.Shared.Implants;
using Content.Shared._EinsteinEngines.Silicon.Components;
using Content.Shared.Movement.Events;
using Robust.Server.Audio;
using Robust.Shared.Player;

namespace Content.Server._Goobstation.Chat;

public sealed class TheSwapSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedBodySystem _body = default!;
    [Dependency] private readonly MetaDataSystem _meta = default!;
    [Dependency] private readonly SharedSubdermalImplantSystem _subdermal = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly AudioSystem _audio = default!;
    [Dependency] private readonly ServerCurrencyManager _currencyManager = default!;


    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<TheSwapComponent, MapInitEvent>(OnMapInit, after: new[] { typeof(SharedBodySystem) });
        SubscribeLocalEvent<TheSwapComponent, UpdateCanMoveEvent>(OnUpdateCanMove);

        SubscribeLocalEvent<BodyComponent, BecomeTheSwapEvent>(OnBecomeTheSwap);
    }

    private void OnUpdateCanMove(Entity<TheSwapComponent> ent, ref UpdateCanMoveEvent args)
    {
        args.Cancel();
    }

    private void OnMapInit(Entity<TheSwapComponent> ent, ref MapInitEvent args)
    {
        if (!TryComp(ent, out BodyComponent? body) || !TryComp(ent, out ActionsComponent? actions))
            return;

        foreach (var leg in body.LegEntities.ToList())
        {
            var ev = new BodyPartEnableChangedEvent(false);
            RaiseLocalEvent(leg, ref ev);
            _body.GibPart(leg, launchGibs: false);
        }

        // Just in case they are in mech or something.
        _transform.AttachToGridOrMap(ent);

        // Iconic name
        if (ent.Comp.Name != null)
            _meta.SetEntityName(ent, ent.Comp.Name);

        // Ask and you shall recieve
        if (ent.Comp.Implant != null)
            _subdermal.AddImplant(ent, ent.Comp.Implant.Value);
        // Not adding action to mind to prevent cheesing it by transferring mind to other entities
        if (ent.Comp.SpellAction != null)
            _actions.AddAction(ent, ent.Comp.SpellAction.Value, component: actions);
    }

    private void OnBecomeTheSwap(Entity<BodyComponent> ent, ref BecomeTheSwapEvent args)
    {
        if (!TryComp(ent, out ActorComponent? actor))
            return;

        if (HasComp<TheSwapComponent>(ent))
        {
            _popup.PopupEntity(Loc.GetString("chat-trigger-already-the-swap"), ent, actor.PlayerSession);
            return;
        }

        if (args.IpcOnly && !HasComp<SiliconComponent>(ent))
        {
            _popup.PopupEntity(Loc.GetString("chat-trigger-the-swap-not-ipc"), ent, actor.PlayerSession);
            return;
        }

        if (args.ServerCurrencyCost > 0)
        {
            if (!_currencyManager.CanAfford(actor.PlayerSession.UserId, args.ServerCurrencyCost, out _))
            {
                _popup.PopupEntity(
                    Loc.GetString("chat-trigger-cant-afford-the-swap",
                        ("currency", Loc.GetString("server-currency-name-plural")),
                        ("cost", args.ServerCurrencyCost)),
                    ent,
                    actor.PlayerSession);
                return;
            }

            _currencyManager.RemoveCurrency(actor.PlayerSession.UserId, args.ServerCurrencyCost);
        }

        _audio.PlayPvs(args.TransformSound, ent);

        EnsureComp<TheSwapComponent>(ent);
    }
}
